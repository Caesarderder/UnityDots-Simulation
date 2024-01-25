//////////////////////////////////////////////////////
// MK Toon Keywords         	    	    	   	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

namespace MK.Toon
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Keywords                                                                                //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public static class Keywords
    {
        public static readonly string albedoMap                = "_MK_ALBEDO_MAP";
        public static readonly string alphaClipping            = "_MK_ALPHA_CLIPPING";
        public static readonly string[] surface                = new string[2]{"_MK_SURFACE_TYPE_OPAQUE", "_MK_SURFACE_TYPE_TRANSPARENT"};
        public static readonly string[] lightTransmission      = new string[3] { "_MK_LIGHT_TRANSMISSION_OFF", "_MK_LIGHT_TRANSMISSION_TRANSLUCENT", "_MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING" };
        public static readonly string thicknessMap             = "_MK_THICKNESS_MAP";
        public static readonly string normalMap                = "_MK_NORMAL_MAP";
        public static readonly string heightMap                = "_MK_HEIGHT_MAP";
        public static readonly string parallax                 = "_MK_PARALLAX";
        public static readonly string occlusionMap             = "_MK_OCCLUSION_MAP";
        public static readonly string[] blend                  = new string[5]{"_MK_BLEND_ALPHA", "_MK_BLEND_PREMULTIPLY", "_MK_BLEND_ADDITIVE", "_MK_BLEND_MULTIPLY", "_MK_BLEND_CUSTOM"};
        public static readonly string[] light                  = new string[4]{"_MK_LIGHT_DEFAULT", "_MK_LIGHT_CEL", "_MK_LIGHT_BANDED", "_MK_LIGHT_RAMP"};
        public static readonly string[] artistic               = new string[4]{"_MK_ARTISTIC_OFF", "_MK_ARTISTIC_DRAWN", "_MK_ARTISTIC_HATCHING", "_MK_ARTISTIC_SKETCH"};
        public static readonly string[] artisticProjection     = new string[2]{"_MK_ARTISTIC_PROJECTION_TANGENT_SPACE", "_MK_ARTISTIC_PROJECTION_SCREEN_SPACE" };
        public static readonly string artisticAnimation        = "_MK_ARTISTIC_ANIMATION_STUTTER";
        public static readonly string[] workflow               = new string[2]{"_MK_WORKFLOW_SPECULAR", "_MK_WORKFLOW_ROUGHNESS"};
        public static readonly string emission                 = "_MK_EMISSION";
        public static readonly string emissionMap              = "_MK_EMISSION_MAP";
        public static readonly string detailMap                = "_MK_DETAIL_MAP";
        public static readonly string[] detailBlend            = new string[3]{"_MK_DETAIL_BLEND_OFF", "_MK_DETAIL_BLEND_MIX", "_MK_DETAIL_BLEND_ADD"};
        public static readonly string detailNormalMap          = "_MK_DETAIL_NORMAL_MAP";
        public static readonly string[] rim                    = new string[3]{"_MK_RIM_OFF", "_MK_RIM_DEFAULT", "_MK_RIM_SPLIT"};
        public static readonly string[] iridescence            = new string[2]{"_MK_IRIDESCENCE_OFF", "_MK_IRIDESCENCE_DEFAULT"};
        public static readonly string[] colorGrading           = new string[3]{"_MK_COLOR_GRADING_OFF", "_MK_COLOR_GRADING_ALBEDO", "_MK_COLOR_GRADING_FINAL_OUTPUT"};
        public static readonly string[] dissolve               = new string[4]{"_MK_DISSOLVE_OFF", "_MK_DISSOLVE_DEFAULT", "_MK_DISSOLVE_BORDER_COLOR", "_MK_DISSOLVE_BORDER_RAMP"};
        public static readonly string goochRamp                = "_MK_GOOCH_RAMP";
        public static readonly string goochBrightMap           = "_MK_GOOCH_BRIGHT_MAP";
        public static readonly string goochDarkMap             = "_MK_GOOCH_DARK_MAP";
        public static readonly string[] diffuse                = new string[3]{"_MK_DIFFUSE_LAMBERT", "_MK_DIFFUSE_OREN_NAYAR", "_MK_DIFFUSE_MINNAERT"};
        public static readonly string[] specular               = new string[3]{"_MK_SPECULAR_OFF", "_MK_SPECULAR_ISOTROPIC", "_MK_SPECULAR_ANISOTROPIC"};
        public static readonly string[] environmentReflections = new string[3]{ "_MK_ENVIRONMENT_REFLECTIONS_OFF", "_MK_ENVIRONMENT_REFLECTIONS_AMBIENT", "_MK_ENVIRONMENT_REFLECTIONS_ADVANCED"};
        public static readonly string fresnelHighlights        = "_MK_FRESNEL_HIGHLIGHTS";
        public static readonly string alembicMotionVectors   = "_ADD_PRECOMPUTED_VELOCITY";
        public static readonly string[] outline                = new string[3]{"_MK_OUTLINE_HULL_OBJECT", "_MK_OUTLINE_HULL_ORIGIN", "_MK_OUTLINE_HULL_CLIP"};
        public static readonly string outlineData              = "_MK_OUTLINE_DATA_UV7";
        public static readonly string outlineMap               = "_MK_OUTLINE_MAP";
        public static readonly string refractionDistortionMap  = "_MK_REFRACTION_DISTORTION_MAP";
        public static readonly string indexOfRefraction        = "_MK_INDEX_OF_REFRACTION";
        public static readonly string outlineNoise             = "_MK_OUTLINE_NOISE";
        public static readonly string receiveShadows           = "_MK_RECEIVE_SHADOWS";
        public static readonly string wrappedLighting          = "_MK_WRAPPED_DIFFUSE";
        public static readonly string[] colorBlend             = new string[6]{"_MK_COLOR_BLEND_MULTIPLY", "_MK_COLOR_BLEND_ADDITIVE", "_MK_COLOR_BLEND_SUBTRACTIVE", "_MK_COLOR_BLEND_OVERLAY", "_MK_COLOR_BLEND_COLOR", "_MK_COLOR_BLEND_DIFFERENCE"};
        public static readonly string flipbook                 = "_MK_FLIPBOOK";
        public static readonly string softFade                 = "_MK_SOFT_FADE";
        public static readonly string cameraFade               = "_MK_CAMERA_FADE";
        public static readonly string thresholdMap             = "_MK_THRESHOLD_MAP";
        public static readonly string pbsMap0                  = "_MK_PBS_MAP_0";
        public static readonly string pbsMap1                  = "_MK_PBS_MAP_1";
        public static readonly string[] vertexAnimation        = new string[4]{"_MK_VERTEX_ANIMATION_OFF", "_MK_VERTEX_ANIMATION_SINE", "_MK_VERTEX_ANIMATION_PULSE", "_MK_VERTEX_ANIMATION_NOISE"};
        public static readonly string vertexAnimationMap       = "_MK_VERTEX_ANIMATION_MAP";
         public static readonly string vertexAnimationStutter  = "_MK_VERTEX_ANIMATION_STUTTER";
    }
}
