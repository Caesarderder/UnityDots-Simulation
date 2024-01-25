using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using static Unity.Entities.SystemAPI;
using static Unity.Mathematics.math;
using ProjectDawn.LocalAvoidance;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst.Intrinsics;

namespace ProjectDawn.Navigation.Editor
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentForceSystemGroup))]
    [UpdateBefore(typeof(AgentSonarAvoidSystem))]
    public partial struct AgentAvoidGizmosSystem : ISystem
    {
        BufferLookup<NavMeshWall> m_NavMeshWallLookup;
        BufferTypeHandle<NavMeshWall> m_NavMeshWallHandle;

        public void OnCreate(ref SystemState state)
        {
            m_NavMeshWallLookup = state.GetBufferLookup<NavMeshWall>(isReadOnly: true);
            m_NavMeshWallHandle = state.GetBufferTypeHandle<NavMeshWall>(isReadOnly: true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingleton<AgentSpatialPartitioningSystem.Singleton>();
            var gizmos = GetSingletonRW<GizmosSystem.Singleton>();

            m_NavMeshWallLookup.Update(ref state);
            m_NavMeshWallHandle.Update(ref state);

            new AgentAvoidJob
            {
                Gizmos = gizmos.ValueRW.CreateCommandBuffer(),
                Spatial = spatial,
                NavMeshWallHandle = m_NavMeshWallHandle,
                NavMeshWallLookup = m_NavMeshWallLookup,
            }.Schedule();
        }

        [BurstCompile]
        unsafe partial struct AgentAvoidJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [ReadOnly]
            public AgentSpatialPartitioningSystem.Singleton Spatial;

            [NativeDisableContainerSafetyRestriction]
            SonarAvoidance Sonar;

            [ReadOnly]
            public BufferTypeHandle<NavMeshWall> NavMeshWallHandle;
            [ReadOnly]
            public BufferLookup<NavMeshWall> NavMeshWallLookup;
            bool HasNavMeshWall;

            public GizmosCommandBuffer Gizmos;

            public void Execute(Entity entity, in AgentBody body, in AgentShape shape, in AgentSonarAvoid avoid, in LocalTransform transform, in DrawGizmos drawGizmos)
            {
                if (body.IsStopped)
                    return;

                float3 desiredDirection = body.Force;

                if (!SonarAvoidance.TryDirectionToRotation(desiredDirection, shape.GetUp(), out var rotation))
                    return;

                UnityEngine.Debug.DrawLine(transform.Position, transform.Position + desiredDirection, UnityEngine.Color.black);

#if EXPERIMENTAL_SONAR_TIME
                float sonarRadius = clamp(length(body.Velocity) * 0.9f, 0.1f, min(distance(body.Destination, transform.Position), avoid.Radius));
#else
                float sonarRadius = min(distance(body.Destination, transform.Position), avoid.Radius);
#endif

                // Recreate avoidance structure
                Sonar.Set(transform.Position, body.Velocity, rotation, shape.Radius, sonarRadius, avoid.MaxAngle * 0.5f);

                // Add blocker behind the velocity
                // This will prevent situations where agent has on right and left equally good paths
                if (math.length(body.Velocity) > 1e-3f)
                    Sonar.InsertObstacle(math.normalizesafe(-body.Velocity), avoid.Angle);

                // Add nearby agents as obstacles
                var action = new Action
                {
                    DrawCircle = new DrawCircle { Gizmos = Gizmos },
                    Sonar = Sonar,
                    Entity = entity,
                    Body = body,
                    Shape = shape,
                    Avoid = avoid,
                    Transform = transform,
                    DesiredDirection = desiredDirection,
                };

#if EXPERIMENTAL_SONAR_TIME
                action.MaxRadius = Sonar.OuterRadius + shape.Radius - 1e-3f;
                action.MaxCostAngle = cos(avoid.MaxAngle * 0.5f);
#endif

                if (shape.Type == ShapeType.Cylinder)
                {
                    Spatial.QueryCylinder(transform.Position, sonarRadius, shape.Height, ref action, avoid.Layers);

                    if (HasNavMeshWall)
                    {
                        var walls = NavMeshWallLookup[entity];
                        // Add navmesh walls
                        for (int i = 0; i < walls.Length; ++i)
                        {
                            NavMeshWall wall = walls[i];

                            float extent = shape.Height * 0.5f;
                            float wallExtent = extent; // TODO: Use from baked navmesh agent id
                            if (abs((transform.Position.y + extent) - (wall.Start.y + wallExtent)) > extent + wallExtent)
                                continue;
                            if (abs((transform.Position.y + extent) - (wall.End.y + wallExtent)) > extent + wallExtent)
                                continue;

                            if (!Sonar.InsertObstacleLineIgnoreRadius(wall.Start, wall.End))
                                continue;

                            DrawWall(wall, shape.Height, new Color(1, 0, 0, 0.75f));
                        }
                    }
                }
                else
                {
                    Spatial.QuerySphere(transform.Position, sonarRadius, ref action, avoid.Layers);
                }

                Sonar.DrawSonar(new DrawArc
                {
                    Gizmos = Gizmos,
                    InnerRadius = Sonar.InnerRadius,
                    OuterRadius = Sonar.OuterRadius,
                });

                bool success = Sonar.FindClosestDirection(out float3 newDirection);

                // If blocked stop enabled, reset to previous direction
                if (!avoid.BlockedStop && !success)
                    newDirection = desiredDirection;

                UnityEngine.Debug.DrawLine(transform.Position, transform.Position + desiredDirection, UnityEngine.Color.white);
                UnityEngine.Debug.DrawLine(transform.Position, transform.Position + newDirection, UnityEngine.Color.red);
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Sonar = new SonarAvoidance(Allocator.Temp);
                if (chunk.Has<NavMeshWall>())
                {
                    HasNavMeshWall = true;
                }
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            {
                HasNavMeshWall = false;
                Sonar.Dispose();
            }

            void DrawWall(NavMeshWall wall, float height, Color color)
            {
                float3 offset = new float3(0, height, 0);

                Gizmos.DrawQuad(wall.Start, wall.End, wall.End + offset, wall.Start + offset, color, true);
                Gizmos.DrawLine(wall.Start, wall.End, UnityEngine.Color.red);
            }

            struct Action : ISpatialQueryEntity
            {
                public DrawCircle DrawCircle;

                public SonarAvoidance Sonar;
                public Entity Entity;
                public AgentBody Body;
                public AgentShape Shape;
                public AgentSonarAvoid Avoid;
                public LocalTransform Transform;
                public float3 DesiredDirection;
#if EXPERIMENTAL_SONAR_TIME
                public float MaxRadius;
                public float MaxCostAngle;
#endif

                public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
                {
                    // Skip itself
                    if (Entity == otherEntity)
                        return;

#if EXPERIMENTAL_SONAR_TIME
                    float3 directionToOther = otherTransform.Position - Transform.Position;

                    // Skip agent outside sonar radius
                    float distance = length(directionToOther);
                    if (distance > MaxRadius + otherShape.Radius)
                        return;

                    // Skip moving agent that is outside the visibility cone
                    if (!otherBody.IsStopped && distance > EPSILON)
                    {
                        float cosAngle = dot(DesiredDirection, directionToOther / distance);
                        if (cosAngle < cos(Avoid.MaxAngle * 0.5f))
                            return;
                    }
#else
                    if (math.dot(DesiredDirection, math.normalizesafe(otherTransform.Position - Transform.Position)) < 0 && math.length(otherBody.Velocity) > 0)
                        return;
#endif

                    if (Shape.Type == ShapeType.Cylinder && otherShape.Type == ShapeType.Cylinder)
                    {
                        float extent = Shape.Height * 0.5f;
                        float otherExtent = otherShape.Height * 0.5f;
                        if (math.abs((Transform.Position.y + extent) - (otherTransform.Position.y + otherExtent)) > extent + otherExtent)
                            return;
                    }

                    Sonar.InsertObstacleCircle(otherTransform.Position, otherBody.Velocity, otherShape.Radius);

                    Sonar.DrawObstacleCircle(DrawCircle, otherTransform.Position, otherBody.Velocity, otherShape.Radius);
                }
            }

            struct DrawArc : SonarAvoidance.IDrawArc
            {
                public GizmosCommandBuffer Gizmos;
                public float InnerRadius;
                public float OuterRadius;
                void SonarAvoidance.IDrawArc.DrawArc(float3 position, float3 up, float3 from, float3 to, float angle, UnityEngine.Color color)
                {
                    Gizmos.DrawSolidArc(position, up, to, math.degrees(angle), OuterRadius, color);
                    Gizmos.DrawWireArc(position, up, to, math.degrees(angle), OuterRadius, UnityEngine.Color.white);
                    Gizmos.DrawLine(position, position + from * OuterRadius, UnityEngine.Color.white);
                    Gizmos.DrawLine(position, position + to * OuterRadius, UnityEngine.Color.white);

                    Gizmos.DrawSolidArc(position, up, to, math.degrees(angle), InnerRadius, new UnityEngine.Color(1, 1, 1, 0.4f));
                }
            }

            struct DrawCircle : SonarAvoidance.IDrawCircle
            {
                public GizmosCommandBuffer Gizmos;

                void SonarAvoidance.IDrawCircle.DrawCircle(float3 center, float3 up, float radius, Color color)
                {
                    Gizmos.DrawSolidDisc(center, up, radius, color);
                }
            }
        }
    }
}
