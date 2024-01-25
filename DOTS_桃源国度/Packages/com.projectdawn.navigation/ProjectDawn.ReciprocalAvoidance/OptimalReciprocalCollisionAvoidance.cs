using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.LocalAvoidance.RVO
{
    /// <summary>
    /// Optimal Reciprocal Collision Avoidance based on https://github.com/snape/RVO2.
    /// This is experimental and currently recommended not to be used.
    /// </summary>
    public struct OptimalReciprocalCollisionAvoidance : IDisposable
    {
        float2 m_Position;
        float2 m_Velocity;
        float2 m_DesiredVelocity;
        float m_MaxSpeed;
        float m_Radius;
        float m_NeighborDist;
        float m_TimeHorizonAgents;
        float m_TimeHorizonObstacles;
        int m_MaxNeighbors;

        NativeList<KeyValuePair<float, Agent>> m_Agents;
        NativeList<ParametricLine> m_Lines;

        public OptimalReciprocalCollisionAvoidance(float2 position, float2 velocity, float2 prefVelocity, int maxNeighbors, float maxSpeed, float neighborDist, float radius, float timeHorizon, float timeHorizonObst)
        {
            m_Position = position;
            m_Velocity = velocity;
            m_DesiredVelocity = prefVelocity;
            m_MaxSpeed = maxSpeed;
            m_Radius = radius;
            m_NeighborDist = neighborDist;
            m_TimeHorizonAgents = timeHorizon;
            m_TimeHorizonObstacles = timeHorizonObst;
            m_MaxNeighbors = maxNeighbors;
            m_Lines = new NativeList<ParametricLine>(maxNeighbors, Allocator.Temp);
            m_Agents = new NativeList<KeyValuePair<float, Agent>>(maxNeighbors, Allocator.Temp);
        }

        public void InsertObstacle(float2 position, float2 velocity, float radius)
        {
            var agent = new Agent()
            {
                Position = position,
                Velocity = velocity,
                Radius = radius,
            };
            float distSq = math.lengthsq(m_Position - position);

            if (distSq < m_NeighborDist*m_NeighborDist)
            {
                if (m_Agents.Length < m_MaxNeighbors)
                {
                    m_Agents.Add(new KeyValuePair<float, Agent>(distSq, agent));
                }

                int i = m_Agents.Length - 1;

                while (i != 0 && distSq < m_Agents[i - 1].Key)
                {
                    m_Agents[i] = m_Agents[i - 1];
                    --i;
                }

                m_Agents[i] = new KeyValuePair<float, Agent>(distSq, agent);
            }
        }

        public void GetDesiredVelocity(float deltaTime, out float2 velocity)
        {
            velocity = 0;

            int numObstLines = m_Lines.Length;

            float invTimeHorizon = 1.0f / m_TimeHorizonAgents;

            /* Create agent ORCA lines. */
            for (int i = 0; i < m_Agents.Length; ++i)
            {
                Agent other = m_Agents[i].Value;
                ParametricLine line = CreateLine(other, invTimeHorizon, deltaTime);
                m_Lines.Add(line);
            }

            int lineFail = linearProgram2(m_Lines, m_MaxSpeed, m_DesiredVelocity, false, ref velocity);

            if (lineFail < m_Lines.Length)
            {
                linearProgram3(m_Lines, numObstLines, lineFail, m_MaxSpeed, ref velocity);
            }
        }

        ParametricLine CreateLine(Agent other, float invTimeHorizon, float deltaTime)
        {
            float2 relativePosition = other.Position - m_Position;
            float2 relativeVelocity = m_Velocity - other.Velocity;
            float distSq = math.lengthsq(relativePosition);
            float combinedRadius = m_Radius + other.Radius;
            float combinedRadiusSq = math.lengthsq(combinedRadius);

            ParametricLine line;
            float2 u;

            if (distSq > combinedRadiusSq)
            {
                /* No collision. */
                float2 w = relativeVelocity - invTimeHorizon * relativePosition;

                /* Vector from cutoff center to relative velocity. */
                float wLengthSq = math.lengthsq(w);
                float dotProduct1 = math.dot(w, relativePosition);

                if (dotProduct1 < 0.0f && math.lengthsq(dotProduct1) > combinedRadiusSq * wLengthSq)
                {
                    /* Project on cut-off circle. */
                    float wLength = math.sqrt(wLengthSq);
                    float2 unitW = w / wLength;

                    line.Direction = new float2(unitW.y, -unitW.x);
                    u = (combinedRadius * invTimeHorizon - wLength) * unitW;
                }
                else
                {
                    /* Project on legs. */
                    float leg = math.sqrt(distSq - combinedRadiusSq);

                    if (Determinant(relativePosition, w) > 0.0f)
                    {
                        /* Project on left leg. */
                        line.Direction = new float2(relativePosition.x * leg - relativePosition.y * combinedRadius, relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                    }
                    else
                    {
                        /* Project on right leg. */
                        line.Direction = -new float2(relativePosition.x * leg + relativePosition.y * combinedRadius, -relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                    }

                    float dotProduct2 = math.dot(relativeVelocity, line.Direction);
                    u = dotProduct2 * line.Direction - relativeVelocity;
                }
            }
            else
            {
                /* Collision. Project on cut-off circle of time timeStep. */
                float invTimeStep = 1.0f / deltaTime;

                /* Vector from cutoff center to relative velocity. */
                float2 w = relativeVelocity - invTimeStep * relativePosition;

                float wLength = math.length(w);
                float2 unitW = w / wLength;

                line.Direction = new float2(unitW.y, -unitW.x);
                u = (combinedRadius * invTimeStep - wLength) * unitW;
            }

            line.Origin = m_Velocity + 0.5f * u;
            return line;
        }

        public void Dispose()
        {
            m_Lines.Dispose();
            m_Agents.Dispose();
        }

        struct Agent
        {
            public float2 Position;
            public float2 Velocity;
            public float Radius;
        }

        /**
         * <summary>Solves a one-dimensional linear program on a specified line
         * subject to linear constraints defined by lines and a circular
         * constraint.</summary>
         *
         * <returns>True if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="lineNo">The specified line constraint.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        static bool linearProgram1(NativeList<ParametricLine> lines, int lineNo, float radius, float2 optVelocity, bool directionOpt, ref float2 result)
        {
            float dotProduct = math.dot(lines[lineNo].Origin, lines[lineNo].Direction);
            float discriminant = math.lengthsq(dotProduct) + math.lengthsq(radius) - math.lengthsq(lines[lineNo].Origin);

            if (discriminant < 0.0f)
            {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            float sqrtDiscriminant = math.sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {
                float denominator = Determinant(lines[lineNo].Direction, lines[i].Direction);
                float numerator = Determinant(lines[i].Direction, lines[lineNo].Origin - lines[i].Origin);

                if (math.abs(denominator) <= math.EPSILON)
                {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < 0.0f)
                    {
                        return false;
                    }

                    continue;
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                {
                    /* ParametricLine i bounds line lineNo on the right. */
                    tRight = math.min(tRight, t);
                }
                else
                {
                    /* ParametricLine i bounds line lineNo on the left. */
                    tLeft = math.max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (math.dot(optVelocity, lines[lineNo].Direction) > 0.0f)
                {
                    /* Take right extreme. */
                    result = lines[lineNo].Origin + tRight * lines[lineNo].Direction;
                }
                else
                {
                    /* Take left extreme. */
                    result = lines[lineNo].Origin + tLeft * lines[lineNo].Direction;
                }
            }
            else
            {
                /* Optimize closest point. */
                float t = math.dot(lines[lineNo].Direction, (optVelocity - lines[lineNo].Origin));

                if (t < tLeft)
                {
                    result = lines[lineNo].Origin + tLeft * lines[lineNo].Direction;
                }
                else if (t > tRight)
                {
                    result = lines[lineNo].Origin + tRight * lines[lineNo].Direction;
                }
                else
                {
                    result = lines[lineNo].Origin + t * lines[lineNo].Direction;
                }
            }

            return true;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <returns>The number of the line it fails on, and the number of lines
         * if successful.</returns>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="optVelocity">The optimization velocity.</param>
         * <param name="directionOpt">True if the direction should be optimized.
         * </param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        static int linearProgram2(NativeList<ParametricLine> lines, float radius, float2 optVelocity, bool directionOpt, ref float2 result)
        {
            if (directionOpt)
            {
                /*
                 * Optimize direction. Note that the optimization velocity is of
                 * unit length in this case.
                 */
                result = optVelocity * radius;
            }
            else if (math.lengthsq(optVelocity) > math.lengthsq(radius))
            {
                /* Optimize closest point and outside circle. */
                result = math.normalize(optVelocity) * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                if (Determinant(lines[i].Direction, lines[i].Origin - result) > 0.0f)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    float2 tempResult = result;
                    if (!linearProgram1(lines, i, radius, optVelocity, directionOpt, ref result))
                    {
                        result = tempResult;

                        return i;
                    }
                }
            }

            return lines.Length;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="numObstLines">Count of obstacle lines.</param>
         * <param name="beginLine">The line on which the 2-d linear program
         * failed.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        static void linearProgram3(NativeList<ParametricLine> lines, int numObstLines, int beginLine, float radius, ref float2 result)
        {
            float distance = 0.0f;

            for (int i = beginLine; i < lines.Length; ++i)
            {
                if (Determinant(lines[i].Direction, lines[i].Origin - result) > distance)
                {
                    /* Result does not satisfy constraint of line i. */
                    NativeList<ParametricLine> projLines = new NativeList<ParametricLine>(numObstLines*2, Allocator.Temp);
                    for (int ii = 0; ii < numObstLines; ++ii)
                    {
                        projLines.Add(lines[ii]);
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {
                        ParametricLine line;

                        float determinant = Determinant(lines[i].Direction, lines[j].Direction);

                        if (math.abs(determinant) <= math.EPSILON)
                        {
                            /* ParametricLine i and line j are parallel. */
                            if (math.dot(lines[i].Direction, lines[j].Direction) > 0.0f)
                            {
                                /* ParametricLine i and line j point in the same direction. */
                                continue;
                            }
                            else
                            {
                                /* ParametricLine i and line j point in opposite direction. */
                                line.Origin = 0.5f * (lines[i].Origin + lines[j].Origin);
                            }
                        }
                        else
                        {
                            line.Origin = lines[i].Origin + (Determinant(lines[j].Direction, lines[i].Origin - lines[j].Origin) / determinant) * lines[i].Direction;
                        }

                        line.Direction = math.normalize(lines[j].Direction - lines[i].Direction);
                        //UnityEngine.Debug.Log($"line.Direction = {line.Direction}");
                        //UnityEngine.Debug.Log($"line.origin = {line.Origin}");
                        projLines.Add(line);
                    }

                    float2 tempResult = result;
                    if (linearProgram2(projLines, radius, new float2(-lines[i].Direction.y, lines[i].Direction.x), true, ref result) < projLines.Length)
                    {
                        /*
                         * This should in principle not happen. The result is by
                         * definition already in the feasible region of this
                         * linear program. If it fails, it is due to small
                         * floating point error, and the current result is kept.
                         */
                        result = tempResult;
                    }
                    projLines.Dispose();

                    distance = Determinant(lines[i].Direction, lines[i].Origin - result);
                }
            }
        }

        static float Determinant(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }
}
