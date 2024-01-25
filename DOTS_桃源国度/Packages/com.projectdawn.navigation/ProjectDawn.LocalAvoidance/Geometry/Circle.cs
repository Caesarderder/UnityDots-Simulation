// @ 2022 Lukas Chodosevicius

using System;
using Unity.Mathematics;

namespace ProjectDawn.LocalAvoidance
{
    public struct Circle
    {
        public float2 Point;
        public float Radius;

        public Circle(float2 point, float radius)
        {
            Point = point;
            Radius = radius;
        }

        public static bool Collide(Circle a, Circle b)
        {
            float r = a.Radius + b.Radius;
            return math.lengthsq(a.Point - b.Point) < r * r - math.EPSILON;
        }

        public static float TangentLineToCircleAngle(float circleRadius, float distanceFromCircle)
        {
             // Here we find tangent line to circle https://en.wikipedia.org/wiki/Tangent_lines_to_circles
            // Tangent line to a circle is a line that touches the circle at exactly one point, never entering the circle's interior
            var opp = circleRadius;
            var hyp = distanceFromCircle;
            var tangentLineAngle = math.asin(math.clamp(opp / hyp, -1, 1));
            return tangentLineAngle;
        }
    }
}
