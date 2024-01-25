namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Represents handle of NavMeshQuery used by <see cref="NavMeshQuerySystem"/>.
    /// </summary>
    public struct NavMeshQueryHandle
    {
        /// <summary>
        /// Unique index of NavMesh query.
        /// </summary>
        public int Index;

        /// <summary>
        /// Invalid NavMesh query.
        /// </summary>
        public static NavMeshQueryHandle Null => new NavMeshQueryHandle();

        public static implicit operator int(NavMeshQueryHandle handle) => handle.Index;
        public static bool operator ==(NavMeshQueryHandle lhs, NavMeshQueryHandle rhs) => lhs.Index == rhs.Index;
        public static bool operator !=(NavMeshQueryHandle lhs, NavMeshQueryHandle rhs) => lhs.Index != rhs.Index;

        public bool Equals(NavMeshQueryHandle other) => other.Index == Index;

        public override bool Equals(object compare) => compare is NavMeshQueryHandle compareNode && Equals(compareNode);

        public override int GetHashCode() => Index;
    }
}
