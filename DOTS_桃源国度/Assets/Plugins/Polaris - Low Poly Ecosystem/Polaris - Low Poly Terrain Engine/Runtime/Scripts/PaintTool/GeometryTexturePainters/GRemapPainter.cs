#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    public class GRemapPainter : IGTexturePainter, IGTexturePainterWithCustomParams, IGTexturePainterWithLivePreview
    {
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int REMAP_TEX = Shader.PropertyToID("_RemapTex");
        private static readonly int MASK = Shader.PropertyToID("_Mask");
        private static readonly int OPACITY = Shader.PropertyToID("_Opacity");
        private static readonly int TERRAIN_MASK = Shader.PropertyToID("_TerrainMask");
        
        private static Material painterMaterial;
        public static Material PainterMaterial
        {
            get
            {
                if (painterMaterial == null)
                {
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.remapPainterShader);
                }
                return painterMaterial;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Remap Painting";
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Remap evelation value by a curve.\n" +
                    "   - Use Left Mouse to paint.");
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

            Texture2D remapTex = GCommon.CreateTextureFromCurve(GTexturePainterCustomParams.Instance.Remap.Curve, 512, 1);
            Material mat = PainterMaterial;
            mat.SetTexture(MAIN_TEX, bg);
            mat.SetTexture(MASK, args.BrushMask);
            mat.SetFloat(OPACITY, Mathf.Pow(args.Opacity, GTerrainTexturePainter.GEOMETRY_OPACITY_EXPONENT));
            mat.SetTexture(REMAP_TEX, remapTex);
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
            GUtilities.DestroyObject(remapTex);
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
            string label = "Remap Painting";
            string id = "remap-painter";

            GCommonGUI.Foldout(label, true, id, () =>
            {
                GRemapPainterParams param = GTexturePainterCustomParams.Instance.Remap;
                param.Curve = EditorGUILayout.CurveField("Curve", param.Curve, Color.red, GCommon.UnitRect);
                GTexturePainterCustomParams.Instance.Remap = param;
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
