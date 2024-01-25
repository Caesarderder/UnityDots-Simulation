//////////////////////////////////////////////////////
// MK Toon Uniform					       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_UNIFORM
	#define MK_TOON_UNIFORM

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
	#else
		#include "UnityCG.cginc"
	#endif

	#include "Pipeline.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// UNIFORM VARIABLES
	/////////////////////////////////////////////////////////////////////////////////////////////
	// The compiler should optimized the code by stripping away unused uniforms
	// This way its also possible to avoid an inconsistent buffer size error and
	// use the SRP Batcher, while compile different variants of the shader
	// Every PerDraw (builtin engine variables) should accessed via the builtin include files
	// Its not clear if a block based setup for the srp batcher is required,
	// therefore all uniforms are grouped this way:
	//
	// fixed 	 | fixed2 	 | fixed3 	 | fixed4
	// half  	 | half2  	 | half3  	 | half4
	// float 	 | float2 	 | float3 	 | float4
	// Sampler2D | Sampler3D
	
	CBUFFER_START(UnityPerMaterial)
		uniform float4 _AlbedoMap_ST;
		uniform float4 _MainTex_ST;
		uniform float4 _DetailMap_ST;

		uniform half4 _AlbedoColor;
		uniform half4 _DissolveBorderColor;
		uniform half4 _OutlineColor;
		uniform half4 _IridescenceColor;
		uniform half4 _RimColor;
		uniform half4 _RimBrightColor;
		uniform half4 _RimDarkColor;
		uniform half4 _GoochDarkColor;
		uniform half4 _GoochBrightColor;
		uniform half4 _VertexAnimationFrequency;

		uniform half3 _DetailColor;
		uniform half3 _SpecularColor;
		uniform half3 _LightTransmissionColor;

		uniform half3 _EmissionColor;

		uniform float _SoftFadeNearDistance;
		uniform float _SoftFadeFarDistance;
		uniform float _CameraFadeNearDistance;
		uniform float _CameraFadeFarDistance;
		uniform float _OutlineFadeMin;
        uniform float _OutlineFadeMax;

		uniform half _AlphaCutoff;
		uniform half _Metallic;
		uniform half _Smoothness;
		uniform half _Roughness;
		uniform half _Anisotropy;
		uniform half _LightTransmissionDistortion;
		uniform half _LightBandsScale;
		uniform half _LightThreshold;
		uniform half _DrawnClampMin;
		uniform half _DrawnClampMax;
		uniform half _Contrast;
		uniform half _Saturation;
		uniform half _Brightness;
		uniform half _DiffuseSmoothness;
		uniform half _DiffuseThresholdOffset;
		uniform half _SpecularSmoothness;
		uniform half _SpecularThresholdOffset;
		uniform half _RimSmoothness;
		uniform half _RimThresholdOffset;
		uniform half _IridescenceSmoothness;
		uniform half _IridescenceThresholdOffset;
		uniform half _LightTransmissionSmoothness;
		uniform half _LightTransmissionThresholdOffset;
		uniform half _RimSize;
		uniform half _IridescenceSize;
		uniform half _DissolveAmount;
		uniform half _DissolveBorderSize;
		uniform half _OutlineNoise;
		uniform half _DiffuseWrap;
		uniform half _DetailMix;
		uniform half _RefractionDistortionFade;
		uniform half _GoochRampIntensity;
		uniform half _VertexAnimationIntensity;
		uniform half _IndirectFade;

		uniform half _DetailNormalMapIntensity;
		uniform half _NormalMapIntensity;
		uniform half _Parallax;
		uniform half _OcclusionMapIntensity;
		uniform half _LightBands;
		uniform half _ThresholdMapScale;
		uniform half _ArtisticFrequency;
		uniform half _DissolveMapScale;
		uniform half _DrawnMapScale;
		uniform half _SketchMapScale;
		uniform half _HatchingMapScale;
		uniform half _OutlineSize;
		uniform half _SpecularIntensity;
		uniform half _LightTransmissionIntensity;
		uniform half _RefractionDistortionMapScale;
		uniform half _IndexOfRefraction;
		uniform half _RefractionDistortion;
	CBUFFER_END

	#ifdef UNITY_DOTS_INSTANCING_ENABLED
		UNITY_DOTS_INSTANCING_START(UserPropertyMetadata)
			UNITY_DOTS_INSTANCED_PROP(float4, _AlbedoMap_ST)
			UNITY_DOTS_INSTANCED_PROP(float4, _MainTex_ST)
			UNITY_DOTS_INSTANCED_PROP(float4, _DetailMap_ST)
			UNITY_DOTS_INSTANCED_PROP(float4, _AlbedoColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _DissolveBorderColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _OutlineColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _IridescenceColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _RimColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _RimBrightColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _RimDarkColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _GoochDarkColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _GoochBrightColor)
			UNITY_DOTS_INSTANCED_PROP(float4, _VertexAnimationFrequency)
			UNITY_DOTS_INSTANCED_PROP(float3, _DetailColor)
			UNITY_DOTS_INSTANCED_PROP(float3, _SpecularColor)
			UNITY_DOTS_INSTANCED_PROP(float3, _LightTransmissionColor)
			UNITY_DOTS_INSTANCED_PROP(float3, _EmissionColor)
			UNITY_DOTS_INSTANCED_PROP(float, _SoftFadeNearDistance)
			UNITY_DOTS_INSTANCED_PROP(float, _SoftFadeFarDistance)
			UNITY_DOTS_INSTANCED_PROP(float, _CameraFadeNearDistance)
			UNITY_DOTS_INSTANCED_PROP(float, _CameraFadeFarDistance)
			UNITY_DOTS_INSTANCED_PROP(float, _OutlineFadeMin)
			UNITY_DOTS_INSTANCED_PROP(float, _OutlineFadeMax)
			UNITY_DOTS_INSTANCED_PROP(float, _AlphaCutoff)
			UNITY_DOTS_INSTANCED_PROP(float, _Metallic)
			UNITY_DOTS_INSTANCED_PROP(float, _Smoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _Roughness)
			UNITY_DOTS_INSTANCED_PROP(float, _Anisotropy)
			UNITY_DOTS_INSTANCED_PROP(float, _LightTransmissionDistortion)
			UNITY_DOTS_INSTANCED_PROP(float, _LightBandsScale)
			UNITY_DOTS_INSTANCED_PROP(float, _LightThreshold)
			UNITY_DOTS_INSTANCED_PROP(float, _DrawnClampMin)
			UNITY_DOTS_INSTANCED_PROP(float, _DrawnClampMax)
			UNITY_DOTS_INSTANCED_PROP(float, _Contrast)
			UNITY_DOTS_INSTANCED_PROP(float, _Saturation)
			UNITY_DOTS_INSTANCED_PROP(float, _Brightness)
			UNITY_DOTS_INSTANCED_PROP(float, _DiffuseSmoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _DiffuseThresholdOffset)
			UNITY_DOTS_INSTANCED_PROP(float, _SpecularSmoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _SpecularThresholdOffset)
			UNITY_DOTS_INSTANCED_PROP(float, _RimSmoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _RimThresholdOffset)
			UNITY_DOTS_INSTANCED_PROP(float, _IridescenceSmoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _IridescenceThresholdOffset)
			UNITY_DOTS_INSTANCED_PROP(float, _LightTransmissionSmoothness)
			UNITY_DOTS_INSTANCED_PROP(float, _LightTransmissionThresholdOffset)
			UNITY_DOTS_INSTANCED_PROP(float, _RimSize)
			UNITY_DOTS_INSTANCED_PROP(float, _IridescenceSize)
			UNITY_DOTS_INSTANCED_PROP(float, _DissolveAmount)
			UNITY_DOTS_INSTANCED_PROP(float, _DissolveBorderSize)
			UNITY_DOTS_INSTANCED_PROP(float, _OutlineNoise)
			UNITY_DOTS_INSTANCED_PROP(float, _DiffuseWrap)
			UNITY_DOTS_INSTANCED_PROP(float, _DetailMix)
			UNITY_DOTS_INSTANCED_PROP(float, _RefractionDistortionFade)
			UNITY_DOTS_INSTANCED_PROP(float, _GoochRampIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _VertexAnimationIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _IndirectFade)
			UNITY_DOTS_INSTANCED_PROP(float, _DetailNormalMapIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _NormalMapIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _Parallax)
			UNITY_DOTS_INSTANCED_PROP(float, _OcclusionMapIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _LightBands)
			UNITY_DOTS_INSTANCED_PROP(float, _ThresholdMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _ArtisticFrequency)
			UNITY_DOTS_INSTANCED_PROP(float, _DissolveMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _DrawnMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _SketchMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _HatchingMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _OutlineSize)
			UNITY_DOTS_INSTANCED_PROP(float, _SpecularIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _LightTransmissionIntensity)
			UNITY_DOTS_INSTANCED_PROP(float, _RefractionDistortionMapScale)
			UNITY_DOTS_INSTANCED_PROP(float, _IndexOfRefraction)
			UNITY_DOTS_INSTANCED_PROP(float, _RefractionDistortion)
		UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

		#if UNITY_VERSION >= 202330
			#define MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(type, name) static type mk_DOTS_Cached##name;
		#elif UNITY_VERSION >= 202210
			#define MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(type, name)
		#else
			#define MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(type, name)
		#endif

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _AlbedoMap_ST)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _MainTex_ST)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _DetailMap_ST)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _AlbedoColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _DissolveBorderColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _OutlineColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _IridescenceColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _RimColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _RimBrightColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _RimDarkColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _GoochDarkColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _GoochBrightColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float4, _VertexAnimationFrequency)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float3, _DetailColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float3, _SpecularColor)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float3, _LightTransmissionColor)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float3, _EmissionColor)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SoftFadeNearDistance)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SoftFadeFarDistance)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _CameraFadeNearDistance)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _CameraFadeFarDistance)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _OutlineFadeMin)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _OutlineFadeMax)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _AlphaCutoff)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Metallic)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Smoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Roughness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Anisotropy)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightTransmissionDistortion)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightBandsScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightThreshold)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DrawnClampMin)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DrawnClampMax)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Contrast)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Saturation)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Brightness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DiffuseSmoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DiffuseThresholdOffset)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SpecularSmoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SpecularThresholdOffset)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RimSmoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RimThresholdOffset)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _IridescenceSmoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _IridescenceThresholdOffset)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightTransmissionSmoothness)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightTransmissionThresholdOffset)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RimSize)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _IridescenceSize)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DissolveAmount)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DissolveBorderSize)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _OutlineNoise)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DiffuseWrap)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DetailMix)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RefractionDistortionFade)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _GoochRampIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _VertexAnimationIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _IndirectFade)

		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DetailNormalMapIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _NormalMapIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _Parallax)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _OcclusionMapIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightBands)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _ThresholdMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _ArtisticFrequency)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DissolveMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _DrawnMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SketchMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _HatchingMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _OutlineSize)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _SpecularIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _LightTransmissionIntensity)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RefractionDistortionMapScale)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _IndexOfRefraction)
		MK_DEFINE_CACHED_DOTS_INSTANCED_PROP(float, _RefractionDistortion)

		#if UNITY_VERSION >= 202210
			#ifndef MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT
				#define MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, name) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, name)
			#endif
		#else
			#ifndef MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT
				#define MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT() UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO
			#endif
		#endif

		#if UNITY_VERSION >= 202330
			void SetupDOTSLitMaterialPropertyCaches()
			{
				mk_DOTS_Cached_AlbedoMap_ST = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AlbedoMap_ST);
				mk_DOTS_Cached_MainTex_ST = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _MainTex_ST);
				mk_DOTS_Cached_DetailMap_ST = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _DetailMap_ST);

				mk_DOTS_Cached_AlbedoColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _AlbedoColor);
				mk_DOTS_Cached_DissolveBorderColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _DissolveBorderColor);
				mk_DOTS_Cached_OutlineColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _OutlineColor);
				mk_DOTS_Cached_IridescenceColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _IridescenceColor);
				mk_DOTS_Cached_RimColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _RimColor);
				mk_DOTS_Cached_RimBrightColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _RimBrightColor);
				mk_DOTS_Cached_RimDarkColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _RimDarkColor);
				mk_DOTS_Cached_GoochDarkColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _GoochDarkColor);
				mk_DOTS_Cached_GoochBrightColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _GoochBrightColor);
				mk_DOTS_Cached_VertexAnimationFrequency = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _VertexAnimationFrequency);

				mk_DOTS_Cached_DetailColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3, _DetailColor);
				mk_DOTS_Cached_SpecularColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3, _SpecularColor);
				mk_DOTS_Cached_LightTransmissionColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3, _LightTransmissionColor);

				mk_DOTS_Cached_EmissionColor = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float3, _EmissionColor);

				mk_DOTS_Cached_SoftFadeNearDistance = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SoftFadeNearDistance);
				mk_DOTS_Cached_SoftFadeFarDistance = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SoftFadeFarDistance);
				mk_DOTS_Cached_CameraFadeNearDistance = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _CameraFadeNearDistance);
				mk_DOTS_Cached_CameraFadeFarDistance = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _CameraFadeFarDistance);
				mk_DOTS_Cached_OutlineFadeMin = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OutlineFadeMin);
				mk_DOTS_Cached_OutlineFadeMax = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OutlineFadeMax);

				mk_DOTS_Cached_AlphaCutoff = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _AlphaCutoff);
				mk_DOTS_Cached_Metallic = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Metallic);
				mk_DOTS_Cached_Smoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Smoothness);
				mk_DOTS_Cached_Roughness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Roughness);
				mk_DOTS_Cached_Anisotropy = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Anisotropy);
				mk_DOTS_Cached_LightTransmissionDistortion = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightTransmissionDistortion);
				mk_DOTS_Cached_LightBandsScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightBandsScale);
				mk_DOTS_Cached_LightThreshold = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightThreshold);
				mk_DOTS_Cached_DrawnClampMin = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DrawnClampMin);
				mk_DOTS_Cached_DrawnClampMax = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DrawnClampMax);
				mk_DOTS_Cached_Contrast = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Contrast);
				mk_DOTS_Cached_Saturation = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Saturation);
				mk_DOTS_Cached_Brightness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Brightness);
				mk_DOTS_Cached_DiffuseSmoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DiffuseSmoothness);
				mk_DOTS_Cached_DiffuseThresholdOffset = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DiffuseThresholdOffset);
				mk_DOTS_Cached_SpecularSmoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SpecularSmoothness);
				mk_DOTS_Cached_SpecularThresholdOffset = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SpecularThresholdOffset);
				mk_DOTS_Cached_RimSmoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RimSmoothness);
				mk_DOTS_Cached_RimThresholdOffset = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RimThresholdOffset);
				mk_DOTS_Cached_IridescenceSmoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _IridescenceSmoothness);
				mk_DOTS_Cached_IridescenceThresholdOffset = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _IridescenceThresholdOffset);
				mk_DOTS_Cached_LightTransmissionSmoothness = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightTransmissionSmoothness);
				mk_DOTS_Cached_LightTransmissionThresholdOffset = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightTransmissionThresholdOffset);
				mk_DOTS_Cached_RimSize = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RimSize);
				mk_DOTS_Cached_IridescenceSize = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _IridescenceSize);
				mk_DOTS_Cached_DissolveAmount = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DissolveAmount);
				mk_DOTS_Cached_DissolveBorderSize = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DissolveBorderSize);
				mk_DOTS_Cached_OutlineNoise = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OutlineNoise);
				mk_DOTS_Cached_DiffuseWrap = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DiffuseWrap);
				mk_DOTS_Cached_DetailMix = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DetailMix);
				mk_DOTS_Cached_RefractionDistortionFade = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RefractionDistortionFade);
				mk_DOTS_Cached_GoochRampIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _GoochRampIntensity);
				mk_DOTS_Cached_VertexAnimationIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _VertexAnimationIntensity);
				mk_DOTS_Cached_IndirectFade = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _IndirectFade);
				mk_DOTS_Cached_DetailNormalMapIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DetailNormalMapIntensity);
				mk_DOTS_Cached_NormalMapIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _NormalMapIntensity);
				mk_DOTS_Cached_Parallax = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _Parallax);
				mk_DOTS_Cached_OcclusionMapIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OcclusionMapIntensity);
				mk_DOTS_Cached_LightBands = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightBands);
				mk_DOTS_Cached_ThresholdMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ThresholdMapScale);
				mk_DOTS_Cached_ArtisticFrequency = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _ArtisticFrequency);
				mk_DOTS_Cached_DissolveMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DissolveMapScale);
				mk_DOTS_Cached_DrawnMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _DrawnMapScale);
				mk_DOTS_Cached_SketchMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SketchMapScale);
				mk_DOTS_Cached_HatchingMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _HatchingMapScale);
				mk_DOTS_Cached_OutlineSize = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _OutlineSize);
				mk_DOTS_Cached_SpecularIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _SpecularIntensity);
				mk_DOTS_Cached_LightTransmissionIntensity = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _LightTransmissionIntensity);
				mk_DOTS_Cached_RefractionDistortionMapScale = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RefractionDistortionMapScale);
				mk_DOTS_Cached_IndexOfRefraction = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _IndexOfRefraction);
				mk_DOTS_Cached_RefractionDistortion = MK_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float, _RefractionDistortion);
			}
		#endif

		#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
		#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSLitMaterialPropertyCaches()

		#if UNITY_VERSION >= 202330
			#define MK_SET_DOTS_INSTANCED_PROP(type, name) mk_DOTS_Cached##name
		#elif UNITY_VERSION >= 202210
			#define MK_SET_DOTS_INSTANCED_PROP(type, name) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, name)
		#else
			#define MK_SET_DOTS_INSTANCED_PROP(type, name) UNITY_ACCESS_DOTS_INSTANCED_PROP_FROM_MACRO(type, Metadata_##name)
		#endif

		#define _AlbedoMap_ST MK_SET_DOTS_INSTANCED_PROP(float4, _AlbedoMap_ST)
		#define _MainTex_ST MK_SET_DOTS_INSTANCED_PROP(float4, _MainTex_ST)
		#define _DetailMap_ST MK_SET_DOTS_INSTANCED_PROP(float4, _DetailMap_ST)
		#define _AlbedoColor MK_SET_DOTS_INSTANCED_PROP(float4, _AlbedoColor)
		#define _DissolveBorderColor MK_SET_DOTS_INSTANCED_PROP(float4, _DissolveBorderColor)
		#define _OutlineColor MK_SET_DOTS_INSTANCED_PROP(float4, _OutlineColor)
		#define _IridescenceColor MK_SET_DOTS_INSTANCED_PROP(float4, _IridescenceColor)
		#define _RimColor MK_SET_DOTS_INSTANCED_PROP(float4, _RimColor)
		#define _RimBrightColor MK_SET_DOTS_INSTANCED_PROP(float4, _RimBrightColor)
		#define _RimDarkColor MK_SET_DOTS_INSTANCED_PROP(float4, _RimDarkColor)
		#define _GoochDarkColor MK_SET_DOTS_INSTANCED_PROP(float4, _GoochDarkColor)
		#define _GoochBrightColor MK_SET_DOTS_INSTANCED_PROP(float4, _GoochBrightColor)
		#define _VertexAnimationFrequency MK_SET_DOTS_INSTANCED_PROP(float4, _VertexAnimationFrequency)
		#define _DetailColor MK_SET_DOTS_INSTANCED_PROP(float3, _DetailColor)
		#define _SpecularColor MK_SET_DOTS_INSTANCED_PROP(float3, _SpecularColor)
		#define _LightTransmissionColor MK_SET_DOTS_INSTANCED_PROP(float3, _LightTransmissionColor)
		#define _EmissionColor MK_SET_DOTS_INSTANCED_PROP(float3, _EmissionColor)
		#define _SoftFadeNearDistance MK_SET_DOTS_INSTANCED_PROP(float, _SoftFadeNearDistance)
		#define _SoftFadeFarDistance MK_SET_DOTS_INSTANCED_PROP(float, _SoftFadeFarDistance)
		#define _CameraFadeNearDistance MK_SET_DOTS_INSTANCED_PROP(float, _CameraFadeNearDistance)
		#define _CameraFadeFarDistance MK_SET_DOTS_INSTANCED_PROP(float, _CameraFadeFarDistance 
		#define _OutlineFadeMin MK_SET_DOTS_INSTANCED_PROP(float, _OutlineFadeMin)
		#define _OutlineFadeMax MK_SET_DOTS_INSTANCED_PROP(float, _OutlineFadeMax)
		#define _AlphaCutoff MK_SET_DOTS_INSTANCED_PROP(float, _AlphaCutoff)
		#define _Metallic MK_SET_DOTS_INSTANCED_PROP(float, _Metallic)
		#define _Smoothness MK_SET_DOTS_INSTANCED_PROP(float, _Smoothness)
		#define _Roughness MK_SET_DOTS_INSTANCED_PROP(float, _Roughness)
		#define _Anisotropy MK_SET_DOTS_INSTANCED_PROP(float, _Anisotropy)
		#define _LightTransmissionDistortion MK_SET_DOTS_INSTANCED_PROP(float, _LightTransmissionDistortion)
		#define _LightBandsScale MK_SET_DOTS_INSTANCED_PROP(float, _LightBandsScale)
		#define _LightThreshold MK_SET_DOTS_INSTANCED_PROP(float, _LightThreshold)
		#define _DrawnClampMin MK_SET_DOTS_INSTANCED_PROP(float, _DrawnClampMin)
		#define _DrawnClampMax MK_SET_DOTS_INSTANCED_PROP(float, _DrawnClampMax)
		#define _Contrast MK_SET_DOTS_INSTANCED_PROP(float, _Contrast)
		#define _Saturation MK_SET_DOTS_INSTANCED_PROP(float, _Saturation)
		#define _Brightness MK_SET_DOTS_INSTANCED_PROP(float, _Brightness)
		#define _DiffuseSmoothness MK_SET_DOTS_INSTANCED_PROP(float, _DiffuseSmoothness)
		#define _DiffuseThresholdOffset MK_SET_DOTS_INSTANCED_PROP(float, _DiffuseThresholdOffset)
		#define _SpecularSmoothness MK_SET_DOTS_INSTANCED_PROP(float, _SpecularSmoothness)
		#define _SpecularThresholdOffset MK_SET_DOTS_INSTANCED_PROP(float, _SpecularThresholdOffset)
		#define _RimSmoothness MK_SET_DOTS_INSTANCED_PROP(float, _RimSmoothness)
		#define _RimThresholdOffset MK_SET_DOTS_INSTANCED_PROP(float, _RimThresholdOffset)
		#define _IridescenceSmoothness MK_SET_DOTS_INSTANCED_PROP(float, _IridescenceSmoothness)
		#define _IridescenceThresholdOffset MK_SET_DOTS_INSTANCED_PROP(float, _IridescenceThresholdOffset)
		#define _LightTransmissionSmoothness MK_SET_DOTS_INSTANCED_PROP(float, _LightTransmissionSmoothness)
		#define _LightTransmissionThresholdOffset MK_SET_DOTS_INSTANCED_PROP(float, _LightTransmissionThresholdOffset)
		#define _RimSize MK_SET_DOTS_INSTANCED_PROP(float, _RimSize)
		#define _IridescenceSize MK_SET_DOTS_INSTANCED_PROP(float, _IridescenceSize)
		#define _DissolveAmount MK_SET_DOTS_INSTANCED_PROP(float, _DissolveAmount)
		#define _DissolveBorderSize MK_SET_DOTS_INSTANCED_PROP(float, _DissolveBorderSize)
		#define _OutlineNoise MK_SET_DOTS_INSTANCED_PROP(float, _OutlineNoise)
		#define _DiffuseWrap MK_SET_DOTS_INSTANCED_PROP(float, _DiffuseWrap)
		#define _DetailMix MK_SET_DOTS_INSTANCED_PROP(float, _DetailMix)
		#define _RefractionDistortionFade MK_SET_DOTS_INSTANCED_PROP(float, _RefractionDistortionFade)
		#define _GoochRampIntensity MK_SET_DOTS_INSTANCED_PROP(float, _GoochRampIntensity)
		#define _VertexAnimationIntensity MK_SET_DOTS_INSTANCED_PROP(float, _VertexAnimationIntensity)
		#define _IndirectFade MK_SET_DOTS_INSTANCED_PROP(float, _IndirectFade)
		#define _DetailNormalMapIntensity MK_SET_DOTS_INSTANCED_PROP(float, _DetailNormalMapIntensity)
		#define _NormalMapIntensity MK_SET_DOTS_INSTANCED_PROP(float, _NormalMapIntensity)
		#define _Parallax MK_SET_DOTS_INSTANCED_PROP(float, _Parallax)
		#define _OcclusionMapIntensity MK_SET_DOTS_INSTANCED_PROP(float, _OcclusionMapIntensity)
		#define _LightBands MK_SET_DOTS_INSTANCED_PROP(float, _LightBands)
		#define _ThresholdMapScale MK_SET_DOTS_INSTANCED_PROP(float, _ThresholdMapScale)
		#define _ArtisticFrequency MK_SET_DOTS_INSTANCED_PROP(float, _ArtisticFrequency)
		#define _DissolveMapScale MK_SET_DOTS_INSTANCED_PROP(float, _DissolveMapScale)
		#define _DrawnMapScale MK_SET_DOTS_INSTANCED_PROP(float, _DrawnMapScale)
		#define _SketchMapScale MK_SET_DOTS_INSTANCED_PROP(float, _SketchMapScale)
		#define _HatchingMapScale MK_SET_DOTS_INSTANCED_PROP(float, _HatchingMapScale)
		#define _OutlineSize MK_SET_DOTS_INSTANCED_PROP(float, _OutlineSize)
		#define _SpecularIntensity MK_SET_DOTS_INSTANCED_PROP(float, _SpecularIntensity)
		#define _LightTransmissionIntensity MK_SET_DOTS_INSTANCED_PROP(float, _LightTransmissionIntensity)
		#define _RefractionDistortionMapScale MK_SET_DOTS_INSTANCED_PROP(float, _RefractionDistortionMapScale)
		#define _IndexOfRefraction MK_SET_DOTS_INSTANCED_PROP(float, _IndexOfRefraction)
		#define _RefractionDistortion MK_SET_DOTS_INSTANCED_PROP(float, _RefractionDistortion)
	#endif

	UNIFORM_SAMPLER_2D(_AlbedoMap);					//1
	UNIFORM_SAMPLER_2D(_AlbedoMap1);					
	UNIFORM_SAMPLER_2D(_AlbedoMap2);					
	UNIFORM_SAMPLER_2D(_AlbedoMap3);					
	UNIFORM_TEXTURE_2D(_RefractionDistortionMap);	//2
	UNIFORM_TEXTURE_2D(_SpecularMap);				//3
	UNIFORM_TEXTURE_2D(_RoughnessMap);				//3
	UNIFORM_TEXTURE_2D(_MetallicMap);				//3
	UNIFORM_TEXTURE_2D(_DetailMap);					//4
	UNIFORM_TEXTURE_2D(_DetailNormalMap);			//5
	UNIFORM_TEXTURE_2D(_NormalMap);					//6
	UNIFORM_TEXTURE_2D(_HeightMap);					//7
	UNIFORM_TEXTURE_2D(_ThicknessMap);				//8
	UNIFORM_TEXTURE_2D(_OcclusionMap);				//9
	UNIFORM_TEXTURE_2D(_ThresholdMap);				//10
	UNIFORM_TEXTURE_2D(_GoochRamp);					//11
	UNIFORM_TEXTURE_2D(_DiffuseRamp);				//12
	UNIFORM_TEXTURE_2D(_SpecularRamp);				//13
	UNIFORM_TEXTURE_2D(_RimRamp);					//14
	UNIFORM_TEXTURE_2D(_LightTransmissionRamp);		//15
	UNIFORM_TEXTURE_2D(_IridescenceRamp);			//16
	UNIFORM_TEXTURE_2D(_SketchMap);					//17
	UNIFORM_TEXTURE_2D(_DrawnMap);					//17
	UNIFORM_TEXTURE_2D(_HatchingBrightMap);			//17
	UNIFORM_TEXTURE_2D(_HatchingDarkMap);			//18
	UNIFORM_TEXTURE_2D(_GoochBrightMap);			//19
	UNIFORM_TEXTURE_2D(_GoochDarkMap);				//20
	UNIFORM_TEXTURE_2D(_DissolveMap);				//21
	UNIFORM_TEXTURE_2D(_DissolveBorderRamp);		//22
	UNIFORM_TEXTURE_2D(_EmissionMap);				//23
	uniform sampler2D _VertexAnimationMap;			//24
	//Depth											//25
	//Refraction									//26
	uniform sampler2D _OutlineMap;					// Only Outline
	uniform sampler2D _NoiseMap;					// Only Vertex
	uniform sampler3D _DitherMaskLOD;				// Only Shadows
#endif