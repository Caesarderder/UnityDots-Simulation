using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    [AddComponentMenu("Agents Navigation/Flock Group (Experimental)")]
    public class FlockGroupAuthoring : EntityBehaviour
    {
        [SerializeField]
        protected AgentAuthoring Leader;

        [SerializeField]
        protected float Radius = 6f;

        [SerializeField, Range(0, 0.3f)]
        protected float Cohesion = 0.1f;

        [SerializeField, Range(0, 0.3f)]
        protected float Alignment = 0.1f;

        [SerializeField]
        internal List<AgentAuthoring> Agents;

        [SerializeField]
        internal bool m_IncludeHierachy = false;

        /// <summary>
        /// Returns default component of <see cref="FlockGroup"/>.
        /// </summary>
        public FlockGroup DefaulFlockGroup => new FlockGroup
        {
            LeaderEntity = Leader.GetOrCreateEntity(),
            Radius = Radius,
            Alignment = Alignment,
            Cohesion = Cohesion,
        };

        /// <summary>
        /// <see cref="FlockGroup"/> component of this Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public FlockGroup EntityFlockGroup
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<FlockGroup>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// <see cref="FlockEntity"/> buffer of this Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public DynamicBuffer<FlockEntity> EntityFlockEntities
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<FlockEntity>(m_Entity);
        }

        /// <summary>
        /// Returns true if entity has <see cref="FlockGroup"/>.
        /// </summary>
        public bool HasEntityFlockGroup => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<FlockGroup>(m_Entity);

        private void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;

            var entity = GetOrCreateEntity();
            manager.AddComponentData(entity, DefaulFlockGroup);

            var agents = manager.AddBuffer<FlockEntity>(entity);
            agents.Capacity = Agents.Count;
            foreach (var agent in Agents)
            {
                agents.Add(new FlockEntity { Value = agent.GetOrCreateEntity() });
            }
            if (m_IncludeHierachy)
            {
                foreach (var agent in transform.GetComponentsInChildren<AgentAuthoring>())
                {
                    agents.Add(new FlockEntity { Value = agent.GetOrCreateEntity() });
                }
            }
        }
    }

    internal class FlockGroupBaker : Baker<FlockGroupAuthoring>
    {
        public override void Bake(FlockGroupAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.DefaulFlockGroup);

            var agents = AddBuffer<FlockEntity>(entity);
            agents.Capacity = authoring.Agents.Count;
            foreach (var agent in authoring.Agents)
            {
                agents.Add(new FlockEntity { Value = GetEntity(agent, TransformUsageFlags.Dynamic) });
            }
            if (authoring.m_IncludeHierachy)
            {
                foreach (var agent in authoring.transform.GetComponentsInChildren<AgentAuthoring>())
                {
                    agents.Add(new FlockEntity { Value = GetEntity(agent, TransformUsageFlags.Dynamic) });
                }
            }
        }
    }
}
