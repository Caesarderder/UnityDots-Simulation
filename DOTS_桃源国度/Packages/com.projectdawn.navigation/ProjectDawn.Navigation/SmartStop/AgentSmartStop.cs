using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// This option allows agent to do smarter stop decision then moving in group.
    /// It works under assumption that by reaching nearby agent that is already idle and have similar destination it can stop as destination is reached.
    /// </summary>
    [System.Serializable]
    public struct HiveMindStop
    {
        public bool Enabled;
        /// <summary>
        /// Radius at which agent will assume similarity of the distance.
        /// </summary>
        [Tooltip("Radius at which agent will assume similarity of the distance.")]
        public float Radius;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static HiveMindStop Default => new()
        {
            Enabled = true,
            Radius = 1,
        };
    }

    /// <summary>
    /// This option allows the agent to make smarter stop decisions than simply deciding if it is stuck.
    /// Every time the agent bumps into a standing agent, it will progress towards stopping. Additionally, by not bumping into one, it will recover from stopping.
    /// Once the progress value is met, the agent will stop.
    /// </summary>
    [System.Serializable]
    public struct GiveUpStop
    {
        public bool Enabled;

        /// <summary>
        /// The speed at which the agent progresses towards stopping.
        /// The higher the value, the faster it will stop.
        /// </summary>
        [Tooltip("The speed at which the agent progresses towards stopping. The higher the value, the faster it will stop.")]
        [Range(0f, 1f)]
        public float FatigueSpeed;

        /// <summary>
        /// The speed at which the agent recovers from stopping.
        /// The lower the value, the less tolerant it will be towards getting stuck.
        /// </summary>
        [Tooltip("The speed at which the agent recovers from stopping. The lower the value, the less tolerant it will be towards getting stuck.")]
        [Range(0f, 1f)]
        public float RecoverySpeed;

        /// <summary>
        /// Returns the default configuration.
        /// </summary>
        public static GiveUpStop Default => new()
        {
            Enabled = false,
            FatigueSpeed = 0.5f,
            RecoverySpeed = 0.4f,
        };
    }

    /// <summary>
    /// For tracking give up progress.
    /// </summary>
    public struct GiveUpStopTimer : IComponentData
    {
        /// <summary>
        /// Current progress towards stopping.
        /// Once it is reached, it will stop.
        /// </summary>
        public float Progress;

        /// <summary>
        /// Destination towards which it will track give up progress.
        /// </summary>
        public float3 Destination;
    }


    /// <summary>
    /// Agent smart stop to handle some common stuck scenarios.
    /// </summary>
    public struct AgentSmartStop : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// This option allows agent to do smarter stop decision then moving in group.
        /// It works under assumption that by reaching nearby agent that is already idle and have similar destination it can stop as destination is reached.
        // </summary>
        public HiveMindStop HiveMindStop;

        public GiveUpStop GiveUpStop;

        /// <summary>
        /// Returns default configuration.
        /// </summary>
        public static AgentSmartStop Default => new()
        {
            HiveMindStop = HiveMindStop.Default,
            GiveUpStop = GiveUpStop.Default,
        };
    }
}
