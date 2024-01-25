//////////////////////////////////////////////////////
// MK Toon Properties								//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

namespace MK.Toon
{
    public static class Properties
    {
        internal static readonly string shaderComponentOutlineName = "Outline";
        internal static readonly string shaderComponentRefractionName = "Refraction";
        internal static readonly string shaderVariantPBSName = "Physically Based";
        internal static readonly string shaderVariantSimpleName = "Simple";
        internal static readonly string shaderVariantUnlitName = "Unlit";

        /////////////////
        // Options     //
        /////////////////
        public static readonly EnumProperty<Workflow> workflow          = new EnumProperty<Workflow>(Uniforms.workflow, Keywords.workflow);
        public static readonly EnumProperty<RenderFace> renderFace      = new EnumProperty<RenderFace>(Uniforms.renderFace);
        public static readonly SurfaceProperty surface                  = new SurfaceProperty(Uniforms.surface, Keywords.surface);
        public static readonly EnumProperty<ZWrite> zWrite              = new EnumProperty<ZWrite>(Uniforms.zWrite);
        public static readonly EnumProperty<ZTest> zTest                = new EnumProperty<ZTest>(Uniforms.zTest);
        public static readonly EnumProperty<BlendFactor> blendSrc       = new EnumProperty<BlendFactor>(Uniforms.blendSrc);
        public static readonly EnumProperty<BlendFactor> blendDst       = new EnumProperty<BlendFactor>(Uniforms.blendDst);
        public static readonly EnumProperty<BlendFactor> blendSrcAlpha  = new EnumProperty<BlendFactor>(Uniforms.blendSrcAlpha);
        public static readonly EnumProperty<BlendFactor> blendDstAlpha  = new EnumProperty<BlendFactor>(Uniforms.blendDstAlpha);
        public static readonly BlendProperty blend                      = new BlendProperty(Uniforms.blend, Keywords.blend);
        public static readonly AlphaClippingProperty alphaClipping      = new AlphaClippingProperty(Uniforms.alphaClipping, Keywords.alphaClipping);

        /////////////////
        // Input       //
        /////////////////
        public static readonly ColorProperty albedoColor                         = new ColorProperty(Uniforms.albedoColor);
        public static readonly RangeProperty alphaCutoff                         = new RangeProperty(Uniforms.alphaCutoff, 0, 1);
        public static readonly TextureProperty albedoMap                         = new TextureProperty(Uniforms.albedoMap, Keywords.albedoMap);
        public static readonly TilingProperty mainTiling                         = new TilingProperty(Uniforms.albedoMap);
        public static readonly OffsetProperty mainOffset                         = new OffsetProperty(Uniforms.albedoMap);
        public static readonly ColorProperty specularColor                       = new ColorProperty(Uniforms.specularColor);
        public static readonly RangeProperty metallic                            = new RangeProperty(Uniforms.metallic, 0, 1);
        public static readonly RangeProperty smoothness                          = new RangeProperty(Uniforms.smoothness, 0, 1);
        public static readonly RangeProperty roughness                           = new RangeProperty(Uniforms.roughness, 0, 1);
        public static readonly TextureProperty specularMap                       = new TextureProperty(Uniforms.specularMap, Keywords.pbsMap0);
        public static readonly TextureProperty roughnessMap                      = new TextureProperty(Uniforms.roughnessMap, Keywords.pbsMap1);
        public static readonly TextureProperty metallicMap                       = new TextureProperty(Uniforms.metallicMap, Keywords.pbsMap0);
        public static readonly FloatProperty normalMapIntensity                  = new FloatProperty(Uniforms.normalMapIntensity);
        public static readonly TextureProperty normalMap                         = new TextureProperty(Uniforms.normalMap, Keywords.normalMap);
        public static readonly RangeProperty parallax                            = new RangeProperty(Uniforms.parallax, Keywords.parallax, 0, 0.1f);
        public static readonly TextureProperty heightMap                         = new TextureProperty(Uniforms.heightMap, Keywords.heightMap);
        public static readonly EnumProperty<LightTransmission> lightTransmission = new EnumProperty<LightTransmission>(Uniforms.lightTransmission);
        public static readonly RangeProperty lightTransmissionDistortion         = new RangeProperty(Uniforms.lightTransmissionDistortion, 0, 1);
        public static readonly ColorProperty lightTransmissionColor              = new ColorProperty(Uniforms.lightTransmissionColor);
        public static readonly TextureProperty thicknessMap                      = new TextureProperty(Uniforms.thicknessMap, Keywords.thicknessMap);
        public static readonly RangeProperty occlusionMapIntensity               = new RangeProperty(Uniforms.occlusionMapIntensity, 0, 1);
        public static readonly TextureProperty occlusionMap                      = new TextureProperty(Uniforms.occlusionMap, Keywords.occlusionMap);
        public static readonly ColorProperty emissionColor                       = new ColorProperty(Uniforms.emissionColor, Keywords.emission);
        public static readonly TextureProperty emissionMap                       = new TextureProperty(Uniforms.emissionMap, Keywords.emissionMap);

