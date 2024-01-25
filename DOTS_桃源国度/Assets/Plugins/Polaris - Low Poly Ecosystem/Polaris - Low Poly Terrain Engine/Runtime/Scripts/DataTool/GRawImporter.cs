#if GRIFFIN
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GRawImporter
    {
        public GStylizedTerrain Terrain { get; set; }
        public GTerrainData DesData { get; set; }
        public GBitDepth BitDepth { get; set; }
        public bool UseRawResolution { get; set; }
        public string FilePath { get; set; }

        public void Import()
        {
            try
            {
#if UNITY_EDITOR
                GCommonGUI.ProgressBar("Working", "Importing RAW...", 1f);
#endif
                if (BitDepth == GBitDepth.Bit8)
                {
                    ImportRaw8();
                }
                else if (BitDepth == GBitDepth.Bit16)
                {
                    ImportRaw16();
                }
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

        private void ImportRaw8()
        {
            byte[] data = File.ReadAllBytes(FilePath);
            int rawResolution = Mathf.FloorToInt(Mathf.Sqrt(data.Length));
            if (rawResolution * rawResolution != data.Length)
                throw new Exception("Error on populate data, RAW file doesn't have squared size or bit depth has been mis-configured!");

            if (UseRawResolution)
                DesData.Geometry.HeightMapResolution = rawResolution;

            float[] heightData = new float[rawResolution * rawResolution];
            for (int i = 0; i < heightData.Length; ++i)
            {
                heightData[i] = data[i] * 1.0f / byte.MaxValue;
            }

            Vector2 uv = Vector2.zero;
            Vector2 enc = Vector2.zero;
            float h = 0;
            Color[] heightMapColors = DesData.Geometry.HeightMap.GetPixels();
            Color[] oldHeightMapColors = new Color[heightMapColors.Length];
            Array.Copy(heightMapColors, oldHeightMapColors, heightMapColors.Length);

            int heightMapResolution = DesData.Geometry.HeightMapResolution;
            for (int z = 0; z < heightMapResolution; ++z)
            {
                for (int x = 0; x < heightMapResolution; ++x)
                {
                    uv.Set(
                        Mathf.InverseLerp(0, heightMapResolution - 1, x),
                        1 - Mathf.InverseLerp(0, heightMapResolution - 1, z));

                    Color c = heightMapColors[GUtilities.To1DIndex(x, z, heightMapResolution)];
                    h = GUtilities.GetValueBilinear(heightData, rawResolution, rawResolution, uv);
                    enc = GCommon.EncodeTerrainHeight(h);
                    c.r = enc.x;
                    c.g = enc.y;
                    heightMapColors[GUtilities.To1DIndex(x, z, heightMapResolution)] = c;
                }
            }

            DesData.Geometry.HeightMap.SetPixels(heightMapColors);
            DesData.Geometry.HeightMap.Apply();

            IEnumerable<Rect> dirtyRects = GCommon.CompareTerrainTexture(DesData.Geometry.ChunkGridSize, oldHeightMapColors, heightMapColors);
            DesData.Geometry.SetRegionDirty(dirtyRects);
            DesData.SetDirty(GTerrainData.DirtyFlags.Geometry);
        }

        private void ImportRaw16()
        {
            byte[] data = File.ReadAllBytes(FilePath);
            int rawResolution = Mathf.FloorToInt(Mathf.Sqrt(data.Length / sizeof(UInt16)));
            if (rawResolution * rawResolution != data.Length / sizeof(UInt16))
                throw new Exception("Error on populate data, RAW file doesn't have squared size or bit depth has been mis-configured!");

            if (UseRawResolution)
                DesData.Geometry.HeightMapResolution = rawResolution;

            float[] heightData = new float[rawResolution * rawResolution];
            for (int i = 0; i < heightData.Length; ++i)
            {
                UInt16 pixelValue = BitConverter.ToUInt16(data, i * 2);
                heightData[i] = pixelValue * 1.0f / UInt16.MaxValue;
            }

            Vector2 uv = Vector2.zero;
            Vector2 enc = Vector2.zero;
            float h = 0;
            Color[] heightMapColors = DesData.Geometry.HeightMap.GetPixels();
            Color[] oldHeightMapColors = new Color[heightMapColors.Length];
            Array.Copy(heightMapColors, oldHeightMapColors, heightMapColors.Length);

            int heightMapResolution = DesData.Geometry.HeightMapResolution;
            for (int z = 0; z < heightMapResolution; ++z)
            {
                for (int x = 0; x < heightMapResolution; ++x)
                {
                    uv.Set(
                        Mathf.InverseLerp(0, heightMapResolution - 1, x),
                        1 - Mathf.InverseLerp(0, heightMapResolution - 1, z));

                    Color c = heightMapColors[GUtilities.To1DIndex(x, z, heightMapResolution)];
                    h = GUtilities.GetValueBilinear(heightData, rawResolution, rawResolution, uv);
                    enc = GCommon.EncodeTerrainHeight(h);
                    c.r = enc.x;
                    c.g = enc.y;
                    heightMapColors[GUtilities.To1DIndex(x, z, heightMapResolution)] = c;
                }
            }

            DesData.Geometry.HeightMap.SetPixels(heightMapColors);
            DesData.Geometry.HeightMap.Apply();

            IEnumerable<Rect> dirtyRects = GCommon.CompareTerrainTexture(DesData.Geometry.ChunkGridSize, oldHeightMapColors, heightMapColors);
            DesData.Geometry.SetRegionDirty(dirtyRects);
            DesData.SetDirty(GTerrainData.DirtyFlags.Geometry);
        }
    }
}
#endif
