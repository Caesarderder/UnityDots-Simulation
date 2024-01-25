#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.ErosionTool
{
    public class GErosionApplier
    {
        public GErosionSimulator Simulator { get; private set; }

        private static Material applyGeometryMaterial;
        private static Material ApplyGeometryMaterial
        {
            get
            {
                if (applyGeometryMaterial == null)
                {
                    applyGeometryMaterial = new Material(GRuntimeSettings.Instance.internalShaders.applyErosionShader);
                }
                return applyGeometryMaterial;
            }
        }

        private static Material applyTextureMaterial;
        private static Material ApplyTextureMaterial
        {
            get
            {
                if (applyTextureMaterial == null)
                {
                    applyTextureMaterial = new Material(GRuntimeSettings.Instance.internalShaders.erosionTexturerShader);
                }
                return applyTextureMaterial;
            }
        }

        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int SIMULATION_DATA = Shader.PropertyToID("_SimulationData");
        private static readonly int FALLOFF_TEXTURE = Shader.PropertyToID("_FalloffTexture");
        private static readonly int BOUNDS = Shader.PropertyToID("_Bounds");

        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int EROSION_MAP = Shader.PropertyToID("_ErosionMap");

        private static readonly int EROSION_ALBEDO = Shader.PropertyToID("_ErosionAlbedo");
        private static readonly int EROSION_METALLIC = Shader.PropertyToID("_ErosionMetallic");
        private static readonly int EROSION_SMOOTHNESS = Shader.PropertyToID("_ErosionSmoothness");
        private static readonly int EROSION_CHANNEL_INDEX = Shader.PropertyToID("_ErosionChannelIndex");
        private static readonly int EROSION_INTENSITY = Shader.PropertyToID("_ErosionIntensity");
        private static readonly int EROSION_EXPONENT = Shader.PropertyToID("_ErosionExponent");

        private static readonly int DEPOSITION_ALBEDO = Shader.PropertyToID("_DepositionAlbedo");
        private static readonly int DEPOSITION_METALLIC = Shader.PropertyToID("_DepositionMetallic");
        private static readonly int DEPOSITION_SMOOTHNESS = Shader.PropertyToID("_DepositionSmoothness");
        private static readonly int DEPOSITION_CHANNEL_INDEX = Shader.PropertyToID("_DepositionChannelIndex");
        private static readonly int DEPOSITION_INTENSITY = Shader.PropertyToID("_DepositionIntensity");
        private static readonly int DEPOSITION_EXPONENT = Shader.PropertyToID("_DepositionExponent");

        private static readonly int PASS_APPLY_SPLAT = 0;
        private static readonly int PASS_APPLY_ALBEDO = 1;
        private static readonly int PASS_APPLY_METALLIC = 2;

        public GErosionApplier(GErosionSimulator s)
        {
            Simulator = s;
        }

        public void ApplyGeometry()
        {
            List<GStylizedTerrain> terrains = Simulator.GetIntersectedTerrains();
            if (terrains.Count == 0)
                return;

            for (int i = 0; i < terrains.Count; ++i)
            {
                ApplyGeometry(terrains[i]);
            }
            for (int i = 0; i < terrains.Count; ++i)
            {
                UpdateGeometry(terrains[i]);
            }
        }

        private void ApplyGeometry(GStylizedTerrain t)
        {
            Vector3[] worldCorner = Simulator.GetQuad();
            Vector2[] uvCorners = new Vector2[worldCorner.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorner[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;

            int resolution = t.TerrainData.Geometry.HeightMapResolution;
            RenderTexture rt = new RenderTexture(resolution, resolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
            GCommon.CopyToRT(t.TerrainData.Geometry.HeightMap, rt);

            ApplyGeometryMaterial.SetTexture(HEIGHT_MAP, t.TerrainData.Geometry.HeightMap);
            ApplyGeometryMaterial.SetTexture(SIMULATION_DATA, Simulator.SimulationData);
            ApplyGeometryMaterial.SetTexture(FALLOFF_TEXTURE, Simulator.FalloffTexture);
            ApplyGeometryMaterial.SetVector(BOUNDS, Simulator.Bounds);

            GCommon.DrawQuad(rt, uvCorners, ApplyGeometryMaterial, 0);

            Color[] oldHeightMapColors = t.TerrainData.Geometry.HeightMap.GetPixels();
            RenderTexture.active = rt;
            t.TerrainData.Geometry.HeightMap.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            t.TerrainData.Geometry.HeightMap.Apply();
            RenderTexture.active = null;
            Color[] newHeightMapColors = t.TerrainData.Geometry.HeightMap.GetPixels();

            rt.Release();
            GUtilities.DestroyObject(rt);

            List<Rect> dirtyRects = new List<Rect>(GCommon.CompareTerrainTexture(t.TerrainData.Geometry.ChunkGridSize, oldHeightMapColors, newHeightMapColors));
            for (int i = 0; i < dirtyRects.Count; ++i)
            {
                t.TerrainData.Geometry.SetRegionDirty(dirtyRects[i]);
                t.TerrainData.Foliage.SetTreeRegionDirty(dirtyRects[i]);
                t.TerrainData.Foliage.SetGrassRegionDirty(dirtyRects[i]);
            }
        }

        private void UpdateGeometry(GStylizedTerrain t)
        {
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
            t.UpdateTreesPosition();
            t.UpdateGrassPatches();
            t.TerrainData.Foliage.ClearTreeDirtyRegions();
            t.TerrainData.Foliage.ClearGrassDirtyRegions();
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
        }

        public void ApplySplat()
        {
            List<GStylizedTerrain> terrains = Simulator.GetIntersectedTerrains();
            if (terrains.Count == 0)
                return;
            for (int i = 0; i < terrains.Count; ++i)
            {
                ApplySplat(terrains[i]);
            }
        }

        private void ApplySplat(GStylizedTerrain t)
        {
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
            RenderTexture[] rtControls = new RenderTexture[controlMapCount];
            for (int i = 0; i < controlMapCount; ++i)
            {
                rtControls[i] = new RenderTexture(controlMapResolution, controlMapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }

            if (!Internal_ApplySplat(t, rtControls))
            {
                for (int i = 0; i < controlMapCount; ++i)
                {
                    rtControls[i].Release();
                    Object.DestroyImmediate(rtControls[i]);
                }
                return;
            }

            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D splatControl = t.TerrainData.Shading.GetSplatControl(i);
                RenderTexture.active = rtControls[i];
                splatControl.ReadPixels(new Rect(0, 0, controlMapResolution, controlMapResolution), 0, 0);
                splatControl.Apply();
                RenderTexture.active = null;

                rtControls[i].Release();
                Object.DestroyImmediate(rtControls[i]);
            }
        }

        public bool Internal_ApplySplat(GStylizedTerrain t, RenderTexture[] rtControls)
        {
            Vector3[] worldCorner = Simulator.GetQuad();
            Vector2[] uvCorners = new Vector2[worldCorner.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorner[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return false;

            GErosionTexturingConfigs config = Simulator.TexturingConfigs;
            Material mat = ApplyTextureMaterial;
            mat.SetTexture(EROSION_MAP, Simulator.ErosionMap);
            mat.SetTexture(FALLOFF_TEXTURE, Simulator.FalloffTexture);
            mat.SetFloat(EROSION_INTENSITY, config.ErosionIntensity);
            mat.SetFloat(EROSION_EXPONENT, config.ErosionExponent);
            mat.SetFloat(DEPOSITION_INTENSITY, config.DepositionIntensity);
            mat.SetFloat(DEPOSITION_EXPONENT, config.DepositionExponent);

            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D control = t.TerrainData.Shading.GetSplatControl(i);
                GCommon.CopyToRT(control, rtControls[i]);

                mat.SetTexture(MAIN_TEX, control);
                if (config.ErosionSplatIndex / 4 == i)
                {
                    mat.SetInt(EROSION_CHANNEL_INDEX, config.ErosionSplatIndex % 4);
                }
                else
                {
                    mat.SetInt(EROSION_CHANNEL_INDEX, -1);
                }

                if (config.DepositionSplatIndex / 4 == i)
                {
                    mat.SetInt(DEPOSITION_CHANNEL_INDEX, config.DepositionSplatIndex % 4);
                }
                else
                {
                    mat.SetInt(DEPOSITION_CHANNEL_INDEX, -1);
                }

                GCommon.DrawQuad(rtControls[i], uvCorners, mat, PASS_APPLY_SPLAT);
            }

            return true;
        }

        public void ApplyAMS()
        {
            List<GStylizedTerrain> terrains = Simulator.GetIntersectedTerrains();
            if (terrains.Count == 0)
                return;
            for (int i = 0; i < terrains.Count; ++i)
            {
                ApplyAMS(terrains[i]);
            }
        }

        private void ApplyAMS(GStylizedTerrain t)
        {
            int albedoResolution = t.TerrainData.Shading.AlbedoMapResolution;
            RenderTexture rtAlbedo = new RenderTexture(albedoResolution, albedoResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            int metallicResolution = t.TerrainData.Shading.MetallicMapResolution;
            RenderTexture rtMetallic = new RenderTexture(metallicResolution, metallicResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            if (!Internal_ApplyAMS(t, rtAlbedo, rtMetallic))
            {
                rtAlbedo.Release();
                Object.DestroyImmediate(rtAlbedo);

                rtMetallic.Release();
                Object.DestroyImmediate(rtMetallic);
                return;
            }

            Texture2D albedoMap = t.TerrainData.Shading.AlbedoMap;
            RenderTexture.active = rtAlbedo;
            albedoMap.ReadPixels(new Rect(0, 0, albedoResolution, albedoResolution), 0, 0);
            albedoMap.Apply();
            RenderTexture.active = null;

            rtAlbedo.Release();
            Object.DestroyImmediate(rtAlbedo);

            Texture2D metallicMap = t.TerrainData.Shading.MetallicMap;
            RenderTexture.active = rtMetallic;
            metallicMap.ReadPixels(new Rect(0, 0, metallicResolution, metallicResolution), 0, 0);
            metallicMap.Apply();
            RenderTexture.active = null;

            rtMetallic.Release();
            Object.DestroyImmediate(rtMetallic);
        }

        public bool Internal_ApplyAMS(GStylizedTerrain t, RenderTexture rtAlbedo, RenderTexture rtMetallic)
        {
            Vector3[] worldCorner = Simulator.GetQuad();
            Vector2[] uvCorners = new Vector2[worldCorner.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorner[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return false;

            GCommon.CopyToRT(t.TerrainData.Shading.AlbedoMap, rtAlbedo);
            GCommon.CopyToRT(t.TerrainData.Shading.MetallicMap, rtMetallic);

            GErosionTexturingConfigs config = Simulator.TexturingConfigs;
            Material mat = ApplyTextureMaterial;
            mat.SetTexture(EROSION_MAP, Simulator.ErosionMap);
            mat.SetTexture(FALLOFF_TEXTURE, Simulator.FalloffTexture);

            mat.SetFloat(EROSION_INTENSITY, config.ErosionIntensity);
            mat.SetFloat(EROSION_EXPONENT, config.ErosionExponent);
            mat.SetColor(EROSION_ALBEDO, config.ErosionAlbedo);
            mat.SetFloat(EROSION_METALLIC, config.ErosionMetallic);
            mat.SetFloat(EROSION_SMOOTHNESS, config.ErosionSmoothness);

            mat.SetFloat(DEPOSITION_INTENSITY, config.DepositionIntensity);
            mat.SetFloat(DEPOSITION_EXPONENT, config.DepositionExponent);
            mat.SetColor(DEPOSITION_ALBEDO, config.DepositionAlbedo);
            mat.SetFloat(DEPOSITION_METALLIC, config.DepositionMetallic);
            mat.SetFloat(DEPOSITION_SMOOTHNESS, config.DepositionSmoothness);

            mat.SetTexture(MAIN_TEX, t.TerrainData.Shading.AlbedoMap);
            GCommon.DrawQuad(rtAlbedo, uvCorners, mat, PASS_APPLY_ALBEDO);

            mat.SetTexture(MAIN_TEX, t.TerrainData.Shading.MetallicMap);
            GCommon.DrawQuad(rtMetallic, uvCorners, mat, PASS_APPLY_METALLIC);

            return true;
        }
    }
}
#endif
