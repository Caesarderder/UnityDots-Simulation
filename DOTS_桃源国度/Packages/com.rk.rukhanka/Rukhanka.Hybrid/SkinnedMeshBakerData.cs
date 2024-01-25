using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using FixedStringName = Unity.Collections.FixedString512Bytes;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
//	RTP - Ready to process
namespace RTP
{
public struct SkinnedMeshBoneDefinition
{
	public FixedStringName name;
	public Hash128 hash;
	public float4x4 bindPose;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public struct SkinnedMeshBoneData: IDisposable
{
	public FixedStringName skeletonName;
	public FixedStringName parentBoneName;
	public UnsafeList<SkinnedMeshBoneDefinition> bones;

	public void Dispose() => bones.Dispose();
}
} // RTP
}


