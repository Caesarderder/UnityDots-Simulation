using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation.Hybrid
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct AgentSetDestinationDeferredSystem : ISystem
    {
        ComponentLookup<AgentBody> m_BodyLookup;
        NativeQueue<Entity> m_Entities;
        NativeQueue<float3> m_Destinations;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_BodyLookup = GetComponentLookup<AgentBody>(isReadOnly:false);
            m_Entities = new NativeQueue<Entity>(Allocator.Persistent);
            m_Destinations = new NativeQueue<float3>(Allocator.Persistent);

            state.EntityManager.AddComponentData(state.SystemHandle, new Singleton
            {
                Entities = m_Entities,
                Destinations = m_Destinations,
            });
        }

        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            m_Entities.Dispose();
            m_Destinations.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            GetSingletonRW<Singleton>();

            if (m_Entities.IsEmpty())
                return;

            m_BodyLookup.Update(ref state);

            state.Dependency = new SetDestinationJob
            {
                Entities = m_Entities,
                Destinations = m_Destinations,
                BodyLookup = m_BodyLookup,
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        struct SetDestinationJob : IJob
        {
            public ComponentLookup<AgentBody> BodyLookup;
            public NativeQueue<Entity> Entities;
            public NativeQueue<float3> Destinations;

            public void Execute()
            {
                while (Entities.TryDequeue(out Entity entity) && Destinations.TryDequeue(out float3 destination))
                {
                    if (BodyLookup.TryGetComponent(entity, out AgentBody body))
                    {
                        body.Destination = destination;
                        body.IsStopped = false;
                        BodyLookup[entity] = body;
                    }
                }
            }
        }

        public struct Singleton : IComponentData
        {
            internal NativeQueue<Entity> Entities;
            internal NativeQueue<float3> Destinations;

            public void SetDestinationDeferred(Entity entity, float3 destination)
            {
                Entities.Enqueue(entity);
                Destinations.Enqueue(destination);
            }
        }
    }
}
