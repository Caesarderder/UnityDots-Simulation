using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using System.Collections.Generic;
using Unity.Entities.Hybrid.Baking;
using UnityEngine;
using Hash128 = UnityEngine.Hash128;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(BakingOnlyEntityAuthoringBakingSystem))]
public partial class RigDefinitionConversionSystem : SystemBase
{
	EntityQuery rigDefinitionQuery;
	ComponentLookup<BakingOnlyEntity> bakingOnlyEntityLookup;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	struct RigDefinitionDataSorter: IComparer<RigDefinitionBakerComponent>
	{
		public int Compare(RigDefinitionBakerComponent a, RigDefinitionBakerComponent b)
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
		.WithAll<RigDefinitionBakerComponent>()
		.WithOptions(EntityQueryOptions.IncludePrefab);

		rigDefinitionQuery = GetEntityQuery(ecb0);

		bakingOnlyEntityLookup = GetComponentLookup<BakingOnlyEntity>(true);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		using var allRigs = rigDefinitionQuery.ToComponentDataArray<RigDefinitionBakerComponent>(Allocator.Temp);
		foreach (var r in allRigs)
			r.rigDefData.Dispose();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnUpdate()
	{
		using var allRigs = rigDefinitionQuery.ToComponentDataArray<RigDefinitionBakerComponent>(Allocator.TempJob);
		if (allRigs.Length == 0) return;

	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
		if (dc.logRigDefinitionBaking)
			Debug.Log($"=== [RigDefinition] BEGIN CONVERSION ===");
	#endif

		allRigs.Sort(new RigDefinitionDataSorter());

		//	Create blob assets
		using var blobAssetsArr = new NativeArray<BlobAssetReference<RigDefinitionBlob>>(allRigs.Length, Allocator.TempJob);

		var startIndex = 0;
		var startHash = allRigs[0].hash;

		using var jobsArr = new NativeList<JobHandle>(allRigs.Length, Allocator.Temp);
		var blobUniqueIndices = new NativeList<int>(Allocator.Temp);

		for (int i = 1; i <= allRigs.Length; ++i)
		{
			RigDefinitionBakerComponent rd = i < allRigs.Length ? allRigs[i] : default;
			if (rd.hash != startHash)
			{
				var numDuplicates = i - startIndex;
				var blobAssetsSlice = new NativeSlice<BlobAssetReference<RigDefinitionBlob>>(blobAssetsArr, startIndex, numDuplicates);
				var refRig = allRigs[startIndex];
				var j = new CreateBlobAssetsJob()
				{
					inData = refRig.rigDefData,
					outBlobAssets = blobAssetsSlice,
					rigHash = new Hash128((uint)rd.hash, 0, 0, 0)
				};

				var jh = j.Schedule();
				jobsArr.Add(jh);
				blobUniqueIndices.Add(startIndex);

				startHash = rd.hash;
				startIndex = i;

			#if RUKHANKA_DEBUG_INFO
				if (dc.logSkinnedMeshBaking)
					Debug.Log($"Creating blob asset for skinned mesh '{refRig.name}'. Entities count: {numDuplicates}");
			#endif
			}
		}

		var combinedJH = JobHandle.CombineDependencies(jobsArr.AsArray());
		using var ecb = new EntityCommandBuffer(Allocator.TempJob);
		bakingOnlyEntityLookup.Update(this);

		var createComponentDatasJob = new CreateComponentDatasJob()
		{
			ecb = ecb.AsParallelWriter(),
			bakerData = allRigs,
			blobAssets = blobAssetsArr,
			bakingOnlyLookup = bakingOnlyEntityLookup
		};

		createComponentDatasJob.ScheduleBatch(allRigs.Length, 32, combinedJH).Complete();
		
		//	Register blob assets in store to prevent memory leaks
		RegisterBlobAssetsInAssetStore(blobAssetsArr, blobUniqueIndices.AsArray());

		ecb.Playback(EntityManager);

	#if RUKHANKA_DEBUG_INFO
		if (dc.logRigDefinitionBaking)
		{
			Debug.Log($"Total converted rigs: {allRigs.Length}");
			Debug.Log($"=== [RigDefinition] END CONVERSION ===");
		}
	#endif
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void RegisterBlobAssetsInAssetStore(NativeArray<BlobAssetReference<RigDefinitionBlob>> blobAssetsArr, NativeArray<int> blobUniqueIndices)
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
