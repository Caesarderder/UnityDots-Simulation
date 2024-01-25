using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
public partial class RigDefinitionConversionSystem
{
	public static readonly AvatarMaskBodyPart[] humanPartToAvatarMaskPartRemapTable = 
	{
		//	Hips = 0,
		AvatarMaskBodyPart.Root,
		//	LeftUpperLeg = 1,
		AvatarMaskBodyPart.LeftLeg,
		//	RightUpperLeg = 2,
		AvatarMaskBodyPart.RightLeg,
		//	LeftLowerLeg = 3,
		AvatarMaskBodyPart.LeftLeg,
		//	RightLowerLeg = 4,
		AvatarMaskBodyPart.RightLeg,
		//	LeftFoot = 5,
		AvatarMaskBodyPart.LeftLeg,
		//	RightFoot = 6,
		AvatarMaskBodyPart.RightLeg,
		//	Spine = 7,
		AvatarMaskBodyPart.Body,
		//	Chest = 8,
		AvatarMaskBodyPart.Body,
		//	Neck = 9,
		AvatarMaskBodyPart.Head,
		//	Head = 10,
		AvatarMaskBodyPart.Head,
		//	LeftShoulder = 11,
		AvatarMaskBodyPart.LeftArm,
		//	RightShoulder = 12,
		AvatarMaskBodyPart.RightArm,
		//	LeftUpperArm = 13,
		AvatarMaskBodyPart.LeftArm,
		//	RightUpperArm = 14,
		AvatarMaskBodyPart.RightArm,
		//	LeftLowerArm = 15,
		AvatarMaskBodyPart.LeftArm,
		//	RightLowerArm = 16,
		AvatarMaskBodyPart.RightArm,
		//	LeftHand = 17,
		AvatarMaskBodyPart.LeftArm,
		//	RightHand = 18,
		AvatarMaskBodyPart.RightArm,
		//	LeftToes = 19,
		AvatarMaskBodyPart.LeftLeg,
		//	RightToes = 20,
		AvatarMaskBodyPart.RightLeg,
		//	LeftEye = 21,
		AvatarMaskBodyPart.Head,
		//	RightEye = 22,
		AvatarMaskBodyPart.Head,
		//	Jaw = 23,
		AvatarMaskBodyPart.Head,
		//	LeftThumbProximal = 24,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftThumbIntermediate = 25,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftThumbDistal = 26,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftIndexProximal = 27,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftIndexIntermediate = 28,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftIndexDistal = 29,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftMiddleProximal = 30,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftMiddleIntermediate = 31,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftMiddleDistal = 32,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftRingProximal = 33,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftRingIntermediate = 34,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftRingDistal = 35,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftLittleProximal = 36,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftLittleIntermediate = 37,
		AvatarMaskBodyPart.LeftFingers,
		//	LeftLittleDistal = 38,
		AvatarMaskBodyPart.LeftFingers,
		//	RightThumbProximal = 39,
		AvatarMaskBodyPart.RightFingers,
		//	RightThumbIntermediate = 40,
		AvatarMaskBodyPart.RightFingers,
		//	RightThumbDistal = 41,
		AvatarMaskBodyPart.RightFingers,
		//	RightIndexProximal = 42,
		AvatarMaskBodyPart.RightFingers,
		//	RightIndexIntermediate = 43,
		AvatarMaskBodyPart.RightFingers,
		//	RightIndexDistal = 44,
		AvatarMaskBodyPart.RightFingers,
		//	RightMiddleProximal = 45,
		AvatarMaskBodyPart.RightFingers,
		//	RightMiddleIntermediate = 46,
		AvatarMaskBodyPart.RightFingers,
		//	RightMiddleDistal = 47,
		AvatarMaskBodyPart.RightFingers,
		//	RightRingProximal = 48,
		AvatarMaskBodyPart.RightFingers,
		//	RightRingIntermediate = 49,
		AvatarMaskBodyPart.RightFingers,
		//	RightRingDistal = 50,
		AvatarMaskBodyPart.RightFingers,
		//	RightLittleProximal = 51,
		AvatarMaskBodyPart.RightFingers,
		//	RightLittleIntermediate = 52,
		AvatarMaskBodyPart.RightFingers,
		//	RightLittleDistal = 53,
		AvatarMaskBodyPart.RightFingers,
		//	UpperChest = 54,
		AvatarMaskBodyPart.Body,
	};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	struct CreateBlobAssetsJob: IJob
	{
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<BlobAssetReference<RigDefinitionBlob>> outBlobAssets;
		public RTP.RigDefinition inData;
		public Hash128 rigHash;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute()
		{
			var data = inData;
			var bb = new BlobBuilder(Allocator.Temp);
			ref var c = ref bb.ConstructRoot<RigDefinitionBlob>();

#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref c.name, ref data.name);
#endif
			var bonesArr = bb.Allocate(ref c.bones, data.rigBones.Length);
			for (int l = 0; l < bonesArr.Length; ++l)
			{
				var db = data.rigBones[l];
				ref var rbi = ref bonesArr[l];
				rbi.hash = db.hash;
				rbi.humanBodyPart = (AvatarMaskBodyPart)(-1);
				rbi.parentBoneIndex = db.parentBoneIndex;
				rbi.refPose = db.refPose;

#if RUKHANKA_DEBUG_INFO
				if (db.name.Length > 0)
					bb.AllocateString(ref rbi.name, ref db.name);
#endif
			}

			if (data.isHuman)
			{
				ref var humanData = ref bb.Allocate(ref c.humanData);
				var humanToRigArr = bb.Allocate(ref humanData.humanBoneToSkeletonBoneIndices, (int)HumanBodyBones.LastBone);
				var humanRotArr = bb.Allocate(ref humanData.humanRotData, data.rigBones.Length);
				
				for (int j = 0; j < humanToRigArr.Length; ++j)
					humanToRigArr[j] = -1;

				for (int l = 0; l < humanRotArr.Length; ++l)
				{
					var db = data.rigBones[l].humanRotationData;
					ref var hrd = ref humanRotArr[l];
					hrd.preRot = db.preRot;
					hrd.postRot = math.inverse(db.postRot);
					hrd.sign = db.sign;
					hrd.minMuscleAngles = db.minAngle;
					hrd.maxMuscleAngles = db.maxAngle;

					if (db.humanRigIndex >= 0)
					{
						humanToRigArr[db.humanRigIndex] = l;
						//	Make muscle neutral ref pose
						ref var rbi = ref bonesArr[l];
						rbi.refPose.rot = math.mul(hrd.preRot, hrd.postRot);
					}
				}

				SetHumanBodyBodyPartForBones(bonesArr, data);
			}

			c.hash = rigHash;
			c.rootBoneIndex = data.rootBoneIndex;

			var rv = bb.CreateBlobAssetReference<RigDefinitionBlob>(Allocator.Persistent);

			for (int i = 0; i < outBlobAssets.Length; ++i)
			{
				outBlobAssets[i] = rv;
			}
		}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		void SetHumanBodyBodyPartForBones(BlobBuilderArray<RigBoneInfo> rigBones, in RTP.RigDefinition rd)
		{
			var rigHumanAvatarMaskBodyParts = new NativeArray<AvatarMaskBodyPart>(rd.rigBones.Length, Allocator.Temp);

			//	Loop over bones and set human bones avatar mask directly
			for (int i = 0; i < rd.rigBones.Length; ++i)
			{
				var b = rd.rigBones[i];
				var humanRigIndex = b.humanRotationData.humanRigIndex;
				if (humanRigIndex >= 0 && humanRigIndex < humanPartToAvatarMaskPartRemapTable.Length)
				{
					rigHumanAvatarMaskBodyParts[i] = humanPartToAvatarMaskPartRemapTable[humanRigIndex];
				}
				else
				{
					rigHumanAvatarMaskBodyParts[i] = (AvatarMaskBodyPart)(-1);
				}
			}

			//	Root bone is special case
			rigHumanAvatarMaskBodyParts[0] = AvatarMaskBodyPart.Root;

			//	For other bones search for parent with body part is set and set it to the same value
			for (int i = 0; i < rigBones.Length; ++i)
			{
				if (rigHumanAvatarMaskBodyParts[i] >= 0)
					continue;

				var l = i;
				var rl = rd.rigBones[l];
				while (rigHumanAvatarMaskBodyParts[l] < 0 && rl.parentBoneIndex >= 0)
				{
					l = rl.parentBoneIndex;
					rl = rd.rigBones[l];
				}

				if (l != i)
					rigHumanAvatarMaskBodyParts[i] = rigHumanAvatarMaskBodyParts[l];
			}

			for (int i = 0; i < rigBones.Length; ++i)
				rigBones[i].humanBodyPart = rigHumanAvatarMaskBodyParts[i];
		}
	}

//=================================================================================================================//

