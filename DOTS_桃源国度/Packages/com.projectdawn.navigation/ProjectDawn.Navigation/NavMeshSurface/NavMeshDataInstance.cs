using Unity.Entities;

namespace ProjectDawn.Navigation
{
    public struct NavMeshDataInstance : IComponentData, ICleanupComponentData
    {
        public UnityEngine.AI.NavMeshDataInstance Value;
    }
}
