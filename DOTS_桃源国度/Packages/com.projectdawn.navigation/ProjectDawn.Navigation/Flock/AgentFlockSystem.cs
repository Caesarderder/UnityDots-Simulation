using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Assertions;
using Color = UnityEngine.Color;

namespace ProjectDawn.Navigation
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentForceSystemGroup))]
    [UpdateAfter(typeof(FlockGroupSystem))]
    public partial class AgentFlockSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var bodyFromEntity = GetComponentLookup<AgentBody>();
            var translationFromEntity = GetComponentLookup<LocalTransform>(true);

            Entities
            .WithReadOnly(translationFromEntity)
            .ForEach((in FlockGroup group, in DynamicBuffer<FlockEntity> flock) =>
            {
                for (int index = 0; index < flock.Length; ++index)
                {
                    var entity = flock[index].Value;
                    if (!bodyFromEntity.TryGetComponent(entity, out AgentBody flockBody))
                        continue;
                    if (!translationFromEntity.TryGetComponent(entity, out LocalTransform flockTransform))
                        continue;

                    float3 cohesionDirection = math.normalizesafe(group.AveragePositions - flockTransform.Position);
                    float3 alignmentDirection = group.AverageDirection;

                    float weight = 1 - (group.Cohesion + group.Alignment);
                    float3 direction = flockBody.Force * weight + cohesionDirection * group.Cohesion + alignmentDirection * group.Alignment;
                    flockBody.Force = math.normalizesafe(direction);
                    bodyFromEntity[entity] = flockBody;
                }
            }).Schedule();
        }
    }
}
