using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    /// <summary>
    /// Enables collisio with other agents.
    /// </summary>
    [RequireComponent(typeof(AgentAuthoring))]
    [AddComponentMenu("Agents Navigation/Agent Collider")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/game-objects/collider.html")]
    public class AgentColliderAuthoring : MonoBehaviour
    {
        [SerializeField]
        protected NavigationLayers m_Layers = NavigationLayers.Everything;

        Entity m_Entity;

        /// <summary>
        /// Returns default component of <see cref="AgentCollider"/>.
        /// </summary>
        public AgentCollider DefaultCollider => new()
        {
            Layers = m_Layers,
        };

        /// <summary>
        /// <see cref="AgentCollider"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentCollider EntityCollider
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentCollider>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
            world.EntityManager.AddComponentData(m_Entity, DefaultCollider);
        }

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.RemoveComponent<AgentCollider>(m_Entity);
        }

        void OnEnable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.SetComponentEnabled<AgentCollider>(m_Entity, true);
        }

        void OnDisable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.SetComponentEnabled<AgentCollider>(m_Entity, false);
        }
    }

    internal class AgentColliderBaker : Baker<AgentColliderAuthoring>
    {
        public override void Bake(AgentColliderAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.DefaultCollider);
        }
    }
}
