#if GRIFFIN
using Pinwheel.Griffin.TextureTool;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    public struct GNoisePainterParams
    {
        [SerializeField]
        private GNoiseType type;
        public GNoiseType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        [SerializeField]
        private Vector2 origin;
        public Vector2 Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = value;
            }
        }

        [SerializeField]
        private float frequency;
        public float Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        [SerializeField]
        private float laccunarity;
        public float Lacunarity
        {
            get
            {
                return laccunarity;
            }
            set
            {
                laccunarity = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private float persistent;
        public float Persistence
        {
            get
            {
                return persistent;
            }
            set
            {
                persistent = Mathf.Clamp(value, 0.01f, 1);
            }
        }

        [SerializeField]
        private int octaves;
        public int Octaves
        {
            get
            {
                return octaves;
            }
            set
            {
                octaves = Mathf.Clamp(value, 1, 4);
            }
        }

        [SerializeField]
        private float seed;
        public float Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private bool useWorldSpace;
        public bool UseWorldSpace
        {
            get
            {
                return useWorldSpace;
            }
            set
            {
                useWorldSpace = value;
            }
        }
    }
}
#endif