        /////////////////
        // Detail      //
        /////////////////
        public static readonly EnumProperty<DetailBlend> detailBlend  = new EnumProperty<DetailBlend>(Uniforms.detailBlend, Keywords.detailBlend);
        public static readonly ColorProperty detailColor              = new ColorProperty(Uniforms.detailColor);
        public static readonly RangeProperty detailMix                = new RangeProperty(Uniforms.detailMix, 0, 1);
        public static readonly TextureProperty detailMap              = new TextureProperty(Uniforms.detailMap);
        public static readonly TilingProperty detailTiling            = new TilingProperty(Uniforms.detailMap);
        public static readonly OffsetProperty detailOffset            = new OffsetProperty(Uniforms.detailMap);
        public static readonly FloatProperty detailNormalMapIntensity = new FloatProperty(Uniforms.detailNormalMapIntensity);
        public static readonly TextureProperty detailNormalMap        = new TextureProperty(Uniforms.detailNormalMap);

        /////////////////
        // Stylize     //
        /////////////////
        public static readonly BoolProperty receiveShadows                         = new BoolProperty(Uniforms.receiveShadows, Keywords.receiveShadows);
        public static readonly BoolProperty wrappedLighting                        = new BoolProperty(Uniforms.wrappedLighting, Keywords.wrappedLighting);
        public static readonly RangeProperty diffuseSmoothness                     = new RangeProperty(Uniforms.diffuseSmoothness, 0, 1);
        public static readonly RangeProperty diffuseThresholdOffset                = new RangeProperty(Uniforms.diffuseThresholdOffset, 0, 1);
        public static readonly RangeProperty specularSmoothness                    = new RangeProperty(Uniforms.specularSmoothness, 0, 1);
        public static readonly RangeProperty specularThresholdOffset               = new RangeProperty(Uniforms.specularThresholdOffset, 0, 1);
        public static readonly RangeProperty rimSmoothness                         = new RangeProperty(Uniforms.rimSmoothness, 0, 1);
        public static readonly RangeProperty rimThresholdOffset                    = new RangeProperty(Uniforms.rimThresholdOffset, 0, 1);
        public static readonly RangeProperty lightTransmissionSmoothness           = new RangeProperty(Uniforms.lightTransmissionSmoothness, 0, 1);
        public static readonly RangeProperty lightTransmissionThresholdOffset      = new RangeProperty(Uniforms.lightTransmissionThresholdOffset, 0, 1);
        public static readonly EnumProperty<Light> light                           = new EnumProperty<Light>(Uniforms.light, Keywords.light);
        public static readonly TextureProperty diffuseRamp                         = new TextureProperty(Uniforms.diffuseRamp);
        public static readonly TextureProperty specularRamp                        = new TextureProperty(Uniforms.specularRamp);
        public static readonly TextureProperty rimRamp                             = new TextureProperty(Uniforms.rimRamp);
        public static readonly TextureProperty lightTransmissionRamp               = new TextureProperty(Uniforms.lightTransmissionRamp);
        public static readonly StepProperty lightBands                             = new StepProperty(Uniforms.lightBands, 2, 12);
        public static readonly RangeProperty lightBandsScale                       = new RangeProperty(Uniforms.lightBandsScale, 0, 1);
        public static readonly RangeProperty lightThreshold                        = new RangeProperty(Uniforms.lightThreshold, 0, 1);
        public static readonly TextureProperty thresholdMap                        = new TextureProperty(Uniforms.thresholdMap, Keywords.thresholdMap);
        public static readonly FloatProperty thresholdMapScale                     = new FloatProperty(Uniforms.thresholdMapScale);
        public static readonly RangeProperty goochRampIntensity                    = new RangeProperty(Uniforms.goochRampIntensity, 0, 1);
        public static readonly TextureProperty goochRamp                           = new TextureProperty(Uniforms.goochRamp, Keywords.goochRamp);
        public static readonly ColorProperty goochBrightColor                      = new ColorProperty(Uniforms.goochBrightColor);
        public static readonly TextureProperty goochBrightMap                      = new TextureProperty(Uniforms.goochBrightMap, Keywords.goochBrightMap);
        public static readonly ColorProperty goochDarkColor                        = new ColorProperty(Uniforms.goochDarkColor);
        public static readonly TextureProperty goochDarkMap                        = new TextureProperty(Uniforms.goochDarkMap, Keywords.goochDarkMap);
        public static readonly EnumProperty<ColorGrading> colorGrading             = new EnumProperty<ColorGrading>(Uniforms.colorGrading, Keywords.colorGrading);
        public static readonly FloatProperty contrast                              = new FloatProperty(Uniforms.contrast);
        public static readonly RangeProperty saturation                            = new RangeProperty(Uniforms.saturation, 0);
        public static readonly RangeProperty brightness                            = new RangeProperty(Uniforms.brightness, 0);
        public static readonly EnumProperty<Iridescence> iridescence               = new EnumProperty<Iridescence>(Uniforms.iridescence, Keywords.iridescence);
        public static readonly TextureProperty iridescenceRamp                     = new TextureProperty(Uniforms.iridescenceRamp);
        public static readonly RangeProperty iridescenceSize                       = new RangeProperty(Uniforms.iridescenceSize, 0, 5);
        public static readonly RangeProperty iridescenceThresholdOffset            = new RangeProperty(Uniforms.iridescenceThresholdOffset, 0, 1);
        public static readonly RangeProperty iridescenceSmoothness                 = new RangeProperty(Uniforms.iridescenceSmoothness, 0, 1);
        public static readonly ColorProperty iridescenceColor                      = new ColorProperty(Uniforms.iridescenceColor);
        public static readonly EnumProperty<Rim> rim                               = new EnumProperty<Rim>(Uniforms.rim, Keywords.rim);
        public static readonly ColorProperty rimColor                              = new ColorProperty(Uniforms.rimColor);
        public static readonly ColorProperty rimBrightColor                        = new ColorProperty(Uniforms.rimBrightColor);
        public static readonly ColorProperty rimDarkColor                          = new ColorProperty(Uniforms.rimDarkColor);
        public static readonly RangeProperty rimSize                               = new RangeProperty(Uniforms.rimSize, 0, 1);
        public static readonly EnumProperty<VertexAnimation> vertexAnimation       = new EnumProperty<VertexAnimation>(Uniforms.vertexAnimation, Keywords.vertexAnimation);
        public static readonly BoolProperty vertexAnimationStutter                 = new BoolProperty(Uniforms.vertexAnimationStutter, Keywords.vertexAnimationStutter);
        public static readonly TextureProperty vertexAnimationMap                  = new TextureProperty(Uniforms.vertexAnimationMap, Keywords.vertexAnimationMap);
        public static readonly RangeProperty vertexAnimationIntensity              = new RangeProperty(Uniforms.vertexAnimationIntensity, 0, 1);
        public static readonly Vector3Property vertexAnimationFrequency            = new Vector3Property(Uniforms.vertexAnimationFrequency);
        public static readonly EnumProperty<Dissolve> dissolve                     = new EnumProperty<Dissolve>(Uniforms.dissolve, Keywords.dissolve);
        public static readonly TextureProperty dissolveMap                         = new TextureProperty(Uniforms.dissolveMap);
        public static readonly FloatProperty dissolveMapScale                      = new FloatProperty(Uniforms.dissolveMapScale);
        public static readonly RangeProperty dissolveAmount                        = new RangeProperty(Uniforms.dissolveAmount, 0, 1);
        public static readonly RangeProperty dissolveBorderSize                    = new RangeProperty(Uniforms.dissolveBorderSize, 0, 1);
        public static readonly TextureProperty dissolveBorderRamp                  = new TextureProperty(Uniforms.dissolveBorderRamp);
        public static readonly ColorProperty dissolveBorderColor                   = new ColorProperty(Uniforms.dissolveBorderColor);
        public static readonly EnumProperty<Artistic> artistic                     = new EnumProperty<Artistic>(Uniforms.artistic, Keywords.artistic);
        public static readonly EnumProperty<ArtisticProjection> artisticProjection = new EnumProperty<ArtisticProjection>(Uniforms.artisticProjection, Keywords.artisticProjection);
        public static readonly RangeProperty artisticFrequency                     = new RangeProperty(Uniforms.artisticFrequency, Keywords.artisticAnimation, 1.0f, 1, 10);
        public static readonly FloatProperty drawnMapScale                         = new FloatProperty(Uniforms.drawnMapScale);
        public static readonly TextureProperty drawnMap                            = new TextureProperty(Uniforms.drawnMap);
        public static readonly FloatProperty hatchingMapScale                      = new FloatProperty(Uniforms.hatchingMapScale);
        public static readonly TextureProperty hatchingBrightMap                   = new TextureProperty(Uniforms.hatchingBrightMap);
        public static readonly TextureProperty hatchingDarkMap                     = new TextureProperty(Uniforms.hatchingDarkMap);
        public static readonly RangeProperty drawnClampMin                         = new RangeProperty(Uniforms.drawnClampMin, 0, 1);
        public static readonly RangeProperty drawnClampMax                         = new RangeProperty(Uniforms.drawnClampMax, 0, 1);
        public static readonly FloatProperty sketchMapScale                        = new FloatProperty(Uniforms.sketchMapScale);
        public static readonly TextureProperty sketchMap                           = new TextureProperty(Uniforms.sketchMap);

