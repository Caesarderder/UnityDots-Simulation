using Unity.Entities;

namespace ProjectDawn.Navigation
{
    public struct NavMeshData : ISharedComponentData, System.IEquatable<NavMeshData>
    {
        public UnityEngine.AI.NavMeshData Value;

        public bool Equals(NavMeshData other) => Value == other.Value;

        public override int GetHashCode()
        {
            if (Value == null)
                return 0;
            return Value.GetHashCode();
        }
    }
}
