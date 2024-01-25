using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.URP
{
    //[CreateAssetMenu(menuName = "Griffin/URP Resources")]
    public class GGriffinUrpResources : ScriptableObject
    {
        private static GGriffinUrpResources instance;
        public static GGriffinUrpResources Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GGriffinUrpResources>("GriffinUrpResources");
                }
                return instance;
            }
        }

        [Header("Terrain Material")]
        [SerializeField]
        private Material terrain4SplatsMaterial;
        public Material Terrain4SplatsMaterial
        {
            get
            {
                return terrain4SplatsMaterial;
            }
            set
            {
                terrain4SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrain4Splats4NormalsMaterial;
        public Material Terrain4Splats4NormalsMaterial
        {
            get
            {
                return terrain4Splats4NormalsMaterial;
            }
            set
            {
                terrain4Splats4NormalsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrain8SplatsMaterial;
        public Material Terrain8SplatsMaterial
        {
            get
            {
                return terrain8SplatsMaterial;
            }
            set
            {
                terrain8SplatsMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainGradientLookupMaterial;
        public Material TerrainGradientLookupMaterial
        {
            get
            {
                return terrainGradientLookupMaterial;
            }
            set
            {
                terrainGradientLookupMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainVertexColorMaterial;
        public Material TerrainVertexColorMaterial
        {
            get
            {
                return terrainVertexColorMaterial;
            }
            set
            {
                terrainVertexColorMaterial = value;
            }
        }

        [SerializeField]
        private Material terrainColorMapMaterial;
        public Material TerrainColorMapMaterial
        {
            get
            {
                return terrainColorMapMaterial;
            }
            set
            {
                terrainColorMapMaterial = value;
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
    }
}
