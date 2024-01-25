//////////////////////////////////////////////////////
// MK Toon Editor UI                			    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEngine;

namespace MK.Toon.Editor
{
    internal class UI
    {
        internal static readonly GUIContent empty = new GUIContent("", "");

        /////////////////
        // Options     //
        /////////////////
        internal static readonly GUIContent workflow = new GUIContent
        (
            "Workflow", 
            "Physically based workflow.\n\n" +
            "Metallic: Metallic + Smoothness\n\n" +
            "Specular: Specular + Smoothness\n\n" +
            "Roughness: Metallic + Roughness"
        );
        internal static readonly GUIContent renderFace = new GUIContent
        (
            "Render Face", 
            "Culling / visibility of faces.\n\n" +
            "Front: Back faces are culled.\n" +
            "Back: Front faces are culled.\n" +
            "Double Sided: Front and back faces are rendered."
        );
        internal static readonly GUIContent surface = new GUIContent
        (
            "Surface", 
            "Opaque: Surface is rendered opaque.\n\n" +
            "Transparent: Surface is rendered transparent."
        );
        internal static readonly GUIContent zWrite = new GUIContent
        (
            "Z-Write", 
            "Controls if the material writes into the depth buffer."
        );
        internal static readonly GUIContent blend = new GUIContent
        (
            "Blending", 
            "Controls how the surface color blends into the background.\n \n" +
            "Alpha/Defaul: Raw albedo alpha is used.\n\n" +
            "Premultiply: Similar to Alpha, but preserves reflections and highlights.\n\n" +
            "Additive: Blending is done additive as an extra layer.\n\n" +
            "Multiply: Background is multiplied into the surface color.\n\n" +
            "Custom: Overwrites the blending and lets you control it via two blend factors, a ZWrite and a ZTest"
        );
        internal static readonly GUIContent zTest = new GUIContent
        (
            "Z Test", 
            "Controls how depth testing should be performed."
        );
        internal static readonly GUIContent overwriteBlend = new GUIContent
        (
            "Overwrite Blending", 
            "Overwrites the blending and lets you control it via two blend factors, a ZWrite and a ZTest. If enabled the surface property does not touch the blending if its changed. The base blending compile directives are still used."
        );
        internal static readonly GUIContent blendSrc = new GUIContent
        (
            "Src Factor RGB", 
            "Generated color is multiplied by the Src Factor."
        );
        internal static readonly GUIContent blendDst = new GUIContent
        (
            "Dst Factor RGB", 
            "Color that is already on the screen is multiplied by the Dst Factor."
        );
        internal static readonly GUIContent blendSrcAlpha = new GUIContent
        (
            "Src Factor Alpha", 
            "Generated alpha is multiplied by the Src Factor."
        );
        internal static readonly GUIContent blendDstAlpha = new GUIContent
        (
            "Dst Factor Alpha", 
            "Alpha that is already on the screen is multiplied by the Dst Factor."
        );
        internal static readonly GUIContent alphaClipping = new GUIContent
        (
            "Alpha Clipping", "Material acts like a cutout shader."
        );

