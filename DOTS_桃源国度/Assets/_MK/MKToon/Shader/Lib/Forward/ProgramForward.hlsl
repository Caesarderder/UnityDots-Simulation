//////////////////////////////////////////////////////
// MK Toon Forward Program			       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_FORWARD
	#define MK_TOON_FORWARD
	
	#include "../Forward/Data.hlsl"
	#include "../Surface.hlsl"
	#include "../Lighting.hlsl"
	#include "../Composite.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	void ForwardVert (VertexInputForward VERTEX_INPUT, out VertexOutputLight vertexOutputLight, out VertexOutputForward vertexOutput MK_DECLARE_V_FACE)
	{
		UNITY_SETUP_INSTANCE_ID(VERTEX_INPUT);
		INITIALIZE_STRUCT(VertexOutputForward, vertexOutput);
		INITIALIZE_STRUCT(VertexOutputLight, vertexOutputLight);
		UNITY_TRANSFER_INSTANCE_ID(VERTEX_INPUT, vertexOutput);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

		#ifdef MK_VERTEX_ANIMATION
			VERTEX_INPUT.vertex.xyz = VertexAnimation(PASS_VERTEX_ANIMATION_ARG(_VertexAnimationMap, PASS_VERTEX_ANIMATION_UV(VERTEX_INPUT.texcoord0.xy), _VertexAnimationIntensity, _VertexAnimationFrequency.xyz, VERTEX_INPUT.vertex.xyz, VERTEX_INPUT.normal));
		#endif

		//Clip Pos
		#ifdef MK_POS_WORLD
			vertexOutput.positionWorld.xyz = ComputeObjectToWorldSpace(VERTEX_INPUT.vertex.xyz);
		#else
			vertexOutput.positionWorld.xyz = 0;
		#endif
		vertexOutputLight.SV_CLIP_POS = ComputeObjectToClipSpace(VERTEX_INPUT.vertex.xyz);

		//vertex positions
		#ifdef MK_NORMAL
			vertexOutput.normalWorld.xyz = ComputeNormalWorld(VERTEX_INPUT.normal);
		#endif
		#ifdef MK_LIT
			#if defined(MK_TBN)
				vertexOutput.tangentWorld.xyz = ComputeTangentWorld(VERTEX_INPUT.tangent.xyz);
            	vertexOutput.bitangentWorld.xyz = ComputeBitangentWorld(vertexOutput.normalWorld.xyz, vertexOutput.tangentWorld.xyz, VERTEX_INPUT.tangent.w * unity_WorldTransformParams.w);
			#endif
		#endif

		//texcoords
		#if defined(MK_TCM) || defined(MK_TCD)
			//XY always RAW Coords
			//interpolated in pixel shader, artistic UV would take an additional texcoord, could be optimized some day...
			vertexOutput.uv.xy = VERTEX_INPUT.texcoord0.xy;
			
			#if defined(MK_OCCLUSION_UV_SECOND)
				vertexOutput.uv.zw = VERTEX_INPUT.texcoord0.zw;
			#else
				vertexOutput.uv.zw = 0;
			#endif
		#endif

		#ifdef MK_PARALLAX
			half3 viewTangent = ComputeViewTangent(ComputeViewWorld(vertexOutput.positionWorld.xyz), vertexOutput.normalWorld.xyz, vertexOutput.tangentWorld.xyz, vertexOutput.bitangentWorld.xyz);
			vertexOutput.normalWorld.w = viewTangent.x;
			vertexOutput.tangentWorld.w = viewTangent.y;
			vertexOutput.bitangentWorld.w = viewTangent.z;
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = VERTEX_INPUT.color;
		#endif

		#ifdef MK_LIT
			#ifdef MK_FORWARD_BASE_PASS
				#ifdef MK_VERTEX_LIGHTING
					// Approximated illumination from non-important point lights
					vertexOutputLight.vertexLighting = ComputeVertexLighting(vertexOutput.positionWorld.xyz, vertexOutput.normalWorld.xyz);
				#endif

				#ifdef MK_LIGHTMAP_UV
					vertexOutputLight.lightmapUV = 0;
					#if defined(MK_URP) || defined(MK_LWRP)
						#if defined(LIGHTMAP_ON)
							vertexOutputLight.lightmapUV.xy = ComputeStaticLightmapUV(VERTEX_INPUT.staticLightmapUV.xy);
						#else
							vertexOutputLight.lightmapUV.rgb = ComputeSHVertex(vertexOutput.positionWorld.xyz, vertexOutput.normalWorld.xyz, ComputeViewWorld(vertexOutput.positionWorld.xyz));
						#endif
					#else
						//lightmaps and ambient
						//Static lightmaps
						#if defined(LIGHTMAP_ON)
							vertexOutputLight.lightmapUV.xy = ComputeStaticLightmapUV(VERTEX_INPUT.staticLightmapUV.xy);
						//If no lightmaps used, do vertex lit if enabled
						#elif defined(UNITY_SHOULD_SAMPLE_SH)
							vertexOutputLight.lightmapUV.rgb = ComputeSHVertex(vertexOutput.positionWorld.xyz, vertexOutput.normalWorld.xyz, ComputeViewWorld(vertexOutput.positionWorld.xyz));
						#endif
					#endif

					#ifdef DYNAMICLIGHTMAP_ON
						vertexOutputLight.lightmapUV.zw = ComputeDynamicLightmapUV(VERTEX_INPUT.dynamicLightmapUV.xy);
					#endif
				#endif
			#endif

			//transform lighting coords
			TRANSFORM_WORLD_TO_SHADOW_COORDS(vertexOutput, VERTEX_INPUT, vertexOutputLight)
		#endif

		#ifdef MK_BARYCENTRIC_POS_CLIP
			vertexOutput.positionClip = vertexOutputLight.SV_CLIP_POS;
		#endif
		#ifdef MK_POS_NULL_CLIP
			vertexOutput.nullClip = ComputeObjectToClipSpace(0);
		#endif

		//vertex fog
		#ifdef MK_FOG
			vertexOutput.positionWorld.w = FogFactorVertex(vertexOutputLight.SV_CLIP_POS.z);
		#endif

		#ifdef MK_FLIPBOOK
			vertexOutput.flipbookUV.xy = VERTEX_INPUT.texcoord0.zw;
			vertexOutput.flipbookUV.z = VERTEX_INPUT.texcoordBlend;
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	MKFragmentOutput ForwardFrag(in VertexOutputLight vertexOutputLight, in VertexOutputForward vertexOutput)
	{
		UNITY_SETUP_INSTANCE_ID(vertexOutput);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(vertexOutput);

		MKFragmentOutput mkFragmentOutput;
		INITIALIZE_STRUCT(MKFragmentOutput, mkFragmentOutput);

		#ifdef MK_LOD_FADE_CROSSFADE
			LODFadeCrossFade(vertexOutputLight.SV_CLIP_POS);
		#endif

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutputLight.SV_CLIP_POS,
			PASS_POSITION_WORLD_ARG(vertexOutput.positionWorld.xyz)
			PASS_FOG_FACTOR_WORLD_ARG(vertexOutput.positionWorld.w)
			PASS_BASE_UV_ARG(vertexOutput.uv)
			PASS_LIGHTMAP_UV_ARG(vertexOutputLight.lightmapUV)
			PASS_VERTEX_COLOR_ARG(vertexOutput.color)
			PASS_NORMAL_WORLD_ARG(vertexOutput.normalWorld.xyz)
			PASS_VERTEX_LIGHTING_ARG(vertexOutputLight.vertexLighting)
			PASS_TANGENT_WORLD_ARG(vertexOutput.tangentWorld.xyz)
			PASS_VIEW_TANGENT_ARG(half3(vertexOutput.normalWorld.w, vertexOutput.tangentWorld.w, vertexOutput.bitangentWorld.w))
			PASS_BITANGENT_WORLD_ARG(vertexOutput.bitangentWorld.xyz)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(vertexOutput.positionClip)
			PASS_NULL_CLIP_ARG(vertexOutput.nullClip)
			PASS_FLIPBOOK_UV_ARG(vertexOutput.flipbookUV)
			#ifdef MK_VFACE
				, vertexOutput.vFace
			#endif
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, vertexOutputLight.SV_CLIP_POS);
		MKPBSData pbsData = ComputePBSData(surface, surfaceData);

		#ifdef MK_LIT
			//Init per mainLight
			MKLight mainLight = ComputeMainLight(surfaceData, vertexOutputLight);

			//Init mainLight data
			MKLightData lightData = ComputeLightData(mainLight, surfaceData);

			//Do per pass light
			LightingIndirect(surface, surfaceData, pbsData, mainLight, lightData);

			#if defined(MK_URP) || defined(MK_LWRP)
				#if UNITY_VERSION >= 202220
					uint meshRenderingLayers = GetMeshRenderingLayer();
					#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
					#endif
					{
						LightingDirect(surface, surfaceData, pbsData, mainLight, lightData, surface.direct);
					}

					#if defined(_ADDITIONAL_LIGHTS)
						half4 gD = surface.goochDark;
						surface.goochDark = 0;
						uint additionalLightCount = GetAdditionalLightsCount();
						half4 additionalDirect = 0;

						MKLight additionalLight;
						MKLightData additionalLightData;
						#if USE_FORWARD_PLUS
							MKInputDataWrapper inputData;
							INITIALIZE_STRUCT(MKInputDataWrapper, inputData);
							inputData.normalizedScreenSpaceUV = surfaceData.screenUV.xy;
							inputData.positionWS = surfaceData.positionWorld;
							for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
							{
								FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
								additionalLight = ComputeAdditionalLight(lightIndex, surfaceData, vertexOutputLight);
								additionalLightData = ComputeLightData(additionalLight, surfaceData);
								#ifdef _LIGHT_LAYERS
									if (IsMatchingLightLayer(additionalLight.layerMask, meshRenderingLayers))
								#endif
								{
									LightingDirectAdditional(surface, surfaceData, pbsData, additionalLight, additionalLightData, additionalDirect);
									surface.direct += additionalDirect;
								}
							}
						#endif
						LIGHT_LOOP_BEGIN(additionalLightCount)
							additionalLight = ComputeAdditionalLight(lightIndex, surfaceData, vertexOutputLight);
							additionalLightData = ComputeLightData(additionalLight, surfaceData);

							#ifdef _LIGHT_LAYERS
								if (IsMatchingLightLayer(additionalLight.layerMask, meshRenderingLayers))
							#endif
							{
								LightingDirectAdditional(surface, surfaceData, pbsData, additionalLight, additionalLightData, additionalDirect);
								surface.direct += additionalDirect;
							}
						LIGHT_LOOP_END
						surface.goochDark = gD;
					#endif
				#elif UNITY_VERSION >= 202120
					uint meshRenderingLayers = GetMeshRenderingLightLayer();
					if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
					{
						LightingDirect(surface, surfaceData, pbsData, mainLight, lightData, surface.direct);
					}

					#ifdef _ADDITIONAL_LIGHTS
						half4 gD = surface.goochDark;
						surface.goochDark = 0;
						uint additionalLightCount = GetAdditionalLightsCount();
						half4 additionalDirect = 0;

						MKLight additionalLight;
						MKLightData additionalLightData;
						#if USE_CLUSTERED_LIGHTING
							MKInputDataWrapper inputData;
							INITIALIZE_STRUCT(MKInputDataWrapper, inputData);
							inputData.normalizedScreenSpaceUV = surfaceData.screenUV.xy;
							inputData.positionWS = surfaceData.positionWorld;
							for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); ++lightIndex)
							{
								additionalLight = ComputeAdditionalLight(lightIndex, surfaceData, vertexOutputLight);
								additionalLightData = ComputeLightData(additionalLight, surfaceData);

								if (IsMatchingLightLayer(additionalLight.layerMask, meshRenderingLayers))
								{
									LightingDirectAdditional(surface, surfaceData, pbsData, additionalLight, additionalLightData, additionalDirect);
									surface.direct += additionalDirect;
								}
							}
						#endif
						LIGHT_LOOP_BEGIN(additionalLightCount)
							additionalLight = ComputeAdditionalLight(lightIndex, surfaceData, vertexOutputLight);
							additionalLightData = ComputeLightData(additionalLight, surfaceData);

							if (IsMatchingLightLayer(additionalLight.layerMask, meshRenderingLayers))
							{
								LightingDirectAdditional(surface, surfaceData, pbsData, additionalLight, additionalLightData, additionalDirect);
								surface.direct += additionalDirect;
							}
						LIGHT_LOOP_END
						surface.goochDark = gD;
					#endif
				#else
					LightingDirect(surface, surfaceData, pbsData, mainLight, lightData, surface.direct);
					#ifdef _ADDITIONAL_LIGHTS
						half4 gD = surface.goochDark;
						surface.goochDark = 0;
						uint additionalLightCount = GetAdditionalLightsCount();
						half4 additionalDirect = 0;
						
						for (uint lightIndex = 0u; lightIndex < additionalLightCount; ++lightIndex)
						{
							MKLight additionalLight = ComputeAdditionalLight(lightIndex, surfaceData, vertexOutputLight);
							MKLightData additionalLightData = ComputeLightData(additionalLight, surfaceData);
							LightingDirectAdditional(surface, surfaceData, pbsData, additionalLight, additionalLightData, additionalDirect);
							surface.direct += additionalDirect;
						}
						surface.goochDark = gD;
					#endif
				#endif
			#else
				LightingDirect(surface, surfaceData, pbsData, mainLight, lightData, surface.direct);
			#endif
		#endif

		//Finalize the output
		Composite(surface, surfaceData, pbsData);

		mkFragmentOutput.svTarget0 = surface.final;
		#ifdef MK_WRITE_RENDERING_LAYERS
			uint renderingLayers = GetMeshRenderingLayer();
			mkFragmentOutput.svTarget1 = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
		#endif

		return mkFragmentOutput;
	}
#endif