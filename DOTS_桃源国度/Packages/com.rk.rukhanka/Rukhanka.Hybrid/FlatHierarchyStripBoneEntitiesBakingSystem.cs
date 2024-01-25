using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(BakingSystemGroup))]
[UpdateBefore(typeof(BakingOnlyEntityAuthoringBakingSystem))]
partial class FlatHierarchyStripBoneEntitiesBakingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var boneEntitiesToRemove in SystemAPI.Query<DynamicBuffer<BoneEntitiesToRemove>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            for (int i = 0; i < boneEntitiesToRemove.Length; ++i)
            {
                var e = boneEntitiesToRemove[i].boneEntity;
                if (EntityManager.Exists(e))
                    ecb.AddComponent<BakingOnlyEntity>(e);
            }
        }

        ecb.Playback(EntityManager);
    }
}
}