        /////////////////
        // Input       //
        /////////////////
        internal static readonly GUIContent albedoColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent alphaCutoff = new GUIContent
        (
            "Alpha Cutoff", 
            "Pixels will be discarded if the albedo alpha minus Cutoff passes a value of < 0."
        );
        internal static readonly GUIContent albedoMap = new GUIContent
        (
            "Albedo", 
            "Albedo (RGBA) represents the base of your Material. Color (RGB) and Alpha (A) are used."
        );
        internal static readonly GUIContent specularColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent smoothness = new GUIContent
        (
            "Smoothness", 
            "Physically Based Shading Smoothness. Controls how the surface spreads the highlights.\n\n" +
            "0/black: Rough highlight, rough surface\n" +
            "1/white: Sharp highlight, smooth surface"
        );
        internal static readonly GUIContent specularMap = new GUIContent
        (
            "Specular", 
            "Physically Based Shading Specular.\n\n" +
            "0/black: Fully dielectric\n" +
            "1/white: Fully reflective\n" +
            "(A) is used to control the surface smoothness."
        );
        internal static readonly GUIContent roughnessMap = new GUIContent
        (
            "Roughness", 
            "Physically Based Shading Roughness. Controls how the surface spreads the highlights. \n\n" +
            "0/black: Sharp highlight, smooth surface\n" +
            "1/white: Rough highlight, rough surface \n" +
            "(R) is used to control the surface roughness."
        );
        internal static readonly GUIContent metallicMap = new GUIContent
        (
            "Metallic", 
            "Physically Based Shading Metallic value (R).\n\n" +
            "0/black: Fully dielectric\n" +
            "1/white: Fully metallic\n" +
            "(A) is used to control the surface smoothness on Metallic Workflow."
        );
        internal static readonly GUIContent normalMapIntensity = new GUIContent
        (
            "Normal Map Scale", 
            ""
        );
        internal static readonly GUIContent normalMap = new GUIContent
        (
            "Normals", 
            "Tangent spaced normal map + intensity."
        );
        internal static readonly GUIContent heightMapScale = new GUIContent
        (
            "Height Map Scale",
            ""
        );
        internal static readonly GUIContent heightMap = new GUIContent
        (
            "Height", 
            "Height map used for parallax mapping and scaling.\n\n" +
            "(R) controls how much height/distortion is applied.\n" +
            "Slider controls the intensity of the parallax mapping."
        );
        internal static readonly GUIContent lightTransmission = new GUIContent
        (
            "Light Transmission", 
            ""
        );
        internal static readonly GUIContent lightTransmissionDistortion = new GUIContent
        (
            "Distortion", 
            ""
        );
        internal static readonly GUIContent lightTransmissionColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent thicknessMap = new GUIContent
        (
            "Thickness", 
            "Thickness Map, distortion and color.\n\n" +
            "(R) is used to control the thickness.\n" +
            "0/black: thick, zero light is transmitted\n" +
            "1/white: thin, light is fully transmitted\n\n" +
            "Slider controls how strong the light spreads based on the mesh normals.\n\n" +
            "Color picker defines the transmitted light tint."
        );
        internal static readonly GUIContent occlusionMapScale = new GUIContent
        (
            "Occlusion Map Scale", 
            ""
        );
        internal static readonly GUIContent occlusionMap = new GUIContent
        (
            "Occlusion", 
            "Direct/Indirect lighting occlusion map and intensity.\n\n" +
            "(R) controls how much direct light is recieved.\n" +
            "(G) controls how much indirect light is recieved.\n\n" +
            "0/black: lighting fully occluded\n" +
            "1/white: default lighting\n\n" +
            "Slider scales the intensity of the occlusion map."
        );
        internal static readonly GUIContent emissionColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent emissionMap = new GUIContent
        (
            "Emission", 
            "Emission Map (RGB) & Color (RGB). Emission makes the material emit light to the environment."
        );

        /////////////////
        // Detail      //
        /////////////////
        internal static readonly GUIContent detailBlend = new GUIContent
        (
            "Blend", 
            ""
        );
        internal static readonly GUIContent detailColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent detailMix = new GUIContent
        (
            "Mix", 
            ""
        );
        internal static readonly GUIContent detailMap = new GUIContent
        (
            "Detail", 
            "Details (RGBA) & Color will be combined with the Albedo(RGB).\n\n" +
            "Blending:\n" +
            "Mix: albedo and detail is interpolated\n" +
            "Add: Albedo and detail are additive\n" +
            "Multiply: Detail is multiplied into albedo.\n\n" +
            "Detail (A) and slider can be used for visibility.\n" +
            "0: No detail visible\n" +
            "1: Detail fully visible"
        );
        internal static readonly GUIContent detailNormalMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent detailNormalMap = new GUIContent
        (
            "Normals", 
            "Tangent spaced normal map + intensity.\n\n" +
            "Detail normals are automatically combined with the main normals."
        );

