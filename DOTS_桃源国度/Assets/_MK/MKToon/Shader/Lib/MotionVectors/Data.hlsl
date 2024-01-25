//////////////////////////////////////////////////////
// MK Toon Motion Vectors Data				       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_MOTION_VECTORS_IO
	#define MK_TOON_MOTION_VECTORS_IO

	#include "../Core.hlsl"
	
	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputMotionVectors
	{
		float4 vertex : POSITION;
		#if defined(MK_VERTEX_ANIMATION_PULSE) || defined(MK_VERTEX_ANIMATION_NOISE) || defined(MK_PARALLAX)
			half3 normal : NORMAL;
		#endif
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			float2 texcoord0 : TEXCOORD0;
		#endif
		#if defined(MK_PARALLAX)
			half4 tangent : TANGENT;
		#endif
		float3 positionOld : TEXCOORD4;
		#if _ADD_PRECOMPUTED_VELOCITY
			float3 alembicMotionVector  : TEXCOORD5;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexOutputMotionVectors
	{
		float4 svPositionClip : SV_POSITION;
		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
		#ifdef MK_TCM
			float2 uv : TEXCOORD0;
		#endif

		#if defined(MK_PARALLAX)
			half3 viewTangent : TEXCOORD1;
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			float4 positionClip : TEXCOORD2;
		#endif
		#ifdef MK_POS_NULL_CLIP
			float4 nullClip : TEXCOORD3;
		#endif
		float4 positionCSNoJitter : POSITION_CS_NO_JITTER;
    	float4 previousPositionCSNoJitter : PREV_POSITION_CS_NO_JITTER;

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
#endif