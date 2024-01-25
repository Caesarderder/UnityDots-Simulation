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
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    public partial struct ReadAgentRectTransformSystem : ISystem
    {
        EntityQuery m_Query;
        ComponentLookup<LocalTransform> m_TransformLookup;
        RectTransformAccessArray m_RectTransformAccessArray;

        public void OnCreate(ref SystemState state)
        {
            m_Query = QueryBuilder()
                .WithAll<Agent>()
                .WithAllRW<RectTransform>()
                .WithAll<LocalTransform>()
                .Build();
            m_TransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var entities = m_Query.ToEntityArray(Allocator.TempJob);
            m_RectTransformAccessArray.Update(m_Query);

            m_TransformLookup.Update(ref state);

            state.Dependency = new ReadAgentTransformJob
            {
                Entities = entities,
                TransformLookup = m_TransformLookup,
            }.Schedule(m_RectTransformAccessArray, state.Dependency);
        }

        [BurstCompile]
        struct ReadAgentTransformJob : IJobParallelForTransform
        {
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> Entities;

            [ReadOnly]
            public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(int index, TransformAccess transformAccess)
            {
                Entity entity = Entities[index];

                var transform = TransformLookup[entity];
                transformAccess.position = transform.Position;
                transformAccess.rotation = transform.Rotation;
            }
        }
    }
}
