//////////////////////////////////////////////////////
// MK Toon Motion Vectors Program			       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_MOTION_VECTORS
	#define MK_TOON_MOTION_VECTORS
	
	#include "../Core.hlsl"
	#include "Data.hlsl"
	#include "../Surface.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	VertexOutputMotionVectors MotionVectorsVert(VertexInputMotionVectors vertexInput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexInput);
		VertexOutputMotionVectors vertexOutput;
		INITIALIZE_STRUCT(VertexOutputMotionVectors, vertexOutput);
		UNITY_TRANSFER_INSTANCE_ID(vertexInput, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

		#ifdef MK_VERTEX_ANIMATION
			vertexInput.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(vertexInput.texcoord0.xy), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, vertexInput.vertex.xyz, vertexInput.normal));
		#endif

		vertexOutput.svPositionClip = ComputeObjectToClipSpace(vertexInput.vertex.xyz);

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = vertexInput.color;
		#endif

		//texcoords
		#if defined(MK_TCM)
			vertexOutput.uv = vertexInput.texcoord0.xy;
		#endif

		#if defined(MK_PARALLAX)
			vertexOutput.viewTangent = ComputeViewTangent(ComputeViewObject(vertexInput.vertex.xyz), vertexInput.normal, vertexInput.tangent.xyz, cross(vertexInput.normal, vertexInput.tangent.xyz) * vertexInput.tangent.w * unity_WorldTransformParams.w);
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			vertexOutput.positionClip = vertexOutput.svPositionClip;
		#endif
		#ifdef MK_POS_NULL_CLIP
			vertexOutput.nullClip = ComputeObjectToClipSpace(0);
		#endif

    	vertexOutput.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(MATRIX_M, vertexInput.vertex));
		float4 prevPos = (unity_MotionVectorsParams.x == 1) ? float4(vertexInput.positionOld, 1) : vertexInput.vertex;
		#if _ADD_PRECOMPUTED_VELOCITY
			prevPos = prevPos - float4(vertexInput.alembicMotionVector, 0);
		#endif
    	vertexOutput.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));
    	ApplyMotionVectorZBias(vertexOutput.svPositionClip);

		return vertexOutput;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	half4 MotionVectorsFrag(VertexOutputMotionVectors vertexOutput) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutput.svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(0)
			PASS_BASE_UV_ARG(float4(vertexOutput.uv, 0, 0))
			PASS_LIGHTMAP_UV_ARG(0)
			PASS_VERTEX_COLOR_ARG(vertexOutput.color)
			PASS_NORMAL_WORLD_ARG(1)
			PASS_VERTEX_LIGHTING_ARG(0)
			PASS_TANGENT_WORLD_ARG(1)
			PASS_VIEW_TANGENT_ARG(vertexOutput.viewTangent)
			PASS_BITANGENT_WORLD_ARG(1)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(vertexOutput.positionClip)
			PASS_NULL_CLIP_ARG(vertexOutput.nullClip)
			PASS_FLIPBOOK_UV_ARG(0)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, vertexOutput.svPositionClip);

		#ifdef MK_LOD_FADE_CROSSFADE
			LODFadeCrossFade(vertexOutput.svPositionClip);
		#endif

    	return float4(CalcNdcMotionVectorFromCsPositions(vertexOutput.positionCSNoJitter, vertexOutput.previousPositionCSNoJitter), 0, 0);
	}
#endif