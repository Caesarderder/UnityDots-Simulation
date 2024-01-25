#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Pinwheel.Griffin.ErosionTool
{
    public class GHydraulicEroder : IDisposable
    {
        public GErosionSimulator Simulator { get; private set; }

        public float WaterSourceAmount { get; set; }
        public float RainRate { get; set; }
        public float FlowRate { get; set; }
        public float ErosionRate { get; set; }
        public float DepositionRate { get; set; }
        public float EvaporationRate { get; set; }
        public Vector3 Bounds { get; set; }

        private RenderTexture outflowVHData;
        private RenderTexture outflowDiagData;
        private RenderTexture velocityData;

        private static readonly int EROSION_MAP = Shader.PropertyToID("_ErosionMap");
        private static readonly int MASK_MAP =  Shader.PropertyToID("_MaskMap");
        private static readonly int MASK_MAP_RESOLUTION = Shader.PropertyToID("_MaskMapResolution");
        private static readonly int WATER_SOURCE_AMOUNT = Shader.PropertyToID("_WaterSourceAmount");
        private static readonly int RAIN_RATE = Shader.PropertyToID("_RainRate");
        private static readonly int FLOW_RATE = Shader.PropertyToID("_FlowRate");
        private static readonly int EROSION_RATE = Shader.PropertyToID("_ErosionRate");
        private static readonly int DEPOSITION_RATE = Shader.PropertyToID("_DepositionRate");
        private static readonly int EVAPORATION_RATE = Shader.PropertyToID("_EvaporationRate");
        private static readonly int BOUNDS = Shader.PropertyToID("_Bounds");
        
        private static readonly int SIMULATION_DATA = Shader.PropertyToID("_SimulationData");
        private static readonly int OUTFLOW_VH_DATA = Shader.PropertyToID("_OutflowVHData");
        private static readonly int OUTFLOW_DIAG_DATA = Shader.PropertyToID("_OutflowDiagData");
        private static readonly int VELOCITY_DATA = Shader.PropertyToID("_VelocityData");
        private static readonly int RANDOM_SEED = Shader.PropertyToID("_RandomSeed");

        private static readonly int KERNEL_INDEX = 0;

        private bool initialized;

        public GHydraulicEroder(GErosionSimulator s)
        {
            Simulator = s;
        }

        public void Init()
        {
            int width = Simulator.SimulationData.width;
            int height = Simulator.SimulationData.height;

            outflowVHData = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            outflowVHData.enableRandomWrite = true;
            outflowVHData.wrapMode = TextureWrapMode.Clamp;
            outflowVHData.Create();

            outflowDiagData = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            outflowDiagData.enableRandomWrite = true;
            outflowDiagData.wrapMode = TextureWrapMode.Clamp;
            outflowDiagData.Create();

            velocityData = new RenderTexture(width, height, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
            velocityData.enableRandomWrite = true;
            velocityData.wrapMode = TextureWrapMode.Clamp;
            velocityData.Create();

            initialized = true;
        }

        public void Simulate()
        {
            if (!initialized)
            {
                throw new System.Exception("POLARIS: Eroder not initialized. Call Init() before running the simulation.");
            }

            ComputeShader shader = GRuntimeSettings.Instance.internalShaders.hydraulicErosionShader;
            shader.SetTexture(KERNEL_INDEX, SIMULATION_DATA, Simulator.SimulationData);
            shader.SetTexture(KERNEL_INDEX, EROSION_MAP, Simulator.ErosionMap);
            shader.SetTexture(KERNEL_INDEX, OUTFLOW_VH_DATA, outflowVHData);
            shader.SetTexture(KERNEL_INDEX, OUTFLOW_DIAG_DATA, outflowDiagData);
            shader.SetTexture(KERNEL_INDEX, VELOCITY_DATA, velocityData);
            shader.SetTexture(KERNEL_INDEX, MASK_MAP, Simulator.SimulationMask);

            shader.SetVector(MASK_MAP_RESOLUTION, new Vector4(Simulator.SimulationMask.width, Simulator.SimulationMask.height, 0, 0));
            shader.SetVector(BOUNDS, Simulator.Bounds);
            shader.SetVector(RANDOM_SEED, UnityEngine.Random.insideUnitCircle);

            shader.SetFloat(WATER_SOURCE_AMOUNT, WaterSourceAmount);
            shader.SetFloat(RAIN_RATE, RainRate);
            shader.SetFloat(FLOW_RATE, FlowRate);
            shader.SetFloat(EROSION_RATE, ErosionRate);
            shader.SetFloat(DEPOSITION_RATE, DepositionRate);
            shader.SetFloat(EVAPORATION_RATE, EvaporationRate);

            int dimX = (int)Simulator.Bounds.x;
            int dimZ = (int)Simulator.Bounds.z;

            int threadGroupX = (dimX + 7) / 8;
            int threadGroupY = 1;
            int threadGroupZ = (dimZ + 7) / 8;

            shader.Dispatch(KERNEL_INDEX, threadGroupX, threadGroupY, threadGroupZ);
        }

        public void Dispose()
        {
            if (outflowVHData != null)
            {
                outflowVHData.Release();
                outflowVHData = null;
            }
            if (outflowDiagData != null)
            {
                outflowDiagData.Release();
                outflowDiagData = null;
            }
            if (velocityData != null)
            {
                velocityData.Release();
                velocityData = null;
            }

            initialized = false;
        }
    }
}
#endif
