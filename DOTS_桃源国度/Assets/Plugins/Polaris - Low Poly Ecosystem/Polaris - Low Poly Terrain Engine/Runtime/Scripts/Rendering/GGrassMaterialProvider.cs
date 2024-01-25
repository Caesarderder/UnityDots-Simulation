#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.Rendering
{
    public static class GGrassMaterialProvider
    {
        public const string FADE_KW = "FADE";

        public static Material GetMaterial(bool isInteractiveGrassEnabled, bool isBillboardEnabled)
        {
            Material mat;
            if (isInteractiveGrassEnabled)
            {
                mat = GetInteractiveGrassMaterial();
            }
            else
            {
                mat = GetNonInteractiveGrassMaterial(isBillboardEnabled);
            }
            if (mat != null)
            {
                mat.EnableKeyword(FADE_KW);
            }
            return mat;
        }

        private static Material GetInteractiveGrassMaterial()
        {
            GRenderPipelineType pipeline = GCommon.CurrentRenderPipeline;
            if (pipeline == GRenderPipelineType.Builtin)
            {
                return GRuntimeSettings.Instance.foliageRendering.grassInteractiveMaterial;
            }
            else if (pipeline == GRenderPipelineType.Universal)
            {
                return GRuntimeSettings.Instance.foliageRendering.urpGrassInteractiveMaterial;
            }
            else
            {
                return null;
            }
        }

        private static Material GetNonInteractiveGrassMaterial(bool isBillboardEnabled)
        {
            if (isBillboardEnabled)
            {
                return GetNonInteractiveBillboardGrassMaterial();
            }
            else
            {
                return GetNonInteractiveNonBillboardGrassMaterial();
            }
        }

        private static Material GetNonInteractiveNonBillboardGrassMaterial()
        {
            GRenderPipelineType pipeline = GCommon.CurrentRenderPipeline;
            if (pipeline == GRenderPipelineType.Builtin)
            {
                return GRuntimeSettings.Instance.foliageRendering.grassMaterial;
            }
            else if (pipeline == GRenderPipelineType.Universal)
            {
                return GRuntimeSettings.Instance.foliageRendering.urpGrassMaterial;
            }
            else
            {
                return null;
            }
        }

        private static Material GetNonInteractiveBillboardGrassMaterial()
        {
            GRenderPipelineType pipeline = GCommon.CurrentRenderPipeline;
            if (pipeline == GRenderPipelineType.Builtin)
            {
                return GRuntimeSettings.Instance.foliageRendering.grassBillboardMaterial;
            }
            else if (pipeline == GRenderPipelineType.Universal)
            {
                return GRuntimeSettings.Instance.foliageRendering.urpGrassBillboardMaterial;
            }
            else
            {
                return null;
            }
        }
    }
}
#endif
