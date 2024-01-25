using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// State of the path.
    /// </summary>
    public enum NavMeshPathState
    {
        /// <summary>
        /// Waiting for new NavMesh path requested to be handled.
        /// </summary>
        WaitingNewPath,
        /// <summary>
        /// NavMesh path building is in progress.
        /// </summary>
        InProgress,
        /// <summary>
        /// NavMesh path building is finished. Check <see cref="NavMeshNode"/> buffer for result.
        /// </summary>
        Finished,
        /// <summary>
        /// NavMesg path building failed. See editor console for more information.
        /// </summary>
        Failed,
        /// <summary>
        /// NavMesh path is no longer valid.
        /// </summary>
        InValid,
    }

    /// <summary>
    /// Agent NavMesh path.
    /// </summary>
    public struct NavMeshPath : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// State of the path.
        /// </summary>
        public NavMeshPathState State;
        /// <summary>
        /// Query handle of current requested path.
        /// </summary>
        public NavMeshQueryHandle QueryHandle;
        /// <summary>
        /// The type ID for the agent in NavMesh.
        /// </summary>
        public int AgentTypeId;
        /// <summary>
        /// Specifies which NavMesh areas are passable. Changing areaMask will make the path stale (see isPathStale).
        /// </summary>
        public int AreaMask;
        /// <summary>
        /// Should the agent attempt to acquire a new path if the existing path becomes invalid?
        /// </summary>
        public bool AutoRepath;
        /// <summary>
        /// Constraint agent to be on the surface. It is useful to disable then used with physics, to allow more freedom motion and precision.
        /// </summary>
        public bool Grounded;
        [System.Obsolete("Constrained is deprecated, please use Grounded.")]
        public bool Constrained { get => Grounded; set => Grounded = value; }
        /// <summary>
        /// Maximum distance on each axis will be used when attempting to map the agent's position or destination onto navmesh.
        /// The higher the value, the bigger the performance cost.
        /// </summary>
        public float3 MappingExtent;
        /// <summary>
        /// Current location of the agent on NavMesh.
        /// </summary>
        public NavMeshLocation Location;
        /// <summary>
        /// End location of the agent on NavMesh.
        /// </summary>
        public NavMeshLocation EndLocation;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static NavMeshPath Default => new()
        {
            State = NavMeshPathState.Finished,
            AreaMask = -1,
            AutoRepath = true,
            MappingExtent = 10,
        };
    }

    /// <summary>
    /// Agent NavMesh single node of path.
    /// </summary>
    public struct NavMeshNode : IBufferElementData
    {
        /// <summary>
        /// Polygon id of NavMesh node.
        /// </summary>
        public PolygonId Value;
    }
}
