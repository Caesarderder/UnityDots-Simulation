using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that steers agent towards destination.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentSteeringSystemGroup))]
    [Obsolete("AgentSteeringSystem is deprecated, please use AgentSeekingSystem!", false)]
    public partial struct AgentSteeringSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AgentSteeringJob().ScheduleParallel();
        }

        [BurstCompile]
        partial struct AgentSteeringJob : IJobEntity
        {
            public void Execute(ref AgentBody body, in AgentSteering steering, in LocalTransform transform)
            {
                if (body.IsStopped)
                    return;

                float3 towards = body.Destination - transform.Position;
                float distance = math.length(towards);
                float3 desiredDirection = distance > math.EPSILON ? towards / distance : float3.zero;
                body.Force = desiredDirection;
                body.RemainingDistance = distance;
            }
        }
    }
}
