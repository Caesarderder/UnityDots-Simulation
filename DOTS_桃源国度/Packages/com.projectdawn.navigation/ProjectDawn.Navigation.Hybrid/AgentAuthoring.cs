using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    public enum AgentMotionType
    {
        /// <summary>
        /// Agent is static. It's entity will not have AgentBody and AgentSteering components.
        /// </summary>
        Static = 0,
        /// <summary>
        /// Agent is dynamic. It's entity will not have AgentSteering component.
        /// </summary>
        Dynamic = 1,
        /// <summary>
        /// Agent is dynamic. It's entity will have have AgentBody and AgentSteering components. 
        /// </summary>
        //[Obsolete("AgentMotionType.Steering is deprecated, please use AgentMotionType.Locomotion!", false)]
        [InspectorName("Steering (Deprecated)")]
        Steering = 2,
        /// <summary>
        /// Agent is dynamic. It's entity will have have AgentBody and AgentLocomotion components. 
        /// </summary>
        DefaultLocomotion = 3,
    }

    /// <summary>
    /// Main component of agent that enables motion.
    /// </summary>
    [AddComponentMenu("Agents Navigation/Agent")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/game-objects/agent.html")]
    public class AgentAuthoring : EntityBehaviour
    {
        [SerializeField]
        internal AgentMotionType MotionType = AgentMotionType.DefaultLocomotion;

        [SerializeField]
        protected float Speed = 3.5f;

        [SerializeField]
        protected float Acceleration = 8;

        [SerializeField]
        protected float AngularSpeed = 120;

        [SerializeField]
        protected float StoppingDistance = 0.1f;

        [SerializeField]
        protected bool AutoBreaking = true;

        [SerializeField]
        protected NavigationLayers m_Layers = NavigationLayers.Default;

        /// <summary>
        /// Returns default component of <see cref="Agent"/>.
        /// </summary>
        public Agent DefaultAgent => new()
        {
            Layers = m_Layers,
        };

        /// <summary>
        /// Returns default component of <see cref="AgentBody"/>.
        /// </summary>
        public AgentBody DefaultBody => new()
        {
            Destination = transform.position,
            IsStopped = true,
        };

        /// <summary>
        /// Returns default component of <see cref="AgentSteering"/>.
        /// </summary>
        [Obsolete("DefaultSteering is deprecated, please use DefaultLocomotion!", false)]
        public AgentSteering DefaultSteering => new()
        {
            Speed = Speed,
            Acceleration = Acceleration,
            AngularSpeed = math.radians(AngularSpeed),
            StoppingDistance = StoppingDistance,
            AutoBreaking = AutoBreaking,
        };

        /// <summary>
        /// Returns default component of <see cref="AgentLocomotion"/>.
        /// </summary>
        public AgentLocomotion DefaultLocomotion => new()
        {
            Speed = Speed,
            Acceleration = Acceleration,
            AngularSpeed = math.radians(AngularSpeed),
            StoppingDistance = StoppingDistance,
            AutoBreaking = AutoBreaking,
        };

        /// <summary>
        /// <see cref="Agent"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public Agent EntityAgent
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Agent>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// <see cref="AgentBody"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentBody EntityBody
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentBody>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// <see cref="AgentSteering"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        [Obsolete("EntitySteering is deprecated, please use EntityLocomotion!", false)]
        public AgentSteering EntitySteering
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentSteering>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// <see cref="AgentLocomotion"/> component of this <see cref="AgentAuthoring"/> Entity.
        /// Accessing this property is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public AgentLocomotion EntityLocomotion
        {
            get => World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<AgentLocomotion>(m_Entity);
            set => World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(m_Entity, value);
        }

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentBody"/>.
        /// </summary>
        public bool HasEntityBody => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentBody>(m_Entity);

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentSteering"/>.
        /// </summary>
        [Obsolete("HasEntitySteering is deprecated, please use HasEntityLocomotion!", false)]
        public bool HasEntitySteering => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentSteering>(m_Entity);

        /// <summary>
        /// Returns true if <see cref="AgentAuthoring"/> entity has <see cref="AgentLocomotion"/>.
        /// </summary>
        public bool HasEntityLocomotion => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<AgentLocomotion>(m_Entity);

        /// <summary>
        /// Sets or updates the destination thus triggering the calculation for a new path.
        /// Calling this method is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public void SetDestination(float3 target)
        {
            var body = EntityBody;
            body.Destination = target;
            body.IsStopped = false;
            EntityBody = body;
        }

        /// <summary>
        /// Sets or updates the destination thus triggering the calculation for a new path.
        /// This call is deferred and destination will only changed later in the frame.
        /// </summary>
        public void SetDestinationDeferred(float3 destination)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return;

            var system = world.GetOrCreateSystem<AgentSetDestinationDeferredSystem>();

            var manager = world.EntityManager;
            var ecb = manager.GetComponentData<AgentSetDestinationDeferredSystem.Singleton>(system);
            ecb.SetDestinationDeferred(m_Entity, destination);
        }

        /// <summary>
        /// Stop agent movement.
        /// Calling this method is potentially heavy operation as it will require wait for agent jobs to finish.
        /// </summary>
        public void Stop()
        {
            var body = EntityBody;
            body.IsStopped = true;
            body.Velocity = 0;
            EntityBody = body;
        }

        void Awake()
        {
            m_Entity = GetOrCreateEntity();
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;

            manager.AddComponentData(m_Entity, new LocalTransform
            {
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = 1,
            });

            // Transform access requires this
            manager.AddComponentObject(m_Entity, transform);

            manager.AddComponentData(m_Entity, DefaultAgent);
            if (MotionType != AgentMotionType.Static)
                manager.AddComponentData(m_Entity, DefaultBody);
#pragma warning disable 0618
            if (MotionType == AgentMotionType.Steering)
                manager.AddComponentData(m_Entity, DefaultSteering);
#pragma warning restore 0618
            if (MotionType == AgentMotionType.DefaultLocomotion)
                manager.AddComponentData(m_Entity, DefaultLocomotion);
        }
    }

    internal class AgentBaker : Baker<AgentAuthoring>
    {
        public override void Bake(AgentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.DefaultAgent);
            if (authoring.MotionType != AgentMotionType.Static)
                AddComponent(entity, authoring.DefaultBody);
#pragma warning disable 0618
            if (authoring.MotionType == AgentMotionType.Steering)
                AddComponent(entity, authoring.DefaultSteering);
#pragma warning restore 0618
            if (authoring.MotionType == AgentMotionType.DefaultLocomotion)
                AddComponent(entity, authoring.DefaultLocomotion);
        }
    }
}
