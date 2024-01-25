//////////////////////////////////////////////////////
// MK Toon Uniforms   	    	   	                //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

namespace MK.Toon
{
    public static class Uniforms
    {
        /////////////////
        // Options     //
        /////////////////
        public static readonly Uniform workflow      = new Uniform("_Workflow");
        public static readonly Uniform renderFace    = new Uniform("_RenderFace");
        public static readonly Uniform surface       = new Uniform("_Surface");
        public static readonly Uniform zWrite        = new Uniform("_ZWrite");
        public static readonly Uniform zTest         = new Uniform("_ZTest");
        public static readonly Uniform blendSrc      = new Uniform("_BlendSrc");
        public static readonly Uniform blendDst      = new Uniform("_BlendDst");
        public static readonly Uniform blendSrcAlpha = new Uniform("_BlendSrcAlpha");
        public static readonly Uniform blendDstAlpha = new Uniform("_BlendDstAlpha");
        public static readonly Uniform blend         = new Uniform("_Blend");
        public static readonly Uniform alphaClipping = new Uniform("_AlphaClipping");

        /////////////////
        // Input       //
        /////////////////
        public static readonly Uniform albedoColor                 = new Uniform("_AlbedoColor");
        public static readonly Uniform alphaCutoff                 = new Uniform("_AlphaCutoff");
        public static readonly Uniform albedoMap                   = new Uniform("_AlbedoMap");
        public static readonly Uniform specularColor               = new Uniform("_SpecularColor");
        public static readonly Uniform metallic                    = new Uniform("_Metallic");
        public static readonly Uniform smoothness                  = new Uniform("_Smoothness");
        public static readonly Uniform roughness                   = new Uniform("_Roughness");
        public static readonly Uniform specularMap                 = new Uniform("_SpecularMap");
        public static readonly Uniform roughnessMap                = new Uniform("_RoughnessMap");
        public static readonly Uniform metallicMap                 = new Uniform("_MetallicMap");
        public static readonly Uniform normalMapIntensity          = new Uniform("_NormalMapIntensity");
        public static readonly Uniform normalMap                   = new Uniform("_NormalMap");
        public static readonly Uniform parallax                    = new Uniform("_Parallax");
        public static readonly Uniform heightMap                   = new Uniform("_HeightMap");
        public static readonly Uniform lightTransmission           = new Uniform("_LightTransmission");
        public static readonly Uniform lightTransmissionDistortion = new Uniform("_LightTransmissionDistortion");
        public static readonly Uniform lightTransmissionColor      = new Uniform("_LightTransmissionColor");
        public static readonly Uniform thicknessMap                = new Uniform("_ThicknessMap");
        public static readonly Uniform occlusionMapIntensity       = new Uniform("_OcclusionMapIntensity");
        public static readonly Uniform occlusionMap                = new Uniform("_OcclusionMap");
        public static readonly Uniform emissionColor               = new Uniform("_EmissionColor");
        public static readonly Uniform emissionMap                 = new Uniform("_EmissionMap");

        /////////////////
        // Detail      //
        /////////////////
        public static readonly Uniform detailBlend              = new Uniform("_DetailBlend");
        public static readonly Uniform detailColor              = new Uniform("_DetailColor");
        public static readonly Uniform detailMix                = new Uniform("_DetailMix");
        public static readonly Uniform detailMap                = new Uniform("_DetailMap");
        public static readonly Uniform detailNormalMapIntensity = new Uniform("_DetailNormalMapIntensity");
        public static readonly Uniform detailNormalMap          = new Uniform("_DetailNormalMap");

