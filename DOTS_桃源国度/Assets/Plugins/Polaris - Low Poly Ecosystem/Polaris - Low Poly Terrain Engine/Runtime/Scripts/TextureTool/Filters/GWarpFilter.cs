#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GWarpFilter : IGTextureFilter
    {
        private static RenderTexture bgRt;

        private static Material mat;
        private static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.warpFilterShader);
                }
                return mat;
            }
        }

        public void Apply(RenderTexture targetRt, GTextureFilterParams param)
        {
            GWarpParams warpParam = param.Warp;
            RenderTexture bg = CloneBg(targetRt);
            Mat.SetTexture("_MainTex", bg);
            Mat.SetTexture("_Mask", warpParam.UseBackgroundAsMask ? (Texture)bg : (Texture)warpParam.Mask);
            Mat.SetFloat("_Strength", warpParam.Strength);
            GCommon.SetMaterialKeywordActive(Mat, "MASK_IS_NORMAL", warpParam.MaskIsNormalMap);

            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, Mat, 0);
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
