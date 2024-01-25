using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using static Unity.Entities.SystemAPI;
using System.Diagnostics;
using UnityEngine.Experimental.AI;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that controls agent NavMesh path.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(AgentPathingSystemGroup))]
    public partial struct NavMeshPathSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var navmesh = GetSingletonRW<NavMeshQuerySystem.Singleton>();
            new NavMeshPathJob
            {
                NavMesh = navmesh.ValueRW
            }.Schedule();
            navmesh.ValueRW.World.AddDependency(state.Dependency);
        }

        [BurstCompile]
        partial struct NavMeshPathJob : IJobEntity
        {
            public NavMeshQuerySystem.Singleton NavMesh;

            public void Execute(Entity entity, ref DynamicBuffer<NavMeshNode> nodes, ref NavMeshPath path, in AgentBody body, in LocalTransform transform)
            {
                switch (path.State)
                {
                    case NavMeshPathState.WaitingNewPath:
                        // Release previous handle if it is valid
                        if (NavMesh.Exist(path.QueryHandle))
                            NavMesh.DestroyQuery(path.QueryHandle);

                        path.Location = NavMesh.MapLocation(transform.Position, path.MappingExtent, path.AgentTypeId, path.AreaMask);
                        path.EndLocation = NavMesh.MapLocation(body.Destination, path.MappingExtent, path.AgentTypeId, path.AreaMask);
                        path.QueryHandle = NavMesh.CreateQuery(path.Location, path.EndLocation, path.AgentTypeId, path.AreaMask);
                        path.State = NavMeshPathState.InProgress;
                        break;

                    case NavMeshPathState.InProgress:
                        var status = NavMesh.GetStatus(path.QueryHandle);
                        switch (status)
                        {
                            case NavMeshQueryStatus.InProgress:
                                break;

                            case NavMeshQueryStatus.FinishedFullPath:
                            case NavMeshQueryStatus.FinishedPartialPath:
                                path.State = NavMeshPathState.Finished;

                                // Copy path polygons into nodes
                                var polygons = NavMesh.GetPolygons(path.QueryHandle);
                                nodes.ResizeUninitialized(polygons.Length);
                                for (int i = 0; i < polygons.Length; ++i)
                                    nodes[i] = new NavMeshNode { Value = polygons[i] };

                                // Release query so it could be reused
                                NavMesh.DestroyQuery(path.QueryHandle);
                                path.QueryHandle = NavMeshQueryHandle.Null;
                                break;

                            case NavMeshQueryStatus.Failed:
                                path.State = NavMeshPathState.Failed;

                                nodes.ResizeUninitialized(0);

                                // Release query so it could be reused
                                NavMesh.DestroyQuery(path.QueryHandle);
                                path.QueryHandle = NavMeshQueryHandle.Null;
                                break;

                            default:
                                break;

                        }
                        break;

                    case NavMeshPathState.InValid:
                        if (path.AutoRepath)
                            path.State = NavMeshPathState.WaitingNewPath;
                        break;
                }
            }
        }
    }
}
