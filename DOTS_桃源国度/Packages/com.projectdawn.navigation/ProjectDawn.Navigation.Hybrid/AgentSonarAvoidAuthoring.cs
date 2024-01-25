using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    /// <summary>
    /// Agent avoidance of nearby agents.
    /// </summary>
    [RequireComponent(typeof(AgentAuthoring))]
    [AddComponentMenu("Agents Navigation/Agent Sonar Avoid")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/game-objects/avoidance/sonar-avoidance.html")]
    public class AgentAvoidAuthoring : MonoBehaviour
    {
        [SerializeField]
        protected float Radius = 2;

        [SerializeField]
        [Range(0, 360)]
        protected float Angle = 230;

        [SerializeField]
        [Range(0, 360)]
        protected float MaxAngle = 300;

        [SerializeField]
        protected SonarAvoidMode Mode = SonarAvoidMode.IgnoreBehindAgents;

        [SerializeField]
        protected bool BlockedStop = false;

        [SerializeField]
        protected bool UseWalls = false;

        [SerializeField]
        protected NavigationLayers m_Layers = NavigationLayers.Everything;

        Entity m_Entity;

        /// <summary>
        /// Returns default component of <see cref="AgentSonarAvoid"/>.
        /// </summary>
        public AgentSonarAvoid DefaultAvoid => new AgentSonarAvoid
        {
            Radius = Radius,
            Angle = math.radians(Angle),
            MaxAngle = math.radians(MaxAngle),
            Mode = Mode,
            BlockedStop = BlockedStop,
            Layers = m_Layers,
        };

        /// <summary>
        /// <see cref="AgentSonarAvoid"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentSonarAvoid EntityAvoid
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentSonarAvoid>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentSonarAvoid"/>.
        /// </summary>
        public bool HasEntityAvoid => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentSonarAvoid>(m_Entity);

        void Awake()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            m_Entity = GetComponent<AgentAuthoring>().GetOrCreateEntity();
            world.EntityManager.AddComponentData(m_Entity, DefaultAvoid);

            // Sync in case it was created as disabled
            if (!enabled)
                world.EntityManager.SetComponentEnabled<AgentSonarAvoid>(m_Entity, false);

            if (UseWalls)
            {
                if (!gameObject.TryGetComponent<AgentNavMeshAuthoring>(out var _))
                    throw new System.InvalidOperationException("Property UseWalls can not be enabled without AgentNavMeshAuthoring component!");
                world.EntityManager.AddComponentData(m_Entity, new NavMeshBoundary { Radius = Radius + 1 });
                world.EntityManager.AddBuffer<NavMeshWall>(m_Entity);
            }
        }

        void OnDestroy()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.RemoveComponent<AgentSonarAvoid>(m_Entity);
            if (UseWalls)
            {
                if (!gameObject.TryGetComponent<AgentNavMeshAuthoring>(out var _))
                    throw new System.InvalidOperationException("Property UseWalls can not be enabled without AgentNavMeshAuthoring component!");
                world.EntityManager.RemoveComponent<NavMeshBoundary>(m_Entity);
                world.EntityManager.RemoveComponent<NavMeshWall>(m_Entity);
            }
        }

        void OnEnable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.SetComponentEnabled<AgentSonarAvoid>(m_Entity, true);
        }

        void OnDisable()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;
            world.EntityManager.SetComponentEnabled<AgentSonarAvoid>(m_Entity, false);
        }
    }

    internal class AgentSonarAvoidBaker : Baker<AgentAvoidAuthoring>
    {
        public override void Bake(AgentAvoidAuthoring authoring) => AddComponent(GetEntity(TransformUsageFlags.Dynamic), authoring.DefaultAvoid);
    }
}
