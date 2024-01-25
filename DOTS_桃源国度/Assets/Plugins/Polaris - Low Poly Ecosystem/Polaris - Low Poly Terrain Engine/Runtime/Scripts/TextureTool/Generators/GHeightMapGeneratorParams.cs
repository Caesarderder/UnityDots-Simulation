#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GHeightMapGeneratorParams
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
        private bool useRealGeometry;
        public bool UseRealGeometry
        {
            get
            {
                return useRealGeometry;
            }
            set
            {
                useRealGeometry = value;
            }
        }
    }
}
#endif
