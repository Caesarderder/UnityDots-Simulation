//////////////////////////////////////////////////////
// MK Toon Forward Data				       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_IO_FORWARD
	#define MK_TOON_IO_FORWARD
	
	#include "../Core.hlsl"
	#include "../Lighting.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// INPUT
	/////////////////////////////////////////////////////////////////////////////////////////////
	struct VertexInputForward
	{
		#ifdef MK_VERTEX_COLOR_REQUIRED
			//use vertexcolors if required
			half4 color : COLOR0;
		#endif
		//vertex position - always needed
		float4 vertex : POSITION;
		#if defined(MK_TCM) || defined(MK_TCD)
			//texcoords0 if needed
			#if defined(MK_FLIPBOOK)// && !defined(UNITY_PARTICLE_INSTANCING_ENABLED)
				float4 texcoord0 : TEXCOORD0;
				float texcoordBlend : TEXCOORD3;
			#else
				#ifdef MK_OCCLUSION_UV_SECOND
					float4 texcoord0 : TEXCOORD0;
				#else
					float2 texcoord0 : TEXCOORD0;
				#endif
			#endif
		#endif
		#if defined(MK_FORWARD_BASE_PASS) && defined(DYNAMICLIGHTMAP_ON)
			//dynammic lightmap uv
			DECLARE_DYNAMIC_LIGHTMAP_INPUT(2)
		#endif
		#ifdef MK_NORMAL
			half3 normal : NORMAL;
		#endif
		#ifdef MK_LIT
			//static lightmap uv
			DECLARE_STATIC_LIGHTMAP_INPUT(1);
			
			//use tangents only if tbn matrix is required
			#if defined(MK_TBN)
				half4 tangent : TANGENT;
			#endif
		#endif
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// OUTPUT
	/////////////////////////////////////////////////////////////////////////////////////////////

	//If no parallax is used, then a 3 component tbn is enough
	#ifdef MK_PARALLAX
		#ifndef TBN_TYPE
			#define TBN_TYPE half4
		#endif
	#else
		#ifndef TBN_TYPE
			#define TBN_TYPE half3
		#endif
	#endif

	//Output Setup
	struct VertexOutputForward
	{
		#if defined(MK_TCM) || defined(MK_TCD)
			float4 uv : TEXCOORD0;
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 color : COLOR0;
		#endif
	
		//WorldPos + Fog
		#if defined(MK_POS_WORLD) || defined(MK_FOG)
			float4 positionWorld : TEXCOORD1; //posWorld XYZ Fog W
		#endif

		#ifdef MK_NORMAL
			TBN_TYPE normalWorld : TEXCOORD2;
		#endif
		#ifdef MK_LIT
			#if defined(MK_TBN)
				TBN_TYPE tangentWorld : TEXCOORD8;
				TBN_TYPE bitangentWorld : TEXCOORD9;
			#endif

			//Interplators 5,6,7 + C1 are reserved for lighting stuff
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