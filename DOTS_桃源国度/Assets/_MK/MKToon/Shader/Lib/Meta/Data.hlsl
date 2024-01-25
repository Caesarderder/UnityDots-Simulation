//////////////////////////////////////////////////////
// MK Toon Meta Data				       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_META_IO
	#define MK_TOON_META_IO

	#ifndef UNITY_PASS_META
		#define UNITY_PASS_META
	#endif

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
	#else
		#include "UnityMetaPass.cginc"
	#endif

	#include "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputMeta
	{
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR;
		#endif
		float4 vertex : POSITION;
		float2 texcoord0 : TEXCOORD0;
		float2 staticLightmapUV : TEXCOORD1;
		#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
			float2 dynamicLightmapUV : TEXCOORD2;
		#endif
		#if defined(MK_PARALLAX)
			half4 tangent : TANGENT;
			half3 normal : NORMAL;
		#endif
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexOutputMeta
	{
		#if defined(MK_TCM) || defined(MK_TCD)
			float4 uv : TEXCOORD0;
		#endif
		float4 svPositionClip : SV_POSITION;
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR;
		#endif
		#ifdef EDITOR_VISUALIZATION
			float2 vizUV : TEXCOORD1;
			float4 lightCoords : TEXCOORD2;
		#endif
		#if defined(MK_PARALLAX)
			half3 viewTangent : TEXCOORD3;
		#endif
	};

#endif