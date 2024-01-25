
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

//=================================================================================================================//
[assembly: InternalsVisibleTo("Rukhanka.Tests")]

namespace Rukhanka
{
partial struct AnimationProcessSystem
{

[BurstCompile]
public struct ComputeBoneAnimationJob: IJobParallelForDefer
{
	[NativeDisableParallelForRestriction]
	public NativeList<BoneTransform> animatedBonesBuffer;
	[NativeDisableParallelForRestriction]
	public NativeList<ulong> boneTransformFlagsArr;
	[ReadOnly]
	public NativeList<int3> boneToEntityArr;
	[ReadOnly]
	public BufferLookup<AnimationToProcessComponent> animationsToProcessLookup;
	[ReadOnly]
	public NativeList<RigDefinitionComponent> rigDefs;
	[ReadOnly]
	public NativeList<Entity> entityArr;
	
	[NativeDisableParallelForRestriction]
	public BufferLookup<RootMotionAnimationStateComponent> rootMotionAnimStateBufferLookup;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute(int globalBoneIndex)
	{
		var boneToEntityIndex = boneToEntityArr[globalBoneIndex];
		var (rigBoneIndex, entityIndex) = (boneToEntityIndex.y, boneToEntityIndex.x);
		var e = entityArr[entityIndex];

		var rigDef = rigDefs[entityIndex];
		var rigBlobAsset = rigDef.rigBlob;
		ref var rb = ref rigBlobAsset.Value.bones[rigBoneIndex];
		var animationsToProcess = animationsToProcessLookup[e];

		//	Early exit if no animations
		if (animationsToProcess.IsEmpty)
			return;

		var transformFlags = RuntimeAnimationData.GetAnimationTransformFlagsRW(boneToEntityArr, boneTransformFlagsArr, globalBoneIndex, rigBlobAsset.Value.bones.Length);
		GetHumanRotationDataForSkeletonBone(out var humanBoneInfo, ref rigBlobAsset.Value.humanData, rigBoneIndex);

		Span<float> layerWeights = stackalloc float[32];
		var refPosWeight = CalculateFinalLayerWeights(layerWeights, animationsToProcess, rb.hash, rb.humanBodyPart);
		float3 totalWeights = refPosWeight;

		var blendedBonePose = BoneTransform.Scale(rb.refPose, refPosWeight);

		var rootMotionDeltaBone = rigDef.applyRootMotion && rigBoneIndex == 0;
		PrepareRootMotionStateBuffers(e, animationsToProcess, out var curRootMotionState, out var newRootMotionState, rootMotionDeltaBone);

		for (int i = 0; i < animationsToProcess.Length; ++i)
		{
			var atp = animationsToProcess[i];

			var animTime = NormalizeAnimationTime(atp.time, ref atp.animation.Value);

			var layerWeight = layerWeights[atp.layerIndex];
			if (layerWeight == 0) continue;

			var boneNameHash = rb.hash;
			if (rigDef.applyRootMotion && (rigBlobAsset.Value.rootBoneIndex == rigBoneIndex || rigBoneIndex == 0))
				ModifyBoneHashForRootMotion(ref boneNameHash);
			
			var animationBoneIndex = GetBoneIndexByHash(ref atp.animation.Value, boneNameHash);

			if (Hint.Likely(animationBoneIndex >= 0))
			{
				// Loop Pose calculus for all bones except root motion
				var calculateLoopPose = atp.animation.Value.loopPoseBlend && rigBoneIndex != 0;
				var additiveReferencePoseTime = math.select(-1.0f, atp.animation.Value.additiveReferencePoseTime, atp.blendMode == AnimationBlendingMode.Additive);
				
				ref var boneAnimation = ref atp.animation.Value.bones[animationBoneIndex];
				var (bonePose, flags) = SampleAnimation(ref boneAnimation, animTime, atp, calculateLoopPose, additiveReferencePoseTime, humanBoneInfo);
				SetTransformFlags(flags, transformFlags, rigBoneIndex);

				float3 modWeight = flags * atp.weight * layerWeight;
				totalWeights += modWeight;

				if (rootMotionDeltaBone)
					ProcessRootMotionDeltas(ref bonePose, ref boneAnimation, atp, i, curRootMotionState, newRootMotionState);
				
				MixPoses(ref blendedBonePose, bonePose, modWeight, atp.blendMode);
			}
		}

		//	Reference pose for root motion delta should be identity
		var boneRefPose = Hint.Unlikely(rootMotionDeltaBone) ? BoneTransform.Identity() : rb.refPose;
		
		BoneTransformMakePretty(ref blendedBonePose, boneRefPose, totalWeights);
		animatedBonesBuffer[globalBoneIndex] = blendedBonePose;

		if (rootMotionDeltaBone)
			SetRootMotionStateToComponentBuffer(newRootMotionState, e);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void ModifyBoneHashForRootMotion(ref Hash128 h)
	{
		h.Value.z = 0xbaad;
		h.Value.w = 0xf00d;
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	int GetBoneIndexByHash(ref AnimationClipBlob acb, in Hash128 boneHash)
	{
		var queryIndex = PerfectHash<Hash128PerfectHashed>.QueryPerfectHashTable(ref acb.bonesPerfectHashSeedTable, boneHash);
		if (queryIndex >= acb.bones.Length || queryIndex < 0)
			return -1;
		var candidateBoneHash = acb.bones[queryIndex].hash;
		return candidateBoneHash == boneHash ? queryIndex : -1;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void PrepareRootMotionStateBuffers
	(
		Entity e,
		in DynamicBuffer<AnimationToProcessComponent> atps,
		out NativeArray<RootMotionAnimationStateComponent> curRootMotionState,
		out NativeArray<RootMotionAnimationStateComponent> newRootMotionState,
		bool isRootMotionBone
	)
	{
		curRootMotionState = default;
		newRootMotionState = default;

		if (Hint.Likely(!isRootMotionBone)) return;

		if (rootMotionAnimStateBufferLookup.HasBuffer(e))
			curRootMotionState = rootMotionAnimStateBufferLookup[e].AsNativeArray();

		newRootMotionState = new NativeArray<RootMotionAnimationStateComponent>(atps.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ProcessRootMotionDeltas
	(
		ref BoneTransform bonePose,
		ref BoneClipBlob boneAnimation,
		in AnimationToProcessComponent atp,
		int animationIndex,
		in NativeArray<RootMotionAnimationStateComponent> curRootMotionState,
		NativeArray<RootMotionAnimationStateComponent> newRootMotionState
	)
	{
		//	Special care for root motion animation loops
		HandleRootMotionLoops(ref bonePose, ref boneAnimation, atp);
	
		BoneTransform rootMotionPrevPose = bonePose;

		// Find animation history in history buffer
		var historyBufferIndex = 0;
		for (; curRootMotionState.IsCreated && historyBufferIndex < curRootMotionState.Length && curRootMotionState[historyBufferIndex].animationHash != atp.animation.Value.hash; ++historyBufferIndex){ }

		var initialFrame = historyBufferIndex >= curRootMotionState.Length;

		if (Hint.Unlikely(!initialFrame))
		{
			rootMotionPrevPose = curRootMotionState[historyBufferIndex].animationState;
		}

		newRootMotionState[animationIndex] = new RootMotionAnimationStateComponent() { animationHash = atp.animation.Value.hash, animationState = bonePose };

		var invPrevPose = BoneTransform.Inverse(rootMotionPrevPose);
		var deltaPose = BoneTransform.Multiply(invPrevPose, bonePose);

		bonePose = deltaPose;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetRootMotionStateToComponentBuffer(in NativeArray<RootMotionAnimationStateComponent> newRootMotionData, Entity e)
	{
		rootMotionAnimStateBufferLookup[e].CopyFrom(newRootMotionData);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetTransformFlags(float3 flags, in AnimationTransformFlags flagArr, int boneIndex)
	{
		if (flags.x > 0)
			flagArr.SetTranslationFlag(boneIndex);
		if (flags.y > 0)
			flagArr.SetRotationFlag(boneIndex);
		if (flags.z > 0)
			flagArr.SetScaleFlag(boneIndex);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void GetHumanRotationDataForSkeletonBone(out HumanRotationData rv, ref BlobPtr<HumanData> hd, int rigBoneIndex)
	{
		rv = default;
		if (hd.IsValid)
		{
			rv = hd.Value.humanRotData[rigBoneIndex];
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	internal static float3 MuscleRangeToRadians(float3 minA, float3 maxA, float3 muscle)
	{
		//	Map [-1; +1] range into [minRot; maxRot]
		var negativeRange = math.min(muscle, 0);
		var positiveRange = math.max(0, muscle);
		var negativeRot = math.lerp(0, minA, -negativeRange);
		var positiveRot = math.lerp(0, maxA, +positiveRange);

		var rv = negativeRot + positiveRot;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MuscleValuesToQuaternion(in HumanRotationData humanBoneInfo, ref BoneTransform bt)
	{
		var r = MuscleRangeToRadians(humanBoneInfo.minMuscleAngles, humanBoneInfo.maxMuscleAngles, bt.rot.value.xyz);
		r *= humanBoneInfo.sign;

		var qx = quaternion.AxisAngle(math.right(), r.x);
		var qy = quaternion.AxisAngle(math.up(), r.y);
		var qz = quaternion.AxisAngle(math.forward(), r.z);
		var qzy = math.mul(qz, qy);
		qzy.value.x = 0;
		bt.rot = math.mul(math.normalize(qzy), qx);

		ApplyHumanoidPostTransform(humanBoneInfo, ref bt);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static float2 NormalizeAnimationTime(float at, ref AnimationClipBlob ac)
	{
		at += ac.cycleOffset;
		var normalizedTime = ac.looped ? math.frac(at) : math.saturate(at);
		var rv = normalizedTime * ac.length;
		return new (rv, normalizedTime);
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void CalculateLoopPose(ref BoneClipBlob boneAnimation, AnimationToProcessComponent atp, ref BoneTransform bonePose, in HumanRotationData hrd, float normalizedTime)
	{
		var animLen = atp.animation.Value.length;
		var lerpFactor = normalizedTime;
		var (rootPoseStart, _) = ProcessAnimationCurves(ref boneAnimation, hrd, 0);
		var (rootPoseEnd, _) = ProcessAnimationCurves(ref boneAnimation, hrd, animLen);

		var dPos = rootPoseEnd.pos - rootPoseStart.pos;
		var dRot = math.mul(math.conjugate(rootPoseEnd.rot), rootPoseStart.rot);
		bonePose.pos -= dPos * lerpFactor;
		bonePose.rot = math.mul(bonePose.rot, math.slerp(quaternion.identity, dRot, lerpFactor));
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void HandleRootMotionLoops(ref BoneTransform bonePose, ref BoneClipBlob boneAnimation, in AnimationToProcessComponent atp)
	{
		ref var animBlob = ref atp.animation.Value;
		if (!animBlob.looped)
			return;

		var numLoopCycles = (int)math.floor(atp.time + atp.animation.Value.cycleOffset);
		if (numLoopCycles < 1)
			return;

		var animLen = atp.animation.Value.length;
		var (endFramePose, _) = SampleAnimation(ref boneAnimation, animLen, atp, false, -1);
		var (startFramePose, _) = SampleAnimation(ref boneAnimation, 0, atp, false, -1);

		var deltaPose = BoneTransform.Multiply(endFramePose, BoneTransform.Inverse(startFramePose));

		BoneTransform accumCyclePose = BoneTransform.Identity();
		for (var c = numLoopCycles; c > 0; c >>= 1)
		{
			if ((c & 1) == 1)
				accumCyclePose = BoneTransform.Multiply(accumCyclePose, deltaPose);
			deltaPose = BoneTransform.Multiply(deltaPose, deltaPose);
		}
		bonePose = BoneTransform.Multiply(accumCyclePose, bonePose);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MixPoses(ref BoneTransform curPose, BoneTransform inPose, float3 weight, AnimationBlendingMode blendMode)
	{
		if (blendMode == AnimationBlendingMode.Override)
		{
			inPose.rot = MathUtils.ShortestRotation(curPose.rot, inPose.rot);
			var scaledPose = BoneTransform.Scale(inPose, weight);

			curPose.pos += scaledPose.pos;
			curPose.rot.value += scaledPose.rot.value;
			curPose.scale += scaledPose.scale;
		}
		else
		{
			curPose.pos += inPose.pos * weight.x;
			quaternion layerRot = math.normalizesafe(new float4(inPose.rot.value.xyz * weight.y, inPose.rot.value.w));
			layerRot = MathUtils.ShortestRotation(curPose.rot, layerRot);
			curPose.rot = math.mul(layerRot, curPose.rot);
			curPose.scale *= (1 - weight.z) + (inPose.scale * weight.z);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static float CalculateFinalLayerWeights(in Span<float> layerWeights, in DynamicBuffer<AnimationToProcessComponent> atp, in Hash128 boneHash, AvatarMaskBodyPart humanAvatarMaskBodyPart)
	{
		var layerIndex = -1;
		var w = 1.0f;
		var refPoseWeight = 1.0f;

		for (int i = atp.Length - 1; i >= 0; --i)
		{
			var a = atp[i];
			if (a.layerIndex == layerIndex) continue;

			var inAvatarMask = IsBoneInAvatarMask(boneHash, humanAvatarMaskBodyPart, a.avatarMask);
			var layerWeight = inAvatarMask ? a.layerWeight : 0;

			var lw = w * layerWeight;
			layerWeights[a.layerIndex] = lw;
			refPoseWeight -= lw;
			if (a.blendMode == AnimationBlendingMode.Override)
				w = w * (1 - layerWeight);
			layerIndex = a.layerIndex;
		}
		return atp[0].blendMode == AnimationBlendingMode.Override ? 0 : layerWeights[0];
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ApplyHumanoidPostTransform(HumanRotationData hrd, ref BoneTransform bt)
	{
		bt.rot = math.mul(math.mul(hrd.preRot, bt.rot), hrd.postRot);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void BoneTransformMakePretty(ref BoneTransform bt, BoneTransform refPose, float3 weights)
	{
		var complWeights = math.saturate(new float3(1) - weights);
		bt.pos += refPose.pos * complWeights.x;
		var shortestRefRot = MathUtils.ShortestRotation(bt.rot.value, refPose.rot.value);
		bt.rot.value += shortestRefRot.value * complWeights.y;
		bt.scale += refPose.scale * complWeights.z;

		bt.rot = math.normalize(bt.rot);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static bool IsBoneInAvatarMask(in Hash128 boneHash, AvatarMaskBodyPart humanAvatarMaskBodyPart, ExternalBlobPtr<AvatarMaskBlob> am)
	{
		// If no avatar mask defined or bone hash is all zeroes assume that bone included
		if (!am.IsCreated || !math.any(boneHash.Value))
			return true;

		return (int)humanAvatarMaskBodyPart >= 0 ?
			IsBoneInHumanAvatarMask(humanAvatarMaskBodyPart, am) :
			IsBoneInGenericAvatarMask(boneHash, am);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static bool IsBoneInHumanAvatarMask(AvatarMaskBodyPart humanBoneAvatarMaskIndex, ExternalBlobPtr<AvatarMaskBlob> am)
	{
		var rv = (am.Value.humanBodyPartsAvatarMask & 1 << (int)humanBoneAvatarMaskIndex) != 0;
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static bool IsBoneInGenericAvatarMask(in Hash128 boneHash, ExternalBlobPtr<AvatarMaskBlob> am)
	{
		for (int i = 0; i < am.Value.includedBoneHashes.Length; ++i)
		{
			var avatarMaskBoneHash = am.Value.includedBoneHashes[i];
			if (avatarMaskBoneHash == boneHash)
				return true;
		}
		return false;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	(BoneTransform, float3) SampleAnimation
	(
		ref BoneClipBlob bcb,
		float2 animTime,
		in AnimationToProcessComponent atp,
		bool calculateLoopPose, 
		float additiveReferencePoseTime,
		in HumanRotationData hrd = default
	)
	{
		var time = animTime.x;
		var timeNrm = animTime.y;

		var (bonePose, flags) = ProcessAnimationCurves(ref bcb, hrd, time);
		
		//	Make additive animation if requested
		if (Hint.Unlikely(additiveReferencePoseTime >= 0))
		{
			var (zeroFramePose, _) = ProcessAnimationCurves(ref bcb, hrd, additiveReferencePoseTime);
			MakeAdditiveAnimation(ref bonePose, zeroFramePose);
		}
		
		if (Hint.Unlikely(calculateLoopPose))
		{
			CalculateLoopPose(ref bcb, atp, ref bonePose, hrd, timeNrm);
		}
		
		return (bonePose, flags);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MakeAdditiveAnimation(ref BoneTransform rv, in BoneTransform zeroFramePose)
	{
		//	If additive layer make difference between reference pose and current animated pose
		rv.pos = rv.pos - zeroFramePose.pos;
		var conjugateZFRot = math.normalizesafe(math.conjugate(zeroFramePose.rot));
		conjugateZFRot = MathUtils.ShortestRotation(rv.rot, conjugateZFRot);
		rv.rot = math.mul(math.normalize(rv.rot), conjugateZFRot);
		rv.scale = rv.scale / zeroFramePose.scale;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	(BoneTransform, float3) ProcessAnimationCurves(ref BoneClipBlob bcb, HumanRotationData hrd, float time)
	{
		var rv = BoneTransform.Identity();

		bool eulerToQuaternion = false;

		float3 flags = 0;
		for (int i = 0; i < bcb.animationCurves.Length; ++i)
		{
			ref var ac = ref bcb.animationCurves[i];
			var interpolatedCurveValue = BlobCurve.SampleAnimationCurve(ref ac.keyFrames, time);

			switch (ac.bindingType)
			{
			case BindingType.Translation:
				rv.pos[ac.channelIndex] = interpolatedCurveValue;
				flags.x = 1;
				break;
			case BindingType.Quaternion:
				rv.rot.value[ac.channelIndex] = interpolatedCurveValue;
				flags.y = 1;
				break;
			case BindingType.EulerAngles:
				eulerToQuaternion = true;
				rv.rot.value[ac.channelIndex] = interpolatedCurveValue;
				flags.y = 1;
				break;
			case BindingType.HumanMuscle:
				rv.rot.value[ac.channelIndex] = interpolatedCurveValue;
				flags.y = 1;
				break;
			case BindingType.Scale:
				rv.scale[ac.channelIndex] = interpolatedCurveValue;
				flags.z = 1;
				break;
			default:
				Debug.Assert(false, "Unknown binding type!");
				break;
			}
		}

		//	If we have got Euler angles instead of quaternion, convert them here
		if (eulerToQuaternion)
		{
			rv.rot = quaternion.Euler(math.radians(rv.rot.value.xyz));
		}

		if (bcb.isHumanMuscleClip)
		{
			MuscleValuesToQuaternion(hrd, ref rv);
		}

		return (rv, flags);
	}
}

//=================================================================================================================//

[BurstCompile]
partial struct ProcessUserCurvesJob: IJobEntity
{
	void Execute(AnimatorParametersAspect apa, in DynamicBuffer<AnimationToProcessComponent> animationsToProcess)
	{
		if (animationsToProcess.IsEmpty) return;

		Span<float> layerWeights = stackalloc float[32];
		var isSetByCurve = new BitField64();
		Span<float> finalParamValues = stackalloc float[apa.ParametersCount()];
		finalParamValues.Clear();

		ComputeBoneAnimationJob.CalculateFinalLayerWeights(layerWeights, animationsToProcess, new Hash128(), (AvatarMaskBodyPart)(-1));

		for (int l = 0; l < animationsToProcess.Length; ++l)
		{
			var atp = animationsToProcess[l];
			var animTime = ComputeBoneAnimationJob.NormalizeAnimationTime(atp.time, ref atp.animation.Value);
			var layerWeight = layerWeights[atp.layerIndex];
			ref var curves = ref atp.animation.Value.curves;
			for (int k = 0; k < curves.Length; ++k)
			{
				ref var c = ref curves[k];
				var paramHash = c.hash.Value.x;
				var paramIdx = apa.GetParameterIndex(new FastAnimatorParameter(paramHash));
				if (paramIdx < 0) continue;

				isSetByCurve.SetBits(paramIdx, true);
				var curveValue = SampleUserCurve(ref c.animationCurves[0].keyFrames, atp, animTime.x);

				if (atp.animation.Value.loopPoseBlend)
					curveValue -= CalculateLoopPose(ref c.animationCurves[0].keyFrames, atp, animTime.y);

				finalParamValues[paramIdx] += curveValue * atp.weight * layerWeight;
			}
		}

		for (int l = 0; l < apa.ParametersCount(); ++l)
		{
			if (isSetByCurve.GetBits(l) == 0) continue;
			apa.SetParameterValueByIndex(l, finalParamValues[l]);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	float SampleUserCurve(ref BlobArray<KeyFrame> curve, in AnimationToProcessComponent atp, float animTime)
	{ 
		var curveValue = BlobCurve.SampleAnimationCurve(ref curve, animTime);
		//	Make additive animation if requested
		if (atp.blendMode == AnimationBlendingMode.Additive)
		{
			var additiveValue = BlobCurve.SampleAnimationCurve(ref curve, atp.animation.Value.additiveReferencePoseTime);
			curveValue -= additiveValue;
		}
		return curveValue;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	float CalculateLoopPose(ref BlobArray<KeyFrame> curve, in AnimationToProcessComponent atp, float normalizedTime)
	{
		var startV = SampleUserCurve(ref curve, atp, 0);
		var endV = SampleUserCurve(ref curve, atp, atp.animation.Value.length);

		var rv = (endV - startV) * normalizedTime;
		return rv;
	}
}

//=================================================================================================================//

[BurstCompile]
struct CalculateBoneOffsetsJob: IJobChunk
{
	[ReadOnly]
	public ComponentTypeHandle<RigDefinitionComponent> rigDefinitionTypeHandle;
	[ReadOnly]
	public NativeArray<int> chunkBaseEntityIndices;
	
	[WriteOnly, NativeDisableContainerSafetyRestriction]
	public NativeList<int2> bonePosesOffsets;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
	{
		var rigDefAccessor = chunk.GetNativeArray(ref rigDefinitionTypeHandle);
		int baseEntityIndex = chunkBaseEntityIndices[unfilteredChunkIndex];
		int validEntitiesInChunk = 0;

		var cee = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
		bonePosesOffsets[0] = 0;

		while (cee.NextEntityIndex(out var i))
		{
			var rigDef = rigDefAccessor[i];

			int entityInQueryIndex = baseEntityIndex + validEntitiesInChunk;
            ++validEntitiesInChunk;

			var boneCount = rigDef.rigBlob.Value.bones.Length;

			var v = new int2
			(
				//	Bone count
				boneCount,
				//	Number of ulong values that can hold bone transform flags
				(boneCount * 4 >> 6) + 1
			);
			bonePosesOffsets[entityInQueryIndex + 1] = v;
		}
	}
}

//=================================================================================================================//

[BurstCompile]
struct CalculatePerBoneInfoJob: IJobChunk
{
	[ReadOnly]
	public ComponentTypeHandle<RigDefinitionComponent> rigDefinitionTypeHandle;
	[ReadOnly]
	public NativeArray<int> chunkBaseEntityIndices;
	[ReadOnly]
	public NativeList<int2> bonePosesOffsets;
	[ReadOnly]
	public NativeList<Entity> entities;
	[WriteOnly, NativeDisableContainerSafetyRestriction]
	public NativeList<int3> boneToEntityIndices;
	[WriteOnly]
	public NativeParallelHashMap<Entity, int2>.ParallelWriter entityToDataOffsetMap;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
	{
		var rigDefAccessor = chunk.GetNativeArray(ref rigDefinitionTypeHandle);
		int baseEntityIndex = chunkBaseEntityIndices[unfilteredChunkIndex];
		int validEntitiesInChunk = 0;

		var cee = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

		while (cee.NextEntityIndex(out var i))
		{
			var rigDef = rigDefAccessor[i];
			int entityInQueryIndex = baseEntityIndex + validEntitiesInChunk;
            ++validEntitiesInChunk;
			var offset = bonePosesOffsets[entityInQueryIndex];

			for (int k = 0, l = rigDef.rigBlob.Value.bones.Length; k < l; ++k)
			{
				boneToEntityIndices[k + offset.x] = new int3(entityInQueryIndex, k, offset.y);
			}

			entityToDataOffsetMap.TryAdd(entities[entityInQueryIndex], offset);
		}
	}
}

//=================================================================================================================//

[BurstCompile]
struct DoPrefixSumJob: IJob
{
	public NativeList<int2> boneOffsets;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute()
	{
		var sum = new int2(0);
		for (int i = 0; i < boneOffsets.Length; ++i)
		{
			var v = boneOffsets[i];
			sum += v;
			boneOffsets[i] = sum;
		}
	}
}

//=================================================================================================================//

[BurstCompile]
struct ResizeDataBuffersJob: IJob
{
	[ReadOnly]
	public NativeList<int2> boneOffsets;
	public RuntimeAnimationData runtimeData;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute()
	{
		var boneBufferLen = boneOffsets[^1];
		runtimeData.animatedBonesBuffer.Resize(boneBufferLen.x, NativeArrayOptions.UninitializedMemory);
		runtimeData.boneToEntityArr.Resize(boneBufferLen.x, NativeArrayOptions.UninitializedMemory);

		//	Clear flags by two resizes
		runtimeData.boneTransformFlagsHolderArr.Resize(0, NativeArrayOptions.UninitializedMemory);
		runtimeData.boneTransformFlagsHolderArr.Resize(boneBufferLen.y, NativeArrayOptions.ClearMemory);
	}
}

//=================================================================================================================//

[BurstCompile]
struct ClearEntityToDataOffsetHashMap: IJob
{
    public NativeParallelHashMap<Entity, int2> entityToDataOffsetMap;
    public int entityCount;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Execute()
	{
		entityToDataOffsetMap.Clear();
		entityToDataOffsetMap.Capacity = math.max(entityCount, entityToDataOffsetMap.Capacity);
	}
}

//=================================================================================================================//

[BurstCompile]
partial struct CopyEntityBoneTransformsToAnimationBuffer: IJobEntity
{
	[WriteOnly, NativeDisableContainerSafetyRestriction]
	public NativeList<BoneTransform> animatedBoneTransforms;
	[ReadOnly]
	public ComponentLookup<RigDefinitionComponent> rigDefComponentLookup;
	[ReadOnly]
	public ComponentLookup<Parent> parentComponentLookup;
	[NativeDisableContainerSafetyRestriction]
	public NativeList<ulong> boneTransformFlags;
	[ReadOnly]
	public NativeParallelHashMap<Entity, int2> entityToDataOffsetMap;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Execute(Entity e, in AnimatorEntityRefComponent aer, in LocalTransform lt)
	{
		if (!rigDefComponentLookup.TryGetComponent(aer.animatorEntity, out var rdc))
			return;

		var boneOffset = RuntimeAnimationData.CalculateBufferOffset(entityToDataOffsetMap, aer.animatorEntity);
		if (boneOffset.x < 0)
			return;
		
		var len = rdc.rigBlob.Value.bones.Length;

		var bonePoses = RuntimeAnimationData.GetAnimationDataForRigRW(animatedBoneTransforms, boneOffset.x, len);
		var transformFlags = AnimationTransformFlags.CreateFromBufferRW(boneTransformFlags, boneOffset.y, len);
		var boneFlags = new bool3
		(
			transformFlags.IsTranslationSet(aer.boneIndexInAnimationRig),
			transformFlags.IsRotationSet(aer.boneIndexInAnimationRig),
			transformFlags.IsScaleSet(aer.boneIndexInAnimationRig)
		);

		if (!math.any(boneFlags))
		{
			var entityPose = new BoneTransform(lt);
			//	Root motion delta should be zero
			if (rdc.applyRootMotion && aer.boneIndexInAnimationRig == 0)
				entityPose = BoneTransform.Identity();
			
			//	For entities without parent we indicate that bone pose is in world space
			if (!parentComponentLookup.HasComponent(e))
				transformFlags.SetAbsoluteTransformFlag(aer.boneIndexInAnimationRig);

			ref var bonePose = ref bonePoses[aer.boneIndexInAnimationRig];

			if (!boneFlags.x)
				bonePose.pos = entityPose.pos;
			if (!boneFlags.y)
				bonePose.rot = entityPose.rot;
			if (!boneFlags.z)
				bonePose.scale = entityPose.scale;
		}
	}
}
}
}
