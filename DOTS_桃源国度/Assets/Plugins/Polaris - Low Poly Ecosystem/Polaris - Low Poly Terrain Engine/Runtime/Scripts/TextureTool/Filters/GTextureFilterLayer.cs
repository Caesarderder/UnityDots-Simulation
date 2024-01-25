#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public class GTextureFilterLayer
    {
        [SerializeField]
        private bool enabled;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        [SerializeField]
        private GTextureFilterType type;
        public GTextureFilterType Type
        {
            get
            {
                return type;
            }
        }

        [SerializeField]
        private GTextureFilterParams param;
        public GTextureFilterParams Param
        {
            get
            {
                return param;
            }
            set
            {
                param = value;
            }
        }

        private GTextureFilterLayer()
        {
            enabled = true;
            type = GTextureFilterType.Curve;
        }

        public GTextureFilterLayer(GTextureFilterType t)
        {
            enabled = true;
            type = t;
            param = GTextureFilterParams.Default;
        }

        public void Apply(RenderTexture targetRt)
        {
            if (!Enabled)
                return;
            if (Type == GTextureFilterType.Curve)
            {
                new GCurveFilter().Apply(targetRt, param);
            }
            else if (Type == GTextureFilterType.Blur)
            {
                new GBlurFilter().Apply(targetRt, param);
            }
            else if (Type == GTextureFilterType.Invert)
            {
                new GInvertFilter().Apply(targetRt, param);
            }
            else if (Type == GTextureFilterType.Step)
            {
                new GStepFilter().Apply(targetRt, param);
            }
            else if (Type == GTextureFilterType.Warp)
            {
                new GWarpFilter().Apply(targetRt, param);
            }
        }
    }
}
#endif
