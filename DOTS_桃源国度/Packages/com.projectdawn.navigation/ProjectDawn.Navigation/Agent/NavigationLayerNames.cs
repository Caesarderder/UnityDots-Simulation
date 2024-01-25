using System;
using UnityEngine;

namespace ProjectDawn.Navigation
{
    [Serializable]
    public class NavigationLayerNames
    {
        static readonly string[] DefaultNames = new[] { "Default" };

        [SerializeField]
        string[] m_Names = Array.Empty<string>();

        public string[] Names => m_Names.Length > 0 ? m_Names : DefaultNames;
    }
}
