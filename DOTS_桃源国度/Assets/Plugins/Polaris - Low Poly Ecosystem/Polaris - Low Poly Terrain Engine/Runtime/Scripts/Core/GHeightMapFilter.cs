#if GRIFFIN
using Pinwheel.Griffin.TextureTool;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [ExecuteInEditMode]
    [System.Serializable]
    public class GHeightMapFilter : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            private set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private bool useBlur;
        public bool UseBlur
        {
            get
            {
                return useBlur;
            }
            set
            {
                useBlur = value;
            }
        }

        [SerializeField]
        private int blurRadius;
        public int BlurRadius
        {
            get
            {
                return blurRadius;
            }
            set
            {
                blurRadius = Mathf.Clamp(value, 1, 5);
            }
        }

        [SerializeField]
        private bool useStep;
        public bool UseStep
        {
            get
            {
                return useStep;
            }
            set
            {
                useStep = value;
            }
        }

        [SerializeField]
        private int stepCount;
        public int StepCount
        {
            get
            {
                return stepCount;
            }
            set
            {
                stepCount = Mathf.Clamp(value, 1, 1024);
            }
        }

        private RenderTexture heightMapBackup;
        private RenderTexture rt;

        private void Reset()
        {
            UseBlur = false;
            BlurRadius = 1;
            UseStep = false;
            StepCount = 25;
        }

        private void OnEnable()
        {
            Terrain = GetComponent<GStylizedTerrain>();
            if (Terrain != null)
            {
                Terrain.PreProcessHeightMap += OnPreProcessHeightMap;
                Terrain.PostProcessHeightMap += OnPostProcessHeightMap;
            }
        }

        private void OnDisable()
        {
            if (Terrain != null)
            {
                Terrain.PreProcessHeightMap -= OnPreProcessHeightMap;
                Terrain.PostProcessHeightMap -= OnPostProcessHeightMap;
            }
            if (heightMapBackup != null)
            {
                GUtilities.DestroyObject(heightMapBackup);
            }
        }

        private void OnPreProcessHeightMap(Texture2D heightMap)
        {
            if (!UseBlur && !UseStep)
                return;

            InitRtSize(heightMap, ref heightMapBackup);
            CopyTo(heightMap, heightMapBackup);
            ApplyFilters(heightMap);
        }

        private void OnPostProcessHeightMap(Texture2D heightMap)
        {
            if (!UseBlur && !UseStep)
                return;

            RestoreHeightMap(heightMap);
        }

        private void CopyTo(Texture2D heightMap, RenderTexture targetRt)
        {
            GCommon.CopyToRT(heightMap, targetRt);
        }

        private void CopyTo(RenderTexture rt, Texture2D targetTex)
        {
            GCommon.CopyFromRT(targetTex, rt);
            targetTex.Apply();
        }

        private void InitRtSize(Texture src, ref RenderTexture targetRt)
        {
            if (targetRt == null)
            {
                targetRt = new RenderTexture(src.width, src.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }
            if (targetRt.width != src.width ||
                targetRt.height != src.height)
            {
                targetRt.Release();
                GUtilities.DestroyObject(targetRt);
                targetRt = new RenderTexture(src.width, src.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }
        }

        private void RestoreHeightMap(Texture2D heightMap)
        {
            if (heightMapBackup != null && heightMapBackup.IsCreated())
            {
                GCommon.CopyFromRT(heightMap, heightMapBackup);
            }
        }

        private void ApplyFilters(Texture2D heightMap)
        {
            InitRtSize(heightMap, ref rt);
            CopyTo(heightMap, rt);
            GTextureFilterParams param = new GTextureFilterParams();
            if (UseBlur)
            {
                GBlurParams blurParam = GBlurParams.Create();
                blurParam.Radius = BlurRadius;
                param.Blur = blurParam;

                GBlurFilter blurFilter = new GBlurFilter();
                blurFilter.Apply(rt, param);
            }
            if (UseStep)
            {
                GStepParams stepParam = GStepParams.Create();
                stepParam.Count = StepCount;
                param.Step = stepParam;

                GStepFilter stepFilter = new GStepFilter();
                stepFilter.Apply(rt, param);
            }
            CopyTo(rt, heightMap);
        }
    }
}
#endif
