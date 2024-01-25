using System;
using Unity.Collections.LowLevel.Unsafe;
using Hash128 = Unity.Entities.Hash128;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
//	RTP - Ready to process
namespace RTP
{
public struct RigBoneInfo: IEquatable<Hash128>
{
	public struct HumanRotationData
	{
		public float3 minAngle, maxAngle;
		public quaternion preRot, postRot;
		public float3 sign;
		public int humanRigIndex;

		public static HumanRotationData Identity()
		{
			var rv = new HumanRotationData()
			{
				preRot = quaternion.identity,
				postRot = quaternion.identity,
				humanRigIndex = -1,
				sign = 1
			};
			return rv;
		}
	};

	public FixedStringName name;
	public Hash128 hash;
	public int parentBoneIndex;
	public BoneTransform refPose;
	public Entity boneObjectEntity;
	public HumanRotationData humanRotationData;

	public bool Equals(Hash128 o) => o == hash;
}

////////////////////////////////////////////////////////////////////////////////////////

public struct RigDefinition: IDisposable
{
	public FixedStringName name;
	public UnsafeList<RigBoneInfo> rigBones;
	public bool isHuman;
	public int rootBoneIndex;

	public void Dispose() => rigBones.Dispose();

	unsafe public override int GetHashCode()
	{
		var hh = new xxHash3.StreamingState();
		hh.Update(name.GetUnsafePtr(), name.Length);
		foreach (var b in rigBones)
		{
			hh.Update(b.hash.Value);
		}

		var rv = math.hash(hh.DigestHash128());
		return (int)rv;
	}
}

} // RTP
}


