#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GColorMapGeneratorParams
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
    }
}
#endif
