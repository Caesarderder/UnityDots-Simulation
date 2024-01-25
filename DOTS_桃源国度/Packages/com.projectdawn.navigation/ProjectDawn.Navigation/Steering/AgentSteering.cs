using System;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Agent's steering towards destination with arrival.
    /// </summary>
    [Obsolete("AgentSteering is deprecated, please use AgentLocomotion!", false)]
    public struct AgentSteering : IComponentData
    {
        /// <summary>
        /// Maximum movement speed when moving to destination.
        /// </summary>
        public float Speed;
        /// <summary>
        /// The maximum acceleration of an agent as it follows a path, given in units / sec^2.
        /// </summary>
        public float Acceleration;
        /// <summary>
        /// Maximum turning speed in (rad/s) while following a path.
        /// </summary>
        public float AngularSpeed;
        /// <summary>
        /// Stop within this distance from the target position.
        /// </summary>
        public float StoppingDistance;
        /// <summary>
        /// Should the agent brake automatically to avoid overshooting the destination point?
        /// </summary>
        public bool AutoBreaking;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentSteering Default => new()
        {
            Speed = 3.5f,
            Acceleration = 8,
            AngularSpeed = math.radians(120),
            StoppingDistance = 0.1f,
            AutoBreaking = true,
        };
    }
}
