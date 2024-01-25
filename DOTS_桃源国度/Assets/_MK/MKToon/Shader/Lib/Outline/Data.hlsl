//////////////////////////////////////////////////////
// MK Toon Outline Data				       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_OUTLINE_ONLY_IO
	#define MK_TOON_OUTLINE_ONLY_IO

	#include "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	#if defined(MK_OUTLINE_DATA_UV7)
		#define OUTLINE_INPUT half3 normalBaked : TEXCOORD7;
	#else
		#define OUTLINE_INPUT
	#endif

	struct VertexInputOutlineOnly
	{
		float4 vertex : POSITION;
		half3 normal : NORMAL;

		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			//texcoords0 if needed
			float2 texcoord0 : TEXCOORD0;
		#endif
		
		#if defined(MK_PARALLAX)
			half4 tangent : TANGENT;
		#endif

		OUTLINE_INPUT
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexOutputOutlineOnly
	{
		float4 svPositionClip : SV_POSITION;
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			float2 uv : TEXCOORD0;
		#endif
		#ifdef MK_FOG
			float fogFactor : TEXCOORD1;
		#endif

		#if defined(MK_PARALLAX)
			half3 viewTangent : TEXCOORD2;
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			float4 positionClip : TEXCOORD3;
		#endif
		#ifdef MK_POS_NULL_CLIP
			float4 nullClip : TEXCOORD4;
		#endif

		#ifdef MK_FLIPBOOK
			float3 flipbookUV : TEXCOORD10;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
#endif