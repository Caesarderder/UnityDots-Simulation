//////////////////////////////////////////////////////
// MK Toon Universal2D Data				       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_UNIVERSAL2D_IO
	#define MK_TOON_UNIVERSAL2D_IO

	#include "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputUniversal2D
	{
		float4 vertex : POSITION;
		#if defined(MK_VERTEX_ANIMATION_PULSE) || defined(MK_VERTEX_ANIMATION_NOISE)
			half3 normal : NORMAL;
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
	struct VertexOutputUniversal2D
	{
		float4 svPositionClip : SV_POSITION;
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			float2 uv : TEXCOORD0;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
#endif