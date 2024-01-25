using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using UnityEngine.Experimental.AI;
using Unity.Collections.LowLevel.Unsafe;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that for each agent that has sonar with wall collects nearby navmesh edges as walls.
    /// Later on these walls are used for avoidance.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentPathingSystemGroup))]
    public partial struct NavMeshBoundarySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var navmesh = GetSingleton<NavMeshQuerySystem.Singleton>();
            new NavMeshBoundaryJob
            {
                NavMesh = navmesh,
            }.ScheduleParallel();
            navmesh.World.AddDependency(state.Dependency);
        }

        [BurstCompile]
        unsafe partial struct NavMeshBoundaryJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [ReadOnly]
            public NavMeshQuerySystem.Singleton NavMesh;

            [NativeDisableContainerSafetyRestriction]
            NativeQueue<PolygonId> NodesToVisit;
            [NativeDisableContainerSafetyRestriction]
            NativeHashSet<PolygonId> VisitedNodes;

            [NativeDisableContainerSafetyRestriction]
            NativeArray<float3> NodeVertices;
            [NativeDisableContainerSafetyRestriction]
            NativeArray<PolygonId> NodeNeighbours;
            [NativeDisableContainerSafetyRestriction]
            NativeArray<byte> NodeEdges;

            public void Execute(in NavMeshPath path, ref NavMeshBoundary boundary, ref DynamicBuffer<NavMeshWall> walls, in LocalTransform transform)
            {
                // Avoid updating to frequently to save performance
                float updateThr = 1;
                if (path.Location.polygon == boundary.Location.polygon && math.distancesq(path.Location.position, boundary.Location.position) < updateThr)
                    return;
                boundary.Location = path.Location;
                walls.Clear();

                NodesToVisit.Clear();
                VisitedNodes.Clear();

                NodesToVisit.Enqueue(boundary.Location.polygon);
                VisitedNodes.Add(boundary.Location.polygon);

                float radius = boundary.Radius + updateThr;
                float radiussq = radius * radius;

                int count = 0;
                while (NodesToVisit.TryDequeue(out PolygonId polygon))
                {
                    if (!NavMesh.GetEdgesAndNeighbors(polygon, NodeVertices.Reinterpret<UnityEngine.Vector3>(), NodeNeighbours, NodeEdges, out int numVertices, out int numNodes))
                        continue;

                    // Check if node is within the range
                    // First one is always included
                    if (!Overlap(NodeVertices.Slice(0, numVertices), transform.Position, radiussq) && count != 0)
                        continue;

                    // Add neighbouring nodes and also set mask if edge is portal
                    bool* portals = stackalloc bool[numVertices];
                    UnsafeUtility.MemClear(portals, sizeof(bool) * numVertices);
                    for (int i = 0; i < numNodes; ++i)
                    {
                        PolygonId node = NodeNeighbours[i];
                        byte edge = NodeEdges[i];

                        portals[edge] = true;

                        if (VisitedNodes.Contains(node))
                            continue;

                        NodesToVisit.Enqueue(node);
                        VisitedNodes.Add(node);
                    }

                    // Go through each edge and check if it is wall
                    for (int i = 0; i < numVertices; i++)
                    {
                        if (portals[i])
                            continue;

                        float3 from = NodeVertices[i];
                        float3 to = NodeVertices[(i + 1) % numVertices];

                        walls.Add(new NavMeshWall(from, to));
                    }

                    count++;
                }
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NodesToVisit = new NativeQueue<PolygonId>(Allocator.Temp);
                VisitedNodes = new NativeHashSet<PolygonId>(16, Allocator.Temp);

                // Polygonal nodes of the NavMesh have a minimum of 3 and a maximum of 6 vertices.
                NodeVertices = new NativeArray<float3>(6, Allocator.Temp);
                NodeNeighbours = new NativeArray<PolygonId>(6, Allocator.Temp);
                NodeEdges = new NativeArray<byte>(6, Allocator.Temp);

                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            {
                NodeVertices.Dispose();
                NodeNeighbours.Dispose();

                NodesToVisit.Dispose();
                VisitedNodes.Dispose();
                NodeEdges.Dispose();
            }

            static bool Overlap(NativeSlice<float3> vertices, float3 position, float radiussq)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    float3 from = vertices[i];
                    float3 to = vertices[(i + 1) % vertices.Length];

                    float3 closestPoint = GetClosestPointToSegment(from, to, position);
                    if (math.distancesq(closestPoint, position) < radiussq)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Returns a point on the perimeter of this rectangle that is closest to the specified point.
            /// </summary>
            static float3 GetClosestPointToSegment(float3 segmentStart, float3 segmentEnd, float3 point)
            {
                float3 towards = segmentEnd - segmentStart;

                float lengthSq = math.lengthsq(towards);
                if (lengthSq < math.EPSILON)
                    return point;

                float t = math.dot(point - segmentStart, towards) / lengthSq;

                // Force within the segment
                t = math.saturate(t);

                return segmentStart + t * towards;
            }
        }
    }
}
