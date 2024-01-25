#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    public class GElevationPainter : IGTexturePainter, IGTexturePainterWithLivePreview
    {
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");

        private static readonly int MAIN_TEX_L = Shader.PropertyToID("_MainTex_Left");
        private static readonly int MAIN_TEX_TL = Shader.PropertyToID("_MainTex_TopLeft");
        private static readonly int MAIN_TEX_T = Shader.PropertyToID("_MainTex_Top");
        private static readonly int MAIN_TEX_TR = Shader.PropertyToID("_MainTex_TopRight");
        private static readonly int MAIN_TEX_R = Shader.PropertyToID("_MainTex_Right");
        private static readonly int MAIN_TEX_RB = Shader.PropertyToID("_MainTex_BottomRight");
        private static readonly int MAIN_TEX_B = Shader.PropertyToID("_MainTex_Bottom");
        private static readonly int MAIN_TEX_BL = Shader.PropertyToID("_MainTex_BottomLeft");

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
                    painterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.elevationPainterShader);
                }
                return painterMaterial;
            }
        }

        public string Instruction
        {
            get
            {
                string s = string.Format(
                    "Gradually modifying terrain geometry.\n" +
                    "   - Hold Left Mouse to raise.\n" +
                    "   - Hold {0} & Left Mouse to lower.\n" +
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
                return "Elevation Painting";
            }
        }

        public List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args)
        {
            return GCommon.HeightMapAndFoliageResourceFlags;
        }

        private void SetupTextureGrid(GStylizedTerrain t, Material mat)
        {
            mat.SetTexture(MAIN_TEX_L,
                t.LeftNeighbor && t.LeftNeighbor.TerrainData ?
                t.LeftNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_TL,
                t.LeftNeighbor && t.LeftNeighbor.TopNeighbor && t.LeftNeighbor.TopNeighbor.TerrainData ?
                t.LeftNeighbor.TopNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);
            mat.SetTexture(MAIN_TEX_TL,
                t.TopNeighbor && t.TopNeighbor.LeftNeighbor && t.TopNeighbor.LeftNeighbor.TerrainData ?
                t.TopNeighbor.LeftNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_T,
                t.TopNeighbor && t.TopNeighbor.TerrainData ?
                t.TopNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_TR,
                t.RightNeighbor && t.RightNeighbor.TopNeighbor && t.RightNeighbor.TopNeighbor.TerrainData ?
                t.RightNeighbor.TopNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);
            mat.SetTexture(MAIN_TEX_TR,
                t.TopNeighbor && t.TopNeighbor.RightNeighbor && t.TopNeighbor.RightNeighbor.TerrainData ?
                t.TopNeighbor.RightNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_R,
                t.RightNeighbor && t.RightNeighbor.TerrainData ?
                t.RightNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_RB,
                t.RightNeighbor && t.RightNeighbor.BottomNeighbor && t.RightNeighbor.BottomNeighbor.TerrainData ?
                t.RightNeighbor.BottomNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);
            mat.SetTexture(MAIN_TEX_RB,
                t.BottomNeighbor && t.BottomNeighbor.RightNeighbor && t.BottomNeighbor.RightNeighbor.TerrainData ?
                t.BottomNeighbor.RightNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_B,
                t.BottomNeighbor && t.BottomNeighbor.TerrainData ?
                t.BottomNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);

            mat.SetTexture(MAIN_TEX_BL,
                t.LeftNeighbor && t.LeftNeighbor.BottomNeighbor && t.LeftNeighbor.BottomNeighbor.TerrainData ?
                t.LeftNeighbor.BottomNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);
            mat.SetTexture(MAIN_TEX_BL,
                t.BottomNeighbor && t.BottomNeighbor.LeftNeighbor && t.BottomNeighbor.LeftNeighbor.TerrainData ?
                t.BottomNeighbor.LeftNeighbor.TerrainData.Geometry.HeightMap :
                Texture2D.blackTexture);
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
            SetupTextureGrid(terrain, mat);
            mat.SetTexture(MASK, args.BrushMask);
            mat.SetFloat(OPACITY, Mathf.Pow(args.Opacity, GTerrainTexturePainter.GEOMETRY_OPACITY_EXPONENT));
            if (args.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, terrain.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }

            int pass =
                args.ActionType == GPainterActionType.Normal ? 0 :
                args.ActionType == GPainterActionType.Negative ? 1 :
                args.ActionType == GPainterActionType.Alternative ? 2 : 0;
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