        /////////////////
        // Stylize     //
        /////////////////
        internal static readonly GUIContent wrappedLighting = new GUIContent
        (
            "Wrapped Lighting", 
            "If enabled the diffuse lighting is more soft (areas get lit more easily).\n\n" +
            "You may need to adjust your light threshold after enable/disable this setting."
        );
        internal static readonly GUIContent receiveShadows = new GUIContent
        (
            "Receive Shadows", 
            "When enabled, other GameObjects can cast shadows onto this GameObject."
        );
        internal static readonly GUIContent diffuseSmoothness = new GUIContent
        (
            "Diffuse", 
            "Smoothness of the diffuse lighting.\n\n" +
            "0: Hard edge for the light/shadow\n" +
            "1: Smooth interpolation for light/shadow"
        );
        internal static readonly GUIContent diffuseThresholdOffset = new GUIContent
        (
            "Diffuse", 
            "Threshold in addition the light threshold."
        );
        internal static readonly GUIContent specularSmoothness = new GUIContent
        (
            "Specular", 
            "Smoothness of the specular lighting.\n\n" +
            "0: Hard edge for the light/shadow\n" +
            "1: Smooth interpolation for light/shadow"
        );
        internal static readonly GUIContent specularThresholdOffset = new GUIContent
        (
            "Specular", 
            "Threshold in addition the light threshold."
        );
        internal static readonly GUIContent rimSmoothness = new GUIContent
        (
            "Rim", "Smoothness of the rim lighting.\n\n" +
            "0: Hard edge for the light/shadow\n" +
            "1: Smooth interpolation for light/shadow"
        );
        internal static readonly GUIContent rimThresholdOffset = new GUIContent
        (
            "Rim", 
            "Threshold in addition the light threshold."
        );
        internal static readonly GUIContent lightTransmissionSmoothness = new GUIContent
        (
            "Light Transmission", "Smoothness of the sss lighting.\n\n" +
            "0: Hard edge for the light/shadow\n" +
            "1: Smooth interpolation for light/shadow"
        );
        internal static readonly GUIContent lightTransmissionThresholdOffset = new GUIContent
        (
            "Light Transmission", 
            "Threshold in addition the light threshold."
        );
        internal static readonly GUIContent light = new GUIContent
        (
            "Style", 
            "The style defines the appearance of light.\n\n" +
            "Builtin: Lighting calculation has no custom style\n\n" +
            "Simple: Typical cel shading style. One light cut is applied to lighting.\n\n" +
            "Multi: A variation to the simple one. Multiple light cuts can be applied.\n\n" +
            "Ramp: Lighting is based on a 2D ramps (R)."
        );
        internal static readonly GUIContent diffuseRamp = new GUIContent
        (
            "Diffuse", 
            "2D Ramp (R) / intensity and falloff for lighting.\n" +
            "Intensity: X axis, left/dark -> right/bright.\n" +
            "Falloff: Y axis, bottom/far -> top/near (non directional lights only)."
        );
        internal static readonly GUIContent specularRamp = new GUIContent
        (
            "Specular", 
            "2D Ramp (R) / intensity and falloff for lighting.\n" +
            "Intensity: X axis, left/dark -> right/bright.\n" +
            "Falloff: Y axis, bottom/far -> top/near (non directional lights only)."
        );
        internal static readonly GUIContent rimRamp = new GUIContent
        (
            "Rim", 
            "2D Ramp (R) / intensity and falloff for lighting.\n" +
            "Intensity: X axis, left/dark -> right/bright.\n" +
            "Falloff: Y axis, bottom/far -> top/near (non directional lights only)."
        );
        internal static readonly GUIContent lightTransmissionRamp = new GUIContent
        (
            "Light Transmission", 
            "2D Ramp (R) / intensity and falloff for lighting.\n" +
            "Intensity: X axis, left/dark -> right/bright.\n" +
            "Falloff: Y axis, bottom/far -> top/near (non directional lights only)."
        );
        internal static readonly GUIContent lightBands = new GUIContent
        (
            "Bands", 
            "Amount of light cuts."
        );
        internal static readonly GUIContent lightBandsScale = new GUIContent
        (
            "Bands Scale", 
            "Interpolation of the light bands. A higher value will “push” the band more into shadowed areas."
        );
        internal static readonly GUIContent lightThreshold = new GUIContent
        (
            "Light Threshold", 
            "Threshold defined for light/shadow. \n\n" +
            "0: Material is more lit (also depends on wrapped lighting => advanced tab). \n" +
            "1: Material is fully shadowed, no lights are visible."
        );
        internal static readonly GUIContent thresholdMap = new GUIContent
        (
            "Threshold", 
            "Threshold texture and scaling. Value is added additive to the light threshold."
        );
        internal static readonly GUIContent thresholdMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent goochRampIntensity = new GUIContent
        (
            "Intensity", 
            ""
        );
        internal static readonly GUIContent goochRamp = new GUIContent
        (
            "Ramp", 
            "2D Ramp (RGB) / interpolated tint based on lighting. Does not affect the light intensity or falloff.\n\n" +
            "Intensity: X axis, left/dark -> right/bright.\n" +
            "Falloff: Y axis, bottom/far -> top/near (non directional lights only). \n\n" +
            "Slider is used for mixing tint into lighting."
        );
        internal static readonly GUIContent goochBrightColor = new GUIContent
        (
            "Bright", 
            ""
        );
        internal static readonly GUIContent goochBrightMap = new GUIContent
        (
            "Bright", 
            "Bright gooch colors are used for lit areas of the material. \n\n" +
            "Bright areas will interpolate between the albedo (RGB) and the bright gooch (RGB)."
        );
        internal static readonly GUIContent goochDarkColor = new GUIContent
        (
            "Dark",
            ""
        );
        internal static readonly GUIContent goochDarkMap = new GUIContent
        (
            "Dark", 
            "Dark gooch colors are used for shadowed areas of the material. \n\n" +
            "Dark areas will interpolate between the albedo (RGB) and the dark gooch (RGB)."
        );
        internal static readonly GUIContent colorGrading = new GUIContent
        (
            "Color Grading", 
            "Controls how the contrast, saturation and brightness is applied.\n\n" +
            "Off: Effect disabled.\n\n" +
            "Albedo: Effect is applied to input albedo only.\n\n" +
            "Final Output: Effect is applied to the final output."
        );
        internal static readonly GUIContent contrast = new GUIContent
        (
            "Contrast", 
            ""
        );
        internal static readonly GUIContent saturation = new GUIContent
        (
            "Saturation", 
            ""
        );
        internal static readonly GUIContent brightness = new GUIContent
        (
            "Brightness", 
            ""
        );
        internal static readonly GUIContent iridescence = new GUIContent
        (
            "Iridescence",
            "Adds a Iridescence effect on top of the surface. Based on the viewing angle ares will change color gradually."
        );
        internal static readonly GUIContent iridescenceRamp = new GUIContent
        (
            "Ramp", 
            "Ramp (RGB) defines the iridescence color based on the viewing angle.\n\n" +
            "Tint: X axis, left/dark -> right/bright.\n" +
            "Color (RGB) tints the effect additionally. (A) is used for visibility."
        );
        internal static readonly GUIContent iridescenceSize = new GUIContent
        (
            "Size", 
            "Size of the iridescence effect."
        );
        internal static readonly GUIContent iridescenceThresholdOffset = new GUIContent
        (
            "Iridescence", 
            "Threshold in addition the light threshold."
        );
        internal static readonly GUIContent iridescenceSmoothness = new GUIContent
        (
            "Iridescence", 
            "Smoothness of the specular lighting."
        );
        internal static readonly GUIContent iridescenceColor = new GUIContent
        (
            "Color", 
            ""
        );
        internal static readonly GUIContent rim = new GUIContent
        (
            "Rim", 
            "Rim lighting can be applied as different variants. Lighting is based on the fresnel effect. Outer visible areas of the Material will be highlighted.\n\n" +
            "Off: Rim lighting is disabled.\n\n" +
            "Default: Rim lighting is applied to the whole material.\n\n" +
            "Split: Rim lighting is split between bright and dark areas."
        );
        internal static readonly GUIContent rimColor = new GUIContent
        (
            "Color", 
            "Color (RGB) defines the resulting color. Alpha is used for visibility."
        );
        internal static readonly GUIContent rimBrightColor = new GUIContent
        (
            "Bright", 
            "Color (RGB) defines the resulting color on lit areas. Alpha is used for visibility."
        );
        internal static readonly GUIContent rimDarkColor = new GUIContent
        (
            "Dark",
            "Color (RGB) defines the resulting color on shadowed areas. Alpha is used for visibility."
        );
        internal static readonly GUIContent rimSize = new GUIContent
        (
            "Size", 
            "Size of the rim effect."
        );
        internal static readonly GUIContent vertexAnimation = new GUIContent
        (
            "Vertex Animation", 
            ".\n\n" +
            "Off: Vertex Animation is disabled.\n" +
            "Sine: Sine based animation, applied in object space.\n" +
            "Pulse: Pulse styled animation.\n" +
            "Noise: Animated noise effect."
        );
        internal static readonly GUIContent vertexAnimationStutter = new GUIContent
        (
            "Stutter", 
            "Enable stuttering for the vertex animation."
        );
        internal static readonly GUIContent vertexAnimationMap = new GUIContent
        (
            "Intensity", 
            "Controls the intensity of the Vertex Animation. Map (R) is used for additional adjustments."
        );
        internal static readonly GUIContent vertexAnimationIntensity = new GUIContent
        (
            "Intensity", 
            ""
        );
        internal static readonly GUIContent vertexAnimationFrequency = new GUIContent
        (
            "Frequency", 
            "Frequency of the animation for X, Y, Z axis."
        );
        internal static readonly GUIContent dissolve = new GUIContent
        (
            "Dissolve", 
            "A dissolving effect can be applied to the material based on a dissolve value and a Ramp (RGB)\n\n" +
            "Off: Dissolve is disabled\n\n" +
            "Simple: Dissolving is done via a map and a dissolve value.\n\n" +
            "Ramp: An additional edge effect for the dissolve is applied."
        );
        internal static readonly GUIContent dissolveMap = new GUIContent
        (
            "Dissolve", 
            "Dissolve map (R) is used for the layout of the dissolve."
        );
        internal static readonly GUIContent dissolveMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent dissolveAmount = new GUIContent
        (
            "Amount", 
            "0: Object fully visible.\n" +
            "1: Object fully dissolved."
        );
        internal static readonly GUIContent dissolveBorderSize = new GUIContent
        (
            "Border", 
            "The border size is used to create a edge effect around the dissolve itself"
        );
        internal static readonly GUIContent dissolveBorderRamp = new GUIContent
        (
            "Ramp", 
            "The ramp, border size and color is used to create a edge effect around the dissolve itself.\n\n" +
            "Colors are used from left to right."
        );
        internal static readonly GUIContent dissolveBorderColor = new GUIContent
        (
            "Color", 
            "The color is used to tint the border."
        );
        internal static readonly GUIContent artistic = new GUIContent
        (
            "Artistic", 
            "Artistic style gives your material a customization for the lighting based on input texture/s.\n\n" +
            "Off: Artistic style is disabled.\n\n" +
            "Drawn: Lighting is customized via a lookup texture (R) and two thresholds for lit and shadowed areas.\n\n" +
            "Hatching: Lighting is customized via two lookup textures (RGB). Six Map channels are used to control how the light interacts from dark to bright in the following order:\n" +
            "Hatching Map Dark: (R) => (G) => (B)\n" +
            "=> Hatching Map Bright: (R) => (G) => (B)\n\n" +
            "Sketch: Lighting is customized via a lookup texture (R) and interpolated into the lighting."
        );
        internal static readonly GUIContent artisticProjection = new GUIContent
        (
            "Projection", 
            "The artistic customization can be projected via Tangent or Screen Space.\n\n" +
            "Tangent Space: Original UV coords are used\n\n" +
            "Screen Space: Projected onto the surface based on the view angle."
        );
        internal static readonly GUIContent artisticStutterFreqency = new GUIContent
        (
            "Frequency", 
            "Animates the artistic effect to give it a constantly redrawn look. A value of 1 disables the animation."
        );
        internal static readonly GUIContent drawnMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent drawnMap = new GUIContent
        (
            "Drawn", 
            "Lookup texture (R) to customize the lighting."
        );
        internal static readonly GUIContent hatchingMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent hatchingBrightMap = new GUIContent
        (
            "Bright", 
            "Lookup texture for brighter areas:\n" +
            "(R) => (G) => (B)"
        );
        internal static readonly GUIContent hatchingDarkMap = new GUIContent
        (
            "Dark", 
            "Lookup texture for darker areas:\n" +
            "(R) => (G) => (B)"
        );
        internal static readonly GUIContent drawnClampMin = new GUIContent
        (
            "Artistic Clamp Min", 
            "Threshold to control the shadowed areas."
        );
        internal static readonly GUIContent drawnClampMax = new GUIContent
        (
            "Artistic Clamp Max", 
            "Threshold to control the lit areas."
        );
        internal static readonly GUIContent sketchMapScale = new GUIContent
        (
            "Scale", 
            ""
        );
        internal static readonly GUIContent sketchMap = new GUIContent
        (
            "Sketch", 
            "Lookup texture (R) to customize the lighting."
        );

