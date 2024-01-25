#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GWarpParams
    {
        private Texture2D mask;
        public Texture2D Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
            }
        }

        [SerializeField]
        private bool maskIsNormalMap;
        public bool MaskIsNormalMap
        {
            get
            {
                return maskIsNormalMap;
            }
            set
            {
                maskIsNormalMap = value;
            }
        }

        [SerializeField]
        private float strength;
        public float Strength
        {
            get
            {
                return strength;
            }
            set
            {
                strength = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool useBackgroundAsMask;
        public bool UseBackgroundAsMask
        {
            get
            {
                return useBackgroundAsMask;
            }
            set
            {
                useBackgroundAsMask = value;
            }
        }

        public static GWarpParams Create()
        {
            GWarpParams param = new GWarpParams();
            param.Mask = null;
            param.MaskIsNormalMap = false;
            param.Strength = 1;
            param.UseBackgroundAsMask = false;
            return param;
        }
    }
}
#endif
