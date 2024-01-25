using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentActionSystemGroup))]
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentSeekingSystemGroup : ComponentSystemGroup { }
}
