#if GRIFFIN
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    [AddComponentMenu("")]
    public abstract class GSpawnFilter : MonoBehaviour
    {
        private static List<Type> allFilters;
        public static List<Type> AllFilters 
        {
            get
            {
                if (allFilters == null)
                    allFilters = new List<Type>();
                return allFilters;
            }
            internal set
            {
                allFilters = value;
            }
        }

        [SerializeField]
        private bool ignore;
        public bool Ignore
        {
            get
            {
                return ignore;
            }
            set
            {
                ignore = value;
            }
        }

        public abstract void Apply(ref GSpawnFilterArgs args);
    }
}
#endif
