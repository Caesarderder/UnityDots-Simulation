using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Experimental.AI;
using static Unity.Entities.SystemAPI;
using UnityEditor.AI;

namespace ProjectDawn.Navigation.Editor
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentPathingSystemGroup))]
    [UpdateAfter(typeof(NavMeshPathSystem))]
    public partial struct NavMeshNodesGizmosSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var navmesh = GetSingleton<NavMeshQuerySystem.Singleton>();
            var gizmos = GetSingletonRW<GizmosSystem.Singleton>();
            new Job
            {
                Navmesh = navmesh,
                Gizmos = gizmos.ValueRW.CreateCommandBuffer(),
            }.Schedule();
            navmesh.World.AddDependency(state.Dependency);
        }

        [BurstCompile]
        partial struct Job : IJobEntity
        {
            [ReadOnly]
            public NavMeshQuerySystem.Singleton Navmesh;
            public GizmosCommandBuffer Gizmos;

            public void Execute(Entity entity, in DrawGizmos drawGizmos, in NavMeshPath path, in DynamicBuffer<NavMeshNode> nodes)
            {
                if (path.State != NavMeshPathState.Finished)
                    return;

                var polygons = nodes.AsNativeArray().Reinterpret<PolygonId>();

                if (!Navmesh.IsPathValid(polygons))
                    return;

                //var areaColor = GetAreaColor(path.)

                NativeArray<float3> vertices = new NativeArray<float3>(24, Allocator.Temp);
                NativeArray<PolygonId> neighbours = new NativeArray<PolygonId>(24, Allocator.Temp);
                NativeArray<byte> indices = new NativeArray<byte>(24, Allocator.Temp);
                for (int i = 0; i < polygons.Length; ++i)
                {
                    if (Navmesh.GetEdgesAndNeighbors(polygons[i], vertices.Reinterpret<UnityEngine.Vector3>(), neighbours, indices, out int numVertices, out int numNeighbours))
                    {
                        var progress = polygons.Length > 1 ? (float) i / (polygons.Length - 1) : 1;
                        var color = UnityEngine.Color.Lerp(new UnityEngine.Color(0f, 0.75f, 1f, 0.15f), new UnityEngine.Color(0f, 0.75f, 1f, 0.35f), progress);
                        Gizmos.DrawAAConvexPolygon(vertices.GetSubArray(0, numVertices), color, true);
                    }
                }
                vertices.Dispose();
                neighbours.Dispose();
                indices.Dispose();
            }

            UnityEngine.Color GetAreaColor(int i)
            {
                UnityEngine.Color result;
                if (i == 0)
                {
                    result = new UnityEngine.Color(0f, 0.75f, 1f, 0.5f);
                }
                else
                {
                    int num = Bit(i, 4) + (Bit(i, 1) * 2 + 1) *63;
                    int num2 = Bit(i, 3) + (Bit(i, 2) * 2 + 1) *63;
                    int num3 = Bit(i, 5) + (Bit(i, 0) * 2 + 1) *63;
                    result = new UnityEngine.Color((float) num / 255f, (float) num2 / 255f, (float) num3 / 255f, 0.5f);
                }
                return result;
            }

            int Bit(int value, int index) => (value & index) != 0 ? 1 : 0;
        }
    }
}
