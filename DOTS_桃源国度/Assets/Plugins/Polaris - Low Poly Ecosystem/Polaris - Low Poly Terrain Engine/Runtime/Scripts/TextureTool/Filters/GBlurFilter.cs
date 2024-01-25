#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GBlurFilter : IGTextureFilter
    {
        private static RenderTexture bgRt;

        private static Material mat;
        private static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.blurRadiusShader);
                }
                return mat;
            }
        }

        public void Apply(RenderTexture targetRt, GTextureFilterParams param)
        {
            GBlurParams blurParam = param.Blur;
            RenderTexture bg = CloneBg(targetRt);

            Mat.SetTexture("_MainTex", bg);
            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, Mat, blurParam.Radius);
        }

        private RenderTexture CloneBg(RenderTexture targetRt)
        {
            if (bgRt == null)
            {
                bgRt = new RenderTexture(targetRt);
            }
            else if (bgRt.width != targetRt.width || bgRt.height != targetRt.height || bgRt.format != targetRt.format)
            {
                bgRt.Release();
                GUtilities.DestroyObject(bgRt);
                bgRt = new RenderTexture(targetRt);
            }

            GCommon.CopyToRT(targetRt, bgRt);
            return bgRt;
        }
    }
}
#endif
