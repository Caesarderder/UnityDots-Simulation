using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    [RequireComponent(typeof(AgentAuthoring))]
    [AddComponentMenu("Agents Navigation/Agent Reciprocal Avoid (Experimental)")]
    [DisallowMultipleComponent]
    public class AgentReciprocalAvoidAuthoring : MonoBehaviour
    {
        [SerializeField]
        protected float Radius = 6;

        [SerializeField]
        protected NavigationLayers m_Layers = NavigationLayers.Everything;

        Entity m_Entity;

        /// <summary>
        /// Returns default component of <see cref="AgentReciprocalAvoid"/>.
        /// </summary>
        public AgentReciprocalAvoid DefaultAvoid => new AgentReciprocalAvoid
        {
            Radius = Radius,
            Layers = m_Layers,
        };

        /// <summary>
        /// <see cref="AgentReciprocalAvoid"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentReciprocalAvoid EntityAvoid
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentReciprocalAvoid>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentReciprocalAvoid"/>.
        /// </summary>
        public bool HasEntityAvoid => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentReciprocalAvoid>(m_Entity);

        void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
            world.EntityManager.AddComponentData(m_Entity, DefaultAvoid);
        }

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
                world.EntityManager.RemoveComponent<AgentReciprocalAvoid>(m_Entity);
        }
    }

    internal class AgentReciprocalAvoidBaker : Baker<AgentReciprocalAvoidAuthoring>
    {
        public override void Bake(AgentReciprocalAvoidAuthoring authoring) => AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.DefaultAvoid);
    }
}
