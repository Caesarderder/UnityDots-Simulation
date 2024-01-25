#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GColorMapGenerator : IGTextureGenerator
    {
        public void Generate(RenderTexture targetRt)
        {
            GColorMapGeneratorParams param = GTextureToolParams.Instance.ColorMap;
            if (param.Terrain == null || param.Terrain.TerrainData == null)
            {
                GCommon.FillTexture(targetRt, Color.black);
            }
            else
            {
                RenderColorMap(param.Terrain, targetRt);
            }
        }

        private void RenderColorMap(GStylizedTerrain t, RenderTexture targetRt)
        {
            GShading shading = t.TerrainData.Shading;
            if (shading.Splats == null)
                return;
            Material mat = GInternalMaterials.SplatsToAlbedoMaterial;

            for (int i = 0; i < shading.SplatControlMapCount; ++i)
            {
                Texture2D controlMap = shading.GetSplatControlOrDefault(i);
                mat.SetTexture("_Control0", controlMap);
                for (int channel = 0; channel < 4; ++channel)
                {
                    int prototypeIndex = i * 4 + channel;
                    if (prototypeIndex < shading.Splats.Prototypes.Count)
                    {
                        GSplatPrototype p = shading.Splats.Prototypes[prototypeIndex];
                        mat.SetTexture("_Splat" + channel, p.Texture);
                        Vector2 terrainSize = new Vector2(t.TerrainData.Geometry.Width, t.TerrainData.Geometry.Length);
                        Vector2 textureScale = new Vector2(
                            p.TileSize.x != 0 ? terrainSize.x / p.TileSize.x : 0,
                            p.TileSize.y != 0 ? terrainSize.y / p.TileSize.y : 0);
                        Vector2 textureOffset = new Vector2(
                            p.TileOffset.x != 0 ? terrainSize.x / p.TileOffset.x : 0,
                            p.TileOffset.y != 0 ? terrainSize.y / p.TileOffset.y : 0);
                        mat.SetTextureScale("_Splat" + channel, textureScale);
                        mat.SetTextureOffset("_Splat" + channel, textureOffset);
                    }
                    else
                    {
                        mat.SetTexture("_Splat" + channel, null);
                        mat.SetTextureScale("_Splat" + channel, Vector2.zero);
                        mat.SetTextureOffset("_Splat" + channel, Vector2.zero);
                    }
                }

                GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, mat, 0);
            }
        }
    }
}
#endif
