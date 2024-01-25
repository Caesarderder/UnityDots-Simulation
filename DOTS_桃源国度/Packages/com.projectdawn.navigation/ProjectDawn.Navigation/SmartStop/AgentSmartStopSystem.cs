using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentActionSystemGroup))]
    public partial struct AgentSmartStopSystem : ISystem
    {
        ComponentLookup<AgentSmartStop> m_SmartStopLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_SmartStopLookup = state.GetComponentLookup<AgentSmartStop>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingleton<AgentSpatialPartitioningSystem.Singleton>();

            m_SmartStopLookup.Update(ref state);

            new AgentSmartStopJob
            {
                Spatial = spatial,
                SmartStopLookup = m_SmartStopLookup,
                DeltaTime = Time.DeltaTime,
            }.Schedule();
        }

        [BurstCompile]
        partial struct AgentSmartStopJob : IJobEntity
        {
            [ReadOnly]
            public AgentSpatialPartitioningSystem.Singleton Spatial;
            [ReadOnly]
            public ComponentLookup<AgentSmartStop> SmartStopLookup;
            public float DeltaTime;

            public void Execute(Entity entity, ref AgentBody body, ref GiveUpStopTimer timer, in AgentSmartStop smartStop, in LocalTransform transform)
            {
                if (body.IsStopped)
                    return;

                // Check if give up needs destination update
                if (smartStop.GiveUpStop.Enabled && math.any(body.Destination != timer.Destination))
                {
                    timer.Destination = body.Destination;
                    timer.Progress = 0.0f;
                }

                // This is just a high performance foreach for nearby agents
                // It is basically as: foreach (var nearbyAgent in GetNearbyAgents()) Spatial.Execute(...)
                // For each nearby agent check if they reached destination
                var action = new FindTargetAction
                {
                    SmartStopLookup = SmartStopLookup,
                    Entity = entity,
                    Body = body,
                    SmartStop = smartStop,
                    Transform = transform,
                };
                Spatial.QuerySphere(transform.Position, smartStop.HiveMindStop.Radius, ref action);

                // Check, if agent should do hive mind stop
                // If any nearby agent reached destination, this agent should stop too
                if (action.Stop)
                {
                    body.Stop();
                    return;
                }

                // Check, if agent should do give up stop
                if (action.Accumulate)
                {
                    timer.Progress = math.min(timer.Progress + DeltaTime * smartStop.GiveUpStop.FatigueSpeed, 1.0f);
                    if (timer.Progress == 1.0f)
                    {
                        body.Stop();
                        return;
                    }
                }
                else
                {
                    timer.Progress = math.max(timer.Progress - DeltaTime * smartStop.GiveUpStop.RecoverySpeed, 0.0f);
                }
            }

            [BurstCompile]
            struct FindTargetAction : ISpatialQueryEntity
            {
                [ReadOnly]
                public ComponentLookup<AgentSmartStop> SmartStopLookup;

                public Entity Entity;
                public AgentBody Body;
                public AgentSmartStop SmartStop;
                public LocalTransform Transform;

                // Output if this agent should stop
                public bool Stop;

                public bool Accumulate;

                public void Execute(Entity entity, AgentBody body, AgentShape shape, LocalTransform transform)
                {
                    // Exclude itself
                    if (Entity == entity)
                        return;

                    // Exclude still moving ones
                    if (!body.IsStopped)
                        return;

                    // Check if they collide
                    float distance = math.distance(Transform.Position, transform.Position);
                    if (SmartStop.HiveMindStop.Radius < distance)
                        return;

                    if (SmartStop.GiveUpStop.Enabled)
                        Accumulate = true;

                    // Check if neaby one has smart stop
                    if (!SmartStopLookup.TryGetComponent(entity, out AgentSmartStop brain) || !brain.HiveMindStop.Enabled)
                        return;

                    // Check if they have similar destinations within the radius
                    float distance2 = math.distance(Body.Destination, body.Destination);
                    if (SmartStop.HiveMindStop.Radius < distance2)
                        return;

                    Stop = true;
                }
            }
        }
    }
}
