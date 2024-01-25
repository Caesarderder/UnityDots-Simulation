using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation.Hybrid
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial struct WriteAgentRectTransformSystem : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<LocalTransform> m_TransformLookup;
        RectTransformAccessArray m_RectTransformAccessArray;

        public void OnCreate(ref SystemState state)
        {
            m_Query = QueryBuilder()
                .WithAll<Agent>()
                .WithAny<RectTransform>()
                .WithAllRW<LocalTransform>()
                .Build();
            m_TransformLookup = state.GetComponentLookup<LocalTransform>();
        }

        public void OnDestroy(ref SystemState state)
        {
            m_RectTransformAccessArray.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entities = m_Query.ToEntityArray(Allocator.TempJob);

            m_RectTransformAccessArray.Update(m_Query);

            m_TransformLookup.Update(ref state);

            state.Dependency = new WriteAgentTransformJob
            {
                Entities = entities,
                TransformLookup = m_TransformLookup,
            }.Schedule(m_RectTransformAccessArray, state.Dependency);
        }

        [BurstCompile]
        struct WriteAgentTransformJob : IJobParallelForTransform
        {
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> Entities;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(int index, [ReadOnly] TransformAccess transformAccess)
            {
                Entity entity = Entities[index];

                var transform = TransformLookup[entity];
                transform.Position = transformAccess.position;
                transform.Rotation = transformAccess.rotation;
                TransformLookup[entity] = transform;
            }
        }
    }
}