        /////////////////
        // Stylize     //
        /////////////////
        public static readonly Uniform receiveShadows                   = new Uniform("_ReceiveShadows");
        public static readonly Uniform wrappedLighting                  = new Uniform("_WrappedLighting");
        public static readonly Uniform diffuseSmoothness                = new Uniform("_DiffuseSmoothness");
        public static readonly Uniform diffuseThresholdOffset           = new Uniform("_DiffuseThresholdOffset");
        public static readonly Uniform specularSmoothness               = new Uniform("_SpecularSmoothness");
        public static readonly Uniform specularThresholdOffset          = new Uniform("_SpecularThresholdOffset");
        public static readonly Uniform rimSmoothness                    = new Uniform("_RimSmoothness");
        public static readonly Uniform rimThresholdOffset               = new Uniform("_RimThresholdOffset");
        public static readonly Uniform lightTransmissionSmoothness      = new Uniform("_LightTransmissionSmoothness");
        public static readonly Uniform lightTransmissionThresholdOffset = new Uniform("_LightTransmissionThresholdOffset");
        public static readonly Uniform light                            = new Uniform("_Light");
        public static readonly Uniform diffuseRamp                      = new Uniform("_DiffuseRamp");
        public static readonly Uniform specularRamp                     = new Uniform("_SpecularRamp");
        public static readonly Uniform rimRamp                          = new Uniform("_RimRamp");
        public static readonly Uniform lightTransmissionRamp            = new Uniform("_LightTransmissionRamp");
        public static readonly Uniform lightBands                       = new Uniform("_LightBands");
        public static readonly Uniform lightBandsScale                  = new Uniform("_LightBandsScale");
        public static readonly Uniform lightThreshold                   = new Uniform("_LightThreshold");
        public static readonly Uniform thresholdMap                     = new Uniform("_ThresholdMap");
        public static readonly Uniform thresholdMapScale                = new Uniform("_ThresholdMapScale");
        public static readonly Uniform goochRampIntensity               = new Uniform("_GoochRampIntensity");
        public static readonly Uniform goochRamp                        = new Uniform("_GoochRamp");
        public static readonly Uniform goochBrightColor                 = new Uniform("_GoochBrightColor");
        public static readonly Uniform goochBrightMap                   = new Uniform("_GoochBrightMap");
        public static readonly Uniform goochDarkColor                   = new Uniform("_GoochDarkColor");
        public static readonly Uniform goochDarkMap                     = new Uniform("_GoochDarkMap");
        public static readonly Uniform colorGrading                     = new Uniform("_ColorGrading");
        public static readonly Uniform contrast                         = new Uniform("_Contrast");
        public static readonly Uniform saturation                       = new Uniform("_Saturation");
        public static readonly Uniform brightness                       = new Uniform("_Brightness");
        public static readonly Uniform iridescence                      = new Uniform("_Iridescence");
        public static readonly Uniform iridescenceRamp                  = new Uniform("_IridescenceRamp");
        public static readonly Uniform iridescenceSize                  = new Uniform("_IridescenceSize");
        public static readonly Uniform iridescenceThresholdOffset       = new Uniform("_IridescenceThresholdOffset");
        public static readonly Uniform iridescenceSmoothness            = new Uniform("_IridescenceSmoothness");
        public static readonly Uniform iridescenceColor                 = new Uniform("_IridescenceColor");
        public static readonly Uniform rim                              = new Uniform("_Rim");
        public static readonly Uniform rimColor                         = new Uniform("_RimColor");
        public static readonly Uniform rimBrightColor                   = new Uniform("_RimBrightColor");
        public static readonly Uniform rimDarkColor                     = new Uniform("_RimDarkColor");
        public static readonly Uniform rimSize                          = new Uniform("_RimSize");
        public static readonly Uniform vertexAnimation                  = new Uniform("_VertexAnimation");
        public static readonly Uniform vertexAnimationStutter           = new Uniform("_VertexAnimationStutter");
        public static readonly Uniform vertexAnimationMap               = new Uniform("_VertexAnimationMap");
        public static readonly Uniform vertexAnimationIntensity         = new Uniform("_VertexAnimationIntensity");
        public static readonly Uniform vertexAnimationFrequency         = new Uniform("_VertexAnimationFrequency");
        public static readonly Uniform dissolve                         = new Uniform("_Dissolve");
        public static readonly Uniform dissolveMap                      = new Uniform("_DissolveMap");
        public static readonly Uniform dissolveMapScale                 = new Uniform("_DissolveMapScale");
        public static readonly Uniform dissolveAmount                   = new Uniform("_DissolveAmount");
        public static readonly Uniform dissolveBorderSize               = new Uniform("_DissolveBorderSize");
        public static readonly Uniform dissolveBorderRamp               = new Uniform("_DissolveBorderRamp");
        public static readonly Uniform dissolveBorderColor              = new Uniform("_DissolveBorderColor");
        public static readonly Uniform artistic                         = new Uniform("_Artistic");
        public static readonly Uniform artisticProjection               = new Uniform("_ArtisticProjection");
        public static readonly Uniform artisticFrequency                = new Uniform("_ArtisticFrequency");
        public static readonly Uniform drawnMapScale                    = new Uniform("_DrawnMapScale");
        public static readonly Uniform drawnMap                         = new Uniform("_DrawnMap");
        public static readonly Uniform hatchingMapScale                 = new Uniform("_HatchingMapScale");
        public static readonly Uniform hatchingBrightMap                = new Uniform("_HatchingBrightMap");
        public static readonly Uniform hatchingDarkMap                  = new Uniform("_HatchingDarkMap");
        public static readonly Uniform drawnClampMin                    = new Uniform("_DrawnClampMin");
        public static readonly Uniform drawnClampMax                    = new Uniform("_DrawnClampMax");
        public static readonly Uniform sketchMapScale                   = new Uniform("_SketchMapScale");
        public static readonly Uniform sketchMap                        = new Uniform("_SketchMap");

