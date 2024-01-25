#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GFoliageDistributionMapGeneratorParams
    {
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private List<int> treePrototypeIndices;
        public List<int> TreePrototypeIndices
        {
            get
            {
                if (treePrototypeIndices == null)
                {
                    treePrototypeIndices = new List<int>();
                }
                return treePrototypeIndices;
            }
            set
            {
                treePrototypeIndices = value;
            }
        }

        [SerializeField]
        private List<int> grassPrototypeIndices;
        public List<int> GrassPrototypeIndices
        {
            get
            {
                if (grassPrototypeIndices == null)
                {
                    grassPrototypeIndices = new List<int>();
                }
                return grassPrototypeIndices;
            }
            set
            {
                grassPrototypeIndices = value;
            }
        }

        [SerializeField]
        private float size;
        public float Size
        {
            get
            {
                return size;
            }
            set
            {
                size = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float opacity;
        public float Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                opacity = Mathf.Clamp01(value);
            }
        }

        private Texture2D brushMask;
        public Texture2D BrushMask
        {
            get
            {
                return brushMask;
            }
            set
            {
                brushMask = value;
            }
        }

        [SerializeField]
        private float rotationMin;
        public float RotationMin
        {
            get
            {
                return rotationMin;
            }
            set
            {
                rotationMin = value;
            }
        }

        [SerializeField]
        private float rotationMax;
        public float RotationMax
        {
            get
            {
                return rotationMax;
            }
            set
            {
                rotationMax = value;
            }
        }
    }
}
#endif
