
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public struct RuntimeAnimationData: IComponentData
{
    public NativeList<BoneTransform> animatedBonesBuffer;
    public NativeParallelHashMap<Entity, int2> entityToDataOffsetMap;
    public NativeList<int3> boneToEntityArr;
	public NativeList<ulong> boneTransformFlagsHolderArr;

/////////////////////////////////////////////////////////////////////////////////

	public static RuntimeAnimationData MakeDefault()
	{
		var rv = new RuntimeAnimationData()
		{
			animatedBonesBuffer = new NativeList<BoneTransform>(Allocator.Persistent),
			entityToDataOffsetMap = new NativeParallelHashMap<Entity, int2>(128, Allocator.Persistent),
			boneToEntityArr = new NativeList<int3>(Allocator.Persistent),
			boneTransformFlagsHolderArr = new NativeList<ulong>(Allocator.Persistent)
		};
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

	public void Dispose()
	{
		animatedBonesBuffer.Dispose();
		entityToDataOffsetMap.Dispose();
		boneToEntityArr.Dispose();
		boneTransformFlagsHolderArr.Dispose();
	}

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 CalculateBufferOffset(in NativeParallelHashMap<Entity, int2> entityToDataOffsetMap, Entity animatedRigEntity)
	{
		if (!entityToDataOffsetMap.TryGetValue(animatedRigEntity, out var offset))
			return -1;

		return offset;
	}

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<BoneTransform> GetAnimationDataForRigRO(in NativeList<BoneTransform> animatedBonesBuffer, int offset, int length)
	{
		var rv = animatedBonesBuffer.GetReadOnlySpan(offset, length);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<BoneTransform> GetAnimationDataForRigRW(in NativeList<BoneTransform> animatedBonesBuffer, int offset, int length)
	{
		var rv = animatedBonesBuffer.GetSpan(offset, length);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<BoneTransform> GetAnimationDataForRigRO(in NativeList<BoneTransform> animatedBonesBuffer, in NativeParallelHashMap<Entity, int2> entityToDataOffsetMap, in RigDefinitionComponent rdc, Entity animatedRigEntity)
	{
		var offset = CalculateBufferOffset(entityToDataOffsetMap, animatedRigEntity);
		if (offset.x < 0)
			return default;
			
		return GetAnimationDataForRigRO(animatedBonesBuffer, offset.x, rdc.rigBlob.Value.bones.Length);
	}

///////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<BoneTransform> GetAnimationDataForRigRW(in NativeList<BoneTransform> animatedBonesBuffer, in NativeParallelHashMap<Entity, int2> entityToDataOffsetMap, in RigDefinitionComponent rdc, Entity animatedRigEntity)
	{
		var offset = CalculateBufferOffset(entityToDataOffsetMap, animatedRigEntity);
		if (offset.x < 0)
			return default;
			
		return GetAnimationDataForRigRW(animatedBonesBuffer, offset.x, rdc.rigBlob.Value.bones.Length);
	}

///////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AnimationTransformFlags GetAnimationTransformFlagsRO(in NativeList<int3> boneToEntityArr, in NativeList<ulong> boneTransformFlagsArr, int globalBoneIndex, int boneCount)
	{
		var boneInfo = boneToEntityArr[globalBoneIndex];
		var rv = AnimationTransformFlags.CreateFromBufferRO(boneTransformFlagsArr, boneInfo.z, boneCount);
		return rv;
	}

///////////////////////////////////////////////////////////////////////////////////////////

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static AnimationTransformFlags GetAnimationTransformFlagsRW(in NativeList<int3> boneToEntityArr, in NativeList<ulong> boneTransformFlagsArr, int globalBoneIndex, int boneCount)
	{
		var boneInfo = boneToEntityArr[globalBoneIndex];
		var rv = AnimationTransformFlags.CreateFromBufferRW(boneTransformFlagsArr, boneInfo.z, boneCount);
		return rv;
	}
}
}
