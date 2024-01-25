#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    [System.Serializable]
    public struct GInternalShaderSettings
    {
        public Shader solidColorShader;
        public Shader copyTextureShader;
        public Shader subDivisionMapShader;
        public Shader blurShader;
        public Shader blurRadiusShader;
        public Shader elevationPainterShader;
        public Shader heightSamplingPainterShader;
        public Shader subdivPainterShader;
        public Shader painterCursorProjectorShader;
        public Shader albedoPainterShader;
        public Shader metallicPainterShader;
        public Shader smoothnessPainterShader;
        public Shader splatPainterShader;
        public Shader visibilityPainterShader;
        public Shader rampMakerShader;
        public Shader pathPainterShader;
        public Shader geometryLivePreviewShader;
        public Shader geometricalHeightMapShader;
        public Shader foliageRemoverShader;
        public Shader maskVisualizerShader;
        public Shader stamperShader;
        public Shader terrainNormalMapShader;
        public Shader terrainPerPixelNormalMapShader;
        public Shader textureStamperBrushShader;
        public Shader grassPreviewShader;
        public Shader navHelperDummyGameObjectShader;
        public Shader splatsToAlbedoShader;
        public Shader unlitChannelMaskShader;
        public Shader channelToGrayscaleShader;
        public Shader heightMapFromMeshShader;
        public Shader curveFilterShader;
        public Shader invertFilterShader;
        public Shader stepFilterShader;
        public Shader warpFilterShader;
        public Shader steepnessMapGeneratorShader;
        public Shader noiseMapGeneratorShader;
        public Shader hydraulicErosionFilter;
        public Shader blendMapGeneratorShader;
        public Shader distributionMapGeneratorShader;
        public Shader interactiveGrassVectorFieldShader;
        public Shader subdivLivePreviewShader;
        public Shader visibilityLivePreviewShader;
        public Shader terracePainterShader;
        public Shader remapPainterShader;
        public Shader noisePainterShader;
        public Shader heightmapConverterEncodeRGShader;
        public Shader heightmapDecodeGrayscaleShader;
        public Shader drawTex2DArraySliceShader;
        public Shader maskPainterShader;
        public Shader mask4ChannelsShader;
    }
}
#endif
