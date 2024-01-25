#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GBlendMapGenerator : IGTextureGenerator
    {
        private static Material mat;
        public static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.blendMapGeneratorShader);
                }
                return mat;
            }
        }

        public void Generate(RenderTexture targetRt)
        {
            GBlendMapGeneratorParams param = GTextureToolParams.Instance.Blend;

            RenderTexture bg = new RenderTexture(targetRt);
            GCommon.CopyToRT(targetRt, bg);

            GCommon.FillTexture(targetRt, Color.black);
            for (int i = 0; i < param.Layers.Count; ++i)
            {
                GBlendLayer l = param.Layers[i];
                Mat.SetTexture("_Background", bg);
                Mat.SetTexture("_Foreground", l.Texture);
                Mat.SetFloat("_Number", l.Number);
                Mat.SetVector("_Vector", l.Vector);
                Mat.SetInt("_Ops", (int)l.BlendOps);
                Mat.SetFloat("_LerpFactor", l.LerpFactor);
                Mat.SetTexture("_LerpMask", l.LerpMask);
                Mat.SetInt("_Saturate", l.Saturate ? 1 : 0);

                GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, Mat, (int)l.DataSource);
                GCommon.CopyToRT(targetRt, bg);
            }

            bg.Release();
            GUtilities.DestroyObject(bg);
        }
    }
}
#endif
