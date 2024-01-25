using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentLocomotionSystemGroup))]
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentDisplacementSystemGroup : ComponentSystemGroup { }
}
