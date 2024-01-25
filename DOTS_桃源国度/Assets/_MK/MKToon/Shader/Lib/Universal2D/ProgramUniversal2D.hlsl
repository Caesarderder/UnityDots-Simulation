//////////////////////////////////////////////////////
// MK Toon Universal2D Program			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_UNIVERSAL2D
	#define MK_TOON_UNIVERSAL2D
	
	#include "../Core.hlsl"
	#include "Data.hlsl"
	#include "../Surface.hlsl"
	#include "../Composite.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	VertexOutputUniversal2D Universal2DVert(VertexInputUniversal2D vertexInput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexInput);
		VertexOutputUniversal2D vertexOutput;
		INITIALIZE_STRUCT(VertexOutputUniversal2D, vertexOutput);
		UNITY_TRANSFER_INSTANCE_ID(vertexInput, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);
		
		#ifdef MK_VERTEX_ANIMATION
			vertexInput.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(vertexInput.texcoord0.xy), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, vertexInput.vertex.xyz, vertexInput.normal));
		#endif

		vertexOutput.svPositionClip = mul(MATRIX_MVP, float4(vertexInput.vertex.xyz, 1.0));

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = vertexInput.color;
		#endif

		//texcoords
		#if defined(MK_TCM)
			vertexOutput.uv = vertexInput.texcoord0 * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
		#endif
		return vertexOutput;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	half4 Universal2DFrag(VertexOutputUniversal2D vertexOutput) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutput.svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(0)
			PASS_BASE_UV_ARG(float4(vertexOutput.uv.xy, 0, 0))
			PASS_LIGHTMAP_UV_ARG(0)
			PASS_VERTEX_COLOR_ARG(vertexOutput.color)
			PASS_NORMAL_WORLD_ARG(1)
			PASS_VERTEX_LIGHTING_ARG(0)
			PASS_TANGENT_WORLD_ARG(1)
			PASS_VIEW_TANGENT_ARG(1)
			PASS_BITANGENT_WORLD_ARG(1)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(0)
			PASS_NULL_CLIP_ARG(0)
			PASS_FLIPBOOK_UV_ARG(0)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, vertexOutput.svPositionClip);
		MKPBSData pbsData = ComputePBSData(surface, surfaceData);
		Composite(surface, surfaceData, pbsData);

    	return half4(surface.final.rgb, surface.alpha);
	}
#endif