using Unity.Entities;

namespace ProjectDawn.Navigation
{
#if AGENTS_NAVIGATION_REGULAR_UPDATE
    [UpdateInGroup(typeof(SimulationSystemGroup))]
#else
#if PHYSICS_1_0_0_OR_NEWER
    [UpdateBefore(typeof(Unity.Physics.Systems.PhysicsSystemGroup))] // Navigation systems needs to execute before physics
#endif
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
#endif
    public partial class AgentSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
#if AGENTS_NAVIGATION_REGULAR_UPDATE
            if (SystemAPI.Time.DeltaTime == 0)
                return;
#endif
            base.OnUpdate();
        }
    }
}
