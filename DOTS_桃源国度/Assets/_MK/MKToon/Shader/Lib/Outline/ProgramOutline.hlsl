//////////////////////////////////////////////////////
// MK Toon Outline Program			       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_OUTLINE_ONLY_BASE
	#define MK_TOON_OUTLINE_ONLY_BASE
	
	#include "../Core.hlsl"
	#include "Data.hlsl"
	#include "../Surface.hlsl"
	#include "../Composite.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	VertexOutputOutlineOnly OutlineVert(VertexInputOutlineOnly vertexInput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexInput);
		VertexOutputOutlineOnly vertexOutput;
		INITIALIZE_STRUCT(VertexOutputOutlineOnly, vertexOutput);
		UNITY_TRANSFER_INSTANCE_ID(vertexInput, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

		half outlineSize = _OutlineSize;
		
		//texcoords
		#if defined(MK_TCM)
			vertexOutput.uv = vertexInput.texcoord0;
		#endif

		#ifdef MK_OUTLINE_MAP
			outlineSize *= tex2Dlod(_OutlineMap, float4(vertexOutput.uv, 0, 0)).r;
		#endif

		#ifdef MK_OUTLINE_NOISE
			outlineSize = lerp(outlineSize, outlineSize * NoiseSimple(normalize(vertexInput.vertex.xyz), vertexInput.normal.xz), _OutlineNoise);
		#endif

		#ifdef MK_VERTEX_ANIMATION
			vertexInput.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(vertexOutput.uv), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, vertexInput.vertex.xyz, vertexInput.normal));
		#endif

		#ifndef MK_LEGACY_SCREEN_SCALING
			#if defined(MK_MULTI_PASS_STEREO_SCALING) || defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				outlineSize *= 0.5;
			#endif
		#endif

		#ifdef MK_OUTLINE_FADING
			float dist = distance(CAMERA_POSITION_WORLD , mul(MATRIX_M, float4(vertexInput.vertex.xyz, 1.0)).xyz);
			#if defined(MK_OUTLINE_FADING_EXPONENTIAL)
				outlineSize *= smoothstep(_OutlineFadeMin, _OutlineFadeMax, dist);
			#elif defined(MK_OUTLINE_FADING_INVERSE_EXPONENTIAL)
				float interp = saturate((dist - _OutlineFadeMin) / (_OutlineFadeMax - _OutlineFadeMin));
				outlineSize *= (interp + (interp - (interp * interp * (3.0f - 2.0f * interp))));
			#else
				outlineSize *= saturate((dist - _OutlineFadeMin) / (_OutlineFadeMax - _OutlineFadeMin));
			#endif
		#endif

		vertexInput.normal = MKSafeNormalize(vertexInput.normal);

		#if defined(MK_OUTLINE_HULL_ORIGIN)
			//float4x4 modelMatrix = MATRIX_M;
			//vertexInput.vertex.xyz += MKSafeNormalize(vertexInput.vertex.xyz) * _OutlineSize * OUTLINE_ORIGIN_SCALE;
			//float3 positionWorld = mul(modelMatrix, float4(vertexInput.vertex.xyz, 1.0)).xyz;
			float3 scaleOrigin = 1 + _OutlineSize * OUTLINE_ORIGIN_SCALE;
			float3x3 scale = float3x3
			(
			 scaleOrigin.x, 0, 0,
			 0, scaleOrigin.y, 0,
			 0, 0, scaleOrigin.z
			);
			float3 positionWorld = mul(scale, vertexInput.vertex.xyz);
			positionWorld = mul(MATRIX_M, float4(positionWorld, 1.0)).xyz;
		#elif defined(MK_OUTLINE_HULL_OBJECT)
			#if defined(MK_OUTLINE_DATA_UV7)
				vertexInput.vertex.xyz += vertexInput.normalBaked * outlineSize * OUTLINE_OBJECT_SCALE;
			#else
				vertexInput.vertex.xyz += vertexInput.normal * outlineSize * OUTLINE_OBJECT_SCALE;
			#endif
		#endif

		#if defined(MK_OUTLINE_HULL_ORIGIN)
			vertexOutput.svPositionClip = ComputeWorldToClipSpace(positionWorld);
		#elif defined(MK_OUTLINE_HULL_CLIP)
			//Make it pixel perfect and SCALED on different aspects and resolutions
			vertexOutput.svPositionClip = ComputeObjectToClipSpace(vertexInput.vertex.xyz);

			#ifndef MK_LEGACY_SCREEN_SCALING
				half oScale;
				oScale = ScaleToFitOrthograpicSize(vertexOutput.svPositionClip.w);
				half scale = ScaleToFitResolution(REFERENCE_ASPECT, REFERENCE_RESOLUTION, _ScreenParams.xy);
			#else
				half oScale = 1;
				half scaledAspect = SafeDivide(REFERENCE_ASPECT.x, SafeDivide(_ScreenParams.x, _ScreenParams.y));
				half scaledResolution = SafeDivide(_ScreenParams.x, REFERENCE_RESOLUTION.x);
				half scale = scaledAspect * scaledResolution;
			#endif

			#if defined(MK_OUTLINE_DATA_UV7)
				half3 normalBakedClip = ComputeNormalObjectToClipSpace(vertexInput.normalBaked.xyz);
				vertexOutput.svPositionClip.xy += 2 * oScale * outlineSize * SafeDivide(normalBakedClip.xy, _ScreenParams.xy) * scale;
			#else
				half3 normalClip = ComputeNormalObjectToClipSpace(vertexInput.normal.xyz);
				vertexOutput.svPositionClip.xy += 2 * oScale * outlineSize * SafeDivide(normalClip.xy, _ScreenParams.xy) * scale;
			#endif
		#else
			vertexOutput.svPositionClip = ComputeObjectToClipSpace(vertexInput.vertex.xyz);
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = vertexInput.color;
		#endif

		#if defined(MK_PARALLAX)
			vertexOutput.viewTangent = ComputeViewTangent(ComputeViewObject(vertexInput.vertex.xyz), vertexInput.normal, vertexInput.tangent.xyz, cross(vertexInput.normal, vertexInput.tangent.xyz) * vertexInput.tangent.w * unity_WorldTransformParams.w);
		#endif

		#ifdef MK_FOG
			vertexOutput.fogFactor = FogFactorVertex(vertexOutput.svPositionClip.z);
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			vertexOutput.positionClip = vertexOutput.svPositionClip;
		#endif
		#ifdef MK_POS_NULL_CLIP
			vertexOutput.nullClip = ComputeObjectToClipSpace(0);
		#endif

		#ifdef MK_FLIPBOOK
			vertexOutput.flipbookUV.xy = VERTEX_INPUT.texcoord0.zw;
			vertexOutput.flipbookUV.z = VERTEX_INPUT.texcoordBlend;
		#endif
		return vertexOutput;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	MKFragmentOutput OutlineFrag(VertexOutputOutlineOnly vertexOutput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		MKFragmentOutput mkFragmentOutput;
		INITIALIZE_STRUCT(MKFragmentOutput, mkFragmentOutput);

		#ifdef MK_LOD_FADE_CROSSFADE
			LODFadeCrossFade(vertexOutput.SV_CLIP_POS);
		#endif

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutput.svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(vertexOutput.fogFactor)
			PASS_BASE_UV_ARG(float4(vertexOutput.uv.xy, 0, 0))
			PASS_LIGHTMAP_UV_ARG(0)
			PASS_VERTEX_COLOR_ARG(vertexOutput.color)
			PASS_NORMAL_WORLD_ARG(1)
			PASS_VERTEX_LIGHTING_ARG(0)
			PASS_TANGENT_WORLD_ARG(1)
			PASS_VIEW_TANGENT_ARG(vertexOutput.viewTangent)
			PASS_BITANGENT_WORLD_ARG(1)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(vertexOutput.positionClip)
			PASS_NULL_CLIP_ARG(vertexOutput.nullClip)
			PASS_FLIPBOOK_UV_ARG(vertexOutput.flipbookUV)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), half4(_OutlineColor.rgb, _AlbedoColor.a), vertexOutput.svPositionClip);
		MKPBSData pbsData = ComputePBSData(surface, surfaceData);
		Composite(surface, surfaceData, pbsData);

		mkFragmentOutput.svTarget0 = surface.final;
		#ifdef MK_WRITE_RENDERING_LAYERS
			uint renderingLayers = GetMeshRenderingLayer();
			mkFragmentOutput.svTarget1 = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
		#endif

		return mkFragmentOutput;
	}
#endif