using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

namespace ProjectDawn.Navigation
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class NavMeshDataSystem : SystemBase
    {
        List<UnityEngine.AI.NavMeshDataInstance> m_Instances = new();

        protected override void OnUpdate()
        {
            Entities.WithNone<NavMeshDataInstance>().ForEach((Entity entity, in NavMeshData data, in LocalTransform transform) =>
            {
                if (data.Value == null)
                    throw new System.Exception("NavMeshSurface is missing bake data.");

                var instance = UnityEngine.AI.NavMesh.AddNavMeshData(data.Value, transform.Position, transform.Rotation);
                EntityManager.AddComponentData(entity, new NavMeshDataInstance { Value = instance });
                m_Instances.Add(instance);
            }).WithStructuralChanges().Run();

            Entities.WithNone<NavMeshData>().ForEach((Entity entity, in NavMeshDataInstance instance) =>
            {
                instance.Value.Remove();
                m_Instances.Remove(instance.Value);
            }).WithStructuralChanges().Run();
        }

        protected override void OnDestroy()
        {
            foreach (var instance in m_Instances)
                instance.Remove();
            m_Instances.Clear();
        }
    }
}
