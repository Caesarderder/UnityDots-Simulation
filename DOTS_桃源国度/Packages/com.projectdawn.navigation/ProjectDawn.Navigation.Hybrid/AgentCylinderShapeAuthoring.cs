using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    /// <summary>
    /// Agent cylindre shape.
    /// </summary>
    [RequireComponent(typeof(AgentAuthoring))]
    [AddComponentMenu("Agents Navigation/Agent Cylinder Shape")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/game-objects/shape.html")]
    public class AgentCylinderShapeAuthoring : MonoBehaviour
    {
        [SerializeField]
        protected float Radius = 0.5f;
        [SerializeField]
        protected float Height = 2f;

        Entity m_Entity;

        /// <summary>
        /// Returns default component of <see cref="AgentShape"/>.
        /// </summary>
        public AgentShape DefaultShape => new AgentShape
        {
            Radius = Radius,
            Height = Height,
            Type = ShapeType.Cylinder,
        };

        /// <summary>
        /// <see cref="AgentShape"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentShape EntityShape
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentShape>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentShape"/>.
        /// </summary>
        public bool HasEntityShape => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentShape>(m_Entity);

        void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
            world.EntityManager.AddComponentData(m_Entity, DefaultShape);
        }

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
                world.EntityManager.RemoveComponent<AgentShape>(m_Entity);
        }
    }

    internal class AgentCapsuleShapeBaker : Baker<AgentCylinderShapeAuthoring>
    {

        public override void Bake(AgentCylinderShapeAuthoring authoring) => AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.DefaultShape);
    }
}
