#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GTextureImporter
    {
        public GStylizedTerrain Terrain { get; set; }
        public GTerrainData DesData { get; set; }
        public Texture2D HeightMap { get; set; }
        public Texture2D MaskMap { get; set; }
        public Texture2D VisibilityMap { get; set; }
        public Texture2D AlbedoMap { get; set; }
        public Texture2D MetallicMap { get; set; }
        public Texture2D[] SplatControlMaps { get; set; }

        public void Import()
        {
            try
            {
#if UNITY_EDITOR
                GCommonGUI.ProgressBar("Working", "Importing textures...", 1f);
#endif
                if (HeightMap != null || VisibilityMap != null)
                {
                    Color[] oldHeightMapColors = DesData.Geometry.HeightMap.GetPixels();

                    Texture2D hm = null;
                    if (HeightMap != null)
                        hm = GCommon.CloneTexture(HeightMap);
                    Texture2D vm = null;
                    if (VisibilityMap != null)
                        vm = GCommon.CloneTexture(VisibilityMap);

                    int desResolution = DesData.Geometry.HeightMapResolution;
                    Color[] newColor = new Color[desResolution * desResolution];
                    Vector2 uv = Vector2.zero;
                    Vector2 enc = Vector2.zero;
                    for (int y = 0; y < desResolution; ++y)
                    {
                        for (int x = 0; x < desResolution; ++x)
                        {
                            uv.Set(
                                Mathf.InverseLerp(0, desResolution, x),
                                Mathf.InverseLerp(0, desResolution, y));

                            Color c = Color.clear;
                            if (hm != null)
                            {
                                enc = GCommon.EncodeTerrainHeight(hm.GetPixelBilinear(uv.x, uv.y).r);
                                c.r = enc.x;
                                c.g = enc.y;
                            }
                            else
                            {
                                c = DesData.Geometry.HeightMap.GetPixelBilinear(uv.x, uv.y);
                            }

                            if (vm != null)
                            {
                                c.a = 1 - vm.GetPixelBilinear(uv.x, uv.y).r;
                            }
                            else
                            {
                                c.a = DesData.Geometry.HeightMap.GetPixelBilinear(uv.x, uv.y).a;
                            }

                            newColor[GUtilities.To1DIndex(x, y, desResolution)] = c;
                        }
                    }

                    DesData.Geometry.HeightMap.SetPixels(newColor);
                    DesData.Geometry.HeightMap.Apply();

                    if (hm != null || vm != null)
                    {
                        Color[] newHeightMapColors = DesData.Geometry.HeightMap.GetPixels();
                        IEnumerable<Rect> dirtyRects = GCommon.CompareTerrainTexture(DesData.Geometry.ChunkGridSize, oldHeightMapColors, newHeightMapColors);
                        DesData.Geometry.SetRegionDirty(dirtyRects);
                        DesData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);

                        if (Terrain != null)
                        {
                            DesData.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                            DesData.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                            Terrain.UpdateTreesPosition();
                            Terrain.UpdateGrassPatches();
                            DesData.Foliage.ClearTreeDirtyRegions();
                            DesData.Foliage.ClearGrassDirtyRegions();
                        }
                    }

                    if (hm != null)
                        GUtilities.DestroyObject(hm);
                    if (vm != null)
                        GUtilities.DestroyObject(vm);
                }

                if (MaskMap != null)
                {
                    GCommon.CopyTexture(MaskMap, DesData.Mask.MaskMap);
                }
                if (AlbedoMap != null)
                {
                    GCommon.CopyTexture(AlbedoMap, DesData.Shading.AlbedoMap);
                    DesData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
                if (MetallicMap != null)
                {
                    GCommon.CopyTexture(MetallicMap, DesData.Shading.MetallicMap);
                    DesData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
                if (SplatControlMaps != null)
                {
                    int count = Mathf.Min(SplatControlMaps.Length, DesData.Shading.SplatControlMapCount);
                    for (int i = 0; i < count; ++i)
                    {
                        if (SplatControlMaps[i] != null)
                        {
                            GCommon.CopyTexture(SplatControlMaps[i], DesData.Shading.GetSplatControl(i));
                        }
                    }
                    DesData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
        }
    }
}
#endif
