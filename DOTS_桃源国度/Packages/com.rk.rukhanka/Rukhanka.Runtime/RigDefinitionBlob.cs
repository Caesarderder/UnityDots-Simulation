
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

public struct RigBoneInfo
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif

	public Hash128 hash;
	public int parentBoneIndex;
	public BoneTransform refPose;
	public AvatarMaskBodyPart humanBodyPart;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct HumanRotationData
{
	public float3 minMuscleAngles, maxMuscleAngles;
	public quaternion preRot, postRot;
	public float3 sign;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct HumanData
{
	public BlobArray<HumanRotationData> humanRotData;
	public BlobArray<int> humanBoneToSkeletonBoneIndices;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct RigDefinitionBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public Hash128 hash;
	public BlobArray<RigBoneInfo> bones;
	public BlobPtr<HumanData> humanData;
	public int rootBoneIndex;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct RigDefinitionComponent: IComponentData, IEnableableComponent
{
	public BlobAssetReference<RigDefinitionBlob> rigBlob;
	public bool applyRootMotion;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct BoneRemapTableBlob
{
	public BlobArray<int> rigBoneToSkinnedMeshBoneRemapIndices;
}

}
