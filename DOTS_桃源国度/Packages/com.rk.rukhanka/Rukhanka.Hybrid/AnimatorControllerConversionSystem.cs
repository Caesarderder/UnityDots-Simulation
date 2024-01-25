#if UNITY_EDITOR

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
public partial class AnimatorControllerConversionSystem: SystemBase
{
	EntityQuery animatorsQuery;
	ComponentLookup<RigDefinitionBakerComponent> rigDefComponentLookup;

	public struct AnimatorBlobAssets
	{
		public BlobAssetReference<ControllerBlob> controllerBlob;
		public BlobAssetReference<ParameterPerfectHashTableBlob> parametersPerfectHashTableBlob;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	struct AnimatorControllerBakerDataSorter: IComparer<AnimatorControllerBakerData>
	{
		public int Compare(AnimatorControllerBakerData a, AnimatorControllerBakerData b)
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
		
		using var eqb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerBakerData>()
		.WithOptions(EntityQueryOptions.IncludePrefab);

		rigDefComponentLookup = GetComponentLookup<RigDefinitionBakerComponent>(true);

		animatorsQuery = GetEntityQuery(eqb0);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		using var controllersData = animatorsQuery.ToComponentDataArray<AnimatorControllerBakerData>(Allocator.Temp);
		foreach (var c in controllersData)
			c.controllerData.Dispose();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnUpdate()
	{
		using var controllersData = animatorsQuery.ToComponentDataArray<AnimatorControllerBakerData>(Allocator.TempJob);
		using var entities = animatorsQuery.ToEntityArray(Allocator.TempJob);
		if (controllersData.Length == 0) return;

#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
		if (dc.logAnimatorBaking)
			Debug.Log($"=== [AnimatorControllerConversionSystem] BEGIN CONVERSION ===");
#endif

		//	Create blob assets
		using var blobAssetsArr = new NativeArray<AnimatorBlobAssets>(controllersData.Length, Allocator.TempJob);
		controllersData.Sort(new AnimatorControllerBakerDataSorter());

		var startIndex = 0;
		var startHash = controllersData[0].hash;

		using var jobsArr = new NativeList<JobHandle>(controllersData.Length, Allocator.Temp);
		var blobUniqueIndices = new NativeList<int>(Allocator.Temp);

		rigDefComponentLookup.Update(this);
		
		for (int i = 1; i <= controllersData.Length; ++i)
		{
			AnimatorControllerBakerData cd = i < controllersData.Length ? controllersData[i] : default;
			if (cd.hash != startHash)
			{
				var numDuplicates = i - startIndex;
				var blobAssetsSlice = new NativeSlice<AnimatorBlobAssets>(blobAssetsArr, startIndex, numDuplicates);
				var refController = controllersData[startIndex];
				var j = new CreateBlobAssetsJob()
				{
					inData = refController,
					outBlobAssets = blobAssetsSlice,
				#if RUKHANKA_DEBUG_INFO
					doLogging = dc.logAnimatorBaking,
				#endif
				};

				var jh = j.Schedule();
				jobsArr.Add(jh);
				blobUniqueIndices.Add(startIndex);

				startHash = cd.hash;
				startIndex = i;

				DebugLogging(refController, numDuplicates);
			}
		}

		var combinedJH = JobHandle.CombineDependencies(jobsArr.AsArray());
		using var ecb = new EntityCommandBuffer(Allocator.TempJob);

		var createComponentDatasJob = new CreateComponentDatasJob()
		{
			ecb = ecb.AsParallelWriter(),
			bakerData = controllersData,
			blobAssets = blobAssetsArr
		};

		createComponentDatasJob.ScheduleBatch(controllersData.Length, 32, combinedJH).Complete();
		
		//	Register blob assets in store to prevent memory leaks
		RegisterBlobAssetsInAssetStore(blobAssetsArr, blobUniqueIndices.AsArray());

		ecb.Playback(EntityManager);
		OnDestroy();

	#if RUKHANKA_DEBUG_INFO
		if (dc.logAnimatorBaking)
		{
			Debug.Log($"Total converted animator controllers: {controllersData.Length}");
			Debug.Log($"=== [AnimatorControllerConversionSystem] END CONVERSION ===");
		}
	#endif
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void RegisterBlobAssetsInAssetStore(NativeArray<AnimatorBlobAssets> blobAssetsArr, NativeArray<int> blobUniqueIndices)
	{
		var bakingSystem = World.GetExistingSystemManaged<BakingSystem>();
		var blobAssetStore = bakingSystem.BlobAssetStore;
		for (var i = 0; i < blobUniqueIndices.Length; ++i)
		{
			var idx = blobUniqueIndices[i];
			var ba = blobAssetsArr[idx];
			if (ba.parametersPerfectHashTableBlob.IsCreated)
				blobAssetStore.TryAdd(ref ba.parametersPerfectHashTableBlob);
			blobAssetStore.TryAdd(ref ba.controllerBlob);
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void DebugLogging(AnimatorControllerBakerData a, int numDuplicates)
	{
	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
		if (!dc.logAnimatorBaking) return;

		Debug.Log($"Creating blob asset for animator: '{a.name}'. Entities: {numDuplicates}. Clips: {a.controllerData.animationClips.Length}. Parameters: {a.controllerData.parameters.Length}. Layers: {a.controllerData.layers.Length}");
	#endif
	}
}
}

#endif