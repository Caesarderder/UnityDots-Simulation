// @ 2022 Lukas Chodosevicius

namespace ProjectDawn.LocalAvoidance
{
    /// <summary>
    /// Internal node handle used by <see cref="SonarAvoidance"/>.
    /// </summary>
    struct SonarNodeHandle
    {
        /// <summary>
        /// Internal index of node inside <see cref="SonarAvoidance"/>.
        /// </summary>
        public int Index;

        /// <summary>
        /// Handle that is null.
        /// </summary>
        public static SonarNodeHandle Null => new SonarNodeHandle { Index = -1 };

        public static implicit operator int(SonarNodeHandle handle) => handle.Index;
        public static bool operator==(SonarNodeHandle lhs, SonarNodeHandle rhs) => lhs.Index == rhs.Index;
        public static bool operator!=(SonarNodeHandle lhs, SonarNodeHandle rhs) => lhs.Index != rhs.Index;
        public bool Equals(SonarNodeHandle other) => other.Index == Index;
        public override bool Equals(object compare) => compare is SonarNodeHandle compareNode && Equals(compareNode);
        public override int GetHashCode() => Index;
    }
}
