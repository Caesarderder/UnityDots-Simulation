using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using static Unity.Entities.SystemAPI;
using NavMeshPath = ProjectDawn.Navigation.NavMeshPath;
using Unity.Collections.LowLevel.Unsafe;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireMatchingQueriesForUpdate]
    public partial class OrderSystem : SystemBase
    {
        Gestures m_Gestures;

        protected override void OnCreate()
        {
            m_Gestures = GameObject.FindObjectOfType<Gestures>(true);
        }

        public struct Singleton : IComponentData
        {
            public NativeList<Entity> SelectedEntities;
        }

        protected override void OnUpdate()
        {
            if (m_Gestures == null)
                return;

            var selection = SystemAPI.GetSingleton<SelectionSystem.Singleton>();
            if (selection.SelectedEntities.IsEmpty)
                return;

            if (m_Gestures.Stop())
            {
                var bodyLookup = GetComponentLookup<AgentBody>();
                var brainLookup = GetComponentLookup<UnitBrain>();
                Dependency = new StopJob
                {
                    BodyLookup = bodyLookup,
                    BrainLookup = brainLookup,
                    SelectedEntities = selection.SelectedEntities,
                }.Schedule(Dependency);
            }

            if (m_Gestures.Confirmation(out float3 destination))
            {
                // Ray cast to find position of environment
                var ray = Camera.main.ScreenPointToRay(destination);
                if (!Physics.Raycast(ray, out var hit))
                    return;

                var navmesh = SystemAPI.GetSingleton<NavMeshQuerySystem.Singleton>();
                var spatial = SystemAPI.GetSingleton<AgentSpatialPartitioningSystem.Singleton>();
                var confirmation = SystemAPI.GetSingletonRW<ConfirmationSystem.Singleton>();
                Dependency = new ConfirmationJob
                {
                    Spatial = spatial,
                    NavMesh = navmesh,
                    TransformLookup = GetComponentLookup<LocalTransform>(true),
                    UnitLookup = GetComponentLookup<Unit>(true),
                    ShapeLooukup = GetComponentLookup<AgentShape>(true),
                    BodyLookup = GetComponentLookup<AgentBody>(),
                    PathLookup = GetComponentLookup<NavMeshPath>(),
                    BrainLookup = GetComponentLookup<UnitBrain>(),
                    FollowLookup = GetComponentLookup<UnitFollow>(),
                    SelectedEntities = selection.SelectedEntities,
                    Ray = ray,
                    Enter = hit.distance,
                    Confirmation = confirmation,
                }.Schedule(Dependency);
            }
        }

        [BurstCompile]
        struct StopJob : IJob
        {
            public ComponentLookup<AgentBody> BodyLookup;
            public ComponentLookup<UnitBrain> BrainLookup;
            [ReadOnly]
            public NativeList<Entity> SelectedEntities;

            public void Execute()
            {
                foreach (var entity in SelectedEntities)
                {
                    if (!BodyLookup.TryGetComponent(entity, out AgentBody body))
                        continue;

                    if (!BrainLookup.TryGetComponent(entity, out UnitBrain brain))
                        continue;

                    brain.State = UnitBrainState.Idle;
                    BrainLookup[entity] = brain;

                    body.IsStopped = true;
                    body.Velocity = 0;
                    BodyLookup[entity] = body;
                }
            }
        }

        [BurstCompile]
        struct ConfirmationJob : IJob
        {
            [ReadOnly]
            public AgentSpatialPartitioningSystem.Singleton Spatial;
            [ReadOnly]
            public NavMeshQuerySystem.Singleton NavMesh;
            [ReadOnly]
            public ComponentLookup<LocalTransform> TransformLookup;
            [ReadOnly]
            public ComponentLookup<Unit> UnitLookup;
            [ReadOnly]
            public ComponentLookup<AgentShape> ShapeLooukup;
            public ComponentLookup<NavMeshPath> PathLookup;
            public ComponentLookup<UnitBrain> BrainLookup;
            public ComponentLookup<UnitFollow> FollowLookup;
            public ComponentLookup<AgentBody> BodyLookup;

            // Selected entities
            [ReadOnly]
            public NativeList<Entity> SelectedEntities;
            public Ray Ray;
            public float Enter;

            [NativeDisableUnsafePtrRestriction]
            public RefRW<ConfirmationSystem.Singleton> Confirmation;

            public void Execute()
            {
                // Raycast environment
                float distanceToEnvironment = Enter;

                // Raycast other agents in case it actually ordered on agent non environment
                var action = new Action
                {
                    UnitLookup = UnitLookup,
                    Ray = Ray,
                    DistanceToAgent = distanceToEnvironment,
                };
                Spatial.QueryLine(Ray.origin, Ray.GetPoint(distanceToEnvironment), ref action);

                // Check if target is valid and closer to ray, in that case attack it
                if (action.Target != Entity.Null)
                {
                    foreach (var entity in SelectedEntities)
                    {
                        if (!BrainLookup.TryGetComponent(entity, out UnitBrain brain))
                            continue;

                        if (!FollowLookup.TryGetComponent(entity, out UnitFollow follow))
                            continue;

                        // Set target
                        follow.Target = action.Target;
                        FollowLookup[entity] = follow;

                        // Set order to attack
                        brain.State = UnitBrainState.Follow;
                        BrainLookup[entity] = brain;
                    }

                    return;
                }

                // Find the minimum circle that would contain all agents
                Circle formationCircle = new Circle();
                float averageCircleRadius = 0;
                int formationCount = 0;
                foreach (var entity in SelectedEntities)
                {
                    if (!TransformLookup.TryGetComponent(entity, out LocalTransform transform))
                        continue;
                    if (!ShapeLooukup.TryGetComponent(entity, out AgentShape shape))
                        continue;

                    var circle = new Circle(transform.Position.xz, shape.Radius);

                    averageCircleRadius += circle.Radius;

                    if (formationCount == 0)
                    {
                        formationCircle = circle;
                    }
                    else
                    {
                        formationCircle = Circle.Union(formationCircle, circle);
                    }

                    formationCount++;
                }

                // Early out if there is no entities
                if (formationCount == 0)
                    return;

                averageCircleRadius /= formationCount;

                // Find out minimum circle that would contain that number of agents
                float maxFormationRadiusSq = ((averageCircleRadius * averageCircleRadius) * (formationCount + 1.9f)) / 0.83f;
                float maxFormationRadius = math.sqrt(maxFormationRadiusSq);

                // Now formation usually dont form perfect circle here we add margin of error
                maxFormationRadius *= 2f;

                //Debug.Log($"Current formation radius {formationCircle.Radius} max {maxFormationRadius}");

                // Check if it needs new formation
                bool formation = formationCircle.Radius <= maxFormationRadius && formationCount > 1;
                //formation = true;

                // Set new destination for each agent
                float3 destination = Ray.GetPoint(distanceToEnvironment);
                foreach (var entity in SelectedEntities)
                {
                    if (!TransformLookup.TryGetComponent(entity, out LocalTransform transform))
                        continue;

                    if (!BodyLookup.TryGetComponent(entity, out AgentBody body))
                        continue;

                    if (!PathLookup.TryGetComponent(entity, out NavMeshPath path))
                        continue;

                    if (!BrainLookup.TryGetComponent(entity, out UnitBrain brain))
                        continue;

                    // Set order to move
                    brain.State = UnitBrainState.Move;
                    BrainLookup[entity] = brain;

                    // Either move all agents in formation or not
                    if (formation)
                    {
                        // For this agent get new formation offset from destination
                        float2 offset = transform.Position.xz - formationCircle.Center;

                        // Get actual position in navmesh (Jobified version of NavMesh.SamplePosition)
                        var location = NavMesh.MapLocation(destination, path.MappingExtent, path.AgentTypeId, path.AreaMask);

                        // Now here we offset location (Jobified version of NavMesh.Raycast)
                        // This is very important in case destination is on the cliff, with this formation position will always be within the cliff
                        var offsetLocation = NavMesh.MoveLocation(location, location.position + new Vector3(offset.x, 0, offset.y), path.AreaMask);

                        // Update to new destination
                        body.Destination = offsetLocation.position;
                        body.IsStopped = false;
                        BodyLookup[entity] = body;
                    }
                    else
                    {
                        // Simply move all entities to same destination
                        body.Destination = destination;
                        body.IsStopped = false;
                        BodyLookup[entity] = body;
                    }
                }

                Confirmation.ValueRW = new ConfirmationSystem.Singleton
                {
                    Position = destination,
                    Play = true,
                };
            }

            struct Action : ISpatialQueryEntity
            {
                [ReadOnly]
                public ComponentLookup<Unit> UnitLookup;

                public Ray Ray;
                public float DistanceToAgent;

                public Entity Target;

                public void Execute(Entity entity, AgentBody body, AgentShape shape, LocalTransform transform)
                {
                    // Check if ray intersects sphere
                    if (IntersectionRayAndSphere(Ray.origin, Ray.direction, transform.Position, shape.Radius, out float t1, out float t2))
                    {
                        // Check if unit is enemy
                        if (!UnitLookup.TryGetComponent(entity, out Unit unit) || unit.Owner == PlayerId.Red)
                            return;

                        float distanceToAgent = math.min(t1, t2);
                        if (distanceToAgent < DistanceToAgent)
                        {
                            Target = entity;
                            DistanceToAgent = distanceToAgent;
                        }
                    }
                }

                /// <summary>
                /// Based on http://kylehalladay.com/blog/tutorial/math/2013/12/24/Ray-Sphere-Intersection.html (There is some mistakes in proposed solution).
                /// </summary>
                static bool IntersectionRayAndSphere(float3 origin, float3 direction, float3 center, float radius, out float t1, out float t2)
                {
                    t1 = 0;
                    t2 = 0;

                    //solve for tc
                    float3 L = center - origin;
                    float tc = math.dot(L, direction);
                    if (tc < 0.0)
                    {
                        //Debug.Log($"0: {math.dot(L, direction)} < 0.0 ");
                        return false;
                    }

                    float d = math.sqrt(math.lengthsq(L) - (tc * tc));
                    if (d > radius)
                    {
                        //Debug.Log($"2: {d} >{radius} ");
                        return false;
                    }

                    //solve for t1c
                    float t1c = math.sqrt((radius * radius) - (d * d));

                    //solve for intersection points
                    t1 = tc - t1c;
                    t2 = tc + t1c;

                    //Debug.Log($"Good ");

                    return true;
                }
            }
        }
    }
}
