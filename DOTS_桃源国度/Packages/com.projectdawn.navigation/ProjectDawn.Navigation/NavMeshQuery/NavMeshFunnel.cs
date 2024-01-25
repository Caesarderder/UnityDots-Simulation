using System;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Experimental.AI;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Used for creating optimal path from navmesh polygons.
    /// </summary>
    public struct NavMeshFunnel : IDisposable
    {
        NativeArray<NavMeshLocation> m_Corners;
        NativeArray<StraightPathFlags> m_StraightPathFlags;
        NativeArray<float> m_VertexSide;
        int m_Capacity;
        int m_Length;

        public NavMeshFunnel(int capacity, Allocator allocator)
        {
            m_Capacity = capacity;
            m_Corners = new NativeArray<NavMeshLocation>(capacity, allocator);
            m_StraightPathFlags = new NativeArray<StraightPathFlags>(capacity, allocator);
            m_VertexSide = new NativeArray<float>(capacity, allocator);
            m_Length = 0;
        }

        public bool IsEndReachable => m_Length > 0 && (m_StraightPathFlags[m_Length - 1] & StraightPathFlags.End) != 0;

        /// <summary>
        /// Recreates corridor with straight path. This method will attempt to build optimal path using NavMesh polygons.
        /// </summary>
        /// <param name="query">The NavMesh query.</param>
        /// <param name="path">Polygons array.</param>
        /// <param name="from">Starting position.</param>
        /// <param name="to">Destination position.</param>
        /// <returns>Returns true if path is valid.</returns>
        public bool TryCreateStraightPath(NavMeshQuery query, NativeSlice<PolygonId> path, float3 from, float3 to)
        {
            var pathStatus = PathUtils.FindStraightPath(
                query,
                from,
                to,
                path,
                path.Length,
                ref m_Corners,
                ref m_StraightPathFlags,
                ref m_VertexSide,
                ref m_Length,
                m_Capacity
            );

            return pathStatus == PathQueryStatus.Success;
        }

        /// <summary>
        /// Returns locations array of the path.
        /// </summary>
        /// <returns>Returns array of locations.</returns>
        public NativeSlice<NavMeshLocation> AsLocations()
        {
            return m_Corners.Slice(0, m_Length);
        }

        /// <summary>
        /// Returns distance of the path.
        /// </summary>
        /// <returns></returns>
        public float GetCornersDistance()
        {
            float distance = 0;
            for (int i = 1; i < m_Length; ++i)
            {
                distance += math.distance(m_Corners[i - 1].position, m_Corners[i].position);
            }
            return distance;
        }

        public void Dispose()
        {
            m_Corners.Dispose();
            m_StraightPathFlags.Dispose();
            m_VertexSide.Dispose();
        }
    }
}
