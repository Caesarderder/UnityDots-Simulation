#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.SplineTool
{
    public static class GSplineToolUtilities
    {
        private static Material copyTextureMaterial;
        private static Material CopyTextureMaterial
        {
            get
            {
                if (copyTextureMaterial == null)
                {
                    copyTextureMaterial = new Material(GRuntimeSettings.Instance.internalShaders.copyTextureShader);
                }
                return copyTextureMaterial;
            }
        }

        public static List<GStylizedTerrain> OverlapTest(int groupId, GSplineCreator spline)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>();
            GCommon.ForEachTerrain(groupId, (t) =>
            {
                if (spline.OverlapTest(t))
                {
                    terrains.Add(t);
                }
            });

            return terrains;
        }

        public static void CopyTexture(Texture src, RenderTexture dst)
        {
            CopyTextureMaterial.SetVector("_StartUV", Vector4.zero);
            CopyTextureMaterial.SetVector("_EndUV", Vector4.one);
            CopyTextureMaterial.SetColor("_DefaultColor", Color.black);
            CopyTextureMaterial.SetTexture("_MainTex", src);
            GCommon.DrawQuad(dst, GCommon.FullRectUvPoints, CopyTextureMaterial, 0);
        }
    }
}
#endif
