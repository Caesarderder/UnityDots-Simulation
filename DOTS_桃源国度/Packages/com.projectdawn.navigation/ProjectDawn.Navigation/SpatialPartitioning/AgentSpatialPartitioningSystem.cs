using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;
using static Unity.Mathematics.math;

namespace ProjectDawn.Navigation
{
    [System.Serializable]
    public class SpatialPartitioningSubSettings : ISubSettings
    {
        [SerializeField]
        [Tooltip("The size of single cell.")]
        float3 m_CellSize = 3;

        [SerializeField]
        [Tooltip("The maximum number of nearby neighbors will be checked to find closest.")]
        QueryCheckMode m_QueryCheck = QueryCheckMode._32;

        [SerializeField]
        [Range(0, 16), Tooltip("The maximum number of nearby neighbors to be included in the avoidance/collision systems will be determined.")]
        int m_QueryCapacity = 0;

        [SerializeField]
        NavigationLayerNames m_Layers = new();

        /// <summary>
        /// The size of single cell.
        /// </summary>
        public float3 CellSize => m_CellSize;
        /// <summary>
        /// The maximum number of nearby neighbors to be included in the avoidance/collision systems will be determined.
        /// </summary>
        public int QueryCapacity => m_QueryCapacity;
        /// <summary>
        /// The maximum number of nearby neighbors will be checked to find closest.
        /// </summary>
        public int QueryChecks => (int)m_QueryCheck;

        public string[] LayerNames => m_Layers.Names;

        public enum QueryCheckMode : int
        {
            _16 = 16,
            _32 = 32,
        }
    }

    /// <summary>
    /// Partitions agents into arbitary size cells. This allows to query nearby agents more efficiently.
    /// Space is partitioned using multi hash map.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentSpatialSystemGroup))]
    public partial struct AgentSpatialPartitioningSystem : ISystem
    {
        internal const int InitialCapacity = 256;
        internal const int MaxCellsPerUnit = 16;

        NativeParallelMultiHashMap<int3, int> m_Map;
        NativeList<Agent> m_Agents;
        NativeList<Entity> m_Entities;
        NativeList<AgentBody> m_Bodies;
        NativeList<AgentShape> m_Shapes;
        NativeList<LocalTransform> m_Transforms;
        int m_Capacity;
        float3 m_CellSize;
        int m_QueryCapacity;
        EntityQuery m_Query;

        bool UseLimitedQuery => m_QueryCapacity > 0;

        internal JobHandle ScheduleUpdate(ref SystemState state, JobHandle dependency)
        {
            int count = m_Query.CalculateEntityCount();

            if (count > m_Capacity)
            {
                // Calculate new capacity size
                var newCapacity = max(count, 64);
                newCapacity = ceilpow2(newCapacity);
                m_Capacity = newCapacity;

                dependency = new ChangeCapacityJob
                {
                    Map = m_Map,
                    Entities = m_Entities,
                    Agents = m_Agents,
                    Bodies = m_Bodies,
                    Shapes = m_Shapes,
                    Transforms = m_Transforms,
                    MapCapacity = UseLimitedQuery ? m_Capacity * MaxCellsPerUnit : m_Capacity,
                    Capacity = m_Capacity,
                }.Schedule(dependency);
            }

            dependency = new ClearJob
            {
                Entities = m_Entities,
                Agents = m_Agents,
                Bodies = m_Bodies,
                Shapes = m_Shapes,
                Transforms = m_Transforms,
                Map = m_Map,
            }.Schedule(dependency);

            var copyHandle = new CopyJob
            {
                Entities = m_Entities,
                Agents = m_Agents,
                Bodies = m_Bodies,
                Shapes = m_Shapes,
                Transforms = m_Transforms,
            }.Schedule(dependency);

            JobHandle hashHandle;
            if (m_QueryCapacity == 0)
            {
                hashHandle = new HashJob
                {
                    Map = m_Map.AsParallelWriter(),
                    CellSize = m_CellSize,
                }.ScheduleParallel(dependency);
            }
            else
            {
                hashHandle = new HashShapeJob
                {
                    Map = m_Map.AsParallelWriter(),
                    CellSize = m_CellSize,
                }.ScheduleParallel(dependency);
            }

            return JobHandle.CombineDependencies(copyHandle, hashHandle);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            GetSingletonRW<Singleton>();
            state.Dependency = ScheduleUpdate(ref state, state.Dependency);
        }

