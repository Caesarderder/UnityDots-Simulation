//////////////////////////////////////////////////////
// MK Toon Depth Normals Program			       	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_DEPTH_NORMALS
	#define MK_TOON_DEPTH_NORMALS
	
	#include "../Core.hlsl"
	#include "Data.hlsl"
	#include "../Surface.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	VertexOutputDepthNormals DepthNormalsVert(VertexInputDepthNormals vertexInput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexInput);
		VertexOutputDepthNormals vertexOutput;
		INITIALIZE_STRUCT(VertexOutputDepthNormals, vertexOutput);
		UNITY_TRANSFER_INSTANCE_ID(vertexInput, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

		#ifdef MK_VERTEX_ANIMATION
			vertexInput.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(vertexInput.texcoord0.xy), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, vertexInput.vertex.xyz, vertexInput.normal));
		#endif

		vertexOutput.svPositionClip = ComputeObjectToClipSpace(vertexInput.vertex.xyz);

		//texcoords
		#if defined(MK_TCM)
			vertexOutput.uv = vertexInput.texcoord0.xy;
		#endif

		vertexOutput.normalWorld.xyz = ComputeNormalWorld(vertexInput.normal);
		#ifdef MK_LIT
			#if defined(MK_TBN)
				vertexOutput.tangentWorld.xyz = ComputeTangentWorld(vertexInput.tangent.xyz);
            	vertexOutput.bitangentWorld.xyz = ComputeBitangentWorld(vertexOutput.normalWorld.xyz, vertexOutput.tangentWorld.xyz, vertexInput.tangent.w * unity_WorldTransformParams.w);
			#endif
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

		return vertexOutput;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	MKFragmentOutput DepthNormalsFrag(VertexOutputDepthNormals vertexOutput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		MKFragmentOutput mkFragmentOutput;
		INITIALIZE_STRUCT(MKFragmentOutput, mkFragmentOutput);

		#ifdef MK_LOD_FADE_CROSSFADE
			LODFadeCrossFade(vertexOutput.svPositionClip);
		#endif

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutput.svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(0)
			PASS_BASE_UV_ARG(float4(vertexOutput.uv, 0, 0))
			PASS_LIGHTMAP_UV_ARG(0)
			PASS_VERTEX_COLOR_ARG(1)
			PASS_NORMAL_WORLD_ARG(vertexOutput.normalWorld.xyz)
			PASS_VERTEX_LIGHTING_ARG(0)
			PASS_TANGENT_WORLD_ARG(vertexOutput.tangentWorld.xyz)
			PASS_VIEW_TANGENT_ARG(vertexOutput.viewTangent)
			PASS_BITANGENT_WORLD_ARG(vertexOutput.bitangentWorld.xyz)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(vertexOutput.positionClip)
			PASS_NULL_CLIP_ARG(vertexOutput.nullClip)
			PASS_FLIPBOOK_UV_ARG(0)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, vertexOutput.svPositionClip);

		//For depth normals pass the MK_SIMPLE directive is currently always set even on unlit shaders to guarantee the normals available
		#if UNITY_VERSION >= 202120
			#if defined(_GBUFFER_NORMALS_OCT)
				float2 octNormalWS = PackNormalOctQuadEncode(surfaceData.normalWorld);
				float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
				half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
				mkFragmentOutput.svTarget0 = half4(packedNormalWS, 0.0);
			#else
				mkFragmentOutput.svTarget0 = half4(surfaceData.normalWorld, 0.0);
			#endif
		#else
			mkFragmentOutput.svTarget0 = half4(PackNormalOctRectEncode(MKSafeNormalize(mul((half3x3) MATRIX_V, surfaceData.normalWorld).xyz)), 0.0, 0.0);
		#endif

		#ifdef MK_WRITE_RENDERING_LAYERS
			uint renderingLayers = GetMeshRenderingLayer();
			mkFragmentOutput.svTarget1 = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
		#endif

		return mkFragmentOutput;
	}
#endif