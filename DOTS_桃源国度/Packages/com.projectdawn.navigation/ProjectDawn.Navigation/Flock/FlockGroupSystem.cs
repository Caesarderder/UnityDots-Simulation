using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using static Unity.Mathematics.math;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that calculates flock grouo average position and direction of flock entities.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentForceSystemGroup))]
    public partial struct FlockGroupSystem : ISystem
    {
        ComponentLookup<AgentBody> m_BodyLookup;
        ComponentLookup<LocalTransform> m_TransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_BodyLookup = state.GetComponentLookup<AgentBody>(true);
            m_TransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_BodyLookup.Update(ref state);
            m_TransformLookup.Update(ref state);

            new FlockGroupJob
            {
                BodyLookup = m_BodyLookup,
                TransformLookup = m_TransformLookup,
            }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct FlockGroupJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<AgentBody> BodyLookup;
            [ReadOnly]
            public ComponentLookup<LocalTransform> TransformLookup;

            public void Execute(ref FlockGroup group, in DynamicBuffer<FlockEntity> flock)
            {
                if (!TransformLookup.TryGetComponent(group.LeaderEntity, out LocalTransform transform))
                    return;

                // Calculate average position and direction of entities
                float3 averagePosition = 0;
                float3 averageDirection = 0;
                int count = 0;
                for (int index = 0; index < flock.Length; ++index)
                {
                    var entity = flock[index].Value;
                    if (!BodyLookup.TryGetComponent(entity, out AgentBody flockBody))
                        continue;
                    if (!TransformLookup.TryGetComponent(entity, out LocalTransform flockTransform))
                        continue;

                    float dist = distance(flockTransform.Position, transform.Position);

                    if (dist < group.Radius)
                    {
                        averagePosition += flockTransform.Position;
                        averageDirection += normalizesafe(flockBody.Velocity);
                        count++;
                    }
                }

                // Early out
                if (count == 0)
                {
                    group.AveragePositions = 0;
                    group.AverageDirection = 0;
                    return;
                }

                group.AveragePositions = averagePosition / count;
                group.AverageDirection = averageDirection / count;
            }
        }
    }
}
