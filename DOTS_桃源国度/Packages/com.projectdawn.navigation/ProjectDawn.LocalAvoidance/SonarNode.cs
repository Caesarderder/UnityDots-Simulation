// @ 2022 Lukas Chodosevicius

namespace ProjectDawn.LocalAvoidance
{
    /// <summary>
    /// Internal node used by <see cref="SonarAvoidance"/>.
    /// </summary>
    struct SonarNode
    {
        /// <summary>
        /// Data.
        /// </summary>
        public Line Line;
        /// <summary>
        /// Left child node.
        /// </summary>
        public SonarNodeHandle Left;
        /// <summary>
        /// Right child node.
        /// </summary>
        public SonarNodeHandle Right;

        /// <summary>
        /// True if node does not have child nodes.
        /// </summary>
        public bool IsLeaf => Left == SonarNodeHandle.Null;

        public SonarNode(Line line)
        {
            Line = line;
            Left = SonarNodeHandle.Null;
            Right = SonarNodeHandle.Null;
        }
    }
}
