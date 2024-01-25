using Unity.Entities;
#if RUKHANKA_WITH_NETCODE
using Unity.NetCode;
#endif
using FixedStringName = Unity.Collections.FixedString512Bytes;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
[InternalBufferCapacity(4)]
[ChunkSerializable]
public struct AnimationToProcessComponent: IBufferElementData
{
	public float weight;
	public float time;
	public ExternalBlobPtr<AnimationClipBlob> animation;
	public ExternalBlobPtr<AvatarMaskBlob> avatarMask;
	public AnimationBlendingMode blendMode;
	public float layerWeight;
	public int layerIndex;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct AnimatorEntityRefComponent: IComponentData
{
	public int boneIndexInAnimationRig;
	public Entity animatorEntity;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if RUKHANKA_WITH_NETCODE
[GhostComponent(PrefabType = GhostPrefabType.Client)]
#endif
public struct AnimatedSkinnedMeshComponent: IComponentData
{
	public Entity animatedRigEntity;
	public int rootBoneIndexInRig;
	public BlobAssetReference<SkinnedMeshInfoBlob> boneInfos;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct RootMotionAnimationStateComponent: IBufferElementData, IEnableableComponent
{
	public Hash128 animationHash;
	public BoneTransform animationState;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//	Define some special bone names
public static class SpecialBones
{
	public readonly static FixedStringName unnamedRootBoneName = "RUKHANKA_UnnamedRootBone";
	public readonly static FixedStringName rootMotionDeltaBoneName = "RUKHANKA_RootDeltaMotionBone";
	public readonly static FixedStringName invalidBoneName = "RUKHANKA_INVALID_BONE";
}
}

