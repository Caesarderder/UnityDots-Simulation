using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{ 

[DisableAutoCreation]
[BurstCompile]
[RequireMatchingQueriesForUpdate]
public partial struct FillAnimationsFromControllerSystem: ISystem
{
	EntityQuery fillAnimationsBufferQuery;

	BufferTypeHandle<AnimatorControllerLayerComponent> controllerLayersBufferHandleRO;
	BufferTypeHandle<AnimatorControllerParameterComponent> controllerParametersBufferHandleRO;
	BufferTypeHandle<AnimationToProcessComponent> animationToProcessBufferHandle;
	EntityTypeHandle entityTypeHandle;

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		var eqBuilder1 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent, AnimationToProcessComponent>();
		
		fillAnimationsBufferQuery = ss.GetEntityQuery(eqBuilder1);

		controllerLayersBufferHandleRO = ss.GetBufferTypeHandle<AnimatorControllerLayerComponent>(true);
		controllerParametersBufferHandleRO = ss.GetBufferTypeHandle<AnimatorControllerParameterComponent>(true);
		animationToProcessBufferHandle = ss.GetBufferTypeHandle<AnimationToProcessComponent>();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		controllerLayersBufferHandleRO.Update(ref ss);
		controllerParametersBufferHandleRO.Update(ref ss);
		animationToProcessBufferHandle.Update(ref ss);
		entityTypeHandle.Update(ref ss);

		var fillAnimationsBufferJob = new FillAnimationsBufferJob()
		{
			controllerLayersBufferHandle = controllerLayersBufferHandleRO,
			controllerParametersBufferHandle = controllerParametersBufferHandleRO,
			animationToProcessBufferHandle = animationToProcessBufferHandle,
			entityTypeHandle = entityTypeHandle,
		};

		ss.Dependency = fillAnimationsBufferJob.ScheduleParallel(fillAnimationsBufferQuery, ss.Dependency);
	}
}
}
