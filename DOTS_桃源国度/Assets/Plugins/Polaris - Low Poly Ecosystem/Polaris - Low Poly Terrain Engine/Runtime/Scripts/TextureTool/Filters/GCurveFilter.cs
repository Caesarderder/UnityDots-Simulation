#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GCurveFilter : IGTextureFilter
    {
        private static RenderTexture bgRt;

        private static Material mat;
        private static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.curveFilterShader);
                }
                return mat;
            }
        }

        public void Apply(RenderTexture targetRt, GTextureFilterParams param)
        {
            GCurveParams curveParam = param.Curve;
            RenderTexture bg = CloneBg(targetRt);
            Texture2D masterCurveTex = GCommon.CreateTextureFromCurve(curveParam.MasterCurve, 1024, 1);
            Texture2D redCurveTex = GCommon.CreateTextureFromCurve(curveParam.RedCurve, 1024, 1);
            Texture2D greenCurveTex = GCommon.CreateTextureFromCurve(curveParam.GreenCurve, 1024, 1);
            Texture2D blueCurveTex = GCommon.CreateTextureFromCurve(curveParam.BlueCurve, 1024, 1);
            Texture2D alphaCurveTex = GCommon.CreateTextureFromCurve(curveParam.AlphaCurve, 1024, 1);
            Mat.SetTexture("_MainTex", bg);
            Mat.SetTexture("_MasterCurve", masterCurveTex);
            Mat.SetTexture("_RedCurve", redCurveTex);
            Mat.SetTexture("_GreenCurve", greenCurveTex);
            Mat.SetTexture("_BlueCurve", blueCurveTex);
            Mat.SetTexture("_AlphaCurve", alphaCurveTex);
            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, Mat, 0);

            GUtilities.DestroyObject(masterCurveTex);
            GUtilities.DestroyObject(redCurveTex);
            GUtilities.DestroyObject(greenCurveTex);
            GUtilities.DestroyObject(blueCurveTex);
            GUtilities.DestroyObject(alphaCurveTex);
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
