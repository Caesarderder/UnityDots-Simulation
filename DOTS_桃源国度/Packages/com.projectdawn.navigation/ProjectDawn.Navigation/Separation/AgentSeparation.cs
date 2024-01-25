using Unity.Entities;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Agent separation from nearby agents.
    /// </summary>
    public struct AgentSeparation : IComponentData
    {
        /// <summary>
        /// Radius at which agent will attempt separate from each other.
        /// </summary>
        public float Radius;
        /// <summary>
        /// The weight of the separation force.
        /// </summary>
        public float Weight;

        public NavigationLayers Layers;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentSeparation Default => new()
        {
            Radius = 2,
            Weight = 1,
            Layers = NavigationLayers.Everything,
        };
    }
}
