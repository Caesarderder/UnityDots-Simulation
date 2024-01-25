#if GRIFFIN
using Pinwheel.Griffin.TextureTool;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    public class GConditionalStampLayer
    {
        [SerializeField]
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        [SerializeField]
        private bool ignore;
        public bool Ignore
        {
            get
            {
                return ignore;
            }
            set
            {
                ignore = value;
            }
        }

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

        ~GConditionalStampLayer()
        {
            if (heightTransitionTexture != null)
                GUtilities.DestroyObject(heightTransitionTexture);
            if (slopeTransitionTexture != null)
                GUtilities.DestroyObject(slopeTransitionTexture);
            if (noiseRemapTexture != null)
                GUtilities.DestroyObject(noiseRemapTexture);
        }

        public void UpdateCurveTextures()
        {
            if (heightTransitionTexture != null)
                GUtilities.DestroyObject(heightTransitionTexture);
            if (slopeTransitionTexture != null)
                GUtilities.DestroyObject(slopeTransitionTexture);
            if (noiseRemapTexture != null)
                GUtilities.DestroyObject(noiseRemapTexture);

            heightTransitionTexture = GCommon.CreateTextureFromCurve(HeightTransition, 256, 1);
            slopeTransitionTexture = GCommon.CreateTextureFromCurve(SlopeTransition, 256, 1);
            noiseRemapTexture = GCommon.CreateTextureFromCurve(NoiseRemap, 256, 1);
        }

        public GConditionalStampLayer()
        {
            Ignore = false;
            BlendHeight = true;
            MinHeight = 0;
            MaxHeight = 1000;
            HeightTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
            BlendSlope = true;
            MinSlope = 0;
            MaxSlope = 90;
            SlopeTransition = AnimationCurve.EaseInOut(0, 1, 1, 1);
            BlendNoise = false;
            NoiseOrigin = Vector2.zero;
            NoiseFrequency = 1f;
            NoiseOctaves = 1;
            NoiseLacunarity = 2;
            NoisePersistence = 0.5f;
            NoiseRemap = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
}
#endif