        /////////////////
        // Advanced    //
        /////////////////
        internal static readonly GUIContent diffuse = new GUIContent
        (
            "Diffuse", 
            "Diffuse lighting can be stylized the following way:\n\n" +
            "Lambert: The most simple lighting based lambertian reflectance.\n\n" +
            "Oren Nayar: A more precise lighting, which takes the roughness of the surface and the view angle into account.\n\n" +
            "Minnaert: Lighting adds some darkening limbs based on the view angle. Good for things like fabric.\n\n" +
            "Even without the matter of intended usage every diffuse style can give your model an unique style."
        );
        internal static readonly GUIContent specular = new GUIContent
        (
            "Specular", 
            "Controls how the specular affects the lighting. \n\n" +
            "Off: Specular is disabled.\n\n" +
            "Isotropic: Round shaped specular highlight.\n\n" +
            "Anisotropic: Anisotrophic stretched specular highlights. Good for Hair / brushed metal."
        );
        internal static readonly GUIContent specularIntensity = new GUIContent
        (
            "Intensity", "Intensity/brightness of the specular highlight.\n\n" +
            "A physically correct value would be 1 (Built-in lighting style). However on some lighting styles you need to exposure your specular highlights."
        );
        internal static readonly GUIContent anisotropy = new GUIContent
        (
            "Anisotropy", 
            "Anisotropic specular strech based on the tangents at a Range between -1 and 1."
        );
        internal static readonly GUIContent lightTransmissionIntensity = new GUIContent
        (
            "Intensity", 
            "Intensity/brightness of the transmitted light.\n\n" +
            "A physically correct value would be 1 (Built-in lighting style). However on some lighting styles you need to exposure your highlights."
        );
        internal static readonly GUIContent environmentReflections = new GUIContent
        (
            "Environment Reflection", 
            "Indirect Lighting from reflections / Global Illumination / Sky.\n\n" +
            "Off: Indirect Lighting is disabled.\n\n" +
            "Ambient: Indirect lighting only uses ambient color.\n\n" +
            "Advanced: Indirect lighting uses reflections / Sky / Ambient / GI."
        );
        internal static readonly GUIContent fresnelHighlights = new GUIContent
        (
            "Fresnel Highlights", 
            "Physically based fresnel highlights"
        );
        internal static readonly GUIContent indirectFade = new GUIContent
        (
            "Indirect Fade", 
            "Fades the indirect lighting between the lit and shadowed areas based on a value between 0 and 1.\n" +
            "0: No indirect on shadowed areas\n" +
            "1: Indirect fully affects shadowed areas\n\n" +
            "A physically correct value would be 1 (Indirect lighting shown on dark areas)."
        );
        internal static readonly GUIContent stencil = new GUIContent
        (
            "Stencil", 
            "Use a custom stencil test based on the unity setup.\n\n" +
            "Builtin: Default Stencil will be used.\n\n" +
            "Custom: Define your own stencil test."
        );
        internal static readonly GUIContent renderPriority = new GUIContent
        (
            "Priority", 
            "Determines the chronological rendering order for a Material. High values are rendered first."
        );
        internal static readonly GUIContent alembicMotionVectors = new GUIContent
        (
            "Alembic Motion Vectors", 
            "When enabled, the material will use motion vectors from the Alembic animation cache. Should not be used on regular meshes or Alembic caches without precomputed motion vectors."
        );
        internal static readonly GUIContent stencilRef = new GUIContent
        (
            "Ref", 
            ""
        );
        internal static readonly GUIContent stencilReadMask = new GUIContent
        (
            "Read Mask", 
            ""
        );
        internal static readonly GUIContent stencilWriteMask = new GUIContent
        (
            "Write Mask", 
            ""
        );
        internal static readonly GUIContent stencilComp = new GUIContent
        (
            "Comp",
            ""
        );
        internal static readonly GUIContent stencilPass = new GUIContent
        (
            "Pass", 
            ""
        );
        internal static readonly GUIContent stencilFail = new GUIContent
        (
            "Fail", 
            ""
        );
        internal static readonly GUIContent stencilZFail = new GUIContent
        (
            "Z Fail", 
            ""
        );
       
