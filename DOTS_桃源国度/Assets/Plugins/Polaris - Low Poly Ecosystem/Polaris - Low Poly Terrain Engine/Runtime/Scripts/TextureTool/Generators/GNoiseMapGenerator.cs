#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GNoiseMapGenerator : IGTextureGenerator
    {
        private static Material mat;
        public static Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.noiseMapGeneratorShader);
                }
                return mat;
            }
        }

        public void Generate(RenderTexture targetRt)
        {
            GNoiseMapGeneratorParams param = GTextureToolParams.Instance.Noise;
            Generate(targetRt, param);
        }

        public void Generate(RenderTexture targetRt, GNoiseMapGeneratorParams param)
        {
            Mat.SetVector("_Origin", param.Origin);
            Mat.SetFloat("_Frequency", param.Frequency);
            Mat.SetFloat("_Lacunarity", param.Lacunarity);
            Mat.SetFloat("_Persistence", param.Persistence);
            Mat.SetInt("_Octaves", param.Octaves);
            Mat.SetFloat("_Seed", param.Seed);

            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, Mat, (int)param.Type);
        }
    }
}
#endif
