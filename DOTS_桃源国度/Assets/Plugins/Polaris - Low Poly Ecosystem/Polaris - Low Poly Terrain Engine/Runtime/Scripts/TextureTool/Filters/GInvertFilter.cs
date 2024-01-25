#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GInvertFilter : IGTextureFilter
    {
        private static RenderTexture bgRt;

        private static Material mat;
        private static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.invertFilterShader);
                }
                return mat;
            }
        }

        public void Apply(RenderTexture targetRt, GTextureFilterParams param)
        {
            GInvertParams invertParam = param.Invert;
            RenderTexture bg = CloneBg(targetRt);
            Mat.SetTexture("_MainTex", bg);
            Mat.SetInt("_InvertRed", invertParam.InvertRed ? 1 : 0);
            Mat.SetInt("_InvertGreen", invertParam.InvertGreen ? 1 : 0);
            Mat.SetInt("_InvertBlue", invertParam.InvertBlue ? 1 : 0);
            Mat.SetInt("_InvertAlpha", invertParam.InvertAlpha ? 1 : 0);
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
