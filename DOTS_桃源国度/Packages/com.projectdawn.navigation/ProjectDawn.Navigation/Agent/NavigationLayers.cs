using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ProjectDawn.Navigation
{
    [System.Flags]
    public enum NavigationLayers
    {
        None = 0,
        Default = 1 << 0,
        Layer1 = 1 << 1,
        Layer2 = 1 << 2,
        Layer3 = 1 << 3,
        Layer4 = 1 << 4,
        Layer5 = 1 << 5,
        Layer6 = 1 << 6,
        Layer7 = 1 << 7,
        Layer8 = 1 << 8,
        Layer9 = 1 << 9,
        Layer10 = 1 << 10,
        Layer11 = 1 << 11,
        Layer12 = 1 << 12,
        Layer13 = 1 << 13,
        Layer14 = 1 << 14,
        Layer15 = 1 << 15,
        Layer16 = 1 << 16,
        Layer17 = 1 << 17,
        Layer18 = 1 << 18,
        Layer19 = 1 << 19,
        Layer20 = 1 << 20,
        Layer21 = 1 << 21,
        Layer22 = 1 << 22,
        Layer23 = 1 << 23,
        Layer24 = 1 << 24,
        Layer25 = 1 << 25,
        Layer26 = 1 << 26,
        Layer27 = 1 << 27,
        Layer28 = 1 << 28,
        Layer29 = 1 << 29,
        Layer30 = 1 << 30,
        Layer31 = 1 << 31,
        Everything = -1,
    }


    public static class NavigationLayerUtility
    {
        public static bool All(this NavigationLayers lhs, NavigationLayers rhs) => (lhs & rhs) == rhs;
        public static bool Any(this NavigationLayers lhs, NavigationLayers rhs) => (lhs & rhs) != 0;
        public static void Add(ref this NavigationLayers lhs, NavigationLayers rhs) => lhs |= rhs;
        public static bool TryGetLayerMask(string name, out NavigationLayers mask)
        {
            var layerNames = AgentsNavigationSettings.Get<SpatialPartitioningSubSettings>().LayerNames;
            for (int layer = 0; layer < layerNames.Length; layer++)
            {
                string layerName = layerNames[layer];
                if (layerName.Equals(name))
                {
                    mask = (NavigationLayers) layer;
                    return true;
                }
            }

            mask = NavigationLayers.None;
            return false;
        }
    }
}
