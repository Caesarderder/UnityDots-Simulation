using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

namespace ProjectDawn.Navigation.Sample.Scenarios
{
    /// <summary>
    /// System that steers agent towards destination.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentSeekingSystemGroup))]
    public partial struct TankSeekingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new TankSteeringJob().ScheduleParallel();
        }

        [BurstCompile]
        partial struct TankSteeringJob : IJobEntity
        {
            public void Execute(ref AgentBody body, in TankLocomotion locomotion, in LocalTransform transform)
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