        /////////////////
        // Advanced    //
        /////////////////
        public static readonly EnumProperty<Diffuse> diffuse                        = new EnumProperty<Diffuse>(Uniforms.diffuse, Keywords.diffuse);
        public static readonly SpecularProperty specular                            = new SpecularProperty(Uniforms.specular, Keywords.specular);
        public static readonly RangeProperty specularIntensity                      = new RangeProperty(Uniforms.specularIntensity, 0);
        public static readonly RangeProperty anisotropy                             = new RangeProperty(Uniforms.anisotropy, -1, 1);
        public static readonly RangeProperty lightTransmissionIntensity             = new RangeProperty(Uniforms.lightTransmissionIntensity, 0);
        public static readonly EnvironmentReflectionProperty environmentReflections = new EnvironmentReflectionProperty(Uniforms.environmentReflections, Keywords.environmentReflections);
        public static readonly BoolProperty fresnelHighlights                       = new BoolProperty(Uniforms.fresnelHighlights, Keywords.fresnelHighlights);
        public static readonly RangeProperty indirectFade                           = new RangeProperty(Uniforms.IndirectFade, 0.0f, 1.0f);
        public static readonly BoolProperty alembicMotionVectors                    = new BoolProperty(Uniforms.alembicMotionVectors, Keywords.alembicMotionVectors);
        public static readonly RenderPriorityProperty renderPriority                = new RenderPriorityProperty(Uniforms.renderPriority);
        //Stencil
        public static readonly StencilModeProperty stencil                          = new StencilModeProperty(Uniforms.stencil);
        public static readonly StepProperty stencilRef                              = new StepProperty(Uniforms.stencilRef, 0, 255);
        public static readonly StepProperty stencilReadMask                         = new StepProperty(Uniforms.stencilReadMask, 0, 255);
        public static readonly StepProperty stencilWriteMask                        = new StepProperty(Uniforms.stencilWriteMask, 0, 255);
        public static readonly EnumProperty<StencilComparison> stencilComp          = new EnumProperty<StencilComparison>(Uniforms.stencilComp);
        public static readonly EnumProperty<StencilOperation> stencilPass           = new EnumProperty<StencilOperation>(Uniforms.stencilPass);
        public static readonly EnumProperty<StencilOperation> stencilFail           = new EnumProperty<StencilOperation>(Uniforms.stencilFail);
        public static readonly EnumProperty<StencilOperation> stencilZFail          = new EnumProperty<StencilOperation>(Uniforms.stencilZFail);
       
