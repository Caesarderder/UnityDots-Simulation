using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Rukhanka.Editor;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[TemporaryBakingType]
public struct SkinnedMeshBakerData: IComponentData
{
	public RTP.SkinnedMeshBoneData skinnedMeshBones;
	public Entity targetEntity;
	public Entity rootBoneEntity;
	public Entity animatedRigEntity;
	public int hash;
#if RUKHANKA_DEBUG_INFO
	public FixedStringName skeletonName;
#endif
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class SkinnedMeshBaker: Baker<SkinnedMeshRenderer>
{
	public override void Bake(SkinnedMeshRenderer a)
	{
		var smbd = CreateSkinnedMeshBoneData(a);

		//	Create additional "bake-only" entity that will be removed from live world
		var be = CreateAdditionalEntity(TransformUsageFlags.None, true);
		var bd = new SkinnedMeshBakerData
		{
			skinnedMeshBones = smbd,
			targetEntity = GetEntity(TransformUsageFlags.Dynamic),
			animatedRigEntity = GetEntity(a.gameObject.GetComponentInParent<RigDefinitionAuthoring>(true), TransformUsageFlags.Dynamic),
			rootBoneEntity = GetEntity(a.rootBone, TransformUsageFlags.Dynamic),
			hash = a.sharedMesh.GetHashCode(),
		#if RUKHANKA_DEBUG_INFO
			skeletonName = a.name
		#endif
		};

		AddComponent(be, bd);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	RTP.SkinnedMeshBoneData CreateSkinnedMeshBoneData(SkinnedMeshRenderer r)
	{ 
		var bnd = new RTP.SkinnedMeshBoneData();
		bnd.bones = new UnsafeList<RTP.SkinnedMeshBoneDefinition>(r.bones.Length, Allocator.Persistent);
		bnd.bones.Length = r.bones.Length;
		bnd.skeletonName = r.name;
		bnd.parentBoneName = r.rootBone != null ? r.rootBone.name : "";
		for (int j = 0; j < r.bones.Length; ++j)
		{
			var b = r.bones[j];
			var oneBoneInfo = new RTP.SkinnedMeshBoneDefinition();
#if RUKHANKA_DEBUG_INFO
			oneBoneInfo.name = b.name;
#endif
			var bn = new FixedStringName(b.name);
			oneBoneInfo.hash = bn.CalculateHash128();
			oneBoneInfo.bindPose = r.sharedMesh.bindposes[j];
			bnd.bones[j] = oneBoneInfo;
		}
		return bnd;
	}
}
}
