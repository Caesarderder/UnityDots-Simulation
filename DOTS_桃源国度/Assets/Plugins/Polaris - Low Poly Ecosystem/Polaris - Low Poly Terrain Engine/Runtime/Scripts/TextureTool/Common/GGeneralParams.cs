#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GGeneralParams
    {
        public const string DEFAULT_DIRECTORY = "Assets/Polaris Exported/";

        [SerializeField]
        private GTextureGenerationMode mode;
        public GTextureGenerationMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        [SerializeField]
        private int resolution;
        public int Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), 32, 4096);
            }
        }

        [SerializeField]
        private GImageFileExtension extension;
        public GImageFileExtension Extension
        {
            get
            {
                return extension;
            }
            set
            {
                extension = value;
            }
        }

        [SerializeField]
        private bool useHighPrecisionTexture;
        public bool UseHighPrecisionTexture
        {
            get
            {
                return useHighPrecisionTexture;
            }
            set
            {
                useHighPrecisionTexture = value;
            }
        }

        [SerializeField]
        private string directory;
        public string Directory
        {
            get
            {
                if (string.IsNullOrEmpty(directory))
                {
                    directory = DEFAULT_DIRECTORY;
                }
                return directory;
            }
            set
            {
                directory = value;
            }
        }
    }
}
#endif