        /////////////////
        // Outline     //
        /////////////////
        public static readonly EnumProperty<Outline> outline         = new EnumProperty<Outline>(Uniforms.outline, Keywords.outline);
        public static readonly EnumProperty<OutlineData> outlineData = new EnumProperty<OutlineData>(Uniforms.outlineData, Keywords.outlineData);
        public static readonly TextureProperty outlineMap            = new TextureProperty(Uniforms.outlineMap, Keywords.outlineMap);
        public static readonly RangeProperty outlineSize             = new RangeProperty(Uniforms.outlineSize, 0);
        public static readonly ColorProperty outlineColor            = new ColorProperty(Uniforms.outlineColor);
        #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
            public static readonly FloatProperty outlineFadeMin      = new FloatProperty(Uniforms.outlineFadeMin);
            public static readonly FloatProperty outlineFadeMax      = new FloatProperty(Uniforms.outlineFadeMax);
        #endif
        public static readonly RangeProperty outlineNoise            = new RangeProperty(Uniforms.outlineNoise, Keywords.outlineNoise, -1, 1);

        /////////////////
        // Refraction  //
        /////////////////
        public static readonly FloatProperty refractionDistortionMapScale = new FloatProperty(Uniforms.refractionDistortionMapScale);
        public static readonly TextureProperty refractionDistortionMap    = new TextureProperty(Uniforms.refractionDistortionMap, Keywords.refractionDistortionMap);
        public static readonly FloatProperty refractionDistortion         = new FloatProperty(Uniforms.refractionDistortion);
        public static readonly RangeProperty refractionDistortionFade     = new RangeProperty(Uniforms.refractionDistortionFade, 0, 1);
        public static readonly RangeProperty indexOfRefraction            = new RangeProperty(Uniforms.indexOfRefraction, Keywords.indexOfRefraction, 0, 0.5f);

