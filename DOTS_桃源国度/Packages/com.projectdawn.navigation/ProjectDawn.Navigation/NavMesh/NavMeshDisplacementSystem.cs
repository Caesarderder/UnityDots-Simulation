using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Experimental.AI;
using static Unity.Entities.SystemAPI;
using Unity.Mathematics;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that forces agents to stay within NavMesh surface.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentDisplacementSystemGroup))]
    [UpdateAfter(typeof(AgentColliderSystem))]
    public partial struct NavMeshDisplacementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var navmesh = GetSingleton<NavMeshQuerySystem.Singleton>();
            new NavMeshPositionJob
            {
                NavMesh = navmesh,
                DeltaTime = Time.DeltaTime,
            }.ScheduleParallel();
            navmesh.World.AddDependency(state.Dependency);
        }

        [BurstCompile]
        partial struct NavMeshPositionJob : IJobEntity
        {
            [ReadOnly]
            public NavMeshQuerySystem.Singleton NavMesh;
            public float DeltaTime;

            public void Execute(ref DynamicBuffer<NavMeshNode> nodes, ref NavMeshPath path, ref LocalTransform transform, ref AgentBody body)
            {
                var location = path.Location;

                // Early out if location is not valid
                if (location.polygon.IsNull())
                    return;

                var newLocation = NavMesh.MoveLocation(location, transform.Position, path.AreaMask);

                if (path.Grounded)
                {
                    transform.Position = newLocation.position;

#if EXPERIMENTAL_SONAR_TIME
                    float stepLength = math.distance(location.position, newLocation.position) / DeltaTime;
                    float speed = math.length(body.Velocity);
                    if (stepLength > speed)
                    {
                        body.Velocity = math.normalizesafe(newLocation.position - location.position) * speed;
                    }
                    else
                    {
                        body.Velocity = (newLocation.position - location.position) / DeltaTime;
                    }
#endif
                }

                NavMesh.ProgressPath(ref nodes, location.polygon, newLocation.polygon);

                path.Location = newLocation;
            }
        }
    }
}
