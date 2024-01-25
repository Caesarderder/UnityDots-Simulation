using ProjectDawn.LocalAvoidance.RVO;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentForceSystemGroup))]
    public partial struct AgentReciprocalAvoidSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingleton<AgentSpatialPartitioningSystem.Singleton>();
            float deltaTime = Time.DeltaTime;
            new AgentReciprocalAvoidJob
            {
                Spatial = spatial,
                DeltaTime = deltaTime,
            }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct AgentReciprocalAvoidJob : IJobEntity
        {
            [ReadOnly]
            public AgentSpatialPartitioningSystem.Singleton Spatial;
            public float DeltaTime;

            public void Execute(Entity entity, ref AgentBody body, in AgentShape shape, in AgentReciprocalAvoid avoid, in AgentLocomotion locomotion, in LocalTransform transform)
            {
                if (body.IsStopped)
                    return;

                if (math.length(body.Velocity) < 1e-3f)
                    return;

                float3 desiredDirection = body.Force;

                var reciprocal = new OptimalReciprocalCollisionAvoidance(transform.Position.xy, body.Velocity.xy, desiredDirection.xy * locomotion.Speed, 50, locomotion.Speed, avoid.Radius, shape.Radius, 10f, 10f);

                var action = new Action
                {
                    Entity = entity,
                    Reciprocal = reciprocal,
                };
                Spatial.QueryCylinder(transform.Position, shape.Radius, shape.Height, ref action, avoid.Layers);

                reciprocal.GetDesiredVelocity(DeltaTime, out float2 velocity);

                // Convert direction. It can be non unit vector, which will desribe that it should go slower
                body.Force = new float3(velocity, 0) / locomotion.Speed;

                reciprocal.Dispose();
            }

            struct Action : ISpatialQueryEntity
            {
                public Entity Entity;
                public OptimalReciprocalCollisionAvoidance Reciprocal;

                public void Execute(Entity entity, AgentBody body, AgentShape shape, LocalTransform transform)
                {
                    // Skip itself
                    if (Entity == entity)
                        return;

                    Reciprocal.InsertObstacle(transform.Position.xy, body.Velocity.xy, shape.Radius);
                }
            }

        }
    }
}
