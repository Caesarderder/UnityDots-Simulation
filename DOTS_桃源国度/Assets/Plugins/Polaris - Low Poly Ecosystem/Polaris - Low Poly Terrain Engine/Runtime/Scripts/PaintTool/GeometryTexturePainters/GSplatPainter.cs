#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    public class GSplatPainter : IGTexturePainter, IGTexturePainterWithLivePreview, IConditionalPainter
    {
        private static readonly int CHANNEL_INDEX = Shader.PropertyToID("_ChannelIndex");
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
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
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splatPainterShader);
                }
                return painterMaterial;
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Paint blend weight on terrain Splat Control maps.\n" +
                    "   - Hold Left Mouse to paint.\n" +
                    "   - Hold Ctrl & Left Mouse to erase selected layer.\n" +
                    "   - Hold Shift & Left Mouse to erase all layer.\n" +
                    "Use a material that utilizes splat maps to see the result.");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Splat Painting";
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args)
        {
            return GCommon.SplatResourceFlags;
        }

        public void BeginPainting(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            if (terrain.TerrainData.Shading.Splats == null)
            {
                return;
            }
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                return;
            }
            int splatIndex = args.SplatIndex;
            if (splatIndex < 0 || splatIndex >= terrain.TerrainData.Shading.SplatControlMapCount * 4)
            {
                return;
            }

            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);

            int controlMapResolution = terrain.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = terrain.TerrainData.Shading.SplatControlMapCount;
            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D currentSplatControl = terrain.TerrainData.Shading.GetSplatControl(i);
                RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, controlMapResolution, i);
                PaintOnRT(terrain, args, rt, uvCorners, i);

                RenderTexture.active = rt;
                currentSplatControl.ReadPixels(new Rect(0, 0, controlMapResolution, controlMapResolution), 0, 0);
                currentSplatControl.Apply();
                RenderTexture.active = null;
            }

            terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            if (args.ForceUpdateGeometry)
            {
                terrain.TerrainData.Geometry.SetRegionDirty(dirtyRect);
            }
        }

        private void PaintOnRT(GStylizedTerrain terrain, GTexturePainterArgs args, RenderTexture rt, Vector2[] uvCorners, int currentSplatIndex)
        {
            Texture2D currentSplatControl = terrain.TerrainData.Shading.GetSplatControl(currentSplatIndex);
            GCommon.CopyToRT(currentSplatControl, rt);

            Material mat = PainterMaterial;
            mat.SetTexture(MASK, args.BrushMask);
            mat.SetFloat(OPACITY, args.Opacity);
            if (args.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, terrain.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }
            args.ConditionalPaintingConfigs.SetupMaterial(terrain, mat);

            mat.SetTexture(MAIN_TEX, currentSplatControl);
            int selectedSplatIndex = args.SplatIndex;
            if (selectedSplatIndex / 4 == currentSplatIndex)
            {
                mat.SetInt(CHANNEL_INDEX, selectedSplatIndex % 4);
            }
            else
            {
                mat.SetInt(CHANNEL_INDEX, -1);
            }
            int pass =
                args.ActionType == GPainterActionType.Normal ? 0 :
                args.ActionType == GPainterActionType.Negative ? 1 :
                args.ActionType == GPainterActionType.Alternative ? 2 : 0;
            GCommon.DrawQuad(rt, uvCorners, mat, pass);
        }

        public void EndPainting(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            if (args.ForceUpdateGeometry)
            {
                terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
            }
        }

        public void Editor_DrawLivePreview(GStylizedTerrain terrain, GTexturePainterArgs args, Camera cam)
        {
#if UNITY_EDITOR
            int splatIndex = args.SplatIndex;
            if (splatIndex < 0 || splatIndex >= terrain.TerrainData.Shading.SplatControlMapCount * 4)
            {
                return;
            }

            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);

            int controlMapResolution = terrain.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = terrain.TerrainData.Shading.SplatControlMapCount;
            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D currentSplatControl = terrain.TerrainData.Shading.GetSplatControl(i);
                RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, controlMapResolution, i);
                PaintOnRT(terrain, args, rt, uvCorners, i);
            }

            Texture[] controls = new Texture[controlMapCount];
            for (int i = 0; i < controlMapCount; ++i)
            {
                controls[i] = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, controlMapResolution, i);
            }

            GLivePreviewDrawer.DrawSplatLivePreview(terrain, cam, controls, dirtyRect);
#endif
        }
    }
}
#endif
