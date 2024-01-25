#if UNITY_EDITOR

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Hash128 = Unity.Entities.Hash128;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Rukhanka.Tests")]

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
public partial class AnimatorControllerConversionSystem
{
//=================================================================================================================//

	[BurstCompile]
	public struct CreateBlobAssetsJob: IJob
	{
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<AnimatorBlobAssets> outBlobAssets;
		public AnimatorControllerBakerData inData;
		[ReadOnly]
		public bool doLogging;

		void AddTransitionBlob(RTP.Transition t, UnsafeList<RTP.State> allStates, UnsafeList<RTP.Parameter> allParams, ref BlobBuilder bb, ref TransitionBlob tb)
		{
		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref tb.name, ref t.name);
		#endif

			var bbc = bb.Allocate(ref tb.conditions, t.conditions.Length);
			for (int ci = 0; ci < t.conditions.Length; ++ci)
			{
				ref var cb = ref bbc[ci];
				var src = t.conditions[ci];
				cb.conditionMode = src.conditionMode;
				cb.paramIdx = allParams.IndexOf(src.paramName);
				cb.threshold = src.threshold;

			#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref cb.name, ref src.name);
			#endif
			}

			tb.hash = t.name.CalculateHash32();
			tb.duration = t.duration;
			tb.exitTime = t.exitTime;
			tb.hasExitTime = t.hasExitTime;
			tb.offset = t.offset;
			tb.hasFixedDuration = t.hasFixedDuration;
			tb.targetStateId = allStates.IndexOf(t.targetStateHash);
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddChildMotionBlob(RTP.ChildMotion cm, ref BlobBuilder bb, ref ChildMotionBlob cmb, ref BlobBuilderArray<AnimationClipBlob> allAnims, in UnsafeList<RTP.Parameter> allParams)
		{
			cmb.threshold = cm.threshold;
			cmb.timeScale = cm.timeScale;
			cmb.position2D = cm.position2D;
			cmb.directBlendParameterIndex = allParams.IndexOf(cm.directBlendParameterName);
			AddMotionBlob(cm.motion, ref bb, ref cmb.motion, ref allAnims, allParams);
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddMotionBlob(RTP.Motion m, ref BlobBuilder bb, ref MotionBlob mb, ref BlobBuilderArray<AnimationClipBlob> allAnims, in UnsafeList<RTP.Parameter> allParams)
		{
		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref mb.name, ref m.name);
		#endif

			mb.type = m.type;
			if (m.animationIndex >= 0 && m.type == MotionBlob.Type.AnimationClip)
			{
				ref var ab = ref bb.SetPointer(ref mb.animationBlob, ref allAnims[m.animationIndex]);
			}

			if (m.type != MotionBlob.Type.None && m.type != MotionBlob.Type.AnimationClip)
			{
				ref var bt = ref mb.blendTree;
				var bbm = bb.Allocate(ref bt.motions, m.blendTree.motions.Length);
				for (int i = 0; i < bbm.Length; ++i)
				{
					AddChildMotionBlob(m.blendTree.motions[i], ref bb, ref bbm[i], ref allAnims, allParams);
				}
				bt.blendParameterIndex = allParams.IndexOf(m.blendTree.blendParameterName);
				bt.blendParameterYIndex = allParams.IndexOf(m.blendTree.blendParameterYName);
				bt.normalizeBlendValues = m.blendTree.normalizeBlendValues;

			#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref bt.name, ref m.blendTree.name);
			#endif
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddStateBlob(RTP.State s, ref BlobBuilder bb, ref StateBlob sb, ref BlobBuilderArray<AnimationClipBlob> allAnims, UnsafeList<RTP.Transition> anyStateTransitions, UnsafeList<RTP.State> allStates, UnsafeList<RTP.Parameter> allParams)
		{
		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref sb.name, ref s.name);
		#endif

			sb.hash = s.name.CalculateHash32();
			sb.speed = s.speed;
			sb.speedMultiplierParameterIndex = allParams.IndexOf(s.speedMultiplierParameter);
			sb.timeParameterIndex = allParams.IndexOf(s.timeParameter);
			sb.cycleOffset = s.cycleOffset;
			sb.cycleOffsetParameterIndex = allParams.IndexOf(s.cycleOffsetParameter);

			var bbt = bb.Allocate(ref sb.transitions, s.transitions.Length + anyStateTransitions.Length);

			//	Any state transitions are first priority
			for (int ti = 0; ti < anyStateTransitions.Length; ++ti)
			{
				var ast = anyStateTransitions[ti];
				//	Do not add transitions to self according to flag
				if (ast.canTransitionToSelf || ast.targetStateHash != s.hashCode)
					AddTransitionBlob(ast, allStates, allParams, ref bb, ref bbt[ti]);
			}

			for (int ti = 0; ti < s.transitions.Length; ++ti)
			{
				var src = s.transitions[ti];
				AddTransitionBlob(src, allStates, allParams, ref bb, ref bbt[ti + anyStateTransitions.Length]);
			}

			//	Add motion
			AddMotionBlob(s.motion, ref bb, ref sb.motion, ref allAnims, allParams);
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddKeyFrameArray(UnsafeList<KeyFrame> kf, ref BlobBuilderArray<KeyFrame> outKf)
		{
			for (int i = 0; i < kf.Length; ++i)
			{
				outKf[i] = kf[i];
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddBoneClipArr(ref BlobBuilder bb, ref BlobArray<BoneClipBlob> bonesBlob, in UnsafeList<RTP.BoneClip> inData, in NativeArray<Hash128> hashesArr)
		{
			var bonesArr = bb.Allocate(ref bonesBlob, inData.Length);
			for (int i = 0; i < bonesArr.Length; ++i)
			{
				ref var boneBlob = ref bonesArr[i];
				var boneInData = inData[i];

				var anmCurvesArr = bb.Allocate(ref boneBlob.animationCurves, boneInData.animationCurves.Length);
				for (int l = 0; l < boneInData.animationCurves.Length; ++l)
				{
					var anmCurveData = boneInData.animationCurves[l];
					ref var anmCurveBlob = ref anmCurvesArr[l];
					var keyFramesArr = bb.Allocate(ref anmCurveBlob.keyFrames, anmCurveData.keyFrames.Length);

					anmCurveBlob.channelIndex = anmCurveData.channelIndex;
					anmCurveBlob.bindingType = anmCurveData.bindingType;
					AddKeyFrameArray(anmCurveData.keyFrames, ref keyFramesArr);
				}

		#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref boneBlob.name, ref boneInData.name);
		#endif
				boneBlob.hash = hashesArr[i];
				boneBlob.isHumanMuscleClip = boneInData.isHumanMuscleClip;
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		NativeArray<Hash128> ConstructClipsHashes(in UnsafeList<RTP.BoneClip> boneClips)
		{
			var rv = new NativeArray<Hash128>(boneClips.Length, Allocator.Temp);
			for (var i = 0; i < boneClips.Length; ++i)
			{
				rv[i] = boneClips[i].nameHash;
			}

			return rv;
		}
		
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
		unsafe void AddAnimationClipBlob(RTP.AnimationClip ac, ref BlobBuilder bb, ref AnimationClipBlob acb)
		{
		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref acb.name, ref ac.name);
		#endif

			acb.hash = ac.hash;

			var boneHashes = ConstructClipsHashes(ac.bones);
			var curveHashes = ConstructClipsHashes(ac.curves);

			var boneReinterpretedHashes = boneHashes.Reinterpret<Hash128PerfectHashed>();
			PerfectHash<Hash128PerfectHashed>.CreateMinimalPerfectHash(boneReinterpretedHashes, out var seedValues, out var shuffleIndices);
			MathUtils.ShuffleArray(ac.bones.AsSpan(), shuffleIndices.AsArray());
			MathUtils.ShuffleArray(boneHashes.AsSpan(), shuffleIndices.AsArray());

			var bonePerfectHashSeeds = bb.Allocate(ref acb.bonesPerfectHashSeedTable, seedValues.Length);
			for (var i = 0; i < seedValues.Length; ++i)
				bonePerfectHashSeeds[i] = seedValues[i];
			
			AddBoneClipArr(ref bb, ref acb.bones, ac.bones, boneHashes);
			AddBoneClipArr(ref bb, ref acb.curves, ac.curves, curveHashes);

			acb.looped = ac.looped;
			acb.length = ac.length;
			acb.loopPoseBlend = ac.loopPoseBlend;
			acb.cycleOffset = ac.cycleOffset;
			acb.additiveReferencePoseTime = ac.additiveReferencePoseTime;
			acb.hasRootMotionCurves = ac.hasRootMotionCurves;
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddAvatarMaskBlob(RTP.AvatarMask am, ref BlobBuilder bb, ref AvatarMaskBlob amb)
		{
			amb.hash = am.hash;
			amb.humanBodyPartsAvatarMask = am.humanBodyPartsAvatarMask;

			if (am.name.Length != 0)
			{
			#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref amb.name, ref am.name);
			#endif
			}

			var avatarMaskArr = bb.Allocate(ref amb.includedBoneHashes, am.includedBonePaths.Length);
			for (int i = 0; i < avatarMaskArr.Length; ++i)
			{
				var ibp = am.includedBonePaths[i];
				avatarMaskArr[i] = ibp.CalculateHash128();
			}

		#if RUKHANKA_DEBUG_INFO
			var avatarMaskNameArr = bb.Allocate(ref amb.includedBoneNames, am.includedBonePaths.Length);
			for (int i = 0; i < avatarMaskNameArr.Length; ++i)
			{
				var ibp = am.includedBonePaths[i];
				bb.AllocateString(ref avatarMaskNameArr[i], ref ibp);
			}
		#endif
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddAllAnimationClips(ref BlobBuilder bb, ref ControllerBlob c, in RTP.Controller data, out BlobBuilderArray<AnimationClipBlob> bbc)
		{
			bbc = bb.Allocate(ref c.animationClips, data.animationClips.Length);
			for (int ai = 0; ai < data.animationClips.Length; ++ai)
			{
				var src = data.animationClips[ai];
				ref var clip = ref bbc[ai];
				AddAnimationClipBlob(src, ref bb, ref clip);
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		internal static BlobAssetReference<ParameterPerfectHashTableBlob> CreateParametersPerfectHashTableBlob(in NativeArray<uint> hashesArr)
		{
			var hashesReinterpretedArr = hashesArr.Reinterpret<UIntPerfectHashed>();
			if (!PerfectHash<UIntPerfectHashed>.CreateMinimalPerfectHash(hashesReinterpretedArr, out var seedValues, out var shuffleIndices))
				return default;

			using var bb2 = new BlobBuilder(Allocator.Temp);
			ref var ppb = ref bb2.ConstructRoot<ParameterPerfectHashTableBlob>();
			var bbh = bb2.Allocate(ref ppb.seedTable, hashesArr.Length);
			for (var hi = 0; hi < hashesArr.Length; ++hi)
			{
				ref var paramRef = ref bbh[hi];
				paramRef = seedValues[hi];
			}
		
			var bbia = bb2.Allocate(ref ppb.indirectionTable, shuffleIndices.Length);
			for (var ii = 0; ii < shuffleIndices.Length; ++ii)
			{
				ref var indirectionIndex = ref bbia[ii];
				indirectionIndex = shuffleIndices[ii];
			}

			seedValues.Dispose();
			shuffleIndices.Dispose();

			var rv = bb2.CreateBlobAssetReference<ParameterPerfectHashTableBlob>(Allocator.Persistent);

			return rv;
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		BlobAssetReference<ParameterPerfectHashTableBlob> AddAllParameters(ref BlobBuilder bb, ref ControllerBlob c, RTP.Controller data)
		{
			//	Create perfect hash table and indirection array
			var hashesArr = new NativeArray<uint>(data.parameters.Length, Allocator.Temp);
			for (int l = 0; l < data.parameters.Length; ++l)
			{
				hashesArr[l] = data.parameters[l].name.CalculateHash32();
			}

			//	Now place parameters in its original places as in authoring animator
			var bba = bb.Allocate(ref c.parameters, data.parameters.Length);
			for	(int pi = 0; pi < data.parameters.Length; ++pi)
			{
				var src = data.parameters[pi];
				ref var p = ref bba[pi];
				p.defaultValue = src.defaultValue;
#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref p.name, ref src.name);
#endif
				p.hash = hashesArr[pi];
				p.type = src.type;
			}

			//	Create separate blob asset for perfect hash table, but only if number of parameters is big enough
			var rv = new BlobAssetReference<ParameterPerfectHashTableBlob>();
			if (data.parameters.Length > 10)
				rv = CreateParametersPerfectHashTableBlob(hashesArr);

			hashesArr.Dispose();

			return rv;
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void AddAllLayers(ref BlobBuilder bb, ref ControllerBlob c, ref BlobBuilderArray<AnimationClipBlob> bbc, RTP.Controller data)
		{
			var bbl = bb.Allocate(ref c.layers, data.layers.Length);
			for (int li = 0; li < data.layers.Length; ++li)
			{
				var src = data.layers[li];
				ref var l = ref bbl[li];

			#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref l.name, ref src.name);
			#endif

				l.defaultStateIndex = src.defaultStateIndex;
				l.blendingMode = src.blendMode;

				// States
				var bbs = bb.Allocate(ref l.states, src.states.Length);
				for (int si = 0; si < src.states.Length; ++si)
				{
					var s = src.states[si];
					AddStateBlob(s, ref bb, ref bbs[si], ref bbc, src.anyStateTransitions, src.states, data.parameters);
				}

				if (src.avatarMask.hash.IsValid)
					AddAvatarMaskBlob(src.avatarMask, ref bb, ref l.avatarMask);
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute()
		{
			var data = inData.controllerData;
			var bb = new BlobBuilder(Allocator.Temp);
			ref var c = ref bb.ConstructRoot<ControllerBlob>();

		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref c.name, ref data.name);
		#endif

			AddAllAnimationClips(ref bb, ref c, data, out var bbc);
			var parameterPerfectHashTableBlob = AddAllParameters(ref bb, ref c, data);
			AddAllLayers(ref bb, ref c, ref bbc, data);

			var rv = bb.CreateBlobAssetReference<ControllerBlob>(Allocator.Persistent);

			//	Entire slice has same blob assets
			for (var i = 0; i < outBlobAssets.Length; ++i)
			{
				outBlobAssets[i] = new AnimatorBlobAssets() { controllerBlob = rv, parametersPerfectHashTableBlob = parameterPerfectHashTableBlob };
			}

		#if RUKHANKA_DEBUG_INFO
			if (outBlobAssets.Length > 0 && doLogging)
				LogAnimatorBakeing(outBlobAssets[0]);
		#endif
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void LogAnimatorBakeing(AnimatorBlobAssets aba)
		{
		#if RUKHANKA_DEBUG_INFO
			Debug.Log($"BAKING Controller: {aba.controllerBlob.Value.name.ToFixedString()}");
			ref var cb = ref aba.controllerBlob.Value;
			for (var i = 0; i < cb.layers.Length; ++i)
			{
				ref var l = ref cb.layers[i];
				Debug.Log($"-> Layer `{l.name.ToFixedString()}` states count: {l.states.Length}, blend mode: {(int)l.blendingMode}, default state index: {l.defaultStateIndex}");

				for (var k = 0; k < l.states.Length; ++k)
				{
					ref var state = ref l.states[k];
					Debug.Log($"--> State `{state.name.ToFixedString()}` motion: {state.motion.name.ToFixedString()}, cycle offset: {state.cycleOffset}, speed: {state.speed}, speed param index: {state.speedMultiplierParameterIndex}, time param index: {state.timeParameterIndex}, transitions count: {state.transitions.Length}");

					for (var m = 0; m < state.transitions.Length; ++m)
					{
						ref var tr = ref state.transitions[m];
						Debug.Log($"---> Transition `{tr.name.ToFixedString()}` duration: {tr.duration}, offset: {tr.offset}, has fixed duration: {tr.hasFixedDuration}, has exit time: {tr.hasExitTime}, exit time: {tr.exitTime}, offset: {tr.offset}, conditions count: {tr.conditions.Length}");

						for (var n = 0; n < tr.conditions.Length; ++n)
						{
							ref var cnd = ref tr.conditions[n];
							Debug.Log($"----> Condition `{cnd.name.ToFixedString()}` mode: {(int)cnd.conditionMode}, param index: {cnd.paramIdx}, threshold: {cnd.threshold.floatValue}");
						}
					}
				}
			}

			ref var pms = ref cb.parameters;
			Debug.Log($"Total parameters count: {pms.Length}");

			for (var i = 0; i < pms.Length; ++i)
			{
				ref var pm = ref pms[i];
				switch (pm.type)
				{
				case ControllerParameterType.Int:
					Debug.Log($"Parameter `{pm.name.ToFixedString()}` type: Int, default value: {pm.defaultValue.intValue}");
					break;
				case ControllerParameterType.Float:
					Debug.Log($"Parameter `{pm.name.ToFixedString()}` type: Float, default value: {pm.defaultValue.floatValue}");
					break;
				case ControllerParameterType.Bool:
					Debug.Log($"Parameter `{pm.name.ToFixedString()}` type: Bool, default value: {pm.defaultValue.boolValue}");
					break;
				case ControllerParameterType.Trigger:
					Debug.Log($"Parameter `{pm.name.ToFixedString()}` type: Trigger, default value: {pm.defaultValue.boolValue}");
					break;
				}
			}

			ref var anms = ref cb.animationClips;
			Debug.Log($"Total animation clips: {anms.Length}");

			for (var i = 0; i < anms.Length; ++i)
			{
				ref var anm = ref anms[i];
				Debug.Log($"Animation `{anm.name.ToFixedString()}`");
			}

			Debug.Log($"END Controller: {aba.controllerBlob.Value.name.ToFixedString()}");
		#endif
		}
	}

//=================================================================================================================//

	[BurstCompile]
	struct CreateComponentDatasJob: IJobParallelForBatch
	{
		[ReadOnly]
		public NativeArray<AnimatorControllerBakerData> bakerData;
		[ReadOnly]
		public NativeArray<AnimatorBlobAssets> blobAssets;

		public EntityCommandBuffer.ParallelWriter ecb;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute(int startIndex, int count)
		{
			for (int i = startIndex; i < startIndex + count; ++i)
			{
				var bd = bakerData[i];
				var e = bd.targetEntity;
				var ba = blobAssets[i];

				var acc = new AnimatorControllerLayerComponent();
				acc.rtd = RuntimeAnimatorData.MakeDefault();
				acc.controller = ba.controllerBlob;

				var buf = ecb.AddBuffer<AnimatorControllerLayerComponent>(startIndex, e);
				ref var cb = ref ba.controllerBlob.Value;
				for (int k = 0; k < cb.layers.Length; ++k)
				{
					acc.layerIndex = k;
					acc.weight = bd.controllerData.layers[k].weight;
					buf.Add(acc);
				}

				//	Add animation to process buffer
				ecb.AddBuffer<AnimationToProcessComponent>(startIndex, e);

				if (cb.parameters.Length > 0)
				{
					//	Add dynamic parameters
					var paramArray = ecb.AddBuffer<AnimatorControllerParameterComponent>(startIndex, e);
					for (int p = 0; p < cb.parameters.Length; ++p)
					{
						ref var pm = ref cb.parameters[p];
						var acpc = new AnimatorControllerParameterComponent()
						{
							value = pm.defaultValue,
							hash = pm.hash,
							type = pm.type,
						};

					#if RUKHANKA_DEBUG_INFO
						pm.name.CopyTo(ref acpc.name);
					#endif

						paramArray.Add(acpc);
					}

					if (ba.parametersPerfectHashTableBlob.IsCreated)
					{
						//	Add perfect hash table used to fast runtime parameter value lookup
						var pht = new AnimatorControllerParameterIndexTableComponent()
						{
							seedTable = ba.parametersPerfectHashTableBlob
						};
						ecb.AddComponent(startIndex, e, pht);
					}
				}
			}
		}
	}
}
}

#endif