        /////////////////
        // Outline     //
        /////////////////
        internal static readonly GUIContent outline = new GUIContent
        (
            "Outline", 
            "Controls how the outline is created.\n\n" +
            "Hull Object: Outline is created as an inverted hull. It’s the most common per object outline method.\n\n" +
            "Hull Origin: Outline is created based on the object space vertex position as an inverted hull. Your geometry needs to be centered.\n\n" +
            "Hull Clip: Similar to the Hull Object method, but it is tweaked towards pixel perfection and avoid’s foreshortening"
        );
        internal static readonly GUIContent outlineData = new GUIContent
        (
            "Data", 
            "Normal: Original normal of the mesh is used.\n\n" +
            "Baked: Outline is created using a different data set of normals. You can use the MK Mesh Utility to create new meshes with a modified UV7 channel or just average the mesh normals in your 3D program and save them to the UV7 channel. Baked normals can only work for static meshes."
        );
        internal static readonly GUIContent outlineMap = new GUIContent
        (
            "Width", 
            "Size of the outline. Map (R) is used for an advanced size setup."
        );
        internal static readonly GUIContent outlineColor = new GUIContent
        (
            "Color", 
            "Color of the outline."
        );
        internal static readonly GUIContent outlineNoise = new GUIContent
        (
            "Noise", 
            "Noise effect applied to the outline."
        );
        #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
        internal static readonly GUIContent outlineFadeMin = new GUIContent
        (
            "Fade Min", 
            "Minimum required distance to show the outline."
        );
        internal static readonly GUIContent outlineFadeMax = new GUIContent
        (
            "Fade Max", 
            "Maximum required distance to show the outline at full size."
        );
        #endif

