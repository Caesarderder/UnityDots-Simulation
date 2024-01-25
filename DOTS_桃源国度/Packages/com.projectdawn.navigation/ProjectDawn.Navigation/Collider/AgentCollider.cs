using Unity.Entities;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// This component tags, if agent should collide with nearby agents.
    /// </summary>
    public struct AgentCollider : IComponentData, IEnableableComponent
    {
        public NavigationLayers Layers;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentCollider Default => new() { Layers = NavigationLayers.Everything };
    }
}
