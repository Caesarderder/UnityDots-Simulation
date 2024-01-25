using System;
using Unity.Entities;

namespace ProjectDawn.Navigation
{
    [Obsolete("AgentTransformSystemGroup is deprecated, please use AgentLocomotionSystemGroup!", false)]
    [UpdateInGroup(typeof(AgentLocomotionSystemGroup))]
    public partial class AgentTransformSystemGroup : ComponentSystemGroup { }
}
