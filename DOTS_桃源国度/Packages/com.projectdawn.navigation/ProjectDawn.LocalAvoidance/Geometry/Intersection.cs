using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProjectDawn.LocalAvoidance
{
    public class Intersection
    {
        /// <summary>
        /// Finds intersection angle range of two movings circles with given pointA velocity and pointB speed.
        /// Returns true if intersection exists and also two solutions. Time values can be negative or even infinity.
        /// </summary>
        public static bool IntersectionOfTwoMovingCircles(Circle circleA, float2 velocityA, Circle circleB, float speedB, float maxDistance, out Line angles, out Line times)
        {
            // Algorithm idea is to find two angles where function IntersectionOfTwoMovingCircles has only one solution
            // My testing showed that IntersectionOfTwoMovingCircles with one solution usually the maximum or minimum the angle where circles still collides
            // These solutions are found using iterative method which is of course has error

            // TODO: Investigate for non iterative solution.
            // In theory function: (circleA.Point.x + velocityA.x * t - circleB.Point.x + math.cos(angle) * speedB * t)^2 + (circleA.Point.y + velocityA.y * t - circleB.Point.y + math.sin(angle) * speedB * t)^2 = (circleA.Radius + circleB.Radius)^2
            // Solved for t and and segment under square root equalized to zero, would yield non iterative solution
            // However that produced much more complex function than interative solution

            // Force circles not to be overlapping
            float2 towards = circleA.Point - circleB.Point;
            float length = math.length(towards);
            float r = circleA.Radius + circleB.Radius + 1e-3f;
            if (length < r)
            {
                circleA.Point = circleB.Point + (towards / length) * r;
                length = r;
            }

            // Find any collision angle where we could iteratvely look for min and max angle of collision
            if (!IntersectionOfTwoMovingCirclesOneSolution(circleA, velocityA, circleB, speedB, out float startAngle, out float startTime))
            {
                angles = new Line();
                times = new Line();
                return false;
            }

            // TODO: Investigate if there is a value exposing it as parameter
            int numIterations = 14;

            // Find initial step size
            // Testing showed that using tangent to circle produces nice good step size
            float startStepSize = 2 * Circle.TangentLineToCircleAngle(circleA.Radius, math.max(speedB * startTime, circleA.Radius));

            float maxTime = speedB > 0 ? maxDistance / speedB : 0;

            UnityEngine.Assertions.Assert.IsTrue(math.isfinite(maxTime));

            // Find min angle
            {
                float stepSize = startStepSize;
                float angle = startAngle;
                float t = 0;
                for (int i = 0; i < numIterations; ++i)
                {
                    float newAngle = math.max(angle - stepSize, startAngle + -math.PI);
                    if (IntersectionOfTwoMovingCircles(circleA, velocityA, circleB, newAngle, speedB, out float t0, out float t1) &&
                        t0 >= 0 &&
                        t1 >= 0 &&
                        t0 < maxTime)
                    {
                        angle = newAngle;
                        t = t0;

                        if (math.abs(t0 - t1) < 1e-3f)
                            break;
                    }
                    else
                    {
                        stepSize /= 2;
                    }
                }
                angles.From = angle;
                times.From = t;
            }

            // Find max angle
            {
                float stepSize = startStepSize;
                float x = startAngle;
                float t = 0;
                for (int i = 0; i < numIterations; ++i)
                {
                    float newX = math.min(x + stepSize, startAngle + math.PI);
                    if (IntersectionOfTwoMovingCircles(circleA, velocityA, circleB, newX, speedB, out float t0, out float t1) &&
                        t0 >= 0 &&
                        t1 >= 0 &&
                        t0 < maxTime)
                    {
                        x = newX;
                        t = t0;

                        if (math.abs(t0 - t1) < 1e-3f)
                            break;
                    }
                    else
                    {
                        stepSize /= 2;
                    }
                }
                angles.To = x;
                times.To = t;
            }

            if (angles.Length == 0)
                return false;

            return true;
        }

        static bool IntersectionOfTwoMovingCirclesOneSolution(Circle circleA, float2 velocityA, Circle circleB, float speedB, out float angle, out float time)
        {
            if (IntersectionOfTwoMovingCircles(circleA, velocityA, circleB, speedB, out float t0, out float t1))
            {
                if (t0 >= 0 && math.isfinite(t0) && (t0 < t1 || !math.isfinite(t1) || t1 < 0))
                {
                    float2 point = circleA.Point + velocityA * t0;
                    float2 direction = math.normalizesafe(point - circleB.Point);
                    angle = math.atan2(direction.y, direction.x);
                    time = t0;
                    return true;
                }
                if (t1 >= 0 && math.isfinite(t1))
                {
                    float2 point = circleA.Point + velocityA * t1;
                    float2 direction = math.normalizesafe(point - circleB.Point);
                    angle = math.atan2(direction.y, direction.x);
                    time = t1;
                    return true;
                }
            }
            angle = 0;
            time = 0;
            return false;
        }

        /// <summary>
        /// Finds intersection times of two movings points with given pointA velocity and pointB speed, angle.
        /// Returns true if intersection exists and also two solutions. Time values can be negative or even infinity.
        /// </summary>
        public static bool IntersectionOfTwoMovingCircles(Circle circleA, float2 velocityA, Circle circleB, float angleB, float speedB, out float time0, out float time1)
        {
            float2 directionB = new float2(math.cos(angleB), math.sin(angleB));
            float2 velocityB = directionB * speedB;
            return IntersectionOfTwoMovingCircles(circleA, velocityA, circleB, velocityB, out time0, out time1);
        }

        /// <summary>
        /// Finds intersection times of two movings points with given pointA velocity and pointB velocity.
        /// Returns true if intersection exists and also two solutions. Time values can be negative or even infinity.
        /// </summary>
        public static bool IntersectionOfTwoMovingCircles(Circle circleA, float2 velocityA, Circle circleB, float2 velocityB, out float time0, out float time1)
        {
            float2 A = circleA.Point;
            float2 B = circleB.Point;
            float2 C = velocityA;
            float2 D = velocityB;

            float m = A.x - B.x;
            float n = A.y - B.y;

            float a = C.x;
            float b = D.x;
            float c = C.y;
            float d = D.y;

            float r = circleA.Radius + circleB.Radius;

            // length((A + C*t) - (B + D*t)) = r
            //
            // ((Ax + Cx*t) - (Bx + Dx*t))^2 + ((Ay + Cy*t) - (By + Dy*t))^2 = r^2
            //
            // (m + a*t - b*t)^2 + (n + c*t - d*t)^2 = r*r

            float discriminant = 2*((a*a) - 2*a*b + (b*b) + (c*c) - 2*c*d + (d*d));

            // positive - two solutions
            // zero - one solution
            // negative - no solutions
            if (discriminant < math.EPSILON)
            {
                time0 = 0;
                time1 = 0;
                return false;
            }

            float r0 = 2*a*m - 2*b*m + 2*c*n - 2*d*n;
            float r4 = ((m*m) + (n*n) - (r*r));
            float r5 = ((a*a) - 2*a*b + (b*b) + (c*c) - 2*c*d + (d*d));
            float r1 = (r0*r0) - 4*r4*r5;
            // Can not square root negative values
            if (r1 < 0)
            {
                time0 = 0;
                time1 = 0;
                return false;
            }
            float r2 = math.sqrt(r1);

            float r3 = -2*a*m + 2*b*m - 2*c*n + 2*d*n;

            time0 = (-r2 + r3) / discriminant;
            time1 = (r2 + r3) / discriminant;

            return true;
        }

        /// <summary>
        /// Finds intersection times of two movings circles with given pointA velocity and pointB speed.
        /// Returns true if intersection exists and also two solutions. Time values can be negative or even infinity.
        /// </summary>
        public static bool IntersectionOfTwoMovingCircles(Circle circleA, float2 velocityA, Circle circleB, float speedB, out float time0, out float time1)
        {
            // Check if circles are not intersecting already
            float distancesq = math.lengthsq(circleA.Point - circleB.Point);
            float r = circleA.Radius + circleB.Radius;
            if (distancesq <= r*r)
            {
                time0 = 0;
                time1 = 0;
                return true;
            }

            float x0 = circleA.Point.x;
            float y0 = circleA.Point.y;

            float x1 = circleB.Point.x;
            float y1 = circleB.Point.y;

            float m = x0 - x1;
            float n = y0 - y1;

            float a = velocityA.x;
            float b = velocityA.y;
            float c = speedB;

            // This function might look very complex, but actually only the solutions adds this complexity.
            // In nutshell this function is basically finding intersection of two parametric lines (https://en.wikipedia.org/wiki/Line_(geometry)), where slope is velocity.
            // It is extended to solve circle intersection problem by adding circle radius offsets.
            //
            // x0 + a*t = x1 + (c*t + r)*x
            // y0 + b*t = y1 + (c*t + r)*y
            //
            // x0 - x1 = c*t*x + r*x - a*t
            // y0 - y1 = c*t*y + r*y - b*t
            //
            // (x0 - x1 - r*x) / (c*x - a) = t
            // (y0 - y1 - r*y) / (c*y - b) = t
            //
            // (x0 - x1 - r*x) / (c*x - a) = (y0 - y1 - r*y) / (c*y - b)
            //
            // (x0 - x1 - r*x) * (c*y - b) = (y0 - y1 - r*y) * (c*x - a)
            //
            // (m - r*x) * (c*y - b) = (n - r*y) * (c*x - a)
            // m = x0 - x1
            // n = y0 - y1

            float discriminant = 2 * ((a*a)*(r*r) - 2*a*c*m*r + (b*b)*(r*r) - 2*b*c*n*r + (c*c)*(m*m) + (c*c)*(n*n));

            // positive - two solutions
            // zero - one solution
            // negative - no solutions
            if (discriminant < math.EPSILON)
            {
                time0 = 0;
                time1 = 0;
                return false;
            }

            // Here we need to decide if we solve it for dx or dy
            // We chose this based on which one has bigger difference as this is required for finding the time value
            bool condition = a != b ? math.abs(a) > math.abs(b) : math.abs(m) > math.abs(n);
            if (condition)
            {
                // Solve for x:
                // (m - r*x) * (c*y - b) = (n - r*y) * (c*x - a)
                // m = x0 - x1
                // n = y0 - y1
                // y = sqrt(1 - x*x)

                float r0 = 2*a*b*n*r - 2*a*c*(n*n) - 2*(b*b)*m*r + 2*b*c*m*n;
                float r1 = (a*a)*(n*n) - (a*a)*(r*r) - 2*a*b*m*n + 2*a*c*m*r + (b*b)*(m*m) - (c*c)*(m*m);
                float r2 = (a*a)*(r*r) - 2*a*c*m*r + (b*b)*(r*r) - 2*b*c*n*r + (c*c)*(m*m) + (c*c)*(n*n);

                float r3 = r0*r0 - 4*r1*r2;
                // Can not square root negative values
                if (r3 < 0)
                {
                    time0 = 0;
                    time1 = 0;
                    return false;
                }
                float r4 = math.sqrt(r3);

                float r5 = -2*a*b*n*r + 2*a*c*(n*n) + 2*(b*b)*m*r - 2*b*c*m*n;

                // We get two solution, because of: y = sqrt(1-x*x)
                float dx0 = (-r4 + r5) / discriminant;
                float dx1 = (r4 + r5) / discriminant;

                // Find times by solving: (m - r*x) / (c*x - a) = t
                time0 = (m - r*dx0) / (c * dx0 - a);
                time1 = (m - r*dx1) / (c * dx1 - a);
            }
            else
            {
                // Solve for y:
                // (m - r*x) * (c*y - b) = (n - r*y) * (c*x - a)
                // m = x0 - x1
                // n = y0 - y1
                // x = sqrt(1 - y*y)

                float r0 = -2*(a*a)*n*r + 2*a*b*m*r + 2*a*c*m*n - 2*b*c*(m*m);
                float r1 = (a*a)*(n*n) - 2*a*b*m*n + (b*b)*(m*m) - (b*b)*(r*r) + 2*b*c*n*r - (c*c)*(n*n);
                float r2 = (a*a)*(r*r) - 2*a*c*m*r + (b*b)*(r*r) - 2*b*c*n*r + (c*c)*(m*m) + (c*c)*(n*n);

                float r3 = r0*r0 - 4*r1*r2;
                // Can not square root negative values
                if (r3 < 0)
                {
                    time0 = 0;
                    time1 = 0;
                    return false;
                }
                float r4 = math.sqrt(r3);

                float r5 = 2*(a*a)*n*r - 2*a*b*m*r - 2*a*c*m*n + 2*b*c*(m*m);

                // We get two solution, because of: x = sqrt(1-y*y)
                float dy0 = (-r4 + r5) / discriminant;
                float dy1 = (r4 + r5) / discriminant;

                // Find times by solving: (n - r*y) / (c*y - b) = t
                time0 = (n - r*dy0) / (c * dy0 - b);
                time1 = (n - r*dy1) / (c * dy1 - b);
            }

            return true;
        }

        /// <summary>
        /// Finds intersection times of two movings points with given pointA velocity and pointB speed.
        /// Returns true if intersection exists and also two solutions. Time values can be negative or even infinity.
        /// </summary>
        public static bool IntersectionOfTwoMovingPoints(float2 pointA, float2 velocityA, float2 pointB, float speedB, out float time0, out float time1)
        {
            // Check if points are not same
            float distancesq = math.lengthsq(pointA - pointB);
            if (distancesq <= math.EPSILON)
            {
                time0 = 0;
                time1 = 0;
                return true;
            }

            float x0 = pointA.x;
            float y0 = pointA.y;

            float x1 = pointB.x;
            float y1 = pointB.y;

            float a = velocityA.x;
            float b = velocityA.y;
            float c = speedB;

            // This function might look very complex, but actually only the solutions adds this complexity.
            // In nutshell this function is basically finding intersection of two parametric lines (https://en.wikipedia.org/wiki/Line_(geometry)), where slope is velocity.
            //
            // x0 + a*t = x1 + c*t*x
            // y0 + b*t = y1 + c*t*y
            //
            // x0 - x1 = c*t*x - a*t
            // y0 - y1 = c*t*y - b*t
            //
            // (x0 - x1) / (c*x - a) = t
            // (y0 - y1) / (c*y - b) = t
            //
            // (x0 - x1) / (c*x - a) = (y0 - y1) / (c*y - b)
            //
            // (x0 - x1) * (c*y - b) = (y0 - y1) * (c*x - a)
            //
            // m * (c*y - b) = n * (c*x - a)
            // m = x0 - x1
            // n = y0 - y1

            float m = x0 - x1;
            float n = y0 - y1;

            float discriminant = (c*c)*(m*m) + (c*c)*(n*n);

            // positive - two solutions
            // zero - one solution
            // negative - no solutions
            if (discriminant < math.EPSILON)
            {
                time0 = 0;
                time1 = 0;
                return false;
            }

            // Here we need to decide if we solve it for dx or dy
            // We chose this based on which one has bigger difference as this is required for finding the time value
            bool condition = a != b ? math.abs(a) > math.abs(b) : math.abs(m) > math.abs(n);
            if (condition)
            {
                // Solve for x:
                // m * (c*y - b) = n * (c*x - a)
                // m = x0 - x1
                // n = y0 - y1
                // y = sqrt(1 - x*x)

                float r0 = -(a*a)*(c*c)*(m*m)*(n*n) + 2*a*b*(c*c)*(m*m*m)*n - (b*b)*(c*c)*(m*m*m*m) + (c*c*c*c)*(m*m*m*m) + (c*c*c*c)*(m*m)*(n*n);

                // Can not square root negative values
                if (r0 < 0)
                {
                    time0 = 0;
                    time1 = 0;
                    return false;
                }
                float r1 = math.sqrt(r0);

                float r2 = a*c*(n*n) - b*c*m*n;

                // We get two solution, because of: y = sqrt(1-x*x)
                float dx0 = (-r1 + r2) / discriminant;
                float dx1 = (r1 + r2) / discriminant;

                // Find times by solving: (x0 - x1) / (c*x - a) = t
                time0 = m / (c * dx0 - a);
                time1 = m / (c * dx1 - a);
            }
            else
            {
                // Solve for y:
                // m * (c*y - b) = n * (c*x - a)
                // m = x0 - x1
                // n = y0 - y1
                // x = sqrt(1 - y*y)

                float r0 = -(a*a)*(c*c)*(n*n*n*n) + 2*a*b*(c*c)*m*(n*n*n) - (b*b)*(c*c)*(m*m)*(n*n) + (c*c*c*c)*(m*m)*(n*n) + (c*c*c*c)*(n*n*n*n);

                // Can not square root negative values
                if (r0 < 0)
                {
                    time0 = 0;
                    time1 = 0;
                    return false;
                }
                float r1 = math.sqrt(r0);

                float r2 = -a*c*m*n + b*c*(m*m);

                // We get two solution, because of: x = sqrt(1-y*y)
                float dy0 = (-r1 + r2) / discriminant;
                float dy1 = (r1 + r2) / discriminant;

                // Find times by solving: (y0 - y1) / (c*y - b) = t
                time0 = n / (c * dy0 - b);
                time1 = n / (c * dy1 - b);
            }

            return true;
        }

        public static bool IntersectionOfCircleAndSegment(Circle circle, float2 segmentStart, float2 segmentEnd, out float2 intersectionStart, out float2 intersectionEnd)
        {
            // Based on https://stackoverflow.com/questions/1073336/circle-line-segment-collision-detection-algorithm
            float2 d = segmentEnd - segmentStart;
            float2 f = segmentStart - circle.Point;

            float a = dot(d, d);
            float b = 2 * dot(f, d);
            float c = dot(f, f) - circle.Radius * circle.Radius;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                intersectionStart = 0;
                intersectionEnd = 0;
                return false;
            }

            discriminant = sqrt(discriminant);

            // Find two solutions
            float2 t = new float2(-b - discriminant, -b + discriminant) / (2 * a);

            // Check if ranges overlap (t.x, t.y) and (0, 1)
            float2 tt = clamp(new float2(0, 1), t.x, t.y);
            if (tt.y - tt.x == 0)

            // Check if solutions are segments
            //if (all(t < 0) || all(t > 1))
            {
                intersectionStart = 0;
                intersectionEnd = 0;
                return false;
            }

            intersectionStart = d * tt.x + segmentStart;
            intersectionEnd = d * tt.y + segmentStart;
            return true;
        }
    }
}
