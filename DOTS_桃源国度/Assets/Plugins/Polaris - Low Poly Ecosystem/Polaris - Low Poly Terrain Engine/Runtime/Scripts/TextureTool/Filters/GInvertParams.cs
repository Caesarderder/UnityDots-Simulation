#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GInvertParams
    {
        [SerializeField]
        private bool invertRed;
        public bool InvertRed
        {
            get
            {
                return invertRed;
            }
            set
            {
                invertRed = value;
            }
        }

        [SerializeField]
        private bool invertGreen;
        public bool InvertGreen
        {
            get
            {
                return invertGreen;
            }
            set
            {
                invertGreen = value;
            }
        }

        [SerializeField]
        private bool invertBlue;
        public bool InvertBlue
        {
            get
            {
                return invertBlue;
            }
            set
            {
                invertBlue = value;
            }
        }

        [SerializeField]
        private bool invertAlpha;
        public bool InvertAlpha
        {
            get
            {
                return invertAlpha;
            }
            set
            {
                invertAlpha = value;
            }
        }

        public static GInvertParams Create()
        {
            GInvertParams param = new GInvertParams();
            param.invertRed = true;
            param.invertGreen = true;
            param.invertBlue = true;
            param.invertAlpha = true;
            return param;
        }
    }
}
#endif