        /////////////////
        // Particles   //
        /////////////////
        public static readonly BoolProperty flipbook                = new BoolProperty(Uniforms.flipbook, Keywords.flipbook);
        public static readonly BoolProperty softFade                = new BoolProperty(Uniforms.softFade, Keywords.softFade);
        public static readonly FloatProperty softFadeNearDistance   = new FloatProperty(Uniforms.softFadeNearDistance);
        public static readonly FloatProperty softFadeFarDistance    = new FloatProperty(Uniforms.softFadeFarDistance);
        public static readonly BoolProperty cameraFade              = new BoolProperty(Uniforms.cameraFade, Keywords.cameraFade);
        public static readonly FloatProperty cameraFadeNearDistance = new FloatProperty(Uniforms.cameraFadeNearDistance);
        public static readonly FloatProperty cameraFadeFarDistance  = new FloatProperty(Uniforms.cameraFadeFarDistance);
        public static readonly EnumProperty<ColorBlend> colorBlend  = new EnumProperty<ColorBlend>(Uniforms.colorBlend, Keywords.colorBlend);

        /////////////////
        // System      //
        /////////////////
        /// <summary>
        /// This function should be called after changing the Albedo Map or AlphaCutoff Properties.
        /// It makes sure that baked system shadows work correctly with alpha clipping.
        /// </summary>
        /// <param name="material"></param>
        public static void UpdateSystemProperties(UnityEngine.Material material)
        {
            material.SetTexture(Uniforms.mainTex.id, Properties.albedoMap.GetValue(material));
            material.SetFloat(Uniforms.cutoff.id, Properties.alphaCutoff.GetValue(material));
            material.SetColor(Uniforms.color.id, Properties.albedoColor.GetValue(material));
        }
    }
}
