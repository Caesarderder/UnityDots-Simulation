// @ 2022 Lukas Chodosevicius

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.LocalAvoidance
{
    /// <summary>
    /// This structure can be used for constructing local objet avoidance structure and finding the closest direction for it.
    /// It is composed of three main methods: Constructor, InsertObstacle and FindClosestDirection.
    /// </summary>
    [DebuggerDisplay("IsCreated = {IsCreated}")]
    [BurstCompile]
    public struct SonarAvoidance : IDisposable
    {
        float3 m_Position;
        quaternion m_Rotation;
        float m_InnerRadius;
        float m_OuterRadius;
        float m_Speed;
        float m_Angle;
        NativeList<SonarNode> m_Nodes;

        /// <summary>
        /// Root node. Always starts at zero handle.
        /// </summary>
        SonarNodeHandle Root => new SonarNodeHandle();

        /// <summary>
        /// Position of sonar.
        /// </summary>
        public float3 Position => m_Position;

        /// <summary>
        /// Rotation of sonar.
        /// </summary>
        public quaternion Rotation => m_Rotation;

        /// <summary>
        /// Inner radius of sonar.
        /// </summary>
        public float InnerRadius => m_InnerRadius;

        /// <summary>
        /// Outer radius of sonar.
        /// </summary>
        public float OuterRadius => m_OuterRadius;

        /// <summary>
        /// Speed of sonar.
        /// </summary>
        public float Speed => m_Speed;

        /// <summary>
        /// True if structure is created.
        /// </summary>
        public bool IsCreated => m_Nodes.IsCreated;

        /// <summary>
        /// Constructs sonar avoidance without properties. After this constructor you must call <see cref="SonarAvoidance.Set"/> at least once to setup correct properties.
        /// </summary>
        /// <param name="allocator">Allocator type</param>
        public SonarAvoidance(Allocator allocator)
        {
            m_Position = 0;
            m_Rotation = quaternion.identity;
            m_InnerRadius = 0;
            m_OuterRadius = 0;
            m_Speed = 0;
            m_Angle = 0;
            m_Nodes = new NativeList<SonarNode>(16, allocator);
        }

        /// <summary>
        /// Reconstructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="direction">Direction of sonar. Note this is forward direction on x axis, not z like LookRotation uses</param>
        /// <param name="up">Up direction</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="angle">The total angle in radians of sonar volume. One way to think it as range from -angle/2 to angle/2 the desired direction and avoidance direction will be.</param>
        public void Set(float3 position, float3 velocity, float3 direction, float3 up, float innerRadius, float outerRadius, float angle)
        {
            Set(position, velocity, DirectionToRotation(direction, up), innerRadius, outerRadius, angle);
        }

        /// <summary>
        /// Reconstructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="rotation">Rotation of sonar</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="angle">The total angle in radians of sonar volume. One way to think it as range from -angle/2 to angle/2 the desired direction and avoidance direction will be.</param
        public void Set(float3 position, float3 velocity, quaternion rotation, float innerRadius, float outerRadius, float angle)
        {
            CheckIsCreated();
            CheckArguments(rotation, innerRadius, outerRadius, angle);

            m_Position = position;
            m_Rotation = rotation;
            m_InnerRadius = innerRadius;
            m_OuterRadius = outerRadius;
            m_Speed = math.length(velocity);
            m_Angle = angle;

            Clear();
        }

        /// <summary>
        /// Remove all inserted obstacles.
        /// </summary>
        public void Clear()
        {
            CheckIsCreated();

            m_Nodes.Clear();

            // Create Root node with childs left and right
            CreateNode(new Line(-m_Angle, m_Angle));
            var left = CreateNode(new Line(-m_Angle, 0));
            var right = CreateNode(new Line(0, m_Angle));
            m_Nodes[Root] = new SonarNode
            {
                Line = new Line(-m_Angle, m_Angle),
                Left = left,
                Right = right,
            };
        }

        /// <summary>
        /// Inserts radius obstacle into sonar.
        /// </summary>
        /// <param name="direction">Direction of obstacle from sonar</param>
        /// <param name="radius">Radius of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacle(float3 direction, float radius)
        {
            CheckIsCreated();

            var directionLS = ToLocalSpace(direction);
            var angle = DirectionLSToAngle(directionLS);

            var radiusHalf = radius * 0.5f;
            var angleRight = angle - radiusHalf;
            var angleLeft = angle + radiusHalf;

            InsertObstacle(new Line(angleRight, angleLeft));
            return true;
        }

        [Obsolete("This method is obsolete, please use InsertObstacleCircle")]
        public bool InsertObstacle(float3 obstaclePosition, float3 obstacleVelocity, float obstacleRadius) => InsertObstacleCircle(obstaclePosition, obstacleVelocity, obstacleRadius);

        /// <summary>
        /// Inserts sphere obstacle into sonar.
        /// </summary>
        /// <param name="obstaclePosition">Position of obstacle</param>
        /// <param name="obstacleVelocity">Velocity of obstacle (Zero can be used for non moving obstacle)</param>
        /// <param name="obstacleRadius">Radius of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacleCircle(float3 obstaclePosition, float3 obstacleVelocity, float obstacleRadius)
        {
            CheckIsCreated();

            //if (math.all(obstacleVelocity == 0))
            //    return InsertObstacleCircle(obstaclePosition, obstacleRadius);

            var obstaclePositionLS = ToLocalSpace(obstaclePosition - m_Position);
            var obstacleVelocityLS = ToLocalSpace(obstacleVelocity);

            Circle current = new Circle(0, m_InnerRadius);
            Circle obstacle = new Circle(obstaclePositionLS, obstacleRadius);
            if (Intersection.IntersectionOfTwoMovingCircles(obstacle, obstacleVelocityLS, current, m_Speed, m_OuterRadius + m_InnerRadius, out Line angles, out Line times))
            {
                InsertObstacle(angles);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts sphere obstacle into sonar.
        /// </summary>
        /// <param name="obstaclePosition">Position of obstacle</param>
        /// <param name="obstacleRadius">Radius of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacleCircle(float3 obstaclePosition, float obstacleRadius)
        {
            CheckIsCreated();

            var obstaclePositionLS = ToLocalSpace(obstaclePosition - m_Position);

            if (math.length(obstaclePositionLS) > obstacleRadius + m_InnerRadius + m_OuterRadius)
                return false;

            var towardsLS = obstaclePositionLS;
            var directionLS = math.normalizesafe(towardsLS);

            float towardsLength = math.length(obstaclePositionLS);

            // Here we find tangent line to circle https://en.wikipedia.org/wiki/Tangent_lines_to_circles
            // Tangent line to a circle is a line that touches the circle at exactly one point, never entering the circle's interior
            var opp = obstacleRadius + m_InnerRadius;
            var hyp = math.max(towardsLength, obstacleRadius);
            var tangentLineAngle = Circle.TangentLineToCircleAngle(opp, hyp);

            // Falloff angle if it is close to the outer radius
            if (towardsLength > m_OuterRadius)
            {
                // Based on forumla: a*a = b*b + c*c - 2*c*b*cosA
                // That finds triangle angle from 3 sides

                float a = opp;
                float b = m_OuterRadius;
                float c = towardsLength;

                float cosA = (a * a - b * b - c * c) / (-2 * b * c);
                float A = math.acos(cosA);
                tangentLineAngle = A;
            }

            // Convert direction to angles
            float angle = DirectionLSToAngle(directionLS);

            var angleRight = angle - tangentLineAngle;
            var angleLeft = angle + tangentLineAngle;

            InsertObstacle2(new Line(angleRight, angleLeft));

            return false;
        }

        /// <summary>
        /// Inserts line obstacle into sonar.
        /// </summary>
        /// <param name="obstacleStartPosition">Start position of obstacle</param>
        /// <param name="obstacleEndPosition">End position of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacleLine(float3 obstacleStartPosition, float3 obstacleEndPosition)
        {
            CheckIsCreated();

            var startLS = ToLocalSpace(obstacleStartPosition - m_Position);
            var endLS = ToLocalSpace(obstacleEndPosition - m_Position);

            // Find segment that intersects with the sonar
            if (!Intersection.IntersectionOfCircleAndSegment(new Circle(0, m_OuterRadius + m_InnerRadius), startLS, endLS, out startLS, out endLS))
                return false;

            float startAngle = DirectionLSToAngle(startLS);
            float diffAngle = SignedAngle(startLS, endLS);
            float endAngle = startAngle + diffAngle;

            float startLength = math.max(math.length(startLS), m_InnerRadius);
            float endLength = math.max(math.length(endLS), m_InnerRadius);

            float startOffset = Circle.TangentLineToCircleAngle(InnerRadius, math.max(startLength, m_InnerRadius));
            float endOffset = Circle.TangentLineToCircleAngle(InnerRadius, math.max(endLength, m_InnerRadius));

            // Falloff angle if it is close to the outer radius
            if (startLength > m_OuterRadius)
            {
                // Based on forumla: a*a = b*b + c*c - 2*c*b*cosA
                // That finds triangle angle from 3 sides

                float a = m_InnerRadius;
                float b = m_OuterRadius;
                float c = startLength;

                float cosA = (a * a - b * b - c * c) / (-2 * b * c);
                float A = math.acos(cosA);
                startOffset = A;
            }
            // Falloff angle if it is close to the outer radius
            if (endLength > m_OuterRadius)
            {
                // Based on forumla: a*a = b*b + c*c - 2*c*b*cosA
                // That finds triangle angle from 3 sides

                float a = m_InnerRadius;
                float b = m_OuterRadius;
                float c = endLength;

                float cosA = (a * a - b * b - c * c) / (-2 * b * c);
                float A = math.acos(cosA);
                endOffset = A;
            }

            float min = math.min(endAngle - endOffset, startAngle - startOffset);
            float max = math.max(startAngle + startOffset, endAngle + endOffset);

            InsertObstacle(new Line(min, max));

            return true;
        }

        /// <summary>
        /// Inserts line obstacle into sonar without using the radius. Counter clockwise wall will be ignored.
        /// </summary>
        /// <param name="obstacleStartPosition">Start position of obstacle</param>
        /// <param name="obstacleEndPosition">End position of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacleLineIgnoreRadius(float3 obstacleStartPosition, float3 obstacleEndPosition)
        {
            CheckIsCreated();

            var startLS = ToLocalSpace(obstacleStartPosition - m_Position);
            var endLS = ToLocalSpace(obstacleEndPosition - m_Position);

            // Find segment that intersects with the sonar
            if (!Intersection.IntersectionOfCircleAndSegment(new Circle(0, m_OuterRadius), startLS, endLS, out startLS, out endLS))
                return false;

            float startAngle = DirectionLSToAngle(startLS);
            float diffAngle = SignedAngle(startLS, endLS);
            float endAngle = startAngle + diffAngle;

            // Ignore the backface
            if (diffAngle <= -0.001f)
                return false;

            if (diffAngle > ( math.PI - 0.001f))
            {
                InsertObstacle(new Line(startAngle, endAngle));
            }
            else
            {
                float min = math.min(endAngle, startAngle);
                float max = math.max(startAngle, endAngle);

                InsertObstacle(new Line(min, max));
            }

            return true;
        }

        /// <summary>
        /// Finds closest desired direction that is not obstructed by obstacle.
        /// </summary>
        /// <param name="direction">Closest direction found</param>
        /// <returns> True if direction was found</returns>
        public bool FindClosestDirection(out float3 direction)
        {
            CheckIsCreated();

            // Find closest angle in left side nodes
            var successLeft = false;
            var angleLeft = float.MaxValue;
            FindClosestAngle(m_Nodes[Root].Left, ref angleLeft, ref successLeft);

            // Find closest angle in right side nodes
            var successRight = false;
            var angleRight = float.MaxValue;
            FindClosestAngle(m_Nodes[Root].Right, ref angleRight, ref successRight);

            if (successLeft && successRight)
            {
                if (math.abs(angleLeft) < math.abs(angleRight))
                {
                    direction = ToWorldSpace(AngleToDirectionLS(angleLeft));
                    return true;
                }
                else
                {
                    direction = ToWorldSpace(AngleToDirectionLS(angleRight));
                    return true;
                }
            }
            else if (successLeft)
            {
                direction = ToWorldSpace(AngleToDirectionLS(angleLeft));
                return true;
            }
            else if (successRight)
            {
                direction = ToWorldSpace(AngleToDirectionLS(angleRight));
                return true;
            }

            direction = float3.zero;
            return false;
        }

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            m_Nodes.Dispose();
        }

        /// <summary>
        /// Constructs copy of sonar avoidance. No memory is shared between copy and original.
        /// </summary>
        /// <param name="other">Copies from</param>
        /// <param name="allocator">Allocator type</param>
        public void CopyFrom(in SonarAvoidance other)
        {
            CheckIsCreated();
            other.CheckIsCreated();

            m_Position = other.m_Position;
            m_Rotation = other.m_Rotation;
            m_InnerRadius = other.m_InnerRadius;
            m_OuterRadius = other.m_OuterRadius;
            m_Speed = other.m_Speed;
            m_Angle = other.m_Angle;

            // Make a copy of nodes
            // Uses unsafe context for faster copy API
            unsafe
            {
                var length = other.m_Nodes.Length;
                m_Nodes.ResizeUninitialized(length);
                UnsafeUtility.MemCpy(m_Nodes.GetUnsafePtr(), other.m_Nodes.GetUnsafePtr(), sizeof(SonarNode) * length);
            }
        }

        /// <summary>
        /// This is almost same as Quaternion.LookRotation just uses forward as x axis instead of z.
        /// </summary>
        /// <param name="forward">Forward direction on x axis</param>
        /// <param name="up">Up direction</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion DirectionToRotation(float3 forward, float3 up)
        {
            float3 t = math.normalize(math.cross(up, forward));
            return new quaternion(new float3x3(forward, math.cross(t, forward), t));
        }

        /// <summary>
        /// This is almost same as Quaternion.LookRotation just uses forward as x axis instead of z.
        /// </summary>
        /// <param name="forward">Forward direction on x axis</param>
        /// <param name="up">Up direction</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDirectionToRotation(float3 forward, float3 up, out quaternion rotation)
        {
            float3 t = math.normalize(math.cross(up, forward));
            rotation = new quaternion(new float3x3(forward, math.cross(t, forward), t));
            if (math.any(!math.isfinite(rotation.value)))
                return false;
            return true;
        }

        void InsertObstacle(Line rangeLS)
        {
            var angleRight = rangeLS.From;
            var angleLeft = rangeLS.To;

            // Early out if obstacle is very small
            if (rangeLS.Length < math.EPSILON)
                return;

            if (angleRight < -math.PI)
            {
                InsertObstacle(m_Nodes[Root].Right, new Line(math.PI + (angleRight + math.PI), math.PI));
                InsertObstacle(m_Nodes[Root].Left, new Line(-math.PI, angleLeft));
            }
            else if (angleLeft > math.PI)
            {
                InsertObstacle(m_Nodes[Root].Right, new Line(angleRight, math.PI));
                InsertObstacle(m_Nodes[Root].Left, new Line(-math.PI, angleLeft - math.PI * 2));
            }
            else
            {
                InsertObstacle(Root, new Line(angleRight, angleLeft));
            }
        }

        bool InsertObstacle2(Line rangeLS)
        {
            CheckIsCreated();

            var angleRight = rangeLS.From;
            var angleLeft = rangeLS.To;

            InsertObstacle(Root, new Line(angleRight, angleLeft));
            return true;
        }

        void InsertObstacle(SonarNodeHandle handle, Line line)
        {
            CheckHandle(handle);

            SonarNode node = m_Nodes[handle];

            var nodeLine = node.Line;
            if (nodeLine.Length == 0)
                return;

            if (Line.CutLine(nodeLine, line, out var result))
            {
                if (node.IsLeaf)
                {
                    if (result.SegmentCount == 2)
                    {
                        node.Left = CreateNode(result.Segment0);
                        node.Right = CreateNode(result.Segment1);
                        m_Nodes[handle] = node;
                    }
                    else
                    {
                        node.Line = result.Segment0;
                        m_Nodes[handle] = node;
                    }
                }
                else
                {
                    InsertObstacle(node.Right, line);
                    InsertObstacle(node.Left, line);
                }
            }
        }

        SonarNodeHandle CreateNode(Line line)
        {
            m_Nodes.Add(new SonarNode(line));
            return new SonarNodeHandle { Index = m_Nodes.Length - 1 };
        }

        void FindClosestAngle(SonarNodeHandle handle, ref float angle, ref bool success)
        {
            var node = m_Nodes[handle];
            var line = node.Line;

            if (line.Length <= 0.01f)
                return;

            if (node.IsLeaf)
            {
                if (math.abs(line.From) < math.abs(angle))
                {
                    angle = line.From;
                    success = true;
                }
                if (math.abs(line.To) < math.abs(angle))
                {
                    angle = line.To;
                    success = true;
                }
            }
            else
            {
                FindClosestAngle(node.Left, ref angle, ref success);
                FindClosestAngle(node.Right, ref angle, ref success);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float2 ToLocalSpace(float3 value) => math.mul(math.conjugate(m_Rotation), value).xz;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float3 ToWorldSpace(float2 value) => math.mul(m_Rotation, new float3(value.x, 0, value.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float2 AngleToDirectionLS(float angle) => new float2(math.cos(angle), math.sin(angle));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float DirectionLSToAngle(float2 direction) => math.atan2(direction.y, direction.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float SignedAngle(float2 a, float2 b) => math.atan2(b.y * a.x - b.x * a.y, math.dot(a, b));

        /// <summary>
        /// Interface used to implement draw arc method.
        /// </summary>
        public interface IDrawArc
        {
            void DrawArc(float3 center, float3 normal, float3 from, float3 to, float angle, UnityEngine.Color color);
        }

        /// <summary>
        /// Interface used to implement draw circle method.
        /// </summary>
        public interface IDrawCircle
        {
            void DrawCircle(float3 center, float3 normal, float radius, UnityEngine.Color color);
        }

        /// <summary>
        /// Draws sonar that is not obstructed by obstacle. Use provided action to draw it.
        /// </summary>
        /// <param name="action">Structure used for drawing</param>
        /// <typeparam name="T">The type of draw structure</typeparam>
        public void DrawSonar<T>(T action) where T : unmanaged, IDrawArc
        {
            CheckIsCreated();
            if (m_Nodes.Length == 0)
                return;
            DrawSonarNode(action, Root, m_Position, m_Rotation);
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it.
        /// </summary>
        /// <param name="action">Structure used for drawing</param>
        /// <param name="position">Position of obstacle</param>
        /// <param name="velocity">Velocity of obstacle</param>
        /// <param name="radius">Radius of obstacle</param>
        /// <param name="numIterations">Number of iterations of obstacle collisions</param>
        /// <typeparam name="T">The type of draw structure</typeparam>
        public void DrawObstacleCircle<T>(T action, float3 position, float3 velocity, float radius, int numIterations = 5) where T : unmanaged, IDrawCircle
        {
            var obstaclePositionLS = ToLocalSpace(position - m_Position);
            var obstacleVelocityLS = ToLocalSpace(velocity);

            Circle current = new Circle(0, m_InnerRadius);
            Circle obstacle = new Circle(obstaclePositionLS, radius);

            var circleA = obstacle;
            var circleB = current;

            // Force circles not to be overlapping
            float2 towards = circleA.Point - circleB.Point;
            float length = math.length(towards);
            float r = circleA.Radius + circleB.Radius + 1e-3f;
            if (length < r)
            {
                circleA.Point = circleB.Point + (towards / length) * r;
                length = r;
            }

            obstacle = circleA;

            float3 up = Vector3.up;
            up = math.mul(m_Rotation, up);

            if (Intersection.IntersectionOfTwoMovingCircles(obstacle, obstacleVelocityLS, current, m_Speed, m_OuterRadius + m_InnerRadius, out Line angles, out Line times))
            {
                for (int i = 0; i < numIterations; ++i)
                {
                    float progress = ((float)i / (numIterations-1));
                    float angle = progress * angles.Length + angles.From;
                    float2 direction = new float2(math.cos(angle), math.sin(angle));
                    float2 testVelocity = direction * m_Speed;

                    if (Intersection.IntersectionOfTwoMovingCircles(obstacle, obstacleVelocityLS, current, testVelocity, out float t0, out float t1))
                    {
                        if (t0 >= 0 && math.isfinite(t0))
                        {
                            float3 p0 = ToWorldSpace(current.Point + t0 * testVelocity) + m_Position;
                            action.DrawCircle(p0, up, radius, new Color32(255, 166, 89, 25));

                            float3 p1 = ToWorldSpace(obstacle.Point + t0 * obstacleVelocityLS) + m_Position;
                            action.DrawCircle(p1, up, m_InnerRadius, new Color32(255, 0, 0, 50));
                        }

                        /*if (t1 >= 0 && math.isfinite(t1))
                        {
                            float3 p0 = ToWorldSpace(current.Point + t1 * testVelocity) + m_Position;
                            action.DrawCircle(p0, up, radius, new Color32(255, 166, 89, 25));

                            float3 p1 = ToWorldSpace(obstacle.Point + t1 * obstacleVelocityLS) + m_Position;
                            action.DrawCircle(p1, up, m_InnerRadius, new Color32(255, 0, 0, 50));
                        }*/
                    }
                }
            }
        }

        void DrawSonarNode<T>(T action, SonarNodeHandle handle, float3 position, quaternion rotation) where T : unmanaged, IDrawArc
        {
            var node = m_Nodes[handle];

            if (node.IsLeaf)
            {
                var line = node.Line;

                if (line.Length == 0)
                    return;

                float3 directionFromWS = ToWorldSpace(AngleToDirectionLS(line.From));
                float3 directionToWS = ToWorldSpace(AngleToDirectionLS(line.To));

                float3 up = Vector3.up;
                up = math.mul(rotation, up);

                action.DrawArc(position, up, directionFromWS, directionToWS, line.Length, new UnityEngine.Color(0, 1, 0, 0.2f));
            }
            else
            {
                DrawSonarNode(action, node.Left, position, rotation);
                DrawSonarNode(action, node.Right, position, rotation);
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        internal void CheckIsCreated()
        {
            if (!IsCreated)
                throw new Exception("SonarAvoidance is not initialized. It can happen if was created with argumentless contructor or disposed.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        void CheckHandle(SonarNodeHandle handle)
        {
            if (handle == SonarNodeHandle.Null)
                throw new Exception("SonarAvoidance handle is not valid.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        internal static void CheckArguments(quaternion rotation, float innerRadius, float outerRadius, float angle)
        {
            if (innerRadius < 0)
                throw new ArgumentException("Radius must be non negative", "innerRadius");
            if (outerRadius <= 0)
                throw new ArgumentException("Radius must be greater than zero", "outerRadius");
            if (math.any(!math.isfinite(rotation.value)))
                throw new ArgumentException($"Rotation cannot be zero or Infinite/NaN. ({rotation.value})", "rotation");
            if (angle > math.PI || angle < 0)
                throw new ArgumentException("Angle must be between 0 and pi.", "angle");
        }
    }

    /// <summary>
    /// Sonar avoidance utility class with some helpful functions.
    /// </summary>
    public static class SonarAvoidanceUtility
    {
        /// <summary>
        /// Draws sonar that is not obstructed by obstacle. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawSonar(this SonarAvoidance sonar)
        {
            sonar.DrawSonar(new DrawArcGizmos 
            {
                InnerRadius = sonar.InnerRadius,
                OuterRadius = sonar.OuterRadius,
            });
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawObstacleCircle(this SonarAvoidance sonar, float3 position, float3 velocity, float radius, int numIterations = 5)
        {
            sonar.DrawObstacleCircle(new DrawCircleGizmos(), position, velocity, radius, numIterations);
        }

        /// <summary>
        /// Draws closest desired direction that is not obstructed by obstacle. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawClosestDirection(this SonarAvoidance sonar)
        {
#if UNITY_EDITOR
            sonar.CheckIsCreated();

            if (!sonar.FindClosestDirection(out var direction))
                return;

            float3 position = sonar.Position;

            float3 up = new float3(0, 1, 0);
            up = math.mul(sonar.Rotation, up);

            float3 r = math.cross(up, direction);
            float3 l = -math.cross(up, direction);

            float3 rr = r * sonar.InnerRadius;
            float3 ll = l * sonar.InnerRadius;

            Vector3[] vertices = new Vector3[4];
            vertices[0] = position + rr;
            vertices[1] = position + direction * sonar.OuterRadius + rr;
            vertices[2] = position + direction * sonar.OuterRadius + ll;
            vertices[3] = position + ll;

            UnityEditor.Handles.color = new Color(0, 0, 1, 0.3f);
            UnityEditor.Handles.DrawAAConvexPolygon(vertices);
#endif
        }

        struct DrawArcGizmos : SonarAvoidance.IDrawArc
        {
            public float InnerRadius;
            public float OuterRadius;

            void SonarAvoidance.IDrawArc.DrawArc(float3 position, float3 up, float3 from, float3 to, float angle, UnityEngine.Color color)
            {
#if UNITY_EDITOR
                // Draw outer solid arc
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawSolidArc(position, up, to, math.degrees(angle), OuterRadius);

                // Draw wire for arc
                UnityEditor.Handles.color =  UnityEngine.Color.white;
                UnityEditor.Handles.DrawWireArc(position, up, to, math.degrees(angle), OuterRadius);
                UnityEditor.Handles.DrawLine(position, position + from * OuterRadius);
                UnityEditor.Handles.DrawLine(position, position + to * OuterRadius);

                // Draw inner solid arc
                UnityEditor.Handles.color = new UnityEngine.Color(1, 1, 1, 0.4f);
                UnityEditor.Handles.DrawSolidArc(position, up, to, math.degrees(angle), InnerRadius);
#endif
            }
        }

        struct DrawCircleGizmos : SonarAvoidance.IDrawCircle
        {
            void SonarAvoidance.IDrawCircle.DrawCircle(float3 position, float3 up, float radius, UnityEngine.Color color)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawSolidArc(position, up, Vector3.right, 360, radius);
#endif
            }
        }
    }
}
