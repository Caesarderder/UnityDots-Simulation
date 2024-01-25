using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentSeekingSystemGroup))]
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentPathingSystemGroup : ComponentSystemGroup { }
}
