#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GStepParams
    {
        [SerializeField]
        private int count;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = Mathf.Clamp(value, 1, 256);
            }
        }

        public static GStepParams Create()
        {
            GStepParams param = new GStepParams();
            param.Count = 256;
            return param;
        }
    }
}
#endif
