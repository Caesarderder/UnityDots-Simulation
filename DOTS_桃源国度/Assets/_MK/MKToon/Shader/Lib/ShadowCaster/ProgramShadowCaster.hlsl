//////////////////////////////////////////////////////
// MK Toon ShadowCaster Program			       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_SHADOWCASTER
	#define MK_TOON_SHADOWCASTER

	#include "../Core.hlsl"

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Shadows.hlsl"
	#endif

	#include "../Surface.hlsl"
	#include "Data.hlsl"

	//This should be excluded from the CBuffer
	uniform float3 _LightDirection;
	uniform float3 _LightPosition;

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	void ShadowCasterVert 
	(
		VertexInputShadowCaster VERTEX_INPUT,
		out VertexOutputShadowCaster vertexOutput
		,out float4 svPositionClip : SV_POSITION
	)
	{
		UNITY_SETUP_INSTANCE_ID(VERTEX_INPUT);
		INITIALIZE_STRUCT(VertexOutputShadowCaster, vertexOutput);
		UNITY_TRANSFER_INSTANCE_ID(VERTEX_INPUT, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

		#ifdef MK_VERTEX_ANIMATION
			VERTEX_INPUT.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(VERTEX_INPUT.texcoord0.xy), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, VERTEX_INPUT.vertex.xyz, VERTEX_INPUT.normal));
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = VERTEX_INPUT.color;
		#endif

		#if defined(MK_TCM)
			vertexOutput.uv = VERTEX_INPUT.texcoord0.xy * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
		#endif

		#if defined(MK_URP) || defined(MK_LWRP)
			half3 normalWorld = ComputeNormalWorld(VERTEX_INPUT.normal);
		#endif
		#ifdef MK_PARALLAX
			vertexOutput.viewTangent = ComputeViewTangent(ComputeViewObject(VERTEX_INPUT.vertex.xyz), VERTEX_INPUT.normal, VERTEX_INPUT.tangent.xyz, cross(VERTEX_INPUT.normal, VERTEX_INPUT.tangent.xyz) * VERTEX_INPUT.tangent.w * unity_WorldTransformParams.w);
		#endif

		#if defined(MK_URP)
			float3 positionWorld = mul(MATRIX_M, float4(VERTEX_INPUT.vertex.xyz, 1.0)).xyz;
			half3 lightDirection;
			#if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW) && defined(MK_URP_2020_2_Or_Newer)
				lightDirection = MKSafeNormalize(_LightPosition - positionWorld);
			#else
				lightDirection = _LightDirection;
			#endif
			svPositionClip = mul(MATRIX_VP, float4(ApplyShadowBias(positionWorld, normalWorld, lightDirection), 1.0));
			#if UNITY_REVERSED_Z
				svPositionClip.z = min(svPositionClip.z, svPositionClip.w * UNITY_NEAR_CLIP_VALUE);
			#else
				svPositionClip.z = max(svPositionClip.z, svPositionClip.w * UNITY_NEAR_CLIP_VALUE);
			#endif
		#elif defined(MK_LWRP)
			float3 positionWorld = mul(MATRIX_M, float4(VERTEX_INPUT.vertex.xyz, 1.0)).xyz;
			float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWorld));
			float scale = invNdotL * _ShadowBias.y;

			positionWorld = _LightDirection * _ShadowBias.xxx + positionWorld;
			positionWorld = normalWorld * scale.xxx + positionWorld;
			svPositionClip = mul(MATRIX_VP, float4(positionWorld, 1));
			#if UNITY_REVERSED_Z
				svPositionClip.z = min(svPositionClip.z, UNITY_NEAR_CLIP_VALUE);
			#else
				svPositionClip.z = max(svPositionClip.z, UNITY_NEAR_CLIP_VALUE);
			#endif
		#else
			TRANSFER_SHADOW_CASTER_NOPOS(vertexOutput, svPositionClip)
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			vertexOutput.positionClip = svPositionClip;
		#endif
		#ifdef MK_POS_NULL_CLIP
			vertexOutput.nullClip = ComputeObjectToClipSpace(0);
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	half4 ShadowCasterFrag 
		(
			VertexOutputShadowCaster vertexOutput,
			float4 svPositionClip : SV_POSITION
		) : SV_Target
	{	
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		#ifdef MK_LOD_FADE_CROSSFADE
			LODFadeCrossFade(vertexOutput.positionClip);
		#endif
		
		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(0)
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
			PASS_FLIPBOOK_UV_ARG(0)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, float4(0,0,0,1));
		
		#if defined(MK_URP) || defined(MK_LWRP)
			return 0;
		#else
			#ifdef MK_SURFACE_TYPE_TRANSPARENT
				#ifdef MK_TOON_DITHER_MASK
					/*
					#ifdef LOD_FADE_CROSSFADE
						#define _LOD_FADE_ON_ALPHA
						alpha *= unity_LODFade.y;
					#endif
					*/
					
					// dither mask alpha blending
					half alphaRef = tex3D(_DitherMaskLOD, float3(svPositionClip.xy*0.25,surface.alpha*0.9375)).a;
					clip(alphaRef - 0.01);
				#else
					clip(surface.alpha - 0.5);
				#endif

				/*
				//Disabled for now
				#ifdef LOD_FADE_CROSSFADE
					#ifdef _LOD_FADE_ON_ALPHA
						#undef _LOD_FADE_ON_ALPHA
					#else
						UnityApplyDitherCrossFade(vpos.xy);
					#endif
				#endif
				*/
			#endif
			SHADOW_CASTER_FRAGMENT(vertexOutput)
		#endif
	}			
#endif