       // [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_Capacity = InitialCapacity;
            m_CellSize = AgentsNavigationSettings.Get<SpatialPartitioningSubSettings>().CellSize;
            m_QueryCapacity = AgentsNavigationSettings.Get<SpatialPartitioningSubSettings>().QueryCapacity;

            m_Map = new NativeParallelMultiHashMap<int3, int>(UseLimitedQuery ? InitialCapacity * MaxCellsPerUnit : InitialCapacity, Allocator.Persistent);
            m_Entities = new NativeList<Entity>(InitialCapacity, Allocator.Persistent);
            m_Agents = new NativeList<Agent>(InitialCapacity, Allocator.Persistent);
            m_Bodies = new NativeList<AgentBody>(InitialCapacity, Allocator.Persistent);
            m_Shapes = new NativeList<AgentShape>(InitialCapacity, Allocator.Persistent);
            m_Transforms = new NativeList<LocalTransform>(InitialCapacity, Allocator.Persistent);

            m_Query = QueryBuilder()
                .WithAll<Agent>()
                .WithAll<AgentBody>()
                .WithAll<AgentShape>()
                .WithAll<LocalTransform>()
                .Build();

            state.EntityManager.AddComponentData(state.SystemHandle, new Singleton
            {
                m_Map = m_Map,
                m_Entities = m_Entities,
                m_Agents = m_Agents,
                m_Bodies = m_Bodies,
                m_Shapes = m_Shapes,
                m_Transforms = m_Transforms,
                m_Capacity = m_Capacity,
                m_CellSize = m_CellSize,
                m_QueryCapacity = m_QueryCapacity,
                m_QueryChecks = AgentsNavigationSettings.Get<SpatialPartitioningSubSettings>().QueryChecks,
            });
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState systemState)
        {
            m_Map.Dispose();
            m_Entities.Dispose();
            m_Agents.Dispose();
            m_Bodies.Dispose();
            m_Shapes.Dispose();
            m_Transforms.Dispose();
        }

        [System.Obsolete("This class is obsolete, please use new settings workflow https://lukaschod.github.io/agents-navigation-docs/manual/settings.html.")]
        public struct Settings : IComponentData
        {
            public float3 CellSize;
            public int QueryCapacity;
        }

        public struct Singleton : IComponentData
        {
            internal NativeParallelMultiHashMap<int3, int> m_Map;
            internal NativeList<Entity> m_Entities;
            internal NativeList<Agent> m_Agents;
            internal NativeList<AgentBody> m_Bodies;
            internal NativeList<AgentShape> m_Shapes;
            internal NativeList<LocalTransform> m_Transforms;
            internal int m_Capacity;
            internal float3 m_CellSize;
            internal int m_QueryCapacity;
            internal int m_QueryChecks;

            public int QueryCapacity => m_QueryCapacity;

