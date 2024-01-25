using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Hash128 = Unity.Entities.Hash128;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
public partial class SkinnedMeshConversionSystem
{
	[BurstCompile]
	public struct CreateBlobAssetsJob: IJob
	{
		public SkinnedMeshBakerData data;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<BlobAssetReference<SkinnedMeshInfoBlob>> outBlobAssets;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute()
		{
			var bb = new BlobBuilder(Allocator.Temp);
			ref var blobAsset = ref bb.ConstructRoot<SkinnedMeshInfoBlob>();

		#if RUKHANKA_DEBUG_INFO
			bb.AllocateString(ref blobAsset.skeletonName, ref data.skeletonName);
		#endif

			var boneInfoArr = bb.Allocate(ref blobAsset.bones, data.skinnedMeshBones.bones.Length);
			for (int i = 0; i < data.skinnedMeshBones.bones.Length; ++i)
			{
				var src = data.skinnedMeshBones.bones[i];
				ref var smbi = ref boneInfoArr[i];
				smbi.hash = src.hash;
				smbi.bindPose = src.bindPose;

			#if RUKHANKA_DEBUG_INFO
				bb.AllocateString(ref smbi.name, ref src.name);
			#endif
			}
			var pn = new FixedStringName(data.skinnedMeshBones.parentBoneName);
			var ph = pn.CalculateHash128();
			blobAsset.rootBoneNameHash = ph;
			blobAsset.hash = new Hash128((uint)data.hash, ph.Value.w, ph.Value.z, ph.Value.y);

			var rv = bb.CreateBlobAssetReference<SkinnedMeshInfoBlob>(Allocator.Persistent);
			for (int i = 0; i < outBlobAssets.Length; ++i)
			{
				outBlobAssets[i] = rv;
			}
		}
	}

//=================================================================================================================//

	[BurstCompile]
	struct CreateComponentDatasJob: IJobParallelForBatch
	{
		[ReadOnly]
		public NativeArray<SkinnedMeshBakerData> bakerData;
		[ReadOnly]
		public NativeArray<BlobAssetReference<SkinnedMeshInfoBlob>> blobAssets;
		[ReadOnly]
		public ComponentLookup<AnimatorEntityRefComponent> animEntityRefLookup;

		public EntityCommandBuffer.ParallelWriter ecb;

	#if RUKHANKA_DEBUG_INFO
		public bool enableLog;
	#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void Execute(int startIndex, int count)
		{
			for (int i = startIndex; i < startIndex + count; ++i)
			{
				var smb = bakerData[i];

				var e = smb.targetEntity;
				var bnc = new AnimatedSkinnedMeshComponent();
				bnc.boneInfos = blobAssets[i];
				bnc.animatedRigEntity = smb.animatedRigEntity;
				bnc.rootBoneIndexInRig = -1;

				if (animEntityRefLookup.HasComponent(smb.rootBoneEntity))
				{
					var are = animEntityRefLookup[smb.rootBoneEntity];
					bnc.rootBoneIndexInRig = are.boneIndexInAnimationRig;
				}
				
				ecb.AddComponent(startIndex, e, bnc);

			#if RUKHANKA_DEBUG_INFO
				if (enableLog)
					Debug.Log($"Adding 'AnimatedSkinnedMeshComponent' to entity '{e.Index}:{e.Version}'");
			#endif
			}
		}
	}
}
}
