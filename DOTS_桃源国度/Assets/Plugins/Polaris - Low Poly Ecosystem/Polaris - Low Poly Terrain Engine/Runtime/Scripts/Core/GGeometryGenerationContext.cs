#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GGeometryGenerationContext
    {
        private Color[] heightMapData;
        private int heightMapResolution;
        private Color[] subDivMapData;
        private int subDivMapResolution;

        public GGeometryGenerationContext(GStylizedTerrain terrain, GTerrainData data)
        {
            subDivMapData = data.Geometry.Internal_SubDivisionMap.GetPixels();
            subDivMapResolution = GCommon.SUB_DIV_MAP_RESOLUTION;
        }

        public Color GetSubdivData(Vector2 uv)
        {
            return GUtilities.GetColorBilinear(subDivMapData, subDivMapResolution, subDivMapResolution, uv);
        }
    }
}
#endif