            /// <summary>
            /// Query agents that intersect with the line.
            /// </summary>
            public int QueryLine<T>(float3 from, float3 to, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                int count = 0;

                // Based on http://www.cse.yorku.ca/~amana/research/grid.pdf

                // Convert to unit voxel size
                from = from / m_CellSize;
                to = to / m_CellSize;

                // Convert to parametric line form: u + v * t, t >= 0
                float3 u = from;
                float3 v = to - from;

                // Find start and end voxel coordinates
                int3 point = (int3) round(from);
                int3 end = (int3) round(to);

                // Initialized to either 1 or - 1 indicating whether X and Y are incremented or decremented as the
                // ray crosses voxel boundaries(this is determined by the sign of the x and y components of → v).
                int3 step = (int3) sign(v);

                float3 boundaryDistance = select(-0.5f, 0.5f, step == 1);

                // Here we find distance to closest voxel boundary on each axis
                // Formula is actually quite simple we just equate parametric line to cloest voxel boundary
                // u + v * t = start + boundaryDistance, step = 1
                // u + v * t = start - boundaryDistance, step = -1
                float3 tMax = select((point + boundaryDistance - u) / v, float.MaxValue, step == 0);

                // TDelta indicates how far along the ray we must move
                // (in units of t) for the horizontal component of such a movement to equal the width of a voxel.
                // Similarly, we store in tDeltaY the amount of movement along the ray which has a vertical component equal to the height of a voxel.
                float3 tDelta = select(abs(1f / v), float.MaxValue, step == 0);

                // Loop through each voxel
                for (int i = 0; i < 100; ++i)
                {
                    // Find all entities in the bucket
                    if (m_Map.TryGetFirstValue(point, out int index, out var iterator))
                    {
                        do
                        {
                            if (!layers.Any(m_Agents[index].Layers))
                                continue;
                            action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                            count++;
                        }
                        while (m_Map.TryGetNextValue(out index, ref iterator));
                    }

                    // Stop if reached the end voxel
                    if (all(point == end))
                        break;

                    // Progress line towards the voxel that will be reached fastest
                    if (tMax.x < tMax.y)
                    {
                        if (tMax.x < tMax.z)
                        {
                            tMax.x = tMax.x + tDelta.x;
                            point.x = point.x + step.x;
                        }
                        else
                        {
                            tMax.z = tMax.z + tDelta.z;
                            point.z = point.z + step.z;
                        }
                    }
                    else
                    {
                        if (tMax.y < tMax.z)
                        {
                            tMax.y = tMax.y + tDelta.y;
                            point.y = point.y + step.y;
                        }
                        else
                        {
                            tMax.z = tMax.z + tDelta.z;
                            point.z = point.z + step.z;
                        }
                    }
                }

                return count;
            }

            /// <summary>
            /// Query agents that intersect with the sphere.
            /// </summary>
            public int QuerySphere<T>(float3 center, float radius, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                int count = 0;

                // Find min and max point in radius
                int3 min = (int3) math.round((center - radius) / m_CellSize);
                int3 max = (int3) math.round((center + radius) / m_CellSize) + 1;

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        for (int k = min.z; k < max.z; ++k)
                        {
                            // Find all entities in the bucket
                            if (m_Map.TryGetFirstValue(new int3(i, j, k), out int index, out var iterator))
                            {
                                do
                                {
                                    if (!layers.Any(m_Agents[index].Layers))
                                        continue;
                                    action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                                    count++;
                                }
                                while (m_Map.TryGetNextValue(out index, ref iterator));
                            }
                        }
                    }
                }

