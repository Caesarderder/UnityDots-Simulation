using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentSpatialSystemGroup : ComponentSystemGroup { }
}
