#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    //[CreateAssetMenu(menuName = "Griffin/Texture Tool Params")]
    public partial class GTextureToolParams : ScriptableObject
    {
        private static GTextureToolParams instance;
        public static GTextureToolParams Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GTextureToolParams>("TextureToolParams");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GTextureToolParams>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private GGeneralParams general;
        public GGeneralParams General
        {
            get
            {
                return general;
            }
            set
            {
                general = value;
            }
        }

        [SerializeField]
        private GLivePreviewParams livePreview;
        public GLivePreviewParams LivePreview
        {
            get
            {
                return livePreview;
            }
            set
            {
                livePreview = value;
            }
        }

        [SerializeField]
        private GHeightMapGeneratorParams heightMap;
        public GHeightMapGeneratorParams HeightMap
        {
            get
            {
                return heightMap;
            }
            set
            {
                heightMap = value;
            }
        }

        [SerializeField]
        private GHeightMapFromMeshGeneratorParams heightMapFromMesh;
        public GHeightMapFromMeshGeneratorParams HeightMapFromMesh
        {
            get
            {
                return heightMapFromMesh;
            }
            set
            {
                heightMapFromMesh = value;
            }
        }

        [SerializeField]
        private GNormalMapGeneratorParams normalMap;
        public GNormalMapGeneratorParams NormalMap
        {
            get
            {
                return normalMap;
            }
            set
            {
                normalMap = value;
            }
        }

        [SerializeField]
        private GSteepnessMapGeneratorParams steepness;
        public GSteepnessMapGeneratorParams Steepness
        {
            get
            {
                return steepness;
            }
            set
            {
                steepness = value;
            }
        }

        [SerializeField]
        private GNoiseMapGeneratorParams noise;
        public GNoiseMapGeneratorParams Noise
        {
            get
            {
                return noise;
            }
            set
            {
                noise = value;
            }
        }

        [SerializeField]
        private GColorMapGeneratorParams colorMap;
        public GColorMapGeneratorParams ColorMap
        {
            get
            {
                return colorMap;
            }
            set
            {
                colorMap = value;
            }
        }

        [SerializeField]
        private GBlendMapGeneratorParams blend;
        public GBlendMapGeneratorParams Blend
        {
            get
            {
                return blend;
            }
            set
            {
                blend = value;
            }
        }

        [SerializeField]
        private GFoliageDistributionMapGeneratorParams treeDistribution;
        public GFoliageDistributionMapGeneratorParams TreeDistribution
        {
            get
            {
                return treeDistribution;
            }
            set
            {
                treeDistribution = value;
            }
        }

        [SerializeField]
        private List<GTextureFilterLayer> filters;
        public List<GTextureFilterLayer> Filters
        {
            get
            {
                if (filters == null)
                {
                    filters = new List<GTextureFilterLayer>();
                }
                return filters;
            }
            set
            {
                filters = value;
            }
        }
    }
}
#endif