        /////////////////
        // Refraction  //
        /////////////////
        internal static readonly GUIContent refractionDistortionMapScale = new GUIContent
        (
            "Scale",
            ""
        );
        internal static readonly GUIContent refractionDistortionMap = new GUIContent
        (
            "DuDv", 
            "Map to control the distortion effect. A normal or dudv map should be applied."
        );
        internal static readonly GUIContent refractionDistortion = new GUIContent
        (
            "Distortion", 
            "Controls how much distortion is applied to the refraction."
        );
        internal static readonly GUIContent indexOfRefraction = new GUIContent
        (
            "IOR", 
            "Index of refraction, controls how much the refraction is bending over the surface."
        );
        internal static readonly GUIContent refractionDistortionFade = new GUIContent
        (
            "Fade", 
            "Controls how visible the refraction is.\n\n" +
            "0: No visible refraction.\n" +
            "1: refraction fully visible."
        );

        /////////////////
        // Particles   //
        /////////////////
        internal static readonly GUIContent flipbook = new GUIContent
        (
            "Flipbook", 
            "Blends the frames in a flip-book together in a smooth animation."
        );
        internal static readonly GUIContent softFade = new GUIContent
        (
            "Soft Fade", 
            "Surface fade out if intersect with other geometry in the depthbuffer."
        );
        internal static readonly GUIContent softFadeNearDistance = new GUIContent
        (
            "Near",
            ""
        );
        internal static readonly GUIContent softFadeFarDistance = new GUIContent
        (
            "Far", 
            ""
        );
        internal static readonly GUIContent cameraFade = new GUIContent
        (
            "Camera Fade", 
            "Surface fade out when get close to the camera."
        );
        internal static readonly GUIContent cameraFadeNearDistance = new GUIContent
        (
            "Near", 
            ""
        );
        internal static readonly GUIContent cameraFadeFarDistance  = new GUIContent
        (
            "Far", 
            ""
        );
        internal static readonly GUIContent colorBlend = new GUIContent
        (
            "Color Blend", 
            "Controls how the input particle color gets applied to the particle surfaces."
        );

        /////////////////
        // Editor Only //
        /////////////////
        internal static readonly GUIContent initialized = new GUIContent
        (
            "Initialization", 
            "Internal only, DONT TOUCH ME"
        );
        internal static readonly GUIContent optionsTab = new GUIContent
        (
            "Options", 
            ""
        );
        internal static readonly GUIContent inputTab = new GUIContent
        (
            "Input", 
            ""
        );
        internal static readonly GUIContent stylizeTab = new GUIContent
        (
            "Stylize", 
            ""
        );
        internal static readonly GUIContent advancedTab = new GUIContent
        (
            "Advanced", 
            ""
        );
        internal static readonly GUIContent particlesTab = new GUIContent
        (
            "Particles", 
            ""
        );
        internal static readonly GUIContent outlineTab = new GUIContent
        (
            "Outline", 
            ""
        );
        internal static readonly GUIContent refractionTab = new GUIContent
        (
            "Refraction", 
            ""
        );
    }
}

#endif