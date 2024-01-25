using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[assembly: InternalsVisibleTo("Rukhanka.Tests")]

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{ 
public partial struct AnimatorControllerSystemJobs
{

[BurstCompile]
public struct StateMachineProcessJob: IJobChunk
{
	public float dt;
	public int frameIndex;
	public BufferTypeHandle<AnimatorControllerLayerComponent> controllerLayersBufferHandle;
	public BufferTypeHandle<AnimatorControllerParameterComponent> controllerParametersBufferHandle;

#if RUKHANKA_DEBUG_INFO
	public bool doLogging;
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
	{
		var layerBuffers = chunk.GetBufferAccessor(ref controllerLayersBufferHandle);
		var parameterBuffers = chunk.GetBufferAccessor(ref controllerParametersBufferHandle);

		var cee = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

		while (cee.NextEntityIndex(out var i))
		{
			var layers = layerBuffers[i];
			var parameters = parameterBuffers.Length > 0 ? parameterBuffers[i].AsNativeArray() : default;

			ExecuteSingle(layers, parameters);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void ExecuteSingle(DynamicBuffer<AnimatorControllerLayerComponent> aclc, NativeArray<AnimatorControllerParameterComponent> acpc)
	{
		for (int i = 0; i < aclc.Length; ++i)
		{
			ref var acc = ref aclc.ElementAt(i);
		#if RUKHANKA_DEBUG_INFO
			//	Make state snapshot to compare it later and log differences
			var controllerDataPreSnapshot = acc;
		#endif

			ProcessLayer(ref acc.controller.Value, acc.layerIndex, acpc, dt, ref acc);

		#if RUKHANKA_DEBUG_INFO
			DoDebugLogging(controllerDataPreSnapshot, acc, frameIndex);
		#endif
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	RuntimeAnimatorData.StateRuntimeData InitRuntimeStateData(int stateID)
	{
		var rv = new RuntimeAnimatorData.StateRuntimeData();
		rv.id = stateID;
		rv.normalizedDuration = 0;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void ExitTransition(ref AnimatorControllerLayerComponent acc)
	{
		if (acc.rtd.activeTransition.id < 0)
			return;
		
		if (CheckTransitionExitConditions(acc.rtd.activeTransition))
		{
			acc.rtd.srcState = acc.rtd.dstState;
			acc.rtd.dstState = acc.rtd.activeTransition = RuntimeAnimatorData.StateRuntimeData.MakeDefault();
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void EnterTransition
	(
		ref AnimatorControllerLayerComponent acc,
		ref LayerBlob layer,
		NativeArray<AnimatorControllerParameterComponent> runtimeParams,
		float srcStateDurationFrameDelta,
		float curStateDuration
	)
	{
		if (acc.rtd.activeTransition.id >= 0)
			return;

		ref var currentState = ref layer.states[acc.rtd.srcState.id];

		for (int i = 0; i < currentState.transitions.Length; ++i)
		{
			ref var t = ref currentState.transitions[i];
			var b = CheckTransitionEnterExitTimeCondition(ref t, acc.rtd.srcState, srcStateDurationFrameDelta) &&
					CheckTransitionEnterConditions(ref t, runtimeParams);
			if (b)
			{
				var timeShouldBeInTransition = GetTimeInSecondsShouldBeInTransition(ref t, acc.rtd.srcState, curStateDuration, srcStateDurationFrameDelta);
				acc.rtd.activeTransition.id	= i;
				acc.rtd.activeTransition.normalizedDuration = timeShouldBeInTransition / CalculateTransitionDuration(ref t, curStateDuration);
				var dstStateDur = CalculateStateDuration(ref layer.states[t.targetStateId], runtimeParams);
				acc.rtd.dstState = InitRuntimeStateData(t.targetStateId);
				acc.rtd.dstState.normalizedDuration += timeShouldBeInTransition / dstStateDur + t.offset;
				break;
			}
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void ProcessLayer(ref ControllerBlob c, int layerIndex, NativeArray<AnimatorControllerParameterComponent> runtimeParams, float dt, ref AnimatorControllerLayerComponent acc)
	{
		ref var layer = ref c.layers[layerIndex];

		var currentStateID = acc.rtd.srcState.id;
		if (currentStateID < 0)
			currentStateID = layer.defaultStateIndex;

		ref var currentState = ref layer.states[currentStateID];
		var curStateDuration = CalculateStateDuration(ref currentState, runtimeParams);

		if (Hint.Unlikely(acc.rtd.srcState.id < 0))
		{
			acc.rtd.srcState = InitRuntimeStateData(layer.defaultStateIndex);
		}

		var srcStateDurationFrameDelta = dt / curStateDuration;
		acc.rtd.srcState.normalizedDuration += srcStateDurationFrameDelta;

		if (acc.rtd.dstState.id >= 0)
		{
			var dstStateDuration = CalculateStateDuration(ref layer.states[acc.rtd.dstState.id], runtimeParams);
			acc.rtd.dstState.normalizedDuration += dt / dstStateDuration;
		}

		if (acc.rtd.activeTransition.id >= 0)
		{
			ref var currentTransitionBlob = ref currentState.transitions[acc.rtd.activeTransition.id];
			var transitionDuration = CalculateTransitionDuration(ref currentTransitionBlob, curStateDuration);
			acc.rtd.activeTransition.normalizedDuration += dt / transitionDuration;
		}

		ExitTransition(ref acc);
		EnterTransition(ref acc, ref layer, runtimeParams, srcStateDurationFrameDelta, curStateDuration);
		//	Check tranision exit conditions one more time in case of Enter->Exit sequence appeared in single frame
		ExitTransition(ref acc);

		ProcessTransitionInterruptions();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	//	p0 = (0,0)
	static (float, float, float) CalculateBarycentric(float2 p1, float2 p2, float2 pt)
	{
		var np2 = new float2(0 - p2.y, p2.x - 0);
		var np1 = new float2(0 - p1.y, p1.x - 0);

		var l1 = math.dot(pt, np2) / math.dot(p1, np2);
		var l2 = math.dot(pt, np1) / math.dot(p2, np1);
		var l0 = 1 - l1 - l2;
		return (l0, l1, l2);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	static unsafe void HandleCentroidCase(ref NativeList<MotionIndexAndWeight> rv, float2 pt, ref BlobArray<ChildMotionBlob> mbArr)
	{
		if (math.any(pt))
			return;

		int i = 0;
		for (; i < mbArr.Length && math.any(mbArr[i].position2D); ++i) { }

		if (i < mbArr.Length)
		{
			var miw = new MotionIndexAndWeight() { motionIndex = i, weight = 1 };
			rv.Add(miw);
		}
		else
		{
			var f = 1.0f / mbArr.Length;
			for (int l = 0; l < mbArr.Length; ++l)
			{
				var miw = new MotionIndexAndWeight() { motionIndex = l, weight = f };
				rv.Add(miw);
			}
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe NativeList<MotionIndexAndWeight> GetBlendTree2DSimpleDirectionalCurrentMotions(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		var rv = new NativeList<MotionIndexAndWeight>(Allocator.Temp);
		var pX = runtimeParams[mb.blendTree.blendParameterIndex];
		var pY = runtimeParams[mb.blendTree.blendParameterYIndex];
		var pt = new float2(pX.FloatValue, pY.FloatValue);
		ref var motions = ref mb.blendTree.motions;

		if (motions.Length < 2)
		{
			if (motions.Length == 1)
				rv.Add(new MotionIndexAndWeight() { weight = 1, motionIndex = 0 });
			return rv;
		}

		HandleCentroidCase(ref rv, pt, ref motions);
		if (rv.Length > 0)
			return rv;

		var centerPtIndex = -1;
		//	Loop over all directions and search for sector that contains requested point
		var dotProductsAndWeights = new NativeList<MotionIndexAndWeight>(motions.Length, Allocator.Temp);
		for (int i = 0; i < motions.Length; ++i)
		{
			ref var m = ref motions[i];
			var motionDir = m.position2D;
			if (!math.any(motionDir))
			{
				centerPtIndex = i;
				continue;
			}
			var angle = math.atan2(motionDir.y, motionDir.x);
			var miw = new MotionIndexAndWeight() { motionIndex = i, weight = angle };
			dotProductsAndWeights.Add(miw);
		}

		var ptAngle = math.atan2(pt.y, pt.x);

		dotProductsAndWeights.Sort();

		// Pick two closest points
		MotionIndexAndWeight d0 = default, d1 = default;
		var l = 0;
		for (; l < dotProductsAndWeights.Length; ++l)
		{
			var d = dotProductsAndWeights[l];
			if (d.weight < ptAngle)
			{
				var ld0 = l == 0 ? dotProductsAndWeights.Length - 1 : l - 1;
				d1 = d;
				d0 = dotProductsAndWeights[ld0];
				break;
			}
		}

		//	Handle last sector
		if (l == dotProductsAndWeights.Length)
		{
			d0 = dotProductsAndWeights[dotProductsAndWeights.Length - 1];
			d1 = dotProductsAndWeights[0];
		}

		ref var m0 = ref motions[d0.motionIndex];
		ref var m1 = ref motions[d1.motionIndex];
		var p0 = m0.position2D;
		var p1 = m1.position2D;
		
		//	Barycentric coordinates for point pt in triangle <p0,p1,0>
		var (l0, l1, l2) = CalculateBarycentric(p0, p1, pt);

		var m0Weight = l1;
		var m1Weight = l2;
		if (l0 < 0)
		{
			var sum = m0Weight + m1Weight;
			m0Weight /= sum;
			m1Weight /= sum;
		}	

		l0 = math.saturate(l0);

		var evenlyDistributedMotionWeight = centerPtIndex < 0 ? 1.0f / motions.Length * l0 : 0;

		var miw0 = new MotionIndexAndWeight() { motionIndex = d0.motionIndex, weight = m0Weight + evenlyDistributedMotionWeight };
		rv.Add(miw0);

		var miw1 = new MotionIndexAndWeight() { motionIndex = d1.motionIndex, weight = m1Weight + evenlyDistributedMotionWeight };
		rv.Add(miw1);

		//	Add other motions of blend tree
		if (evenlyDistributedMotionWeight > 0)
		{
			for (int i = 0; i < motions.Length; ++i)
			{
				if (i != d0.motionIndex && i != d1.motionIndex)
				{
					var miw = new MotionIndexAndWeight() { motionIndex = i, weight = evenlyDistributedMotionWeight };
					rv.Add(miw);
				}
			}
		}

		//	Add centroid motion
		if (centerPtIndex >= 0)
		{
			var miw = new MotionIndexAndWeight() { motionIndex = centerPtIndex, weight = l0 };
			rv.Add(miw);
		}

		dotProductsAndWeights.Dispose();

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe NativeList<MotionIndexAndWeight> GetBlendTree2DFreeformCartesianCurrentMotions(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		var pX = runtimeParams[mb.blendTree.blendParameterIndex];
		var pY = runtimeParams[mb.blendTree.blendParameterYIndex];
		var p = new float2(pX.FloatValue, pY.FloatValue);
		ref var motions = ref mb.blendTree.motions;
		Span<float> hpArr = stackalloc float[motions.Length];

		var hpSum = 0.0f;

		//	Calculate influence factors
		for (int i = 0; i < motions.Length; ++i)
		{
			var pi = motions[i].position2D;
			var pip = p - pi;

			var w = 1.0f;

			for (int j = 0; j < motions.Length && w > 0; ++j)
			{
				if (i == j) continue;
				var pj = motions[j].position2D;
				var pipj = pj - pi;
				var f = math.dot(pip, pipj) / math.lengthsq(pipj);
				var hj = math.max(1 - f, 0);
				w = math.min(hj, w);
			}
			hpSum += w;
			hpArr[i] = w;
		}

		var rv = new NativeList<MotionIndexAndWeight>(motions.Length, Allocator.Temp);
		//	Calculate weight functions
		for (int i = 0; i < motions.Length; ++i)
		{
			var w = hpArr[i] / hpSum;
			if (w > 0)
			{
				var miw = new MotionIndexAndWeight() { motionIndex = i, weight = w };
				rv.Add(miw);
			}
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	static float CalcAngle(float2 a, float2 b)
	{
		var cross = a.x * b.y - a.y * b.x;
		var dot = math.dot(a, b);
		var tanA = new float2(cross, dot);
		var rv = math.atan2(tanA.x, tanA.y);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	static float2 CalcAngleWeights(float2 i, float2 j, float2 s)
	{
		float2 rv = 0;
		if (!math. any(i))
		{
			rv.x = CalcAngle(j, s);
			rv.y = 0;
		}
		else if (!math.any(j))
		{
			rv.x = CalcAngle(i, s);
			rv.y = rv.x;
		}
		else
		{
			rv.x = CalcAngle(i, j);
			if (!math.any(s))
				rv.y = rv.x;
			else
				rv.y = CalcAngle(i, s);
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static unsafe NativeList<MotionIndexAndWeight> GetBlendTree2DFreeformDirectionalCurrentMotions(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		var pX = runtimeParams[mb.blendTree.blendParameterIndex];
		var pY = runtimeParams[mb.blendTree.blendParameterYIndex];
		var p = new float2(pX.FloatValue, pY.FloatValue);
		var lp = math.length(p);

		ref var motions = ref mb.blendTree.motions;
		Span<float> hpArr = stackalloc float[motions.Length];

		var hpSum = 0.0f;

		//	Calculate influence factors
		for (int i = 0; i < motions.Length; ++i)
		{
			var pi = motions[i].position2D;
			var lpi = math.length(pi);

			var w = 1.0f;

			for (int j = 0; j < motions.Length && w > 0; ++j)
			{
				if (i == j) continue;
				var pj = motions[j].position2D;
				var lpj = math.length(pj);

				var pRcpMiddle = math.rcp((lpj + lpi) * 0.5f);
				var lpip = (lp - lpi) * pRcpMiddle;
				var lpipj = (lpj - lpi) * pRcpMiddle;
				var angleWeights = CalcAngleWeights(pi, pj, p);

				var pip = new float2(lpip, angleWeights.y);
				var pipj = new float2(lpipj, angleWeights.x);

				var f = math.dot(pip, pipj) / math.lengthsq(pipj);
				var hj = math.saturate(1 - f);
				w = math.min(hj, w);
			}
			hpSum += w;
			hpArr[i] = w;	
		}

		var rv = new NativeList<MotionIndexAndWeight>(motions.Length, Allocator.Temp);
		//	Calculate weight functions
		for (int i = 0; i < motions.Length; ++i)
		{
			var w = hpArr[i] / hpSum;
			if (w > 0)
			{
				var miw = new MotionIndexAndWeight() { motionIndex = i, weight = w };
				rv.Add(miw);
			}
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public static NativeList<MotionIndexAndWeight> GetBlendTree1DCurrentMotions(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		var blendTreeParameter = runtimeParams[mb.blendTree.blendParameterIndex];
		ref var motions = ref mb.blendTree.motions;
		var i0 = 0;
		var i1 = 0;
		bool found = false;
		for (int i = 0; i < motions.Length && !found; ++i)
		{
			ref var m = ref motions[i];
			i0 = i1;
			i1 = i;
			if (m.threshold > blendTreeParameter.FloatValue)
				found = true;
		}
		if (!found)
		{
			i0 = i1 = motions.Length - 1;
		}

		var motion0Threshold = motions[i0].threshold;
		var motion1Threshold = motions[i1].threshold;
		float f = i1 == i0 ? 0 : (blendTreeParameter.FloatValue - motion0Threshold) / (motion1Threshold - motion0Threshold);

		var rv = new NativeList<MotionIndexAndWeight>(2, Allocator.Temp);
		rv.Add(new MotionIndexAndWeight { motionIndex = i0, weight = 1 - f });
		rv.Add(new MotionIndexAndWeight { motionIndex = i1, weight = f });
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static NativeList<MotionIndexAndWeight> GetBlendTreeDirectCurrentMotions(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		ref var motions = ref mb.blendTree.motions;
		var rv = new NativeList<MotionIndexAndWeight>(motions.Length, Allocator.Temp);

		var weightSum = 0.0f;
		for (int i = 0; i < motions.Length; ++i)
		{
			ref var cm = ref motions[i];
			var w = cm.directBlendParameterIndex >= 0 ? runtimeParams[cm.directBlendParameterIndex].FloatValue : 0;
			if (w > 0)
			{
				var miw = new MotionIndexAndWeight() { motionIndex = i, weight = w };
				weightSum += miw.weight;
				rv.Add(miw);
			}
		}

		if (mb.blendTree.normalizeBlendValues && weightSum > 1)
		{
			for (int i = 0; i < rv.Length; ++i)
			{
				var miw = rv[i];
				miw.weight = miw.weight / weightSum;
				rv[i] = miw;
			}
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	unsafe float CalculateMotionDuration(ref MotionBlob mb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams, float weight)
	{
		if (weight == 0) return 0;

		NativeList<MotionIndexAndWeight> blendTreeMotionsAndWeights = default;
		switch (mb.type)
		{
		case MotionBlob.Type.None:
			return 1;
		case MotionBlob.Type.AnimationClip:
			return mb.animationBlob.Value.length * weight;
		case MotionBlob.Type.BlendTreeDirect:
			blendTreeMotionsAndWeights = GetBlendTreeDirectCurrentMotions(ref mb, runtimeParams);
			break;
		case MotionBlob.Type.BlendTree1D:
			blendTreeMotionsAndWeights = GetBlendTree1DCurrentMotions(ref mb, runtimeParams);
			break;
		case MotionBlob.Type.BlendTree2DSimpleDirectional:
			blendTreeMotionsAndWeights = GetBlendTree2DSimpleDirectionalCurrentMotions(ref mb, runtimeParams);
			break;
		case MotionBlob.Type.BlendTree2DFreeformCartesian:
			blendTreeMotionsAndWeights = GetBlendTree2DFreeformCartesianCurrentMotions(ref mb, runtimeParams);
			break;
		case MotionBlob.Type.BlendTree2DFreeformDirectional:
			blendTreeMotionsAndWeights = GetBlendTree2DFreeformDirectionalCurrentMotions(ref mb, runtimeParams);
			break;
		default:
			Debug.Log($"Unsupported blend tree type");
			break;
		}

		var rv = CalculateBlendTreeMotionDuration(blendTreeMotionsAndWeights, ref mb.blendTree.motions, runtimeParams, weight);
		if (blendTreeMotionsAndWeights.IsCreated) blendTreeMotionsAndWeights.Dispose();
		
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	float CalculateBlendTreeMotionDuration(NativeList<MotionIndexAndWeight> miwArr, ref BlobArray<ChildMotionBlob> motions, in NativeArray<AnimatorControllerParameterComponent> runtimeParams, float weight)
	{
		if (!miwArr.IsCreated || miwArr.IsEmpty)
			return 1;

		var weightSum = 0.0f;
		for (int i = 0; i < miwArr.Length; ++i)
			weightSum += miwArr[i].weight;

		//	If total weight less then 1, normalize weights
		if (Hint.Unlikely(weightSum < 1))
		{
			for (int i = 0; i < miwArr.Length; ++i)
			{
				var miw = miwArr[i];
				miw.weight = miw.weight / weightSum;
				miwArr[i] = miw;
			}
		}

		var rv = 0.0f;
		for (int i = 0; i < miwArr.Length; ++i)
		{
			var miw = miwArr[i];
			ref var m = ref motions[miw.motionIndex];
			rv += CalculateMotionDuration(ref m.motion, runtimeParams, weight * miw.weight) / m.timeScale;
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	float CalculateTransitionDuration(ref TransitionBlob tb, float curStateDuration)
	{
		var rv = tb.duration;
		if (!tb.hasFixedDuration)
		{
			rv *= curStateDuration;
		}
		return math.max(rv, 0.0001f);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	float CalculateStateDuration(ref StateBlob sb, in NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		var motionDuration = CalculateMotionDuration(ref sb.motion, runtimeParams, 1);
		var speedMultiplier = 1.0f;
		if (sb.speedMultiplierParameterIndex >= 0)
		{
			speedMultiplier = runtimeParams[sb.speedMultiplierParameterIndex].FloatValue;
		}
		return motionDuration / (sb.speed * speedMultiplier);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	internal static float GetLoopAwareTransitionExitTime(float exitTime, float normalizedDuration, float speedSign)
	{
		var rv = exitTime;
		if (exitTime <= 1.0f)
		{
			//	Unity animator logic and documentation mismatch. Documentation says that exit time loop condition should be when transition exitTime less then 1, but in practice it will loop when exitTime is less or equal(!) to 1.
			exitTime = math.min(exitTime, 0.9999f);
			var snd = normalizedDuration * speedSign;

			var f = math.frac(snd);
			rv += (int)snd;
			if (f > exitTime)
				rv += 1;
		}
		return rv * speedSign;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	float GetTimeInSecondsShouldBeInTransition(ref TransitionBlob tb, RuntimeAnimatorData.StateRuntimeData curStateRTD, float curStateDuration, float frameDT)
	{
		if (!tb.hasExitTime) return 0;

		//	This should be always less then curStateRTD.normalizedDuration
		var loopAwareExitTime = GetLoopAwareTransitionExitTime(tb.exitTime, curStateRTD.normalizedDuration - frameDT, math.sign(frameDT));
		var loopDelta = curStateRTD.normalizedDuration - loopAwareExitTime;
		var rv = loopDelta * curStateDuration;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckTransitionEnterExitTimeCondition
	(
		ref TransitionBlob tb,
		RuntimeAnimatorData.StateRuntimeData curStateRuntimeData,
		float srcStateDurationFrameDelta
	)
	{
		var normalizedStateDuration = curStateRuntimeData.normalizedDuration; 

		var noNormalConditions = tb.conditions.Length == 0;
		if (!tb.hasExitTime) return !noNormalConditions;

		var l0 = normalizedStateDuration - srcStateDurationFrameDelta;
		var l1 = normalizedStateDuration;
		var speedSign = math.select(-1, 1, l0 < l1);

		var loopAwareExitTime = GetLoopAwareTransitionExitTime(tb.exitTime, l0, speedSign);

		if (speedSign < 0)
			MathUtils.Swap(ref l0, ref l1);

		var rv = loopAwareExitTime > l0 && loopAwareExitTime <= l1;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckIntCondition(in AnimatorControllerParameterComponent param, ref ConditionBlob c)
	{
		var rv = true;
		switch (c.conditionMode)
		{
		case AnimatorConditionMode.Equals:
			if (param.IntValue != c.threshold.intValue) rv = false;
			break;
		case AnimatorConditionMode.Greater:
			if (param.IntValue <= c.threshold.intValue) rv = false;
			break;
		case AnimatorConditionMode.Less:
			if (param.IntValue >= c.threshold.intValue) rv = false;
			break;
		case AnimatorConditionMode.NotEqual:
			if (param.IntValue == c.threshold.intValue) rv = false;
			break;
		default:
			Debug.LogError($"Unsupported condition type for int parameter value!");
			break;
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckFloatCondition(in AnimatorControllerParameterComponent param, ref ConditionBlob c)
	{
		var rv = true;
		switch (c.conditionMode)
		{
		case AnimatorConditionMode.Greater:
			if (param.FloatValue <= c.threshold.floatValue) rv = false;
			break;
		case AnimatorConditionMode.Less:
			if (param.FloatValue >= c.threshold.floatValue) rv = false;
			break;
		default:
			Debug.LogError($"Unsupported condition type for int parameter value!");
			break;
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckBoolCondition(in AnimatorControllerParameterComponent param, ref ConditionBlob c)
	{
		var rv = true;
		switch (c.conditionMode)
		{
		case AnimatorConditionMode.If:
			rv = param.BoolValue;
			break;
		case AnimatorConditionMode.IfNot:
			rv = !param.BoolValue;
			break;
		default:
			Debug.LogError($"Unsupported condition type for int parameter value!");
			break;
		}
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void ResetTriggers(ref TransitionBlob tb, NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		for (int i = 0; i < tb.conditions.Length; ++i)
		{
			ref var c = ref tb.conditions[i];
			var param = runtimeParams[c.paramIdx];
			if (param.type == ControllerParameterType.Trigger)
			{
				param.value.boolValue = false;
				runtimeParams[c.paramIdx] = param;
			}
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckTransitionEnterConditions(ref TransitionBlob tb, NativeArray<AnimatorControllerParameterComponent> runtimeParams)
	{
		if (tb.conditions.Length == 0)
			return true;

		var rv = true;
		var hasTriggers = false;
		for (int i = 0; i < tb.conditions.Length && rv; ++i)
		{
			ref var c = ref tb.conditions[i];
			var param = runtimeParams[c.paramIdx];

			switch (param.type)
			{
			case ControllerParameterType.Float:
				rv = CheckFloatCondition(param, ref c);
				break;
			case ControllerParameterType.Int:
				rv = CheckIntCondition(param, ref c);
				break;
			case ControllerParameterType.Bool:
				rv = CheckBoolCondition(param, ref c);
				break;
			case ControllerParameterType.Trigger:
				rv = CheckBoolCondition(param, ref c);
				hasTriggers = true;
				break;
			}
		}

		if (hasTriggers && rv)
			ResetTriggers(ref tb, runtimeParams);

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	bool CheckTransitionExitConditions(RuntimeAnimatorData.StateRuntimeData transitionRuntimeData)
	{
		return transitionRuntimeData.normalizedDuration >= 1;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void ProcessTransitionInterruptions()
	{
		// Not implemented yet
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	void DoDebugLogging(AnimatorControllerLayerComponent prevData, AnimatorControllerLayerComponent curData, int frameIndex)
	{
	#if RUKHANKA_DEBUG_INFO
		if (!doLogging) return;

		ref var c = ref curData.controller.Value;
		ref var layer = ref c.layers[curData.layerIndex];
		ref var currentState = ref layer.states[curData.rtd.srcState.id];

		var layerName = layer.name.ToFixedString();
		var controllerName = c.name.ToFixedString();
		var curStateName = currentState.name.ToFixedString();

		Debug.Log($"[{frameIndex}:{controllerName}:{layerName}] In state: '{curStateName}' with normalized duration: {curData.rtd.srcState.normalizedDuration}");

		//	Exit transition event
		if (prevData.rtd.activeTransition.id >= 0 && curData.rtd.activeTransition.id != prevData.rtd.activeTransition.id)
		{
			ref var t = ref layer.states[prevData.rtd.srcState.id].transitions[prevData.rtd.activeTransition.id];
			Debug.Log($"[{frameIndex}:{controllerName}:{layerName}] Exiting transition: '{t.name.ToFixedString()}'");
		}

		//	Enter transition event
		if (curData.rtd.activeTransition.id >= 0)
		{
			ref var t = ref layer.states[curData.rtd.srcState.id].transitions[curData.rtd.activeTransition.id];
			if (curData.rtd.activeTransition.id != prevData.rtd.activeTransition.id)
			{
				Debug.Log($"[{frameIndex}:{controllerName}:{layerName}] Entering transition: '{t.name.ToFixedString()}' with time: {curData.rtd.activeTransition.normalizedDuration}");
			}
			else
			{
				Debug.Log($"[{frameIndex}:{controllerName}:{layerName}] In transition: '{t.name.ToFixedString()}' with time: {curData.rtd.activeTransition.normalizedDuration}");
			}
			ref var dstState = ref layer.states[curData.rtd.dstState.id];
			Debug.Log($"[{frameIndex}:{controllerName}:{layerName}] Target state: '{dstState.name.ToFixedString()}' with time: {curData.rtd.dstState.normalizedDuration}");
		}
	#endif
	}
}
}
}
