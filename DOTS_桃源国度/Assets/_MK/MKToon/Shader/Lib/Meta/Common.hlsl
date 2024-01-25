//////////////////////////////////////////////////////
// MK Toon Meta Common				       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_META_COMMON
	#define MK_TOON_META_COMMON

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
	#else
		#include "UnityMetaPass.cginc"
	#endif

	#include "../Core.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Meta Common
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct MKMetaData
	{
		half3 albedo;
		half3 emission;
		half3 specular;
		#ifdef EDITOR_VISUALIZATION
			float2 vizUV;
			float4 lightCoords;
		#endif
	};

	inline float4 ComputeMetaPosition(float4 vertexPosObject, float2 staticLightmapUV, float2 dynamicLightmapUV, float4 staticLightmapST, float4 dynLightmapST)
	{
		#if defined(MK_URP)
			return MetaVertexPosition(vertexPosObject, staticLightmapUV, dynamicLightmapUV, staticLightmapST, dynLightmapST);
		#elif defined(MK_LWRP)
			return MetaVertexPosition(vertexPosObject, staticLightmapUV, dynamicLightmapUV, staticLightmapST);
		#else
			return UnityMetaVertexPosition(vertexPosObject, staticLightmapUV, dynamicLightmapUV, staticLightmapST, dynLightmapST);
		#endif
	}
	
	inline half4 ComputeMetaOutput(MKMetaData mkMetaData)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			MetaInput metaInput;

			metaInput.Emission = mkMetaData.emission;
			#if UNITY_VERSION >= 202120
				metaInput.Albedo = mkMetaData.albedo + mkMetaData.specular;
			#else
				metaInput.Albedo = mkMetaData.albedo;
				metaInput.SpecularColor = mkMetaData.specular;
			#endif

			return MetaFragment(metaInput);
		#else
			UnityMetaInput untiyMetaInput;

			untiyMetaInput.Albedo = mkMetaData.albedo;
			untiyMetaInput.Emission = mkMetaData.emission;
			untiyMetaInput.SpecularColor = mkMetaData.specular;
			#ifdef EDITOR_VISUALIZATION
				untiyMetaInput.VizUV = mkMetaData.vizUV;
				untiyMetaInput.LightCoord = mkMetaData.lightCoords;
			#endif

			return UnityMetaFragment(untiyMetaInput);
		#endif
	}
#endif