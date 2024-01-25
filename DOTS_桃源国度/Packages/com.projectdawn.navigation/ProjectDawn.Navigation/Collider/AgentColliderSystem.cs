using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using static Unity.Entities.SystemAPI;
using Random = Unity.Mathematics.Random;

namespace ProjectDawn.Navigation
{
    [System.Serializable]
    public class ColliderSubSettings : ISubSettings
    {
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("The number of iterations per frame for resolving collisions will be determined. A higher number will result in more accurate collision resolution, but it will also incur a greater performance cost.")]
        [UnityEngine.Range(1, 8)]
        int m_Iterations = 4;

        /// <summary>
        /// The number of iterations per frame for resolving collisions will be determined. A higher number will result in more accurate collision resolution, but it will also incur a greater performance cost.
        /// </summary>
        public int Iterations => m_Iterations;
    }

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentDisplacementSystemGroup))]
    public partial struct AgentColliderSystem : ISystem
    {
        int m_Iterations;
        const float ResolveFactor = 0.7f;

        SystemHandle m_SpatialPartitioningSystem;

        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_SpatialPartitioningSystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<AgentSpatialPartitioningSystem>();
            m_Iterations = AgentsNavigationSettings.Get<ColliderSubSettings>().Iterations;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingletonRW<AgentSpatialPartitioningSystem.Singleton>();
            var world = state.WorldUnmanaged;
            ref var spatialSystem = ref world.GetUnsafeSystemRef<AgentSpatialPartitioningSystem>(m_SpatialPartitioningSystem);

            var job = new AgentColliderJob
            {
                Spatial = spatial.ValueRO,
                ResolveFactor = ResolveFactor,
            };

            for (int iteration = 0; iteration < m_Iterations; ++iteration)
            {
                state.Dependency = spatialSystem.ScheduleUpdate(ref world.ResolveSystemStateRef(m_SpatialPartitioningSystem), state.Dependency);
                state.Dependency = job.ScheduleParallel(state.Dependency);
            }
        }
    }

    [BurstCompile]
    partial struct AgentColliderJob : IJobEntity
    {
        [ReadOnly]
        public AgentSpatialPartitioningSystem.Singleton Spatial;
        public float ResolveFactor;

        public void Execute(Entity entity, ref AgentBody body, ref LocalTransform transform, in AgentShape shape, in AgentCollider collider)
        {
            // TODO: Expose this as option to save performance
            //if (body.IsStopped)
            //    return;

            if (shape.Type == ShapeType.Cylinder)
            {
                var action = new CylindersCollision
                {
                    Entity = entity,
                    Body = body,
                    Shape = shape,
                    Transform = transform,
                    ResolveFactor = ResolveFactor,
                };

                Spatial.QueryCylinder(transform.Position, shape.Radius, shape.Height, Spatial.m_QueryCapacity, ref action, collider.Layers);

                if (action.Weight > 0)
                {
                    action.Displacement = action.Displacement / action.Weight;
                    transform.Position += action.Displacement;
                }
            }
            else
            {
                var action = new CirclesCollision
                {
                    Entity = entity,
                    Body = body,
                    Shape = shape,
                    Transform = transform,
                    ResolveFactor = ResolveFactor,
                };

                Spatial.QueryCircle(transform.Position, shape.Radius, Spatial.m_QueryCapacity, ref action, collider.Layers);

                if (action.Weight > 0)
                {
                    action.Displacement = action.Displacement / action.Weight;
                    transform.Position += new float3(action.Displacement, 0);
                }
            }
        }

        struct CirclesCollision : ISpatialQueryEntity
        {
            public Entity Entity;
            public AgentBody Body;
            public AgentShape Shape;
            public LocalTransform Transform;

            public float2 Displacement;
            public float Weight;
            public float ResolveFactor;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
            {
                if (otherShape.Type != ShapeType.Circle)
                    return;

                if (Body.IsStopped && !otherBody.IsStopped)
                    return;

                float2 towards = Transform.Position.xy - otherTransform.Position.xy;

                float distancesq = math.lengthsq(towards);
                float radiusSum = Shape.Radius + otherShape.Radius;
                if (distancesq > radiusSum * radiusSum || Entity == otherEntity)
                    return;

                float distance = math.sqrt(distancesq);
                float penetration = radiusSum - distance;

                if (distance < 0.0001f)
                {
                    // Avoid both having same displacement
                    if (otherEntity.Index > Entity.Index)
                    {
                        towards = -Body.Velocity.xy;
                    }
                    else
                    {
                        towards = Body.Velocity.xy;
                    }
                    penetration = 0.01f;
                }
                else
                {
                    penetration = (penetration / distance) * ResolveFactor;
                }

                Displacement += towards * penetration;
                Weight++;
            }
        }

        struct CylindersCollision : ISpatialQueryEntity
        {
            public Entity Entity;
            public AgentBody Body;
            public AgentShape Shape;
            public LocalTransform Transform;

            public float3 Displacement;
            public float Weight;
            public float ResolveFactor;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
            {
                if (otherShape.Type != ShapeType.Cylinder)
                    return;

                if (Body.IsStopped && !otherBody.IsStopped)
                    return;

                float extent = Shape.Height * 0.5f;
                float otherExtent = otherShape.Height * 0.5f;
                if (math.abs((Transform.Position.y + extent) - (otherTransform.Position.y + otherExtent)) > extent + otherExtent)
                    return;

                float2 towards = Transform.Position.xz - otherTransform.Position.xz;
                float distancesq = math.lengthsq(towards);
                float radius = Shape.Radius + otherShape.Radius;
                if (distancesq > radius * radius || Entity == otherEntity)
                    return;

                float distance = math.sqrt(distancesq);
                float penetration = radius - distance;

                if (distance < 0.0001f)
                {
                    // Avoid both having same displacement
                    if (otherEntity.Index > Entity.Index)
                    {
                        towards = -Body.Velocity.xz;
                    }
                    else
                    {
                        towards = Body.Velocity.xz;
                    }
                    if (math.length(towards) < 0.0001f)
                    {
                        float2 avoidDirection = new Random((uint) Entity.Index + 1).NextFloat2Direction();
                        towards = avoidDirection;
                    }
                    penetration = 0.01f;
                }
                else
                {
                    penetration = (penetration / distance) * ResolveFactor;
                }

                Displacement += new float3(towards.x, 0, towards.y) * penetration;
                Weight++;
            }
        }
    }
}
