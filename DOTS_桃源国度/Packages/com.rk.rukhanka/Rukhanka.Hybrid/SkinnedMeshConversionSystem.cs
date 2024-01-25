using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{
[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(RigDefinitionConversionSystem))]
public partial class SkinnedMeshConversionSystem : SystemBase
{
	EntityQuery skinnedMeshRenderersQuery;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	struct SkinnedMeshBakerDataSorter: IComparer<SkinnedMeshBakerData>
	{
		public int Compare(SkinnedMeshBakerData a, SkinnedMeshBakerData b)
		{
			if (a.hash < b.hash) return -1;
			else if (a.hash > b.hash) return 1;
			return 0;
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnCreate()
	{
		base.OnCreate();

		using var ecb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<SkinnedMeshBakerData>()
		.WithOptions(EntityQueryOptions.IncludePrefab);

		skinnedMeshRenderersQuery = GetEntityQuery(ecb0);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		//	Cleanup conversion data
		using var skinnedMeshesData = skinnedMeshRenderersQuery.ToComponentDataArray<SkinnedMeshBakerData>(Allocator.Temp);
		foreach (var s in skinnedMeshesData)
			s.skinnedMeshBones.Dispose();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnUpdate()
	{
		using var skinnedMeshesData = skinnedMeshRenderersQuery.ToComponentDataArray<SkinnedMeshBakerData>(Allocator.TempJob);
		if (skinnedMeshRenderersQuery.IsEmpty) return;

	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
		if (dc.logSkinnedMeshBaking)
			Debug.Log($"=== [SkinnedMeshConversionSystem] BEGIN CONVERSION ===");
	#endif

		//	Prepare data for blob assets
		skinnedMeshesData.Sort(new SkinnedMeshBakerDataSorter());

		var startIndex = 0;
		var startHash = skinnedMeshesData[0].hash;

		using var jobsArr = new NativeList<JobHandle>(skinnedMeshesData.Length, Allocator.Temp);
		using var blobAssetsArr = new NativeArray<BlobAssetReference<SkinnedMeshInfoBlob>>(skinnedMeshesData.Length, Allocator.TempJob);
		var blobUniqueIndices = new NativeList<int>(Allocator.Temp);

		for (int i = 1; i <= skinnedMeshesData.Length; ++i)
		{
			SkinnedMeshBakerData rd = i < skinnedMeshesData.Length ? skinnedMeshesData[i] : default;
			if (rd.hash != startHash)
			{
				var numDuplicates = i - startIndex;
				var blobAssetsSlice = new NativeSlice<BlobAssetReference<SkinnedMeshInfoBlob>>(blobAssetsArr, startIndex, numDuplicates);
				var refSkinnedMesh = skinnedMeshesData[startIndex];
				var j = new CreateBlobAssetsJob()
				{
					data = refSkinnedMesh,
					outBlobAssets = blobAssetsSlice,
				};

				var jh = j.Schedule();
				jobsArr.Add(jh);
				blobUniqueIndices.Add(startIndex);

				startHash = rd.hash;
				startIndex = i;
			#if RUKHANKA_DEBUG_INFO
				if (dc.logSkinnedMeshBaking)
					Debug.Log($"Creating blob asset for skinned mesh '{refSkinnedMesh.skeletonName}'. Entities count: {numDuplicates}");
			#endif
			}
		}

		var combinedJH = JobHandle.CombineDependencies(jobsArr.AsArray());
		using var ecb = new EntityCommandBuffer(Allocator.TempJob);

		var animatedEntityRefLookup = GetComponentLookup<AnimatorEntityRefComponent>(true);

		var createComponentDatasJob = new CreateComponentDatasJob()
		{
			ecb = ecb.AsParallelWriter(),
			bakerData = skinnedMeshesData,
			blobAssets = blobAssetsArr,
			animEntityRefLookup = animatedEntityRefLookup,
		#if RUKHANKA_DEBUG_INFO
			enableLog = dc.logSkinnedMeshBaking
		#endif
		};

		createComponentDatasJob.ScheduleBatch(skinnedMeshesData.Length, 32, combinedJH).Complete();
		
		//	Register blob assets in store to prevent memory leaks
		RegisterBlobAssetsInAssetStore(blobAssetsArr, blobUniqueIndices.AsArray());

		ecb.Playback(EntityManager);

	#if RUKHANKA_DEBUG_INFO
		if (dc.logSkinnedMeshBaking)
		{
			Debug.Log($"Total converted skinned meshes: {skinnedMeshesData.Length}");
			Debug.Log($"=== [SkinnedMeshConversionSystem] END CONVERSION ===");
		}
	#endif
	}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void RegisterBlobAssetsInAssetStore(NativeArray<BlobAssetReference<SkinnedMeshInfoBlob>> blobAssetsArr, NativeArray<int> blobUniqueIndices)
	{
		var bakingSystem = World.GetExistingSystemManaged<BakingSystem>();
		var blobAssetStore = bakingSystem.BlobAssetStore;
		for (var i = 0; i < blobUniqueIndices.Length; ++i)
		{
			var idx = blobUniqueIndices[i];
			var ba = blobAssetsArr[idx];
			blobAssetStore.TryAdd(ref ba);
		}
	}
}
}
