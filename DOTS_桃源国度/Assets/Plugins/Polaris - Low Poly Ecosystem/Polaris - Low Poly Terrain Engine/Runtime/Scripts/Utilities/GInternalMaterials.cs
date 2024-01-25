#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GInternalMaterials
    {
        private static Material unlitTextureMaterial;
        public static Material UnlitTextureMaterial
        {
            get
            {
                if (unlitTextureMaterial == null)
                {
                    unlitTextureMaterial = new Material(Shader.Find("Unlit/Texture"));
                }
                return unlitTextureMaterial;
            }
        }

        private static Material unlitTransparentMaterial;
        public static Material UnlitTransparentMaterial
        {
            get
            {
                if (unlitTransparentMaterial == null)
                {
                    unlitTransparentMaterial = new Material(Shader.Find("Unlit/Transparent"));
                }
                return unlitTransparentMaterial;
            }
        }

        private static Material copyTextureMaterial;
        public static Material CopyTextureMaterial
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

        private static Material subDivisionMapMaterial;
        public static Material SubDivisionMapMaterial
        {
            get
            {
                if (subDivisionMapMaterial == null)
                {
                    subDivisionMapMaterial = new Material(GRuntimeSettings.Instance.internalShaders.subDivisionMapShader);
                    subDivisionMapMaterial.SetFloat("_Epsilon", GCommon.SUB_DIV_EPSILON);
                    subDivisionMapMaterial.SetFloat("_PixelOffset", GCommon.SUB_DIV_PIXEL_OFFSET);
                    subDivisionMapMaterial.SetFloat("_Step", GCommon.SUB_DIV_STEP);
                }
                return subDivisionMapMaterial;
            }
        }

        private static Material blurMaterial;
        public static Material BlurMaterial
        {
            get
            {
                if (blurMaterial == null)
                {
                    blurMaterial = new Material(GRuntimeSettings.Instance.internalShaders.blurShader);
                }
                return blurMaterial;
            }
        }

        private static Material blurRadiusMaterial;
        public static Material BlurRadiusMaterial
        {
            get
            {
                if (blurRadiusMaterial == null)
                {
                    blurRadiusMaterial = new Material(GRuntimeSettings.Instance.internalShaders.blurRadiusShader);
                }
                return blurRadiusMaterial;
            }
        }

        


        private static Material painterCursorProjectorMaterial;
        public static Material PainterCursorProjectorMaterial
        {
            get
            {
                if (painterCursorProjectorMaterial == null)
                {
                    painterCursorProjectorMaterial = new Material(GRuntimeSettings.Instance.internalShaders.painterCursorProjectorShader);
                }
                return painterCursorProjectorMaterial;
            }
        }

        private static Material solidColorMaterial;
        public static Material SolidColorMaterial
        {
            get
            {
                if (solidColorMaterial == null)
                {
                    solidColorMaterial = new Material(GRuntimeSettings.Instance.internalShaders.solidColorShader);
                }
                return solidColorMaterial;
            }
        }


        private static Material rampMakerMaterial;
        public static Material RampMakerMaterial
        {
            get
            {
                if (rampMakerMaterial == null)
                {
                    rampMakerMaterial = new Material(GRuntimeSettings.Instance.internalShaders.rampMakerShader);
                }
                return rampMakerMaterial;
            }
        }

        private static Material geometryLivePreviewMaterial;
        public static Material GeometryLivePreviewMaterial
        {
            get
            {
                if (geometryLivePreviewMaterial == null)
                {
                    geometryLivePreviewMaterial = new Material(GRuntimeSettings.Instance.internalShaders.geometryLivePreviewShader);
                }
                return geometryLivePreviewMaterial;
            }
        }

        private static Material geometricalHeightMapMaterial;
        public static Material GeometricalHeightMapMaterial
        {
            get
            {
                if (geometricalHeightMapMaterial == null)
                {
                    geometricalHeightMapMaterial = new Material(GRuntimeSettings.Instance.internalShaders.geometricalHeightMapShader);
                }
                return geometricalHeightMapMaterial;
            }
        }

        private static Material foliageRemoverMaterial;
        public static Material FoliageRemoverMaterial
        {
            get
            {
                if (foliageRemoverMaterial == null)
                {
                    foliageRemoverMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splineMaskShader);
                }
                return foliageRemoverMaterial;
            }
        }

        private static Material maskVisualizerMaterial;
        public static Material MaskVisualizerMaterial
        {
            get
            {
                if (maskVisualizerMaterial == null)
                {
                    maskVisualizerMaterial = new Material(GRuntimeSettings.Instance.internalShaders.maskVisualizerShader);
                }
                return maskVisualizerMaterial;
            }
        }

        private static Material stamperMaterial;
        public static Material StamperMaterial
        {
            get
            {
                if (stamperMaterial == null)
                {
                    stamperMaterial = new Material(GRuntimeSettings.Instance.internalShaders.stamperShader);
                }
                return stamperMaterial;
            }
        }

        private static Material terrainNormalMapRendererMaterial;
        public static Material TerrainNormalMapRendererMaterial
        {
            get
            {
                if (terrainNormalMapRendererMaterial == null)
                {
                    terrainNormalMapRendererMaterial = new Material(GRuntimeSettings.Instance.internalShaders.terrainNormalMapShader);
                }
                return terrainNormalMapRendererMaterial;
            }
        }

        private static Material terrainPerPixelNormalMapRendererMaterial;
        public static Material TerrainPerPixelNormalMapRendererMaterial
        {
            get
            {
                if (terrainPerPixelNormalMapRendererMaterial == null)
                {
                    terrainPerPixelNormalMapRendererMaterial = new Material(GRuntimeSettings.Instance.internalShaders.terrainPerPixelNormalMapShader);
                }
                return terrainPerPixelNormalMapRendererMaterial;
            }
        }

        private static Material textureStamperBrushMaterial;
        public static Material TextureStamperBrushMaterial
        {
            get
            {
                if (textureStamperBrushMaterial == null)
                {
                    textureStamperBrushMaterial = new Material(GRuntimeSettings.Instance.internalShaders.textureStamperBrushShader);
                }
                return textureStamperBrushMaterial;
            }
        }

        private static Material grassPreviewMaterial;
        public static Material GrassPreviewMaterial
        {
            get
            {
                if (grassPreviewMaterial == null)
                {
                    grassPreviewMaterial = new Material(GRuntimeSettings.Instance.internalShaders.grassPreviewShader);
                }
                return grassPreviewMaterial;
            }
        }

        private static Material navHelperDummyGameObjectMaterial;
        public static Material NavHelperDummyGameObjectMaterial
        {
            get
            {
                if (navHelperDummyGameObjectMaterial == null)
                {
                    navHelperDummyGameObjectMaterial = new Material(GRuntimeSettings.Instance.internalShaders.navHelperDummyGameObjectShader);
                }
                Color cyan = Color.cyan;
                navHelperDummyGameObjectMaterial.SetColor("_Color", new Color(cyan.r, cyan.g, cyan.b, 0.5f));
                return navHelperDummyGameObjectMaterial;
            }
        }

        private static Material splatsToAlbedoMaterial;
        public static Material SplatsToAlbedoMaterial
        {
            get
            {
                if (splatsToAlbedoMaterial == null)
                {
                    splatsToAlbedoMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splatsToAlbedoShader);
                }
                return splatsToAlbedoMaterial;
            }
        }

        private static Material unlitChannelMaskMaterial;
        public static Material UnlitChannelMaskMaterial
        {
            get
            {
                if (unlitChannelMaskMaterial == null)
                {
                    unlitChannelMaskMaterial = new Material(GRuntimeSettings.Instance.internalShaders.unlitChannelMaskShader);
                }
                return unlitChannelMaskMaterial;
            }
        }

        private static Material channelToGrayscaleMaterial;
        public static Material ChannelToGrayscaleMaterial
        {
            get
            {
                if (channelToGrayscaleMaterial == null)
                {
                    channelToGrayscaleMaterial = new Material(GRuntimeSettings.Instance.internalShaders.channelToGrayscaleShader);
                }
                return channelToGrayscaleMaterial;
            }
        }

        private static Material heightMapFromMeshMaterial;
        public static Material HeightMapFromMeshMaterial
        {
            get
            {
                if (heightMapFromMeshMaterial == null)
                {
                    heightMapFromMeshMaterial = new Material(GRuntimeSettings.Instance.internalShaders.heightMapFromMeshShader);
                }
                return heightMapFromMeshMaterial;
            }
        }

        private static Material interactiveGrassVectorFieldMaterial;
        public static Material InteractiveGrassVectorFieldMaterial
        {
            get
            {
                if (interactiveGrassVectorFieldMaterial == null)
                {
                    interactiveGrassVectorFieldMaterial = new Material(GRuntimeSettings.Instance.internalShaders.interactiveGrassVectorFieldShader);
                }
                return interactiveGrassVectorFieldMaterial;
            }
        }

        private static Material subdivLivePreviewMaterial;
        public static Material SubdivLivePreviewMaterial
        {
            get
            {
                if (subdivLivePreviewMaterial == null)
                {
                    subdivLivePreviewMaterial = new Material(GRuntimeSettings.Instance.internalShaders.subdivLivePreviewShader);
                }
                return subdivLivePreviewMaterial;
            }
        }

        private static Material visibilityLivePreviewMaterial;
        public static Material VisibilityLivePreviewMaterial
        {
            get
            {
                if (visibilityLivePreviewMaterial == null)
                {
                    visibilityLivePreviewMaterial = new Material(GRuntimeSettings.Instance.internalShaders.visibilityLivePreviewShader);
                }
                return visibilityLivePreviewMaterial;
            }
        }

        private static Material heightmapConverterEncodeRGMaterial;
        public static Material HeightmapConverterEncodeRGMaterial
        {
            get
            {
                if (heightmapConverterEncodeRGMaterial == null)
                {
                    heightmapConverterEncodeRGMaterial = new Material(GRuntimeSettings.Instance.internalShaders.heightmapConverterEncodeRGShader);
                }
                return heightmapConverterEncodeRGMaterial;
            }
        }

        private static Material heightmapDecodeGrayscaleMaterial;
        public static Material HeightmapDecodeGrayscaleMaterial
        {
            get
            {
                if (heightmapDecodeGrayscaleMaterial == null)
                {
                    heightmapDecodeGrayscaleMaterial = new Material(GRuntimeSettings.Instance.internalShaders.heightmapDecodeGrayscaleShader);
                }
                return heightmapDecodeGrayscaleMaterial;
            }
        }

        private static Material drawTex2DArraySliceMaterial;
        public static Material DrawTex2DArraySliceMaterial
        {
            get
            {
                if (drawTex2DArraySliceMaterial == null)
                {
                    drawTex2DArraySliceMaterial = new Material(GRuntimeSettings.Instance.internalShaders.drawTex2DArraySliceShader);
                }
                return drawTex2DArraySliceMaterial;
            }
        }

        private static Material mask4ChannelsMaterial;
        public static Material Mask4ChannelsMaterial
        {
            get
            {
                if (mask4ChannelsMaterial == null)
                {
                    mask4ChannelsMaterial = new Material(GRuntimeSettings.Instance.internalShaders.mask4ChannelsShader);
                }
                return mask4ChannelsMaterial;
            }
        }
    }
}
#endif
