using Unity.Burst;
using Unity.Collections;
using Unity.Deformations;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

[DisableAutoCreation]
[RequireMatchingQueriesForUpdate]
partial struct AnimationApplicationSystem: ISystem
{
	private EntityQuery
		boneObjectEntitiesWithParentQuery,
		boneObjectEntitiesNoParentQuery;

	NativeParallelHashMap<Hash128, BlobAssetReference<BoneRemapTableBlob>> rigToSkinnedMeshRemapTables;

/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		using var eqb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorEntityRefComponent, Parent>()
		.WithAllRW<LocalTransform>();
		boneObjectEntitiesWithParentQuery = ss.GetEntityQuery(eqb0);

		using var eqb1 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorEntityRefComponent>()
		.WithNone<Parent>()
		.WithAllRW<LocalTransform>();
		boneObjectEntitiesNoParentQuery = ss.GetEntityQuery(eqb1);

		rigToSkinnedMeshRemapTables = new NativeParallelHashMap<Hash128, BlobAssetReference<BoneRemapTableBlob>>(128, Allocator.Persistent);
	}
	
/////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnDestroy(ref SystemState ss)
	{
		rigToSkinnedMeshRemapTables.Dispose();
	}

/////////////////////////////////////////////////////////////////////////////////

    [BurstCompile]
    public void OnUpdate(ref SystemState ss)
    {
		ref var runtimeData = ref SystemAPI.GetSingletonRW<RuntimeAnimationData>().ValueRW;

		var fillRigtoSkinnedMeshRemapTablesJH = FillRigToSkinBonesRemapTableCache(ref ss);

		//	Compute root motion
		var rootMotionJobHandle = ComputeRootMotion(ref ss, runtimeData, fillRigtoSkinnedMeshRemapTablesJH);

		//	Propagate local animated transforms to the entities with parents
		var propagateTRSWithParentsJobHandle = PropagateAnimatedBonesToEntitiesTRS(ref ss, runtimeData, boneObjectEntitiesWithParentQuery, rootMotionJobHandle);

		//	Convert local bone transforms to absolute (root relative) transforms
		var makeAbsTransformsJobHandle = MakeAbsoluteBoneTransforms(ref ss, runtimeData, propagateTRSWithParentsJobHandle);

		//	Propagate absolute animated transforms to the entities without parents
		var propagateTRNoParentsJobHandle = PropagateAnimatedBonesToEntitiesTRS(ref ss, runtimeData, boneObjectEntitiesNoParentQuery, makeAbsTransformsJobHandle);

		//	Make corresponding skin matrices for all skinned meshes
		var applySkinJobHandle = ApplySkinning(ref ss, runtimeData, propagateTRNoParentsJobHandle);

		ss.Dependency = applySkinJobHandle;
    }

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle ComputeRootMotion(ref SystemState ss, in RuntimeAnimationData runtimeData, JobHandle dependsOn)
	{
		var computeRootMotionJob = new ComputeRootMotionJob()
		{
			animatedBonePoses = runtimeData.animatedBonesBuffer,
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap
		};

		var jh = computeRootMotionJob.ScheduleParallel(dependsOn);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle FillRigToSkinBonesRemapTableCache(ref SystemState ss)
	{
		var rigDefinitionComponentLookup = SystemAPI.GetComponentLookup<RigDefinitionComponent>(true);

	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dc);
	#endif
		var skinnedMeshWithAnimatorQuery = SystemAPI.QueryBuilder().WithAll<SkinMatrix, AnimatedSkinnedMeshComponent>().Build();
		var skinnedMeshes = skinnedMeshWithAnimatorQuery.ToComponentDataListAsync<AnimatedSkinnedMeshComponent>(ss.WorldUpdateAllocator, ss.Dependency, out var skinnedMeshFromQueryJH);

		var j = new FillRigToSkinBonesRemapTableCacheJob()
		{
			rigDefinitionArr = rigDefinitionComponentLookup,
			rigToSkinnedMeshRemapTables = rigToSkinnedMeshRemapTables,
			skinnedMeshes = skinnedMeshes,
		#if RUKHANKA_DEBUG_INFO
			doLogging = dc.logAnimationCalculationProcesses
		#endif
		};

		var rv = j.Schedule(skinnedMeshFromQueryJH);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle PropagateAnimatedBonesToEntitiesTRS(ref SystemState ss, in RuntimeAnimationData runtimeData, EntityQuery eq, JobHandle dependsOn)
	{
		var rigDefinitionComponentLookup = SystemAPI.GetComponentLookup<RigDefinitionComponent>(true);

		var propagateAnimationJob = new PropagateBoneTransformToEntityTRSJob()
		{
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap,
			boneTransforms = runtimeData.animatedBonesBuffer,
			rigDefLookup = rigDefinitionComponentLookup,
		};

		var jh = propagateAnimationJob.ScheduleParallel(eq, dependsOn);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle MakeAbsoluteBoneTransforms(ref SystemState ss, in RuntimeAnimationData runtimeData, JobHandle dependsOn)
	{
		var makeAbsTransformsJob = new MakeAbsoluteTransformsJob()
		{
			boneTransforms = runtimeData.animatedBonesBuffer,
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap,
			boneTransformFlags = runtimeData.boneTransformFlagsHolderArr
		};

		var jh = makeAbsTransformsJob.ScheduleParallel(dependsOn);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle ApplySkinning(ref SystemState ss, in RuntimeAnimationData runtimeData, JobHandle dependsOn)
	{
		var rigDefinitionComponentLookup = SystemAPI.GetComponentLookup<RigDefinitionComponent>(true);

		var animationApplyJob = new ApplyAnimationToSkinnedMeshJob()
		{
			boneTransforms = runtimeData.animatedBonesBuffer,
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap,
			rigDefinitionLookup = rigDefinitionComponentLookup,
			rigToSkinnedMeshRemapTables = rigToSkinnedMeshRemapTables,
		};

		var jh = animationApplyJob.ScheduleParallel(dependsOn);
		return jh;
	}

}
}
