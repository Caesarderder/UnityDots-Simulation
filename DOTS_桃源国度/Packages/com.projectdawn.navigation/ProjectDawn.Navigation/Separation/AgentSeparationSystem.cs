using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using static Unity.Entities.SystemAPI;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// System that calculates separation direction from nearby agents.
    /// </summary>
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentForceSystemGroup))]
    public partial struct AgentSeparationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingleton<AgentSpatialPartitioningSystem.Singleton>();
            new AgentSeparationJob
            {
                Spatial = spatial,
            }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct AgentSeparationJob : IJobEntity
        {
            [ReadOnly]
            public AgentSpatialPartitioningSystem.Singleton Spatial;

            public void Execute(Entity entity, ref AgentBody body, in AgentSeparation separation, in AgentShape shape, in LocalTransform transform)
            {
                float radius = math.max(shape.Radius, separation.Radius);

                if (shape.Type == ShapeType.Cylinder)
                {
                    var action = new CylindersSeperation
                    {
                        Entity = entity,
                        Body = body,
                        Shape = shape,
                        Separation = separation,
                        Transform = transform,
                        Radius = radius,
                    };

                    Spatial.QueryCylinder(transform.Position, radius, shape.Height, Spatial.QueryCapacity, ref action, separation.Layers);

                    if (action.Weight > 0)
                        body.Force += action.Force * separation.Weight;
                }
                else
                {
                    var action = new CirclesSeperation
                    {
                        Entity = entity,
                        Body = body,
                        Shape = shape,
                        Separation = separation,
                        Transform = transform,
                        Radius = radius,
                    };

                    Spatial.QueryCircle(transform.Position, radius, Spatial.QueryCapacity, ref action, separation.Layers);

                    if (action.Weight > 0)
                        body.Force.xy += action.Force * separation.Weight;
                }
            }

            struct CylindersSeperation : ISpatialQueryEntity
            {
                public Entity Entity;
                public AgentBody Body;
                public AgentShape Shape;
                public AgentSeparation Separation;
                public LocalTransform Transform;
                public float Radius;

                public float3 Force;
                public float Weight;

                public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
                {
                    float2 towards = Transform.Position.xz - otherTransform.Position.xz;
                    float distance = math.length(towards);
                    float radiusSum = Radius + otherShape.Radius;
                    if (distance > radiusSum || Entity == otherEntity)
                        return;

                    float extent = Shape.Height * 0.5f;
                    float otherExtent = otherShape.Height * 0.5f;
                    if (math.abs((Transform.Position.y + extent) - (otherTransform.Position.y + otherExtent)) > extent + otherExtent)
                        return;

                    var force = math.normalizesafe(towards) * ((radiusSum - distance) / radiusSum);
                    Force += new float3(force.x, 0, force.y);
                    Weight++;
                }
            }

            struct CirclesSeperation : ISpatialQueryEntity
            {
                public Entity Entity;
                public AgentBody Body;
                public AgentShape Shape;
                public AgentSeparation Separation;
                public LocalTransform Transform;
                public float Radius;

                public float2 Force;
                public float Weight;

                public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
                {
                    float2 towards = Transform.Position.xy - otherTransform.Position.xy;
                    float distance = math.length(towards);
                    float radiusSum = Radius + otherShape.Radius;
                    if (distance > radiusSum || Entity == otherEntity)
                        return;

                    Force += math.normalizesafe(towards) * ((radiusSum - distance) / radiusSum);
                    Weight++;
                }
            }
        }
    }
}
