#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    public class GMaskPainter : IGTexturePainter, IGTexturePainterWithCustomParams, IGTexturePainterWithLivePreview, IConditionalPainter
    {

        private static Material painterMaterial;
        public static Material PainterMaterial
        {
            get
            {
                if (painterMaterial == null)
                {
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.maskPainterShader);
                }
                return painterMaterial;
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Modify terrain mask.\n" +
                    "   - Hold Left Mouse to paint.\n" +
                    "   - Hold {0} & Left Mouse to erase.\n" +
                    "   - Hold {1} & Left Mouse to smooth.",
                    "Ctrl",
                    "Shift");
                return s;
            }
        }

        public string HistoryPrefix
        {
            get
            {
                return "Mask Painting";
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args)
        {
            return GCommon.MaskMapResourceFlags;
        }

        private void SetupTextureGrid(GStylizedTerrain t, Material mat)
        {
            mat.SetTexture("_MainTex_Left",
                t.LeftNeighbor && t.LeftNeighbor.TerrainData ?
                t.LeftNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_TopLeft",
                t.LeftNeighbor && t.LeftNeighbor.TopNeighbor && t.LeftNeighbor.TopNeighbor.TerrainData ?
                t.LeftNeighbor.TopNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);
            mat.SetTexture("_MainTex_TopLeft",
                t.TopNeighbor && t.TopNeighbor.LeftNeighbor && t.TopNeighbor.LeftNeighbor.TerrainData ?
                t.TopNeighbor.LeftNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_Top",
                t.TopNeighbor && t.TopNeighbor.TerrainData ?
                t.TopNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_TopRight",
                t.RightNeighbor && t.RightNeighbor.TopNeighbor && t.RightNeighbor.TopNeighbor.TerrainData ?
                t.RightNeighbor.TopNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);
            mat.SetTexture("_MainTex_TopRight",
                t.TopNeighbor && t.TopNeighbor.RightNeighbor && t.TopNeighbor.RightNeighbor.TerrainData ?
                t.TopNeighbor.RightNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_Right",
                t.RightNeighbor && t.RightNeighbor.TerrainData ?
                t.RightNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_BottomRight",
                t.RightNeighbor && t.RightNeighbor.BottomNeighbor && t.RightNeighbor.BottomNeighbor.TerrainData ?
                t.RightNeighbor.BottomNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);
            mat.SetTexture("_MainTex_BottomRight",
                t.BottomNeighbor && t.BottomNeighbor.RightNeighbor && t.BottomNeighbor.RightNeighbor.TerrainData ?
                t.BottomNeighbor.RightNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_Bottom",
                t.BottomNeighbor && t.BottomNeighbor.TerrainData ?
                t.BottomNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);

            mat.SetTexture("_MainTex_BottomLeft",
                t.LeftNeighbor && t.LeftNeighbor.BottomNeighbor && t.LeftNeighbor.BottomNeighbor.TerrainData ?
                t.LeftNeighbor.BottomNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);
            mat.SetTexture("_MainTex_BottomLeft",
                t.BottomNeighbor && t.BottomNeighbor.LeftNeighbor && t.BottomNeighbor.LeftNeighbor.TerrainData ?
                t.BottomNeighbor.LeftNeighbor.TerrainData.Mask.MaskMapOrDefault :
                Texture2D.blackTexture);
        }

        public void BeginPainting(GStylizedTerrain terrain, GTexturePainterArgs args)
        {
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                return;
            }

            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);

            int maskResolution = terrain.TerrainData.Mask.MaskMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, maskResolution);
            PaintOnRT(terrain, args, rt, uvCorners);

            RenderTexture.active = rt;
            terrain.TerrainData.Mask.MaskMap.ReadPixels(
                new Rect(0, 0, maskResolution, maskResolution), 0, 0);
            terrain.TerrainData.Mask.MaskMap.Apply();
            RenderTexture.active = null;

            if (args.ForceUpdateGeometry)
            {
                terrain.TerrainData.Geometry.SetRegionDirty(dirtyRect);
            }
        }

        private void PaintOnRT(GStylizedTerrain terrain, GTexturePainterArgs args, RenderTexture rt, Vector2[] uvCorners)
        {
            Texture2D bg = terrain.TerrainData.Mask.MaskMapOrDefault;
            GCommon.CopyToRT(bg, rt);

            Material mat = PainterMaterial;
            mat.SetTexture("_MainTex", bg);
            SetupTextureGrid(terrain, mat);
            mat.SetTexture("_Mask", args.BrushMask);
            mat.SetFloat("_Opacity", args.Opacity);
            GMaskPainterParams param = GTexturePainterCustomParams.Instance.Mask;
            Vector4 channel;
            if (param.Channel == GTextureChannel.R)
            {
                channel = new Vector4(1, 0, 0, 0);
            }
            else if (param.Channel == GTextureChannel.G)
            {
                channel = new Vector4(0, 1, 0, 0);
            }
            else if (param.Channel == GTextureChannel.B)
            {
                channel = new Vector4(0, 0, 1, 0);
            }
            else
            {
                channel = new Vector4(0, 0, 0, 1);
            }
            mat.SetVector("_Channel", channel);
            args.ConditionalPaintingConfigs.SetupMaterial(terrain, mat);

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
            if (GTexturePainterCustomParams.Instance.Mask.Visualize == false)
                return;

            Vector2[] uvCorners = GPaintToolUtilities.WorldToUvCorners(terrain, args.WorldPointCorners);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);

            int maskResolution = terrain.TerrainData.Mask.MaskMapResolution;
            RenderTexture rt = GTerrainTexturePainter.Internal_GetRenderTexture(terrain, maskResolution);
            PaintOnRT(terrain, args, rt, uvCorners);

            GLivePreviewDrawer.DrawMask4ChannelsLivePreview(
                terrain, cam,
                rt,
                GCommon.UnitRect);
#endif
        }

        public void Editor_DrawCustomParamsGUI()
        {
#if UNITY_EDITOR
            string label = "Mask Painting";
            string id = "mask-painter";

            GCommonGUI.Foldout(label, true, id, () =>
            {
                GMaskPainterParams param = GTexturePainterCustomParams.Instance.Mask;
                string[] labels = new string[]
                {
                    "R (Terrain Mask)",
                    "G (Smooth Normal Mask)",
                    "B (Water Source)",
                    "A"
                };
                int[] values = new int[]
                {
                    1,
                    2,
                    4,
                    8
                };

                param.Channel = (GTextureChannel)EditorGUILayout.IntPopup("Channel", (int)param.Channel, labels, values);
                param.Visualize = EditorGUILayout.Toggle("Visualize", param.Visualize);
                GTexturePainterCustomParams.Instance.Mask = param;
                EditorUtility.SetDirty(GTexturePainterCustomParams.Instance);
            });
#endif
        }
    }
}
#endif
