using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    [Obsolete("This class is obsolete, please use new settings workflow https://lukaschod.github.io/agents-navigation-docs/manual/settings.html.")]
    [AddComponentMenu("Agents Navigation/Settings/Nav Mesh Settings")]
    [DisallowMultipleComponent]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/authoring.html")]
    public class NavMeshSettingsAuthoring : SettingsBehaviour
    {
        [Tooltip("TODO")]
        [SerializeField]
        protected int m_MaxIterations = 1024;

        [Tooltip("TODO")]
        [SerializeField]
        protected int m_MaxPath = 1024;

        /// <summary>
        /// Returns default component of <see cref="AgentSpatialPartitioningSystem.Settings"/>.
        /// </summary>
        public NavMeshQuerySystem.Settings DefaultSettings => new()
        {
            MaxIterations = m_MaxIterations,
            MaxPathSize = m_MaxPath,
        };

        public override Entity GetOrCreateEntity()
        {
            return Entity.Null;
        }
    }
}
