#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.BuiltinRP
{
    //[CreateAssetMenu(menuName = "Griffin/BRP Resources")]
    public class GGriffinBrpResources : ScriptableObject
    {
        private static GGriffinBrpResources instance;
        public static GGriffinBrpResources Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GGriffinBrpResources>("GriffinBrpResources");
                }
                return instance;
            }
        }

        [Header("Terrain PBR Material")]
        [SerializeField]
        private Material terrainPbr4SplatsMaterial;
        public Material TerrainPbr4SplatsMaterial
        {
            get
            {
                return terrainPbr4SplatsMaterial;
            }
            set
            {
                terrainPbr4SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainPbr4Splats4NormalsMaterial;
        public Material TerrainPbr4Splats4NormalsMaterial
        {
            get
            {
                return terrainPbr4Splats4NormalsMaterial;
            }
            set
            {
                terrainPbr4Splats4NormalsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainPbr8SplatsMaterial;
        public Material TerrainPbr8SplatsMaterial
        {
            get
            {
                return terrainPbr8SplatsMaterial;
            }
            set
            {
                terrainPbr8SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainPbrGradientLookupMaterial;
        public Material TerrainPbrGradientLookupMaterial
        {
            get
            {
                return terrainPbrGradientLookupMaterial;
            }
            set
            {
                terrainPbrGradientLookupMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainPbrVertexColorMaterial;
        public Material TerrainPbrVertexColorMaterial
        {
            get
            {
                return terrainPbrVertexColorMaterial;
            }
            set
            {
                terrainPbrVertexColorMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainPbrColorMapMaterial;
        public Material TerrainPbrColorMapMaterial
        {
            get
            {
                return terrainPbrColorMapMaterial;
            }
            set
            {
                terrainPbrColorMapMaterial = value;
            }
        }

        [Header("Terrain Lambert Material")]
        [SerializeField]
        private Material terrainLambert4SplatsMaterial;
        public Material TerrainLambert4SplatsMaterial
        {
            get
            {
                return terrainLambert4SplatsMaterial;
            }
            set
            {
                terrainLambert4SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainLambert4Splats4NormalsMaterial;
        public Material TerrainLambert4Splats4NormalsMaterial
        {
            get
            {
                return terrainLambert4Splats4NormalsMaterial;
            }
            set
            {
                terrainLambert4Splats4NormalsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainLambert8SplatsMaterial;
        public Material TerrainLambert8SplatsMaterial
        {
            get
            {
                return terrainLambert8SplatsMaterial;
            }
            set
            {
                terrainLambert8SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainLambertGradientLookupMaterial;
        public Material TerrainLambertGradientLookupMaterial
        {
            get
            {
                return terrainLambertGradientLookupMaterial;
            }
            set
            {
                terrainLambertGradientLookupMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainLambertVertexColorMaterial;
        public Material TerrainLambertVertexColorMaterial
        {
            get
            {
                return terrainLambertVertexColorMaterial;
            }
            set
            {
                terrainLambertVertexColorMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainLambertColorMapMaterial;
        public Material TerrainLambertColorMapMaterial
        {
            get
            {
                return terrainLambertColorMapMaterial;
            }
            set
            {
                terrainLambertColorMapMaterial = value;
            }
        }

        [Header("Terrain Blinn-Phong Material")]
        [SerializeField]
        private Material terrainBlinnPhong4SplatsMaterial;
        public Material TerrainBlinnPhong4SplatsMaterial
        {
            get
            {
                return terrainBlinnPhong4SplatsMaterial;
            }
            set
            {
                terrainBlinnPhong4SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainBlinnPhong4Splats4NormalsMaterial;
        public Material TerrainBlinnPhong4Splats4NormalsMaterial
        {
            get
            {
                return terrainBlinnPhong4Splats4NormalsMaterial;
            }
            set
            {
                terrainBlinnPhong4Splats4NormalsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainBlinnPhong8SplatsMaterial;
        public Material TerrainBlinnPhong8SplatsMaterial
        {
            get
            {
                return terrainBlinnPhong8SplatsMaterial;
            }
            set
            {
                terrainBlinnPhong8SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainBlinnPhongGradientLookupMaterial;
        public Material TerrainBlinnPhongGradientLookupMaterial
        {
            get
            {
                return terrainBlinnPhongGradientLookupMaterial;
            }
            set
            {
                terrainBlinnPhongGradientLookupMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainBlinnPhongVertexColorMaterial;
        public Material TerrainBlinnPhongVertexColorMaterial
        {
            get
            {
                return terrainBlinnPhongVertexColorMaterial;
            }
            set
            {
                terrainBlinnPhongVertexColorMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainBlinnPhongColorMapMaterial;
        public Material TerrainBlinnPhongColorMapMaterial
        {
            get
            {
                return terrainBlinnPhongColorMapMaterial;
            }
            set
            {
                terrainBlinnPhongColorMapMaterial = value;
            }
        }

        [Header("Foliage Material")]
        [SerializeField]
        private Material grassMaterial;
        public Material GrassMaterial
        {
            get
            {
                return grassMaterial;
            }
            set
            {
                grassMaterial = value;
            }
        }

        [SerializeField]
        private Material grassBillboardMaterial;
        public Material GrassBillboardMaterial
        {
            get
            {
                return grassBillboardMaterial;
            }
            set
            {
                grassBillboardMaterial = value;
            }
        }

        [SerializeField]
        private Material grassInteractiveMaterial;
        public Material GrassInteractiveMaterial
        {
            get
            {
                return grassInteractiveMaterial;
            }
            set
            {
                grassInteractiveMaterial = value;
            }
        }

        [SerializeField]
        private Material treeBillboardMaterial;
        public Material TreeBillboardMaterial
        {
            get
            {
                return treeBillboardMaterial;
            }
            set
            {
                treeBillboardMaterial = value;
            }
        }

        [SerializeField]
        private Material grassPreviewMaterial;
        public Material GrassPreviewMaterial
        {
            get
            {
                return grassPreviewMaterial;
            }
            set
            {
                grassPreviewMaterial = value;
            }
        }
    }
}
#endif
