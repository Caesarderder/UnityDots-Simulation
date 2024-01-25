#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Pinwheel.Griffin.PaintTool
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class GSpawnFilterInitializer
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        public static void InitFilters()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            GSpawnFilter.AllFilters = loadedTypes.FindAll(
                t => t.IsSubclassOf(typeof(GSpawnFilter)));
        }
    }
}
#endif
