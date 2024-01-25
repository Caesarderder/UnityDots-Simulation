#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Griffin.ErosionTool
{
    public class GThermalEroder : IDisposable
    {
        public GErosionSimulator Simulator { get; private set; }

        public Texture MaskMap { get; set; }
        public float ErosionRate { get; set; }
        public float RestingAngle { get; set; }
        public Vector3 Bounds { get; set; }

        private RenderTexture soilVHData;
        private RenderTexture soilDiagData;

        private static readonly int SIMULATION_DATA = Shader.PropertyToID("_SimulationData");
        private static readonly int EROSION_MAP = Shader.PropertyToID("_ErosionMap");
        private static readonly int SOIL_VH_DATA = Shader.PropertyToID("_SoilVHData");
        private static readonly int SOIL_DIAG_DATA = Shader.PropertyToID("_SoilDiagData");
        private static readonly int MASK_MAP = Shader.PropertyToID("_MaskMap");
        private static readonly int MASK_MAP_RESOLUTION = Shader.PropertyToID("_MaskMapResolution");
        private static readonly int EROSION_RATE = Shader.PropertyToID("_ErosionRate");
        private static readonly int RESTING_ANGLE = Shader.PropertyToID("_RestingAngle");
        private static readonly int BOUNDS = Shader.PropertyToID("_Bounds");

        private static readonly int KERNEL_INDEX = 0;

        private bool initialized;

        public GThermalEroder(GErosionSimulator s)
        {
            Simulator = s;
        }

        public void Init()
        {
            int width = Simulator.SimulationData.width;
            int height = Simulator.SimulationData.height;

            soilVHData = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            soilVHData.enableRandomWrite = true;
            soilVHData.wrapMode = TextureWrapMode.Clamp;
            soilVHData.Create();

            soilDiagData = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            soilDiagData.enableRandomWrite = true;
            soilDiagData.wrapMode = TextureWrapMode.Clamp;
            soilDiagData.Create();

            initialized = true;
        }

        public void Simulate()
        {
            if (!initialized)
            {
                throw new System.Exception("POLARIS: Eroder not initialized. Call Init() before running the simulation.");
            }

            ComputeShader shader = GRuntimeSettings.Instance.internalShaders.thermalErosionShader;
            shader.SetTexture(KERNEL_INDEX, SIMULATION_DATA, Simulator.SimulationData);
            shader.SetTexture(KERNEL_INDEX, EROSION_MAP, Simulator.ErosionMap);
            shader.SetTexture(KERNEL_INDEX, SOIL_VH_DATA, soilVHData);
            shader.SetTexture(KERNEL_INDEX, SOIL_DIAG_DATA, soilDiagData);
            shader.SetTexture(KERNEL_INDEX, MASK_MAP, Simulator.SimulationMask);

            shader.SetVector(MASK_MAP_RESOLUTION, new Vector4(Simulator.SimulationMask.width, Simulator.SimulationMask.height, 0, 0));
            shader.SetVector(BOUNDS, Simulator.Bounds);

            shader.SetFloat(EROSION_RATE, ErosionRate);
            shader.SetFloat(RESTING_ANGLE, RestingAngle);

            int dimX = (int)Simulator.Bounds.x;
            int dimZ = (int)Simulator.Bounds.z;

            int threadGroupX = (dimX + 7) / 8;
            int threadGroupY = 1;
            int threadGroupZ = (dimZ + 7) / 8;

            shader.Dispatch(KERNEL_INDEX, threadGroupX, threadGroupY, threadGroupZ);
        }

        public void Dispose()
        {
            if (soilVHData != null)
            {
                soilVHData.Release();
                soilVHData = null;
            }
            if (soilDiagData != null)
            {
                soilDiagData.Release();
                soilDiagData = null;
            }

            initialized = false;
        }
    }
}
#endif
