#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GTextureFilterParams
    {
        public static GTextureFilterParams Default
        {
            get
            {
                GTextureFilterParams param = new GTextureFilterParams();
                param.Blur = GBlurParams.Create();
                param.Curve = GCurveParams.Create();
                param.Invert = GInvertParams.Create();
                param.Step = GStepParams.Create();
                param.Warp = GWarpParams.Create();
                return param;
            }
        }

        [SerializeField]
        private GBlurParams blur;
        public GBlurParams Blur
        {
            get
            {
                return blur;
            }
            set
            {
                blur = value;
            }
        }

        [SerializeField]
        private GCurveParams curve;
        public GCurveParams Curve
        {
            get
            {
                return curve;
            }
            set
            {
                curve = value;
            }
        }

        [SerializeField]
        private GInvertParams invert;
        public GInvertParams Invert
        {
            get
            {
                return invert;
            }
            set
            {
                invert = value;
            }
        }

        [SerializeField]
        private GStepParams step;
        public GStepParams Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
            }
        }

        [SerializeField]
        private GWarpParams warp;
        public GWarpParams Warp
        {
            get
            {
                return warp;
            }
            set
            {
                warp = value;
            }
        }
    }
}
#endif
