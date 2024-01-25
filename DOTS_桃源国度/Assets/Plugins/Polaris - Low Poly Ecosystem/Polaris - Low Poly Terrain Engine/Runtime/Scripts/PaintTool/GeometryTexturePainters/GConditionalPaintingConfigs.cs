#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    public class GConditionalPaintingConfigs
    {
        private static readonly string BLEND_HEIGHT_KW = "BLEND_HEIGHT";
        private static readonly int HEIGHT_MAP = Shader.PropertyToID("_HeightMap");
        private static readonly int MIN_HEIGHT = Shader.PropertyToID("_MinHeight");
        private static readonly int MAX_HEIGHT = Shader.PropertyToID("_MaxHeight");
        private static readonly int HEIGHT_TRANSITION = Shader.PropertyToID("_HeightTransition");

        private static readonly string BLEND_SLOPE_KW = "BLEND_SLOPE";
        private static readonly int NORMAL_MAP = Shader.PropertyToID("_NormalMap");
        private static readonly int MIN_SLOPE = Shader.PropertyToID("_MinSlope");
        private static readonly int MAX_SLOPE = Shader.PropertyToID("_MaxSlope");
        private static readonly int SLOPE_TRANSITION = Shader.PropertyToID("_SlopeTransition");

        private static readonly string BLEND_NOISE_KW = "BLEND_NOISE";
        private static readonly int NOISE_ORIGIN = Shader.PropertyToID("_NoiseOrigin");
        private static readonly int NOISE_FREQUENCY = Shader.PropertyToID("_NoiseFrequency");
        private static readonly int NOISE_OCTAVES = Shader.PropertyToID("_NoiseOctaves");
        private static readonly int NOISE_LACUNARITY = Shader.PropertyToID("_NoiseLacunarity");
        private static readonly int NOISE_PERSISTENCE = Shader.PropertyToID("_NoisePersistence");
        private static readonly int NOISE_REMAP = Shader.PropertyToID("_NoiseRemap");
        private static readonly int TERRAIN_DIMENSION = Shader.PropertyToID("_TerrainDimension");

        [SerializeField]
        private bool blendHeight;
        public bool BlendHeight
        {
            get
            {
                return blendHeight;
            }
            set
            {
                blendHeight = value;
            }
        }

        [SerializeField]
        private float minHeight;
        public float MinHeight
        {
            get
            {
                return minHeight;
            }
            set
            {
                minHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float maxHeight;
        public float MaxHeight
        {
            get
            {
                return maxHeight;
            }
            set
            {
                maxHeight = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve heightTransition;
        public AnimationCurve HeightTransition
        {
            get
            {
                if (heightTransition == null)
                {
                    heightTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
                }
                return heightTransition;
            }
            set
            {
                heightTransition = value;
            }
        }

        [SerializeField]
        private bool blendSlope;
        public bool BlendSlope
        {
            get
            {
                return blendSlope;
            }
            set
            {
                blendSlope = value;
            }
        }

        [SerializeField]
        private GNormalMapMode normalMapMode;
        public GNormalMapMode NormalMapMode
        {
            get
            {
                return normalMapMode;
            }
            set
            {
                normalMapMode = value;
            }
        }

        [SerializeField]
        private float minSlope;
        public float MinSlope
        {
            get
            {
                return minSlope;
            }
            set
            {
                minSlope = Mathf.Clamp(value, 0, 90);
            }
        }

        [SerializeField]
        private float maxSlope;
        public float MaxSlope
        {
            get
            {
                return maxSlope;
            }
            set
            {
                maxSlope = Mathf.Clamp(value, 0, 90);
            }
        }

        [SerializeField]
        private AnimationCurve slopeTransition;
        public AnimationCurve SlopeTransition
        {
            get
            {
                if (slopeTransition == null)
                {
                    slopeTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
                }
                return slopeTransition;
            }
            set
            {
                slopeTransition = value;
            }
        }

        [SerializeField]
        private bool blendNoise;
        public bool BlendNoise
        {
            get
            {
                return blendNoise;
            }
            set
            {
                blendNoise = value;
            }
        }

        [SerializeField]
        private Vector2 noiseOrigin;
        public Vector2 NoiseOrigin
        {
            get
            {
                return noiseOrigin;
            }
            set
            {
                noiseOrigin = value;
            }
        }

        [SerializeField]
        private float noiseFrequency;
        public float NoiseFrequency
        {
            get
            {
                return noiseFrequency;
            }
            set
            {
                noiseFrequency = value;
            }
        }

        [SerializeField]
        private int noiseOctaves;
        public int NoiseOctaves
        {
            get
            {
                return noiseOctaves;
            }
            set
            {
                noiseOctaves = Mathf.Clamp(value, 1, 4);
            }
        }

        [SerializeField]
        private float noiseLacunarity;
        public float NoiseLacunarity
        {
            get
            {
                return noiseLacunarity;
            }
            set
            {
                noiseLacunarity = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private float noisePersistence;
        public float NoisePersistence
        {
            get
            {
                return noisePersistence;
            }
            set
            {
                noisePersistence = Mathf.Clamp(value, 0.01f, 1f);
            }
        }

        [SerializeField]
        private AnimationCurve noiseRemap;
        public AnimationCurve NoiseRemap
        {
            get
            {
                if (noiseRemap == null)
                {
                    noiseRemap = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }
                return noiseRemap;
            }
            set
            {
                noiseRemap = value;
            }
        }

        internal Texture2D heightTransitionTexture;
        internal Texture2D slopeTransitionTexture;
        internal Texture2D noiseRemapTexture;

        public void UpdateCurveTextures()
        {
            CleanUp();

            heightTransitionTexture = GCommon.CreateTextureFromCurve(HeightTransition, 256, 1);
            slopeTransitionTexture = GCommon.CreateTextureFromCurve(SlopeTransition, 256, 1);
            noiseRemapTexture = GCommon.CreateTextureFromCurve(NoiseRemap, 256, 1);
        }

        public void CleanUp()
        {
            if (heightTransitionTexture != null)
                GUtilities.DestroyObject(heightTransitionTexture);
            if (slopeTransitionTexture != null)
                GUtilities.DestroyObject(slopeTransitionTexture);
            if (noiseRemapTexture != null)
                GUtilities.DestroyObject(noiseRemapTexture);
        }

        public GConditionalPaintingConfigs()
        {
            BlendHeight = false;
            MinHeight = 0;
            MaxHeight = 1000;
            HeightTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
            BlendSlope = false;
            MinSlope = 0;
            MaxSlope = 90;
            SlopeTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
            BlendNoise = false;
            NoiseOrigin = Vector2.zero;
            NoiseFrequency = 100f;
            NoiseOctaves = 1;
            NoiseLacunarity = 2;
            NoisePersistence = 0.5f;
            NoiseRemap = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        public void SetupMaterial(GStylizedTerrain t, Material painterMat)
        {
            int resolution = t.TerrainData.Geometry.HeightMapResolution;
            Vector3 terrainSize = t.TerrainData.Geometry.Size;

            if (BlendHeight)
            {
                RenderTexture heightMap = t.GetHeightMap(resolution);
                painterMat.EnableKeyword(BLEND_HEIGHT_KW);
                painterMat.SetTexture(HEIGHT_MAP, heightMap);
                painterMat.SetFloat(MIN_HEIGHT, GUtilities.InverseLerpUnclamped(0, terrainSize.y, MinHeight));
                painterMat.SetFloat(MAX_HEIGHT, GUtilities.InverseLerpUnclamped(0, terrainSize.y, MaxHeight));
                painterMat.SetTexture(HEIGHT_TRANSITION, heightTransitionTexture);
            }
            else
            {
                painterMat.DisableKeyword(BLEND_HEIGHT_KW);
            }

            if (BlendSlope)
            {
                RenderTexture normalMap =
                    NormalMapMode == GNormalMapMode.Sharp ? t.GetSharpNormalMap(resolution) :
                    NormalMapMode == GNormalMapMode.Interpolated ? t.GetInterpolatedNormalMap(resolution) :
                    NormalMapMode == GNormalMapMode.PerPixel ? t.GetPerPixelNormalMap(resolution) : null;
                painterMat.EnableKeyword(BLEND_SLOPE_KW);
                painterMat.SetTexture(NORMAL_MAP, normalMap);
                painterMat.SetFloat(MIN_SLOPE, MinSlope * Mathf.Deg2Rad);
                painterMat.SetFloat(MAX_SLOPE, MaxSlope * Mathf.Deg2Rad);
                painterMat.SetTexture(SLOPE_TRANSITION, slopeTransitionTexture);
            }
            else
            {
                painterMat.DisableKeyword(BLEND_SLOPE_KW);
            }

            if (BlendNoise)
            {
                painterMat.EnableKeyword(BLEND_NOISE_KW);
                painterMat.SetVector(NOISE_ORIGIN, NoiseOrigin);
                painterMat.SetFloat(NOISE_FREQUENCY, NoiseFrequency);
                painterMat.SetFloat(NOISE_OCTAVES, NoiseOctaves);
                painterMat.SetFloat(NOISE_LACUNARITY, NoiseLacunarity);
                painterMat.SetFloat(NOISE_PERSISTENCE, NoisePersistence);
                painterMat.SetTexture(NOISE_REMAP, noiseRemapTexture);
                painterMat.SetVector(TERRAIN_DIMENSION, new Vector4(t.transform.position.x, t.transform.position.z, terrainSize.x, terrainSize.z));
            }
            else
            {
                painterMat.DisableKeyword(BLEND_NOISE_KW);
            }
        }
    }
}
#endif