                return count;
            }

            /// <summary>
            /// Query agents that intersect with the sphere.
            /// </summary>
            public int QueryCircle<T>(float3 center, float radius, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                int count = 0;

                // Find min and max point in radius
                int2 min = (int2) round((center.xy - radius) / m_CellSize.xy);
                int2 max = (int2) round((center.xy + radius) / m_CellSize.xy) + 1;
                int k = (int) round(center.z / m_CellSize.z);

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        // Find all entities in the bucket
                        if (m_Map.TryGetFirstValue(new int3(i, j, k), out int index, out var iterator))
                        {
                            do
                            {
                                if (!layers.Any(m_Agents[index].Layers))
                                    continue;
                                action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                                count++;
                            }
                            while (m_Map.TryGetNextValue(out index, ref iterator));
                        }
                    }
                }

                return count;
            }

            /// <summary>
            /// Query agents that intersect with the circle.
            /// </summary>
            public int QueryCircle<T>(float3 center, float radius, int maxCount, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                if (maxCount == 0)
                    return QueryCircle(center, radius, ref action, layers);

                var entries = new FixedEntries(center.xy, -1, float.MaxValue, maxCount);

                var map = m_Map;

                float2 cellSize = m_CellSize.xy;
                int2 min = (int2) round((center.xy - radius) / cellSize);
                int2 max = (int2) round((center.xy + radius) / cellSize);
                int z = (int) round(center.z / m_CellSize.z);

                int2 size = max - min + 1;
                int2 halfSize = size / 2;
                int maxSide = math.max(size.x, size.y);
                int maxIterations = maxSide * maxSide;

                int2 point = 0;
                int2 offset = min + (maxSide - 1) / 2;
                int2 step = new int2(0, -1);

                int count = 0;

                for (int i = 0; i < maxIterations; i++)
                {
                    // Check if current nodes is within the rectangle we want
                    if (math.all(-halfSize <= point) && math.all(point <= halfSize))
                    {
                        // Find all entities in the bucket
                        if (map.TryGetFirstValue(new int3(point + offset, z), out int index, out var iterator))
                        {
                            do
                            {
                                if (!layers.Any(m_Agents[index].Layers))
                                    continue;
                                if (entries.Add(index, m_Transforms[index].Position.xy))
                                {
                                    count++;
                                    if (count == m_QueryChecks)
                                        goto FULL;
                                }
                            }
                            while (map.TryGetNextValue(out index, ref iterator));
                        }
                    }
                    // Check if we need to change spiral direction
                    if ((point.x == point.y) || ((point.x < 0) && (point.x == -point.y)) || ((point.x > 0) && (point.x == 1 - point.y)))
                    {
                        // Swap and negate x
                        int temp = step.x;
                        step.x = -step.y;
                        step.y = temp;
                    }
                    // Step
                    point += step;
                }

                FULL:

                int entry = 0;
                while (true)
                {
                    var index = entries[entry++];

                    if (index == -1)
                        break;

                    action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                }

                return entry;
            }

            /// <summary>
            /// Query agents that intersect with the cylinder.
            /// </summary>
            public int QueryCylinder<T>(float3 center, float radius, float height, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                int count = 0;

                // Find min and max point in radius
                int3 min = (int3) math.round((center - new float3(radius, 0, radius)) / m_CellSize);
                int3 max = (int3) math.round((center + new float3(radius, height, radius)) / m_CellSize) + 1;

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        for (int k = min.z; k < max.z; ++k)
                        {
                            // Find all entities in the bucket
                            if (m_Map.TryGetFirstValue(new int3(i, j, k), out int index, out var iterator))
                            {
                                do
                                {
                                    if (!layers.Any(m_Agents[index].Layers))
                                        continue;
                                    action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                                    count++;
                                }
                                while (m_Map.TryGetNextValue(out index, ref iterator));
                            }
                        }
                    }
                }

                return count;
            }

            /// <summary>
            /// Query agents that intersect with the sphere.
            /// </summary>
            public int QueryCylinder<T>(float3 center, float radius, float height, int maxCount, ref T action, NavigationLayers layers = NavigationLayers.Everything) where T : unmanaged, ISpatialQueryEntity
            {
                if (maxCount == 0)
                    return QueryCylinder(center, radius, height, ref action, layers);

                var entries = new FixedEntries(center.xz, -1, float.MaxValue, maxCount);

                var map = m_Map;

                float3 cellSize = m_CellSize;
                int3 min = (int3) round((center - new float3(radius, 0, radius)) / cellSize);
                int3 max = (int3) round((center + new float3(radius, height, radius)) / cellSize);

                int2 size = max.xz - min.xz + 1;
                int2 halfSize = size / 2;
                int maxSide = math.max(size.x, size.y);
                int maxIterations = maxSide * maxSide;

                int2 point = 0;
                int2 offset = min.xz + (maxSide - 1) / 2;
                int2 step = new int2(0, -1);

                int count = 0;
                for (int i = 0; i < maxIterations; i++)
                {
                    // Check if current nodes is within the rectangle we want
                    if (math.all(-halfSize <= point) && math.all(point <= halfSize))
                    {
                        int2 offsetedPoint = point + offset;
                        for (int y = min.y; y < max.y + 1; y++)
                        {
                            // Find all entities in the bucket
                            if (map.TryGetFirstValue(new int3(offsetedPoint.x, y, offsetedPoint.y), out int index, out var iterator))
                            {
                                do
                                {
                                    if (!layers.Any(m_Agents[index].Layers))
                                        continue;
                                    if (entries.Add(index, m_Transforms[index].Position.xz))
                                    {
                                        count++;
                                        if (count == m_QueryChecks)
                                            goto FULL;
                                    }
                                }
                                while (map.TryGetNextValue(out index, ref iterator));
                            }
                        }
                    }
                    // Check if we need to change spiral direction
                    if ((point.x == point.y) || ((point.x < 0) && (point.x == -point.y)) || ((point.x > 0) && (point.x == 1 - point.y)))
                    {
                        // Swap and negate x
                        int temp = step.x;
                        step.x = -step.y;
                        step.y = temp;
                    }
                    // Step
                    point += step;
                }

                FULL:

                int entry = 0;
                while (true)
                {
                    var index = entries[entry++];

                    if (index == -1)
                        break;

                    action.Execute(m_Entities[index], m_Bodies[index], m_Shapes[index], m_Transforms[index]);
                }

                return entry;
            }

            /// <summary>
            /// Query partitions that intersect with the sphere.
            /// </summary>
            public int QuerySphereCells<T>(float3 center, float radius, T action) where T : unmanaged, ISpatialQueryVolume
            {
                int count = 0;

                // Find min and max point in radius
                int3 min = (int3) math.round((center - radius) / m_CellSize);
                int3 max = (int3) math.round((center + radius) / m_CellSize) + 1;

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        for (int k = min.z; k < max.z; ++k)
                        {
                            action.Execute(new float3(i, j, k) * m_CellSize, m_CellSize);
                            count++;
                        }
                    }
                }

                return count;
            }

            /// <summary>
            /// Query partitions that intersect with the cylinder.
            /// </summary>
            public int QueryCylinderCells<T>(float3 center, float radius, float height, T action) where T : unmanaged, ISpatialQueryVolume
            {
                int count = 0;

                // Find min and max point in radius
                int3 min = (int3) math.round((center - new float3(radius, 0, radius)) / m_CellSize);
                int3 max = (int3) math.round((center + new float3(radius, height, radius)) / m_CellSize) + 1;

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        for (int k = min.z; k < max.z; ++k)
                        {
                            action.Execute(new float3(i, j, k) * m_CellSize, m_CellSize);
                            count++;
                        }
                    }
                }

                return count;
            }

            unsafe struct FixedEntries
            {
                public const int Capacity = 32;

                float2 m_Center;
                fixed int m_Indices[Capacity];
                fixed float m_Distances[Capacity];
                int m_Length;

                public int this[int index] => m_Indices[index];

                public FixedEntries(float2 center, int defaultIndex, float defaultDistance, int length)
                {
                    // TODO: check that length is less than capacity

                    m_Center = center;
                    m_Length = length;

                    // Additional entry will act as null terminator
                    length++;

                    for (int entryIndex = 0; entryIndex < length; entryIndex++)
                    {
                        m_Indices[entryIndex] = defaultIndex;
                        m_Distances[entryIndex] = defaultDistance;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool Add(int index, float2 point)
                {
                    float distance = distancesq(m_Center, point);

                    int minEntryIndex = -1;
                    float minDistance = distance;

                    // Find min distance entry that is smaller than requested distance
                    for (int entryIndex = m_Length; entryIndex-- > 0;)
                    {
                        // All indices must be unique
                        if (index == m_Indices[entryIndex])
                            return false;

                        if (minDistance <= m_Distances[entryIndex])
                        {
                            minEntryIndex = entryIndex;
                            minDistance = m_Distances[entryIndex];
                        }
                    }

                    // Failed to find min entry index
                    if (minEntryIndex == -1)
                        return true;

                    // Update entry with new one
                    m_Indices[minEntryIndex] = index;
                    m_Distances[minEntryIndex] = distance;

                    return true;
                }
            }
        }
    }

    public interface ISpatialQueryEntity
    {
        void Execute(Entity entity, AgentBody body, AgentShape shape, LocalTransform transform);
    }

    public interface ISpatialQueryVolume
    {
        void Execute(float3 position, float3 size);
    }

    [BurstCompile]
    partial struct CopyJob : IJobEntity
    {
        public NativeList<Entity> Entities;
        public NativeList<Agent> Agents;
        public NativeList<AgentBody> Bodies;
        public NativeList<AgentShape> Shapes;
        public NativeList<LocalTransform> Transforms;

        void Execute(Entity entity, in Agent agent, in AgentBody body, in AgentShape shape, in LocalTransform transform)
        {
            Entities.Add(entity);
            Agents.Add(agent);
            Bodies.Add(body);
            Shapes.Add(shape);
            Transforms.Add(transform);
        }
    }

    [BurstCompile]
    partial struct HashJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Map;
        public float3 CellSize;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in Agent agent, in AgentBody body, in AgentShape shape, in LocalTransform transform)
        {
            int3 cell = (int3) math.round((transform.Position) / CellSize);
            Map.Add(cell, entityInQueryIndex);
        }
    }

    [BurstCompile]
    partial struct HashShapeJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Map;
        public float3 CellSize;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in Agent agent, in AgentBody body, in AgentShape shape, in LocalTransform transform)
        {
            if (shape.Type == ShapeType.Cylinder)
            {
                float3 center = transform.Position;
                float radius = shape.Radius;
                float height = shape.Height;

                // Find min and max point in radius
                int3 min = (int3) math.round((center - new float3(radius, 0, radius)) / CellSize);
                int3 max = (int3) math.round((center + new float3(radius, height, radius)) / CellSize) + 1;

                int count = 0;
                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        for (int k = min.z; k < max.z; ++k)
                        {
                            Map.Add(new int3(i, j, k), entityInQueryIndex);

                            if (count == AgentSpatialPartitioningSystem.MaxCellsPerUnit)
                                break;
                        }
                    }
                }
            }
            else
            {
                float3 center = transform.Position;
                float radius = shape.Radius;

                // Find min and max point in radius
                int2 min = (int2) round((center.xy - radius) / CellSize.xy);
                int2 max = (int2) round((center.xy + radius) / CellSize.xy) + 1;
                int k = (int) round(center.z / CellSize.z);

                int count = 0;

                for (int i = min.x; i < max.x; ++i)
                {
                    for (int j = min.y; j < max.y; ++j)
                    {
                        Map.Add(new int3(i, j, k), entityInQueryIndex);

                        if (count == AgentSpatialPartitioningSystem.MaxCellsPerUnit)
                            break;
                    }
                }
            }
        }
    }

    [BurstCompile]
    struct ClearJob : IJob
    {
        public NativeParallelMultiHashMap<int3, int> Map;
        public NativeList<Entity> Entities;
        public NativeList<Agent> Agents;
        public NativeList<AgentBody> Bodies;
        public NativeList<AgentShape> Shapes;
        public NativeList<LocalTransform> Transforms;

        public void Execute()
        {
            Map.Clear();
            Entities.Clear();
            Agents.Clear();
            Bodies.Clear();
            Shapes.Clear();
            Transforms.Clear();
        }
    }

    [BurstCompile]
    struct ChangeCapacityJob : IJob
    {
        public NativeParallelMultiHashMap<int3, int> Map;
        public NativeList<Entity> Entities;
        public NativeList<Agent> Agents;
        public NativeList<AgentBody> Bodies;
        public NativeList<AgentShape> Shapes;
        public NativeList<LocalTransform> Transforms;
        public int MapCapacity;
        public int Capacity;

        public void Execute()
        {
            Map.Capacity = MapCapacity;
            Entities.Capacity = Capacity;
            Agents.Capacity = Capacity;
            Bodies.Capacity = Capacity;
            Shapes.Capacity = Capacity;
            Transforms.Capacity = Capacity;
        }
    }
}
