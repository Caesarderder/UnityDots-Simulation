using Unity.Entities;

namespace ProjectDawn.Navigation.Sample.Scenarios
{
    /// <summary>
    /// Tanks locomotion that moves towards destination with arrival.
    /// </summary>
    public struct TankLocomotion : IComponentData
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
    }
}
