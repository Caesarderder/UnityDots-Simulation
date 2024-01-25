using Unity.AI.Navigation;
using Unity.Entities;

namespace ProjectDawn.Navigation.Hybrid
{
    public class NavMeshSurfaceBaker : Baker<NavMeshSurface>
    {
        public override void Bake(NavMeshSurface authoring)
        {
            AddSharedComponentManaged(GetEntity(TransformUsageFlags.Dynamic), new NavMeshData { Value = authoring.navMeshData });
        }
    }
}
