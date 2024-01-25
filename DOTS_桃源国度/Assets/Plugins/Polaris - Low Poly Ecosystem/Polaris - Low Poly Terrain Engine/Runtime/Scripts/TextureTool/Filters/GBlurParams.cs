#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GBlurParams
    {
        [SerializeField]
        private int radius;
        public int Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = Mathf.Clamp(value, 0, 20);
            }
        }

        public static GBlurParams Create()
        {
            GBlurParams param = new GBlurParams();
            param.radius = 10;
            return param;
        }
    }
}
#endif
