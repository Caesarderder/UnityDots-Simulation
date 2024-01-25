//////////////////////////////////////////////////////
// MK Toon Config					       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_DEFINES
	#define MK_TOON_DEFINES

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Custom User Config
	/////////////////////////////////////////////////////////////////////////////////////////////
	#include "GlobalShaderFeatures.hlsl"
	//Force (baked & mixed) lightmaps
	/*
	#ifndef MK_FORCE_LIGHTMAPS
		#define MK_FORCE_LIGHTMAPS
	#endif
	*/

	//Enable a secondary UV set for occlusion (Z and W channels of the main UV are used)
	/*
	#ifndef MK_OCCLUSION_UV_SECOND
		#define MK_OCCLUSION_UV_SECOND
	#endif
	*/
	
	/*
	//Enable alpha blending for lighting
	#ifndef MK_LIGHTING_ALPHA
		#define MK_LIGHTING_ALPHA
	#endif
	*/

	//Enable Legacy Banded Lighting - Banded Lighting was reworked with 3.0.13
	/*
	#ifndef MK_LEGACY_BANDED_LIGHTING
		#define MK_LEGACY_BANDED_LIGHTING
	#endif
	*/
	
	#if defined(MK_OUTLINE_FADING_LINEAR) || defined(MK_OUTLINE_FADING_EXPONENTIAL) || defined(MK_OUTLINE_FADING_INVERSE_EXPONENTIAL)
		#ifndef MK_OUTLINE_FADING
			#define MK_OUTLINE_FADING
		#endif
	#endif

	// ------------------------------------------------------------------------------------------

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
	#else
		#include "UnityCG.cginc"
	#endif
	
	// ------------------------------------------------------------------------------------------
	// Note: Every define should have a "MK" prefix to avoid compile issues when using external funtions
	// ------------------------------------------------------------------------------------------

	// because of maximum of interpolators / graphics API operations some features are dependent of Shader Models
	// Aniso Spec, Normal Mapping, Heightmapping, Vertex Animation Map are foreced to required SM30+
	// Flipbook requires SM35+
	// the following maps are skipped on < SM35: Depth => SoftFade, Occlusion, DetailNormal, Normal, DissolveBorder, Height, PBS1, Gooch Bright and Dark, Thickness
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// Basic setup
	/////////////////////////////////////////////////////////////////////////////////////////////

	#if UNITY_VERSION >= 202220 && defined(LOD_FADE_CROSSFADE)
		#ifndef MK_LOD_FADE_CROSSFADE
			#define MK_LOD_FADE_CROSSFADE
		#endif
	#endif

	#if UNITY_VERSION >= 202220 && defined(MK_URP) && defined(_WRITE_RENDERING_LAYERS)
		#ifndef MK_WRITE_RENDERING_LAYERS
			#define MK_WRITE_RENDERING_LAYERS
		#endif
	#endif

	#if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH)
		#ifndef MK_SHADER_API_MOBILE
			#define MK_SHADER_API_MOBILE
		#endif
	#endif

	#if SHADER_TARGET >= 35 && !defined(MK_SHADER_API_MOBILE) && defined(MK_LOCAL_ANTIALIASING)
		#ifndef MK_LOCAL_ANTIALIASING
			#define MK_LOCAL_ANTIALIASING
		#endif
	#endif

	#if defined(MK_URP) && UNITY_VERSION >= 202110
		#ifndef MK_URP_2021_1_Or_Newer
			#define MK_URP_2021_1_Or_Newer
		#endif
	#endif
	#if defined(MK_URP) && UNITY_VERSION >= 202020
		#ifndef MK_URP_2020_2_Or_Newer
			#define MK_URP_2020_2_Or_Newer
		#endif
	#endif

	#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
		#ifndef MK_PROBE_VOLUMES
			#define MK_PROBE_VOLUMES
		#endif
	#endif

	//Just a safety thing, not supported on every device
	#if defined(UNITY_COMPILER_HLSL) || defined(SHADER_API_PSSL) || defined(UNITY_COMPILER_HLSLCC)
		#define INITIALIZE_STRUCT(type, name) name = (type)0;
	#else
		#define INITIALIZE_STRUCT(type, name)
	#endif

	#if defined(MK_PARTICLES)
	#else //Standard
		#ifndef MK_STANDARD
			#define MK_STANDARD
		#endif
	#endif

	#ifdef MK_LEGACY_RP
		#ifndef MK_PARTICLES
			//handle particles instancing on legacy rp
			#include "UnityStandardParticleInstancing.cginc"
		#endif
	#endif

	#if !defined(MK_PBS) && !defined(MK_SIMPLE) && !defined(MK_UNLIT)
		#ifndef MK_UNLIT
			#define MK_UNLIT
		#endif
	#endif

	#if defined(MK_PBS) || defined(MK_SIMPLE)
		#ifndef MK_LIT
			#define MK_LIT
		#endif
	#endif

	#ifdef MK_PBS
		#if defined(_MK_WORKFLOW_SPECULAR)
			#ifndef MK_WORKFLOW_SPECULAR
				#define MK_WORKFLOW_SPECULAR
			#endif
		#elif defined(_MK_WORKFLOW_ROUGHNESS)
			#ifndef MK_WORKFLOW_ROUGHNESS
				#define MK_WORKFLOW_ROUGHNESS
			#endif
		#else
			#ifndef MK_WORKFLOW_METALLIC
				#define MK_WORKFLOW_METALLIC
			#endif
		#endif
	#endif

	#if defined(MK_WORKFLOW_METALLIC) || defined(MK_WORKFLOW_SPECULAR) || defined(MK_WORKFLOW_ROUGHNESS)
		#ifndef MK_WORKFLOW_PBS
			#define MK_WORKFLOW_PBS
		#endif
	#endif

	#ifdef MK_WORKFLOW_PBS
		#ifdef _MK_PBS_MAP_0
			#ifndef MK_PBS_MAP_0
				#define MK_PBS_MAP_0
			#endif
		#endif
		#if defined(_MK_PBS_MAP_1) && SHADER_TARGET >= 35
			#ifndef MK_PBS_MAP_1
				#define MK_PBS_MAP_1
			#endif
		#endif
	#else
		#ifdef _MK_PBS_MAP_0
			#ifndef MK_PBS_MAP_0
				#define MK_PBS_MAP_0
			#endif
		#endif
	#endif
	
	#ifdef _MK_SURFACE_TYPE_TRANSPARENT
		#ifndef MK_SURFACE_TYPE_TRANSPARENT
			#define MK_SURFACE_TYPE_TRANSPARENT
		#endif
	#else
		#ifndef MK_SURFACE_TYPE_OPAQUE
			#define MK_SURFACE_TYPE_OPAQUE
		#endif
	#endif
	#ifdef _MK_ALPHA_CLIPPING
		#ifndef MK_ALPHA_CLIPPING
			#define MK_ALPHA_CLIPPING
		#endif
	#endif
	#if defined(MK_ALPHA_CLIPPING) || defined(MK_SURFACE_TYPE_TRANSPARENT)
		#ifndef MK_ALPHA_LOOKUP
			#define MK_ALPHA_LOOKUP
		#endif
	#endif

	#if defined(_MK_FLIPBOOK) && SHADER_TARGET >= 35
		//flipbook can only be used if > 10 interpolators are available
		#ifndef MK_FLIPBOOK
			#define MK_FLIPBOOK
		#endif
	#endif

	#ifdef MK_SURFACE_TYPE_TRANSPARENT
		#if defined(_MK_BLEND_PREMULTIPLY)
			#ifndef MK_BLEND_PREMULTIPLY
				#define MK_BLEND_PREMULTIPLY
			#endif
		// Premul and additive results in the same keyword but different blend => premul
		#elif defined(_MK_BLEND_ADDITIVE)
			#ifndef MK_BLEND_ADDITIVE
				#define MK_BLEND_ADDITIVE
			#endif
		#elif defined(_MK_BLEND_MULTIPLY)
			#ifndef MK_BLEND_MULTIPLY
				#define MK_BLEND_MULTIPLY
			#endif
		#else
			#ifndef MK_BLEND_ALPHA
				#define MK_BLEND_ALPHA
			#endif
		#endif

		#ifdef MK_PARTICLES
			#if defined(_MK_COLOR_BLEND_ADDITIVE)
				#ifndef MK_COLOR_BLEND_ADDITIVE
					#define MK_COLOR_BLEND_ADDITIVE
				#endif
			#elif defined(_MK_COLOR_BLEND_SUBTRACTIVE)
				#ifndef MK_COLOR_BLEND_SUBTRACTIVE
					#define MK_COLOR_BLEND_SUBTRACTIVE
				#endif
			#elif defined(_MK_COLOR_BLEND_OVERLAY)
				#ifndef MK_COLOR_BLEND_OVERLAY
					#define MK_COLOR_BLEND_OVERLAY
				#endif
			#elif defined(_MK_COLOR_BLEND_COLOR)
				#ifndef MK_COLOR_BLEND_COLOR
					#define MK_COLOR_BLEND_COLOR
				#endif
			#elif defined(_MK_COLOR_BLEND_DIFFERENCE)
				#ifndef MK_COLOR_BLEND_DIFFERENCE
					#define MK_COLOR_BLEND_DIFFERENCE
				#endif
			#else
				#ifndef MK_COLOR_BLEND_MULTIPLY
					#define MK_COLOR_BLEND_MULTIPLY
				#endif
			#endif
		#endif

		#if defined(MK_COLOR_BLEND_ADDITIVE) || defined(MK_COLOR_BLEND_SUBTRACTIVE) || defined(MK_COLOR_BLEND_OVERLAY) || defined(MK_COLOR_BLEND_COLOR) || defined(MK_COLOR_BLEND_DIFFERENCE) || defined(MK_COLOR_BLEND_MULTIPLY)
			#ifndef MK_COLOR
				#define MK_COLOR
			#endif
		#endif

		#if defined(_MK_SOFT_FADE) && SHADER_TARGET >= 35
			#ifndef MK_SOFT_FADE
				#define MK_SOFT_FADE
			#endif
		#endif

		#ifdef _MK_CAMERA_FADE
			#ifndef MK_CAMERA_FADE
				#define MK_CAMERA_FADE
			#endif
		#endif
	#endif

	#ifdef _MK_ALBEDO_MAP
		#ifndef MK_ALBEDO_MAP
			#define MK_ALBEDO_MAP
		#endif
	#endif

	#ifdef _MK_DETAIL_MAP
		#ifndef MK_DETAIL_MAP
			#define MK_DETAIL_MAP
		#endif
	#endif

	#ifdef MK_DETAIL_MAP
		#if defined(_MK_DETAIL_BLEND_MIX)
			#ifndef MK_DETAIL_BLEND_MIX
				#define MK_DETAIL_BLEND_MIX
			#endif
		#elif defined(_MK_DETAIL_BLEND_ADD)
			#ifndef MK_DETAIL_BLEND_ADD
				#define MK_DETAIL_BLEND_ADD
			#endif
		#else
			#ifndef MK_DETAIL_BLEND_MULTIPLY
				#define MK_DETAIL_BLEND_MULTIPLY
			#endif
		#endif
	#endif
	#if defined(_MK_DETAIL_NORMAL_MAP) && SHADER_TARGET >= 35
		#ifndef MK_DETAIL_NORMAL_MAP
			#define MK_DETAIL_NORMAL_MAP
		#endif
	#endif

	#if defined(MK_ALBEDO_MAP)
		#ifndef MK_TEXCLR
			#define MK_TEXCLR
		#endif
	#else
		#ifndef MK_VERTCLR
			#define MK_VERTCLR
		#endif
	#endif

	#ifdef MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP
		#ifndef MK_VERTCLR
			#define MK_VERTCLR
		#endif
	#endif
	
	#ifdef MK_LIT
		#if (UNITY_VERSION >= 202020 && defined(_SCREEN_SPACE_OCCLUSION)) && !defined(MK_SURFACE_TYPE_TRANSPARENT)
			#ifndef MK_SCREEN_SPACE_OCCLUSION
				#define MK_SCREEN_SPACE_OCCLUSION
			#endif
		#endif
		#ifdef _MK_RECEIVE_SHADOWS
			#ifndef MK_RECEIVE_SHADOWS
				#define MK_RECEIVE_SHADOWS
			#endif
		#endif
		#ifdef _MK_THRESHOLD_MAP
			#ifndef MK_THRESHOLD_MAP
				#define MK_THRESHOLD_MAP
			#endif
		#endif
		#ifdef MK_FORWARD_BASE_PASS
			#if defined(MK_URP) && defined(_ADDITIONAL_LIGHTS_VERTEX) || defined(MK_LWRP) && defined(_ADDITIONAL_LIGHTS_VERTEX) || defined(MK_LEGACY_RP) && defined(VERTEXLIGHT_ON)
				#ifndef MK_VERTEX_LIGHTING
					#define MK_VERTEX_LIGHTING
				#endif
			#endif
		#endif

		#if defined(_MK_NORMAL_MAP) && SHADER_TARGET >= 35
			#ifndef MK_NORMAL_MAP
				#define MK_NORMAL_MAP
			#endif
		#endif

		#ifdef _MK_LIGHT_TRANSMISSION_TRANSLUCENT
			#ifndef MK_LIGHT_TRANSMISSION_TRANSLUCENT
				#define MK_LIGHT_TRANSMISSION_TRANSLUCENT
			#endif
		#endif
		#ifdef _MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING
			#ifndef MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING
				#define MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING
			#endif
		#endif

		#if defined(_MK_THICKNESS_MAP) && SHADER_TARGET >= 35
			#ifndef MK_THICKNESS_MAP
				#define MK_THICKNESS_MAP
			#endif
		#endif

		#if defined(MK_LIGHT_TRANSMISSION_TRANSLUCENT) || defined(MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING)
			#ifndef MK_LightTransmission
				#define MK_LightTransmission
			#endif
		#endif

		#if defined(_MK_HEIGHT_MAP) && SHADER_TARGET >= 35
			#ifndef MK_HEIGHT_MAP
				#define MK_HEIGHT_MAP
			#endif
		#endif

		#if defined(_MK_OCCLUSION_MAP) && SHADER_TARGET >= 35
			#ifndef MK_OCCLUSION_MAP
				#define MK_OCCLUSION_MAP
			#endif
		#endif

		#if defined(_MK_LIGHT_CEL)
			#ifndef MK_LIGHT_CEL
				#define MK_LIGHT_CEL
			#endif
		#elif defined(_MK_LIGHT_BANDED)
			#ifndef MK_LIGHT_BANDED
				#define MK_LIGHT_BANDED
			#endif
		#elif defined(_MK_LIGHT_RAMP)
			#ifndef MK_LIGHT_RAMP
				#define MK_LIGHT_RAMP
			#endif
		#else
			#ifndef MK_LIGHT_BUILTIN
				#define MK_LIGHT_BUILTIN
			#endif
		#endif

		#if defined(_MK_ARTISTIC_DRAWN)
			#ifndef MK_ARTISTIC_DRAWN
				#define MK_ARTISTIC_DRAWN
			#endif
		#elif defined(_MK_ARTISTIC_HATCHING)
			#ifndef MK_ARTISTIC_HATCHING
				#define MK_ARTISTIC_HATCHING
			#endif
		#elif defined(_MK_ARTISTIC_SKETCH)
			#ifndef MK_ARTISTIC_SKETCH
				#define MK_ARTISTIC_SKETCH
			#endif
		#else
			#ifndef MK_ARTISTIC_OFF
				#define MK_ARTISTIC_OFF
			#endif
		#endif

		#if defined(_MK_ARTISTIC_DRAWN) || defined(MK_ARTISTIC_HATCHING) || defined(MK_ARTISTIC_SKETCH)
			#ifndef MK_ARTISTIC
				#define MK_ARTISTIC
			#endif
		#endif

		#if defined(MK_ARTISTIC)
			#if defined(_MK_ARTISTIC_PROJECTION_SCREEN_SPACE)
				#ifndef MK_ARTISTIC_PROJECTION_SCREEN_SPACE
					#define MK_ARTISTIC_PROJECTION_SCREEN_SPACE
				#endif
			#else
				#ifndef MK_ARTISTIC_PROJECTION_TANGENT_SPACE
					#define MK_ARTISTIC_PROJECTION_TANGENT_SPACE
				#endif
			#endif

			#if defined(_MK_ARTISTIC_ANIMATION_STUTTER)
				#ifndef MK_ARTISTIC_ANIMATION_STUTTER
					#define MK_ARTISTIC_ANIMATION_STUTTER
				#endif
			#endif
		#endif

		#if defined(_MK_GOOCH_RAMP)
			#ifndef MK_GOOCH_RAMP
				#define MK_GOOCH_RAMP
			#endif
		#endif
		#if defined(_MK_GOOCH_BRIGHT_MAP) && SHADER_TARGET >= 35
			#ifndef MK_GOOCH_BRIGHT_MAP
				#define MK_GOOCH_BRIGHT_MAP
			#endif
		#endif
		#if defined(_MK_GOOCH_DARK_MAP) && SHADER_TARGET >= 35
			#ifndef MK_GOOCH_DARK_MAP
				#define MK_GOOCH_DARK_MAP
			#endif
		#endif
		#if defined(MK_GOOCH_BRIGHT_MAP) && defined(MK_GOOCH_DARK_MAP)
			#ifndef MK_GOOCH_BRIGHT_AND_DARK_MAP
				#define MK_GOOCH_BRIGHT_AND_DARK_MAP
			#endif
		#endif

		#if defined(MK_GOOCH_BRIGHT_MAP) || defined(MK_GOOCH_DARK_MAP) || defined(MK_GOOCH_BRIGHT_AND_DARK_MAP)
			#ifndef MK_GOOCH_MAP
				#define MK_GOOCH_MAP
			#endif
		#endif
		
		#if defined(MK_FORWARD_BASE_PASS)
			#if defined(_MK_RIM_DEFAULT)
				#ifndef MK_RIM_DEFAULT
					#define MK_RIM_DEFAULT
				#endif
			#elif defined(_MK_RIM_SPLIT)
				#ifndef MK_RIM_SPLIT
					#define MK_RIM_SPLIT
				#endif
			#endif
		#endif

		#if defined(MK_RIM_DEFAULT) || defined(MK_RIM_SPLIT)
			#ifndef MK_RIM
				#define MK_RIM
			#endif
		#endif

		#if SHADER_TARGET >= 30 && defined(MK_RIM) && defined(MK_SURFACE_TYPE_TRANSPARENT) && !defined(MK_SHADER_API_MOBILE)
			#ifndef MK_VFACE
				#define MK_VFACE
			#endif
		#endif

		#if defined(MK_FORWARD_BASE_PASS)
			#ifdef _MK_IRIDESCENCE_DEFAULT
				#ifndef MK_IRIDESCENCE_DEFAULT
					#define MK_IRIDESCENCE_DEFAULT
				#endif
			#endif
			#ifdef MK_IRIDESCENCE_DEFAULT
				#ifndef MK_IRIDESCENCE
					#define MK_IRIDESCENCE
				#endif
			#endif
		#endif

		#if (defined(MK_META_PASS) || defined(MK_FORWARD_BASE_PASS))
			#ifdef _MK_EMISSION
				#ifndef MK_EMISSION
					#define MK_EMISSION
				#endif
			#endif
			#ifdef _MK_EMISSION_MAP
				#ifndef MK_EMISSION_MAP
					#define MK_EMISSION_MAP
				#endif
			#endif
		#endif

		#if defined(_MK_DIFFUSE_OREN_NAYAR)
			#ifndef MK_DIFFUSE_OREN_NAYAR
				#define MK_DIFFUSE_OREN_NAYAR
			#endif
		#elif defined(_MK_DIFFUSE_MINNAERT)
			#ifndef MK_DIFFUSE_MINNAERT
				#define MK_DIFFUSE_MINNAERT
			#endif
		#else
			#ifndef MK_DIFFUSE_LAMBERT
				#define MK_DIFFUSE_LAMBERT
			#endif
		#endif

		#if defined(_MK_WRAPPED_DIFFUSE)
			#ifndef MK_WRAPPED_DIFFUSE
				#define MK_WRAPPED_DIFFUSE
			#endif
		#endif

		#if defined(_MK_SPECULAR_ISOTROPIC)
			#ifndef MK_SPECULAR_ISOTROPIC
				#define MK_SPECULAR_ISOTROPIC
			#endif
		#elif defined(_MK_SPECULAR_ANISOTROPIC) && SHADER_TARGET >= 30
			#ifndef MK_SPECULAR_ANISOTROPIC
				#define MK_SPECULAR_ANISOTROPIC
			#endif
		#endif

		#if defined(MK_SPECULAR_ISOTROPIC) || defined(MK_SPECULAR_ANISOTROPIC)
			#ifndef MK_SPECULAR
				#define MK_SPECULAR
			#endif
		#endif

		#if defined(MK_FORWARD_BASE_PASS)
			#if defined(_MK_ENVIRONMENT_REFLECTIONS_ADVANCED)
				#ifndef MK_ENVIRONMENT_REFLECTIONS_ADVANCED
					#define MK_ENVIRONMENT_REFLECTIONS_ADVANCED
				#endif
			#elif defined(_MK_ENVIRONMENT_REFLECTIONS_AMBIENT) || defined(MK_FORCE_LIGHTMAPS)
				#ifndef MK_ENVIRONMENT_REFLECTIONS_AMBIENT
					#define MK_ENVIRONMENT_REFLECTIONS_AMBIENT
				#endif
			#else
				#ifndef MK_ENVIRONMENT_REFLECTIONS_OFF
					#define MK_ENVIRONMENT_REFLECTIONS_OFF
				#endif
			#endif

			#if defined(MK_ENVIRONMENT_REFLECTIONS_ADVANCED) || defined(MK_ENVIRONMENT_REFLECTIONS_AMBIENT)
				#ifndef MK_ENVIRONMENT_REFLECTIONS
					#define MK_ENVIRONMENT_REFLECTIONS
				#endif
			#endif
		#endif

		#if defined(MK_FORWARD_BASE_PASS) && defined(MK_ENVIRONMENT_REFLECTIONS)
			#ifdef _MK_FRESNEL_HIGHLIGHTS
				#ifndef MK_FRESNEL_HIGHLIGHTS
					#define MK_FRESNEL_HIGHLIGHTS
				#endif
			#endif
		#endif

		#if defined(MK_ENVIRONMENT_REFLECTIONS) || defined(MK_EMISSION) || defined(MK_FRESNEL_HIGHLIGHTS)
			#ifndef MK_INDIRECT
				#define MK_INDIRECT
			#endif
		#endif
	#endif

	#if defined(_MK_COLOR_GRADING_ALBEDO)
		#ifndef MK_COLOR_GRADING_ALBEDO
			#define MK_COLOR_GRADING_ALBEDO
		#endif
	#elif defined(_MK_COLOR_GRADING_FINAL_OUTPUT)
		#ifndef MK_COLOR_GRADING_FINAL_OUTPUT
			#define MK_COLOR_GRADING_FINAL_OUTPUT
		#endif
	#endif
	#if defined(MK_COLOR_GRADING_ALBEDO) || defined(MK_COLOR_GRADING_FINAL_OUTPUT)
		#ifndef MK_COLOR_GRADING
			#define MK_COLOR_GRADING
		#endif
	#endif

	#if defined(_MK_VERTEX_ANIMATION_SINE)
		#ifndef MK_VERTEX_ANIMATION_SINE
			#define MK_VERTEX_ANIMATION_SINE
		#endif
	#elif defined(_MK_VERTEX_ANIMATION_PULSE)
		#ifndef MK_VERTEX_ANIMATION_PULSE
			#define MK_VERTEX_ANIMATION_PULSE
		#endif
	#elif defined(_MK_VERTEX_ANIMATION_NOISE)
		#ifndef MK_VERTEX_ANIMATION_NOISE
			#define MK_VERTEX_ANIMATION_NOISE
		#endif
	#endif

	#if defined(MK_VERTEX_ANIMATION_SINE) || defined(MK_VERTEX_ANIMATION_PULSE) || defined(MK_VERTEX_ANIMATION_NOISE)
		#ifndef MK_VERTEX_ANIMATION
			#define MK_VERTEX_ANIMATION
		#endif
	#endif

	#if defined(MK_VERTEX_ANIMATION)
		#if defined(_MK_VERTEX_ANIMATION_MAP) && SHADER_TARGET >= 30
			#ifndef MK_VERTEX_ANIMATION_MAP
				#define MK_VERTEX_ANIMATION_MAP
			#endif
		#endif

		#if defined(_MK_VERTEX_ANIMATION_STUTTER)
			#ifndef MK_VERTEX_ANIMATION_STUTTER
				#define MK_VERTEX_ANIMATION_STUTTER
			#endif
		#endif

		#if defined(MK_VERTEX_ANIMATION_MAP)
			#define PASS_VERTEX_ANIMATION_UV(uv) uv
		#else
			#define PASS_VERTEX_ANIMATION_UV(uv) 0
		#endif
	#endif

	#if defined(_MK_DISSOLVE_DEFAULT)
		#ifndef MK_DISSOLVE_DEFAULT
			#define MK_DISSOLVE_DEFAULT
		#endif
	#elif defined(_MK_DISSOLVE_BORDER_COLOR)
		#ifndef MK_DISSOLVE_BORDER_COLOR
			#define MK_DISSOLVE_BORDER_COLOR
		#endif
	#elif defined(_MK_DISSOLVE_BORDER_RAMP) && SHADER_TARGET >= 35
		#ifndef MK_DISSOLVE_BORDER_RAMP
			#define MK_DISSOLVE_BORDER_RAMP
		#endif
	#endif

	#if defined(MK_DISSOLVE_DEFAULT) || defined(MK_DISSOLVE_BORDER_COLOR) || defined(MK_DISSOLVE_BORDER_RAMP)
		#ifndef MK_DISSOLVE
			#define MK_DISSOLVE
		#endif
	#endif

	#ifdef MK_OUTLINE_PASS
		#if defined(_MK_OUTLINE_HULL_ORIGIN)
			#ifndef MK_OUTLINE_HULL_ORIGIN
				#define MK_OUTLINE_HULL_ORIGIN
			#endif
		#elif defined(_MK_OUTLINE_HULL_CLIP)
			#ifndef MK_OUTLINE_HULL_CLIP
				#define MK_OUTLINE_HULL_CLIP
			#endif
		#else
			#ifndef MK_OUTLINE_HULL_OBJECT
				#define MK_OUTLINE_HULL_OBJECT
			#endif
		#endif

		#if defined(_MK_OUTLINE_DATA_UV7)
			#ifndef MK_OUTLINE_DATA_UV7
				#define MK_OUTLINE_DATA_UV7
			#endif
		#else
			#ifndef MK_OUTLINE_DATA_NORMAL
				#define MK_OUTLINE_DATA_NORMAL
			#endif
		#endif

		#if defined(_MK_OUTLINE_NOISE)
			#ifndef MK_OUTLINE_NOISE
				#define MK_OUTLINE_NOISE
			#endif
		#else
			#ifndef MK_OUTLINE_NOISE_OFF
				#define MK_OUTLINE_NOISE_OFF
			#endif
		#endif

		#if defined(_MK_OUTLINE_MAP)
			#ifndef MK_OUTLINE_MAP
				#define MK_OUTLINE_MAP
			#endif
		#endif
	#endif

	#ifdef MK_REFRACTION
		#ifdef _MK_REFRACTION_DISTORTION_MAP
			#ifndef MK_REFRACTION_DISTORTION_MAP
				#define MK_REFRACTION_DISTORTION_MAP
			#endif
		#endif
		#ifdef _MK_INDEX_OF_REFRACTION
			#ifndef MK_INDEX_OF_REFRACTION
				#define MK_INDEX_OF_REFRACTION
			#endif
		#endif
	#endif

	#if defined(MK_DEPTH_NORMALS_PASS) || defined(MK_PROBE_VOLUMES) || defined(MK_LIT) || defined(MK_INDEX_OF_REFRACTION) || defined(MK_VERTEX_ANIMATION_PULSE) || defined(MK_VERTEX_ANIMATION_NOISE)
		#ifndef MK_NORMAL
			#define MK_NORMAL
		#endif
	#endif

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Constants
	/////////////////////////////////////////////////////////////////////////////////////////////
	#ifndef OUTLINE_OBJECT_SCALE
		#define OUTLINE_OBJECT_SCALE 0.0005
	#endif
	#ifndef OUTLINE_ORIGIN_SCALE
		#define OUTLINE_ORIGIN_SCALE 0.001
	#endif
	#ifndef K_SPEC_DIELECTRIC_MIN
		#define K_SPEC_DIELECTRIC_MIN 0.04
	#endif
	#ifndef K_SPEC_DIELECTRIC_MAX
		#define K_SPEC_DIELECTRIC_MAX 0.96
	#endif
	#ifndef HALF_MIN
		#define HALF_MIN 6.10e-5
	#endif
	#ifndef ONE_MINUS_HALF_MIN
		#define ONE_MINUS_HALF_MIN 0.999939
	#endif
	#ifndef T_Q
		#define T_Q 0.125
	#endif
	#ifndef T_H
		#define T_H 0.25
	#endif
	#ifndef T_V
		#define T_V 0.5
	#endif
	#ifndef THRESHOLD_OFFSET_NORMALIZER
		#define THRESHOLD_OFFSET_NORMALIZER 0.125
	#endif
	/*
	#ifndef SHINE_MULT
		//approximately URP smoothness base
		#define SHINE_MULT 1024
	#endif
	*/
	#ifndef PI
		#define PI 3.141592
	#endif
	#ifndef PI_TWO
		#define PI_TWO 6.283185
	#endif
	#ifndef PI_H
		#define PI_H 1.570796
	#endif
	#ifndef PI_P2
		#define PI_P2 9.869604
	#endif
	#ifndef INV_PI
		#define INV_PI 0.318309
	#endif
	#ifndef REL_LUMA
		#define REL_LUMA half3(0.2126,0.7152,0.0722)
	#endif
	#ifndef REFRACTION_DISTORTION_SCALE
		#define REFRACTION_DISTORTION_SCALE 0.1
	#endif
	#ifndef HALF3_ONE
		#define HALF3_ONE half3(1.0h, 1.0h, 1.0h)
	#endif
	#ifndef REFERENCE_RESOLUTION
		#define REFERENCE_RESOLUTION half2(3840, 2160)
	#endif
	#ifndef REFERENCE_ASPECT
		#define REFERENCE_ASPECT half2(1.777778, 0.5625)
	#endif
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// Input dependent defines
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if defined(MK_PBS_MAP_0) || defined(MK_PBS_MAP_1) || defined(MK_POLYBRUSH) || defined(MK_VERTEX_ANIMATION_NOISE) || defined(MK_OUTLINE_MAP) || defined(MK_OUTLINE_NOISE) || defined(MK_VERTEX_ANIMATION_MAP) || defined(MK_THRESHOLD_MAP) || defined(MK_PARTICLES) || defined(MK_ALBEDO_MAP) || defined(MK_DISSOLVE) || defined(MK_GOOCH_MAP) || defined(MK_THICKNESS_MAP) || defined(MK_NORMAL_MAP) || defined(MK_DETAIL_NORMAL_MAP) || defined(MK_EMISSION_MAP) || defined(MK_OCCLUSION_MAP) || defined(MK_HEIGHT_MAP) || defined(MK_ARTISTIC_HATCHING) || defined(MK_ARTISTIC_DRAWN) || defined(MK_ARTISTIC_SKETCH) || defined(MK_REFRACTION_DISTORTION_MAP)
		#ifndef MK_TCM
			#define MK_TCM
		#endif
	#endif

	#if defined(MK_DETAIL_MAP) || defined(MK_DETAIL_NORMAL_MAP)
		#ifndef MK_TCD
			#define MK_TCD
		#endif
	#endif

	#if defined(MK_LIT) && defined(MK_HEIGHT_MAP) && defined(_MK_PARALLAX) && SHADER_TARGET >= 35
		#ifndef MK_PARALLAX
			#define MK_PARALLAX
		#endif
	#endif

	#if defined(MK_PARALLAX)
		#ifndef MK_VD_O
			#define MK_VD_O
		#endif
	#endif
	
	#if defined(MK_NORMAL_MAP) || defined(MK_DETAIL_NORMAL_MAP) || defined(MK_VD_O) || defined(MK_SPECULAR_ANISOTROPIC)
		#ifndef	MK_TBN	
			#define MK_TBN
		#endif
	#else
		#ifndef MK_WN
			#define MK_WN
		#endif
	#endif

	#if defined(MK_PROBE_VOLUMES) || defined(MK_REFRACTION) || defined(MK_IRIDESCENCE) || defined(MK_LightTransmission) || defined(MK_RIM) || defined(MK_SPECULAR) || defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH) || defined(DYNAMICLIGHTMAP_ON) || defined(DIRLIGHTMAP_COMBINED) || defined(MK_RIM) || defined(MK_ENVIRONMENT_REFLECTIONS_ADVANCED) || defined(MK_FRESNEL_HIGHLIGHTS) || defined(MK_DIFFUSE_MINNAERT) || defined(MK_DIFFUSE_OREN_NAYAR)
		#ifndef MK_VD
			#define MK_VD
		#endif
	#endif

	#if	defined(MK_ARTISTIC) && defined(MK_ARTISTIC_PROJECTION_SCREEN_SPACE) || defined(MK_DISSOLVE_PROJECTION_SCREEN_SPACE)
		#ifndef MK_NORMALIZED_SCREEN_UV
			#define MK_NORMALIZED_SCREEN_UV
		#endif
	#endif

	#if	(UNITY_VERSION >= 202220 && defined(MK_INDIRECT)) || defined(USE_CLUSTERED_LIGHTING) || defined(USE_FORWARD_PLUS) || defined(MK_REFRACTION) || defined(MK_SOFT_FADE) || defined(MK_CAMERA_FADE) || defined(MK_NORMALIZED_SCREEN_UV) || defined(MK_SCREEN_SPACE_OCCLUSION)
		#ifndef MK_SCREEN_UV
			#define MK_SCREEN_UV
		#endif
	#endif
	
	#if defined(MK_NORMALIZED_SCREEN_UV)
		#ifndef MK_POS_NULL_CLIP
			#define MK_POS_NULL_CLIP
		#endif
	#endif

	#if defined(MK_POS_NULL_CLIP) || defined(MK_REFRACTION) || defined(MK_SOFT_FADE) || defined(MK_CAMERA_FADE) || defined(MK_SCREEN_UV)
		#ifndef MK_BARYCENTRIC_POS_CLIP
			#define MK_BARYCENTRIC_POS_CLIP
		#endif
	#endif

	#ifdef MK_LIT
		#ifndef MK_N_DOT_L
			#define MK_N_DOT_L
		#endif
	#endif

	#if defined(MK_IRIDESCENCE) || defined(MK_FRESNEL_HIGHLIGHTS) || defined(MK_SPECULAR) || defined(MK_DIFFUSE_MINNAERT) || defined(MK_DIFFUSE_OREN_NAYAR) || defined(MK_RIM)
		#ifndef MK_V_DOT_N
			#define MK_V_DOT_N
		#endif
	#endif
	#ifdef MK_DIFFUSE_OREN_NAYAR
		#ifndef MK_V_DOT_L
			#define MK_V_DOT_L
		#endif
	#endif
	#if defined(MK_SPECULAR)
		#ifdef MK_SPECULAR_ANISOTROPIC
			#ifndef MK_T_DOT_LHV
				#define MK_T_DOT_LHV
			#endif
			#ifndef MK_B_DOT_LHV
				#define MK_B_DOT_LHV
			#endif
		#endif
		#ifndef MK_LHV
			#define MK_LHV
		#endif
		#ifndef MK_N_DOT_LHV
			#define MK_N_DOT_LHV
		#endif
		//used for C.Sch Fresnel, if switched back to Schlick this should be disabled
		#ifndef MK_L_DOT_LHV
			#define MK_L_DOT_LHV
		#endif
		/*
		//used for Schlick Fresnel
		#ifndef MK_V_DOT_LHV
			#define MK_V_DOT_LHV
		#endif
		*/
	#endif

	#ifdef MK_LightTransmission
		#ifndef MK_LND
			#define MK_LND
		#endif
		#ifndef MK_V_DOT_LND
			#define MK_V_DOT_LND
		#endif
	#endif
	
	/*
	#ifdef MK_LIGHTMODEL_PHONG
		#ifndef MK_ML_REF_N
			#define MK_ML_REF_N
		#endif
	#endif
	*/
	#ifdef MK_ENVIRONMENT_REFLECTIONS_ADVANCED
		#ifndef MK_MV_REF_N
			#define MK_MV_REF_N
		#endif
	#endif
	/*
	#if defined(MK_TLD) || defined(MK_TLM)
		#ifndef MK_ML_DOT_V
			#define MK_ML_DOT_V
		#endif
	#endif
	*/
	/*
	#ifdef MK_LIGHTMODEL_PHONG
		#ifndef MK_ML_REF_N_DOT_V
			#define MK_ML_REF_N_DOT_V
		#endif
	#endif
	*/

	#if defined(MK_TBN) || defined(MK_FLIPBOOK) || defined(MK_TCM) || defined(MK_TCD) || defined(MK_T_DOT_LHV) || defined(MK_B_DOT_LHV) || defined(MK_WORKFLOW_PBS) || defined(MK_ML_REF_N_DOT_V) || defined(MK_ML_DOT_V) || defined(MK_MV_REF_N) || defined(MK_ML_REF_N) || defined(MK_N_DOT_LHV) || defined(MK_LHV) || defined(MK_N_DOT_L) || defined(MK_V_DOT_L) || defined(MK_V_DOT_N) || defined(MK_LIT) || defined(MK_DISSOLVE) || defined(MK_REFRACTION)
		#ifndef MK_SURFACE_DATA_REQUIRED
			#define MK_SURFACE_DATA_REQUIRED
		#endif
	#endif

	#if defined(MK_LIT) || defined(MK_VD)
		#ifndef MK_POS_WORLD
			#define MK_POS_WORLD
		#endif
	#endif

	#if defined(MK_FORWARD_BASE_PASS) || defined(MK_OUTLINE_PASS)
		#ifndef MK_FOG
			#define MK_FOG
		#endif
	#endif

	//#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON) || defined(MK_ENVIRONMENT_REFLECTIONS) || (defined( SHADOWS_SCREEN ) && defined( LIGHTMAP_ON ))
	#ifdef MK_LIT
		#ifndef MK_LIGHTMAP_UV
			#define MK_LIGHTMAP_UV
		#endif
	#endif

	#if defined(MK_VERTCLR) || defined(MK_PARTICLES) || defined(MK_POLYBRUSH)
		#ifndef MK_VERTEX_COLOR_REQUIRED
			#define MK_VERTEX_COLOR_REQUIRED
		#endif
	#endif

	#if !defined(MK_DECLARE_V_FACE) && defined(MK_VFACE)
		#define MK_DECLARE_V_FACE , half vFace : VFACE
	#else
		#define MK_DECLARE_V_FACE
	#endif
#endif