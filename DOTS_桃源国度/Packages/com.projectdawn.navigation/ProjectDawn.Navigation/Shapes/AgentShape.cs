using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Navigation
{
    public enum ShapeType
    {
        Circle,
        Cylinder,
    }

    /// <summary>
    /// Represents agent shape in space.
    /// In the future this component might be split into specific shape components instead of having one big generic.
    /// </summary>
    public struct AgentShape : IComponentData
    {
        /// <summary>
        /// The radius of the agent.
        /// </summary>
        public float Radius;
        /// <summary>
        /// The height of the agent for purposes of passing under obstacles, etc.
        /// </summary>
        public float Height;
        /// <summary>
        /// The type of shape.
        /// </summary>
        public ShapeType Type;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentShape Default => new()
        {
            Radius = 0.5f,
            Height = 2,
            Type = ShapeType.Cylinder,
        };

        /// <summary>
        /// Returns up vector of shape.
        /// </summary>
        /// <returns>Returns up vector of shape.</returns>
        public float3 GetUp()
        {
            if (Type == ShapeType.Cylinder)
                return new float3(0, 1, 0);
            else
                return new float3(0, 0, 1);
        }
    }
}
