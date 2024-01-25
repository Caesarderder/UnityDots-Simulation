using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Agent avoidance of nearby agents using Sonar Avoidance algorithm.
    /// </summary>
    public struct AgentSonarAvoid : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// The maximum distance at which agent will attempt to avoid nearby agents.
        /// </summary>
        public float Radius;
        /// <summary>
        /// The angle of obstacle inserted behind agent velocity.
        /// </summary>
        public float Angle;
        /// <summary>
        /// The maximum angle at which agent will attempt to nearby agents.
        /// </summary>
        public float MaxAngle;
        /// <summary>
        /// Mode that modifies avoidance behaviour.
        /// </summary>
        public SonarAvoidMode Mode;
        /// <summary>
        /// Whenever agent should stop if all directions are blocked.
        /// </summary>
        public bool BlockedStop;

        public NavigationLayers Layers;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentSonarAvoid Default => new()
        {
            Radius = 2,
            Angle = math.radians(230),
            MaxAngle = math.radians(300),
            Mode = SonarAvoidMode.IgnoreBehindAgents,
            BlockedStop = false,
            Layers = NavigationLayers.Everything,
        };
    }
}
