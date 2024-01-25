using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Rukhanka.Editor;
using System.Reflection;
using System.Collections.Generic;
using System;
using Unity.Mathematics;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[TemporaryBakingType]
public struct RigDefinitionBakerComponent: IComponentData
{
	public RTP.RigDefinition rigDefData;
	public Entity targetEntity;
	public bool applyRootMotion;
	public int hash;
#if RUKHANKA_DEBUG_INFO
	public FixedStringName name;
#endif
}

[TemporaryBakingType]
public struct BoneEntitiesToRemove : IBufferElementData
{
	public Entity boneEntity;
}

internal class InternalSkeletonBone
{
	public string name;
	public string parentName;
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class RigDefinitionBaker: Baker<RigDefinitionAuthoring>
{
	static FieldInfo parentBoneNameField;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	static RigDefinitionBaker()
	{
		parentBoneNameField = typeof(SkeletonBone).GetField("parentName", BindingFlags.NonPublic | BindingFlags.Instance);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public override void Bake(RigDefinitionAuthoring a)
	{
		var animator = GetComponent<Animator>();
		var e = GetEntity(TransformUsageFlags.Dynamic);
		
		var processedRig = CreateRigDefinitionFromRigAuthoring(e, a, animator);
		var acbd = new RigDefinitionBakerComponent
		{
			rigDefData = processedRig,
			targetEntity = GetEntity(TransformUsageFlags.Dynamic),
			hash = processedRig.GetHashCode(),
			applyRootMotion = animator.applyRootMotion,
		#if RUKHANKA_DEBUG_INFO
			name = a.name
		#endif
		};

		DependsOn(a);
		AddComponent(e, acbd);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	InternalSkeletonBone CreateSkeletonBoneFromTransform(Transform t, string parentName)
	{
		var bone = new InternalSkeletonBone();
		bone.name = t.name;
		bone.position = t.localPosition;
		bone.rotation = t.localRotation;
		bone.scale = t.localScale;
		bone.parentName = parentName;
		return bone;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void TransformHierarchyWalk(Transform parent, List<InternalSkeletonBone> sb)
	{
		for (int i = 0; i < parent.childCount; ++i)
		{
			var c = parent.GetChild(i);
			var ct = c.transform;
			var bone = CreateSkeletonBoneFromTransform(ct, parent.name);
			sb.Add(bone);

			TransformHierarchyWalk(ct, sb);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	List<InternalSkeletonBone> CreateAvatarFromObjectHierarchy(GameObject root)
	{
		//	Manually fill all bone transforms
		var sb = new List<InternalSkeletonBone>();
		var rootBone = CreateSkeletonBoneFromTransform(root.transform, "");
		sb.Add(rootBone);

		TransformHierarchyWalk(root.transform, sb);
		return sb;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	int GetRigRootBoneIndex(Animator anm, List<InternalSkeletonBone> rigBones)
	{
		var a = anm.avatar;
		if (a == null)
			return 0;
		
		var rootBoneName = a.GetRootMotionNodeName();
		if (anm.avatar.isHuman)
		{
			var hd = anm.avatar.humanDescription;
			var humanBoneIndexInDesc = Array.FindIndex(hd.human, x => x.humanName == "Hips");
			rootBoneName = hd.human[humanBoneIndexInDesc].boneName;
		}
		var rv = rigBones.FindIndex(x => x.name == rootBoneName);
		return math.max(rv, 0);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	List<InternalSkeletonBone> CreateInternalRigRepresentation(Avatar avatar, RigDefinitionAuthoring rd)
	{
		if (avatar == null)
		{
			return CreateAvatarFromObjectHierarchy(rd.gameObject);
		}
		
		var skeleton = avatar.humanDescription.skeleton;
		var rv = new List<InternalSkeletonBone>();
		for (var i = 0; i < skeleton.Length; ++i)
		{
			var sb = skeleton[i];
			var isb = new InternalSkeletonBone()
			{
				name = sb.name,
				position = sb.position,
				rotation = sb.rotation,
				scale = sb.scale,
				parentName = (string)parentBoneNameField.GetValue(sb)
			};
			rv.Add(isb);
		}

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	RTP.RigDefinition CreateRigDefinitionFromRigAuthoring(Entity rigEntity, RigDefinitionAuthoring rigDef, Animator animator)
	{
		var avatar = animator.avatar;

		var rv = new RTP.RigDefinition();
		rv.rigBones = new UnsafeList<RTP.RigBoneInfo>(60, Allocator.Persistent);

		rv.name = rigDef.gameObject.name;
		rv.isHuman = avatar != null && avatar.isHuman;

		var skeletonBones = CreateInternalRigRepresentation(avatar, rigDef);
		if (skeletonBones.Count == 0)
		{
			Debug.LogError($"Unity avatar '{avatar.name}' setup is incorrect. Follow <a href=\"https://docs.rukhanka.com/getting_started#rig-definition\">documentation</a> about avatar setup process please.");
			return rv;
		}

		for (int i = 0; i < skeletonBones.Count; ++i)
		{
			var ab = CreateRigBoneInfo(rigDef, skeletonBones, avatar, i);
			rv.rigBones.Add(ab);
		}
		
		rv.rootBoneIndex = GetRigRootBoneIndex(animator, skeletonBones);
		
		ProcessBoneStrippingMask(rigEntity, rigDef, rv.rigBones);

		return rv;
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	RTP.RigBoneInfo.HumanRotationData GetHumanoidBoneRotationData(Avatar a, string boneName)
	{
		if (a == null || !a.isHuman)
			return RTP.RigBoneInfo.HumanRotationData.Identity();

		var hd = a.humanDescription;
		var humanBoneInSkeletonIndex = Array.FindIndex(hd.human, x => x.boneName == boneName);
		if (humanBoneInSkeletonIndex < 0)
			return RTP.RigBoneInfo.HumanRotationData.Identity();
			
		var humanBones = HumanTrait.BoneName;
		var humanBoneDef = hd.human[humanBoneInSkeletonIndex];
		var humanBoneId = Array.FindIndex(humanBones, x => x == humanBoneDef.humanName);
		Debug.Assert(humanBoneId >= 0);

		var rv = RTP.RigBoneInfo.HumanRotationData.Identity();
		rv.preRot = a.GetPreRotation(humanBoneId);
		rv.postRot = a.GetPostRotation(humanBoneId);
		rv.sign = a.GetLimitSign(humanBoneId);
		rv.humanRigIndex = humanBoneId;

		var minA = humanBoneDef.limit.min;
		var maxA = humanBoneDef.limit.max;
		if (humanBoneDef.limit.useDefaultValues)
		{
			minA.x = HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(humanBoneId, 0));
			minA.y = HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(humanBoneId, 1));
			minA.z = HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(humanBoneId, 2));

			maxA.x = HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(humanBoneId, 0));
			maxA.y = HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(humanBoneId, 1));
			maxA.z = HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(humanBoneId, 2));
		}
		rv.minAngle = math.radians(minA);
		rv.maxAngle = math.radians(maxA);

		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	Entity GetEntityForBone(Transform t, TransformUsageFlags boneFlags)
	{
		if (t == null || t.GetComponent<SkinnedMeshRenderer>() != null)
			return Entity.Null;

		return GetEntity(t, boneFlags);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	RTP.RigBoneInfo CreateRigBoneInfo(RigDefinitionAuthoring rda, List<InternalSkeletonBone> skeletonBones, Avatar avatar, int boneIndex)
	{
		var boneIsObjectRoot = boneIndex == 0;
		var skeletonBone = skeletonBones[boneIndex];
		var t = boneIsObjectRoot ? rda.transform : Toolbox.FindChildRecursively(rda.transform, skeletonBone.name);

		var name = skeletonBone.name;
		// Special handling of hierarchy root
		if (boneIsObjectRoot)
		{
			name = SpecialBones.unnamedRootBoneName.ToString();
		}

		var parentBoneIndex = skeletonBones.FindIndex(x => x.name == skeletonBone.parentName);

		var pose = new BoneTransform()
		{
			pos = skeletonBone.position,
			rot = skeletonBone.rotation,
			scale = skeletonBone.scale
		};

		//	Add humanoid avatar info
		var humanRotData = GetHumanoidBoneRotationData(avatar, name);

		var boneName = new FixedStringName(name);
		var boneHash = boneName.CalculateHash128();
		var boneTransformFlags = TransformUsageFlags.Dynamic;
		if (rda.boneStrippingMask != null && !boneIsObjectRoot)
			boneTransformFlags |= TransformUsageFlags.WorldSpace;
		
		var ab = new RTP.RigBoneInfo()
		{
			name = boneName,
			hash = boneHash,
			parentBoneIndex = parentBoneIndex,
			refPose = pose,
			boneObjectEntity = GetEntityForBone(t, boneTransformFlags),
			humanRotationData = humanRotData,
		};
		return ab;
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ProcessBoneStrippingMask(Entity rigEntity, RigDefinitionAuthoring rda, UnsafeList<RTP.RigBoneInfo> rigBones)
	{
		if (rda.boneStrippingMask == null) return;

		var m = rda.boneStrippingMask;
		var bonesToRemove = AddBuffer<BoneEntitiesToRemove>(rigEntity);
        
		for (int i = 0; i < m.transformCount; ++i)
		{
			var isActive = m.GetTransformActive(i);
			if (isActive) continue;
			
			var path = m.GetTransformPath(i);
			var boneIndex = 0;
			for (; boneIndex < rigBones.Length && !path.EndsWith(rigBones[boneIndex].name.ToString()); ++boneIndex) { }

			if (boneIndex < rigBones.Length)
			{
				bonesToRemove.Add(new BoneEntitiesToRemove() { boneEntity = rigBones[boneIndex].boneObjectEntity});
			}
		}
	}
}
}
