#if GRIFFIN
using Pinwheel.Griffin.TextureTool;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    public static class GStampLayerMaskRenderer
    {
        private const string BLEND_HEIGHT_KW = "BLEND_HEIGHT";
        private const string BLEND_SLOPE_KW = "BLEND_SLOPE";
        private const string BLEND_NOISE_KW = "BLEND_NOISE";

        public static void Render(
            RenderTexture rt,
            GConditionalStampLayer layer,
            GStylizedTerrain terrain,
            Matrix4x4 stamperTransform,
            Texture mask,
            Texture falloffTexture,
            Vector2[] uvPoints,
            bool useTerrainMask)
        {
            GCommon.ClearRT(rt);
            if (layer.Ignore)
            {
                return;
            }

            Material brushMat = Object.Instantiate(GInternalMaterials.TextureStamperBrushMaterial);

            int resolution = Mathf.Max(rt.width, rt.height);
            //no need to release these maps
            RenderTexture heightMap = terrain.GetHeightMap(resolution);
            RenderTexture normalMap =
                layer.NormalMapMode == GNormalMapMode.Sharp ? terrain.GetSharpNormalMap(resolution) :
                layer.NormalMapMode == GNormalMapMode.Interpolated ? terrain.GetInterpolatedNormalMap(resolution) :
                layer.NormalMapMode == GNormalMapMode.PerPixel ? terrain.GetPerPixelNormalMap(resolution) : null;

            brushMat.SetTexture("_HeightMap", heightMap);

            Vector3 position = stamperTransform.MultiplyPoint(Vector3.zero);
            Vector3 scale = stamperTransform.lossyScale;

            float stamperMinHeight = terrain.WorldPointToNormalized(position).y;
            float stamperMaxHeight = terrain.WorldPointToNormalized(position + Vector3.up * scale.y).y;
            brushMat.SetFloat("_StamperMinHeight", stamperMinHeight);
            brushMat.SetFloat("_StamperMaxHeight", stamperMaxHeight);

            Vector3 terrainSize = new Vector3(
                terrain.TerrainData.Geometry.Width,
                terrain.TerrainData.Geometry.Height,
                terrain.TerrainData.Geometry.Length);

            brushMat.SetTexture("_Mask", mask);
            brushMat.SetTexture("_Falloff", falloffTexture);
            brushMat.SetFloat("_MinHeight", GUtilities.InverseLerpUnclamped(0, terrainSize.y, layer.MinHeight));
            brushMat.SetFloat("_MaxHeight", GUtilities.InverseLerpUnclamped(0, terrainSize.y, layer.MaxHeight));
            brushMat.SetTexture("_HeightTransition", layer.heightTransitionTexture);
            brushMat.SetFloat("_MinSlope", layer.MinSlope * Mathf.Deg2Rad);
            brushMat.SetFloat("_MaxSlope", layer.MaxSlope * Mathf.Deg2Rad);
            brushMat.SetTexture("_SlopeTransition", layer.slopeTransitionTexture);
            brushMat.SetVector("_NoiseOrigin", layer.NoiseOrigin);
            brushMat.SetFloat("_NoiseFrequency", layer.NoiseFrequency);
            brushMat.SetInt("_NoiseOctaves", layer.NoiseOctaves);
            brushMat.SetFloat("_NoiseLacunarity", layer.NoiseLacunarity);
            brushMat.SetFloat("_NoisePersistence", layer.NoisePersistence);
            brushMat.SetTexture("_NoiseRemap", layer.noiseRemapTexture);
            brushMat.SetTexture("_NormalMap", normalMap);
            if (useTerrainMask)
            {
                brushMat.SetTexture("_TerrainMask", terrain.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                brushMat.SetTexture("_TerrainMask", Texture2D.blackTexture);
            }

            GCommon.SetMaterialKeywordActive(brushMat, BLEND_HEIGHT_KW, layer.BlendHeight);
            GCommon.SetMaterialKeywordActive(brushMat, BLEND_SLOPE_KW, layer.BlendSlope);
            GCommon.SetMaterialKeywordActive(brushMat, BLEND_NOISE_KW, layer.BlendNoise);
            DrawOnBrushTexture(rt, uvPoints, brushMat, 0);
        }

        private static void DrawOnBrushTexture(RenderTexture rt, Vector2[] quadCorners, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.MultiTexCoord(0, new Vector3(0, 0, 0));
            GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0);

            GL.MultiTexCoord(0, new Vector3(0, 1, 0));
            GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0);

            GL.MultiTexCoord(0, new Vector3(1, 1, 0));
            GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0);

            GL.MultiTexCoord(0, new Vector3(1, 0, 0));
            GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }
    }
}
#endif
