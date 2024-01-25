#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.ErosionTool
{
    [ExecuteInEditMode]
    public class GErosionSimulator : MonoBehaviour
    {
        [SerializeField]
        private int groupId;
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private bool enableTerrainMask;
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        private AnimationCurve falloffCurve;
        public AnimationCurve FalloffCurve
        {
            get
            {
                if (falloffCurve == null)
                {
                    falloffCurve = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return falloffCurve;
            }
            set
            {
                falloffCurve = value;
            }
        }

        [SerializeField]
        private float detailLevel;
        public float DetailLevel
        {
            get
            {
                return detailLevel;
            }
            set
            {
                detailLevel = Mathf.Clamp(value, 0.2f, 2f);
            }
        }

        [SerializeField]
        private GHydraulicErosionConfigs hydraulicConfigs;
        public GHydraulicErosionConfigs HydraulicConfigs
        {
            get
            {
                return hydraulicConfigs;
            }
            set
            {
                hydraulicConfigs = value;
            }
        }

        [SerializeField]
        private GThermalErosionConfigs thermalConfigs;
        public GThermalErosionConfigs ThermalConfigs
        {
            get
            {
                return thermalConfigs;
            }
            set
            {
                thermalConfigs = value;
            }
        }

        [SerializeField]
        private GErosionTexturingConfigs texturingConfigs;
        public GErosionTexturingConfigs TexturingConfigs
        {
            get
            {
                return texturingConfigs;
            }
            set
            {
                texturingConfigs = value;
            }
        }

        private RenderTexture simulationData;
        public RenderTexture SimulationData
        {
            get
            {
                return simulationData;
            }
        }

        private RenderTexture simulationMask;
        public RenderTexture SimulationMask
        {
            get
            {
                return simulationMask;
            }
        }

        private RenderTexture erosionMap;
        public RenderTexture ErosionMap
        {
            get
            {
                return erosionMap;
            }
        }

        private Texture2D falloffTexture;
        public Texture2D FalloffTexture
        {
            get
            {
                return falloffTexture;
            }
        }

        private Vector3 bounds;
        public Vector3 Bounds
        {
            get
            {
                return bounds;
            }
        }

        private void Reset()
        {
            groupId = -1;
            enableTerrainMask = false;
            falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
            detailLevel = 0.5f;
            hydraulicConfigs = new GHydraulicErosionConfigs();
            thermalConfigs = new GThermalErosionConfigs();
            texturingConfigs = new GErosionTexturingConfigs();
        }

        private void OnEnable()
        {
            UpdateFalloffTexture();
            Initialize();
        }

        private void OnDisable()
        {
            CleanUp();
        }

        public void Initialize()
        {
            GErosionInitializer initializer = new GErosionInitializer(this);
            initializer.Init(ref bounds, ref simulationData, ref simulationMask, ref erosionMap);
        }

        public void CleanUp()
        {
            if (simulationData != null)
            {
                simulationData.Release();
                simulationData = null;
            }

            if (simulationMask != null)
            {
                simulationMask.Release();
                simulationMask = null;
            }

            if (erosionMap != null)
            {
                erosionMap.Release();
                erosionMap = null;
            }

            if (falloffTexture != null)
            {
                GUtilities.DestroyObject(falloffTexture);
            }
        }

        public void SimulateHydraulicErosion()
        {
            GHydraulicEroder eroder = new GHydraulicEroder(this);
            eroder.Init();

            int iteration = HydraulicConfigs.IterationCount;
            for (int i = 0; i < iteration; ++i)
            {
                float t = i * 1.0f / iteration;
                eroder.WaterSourceAmount = HydraulicConfigs.WaterSourceAmount * HydraulicConfigs.WaterSourceOverTime.Evaluate(t) * HydraulicConfigs.WaterSourceMultiplier;
                eroder.RainRate = HydraulicConfigs.RainRate * HydraulicConfigs.RainOverTime.Evaluate(t) * HydraulicConfigs.RainMultiplier;
                eroder.FlowRate = HydraulicConfigs.FlowRate * HydraulicConfigs.FlowOverTime.Evaluate(t) * HydraulicConfigs.FlowMultiplier;
                eroder.ErosionRate = HydraulicConfigs.ErosionRate * HydraulicConfigs.ErosionOverTime.Evaluate(t) * HydraulicConfigs.ErosionMultiplier;
                eroder.DepositionRate = HydraulicConfigs.DepositionRate * HydraulicConfigs.DepositionOverTime.Evaluate(t) * HydraulicConfigs.DepositionMultiplier;
                eroder.EvaporationRate = HydraulicConfigs.EvaporationRate * HydraulicConfigs.EvaporationOverTime.Evaluate(t) * HydraulicConfigs.EvaporationMultiplier;
                eroder.Bounds = Bounds;

                eroder.Simulate();
            }

            eroder.Dispose();
        }

        public void SimulateThermalErosion()
        {
            GThermalEroder eroder = new GThermalEroder(this);
            eroder.Init();

            int iteration = ThermalConfigs.IterationCount;
            for (int i = 0; i < iteration; ++i)
            {
                float t = i * 1.0f / iteration;
                eroder.MaskMap = SimulationMask;
                eroder.ErosionRate = ThermalConfigs.ErosionRate * ThermalConfigs.ErosionOverTime.Evaluate(t) * ThermalConfigs.ErosionMultiplier;
                eroder.RestingAngle = ThermalConfigs.RestingAngle * ThermalConfigs.RestingAngleOverTime.Evaluate(t) * ThermalConfigs.RestingAngleMultiplier;
                eroder.Bounds = Bounds;

                eroder.Simulate();
            }

            eroder.Dispose();
        }

        public void ApplyGeometry()
        {
            GErosionApplier applier = new GErosionApplier(this);
            applier.ApplyGeometry();
        }

        public void ApplyTexture()
        {
            GErosionApplier applier = new GErosionApplier(this);
            if (TexturingConfigs.TexturingMode == GErosionTexturingConfigs.GMode.Splat)
            {
                applier.ApplySplat();
            }
            else
            {
                applier.ApplyAMS();
            }
        }

        public List<GStylizedTerrain> GetIntersectedTerrains()
        {
            Vector3[] worldCorner = GetQuad();
            return GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(GroupId, worldCorner));
        }

        public Vector3[] GetQuad()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Vector3[] quad = new Vector3[4]
            {
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f)),
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f))
            };

            return quad;
        }

        public void UpdateFalloffTexture()
        {
            if (falloffTexture != null)
            {
                GUtilities.DestroyObject(falloffTexture);
            }
            falloffTexture = GCommon.CreateTextureFromCurve(FalloffCurve, 2048, 1);
        }
    }
}
#endif