        /////////////////
        // Advanced    //
        /////////////////
        public static readonly Uniform diffuse                    = new Uniform("_Diffuse");
        public static readonly Uniform specular                   = new Uniform("_Specular");
        public static readonly Uniform specularIntensity          = new Uniform("_SpecularIntensity");
        public static readonly Uniform anisotropy                 = new Uniform("_Anisotropy");
        public static readonly Uniform lightTransmissionIntensity = new Uniform("_LightTransmissionIntensity");
        public static readonly Uniform environmentReflections     = new Uniform("_EnvironmentReflections");
        public static readonly Uniform fresnelHighlights          = new Uniform("_FresnelHighlights");
        public static readonly Uniform IndirectFade               = new Uniform("_IndirectFade");
        public static readonly Uniform alembicMotionVectors     = new Uniform("_AlembicMotionVectors");
        public static readonly Uniform stencil                    = new Uniform("_Stencil");
        public static readonly Uniform renderPriority             = new Uniform("_RenderPriority");
        public static readonly Uniform stencilRef                 = new Uniform("_StencilRef");
        public static readonly Uniform stencilReadMask            = new Uniform("_StencilReadMask");
        public static readonly Uniform stencilWriteMask           = new Uniform("_StencilWriteMask");
        public static readonly Uniform stencilComp                = new Uniform("_StencilComp");
        public static readonly Uniform stencilPass                = new Uniform("_StencilPass");
        public static readonly Uniform stencilFail                = new Uniform("_StencilFail");
        public static readonly Uniform stencilZFail               = new Uniform("_StencilZFail");
       
        /////////////////
        // Outline     //
        /////////////////
        public static readonly Uniform outline        = new Uniform("_Outline");
        public static readonly Uniform outlineData    = new Uniform("_OutlineData");
        public static readonly Uniform outlineMap     = new Uniform("_OutlineMap");
        public static readonly Uniform outlineSize    = new Uniform("_OutlineSize");
        #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
        public static readonly Uniform outlineFadeMin = new Uniform("_OutlineFadeMin");
        public static readonly Uniform outlineFadeMax = new Uniform("_OutlineFadeMax");
        #endif
        public static readonly Uniform outlineColor   = new Uniform("_OutlineColor");
        public static readonly Uniform outlineNoise   = new Uniform("_OutlineNoise");

        /////////////////
        // Refraction  //
        /////////////////
        public static readonly Uniform refractionDistortionMapScale = new Uniform("_RefractionDistortionMapScale");
        public static readonly Uniform refractionDistortionMap      = new Uniform("_RefractionDistortionMap");
        public static readonly Uniform refractionDistortion         = new Uniform("_RefractionDistortion");
        public static readonly Uniform indexOfRefraction            = new Uniform("_IndexOfRefraction");
        public static readonly Uniform refractionDistortionFade     = new Uniform("_RefractionDistortionFade");

        /////////////////
        // Particles   //
        /////////////////
        public static readonly Uniform flipbook               = new Uniform("_Flipbook");
        public static readonly Uniform softFade               = new Uniform("_SoftFade");
        public static readonly Uniform softFadeNearDistance   = new Uniform("_SoftFadeNearDistance");
        public static readonly Uniform softFadeFarDistance    = new Uniform("_SoftFadeFarDistance");
        public static readonly Uniform cameraFade             = new Uniform("_CameraFade");
        public static readonly Uniform cameraFadeNearDistance = new Uniform("_CameraFadeNearDistance");
        public static readonly Uniform cameraFadeFarDistance  = new Uniform("_CameraFadeFarDistance");
        public static readonly Uniform colorBlend             = new Uniform("_ColorBlend");

        /////////////////
        // Editor Only //
        /////////////////
        public static readonly Uniform initialized   = new Uniform("_Initialized");
        public static readonly Uniform optionsTab    = new Uniform("_OptionsTab");
        public static readonly Uniform inputTab      = new Uniform("_InputTab");
        public static readonly Uniform stylizeTab    = new Uniform("_StylizeTab");
        public static readonly Uniform advancedTab   = new Uniform("_AdvancedTab");
        public static readonly Uniform particlesTab  = new Uniform("_ParticlesTab");
        public static readonly Uniform outlineTab    = new Uniform("_OutlineTab");
        public static readonly Uniform refractionTab = new Uniform("_RefractionTab");

        /////////////////
        // System      //
        /////////////////
        public static readonly Uniform mainTex = new Uniform("_MainTex");
        public static readonly Uniform cutoff  = new Uniform("_Cutoff");
        public static readonly Uniform color  = new Uniform("_Color");
    }
}
