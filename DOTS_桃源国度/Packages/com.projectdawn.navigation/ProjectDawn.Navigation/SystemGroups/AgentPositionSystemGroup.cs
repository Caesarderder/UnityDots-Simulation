using System;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [Obsolete("AgentPositionSystemGroup is deprecated, please use AgentDisplacementSystemGroup!", false)]
    [UpdateInGroup(typeof(AgentDisplacementSystemGroup))]
    public partial class AgentPositionSystemGroup : ComponentSystemGroup { }
}
