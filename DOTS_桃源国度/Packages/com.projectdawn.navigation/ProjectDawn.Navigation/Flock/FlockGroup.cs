using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Agent group for flocking. Modifies desired direction to match flock direction.
    /// </summary>
    public struct FlockGroup : IComponentData
    {
        /// <summary>
        /// Leader of the flock.
        /// </summary>
        public Entity LeaderEntity;
        /// <summary>
        /// Maximum radius of the flock from leader.
        /// </summary>
        public float Radius;
        /// <summary>
        /// Scaler of cohesion.
        /// </summary>
        public float Cohesion;
        /// <summary>
        /// Scaler of alignment.
        /// </summary>
        public float Alignment;
        /// <summary>
        /// Average position of the flock.
        /// </summary>
        public float3 AveragePositions;
        /// <summary>
        /// Average direction of the flock.
        /// </summary>
        public float3 AverageDirection;
    }

    /// <summary>
    /// Single entity of the <see cref="FlockGroup"/>.
    /// </summary>
    public struct FlockEntity : IBufferElementData
    {
        public Entity Value;
    }
}
