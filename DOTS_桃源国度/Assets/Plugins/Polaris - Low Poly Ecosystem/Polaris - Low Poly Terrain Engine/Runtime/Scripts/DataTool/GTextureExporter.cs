#if GRIFFIN
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.DataTool
{
    public class GTextureExporter
    {
        public GTerrainData SrcData { get; set; }
        public bool ExportHeightMap { get; set; }
        public bool ExportVisibilityMap { get; set; }
        public bool ExportAlbedoMap { get; set; }
        public bool ExportMetallicMap { get; set; }
        public bool ExportGradientLookupMaps { get; set; }
        public bool ExportSplatControlMaps { get; set; }
        public string DataDirectory { get; set; }

        public void Export()
        {
            try
            {
#if UNITY_EDITOR
                GCommonGUI.ProgressBar("Working", "Exporting terrain textures...", 1f);
#endif
                if (ExportHeightMap)
                {
                    DoExportHeightMap();
                }
                if (ExportVisibilityMap)
                {
                    DoExportVisibilityMap();
                }
                if (ExportAlbedoMap)
                {
                    DoExportAlbedoMap();
                }
                if (ExportMetallicMap)
                {
                    DoExportMetallicMap();
                }
                if (ExportGradientLookupMaps)
                {
                    DoExportGradientLookupMaps();
                }
                if (ExportSplatControlMaps)
                {
                    DoExportSplatControlMaps();
                }
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
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

        private void DoExportHeightMap()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);
            Color[] colors = SrcData.Geometry.HeightMap.GetPixels();
            Vector2 enc = Vector2.zero;
            float h = 0;
            for (int i = 0; i < colors.Length; ++i)
            {
                enc.Set(colors[i].r, colors[i].g);
                h = GCommon.DecodeTerrainHeight(enc);
                colors[i] = new Color(h, 0, 0, 0);
            }
            Texture2D tex = new Texture2D(SrcData.Geometry.HeightMapResolution, SrcData.Geometry.HeightMapResolution, TextureFormat.RGBAFloat, false);
            tex.SetPixels(colors);
            tex.Apply();
            byte[] data = tex.EncodeToPNG();
            GUtilities.DestroyObject(tex);
            string fileName = string.Format("HeightMap_{0}x{0}_{1}.png", SrcData.Geometry.HeightMapResolution, SrcData.Id);
            string filePath = Path.Combine(DataDirectory, fileName);
            File.WriteAllBytes(filePath, data);
        }

        private void DoExportVisibilityMap()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);
            Color[] colors = SrcData.Geometry.HeightMap.GetPixels();
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = new Color(1 - colors[i].a, 0, 0, 0);
            }
            Texture2D tex = new Texture2D(SrcData.Geometry.HeightMapResolution, SrcData.Geometry.HeightMapResolution);
            tex.SetPixels(colors);
            tex.Apply();
            byte[] data = tex.EncodeToPNG();
            GUtilities.DestroyObject(tex);
            string fileName = string.Format("VisibilityMap_{0}x{0}_{1}.png", SrcData.Geometry.HeightMapResolution, SrcData.Id);
            string filePath = Path.Combine(DataDirectory, fileName);
            File.WriteAllBytes(filePath, data);
        }

        private void DoExportAlbedoMap()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);
            Texture2D albedo = GCommon.CloneAndResizeTexture(SrcData.Shading.AlbedoMapOrDefault, SrcData.Shading.AlbedoMapResolution, SrcData.Shading.AlbedoMapResolution);
            byte[] data = albedo.EncodeToPNG();
            string fileName = string.Format("AlbedoMap_{0}x{0}_{1}.png", SrcData.Shading.AlbedoMapResolution, SrcData.Id);
            string filePath = Path.Combine(DataDirectory, fileName);
            File.WriteAllBytes(filePath, data);
            GUtilities.DestroyObject(albedo);
        }

        private void DoExportMetallicMap()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);
            Texture2D metallic = GCommon.CloneAndResizeTexture(SrcData.Shading.MetallicMapOrDefault, SrcData.Shading.MetallicMapResolution, SrcData.Shading.MetallicMapResolution);
            byte[] data = metallic.EncodeToPNG();
            string fileName = string.Format("MetallicMap_{0}x{0}_{1}.png", SrcData.Shading.MetallicMapResolution, SrcData.Id);
            string filePath = Path.Combine(DataDirectory, fileName);
            File.WriteAllBytes(filePath, data);
            GUtilities.DestroyObject(metallic);
        }

        private void DoExportGradientLookupMaps()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);
            SrcData.Shading.UpdateLookupTextures();
            SrcData.Shading.UpdateMaterials();
            Texture2D[] lookupTextures = new Texture2D[]
            {
                SrcData.Shading.ColorByHeightMap,
                SrcData.Shading.ColorByNormalMap,
                SrcData.Shading.ColorBlendMap
            };

            for (int i = 0; i < lookupTextures.Length; ++i)
            {
                Texture2D tex = lookupTextures[i];
                byte[] data = tex.EncodeToPNG();
                string fileName = string.Format("{0}_{1}x{2}_{3}.png", tex.name.Replace(" ", ""), tex.width, tex.height, SrcData.Id);
                string filePath = Path.Combine(DataDirectory, fileName);
                File.WriteAllBytes(filePath, data);
            }
        }

        private void DoExportSplatControlMaps()
        {
            GUtilities.EnsureDirectoryExists(DataDirectory);

            int controlCount = SrcData.Shading.SplatControlMapCount;
            for (int i = 0; i < controlCount; ++i)
            {
                Texture2D tex = GCommon.CloneAndResizeTexture(SrcData.Shading.GetSplatControlOrDefault(i), SrcData.Shading.SplatControlResolution, SrcData.Shading.SplatControlResolution);
                byte[] data = tex.EncodeToPNG();
                string fileName = string.Format("{0}_{1}x{2}_{3}.png", tex.name.Replace(" ", ""), tex.width, tex.height, SrcData.Id);
                string filePath = Path.Combine(DataDirectory, fileName);
                File.WriteAllBytes(filePath, data);
                GUtilities.DestroyObject(tex);
            }
        }
    }
}
#endif
