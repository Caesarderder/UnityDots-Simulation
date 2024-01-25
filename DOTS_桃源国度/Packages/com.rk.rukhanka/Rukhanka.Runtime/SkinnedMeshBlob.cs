using Hash128 = Unity.Entities.Hash128;
using Unity.Mathematics;
using Unity.Entities;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
public struct SkinnedMeshBoneInfo
{
#if RUKHANKA_DEBUG_INFO
	public BlobString name;
#endif
	public Hash128 hash;
	public float4x4 bindPose;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct SkinnedMeshInfoBlob
{
#if RUKHANKA_DEBUG_INFO
	public BlobString skeletonName;
#endif
	public Hash128 hash;
	public Hash128 rootBoneNameHash;
	public BlobArray<SkinnedMeshBoneInfo> bones;
}

}
