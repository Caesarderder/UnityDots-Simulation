using System;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [UpdateAfter(typeof(AgentSpatialSystemGroup))]
    [UpdateInGroup(typeof(AgentSystemGroup))]
    public partial class AgentActionSystemGroup : ComponentSystemGroup { }
}
