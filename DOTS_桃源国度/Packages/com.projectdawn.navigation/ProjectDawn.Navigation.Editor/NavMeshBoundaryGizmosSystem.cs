using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Experimental.AI;
using static Unity.Entities.SystemAPI;
using UnityEditor.AI;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

namespace ProjectDawn.Navigation.Editor
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentPathingSystemGroup))]
    [UpdateAfter(typeof(NavMeshBoundarySystem))]
    public partial struct NavMeshBoundaryGizmosSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gizmos = GetSingletonRW<GizmosSystem.Singleton>();
            new Job
            {
                Gizmos = gizmos.ValueRW.CreateCommandBuffer(),
            }.Schedule();
        }

        [BurstCompile]
        partial struct Job : IJobEntity
        {
            public GizmosCommandBuffer Gizmos;

            public void Execute(in AgentShape shape, in DynamicBuffer<NavMeshWall> walls, in LocalTransform transform, in DrawGizmos drawGizmos)
            {
                foreach (var wall in walls)
                {
                    DrawWall(wall, shape.Height, new Color(0.35f, 0.35f, 0.35f, 0.75f));
                }
            }
            void DrawWall(NavMeshWall wall, float height, Color color)
            {
                float3 offset = new float3(0, height, 0);

                Gizmos.DrawQuad(wall.Start, wall.End, wall.End + offset, wall.Start + offset, color, true);
                Gizmos.DrawLine(wall.Start, wall.End, new Color(0.35f, 0.35f, 0.35f, 1));

                float3 direction = (wall.End - wall.Start);
                float3 normal = math.cross(offset, direction);
                float3 center = (wall.Start + wall.End + offset) * 0.5f;
                Gizmos.DrawArrow(center, normal, 0.1f, new Color(0.35f, 0.35f, 0.35f, 1));
            }
        }
    }
}
