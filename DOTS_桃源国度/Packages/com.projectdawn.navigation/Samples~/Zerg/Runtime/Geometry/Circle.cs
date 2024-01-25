using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    /// <summary>
    /// A circle is a shape consisting of all points in a plane that are at a given distance from a given point, the centre.
    /// </summary>
    [DebuggerDisplay("Center = {Center}, Radius = {Radius}")]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Circle : IEquatable<Circle>
    {
        /// <summary>
        /// Center of the circle.
        /// </summary>
        public float2 Center;

        /// <summary>
        /// Radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Diameter of the circle. Diameter = 2 * Radius. 
        /// </summary>
        public float Diameter
        {
            get => 2f * Radius;
            set => Radius = value * 0.5f;
        }

        /// <summary>
        /// Returns the perimeter of the circle.
        /// The perimeter of a circle is its boundary or the complete arc length of the periphery of a circle.
        /// </summary>
        public float Perimeter => 2f * math.PI * Radius;

        /// <summary>
        /// Returns the area of the circle.
        /// </summary>
        public float Area => math.PI * Radius * Radius;

        public Circle(float2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <inheritdoc />
        public bool Equals(Circle other) => math.all(Center == other.Center & Radius == other.Radius);

        /// <inheritdoc />
        public override bool Equals(object other) => throw new NotImplementedException();

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc />
        public static bool operator ==(Circle lhs, Circle rhs) => math.all(lhs.Center == rhs.Center & lhs.Radius == rhs.Radius);

        /// <inheritdoc />
        public static bool operator !=(Circle lhs, Circle rhs) => !(lhs == rhs);

        /// <summary>
        /// Returns a point on the perimeter of this circle that is closest to the specified point.
        /// </summary>
        public float2 ClosestPoint(float2 point)
        {
            float2 towards = point - Center;
            float length = math.length(towards);
            if (length < math.EPSILON)
                return point;

            // TODO: Performance check branch vs bursted max
            if (length < Radius)
                return point;

            return Center + Radius*(towards/length);
        }

        /// <summary>
        /// Returns minimum bounding circle that contains both circles.
        /// </summary>
        public static Circle Union(Circle a, Circle b)
        {
            float2 towards = a.Center - b.Center;
            float length = math.length(towards);
            if (length < math.EPSILON)
                return new Circle(a.Center, math.max(a.Radius, b.Radius));

            float2 origin = b.Center;
            float2 direction = towards / length;

            float min = math.min(-b.Radius, length - a.Radius);
            float max = math.max(b.Radius, length + a.Radius);

            float2 from = origin + direction * min;
            float2 to = origin + direction * max;
            return new Circle((from + to) * 0.5f, (max - min) * 0.5f);
            //return new Circle((a.Center + b.Center) * 0.5f, math.distance(a.Center, b.Center) * 0.5f + math.max(a.Radius, b.Radius));
        }
    }
}
