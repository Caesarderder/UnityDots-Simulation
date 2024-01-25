using System;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [Obsolete("AgentSteeringSystemGroup is deprecated, please use AgentSeekingSystemGroup!", false)]
    [UpdateInGroup(typeof(AgentSeekingSystemGroup))]
    public partial class AgentSteeringSystemGroup : ComponentSystemGroup { }
}
