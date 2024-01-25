using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentPathingSystemGroup))]
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentForceSystemGroup : ComponentSystemGroup { }
}
