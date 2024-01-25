#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    public class GTerracePainter : IGTexturePainter, IGTexturePainterWithCustomParams, IGTexturePainterWithLivePreview
    {
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int MASK = Shader.PropertyToID("_Mask");
        private static readonly int OPACITY = Shader.PropertyToID("_Opacity");
        private static readonly int STEP_COUNT = Shader.PropertyToID("_StepCount");
        private static readonly int TERRAIN_MASK = Shader.PropertyToID("_TerrainMask");

        private static Material painterMaterial;
        public static Material PainterMaterial
        {
            get
            {
                if (painterMaterial == null)
                {
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.terracePainterShader);
                }
                return painterMaterial;
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Paint terrace (step) effect on terrain geometry.\n" +
                    "   - Use Left Mouse to paint.");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Terrace Painting";
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

            Material mat = PainterMaterial;
            mat.SetTexture(MAIN_TEX, bg);
            mat.SetTexture(MASK, args.BrushMask);
            mat.SetFloat(OPACITY, Mathf.Pow(args.Opacity, GTerrainTexturePainter.GEOMETRY_OPACITY_EXPONENT));
            mat.SetInt(STEP_COUNT, GTexturePainterCustomParams.Instance.Terrace.StepCount);
            if (args.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, terrain.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }
            int pass = 0;
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
            string label = "Terrace Painting";
            string id = "terrace-painter-params";

            GCommonGUI.Foldout(label, true, id, () =>
            {
                GTerracePainterParams param = GTexturePainterCustomParams.Instance.Terrace;
                param.StepCount = EditorGUILayout.IntField("Step Count", param.StepCount);
                GTexturePainterCustomParams.Instance.Terrace = param;
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
    }
}
#endif
