#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Griffin.ErosionTool
{
    public class GErosionInitializer
    {
        public GErosionSimulator Simulator { get; private set; }

        private Material fetchWorldDataMaterial;
        private Material FetchWorldDataMaterial
        {
            get
            {
                if (fetchWorldDataMaterial == null)
                {
                    fetchWorldDataMaterial = new Material(GRuntimeSettings.Instance.internalShaders.fetchWorldDataShader);
                }
                return fetchWorldDataMaterial;
            }
        }

        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int BOUNDS = Shader.PropertyToID("_Bounds");
        private static readonly int ENABLE_TERRAIN_MASK = Shader.PropertyToID("_EnableTerrainMask");
        private static readonly int PASS_HEIGHT = 0;
        private static readonly int PASS_MASK = 1;
        private static readonly int PASS_EROSION_MAP = 2;

        public GErosionInitializer(GErosionSimulator s)
        {
            Simulator = s;
        }

        public void Init(ref Vector3 bounds, ref RenderTexture simulationData, ref RenderTexture simulationMask, ref RenderTexture erosionMap)
        {
            List<GStylizedTerrain> terrains = Simulator.GetIntersectedTerrains();
            bounds = GetSimulationBounds(terrains);
            CreateRenderTexture(bounds, ref simulationData, ref simulationMask, ref erosionMap);
            FetchWorldData(terrains, bounds, ref simulationData, ref simulationMask, ref erosionMap);
        }

        private Vector3 GetSimulationBounds(List<GStylizedTerrain> terrains)
        {
            Vector3 bounds = new Vector3();
            bounds.x = Mathf.CeilToInt(Simulator.DetailLevel * Simulator.transform.lossyScale.x);
            bounds.z = Mathf.CeilToInt(Simulator.DetailLevel * Simulator.transform.lossyScale.z);
            for (int i = 0; i < terrains.Count; ++i)
            {
                bounds.y = Mathf.Max(bounds.y, terrains[i].TerrainData.Geometry.Height);
            }
            return bounds;
        }

        private void CreateRenderTexture(Vector3 bounds, ref RenderTexture simulationData, ref RenderTexture simulationMask, ref RenderTexture erosionMap)
        {
            if (simulationData != null)
            {
                simulationData.Release();
            }

            if (simulationMask!=null)
            {
                simulationMask.Release();
            }

            if (erosionMap!=null)
            {
                erosionMap.Release();
            }

            int width = Mathf.Clamp((int)bounds.x, 1, 4096);
            int height = Mathf.Clamp((int)bounds.z, 1, 4096);
            int depth = 0;
            RenderTextureReadWrite rw = RenderTextureReadWrite.Linear;

            simulationData = new RenderTexture(width, height, depth, RenderTextureFormat.ARGBFloat, rw);
            simulationData.enableRandomWrite = true;
            simulationData.wrapMode = TextureWrapMode.Clamp;
            simulationData.Create();

            simulationMask = new RenderTexture(width, height, depth, RenderTextureFormat.ARGBFloat, rw);
            simulationMask.enableRandomWrite = true;
            simulationMask.wrapMode = TextureWrapMode.Clamp;
            simulationMask.Create();

            erosionMap = new RenderTexture(width, height, depth, RenderTextureFormat.RGFloat, rw);
            erosionMap.enableRandomWrite = true;
            erosionMap.wrapMode = TextureWrapMode.Clamp;
            erosionMap.Create();
        }

        private void FetchWorldData(List<GStylizedTerrain> terrains, Vector3 bounds, ref RenderTexture simulationData, ref RenderTexture simulationMask, ref RenderTexture erosionMap)
        {
            GCommon.DrawQuad(erosionMap, GCommon.FullRectUvPoints, FetchWorldDataMaterial, PASS_EROSION_MAP);

            for (int i = 0; i < terrains.Count; ++i)
            {
                GStylizedTerrain t = terrains[i];
                Vector3 terrainPos = t.transform.position;
                Vector3 terrainSize = t.TerrainData.Geometry.Size;
                Vector3[] terrainWorldCorners = new Vector3[4]
                {
                    new Vector3(terrainPos.x, 0, terrainPos.z),
                    new Vector3(terrainPos.x, 0, terrainPos.z + terrainSize.z),
                    new Vector3(terrainPos.x + terrainSize.x, 0, terrainPos.z + terrainSize.z),
                    new Vector3(terrainPos.x + terrainSize.x, 0, terrainPos.z)
                };

                Vector2[] uvCorner = new Vector2[4];
                for (int c = 0; c < uvCorner.Length; ++c)
                {
                    Vector3 simSpaceCorner = Simulator.transform.InverseTransformPoint(terrainWorldCorners[c]);
                    uvCorner[c] = new Vector2(simSpaceCorner.x + 0.5f, simSpaceCorner.z + 0.5f);
                }

                FetchWorldDataMaterial.SetTexture(MAIN_TEX, t.TerrainData.Geometry.HeightMap);
                FetchWorldDataMaterial.SetVector(BOUNDS, bounds);
                GCommon.DrawQuad(simulationData, uvCorner, FetchWorldDataMaterial, PASS_HEIGHT);

                FetchWorldDataMaterial.SetTexture(MAIN_TEX, t.TerrainData.Mask.MaskMapOrDefault);
                FetchWorldDataMaterial.SetFloat(ENABLE_TERRAIN_MASK, Simulator.EnableTerrainMask ? 1 : 0);
                GCommon.DrawQuad(simulationMask, uvCorner, FetchWorldDataMaterial, PASS_MASK);
            }
        }
    }
}
#endif
