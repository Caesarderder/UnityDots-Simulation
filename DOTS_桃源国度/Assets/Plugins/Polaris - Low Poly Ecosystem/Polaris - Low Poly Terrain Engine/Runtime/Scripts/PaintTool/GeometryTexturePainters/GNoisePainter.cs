#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using Pinwheel.Griffin.TextureTool;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    public class GNoisePainter : IGTexturePainter, IGTexturePainterWithCustomParams, IGTexturePainterWithLivePreview
    {
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MASK = Shader.PropertyToID("_Mask");
        private static readonly int OPACITY = Shader.PropertyToID("_Opacity");
        private static readonly int NOISE_MAP = Shader.PropertyToID("_NoiseMap");
        private static readonly int TERRAIN_MASK = Shader.PropertyToID("_TerrainMask");
        private static readonly string WORLD_SPACE_KW = "USE_WORLD_SPACE";

        private static Material painterMaterial;
        public static Material PainterMaterial
        {
            get
            {
                if (painterMaterial == null)
                {
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.noisePainterShader);
                }
                return painterMaterial;
            }
        }


        public string HistoryPrefix
        {
            get
            {
                return "Noise Painting";
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Raise or lower terrain surface using noise.\n" +
                    "   - Hold Left Mouse to raise.\n" +
                    "   - Hold Ctrl & Left Mouse to lower.");
                return s;
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args)
        {
            return GCommon.HeightMapAndFoliageResourceFlags;
        }

        public void BeginPainting(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                return;
            }
            if (args.MouseEventType == GPainterMouseEventType.Down)
            {
                terrain.ForceLOD(0);
                GRuntimeSettings.Instance.isEditingGeometry = true;
            }

            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);

            int heightMapResolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, heightMapResolution);
            PaintOnRT(terrain, args, rt, uvCorners);

            RenderTexture.active = rt;
            terrain.TerrainData.Geometry.HeightMap.ReadPixels(
                new Rect(0, 0, heightMapResolution, heightMapResolution), 0, 0);
            terrain.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;

            terrain.TerrainData.Geometry.SetRegionDirty(dirtyRect);
            terrain.TerrainData.Foliage.SetTreeRegionDirty(dirtyRect);
            terrain.TerrainData.Foliage.SetGrassRegionDirty(dirtyRect);
        }

        private void PaintOnRT(GStylizedTerrain terrain, GTexturePainterArgs args, RenderTexture rt, Vector2[] uvCorners)
        {
            Texture2D bg = terrain.TerrainData.Geometry.HeightMap;
            GCommon.CopyToRT(bg, rt);

            int heightMapResolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture noiseMap = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, heightMapResolution, 1);
            RenderNoiseTexture(noiseMap, terrain);

            Material mat = PainterMaterial;
            mat.SetTexture(MAIN_TEX, bg);
            mat.SetTexture(MASK, args.BrushMask);
            mat.SetFloat(OPACITY, Mathf.Pow(args.Opacity, GTerrainTexturePainter.GEOMETRY_OPACITY_EXPONENT));
            mat.SetTexture(NOISE_MAP, noiseMap);
            if (args.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, terrain.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }
            GCommon.SetMaterialKeywordActive(mat, WORLD_SPACE_KW, GTexturePainterCustomParams.Instance.Noise.UseWorldSpace);
            int pass =
                args.ActionType == GPainterActionType.Normal ? 0 :
                args.ActionType == GPainterActionType.Negative ? 1 : 0;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);
        }

        public void EndPainting(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                terrain.ForceLOD(-1);
                GRuntimeSettings.Instance.isEditingGeometry = false;
                terrain.UpdateTreesPosition();
                terrain.UpdateGrassPatches();
                terrain.TerrainData.Foliage.ClearTreeDirtyRegions();
                terrain.TerrainData.Foliage.ClearGrassDirtyRegions();
            }
        }

        public void Editor_DrawCustomParamsGUI()
        {
#if UNITY_EDITOR
            string label = "Noise Painting";
            string id = "noise-painter";

            GCommonGUI.Foldout(label, true, id, () =>
            {
                GNoisePainterParams param = GTexturePainterCustomParams.Instance.Noise;

                EditorGUILayout.GetControlRect(GUILayout.Height(1));
                Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(200));
                RenderTexture preview = GTerrainTexturePainter.Internal_GetRenderTexture(null, 200, 0);
                RenderNoiseTexture(preview, null);
                EditorGUI.DrawPreviewTexture(r, preview, null, ScaleMode.ScaleToFit);
                EditorGUILayout.GetControlRect(GUILayout.Height(1));

                param.Type = (GNoiseType)EditorGUILayout.EnumPopup("Type", param.Type);
                param.Origin = GCommonGUI.InlineVector2Field("Origin", param.Origin);
                param.Frequency = EditorGUILayout.FloatField("Frequency", param.Frequency);
                param.Lacunarity = EditorGUILayout.FloatField("Lacunarity", param.Lacunarity);
                param.Persistence = EditorGUILayout.FloatField("Persistence", param.Persistence);
                param.Octaves = EditorGUILayout.IntField("Octaves", param.Octaves);
                param.Seed = EditorGUILayout.FloatField("Seed", param.Seed);
                param.UseWorldSpace = EditorGUILayout.Toggle("World Space", param.UseWorldSpace);

                GTexturePainterCustomParams.Instance.Noise = param;

                EditorUtility.SetDirty(GTexturePainterCustomParams.Instance);
            });

#endif
        }

        public void Editor_DrawLivePreview(GStylizedTerrain terrain, GTexturePainterArgs args, Camera cam)
        {
#if UNITY_EDITOR
            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            int heightMapResolution = terrain.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, heightMapResolution);
            PaintOnRT(terrain, args, rt, uvCorners);

            GLivePreviewDrawer.DrawGeometryLivePreview(terrain, cam, rt, dirtyRect);
#endif
        }

        private void RenderNoiseTexture(RenderTexture noiseMap, GStylizedTerrain terrain)
        {
            GNoisePainterParams param = GTexturePainterCustomParams.Instance.Noise;
            GNoiseMapGenerator gen = new GNoiseMapGenerator();
            GNoiseMapGeneratorParams genParam = new GNoiseMapGeneratorParams();
            genParam.Type = param.Type;
            if (param.UseWorldSpace && terrain != null)
            {
                Vector3 terrainSize = new Vector3(
                   terrain.TerrainData.Geometry.Width,
                   terrain.TerrainData.Geometry.Height,
                   terrain.TerrainData.Geometry.Length);
                Vector3 terrainPos = terrain.transform.position;

                if (genParam.Type == GNoiseType.Voronoi)
                {
                    genParam.Origin = param.Origin + new Vector2(terrainPos.x / terrainSize.x, terrainPos.z / terrainSize.z);
                }
                else
                {
                    genParam.Origin = param.Origin + new Vector2(terrainPos.x * param.Frequency / terrainSize.x, terrainPos.z * param.Frequency / terrainSize.z);
                }
            }
            else
            {
                genParam.Origin = param.Origin;
            }
            genParam.Frequency = param.Frequency;
            genParam.Lacunarity = param.Lacunarity;
            genParam.Persistence = param.Persistence;
            genParam.Octaves = param.Octaves;
            genParam.Seed = param.Seed;
            gen.Generate(noiseMap, genParam);
        }
    }
}
#endif