	[BurstCompile]
	struct CreateComponentDatasJob: IJobParallelForBatch
	{
		[ReadOnly]
		public NativeArray<RigDefinitionBakerComponent> bakerData;
		[ReadOnly]
		public NativeArray<BlobAssetReference<RigDefinitionBlob>> blobAssets;
		[ReadOnly]
		public ComponentLookup<BakingOnlyEntity> bakingOnlyLookup;

		public EntityCommandBuffer.ParallelWriter ecb;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute(int startIndex, int count)
		{
			for (int i = startIndex; i < startIndex + count; ++i)
			{
				var rigBlob = blobAssets[i];
				var rd = bakerData[i];

				var rdc = new RigDefinitionComponent()
				{
					rigBlob = rigBlob,
					applyRootMotion = rd.applyRootMotion
				};

				ecb.AddComponent(startIndex, rd.targetEntity, rdc);

				for (int l = 0; l < rd.rigDefData.rigBones.Length; ++l)
				{
					var rb = rd.rigDefData.rigBones[l];

					var boneEntity = rb.boneObjectEntity;
					if (boneEntity != Entity.Null && !bakingOnlyLookup.HasComponent(boneEntity))
					{
						var animatorEntityRefComponent = new AnimatorEntityRefComponent()
						{
							animatorEntity = rd.targetEntity,
							boneIndexInAnimationRig = l
						};
						ecb.AddComponent(startIndex, boneEntity, animatorEntityRefComponent);
					}
				}

				ecb.AddBuffer<RootMotionAnimationStateComponent>(startIndex, rd.targetEntity);
			}
		}
	}
} 
}
