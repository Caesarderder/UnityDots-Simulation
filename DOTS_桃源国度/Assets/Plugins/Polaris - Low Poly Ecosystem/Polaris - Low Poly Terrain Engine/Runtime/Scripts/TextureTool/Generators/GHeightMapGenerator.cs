#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GHeightMapGenerator : IGTextureGenerator
    {
        public void Generate(RenderTexture targetRt)
        {
            GHeightMapGeneratorParams param = GTextureToolParams.Instance.HeightMap;
            if (param.Terrain == null || param.Terrain.TerrainData == null)
            {
                GCommon.CopyToRT(Texture2D.blackTexture, targetRt);
            }
            else
            {
                if (param.UseRealGeometry)
                {
                    RenderGeometryHeightMap(param.Terrain, targetRt);
                }
                else
                {
                    RenderPixelHeightMap(param.Terrain, targetRt);
                }
            }
        }

        private void RenderGeometryHeightMap(GStylizedTerrain terrain, RenderTexture targetRt)
        {
            GGeneralParams param = GTextureToolParams.Instance.General;
            RenderTexture rt = terrain.GetHeightMap(param.Resolution);
            GCommon.CopyToRT(rt, targetRt);
        }

        private void RenderPixelHeightMap(GStylizedTerrain terrain, RenderTexture targetRt)
        {
            Texture2D source = terrain.TerrainData.Geometry.HeightMap;
            RenderTexture.active = targetRt;
            Material mat = GInternalMaterials.HeightmapDecodeGrayscaleMaterial;
            mat.SetTexture("_MainTex", source);
            Graphics.Blit(source, mat);
            RenderTexture.active = null;
        }
    }
}
#endif
