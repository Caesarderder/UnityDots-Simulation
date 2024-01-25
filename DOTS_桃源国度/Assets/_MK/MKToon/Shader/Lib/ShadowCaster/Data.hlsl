//////////////////////////////////////////////////////
// MK Toon ShadowCaster Data			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_SHADOWCASTER_IO
	#define MK_TOON_SHADOWCASTER_IO

	#include "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputShadowCaster
	{
		float4 vertex : POSITION;
		half3 normal : NORMAL;
		#ifdef MK_PARALLAX
			half4 tangent : TANGENT;
		#endif
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			float2 texcoord0 : TEXCOORD0;
		#endif
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexOutputShadowCaster
	{	
		#ifdef MK_LEGACY_RP
			V2F_SHADOW_CASTER_NOPOS
		#endif
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_PARALLAX
			half3 viewTangent : TEXCOORD6;
		#endif
		#ifdef MK_TCM
			float2 uv : TEXCOORD7;
		#endif
		#ifdef MK_BARYCENTRIC_POS_CLIP
			float4 positionClip : TEXCOORD2;
		#endif
		#ifdef MK_POS_NULL_CLIP
			float4 nullClip : TEXCOORD3;
		#endif
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
#endif