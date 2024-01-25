#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.ErosionTool
{
    [System.Serializable]
    public class GErosionTexturingConfigs
    {
        public enum GMode
        {
            Splat, AlbedoMetallicSmoothness
        }

        [SerializeField]
        private GMode texturingMode;
        public GMode TexturingMode
        {
            get
            {
                return texturingMode;
            }
            set
            {
                texturingMode = value;
            }
        }

        [SerializeField]
        private float erosionIntensity;
        public float ErosionIntensity
        {
            get
            {
                return erosionIntensity;
            }
            set
            {
                erosionIntensity = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float erosionExponent;
        public float ErosionExponent
        {
            get
            {
                return erosionExponent;
            }
            set
            {
                erosionExponent = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int erosionSplatIndex;
        public int ErosionSplatIndex
        {
            get
            {
                return erosionSplatIndex;
            }
            set
            {
                erosionSplatIndex = value;
            }
        }

        [SerializeField]
        private Color erosionAlbedo;
        public Color ErosionAlbedo
        {
            get
            {
                return erosionAlbedo;
            }
            set
            {
                erosionAlbedo = value;
            }
        }

        [SerializeField]
        private float erosionMetallic;
        public float ErosionMetallic
        {
            get
            {
                return erosionMetallic;
            }
            set
            {
                erosionMetallic = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float erosionSmoothness;
        public float ErosionSmoothness
        {
            get
            {
                return erosionSmoothness;
            }
            set
            {
                erosionSmoothness = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float depositionIntensity;
        public float DepositionIntensity
        {
            get
            {
                return depositionIntensity;
            }
            set
            {
                depositionIntensity = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float depositionExponent;
        public float DepositionExponent
        {
            get
            {
                return depositionExponent;
            }
            set
            {
                depositionExponent = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int depositionSplatIndex;
        public int DepositionSplatIndex
        {
            get
            {
                return depositionSplatIndex;
            }
            set
            {
                depositionSplatIndex = value;
            }
        }

        [SerializeField]
        private Color depositionAlbedo;
        public Color DepositionAlbedo
        {
            get
            {
                return depositionAlbedo;
            }
            set
            {
                depositionAlbedo = value;
            }
        }

        [SerializeField]
        private float depositionMetallic;
        public float DepositionMetallic
        {
            get
            {
                return depositionMetallic;
            }
            set
            {
                depositionMetallic = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float depositionSmoothness;
        public float DepositionSmoothness
        {
            get
            {
                return depositionSmoothness;
            }
            set
            {
                depositionSmoothness = Mathf.Clamp01(value);
            }
        }

        public GErosionTexturingConfigs()
        {
            texturingMode = GMode.Splat;
            erosionIntensity = 1;
            erosionExponent = 1;
            erosionSplatIndex = 0;
            erosionAlbedo = Color.red;
            erosionMetallic = 0;
            erosionSmoothness = 0;

            depositionIntensity = 1;
            depositionExponent = 1;
            depositionSplatIndex = 0;
            depositionAlbedo = Color.green;
            depositionMetallic = 0;
            depositionSmoothness = 0;
        }
    }
}
#endif
