#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [System.Serializable]
    public struct GMaskOverride
    {
        [SerializeField]
        private bool overrideMaskMapResolution;
        public bool OverrideMaskMapResolution
        {
            get
            {
                return overrideMaskMapResolution;
            }
            set
            {
                overrideMaskMapResolution = value;
            }
        }

        [SerializeField]
        private int maskMapResolution;
        public int MaskMapResolution
        {
            get
            {
                return maskMapResolution;
            }
            set
            {
                maskMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
            }
        }

        public void Reset()
        {
            OverrideMaskMapResolution = false;

            MaskMapResolution = GRuntimeSettings.Instance.maskDefault.maskMapResolution;
        }

        public void Override(GMask m)
        {
            if (OverrideMaskMapResolution)
                m.MaskMapResolution = MaskMapResolution;
        }
    }
}
#endif
