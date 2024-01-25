using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    [Obsolete("This class is obsolete, please use new settings workflow https://lukaschod.github.io/agents-navigation-docs/manual/settings.html.")]
    [AddComponentMenu("Agents Navigation/Settings/Spatial Partitioning Settings")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/settings.html")]
    public class SpatialPartitioningSettingsAuthoring : SettingsBehaviour
    {
        [Tooltip("The size of single partition.")]
        [SerializeField]
        protected float3 CellSize = 3;

        /// <summary>
        /// Returns default component of <see cref="AgentSpatialPartitioningSystem.Settings"/>.
        /// </summary>
        public AgentSpatialPartitioningSystem.Settings DefaultSettings => new AgentSpatialPartitioningSystem.Settings
        {
            CellSize = CellSize,
        };

        public override Entity GetOrCreateEntity()
        {
            return Entity.Null;
        }
    }
}
