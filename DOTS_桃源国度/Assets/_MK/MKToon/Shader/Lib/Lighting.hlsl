//////////////////////////////////////////////////////
// MK Toon Lighting						       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_LIGHTING
	#define MK_TOON_LIGHTING

	#include "Core.hlsl"

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
	#else
		#include "AutoLight.cginc"
		#include "UnityGlobalIllumination.cginc"

		#ifdef POINT
			#define UNITY_LIGHT_ATTENUATION_DISTANCE(destName, input, worldPos) \
			unityShadowCoord3 lightDistanceCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
			fixed destName = tex2D(_LightTexture0, dot(lightDistanceCoord, lightDistanceCoord).rr).r;
		#endif

		#ifdef SPOT
			#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord4 lightDistanceCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1))
			#else
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord4 lightDistanceCoord = input._LightCoord
			#endif
			#define UNITY_LIGHT_ATTENUATION_DISTANCE(destName, input, worldPos) \
			DECLARE_LIGHT_DISTANCE_COORD(input, worldPos); \
			fixed destName = (lightDistanceCoord.z > 0) * UnitySpotCookie(lightDistanceCoord) * UnitySpotAttenuate(lightDistanceCoord.xyz);
		#endif

		#ifdef DIRECTIONAL
			#define UNITY_LIGHT_ATTENUATION_DISTANCE(destName, input, worldPos) fixed destName = UNITY_SHADOW_ATTENUATION(input, worldPos);
		#endif

		#ifdef POINT_COOKIE
			#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord3 lightDistanceCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz
			#else
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord3 lightDistanceCoord = input._LightCoord
			#endif
			#define UNITY_LIGHT_ATTENUATION_DISTANCE(destName, input, worldPos) \
			DECLARE_LIGHT_DISTANCE_COORD(input, worldPos); \
			fixed destName = tex2D(_LightTextureB0, dot(lightDistanceCoord, lightDistanceCoord).rr).r * texCUBE(_LightTexture0, lightDistanceCoord).w;
		#endif

		#ifdef DIRECTIONAL_COOKIE
			#if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord2 lightDistanceCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xy
			#else
				#define DECLARE_LIGHT_DISTANCE_COORD(input, worldPos) unityShadowCoord2 lightDistanceCoord = input._LightCoord
			#endif
			#define UNITY_LIGHT_ATTENUATION_DISTANCE(destName, input, worldPos) \
			DECLARE_LIGHT_DISTANCE_COORD(input, worldPos); \
			fixed destName = tex2D(_LightTexture0, lightDistanceCoord).w;
		#endif
	#endif

	#include "Surface.hlsl"
	
	// ------------------------------------------------------------------------------------------
	// Note: The complete lighting is not entirely physically "correct"
	//		 Distribution, Fresnel and Geometric terms are customized
	//		 Because its a toon shader its required to mix colors up to a qualitative artistic look
	//		 However most calculations follow a realistic input and create a toon output based on it
	//		 	Schlick Visibility is avoided by now to maintain a scalar pipeline. C.Sch. approximation is post multiplied instead
	//			Diffuse terms are expected to be a scalar instead of vec3, its later scaled to rgb
	//	 	 	The gooch is not implemented the way it was originally developed, spec / lightTransmission applied after gooch (should match the original implementation)
	//		 	Minnaert and Oren Nayar[0, PI / 2] roughness is not straightforward, in this implementation the GGX roughness is still used.
	//			Oren Nayar albedo is expected to be 1 to maintain scalar pipeline
	//		 Lighting instructions are split on 4 component => (vector (RGB), scalar (A))
	//		 	not all operations can be done in a single cycle, therefore the goal is to compute the raw value and then try to fit into a MAD for the final output
	//			this should give best trade off in terms of readability and performance
	// ------------------------------------------------------------------------------------------

	struct MKLight
	{
		half3 color;
		half3 radiometricColor;
		half3 dirWorld;
		half attenuation;
		half distanceAttenuation;
		half shadowAttenuation;
		#if defined(MK_URP) && UNITY_VERSION >= 202120
			uint layerMask;
		#endif

		#ifdef MK_LEGACY_RP
			#ifdef MK_HANDLE_SHADOWS_BLENDING_IN_GI
				half giBlend;
			#endif
		#endif
	};

	struct MKGI
	{
		half3 diffuse;
		half3 specular;
	};

	struct MKGlossyEnvironmentData
	{
		half roughness;
		half3 reflectDirection;
	};

	struct MKLightData
	{
		#ifdef MK_V_DOT_L
			half VoL;
		#endif
		#ifdef MK_N_DOT_L
			half NoLRaw;
			half NoL;
		#endif
		#ifdef MK_LND
			half3 LND;
		#endif
		#ifdef MK_V_DOT_LND
			half VoLND;
		#endif
		#ifdef MK_LHV
			half3 LHV;
		#endif
		#ifdef MK_L_DOT_LHV
			half LoLHV;
		#endif
		#ifdef MK_V_DOT_LHV
			half VoLHV;
			half oneMinusVoHV;
		#endif
		#ifdef MK_T_DOT_LHV
			half ToLHV;
		#endif
		#ifdef MK_B_DOT_LHV
			half BoLHV;
		#endif
		#ifdef MK_N_DOT_LHV
			half NoLHV;
		#endif
		#ifdef MK_ML_REF_N
			half3 MLrN;
		#endif
		#ifdef MK_ML_DOT_V
			half MLoV;
		#endif
		#ifdef MK_ML_REF_N_DOT_V
			half MLrNoV;
		#endif
	};

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Lighting Helpers
	/////////////////////////////////////////////////////////////////////////////////////////////
	#define DECLARE_LIGHTMAP_UV(i) float4 lightmapUV : TEXCOORD##i
	#define DECLARE_STATIC_LIGHTMAP_INPUT(i) float2 staticLightmapUV : TEXCOORD##i

	#if defined(MK_URP) || defined(MK_LWRP)
		float4 MKGetShadowCoord(VertexPositionInputs vertexInput)
		{
			#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(MK_SURFACE_TYPE_TRANSPARENT)
				return ComputeScreenPos(vertexInput.positionCS);
			#else
				return TransformWorldToShadowCoord(vertexInput.positionWS);
			#endif
		}
		#define DECLARE_DYNAMIC_LIGHTMAP_INPUT(i) float2 dynamicLightmapUV : TEXCOORD##i;
		/*
		#if !defined(_MAIN_LIGHT_SHADOWS_CASCADE)
			#define DECLARE_LIGHTING_COORDS(i, j) float4 _ShadowCoord : TEXCOORD##i;
		#else
			#define DECLARE_LIGHTING_COORDS(i, j)
		#endif
		*/
		//lighting coords are currently also used for later PS usage when cascade is enabled, so always set shadow coord
		#define DECLARE_LIGHTING_COORDS(i, j) float4 _ShadowCoord : TEXCOORD##i;
		#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
			#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(MK_SURFACE_TYPE_TRANSPARENT)
				#define TRANSFORM_WORLD_TO_SHADOW_COORDS(o, i, l) l._ShadowCoord = ComputeScreenPos(l.SV_CLIP_POS);
			#else
				#define TRANSFORM_WORLD_TO_SHADOW_COORDS(o, i, l) l._ShadowCoord = TransformWorldToShadowCoord(o.positionWorld.xyz);
			#endif
		#else
			#define TRANSFORM_WORLD_TO_SHADOW_COORDS(o, i, l) l._ShadowCoord = float4(0, 0, 0, 0);
		#endif
	#else
		#define DECLARE_DYNAMIC_LIGHTMAP_INPUT(i) float2 dynamicLightmapUV : TEXCOORD##i;
		#if UNITY_VERSION >= 201810
			#define DECLARE_LIGHTING_COORDS(i, j) UNITY_LIGHTING_COORDS(6,7)
		#else
			#define DECLARE_LIGHTING_COORDS(i, j) UNITY_SHADOW_COORDS(6)
		#endif 
		#if UNITY_VERSION >= 201810
			#define TRANSFORM_WORLD_TO_SHADOW_COORDS(o, i, l) UNITY_TRANSFER_LIGHTING(l, i.staticLightmapUV);
		#else
			#define TRANSFORM_WORLD_TO_SHADOW_COORDS(o, i, l) UNITY_TRANSFER_SHADOW(l, i.staticLightmapUV);
		#endif
	#endif

	struct VertexOutputLight
	{
		float4 SV_CLIP_POS : SV_POSITION;
		#ifdef MK_LIT
			#ifdef MK_VERTEX_LIGHTING
				//should be automatically clamped (0 - 1) at a 8bit precision, still enough for a simple vertex lighting
				half3 vertexLighting : COLOR1;
			#endif
			#ifdef MK_LIGHTMAP_UV
				DECLARE_LIGHTMAP_UV(5);
			#endif
			DECLARE_LIGHTING_COORDS(6, 7)
		#endif
	};

	inline half3 ComputeSHVertex(half3 normalWorld)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			return SampleSHVertex(normalWorld);
		#else
			return ShadeSHPerVertex(normalWorld, 0); //Base Ambient = 0 0 0
		#endif
	}

	inline half3 ComputeSHVertex(float3 positionWorld, half3 normalWorld, half3 viewDirectionWorld)
	{
		#if defined(MK_URP) && UNITY_VERSION >= 202310 && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
			return SampleProbeVolumeVertex(positionWorld, normalWorld, viewDirectionWorld);
		#else
			return ComputeSHVertex(normalWorld);
		#endif
	}

	inline float2 ComputeStaticLightmapUV(float2 staticLightmapUV)
	{
		return staticLightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
	}
	inline float2 ComputeDynamicLightmapUV(float2 dynamicLightmapUV)
	{
		return dynamicLightmapUV * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	}

	#if defined(MK_LIGHT_BANDED)
		#define LIGHT_STYLE_RAW_1D(value, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = Banding(value, _LightBands, smoothnessMin, smoothnessMax, threshold, _LightBandsScale)
	#elif defined(MK_LIGHT_CEL)
		#define LIGHT_STYLE_RAW_1D(value, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = Cel(threshold, smoothnessMin, smoothnessMax, value)
	#elif defined(MK_LIGHT_RAMP)
		#define LIGHT_STYLE_RAW_1D(value, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = SampleRamp1D(PASS_TEXTURE_2D(ramp, samplerRamp), value).r
	#else //MK_LIGHT_BUILTIN
		#define LIGHT_STYLE_RAW_1D(value, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = max(0.0, value)
	#endif

	#if defined(MK_LIGHT_BANDED)
		#define LIGHT_STYLE_RAW_2D(value, atten, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = Banding(value, _LightBands, smoothnessMin, smoothnessMax, threshold, _LightBandsScale)
	#elif defined(MK_LIGHT_CEL)
		#define LIGHT_STYLE_RAW_2D(value, atten, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = Cel(threshold, smoothnessMin, smoothnessMax, value)
	#elif defined(MK_LIGHT_RAMP)
		#define LIGHT_STYLE_RAW_2D(value, atten, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = SampleRamp2D(PASS_TEXTURE_2D(ramp, samplerRamp), half2(value, atten)).r
	#else //MK_LIGHT_BUILTIN
		#define LIGHT_STYLE_RAW_2D(value, atten, threshold, smoothnessMin, smoothnessMax, ramp, samplerRamp) value = max(0.0, value)
	#endif

	#if defined(MK_ARTISTIC_DRAWN)
		#define ARTISTIC_RAW(value) value = Drawn(value, surface.artistic0, _DrawnClampMin, _DrawnClampMax)
	#elif defined(MK_ARTISTIC_HATCHING)
		#define ARTISTIC_RAW(value) value = Hatching(surface.artistic0, surface.artistic1, value, 0.166667h)
	#elif defined(MK_ARTISTIC_SKETCH)
		#define ARTISTIC_RAW(value) value = Sketch(surface.artistic0, 1, value)
	#else
		#define ARTISTIC_RAW(value)
	#endif

	#if defined(MK_ARTISTIC_DRAWN)
		#define ARTISTIC_RAW_ADDITIVE(value) value = Drawn(value, surface.artistic0, _DrawnClampMax)
	#elif defined(MK_ARTISTIC_HATCHING)
		#define ARTISTIC_RAW_ADDITIVE(value) value = Hatching(surface.artistic0, surface.artistic1, value, 0)
	#elif defined(MK_ARTISTIC_SKETCH)
		#define ARTISTIC_RAW_ADDITIVE(value) value = Sketch(surface.artistic0, value)
	#else
		#define ARTISTIC_RAW_ADDITIVE(value)
	#endif

	#define TRANSFER_SCALAR_TO_VECTOR(value) value.rgb = value.a

	//Isotropic Reflection
	inline MKGlossyEnvironmentData SetupGlossyEnvironmentData(half3 reflectDirection, half roughness)
	{
		MKGlossyEnvironmentData data;
		data.roughness = roughness;
		data.reflectDirection = reflectDirection;
		return data;
	}

	//Anisotropic Reflection
	inline MKGlossyEnvironmentData SetupGlossyEnvironmentData(half3 reflectDirection, half3 bitangentWorld, half3 tangentWorld,  half3 normalWorld, half anisotropy, half roughness)
	{
		MKGlossyEnvironmentData data;
		data.roughness = roughness;

		//based on Rendering the World of Far Cry 4, McAuley Stephen
		//streching could be optimized to align the aniso direction in a more correct way
		half3 stretchDir;
		#if SHADER_TARGET >= 30
			stretchDir = anisotropy > 0 ? bitangentWorld : tangentWorld;
		#else
			stretchDir = bitangentWorld;
		#endif
		half3 reflectNormal = MKSafeNormalize(lerp(normalWorld, cross(cross(reflectDirection, stretchDir), stretchDir), abs(anisotropy) * 0.5));
		data.reflectDirection = reflectDirection - 2.0 * dot(reflectNormal, reflectDirection) * reflectNormal;
		return data;
	}

	//GI functions should match input
	inline MKGI MKGlobalIllumination(in MKGlossyEnvironmentData glossyED, half occlusion, MKLight mkLight, in MKSurfaceData surfaceData)
	{
		MKGI gi;
		INITIALIZE_STRUCT(MKGI, gi);
		gi.diffuse = half3(0,0,0);
		gi.specular = half3(0,0,0);
		#ifdef MK_INDIRECT
			#if defined(MK_URP) || defined(MK_LWRP)
				#if UNITY_VERSION >= 202310
					#if defined(DYNAMICLIGHTMAP_ON)
						gi.diffuse = SAMPLE_GI(surfaceData.lightmapUV.xy, surfaceData.lightmapUV.zw, surfaceData.lightmapUV.rgb, surfaceData.normalWorld);
					#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
						gi.diffuse = SAMPLE_GI(surfaceData.lightmapUV.rgb,
							GetAbsolutePositionWS(surfaceData.positionWorld),
							surfaceData.normalWorld,
							surfaceData.viewWorld,
							surfaceData.svPositionClip.xy);
					#else
						gi.diffuse = SAMPLE_GI(surfaceData.lightmapUV.xy, surfaceData.lightmapUV.rgb, surfaceData.normalWorld);
					#endif
				#else
					#if defined(LIGHTMAP_ON) && defined(DYNAMICLIGHTMAP_ON)
						gi.diffuse = SampleLightmap(surfaceData.lightmapUV.xy, surfaceData.lightmapUV.zw, surfaceData.normalWorld);
					#elif defined(DYNAMICLIGHTMAP_ON)
						gi.diffuse = SampleLightmap(0, surfaceData.lightmapUV.zw, surfaceData.normalWorld);
					#elif defined(LIGHTMAP_ON)
						#if UNITY_VERSION >= 202120
							gi.diffuse = SampleLightmap(surfaceData.lightmapUV.xy, 0, surfaceData.normalWorld);
						#else
							gi.diffuse = SampleLightmap(surfaceData.lightmapUV.xy, surfaceData.normalWorld);
						#endif
					#else
						gi.diffuse = SampleSHPixel(surfaceData.lightmapUV.rgb, surfaceData.normalWorld);
					#endif
				#endif

				gi.diffuse *= occlusion;
				#if UNITY_VERSION >= 202220
					gi.specular = GlossyEnvironmentReflection(glossyED.reflectDirection, surfaceData.positionWorld, glossyED.roughness, occlusion, surfaceData.screenUV.xy);
				#else
					gi.specular = GlossyEnvironmentReflection(glossyED.reflectDirection, glossyED.roughness, occlusion);
				#endif

				return gi;
			#else
				UnityGIInput giInput;
				UnityLight unityLight;
				unityLight.color = mkLight.color;
				unityLight.dir = mkLight.dirWorld;
				giInput.light = unityLight;
				giInput.worldPos = surfaceData.positionWorld;
				#ifdef MK_ENVIRONMENT_REFLECTIONS_AMBIENT
					giInput.worldViewDir = -surfaceData.viewWorld;
				#else
					giInput.worldViewDir = 0;
				#endif
				giInput.atten = mkLight.attenuation;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
					giInput.ambient = 0;
					giInput.lightmapUV = surfaceData.lightmapUV;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					giInput.ambient = surfaceData.lightmapUV.rgb;
					giInput.lightmapUV = 0;
				#endif

				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
					giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif

				//indirect specular depends on the _GLOSSYREFLECTIONS_OFF keyword by default
				//however its not defined in the non glossy MKGI function and we get no indirect specular via the unity_IndirectSpecColor RGB
				//so we always create the glossy environment
				Unity_GlossyEnvironmentData uge;
				uge.roughness = glossyED.roughness;
				uge.reflUVW = glossyED.reflectDirection;

				UnityGI unityGI = UnityGlobalIllumination(giInput, occlusion, surfaceData.normalWorld, uge);

				gi.diffuse = unityGI.indirect.diffuse;
				gi.specular = unityGI.indirect.specular;
			#endif
			return gi;
		#endif
		return gi;
	}

	inline half3 ComputeVertexLighting(float3 positionWorld, half3 normalWorld)
	{
		#if defined(MK_URP) || defined(MK_LWRP)
			return VertexLighting(positionWorld, normalWorld);
		#else
			return Shade4PointLights
			(
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, positionWorld, normalWorld
			);
		#endif
	}

	// Most clamping is using saturate instead of max due to alu/performance reasons
	MKLightData ComputeLightData(in MKLight light, in MKSurfaceData surfaceData)
	{
		MKLightData lightData;
		INITIALIZE_STRUCT(MKLightData, lightData);

		#ifdef MK_V_DOT_L
			lightData.VoL = saturate(dot(light.dirWorld, surfaceData.viewWorld));
		#endif
		#ifdef MK_N_DOT_L
			lightData.NoLRaw = dot(surfaceData.normalWorld, light.dirWorld);
			lightData.NoL = saturate(dot(surfaceData.normalWorld, light.dirWorld));
		#endif
		#ifdef MK_LND
			//instead of the surface normal, normalized pos in object space could be used
			#ifdef MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING
				lightData.LND = light.dirWorld + surfaceData.normalWorld * dot(_LightTransmissionDistortion, REL_LUMA);
			#else //Translucent
				lightData.LND = light.dirWorld + surfaceData.normalWorld * _LightTransmissionDistortion;
			#endif
		#endif
		#ifdef MK_V_DOT_LND
			lightData.VoLND = saturate(dot(surfaceData.viewWorld, -lightData.LND));
		#endif
		#ifdef MK_LHV
			lightData.LHV = MKSafeNormalize(light.dirWorld + surfaceData.viewWorld);
		#endif
		#ifdef MK_V_DOT_LHV
			lightData.VoLHV = saturate(dot(lightData.LHV, surfaceData.viewWorld));
			lightData.oneMinusVoHV = 1.0 - lightData.VoLHV;
		#endif
		//tohv and bohv should not be clamped because of the aniso usage
		#ifdef MK_T_DOT_LHV
			lightData.ToLHV = dot(surfaceData.tangentWorld, lightData.LHV);
		#endif
		#ifdef MK_B_DOT_LHV
			lightData.BoLHV = dot(surfaceData.bitangentWorld, lightData.LHV);
		#endif
		#ifdef MK_N_DOT_LHV
			lightData.NoLHV = saturate(dot(surfaceData.normalWorld, lightData.LHV));
		#endif
		#ifdef MK_L_DOT_LHV
			lightData.LoLHV = dot(light.dirWorld, lightData.LHV);
		#endif
		#ifdef MK_ML_REF_N
			lightData.MLrN = reflect(-light.dirWorld, surfaceData.normalWorld);
		#endif
		#ifdef MK_ML_DOT_V
			lightData.MLoV = saturate(dot(-light.dirWorld, surfaceData.viewWorld));
		#endif
		#ifdef MK_ML_REF_N_DOT_V
			lightData.MLrNoV = saturate(dot(lightData.MLrN, surfaceData.viewWorld));
		#endif

		return lightData;
	}

	#ifdef MK_LIT
		//not lit variants should result in compile issue if a light is accidentally used
		#if defined(MK_URP) || defined(MK_LWRP)
			MKLight ConvertURPLightToMKLight(Light light)
			{
				MKLight mkLight;
				INITIALIZE_STRUCT(MKLight, mkLight);

				mkLight.color = light.color;
				mkLight.radiometricColor = mkLight.color * PI;
				mkLight.attenuation = light.shadowAttenuation * light.distanceAttenuation;
				mkLight.dirWorld = light.direction;

				mkLight.distanceAttenuation = light.distanceAttenuation;
				mkLight.shadowAttenuation = light.shadowAttenuation;
				#if UNITY_VERSION >= 202120
					mkLight.layerMask = light.layerMask;
				#endif

				return mkLight;
			}

			Light ConvertMKLightToURPLight(MKLight mkLight)
			{
				Light light;
				INITIALIZE_STRUCT(Light, light);

				light.color = mkLight.color;
				light.direction = mkLight.dirWorld;
				
				light.distanceAttenuation = mkLight.distanceAttenuation;
				light.shadowAttenuation = mkLight.shadowAttenuation;
				#if UNITY_VERSION >= 202120
					light.layerMask = mkLight.layerMask;
				#endif

				return light;
			}
		#endif

		#if defined(MK_URP) || defined(MK_LWRP)
			#ifdef MK_LINEAR_lIGHT_DISTANCE_ATTENUATION
				inline void LinearDistanceAttenuation(inout Light light, in MKSurfaceData surfaceData, in int index)
				{
					//color & light layers should be untouched
					#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
						float4 lightPositionWS = _AdditionalLightsBuffer[index].position;
						half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[index].attenuation;
						half4 spotDirection = _AdditionalLightsBuffer[index].spotDirection;
						half4 lightOcclusionProbeInfo = _AdditionalLightsBuffer[index].occlusionProbeChannels;
					#else
						float4 lightPositionWS = _AdditionalLightsPosition[index];
						half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[index];
						half4 spotDirection = _AdditionalLightsSpotDir[index];
						half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[index];
					#endif

					float3 LightToWorld = lightPositionWS.xyz - surfaceData.positionWorld * lightPositionWS.w;
					float distanceSqr = max(dot(LightToWorld, LightToWorld), HALF_MIN);
					half3 LightDir = half3(LightToWorld * rsqrt(distanceSqr));

					float lightRangeSqr = Rcp(distanceAndSpotAttenuation.x);
					light.distanceAttenuation = saturate(saturate((1 - (distanceSqr / lightRangeSqr)) * 5.0) * Rcp(1.0 + 25.0 * distanceSqr / lightRangeSqr)) * AngleAttenuation(spotDirection.xyz, LightDir, distanceAndSpotAttenuation.zw);
				}
			#endif
		#endif

		MKLight ComputeMainLight(in MKSurfaceData surfaceData, inout VertexOutputLight vertexOutputLight)
		{
			MKLight mkLight;
			INITIALIZE_STRUCT(MKLight, mkLight);

			#if defined(MK_URP) || defined(MK_LWRP)
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					//skip interpolator...
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					vertexOutputLight._ShadowCoord = TransformWorldToShadowCoord(surfaceData.positionWorld);
				#else
					vertexOutputLight._ShadowCoord = float4(0, 0, 0, 0);
				#endif
			#endif
			
			#if defined(MK_URP) || defined(MK_LWRP)
				Light light;
				INITIALIZE_STRUCT(Light, light);

				#if defined(MK_URP_2020_2_Or_Newer)
					light = GetMainLight(vertexOutputLight._ShadowCoord, surfaceData.positionWorld, surfaceData.shadowMask);
				#else
					light = GetMainLight(vertexOutputLight._ShadowCoord);
				#endif

				mkLight.color = light.color;
				mkLight.radiometricColor = mkLight.color * PI;
				mkLight.attenuation = light.distanceAttenuation * light.shadowAttenuation;
				mkLight.dirWorld = light.direction;

				mkLight.distanceAttenuation = light.distanceAttenuation;
				mkLight.shadowAttenuation = light.shadowAttenuation;
				#if UNITY_VERSION >= 202120
					mkLight.layerMask = light.layerMask;
				#endif
			#else
				//lightdirection and attenuation
				#ifdef USING_DIRECTIONAL_LIGHT
					mkLight.dirWorld = MKSafeNormalize(_WorldSpaceLightPos0.xyz);
					mkLight.distanceAttenuation = 1;
				#else
					mkLight.dirWorld = MKSafeNormalize(_WorldSpaceLightPos0.xyz - surfaceData.positionWorld);
					//Atten Old
					//mkLight.distanceAttenuation = saturate(1.0 - ((distance(_WorldSpaceLightPos0.xyz, surfaceData.positionWorld)) / Rcp(_LightPositionRange.w)));
					UNITY_LIGHT_ATTENUATION_DISTANCE(distanceAtten, vertexOutputLight, surfaceData.positionWorld);
					mkLight.distanceAttenuation = distanceAtten;
				#endif

				UNITY_LIGHT_ATTENUATION(atten, vertexOutputLight, surfaceData.positionWorld);
				mkLight.shadowAttenuation = UNITY_SHADOW_ATTENUATION(vertexOutputLight, surfaceData.positionWorld);

				#ifdef MK_HANDLE_SHADOWS_BLENDING_IN_GI
					mkLight.giBlend = 0;
					atten = mkLight.shadowAttenuation;
					float shadowFade = UnityComputeShadowFade(UnityComputeShadowFadeDistance(surfaceData.positionWorld, dot(_WorldSpaceCameraPos - surfaceData.positionWorld, UNITY_MATRIX_V[2].xyz)));
					float bakedOcclusion = UnitySampleBakedOcclusion(surfaceData.lightmapUV.xy, surfaceData.positionWorld);
					atten = UnityMixRealtimeAndBakedShadows(atten, bakedOcclusion, shadowFade);
					#if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
						mkLight.giBlend = atten;
					#endif
				#endif

				mkLight.attenuation = atten;
				mkLight.color = _LightColor0.rgb;
				mkLight.radiometricColor = mkLight.color * PI;
			#endif

			return mkLight;
		}

		MKLight ComputeAdditionalLight(int index, in MKSurfaceData surfaceData, inout VertexOutputLight vertexOutputLight)
		{
			#if defined(MK_URP) || defined(MK_LWRP)
				MKLight mkLight;
				INITIALIZE_STRUCT(MKLight, mkLight);
				Light light;
				INITIALIZE_STRUCT(Light, light);
				#if defined(MK_URP_2020_2_Or_Newer)
					light = GetAdditionalLight(index, surfaceData.positionWorld, surfaceData.shadowMask);
				#else
					light = GetAdditionalLight(index, surfaceData.positionWorld);
				#endif

				#ifdef MK_LINEAR_lIGHT_DISTANCE_ATTENUATION
					LinearDistanceAttenuation(light, surfaceData, GetPerObjectLightIndex(index));
				#endif
				mkLight.distanceAttenuation = light.distanceAttenuation;
				mkLight.color = light.color;
				mkLight.radiometricColor = mkLight.color * PI;
				mkLight.attenuation = light.distanceAttenuation * light.shadowAttenuation;
				mkLight.dirWorld = light.direction;

				mkLight.shadowAttenuation = light.shadowAttenuation;
				#if UNITY_VERSION >= 202120
					mkLight.layerMask = light.layerMask;
				#endif

				return mkLight;
			#else
				//On Legacy RP additional lights are computed per pass
				return ComputeMainLight(surfaceData, vertexOutputLight);
			#endif
		}
	#endif

	inline half4 RimRawBright(half ndl, half size, half oneMinusVoN, half smoothness, Surface surface, MKLight light)
	{
		//to get a smoother rim ndl is multiplied into the interpolation
		//to get a harder rim ndl could be multiplied afterwards
		half4 rim;
		rim.a = pow(oneMinusVoN, size);

		#ifdef MK_THRESHOLD_MAP
			rim.a -= _RimThresholdOffset * surface.thresholdOffset;
			rim.a += _RimThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
		#endif

		LIGHT_STYLE_RAW_2D(rim.a, light.distanceAttenuation, T_V, smoothness, smoothness, _RimRamp, SAMPLER_CLAMPED_MAIN);
		rim.a *= ndl;
		ARTISTIC_RAW(rim.a);
		TRANSFER_SCALAR_TO_VECTOR(rim);

		return rim;
	}

	inline half4 RimRawDark(half ndl, half size, half oneMinusVoN, half smoothness, Surface surface, MKLight light)
	{
		half4 rim;
		rim.a = pow(oneMinusVoN, size);

		#ifdef MK_THRESHOLD_MAP
			rim.a -= _RimThresholdOffset * surface.thresholdOffset;
			rim.a += _RimThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
		#endif

		LIGHT_STYLE_RAW_2D(rim.a, light.distanceAttenuation, T_V, smoothness, smoothness, _RimRamp, SAMPLER_CLAMPED_MAIN);
		rim.a *= ndl;
		ARTISTIC_RAW(rim.a);
		TRANSFER_SCALAR_TO_VECTOR(rim);
		
		return rim;
	}

	//Rim with smooth interpolation
	inline half4 RimRawEverything(half size, half oneMinusVoN, half smoothness, Surface surface)
	{
		half4 rim;
		rim.a = pow(oneMinusVoN, size);

		#ifdef MK_THRESHOLD_MAP
			rim.a -= _RimThresholdOffset * surface.thresholdOffset;
			rim.a += _RimThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
		#endif
		
		LIGHT_STYLE_RAW_1D(rim.a, T_V, smoothness, smoothness, _RimRamp, SAMPLER_CLAMPED_MAIN);
		ARTISTIC_RAW(rim.a);
		TRANSFER_SCALAR_TO_VECTOR(rim);

		return rim;
	}

	inline half4 Iridescence(half size, half oneMinusVoN, half smoothness, Surface surface)
	{
		half4 iridescence;
		iridescence.a = pow(oneMinusVoN, size);

		#ifdef MK_THRESHOLD_MAP
			iridescence.a -= _IridescenceThresholdOffset * surface.thresholdOffset;
			iridescence.a += _IridescenceThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
		#endif
		
		#if !defined(MK_LIGHT_RAMP)
			//only style scalar iridescence if lighting is not set to ramp
			LIGHT_STYLE_RAW_1D(iridescence.a, T_V, smoothness, smoothness, _DiffuseRamp, SAMPLER_CLAMPED_MAIN);
		#endif
		ARTISTIC_RAW(iridescence.a);
		TRANSFER_SCALAR_TO_VECTOR(iridescence);
		
		iridescence.rgb *= (SampleRamp1D(PASS_TEXTURE_2D(_IridescenceRamp, SAMPLER_CLAMPED_MAIN), iridescence.a).rgb * _IridescenceColor.rgb);
		return iridescence;
	}

	inline half Minnaert(half ndl, half vdn, half roughness)
	{
		return ndl * pow(saturate(ndl) * vdn, roughness);
	}

	half OrenNayar(half ndl, half ndv, half vdl, half roughness)
	{
		half3 rough = SafeDivide(roughness, roughness + half3(0.33, 0.13, 0.09));
		half3 coeff = half3(1, 0, 0) + half3(-0.5, 0.17, 0.45) * rough;
		half diff = vdl - ndl * ndv;
		diff = SafeDivide(diff, lerp(max(ndl, ndv), 1, step(diff, 0)));

		return ndl * (coeff.z * diff + coeff.y + coeff.x);
	}

	//Scale of the aniso
	inline half2 AnisoScale(half roughness, half anisotropy)
	{
		//original ggx roughness
		//half aspect = sqrt(1.0 - 0.9 * anisotropy);
		//return half2(roughness / aspect, roughness * aspect);

		//approximated
		return half2(roughness * (1 + 0.9 * anisotropy), roughness * (1 - 0.9 * anisotropy));
	}

	inline half DistributionGGX(half NoLHV, half ToLHV, half BoLHV, half roughness, half anisoropy)
	{
		half2 anisoScale = AnisoScale(roughness, anisoropy);
		half anisoStretch = anisoScale.x * anisoScale.y;

		half p = Rcp(anisoStretch);
		half ay = FastPow2(ToLHV) / FastPow2(anisoScale.x);
		half ax = FastPow2(BoLHV) / FastPow2(anisoScale.y);

		return INV_PI * p * Rcp(FastPow2(ay + ax + FastPow2(NoLHV)));
	}

	inline half DistributionGGX(half NoLHV, half roughnessP4)
	{
		return SafeDivide(roughnessP4, (PI * FastPow2(FastPow2(NoLHV) * (roughnessP4 - 1.0) + 1)));
	}

	inline half GeometricSchlickGGX(half VoN, half roughness)
	{	
		return SafeDivide(VoN, (VoN * (1.0 - roughness) + roughness));
	}

	inline half GeometricSmithGGX(half VoN, half NoL, half roughness)
	{
		half directRoughness = FastPow2(roughness + 1.0) * 0.125;
		return GeometricSchlickGGX(VoN, directRoughness) * GeometricSchlickGGX(NoL, directRoughness);
	}

	inline half3 FresnelCSch(half LoLHV, half3 f0)
	{
		//limit, because C.Sch goes to infinity instead of 1.
		return saturate(SafeDivide(f0, LoLHV));
	}

	//Aniso specular blinn phong
	inline half BlinnSpecularAniso(half3 normal, half3 halfV, half ndhv, half shine, half offset, half4 aDir, half ndl)
	{
		half term = pow(lerp(ndhv, max(0.0, sin(radians((dot(MKSafeNormalize(normal + aDir.rgb), halfV) + offset) * 180.0))), aDir.a), shine);
		#if SHADER_TARGET >= 30
			return (ndl > 0.0) ? term : 0.0;
		#else
			return term;
		#endif
	}

	//specular blinn phong
	inline half BlinnSpecular(half ndhv, half shine)
	{
		//exp2 instead of linear SHINE_MULT to match URP behavior
		return pow(ndhv, exp2(10 * shine + 1));
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Lighting Indirect
	/////////////////////////////////////////////////////////////////////////////////////////////
	inline void LightingIndirect(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData, in MKLight light, in MKLightData lightData)
	{
		#ifdef MK_INDIRECT
			#if defined(MK_ENVIRONMENT_REFLECTIONS_ADVANCED)
				#ifdef MK_SPECULAR_ANISOTROPIC
					MKGlossyEnvironmentData ged = SetupGlossyEnvironmentData(-surfaceData.viewWorld, surfaceData.bitangentWorld, surfaceData.tangentWorld, surfaceData.normalWorld, _Anisotropy, pbsData.roughness);
				#else
					MKGlossyEnvironmentData ged = SetupGlossyEnvironmentData(surfaceData.MVrN, pbsData.roughness);
				#endif
			#elif defined(MK_ENVIRONMENT_REFLECTIONS_AMBIENT)
				MKGlossyEnvironmentData ged = SetupGlossyEnvironmentData(0, pbsData.roughness);
			#endif

			#if defined(MK_ENVIRONMENT_REFLECTIONS)
				MKGI gi;
				#if defined(MK_SCREEN_SPACE_OCCLUSION) && defined(MK_URP_2020_2_Or_Newer)
					surface.occlusion.r *= surfaceData.ambientOcclusion.indirectAmbientOcclusion;
				#endif

				#if defined(MK_ENVIRONMENT_REFLECTIONS_ADVANCED)
					gi = MKGlobalIllumination(ged, surface.occlusion.r, light, surfaceData);
				#elif defined(MK_ENVIRONMENT_REFLECTIONS_AMBIENT)
					gi = MKGlobalIllumination(ged, surface.occlusion.r, light, surfaceData);
				#endif
				#if defined(MK_URP) || defined(MK_LWRP)
					Light urpLight;
					INITIALIZE_STRUCT(Light, urpLight);
					urpLight = ConvertMKLightToURPLight(light);
					MixRealtimeAndBakedGI(urpLight, surfaceData.normalWorld, gi.diffuse);
				#endif
				/*
				#ifdef MK_ARTISTIC
					surface.indirect = lerp(0, gi.diffuse * pbsData.diffuseRadiance, surface.direct);
				#else
					surface.indirect = gi.diffuse * pbsData.diffuseRadiance;
				#endif
				*/
				surface.indirect = gi.diffuse * pbsData.diffuseRadiance;
			#else
				surface.indirect = pbsData.specularRadiance * pbsData.reflectivity;
			#endif

			half3 iL;
			half3 indirectReflectRadiance;
			#if defined(MK_PBS)
				indirectReflectRadiance = pbsData.specularRadiance;
			#else //Simple
				indirectReflectRadiance = 0;
			#endif

			#ifdef MK_FRESNEL_HIGHLIGHTS
				//lerp(indirectReflectRadiance, saturate(pbsData.smoothness + pbsData.reflectivity), pbsData.fresnel); // lerp(0.33, 1, pbsData.smoothness)
				iL = pbsData.fresnel;
			#else
				iL = indirectReflectRadiance;
			#endif
			#if defined(MK_ENVIRONMENT_REFLECTIONS)
				surface.indirect += Rcp(pbsData.roughnessPow4 + 1.0) * gi.specular * iL;
			#endif

			#ifdef MK_PBS
				surface.indirectFade = lerp(surface.indirectFade, pbsData.reflectivity, pbsData.reflectivity);
			#endif

			#ifdef MK_EMISSION
				//Emission handled as indirect
				surface.indirect += surface.emission;
			#endif
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Lighting Direct
	/////////////////////////////////////////////////////////////////////////////////////////////
	inline half MKLightingDiffuse(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData, in MKLight light, in MKLightData lightData)
	{
		#ifdef MK_LIT
			half diffuse;
			#if defined(MK_DIFFUSE_MINNAERT)
				diffuse = Minnaert(lightData.NoLRaw, surfaceData.VoN, pbsData.roughnessPow2);
			#elif defined(MK_DIFFUSE_OREN_NAYAR)
				diffuse = OrenNayar(lightData.NoLRaw, surfaceData.VoN, lightData.VoL, pbsData.roughnessPow2);
			#else
				//MK_SIMPLE
				diffuse = lightData.NoLRaw;
			#endif

			#if defined(MK_WRAPPED_DIFFUSE)
				diffuse = HalfWrap(diffuse, 0.5);
			#endif

			#ifdef MK_THRESHOLD_MAP
				diffuse -= _DiffuseThresholdOffset * surface.thresholdOffset;
				diffuse += _DiffuseThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
			#endif

			//Lighting could be optimized by combining every component (diffuse, specular, lightTransmission, indirect/direct), may break gooch
			#ifdef MK_STYLIZE_SYSTEM_SHADOWS
				LIGHT_STYLE_RAW_2D(light.shadowAttenuation, 1, _LightThreshold, _DiffuseSmoothness * 0.5, _DiffuseSmoothness * 0.5, _DiffuseRamp, SAMPLER_CLAMPED_MAIN);
				diffuse *= light.shadowAttenuation;
			#endif
			LIGHT_STYLE_RAW_2D(diffuse, light.distanceAttenuation, _LightThreshold, _DiffuseSmoothness * 0.5, _DiffuseSmoothness * 0.5, _DiffuseRamp, SAMPLER_CLAMPED_MAIN);
			#ifndef MK_STYLIZE_SYSTEM_SHADOWS
				diffuse *= light.attenuation;
			#else
				diffuse *= light.distanceAttenuation;
			#endif

			#ifdef MK_LEGACY_RP
				#ifdef MK_HANDLE_SHADOWS_BLENDING_IN_GI
					diffuse *= saturate(1.0 - light.giBlend);
				#endif
			#endif

			return diffuse;
		#else
			return 1;
		#endif
	}

	inline void MKLightingSFX(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData, in MKLight light, in MKLightData lightData, in half4 diffuse, inout half4 finalLightColor)
	{
		#ifdef MK_LIT
			half3 goochRamp;
			#ifdef MK_GOOCH_RAMP
				goochRamp = lerp(1.0, SampleRamp2D(PASS_TEXTURE_2D(_GoochRamp, SAMPLER_CLAMPED_MAIN), half2(diffuse.a, light.distanceAttenuation)).rgb, _GoochRampIntensity);
			#else
				goochRamp = 1.0;
			#endif
			half3 gooch;
			//Gooch needs to be applied on diffuse only to not distract other light styles such as indirect, spec, lightTransmission
			gooch = goochRamp * lerp(surface.goochDark.rgb, surface.goochBright.rgb, max(diffuse.r, max(diffuse.g, diffuse.b)));

			//#ifdef MK_GOOCH_RAMP
			//	gooch.rgb = lerp(gooch.rgb, SampleRamp2D(PASS_TEXTURE_2D(_GoochRamp, SAMPLER_CLAMPED_MAIN), half2(diffuse.a, light.distanceAttenuation)).rgb, _GoochRampIntensity);
			//#endif

			//Surface Direct + Gooch
			#ifdef MK_SPECULAR
				half4 specular;
				#if defined(MK_PBS)
					//Distribution - Geometric - Fresnel
					half distribution, geometric;
					half3 sFresnel;
					#ifdef MK_SPECULAR_ANISOTROPIC
						//BRDF Aniso Specular
						distribution = DistributionGGX(lightData.NoLHV, lightData.ToLHV, lightData.BoLHV, pbsData.roughnessPow2, _Anisotropy);
						//Isotropic Geo term is producing more pleasant results so its used for now
						geometric = GeometricSmithGGX(surfaceData.VoN, lightData.NoL, pbsData.roughness);
						sFresnel = FresnelCSch(lightData.LoLHV, pbsData.specularRadiance);
					#else
						distribution = DistributionGGX(lightData.NoLHV, pbsData.roughnessPow4);
						geometric = GeometricSmithGGX(surfaceData.VoN, lightData.NoL, pbsData.roughness);
						sFresnel = FresnelCSch(lightData.LoLHV, pbsData.specularRadiance);
					#endif
					specular.a = SafeDivide(distribution * geometric, 4 * surfaceData.VoN * lightData.NoL);
				#else //MK_SIMPLE Iso Only
					specular.a = BlinnSpecular(lightData.NoLHV, pbsData.smoothness);
				#endif

				#ifdef MK_SHADER_API_MOBILE
					specular.a = clamp(specular.a, 0.0, 100.0);
				#endif

				#ifdef MK_THRESHOLD_MAP
					specular.a -= _SpecularThresholdOffset * surface.thresholdOffset;
					specular.a += _SpecularThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
				#endif

				// specular could be thresholded using: lerp(T_Q, 1, _LightThreshold) but confuses the user because specular is influenced by smoothness and threshold then
				LIGHT_STYLE_RAW_2D(specular.a, light.distanceAttenuation, T_V, _SpecularSmoothness * 0.5, _SpecularSmoothness * 0.5, _SpecularRamp, SAMPLER_CLAMPED_MAIN);
				ARTISTIC_RAW_ADDITIVE(specular.a);
				TRANSFER_SCALAR_TO_VECTOR(specular);

				#ifdef MK_PBS
					finalLightColor.rgb = ((sFresnel * _SpecularIntensity) * specular.rgb + (pbsData.diffuseRadiance * INV_PI)) * light.radiometricColor * gooch;
				#else
					finalLightColor.rgb = (pbsData.specularRadiance * _SpecularIntensity * specular.rgb + (pbsData.diffuseRadiance * INV_PI)) * light.radiometricColor * gooch;
				#endif
			#else
				finalLightColor.rgb = (pbsData.diffuseRadiance * INV_PI) * light.radiometricColor * gooch;
			#endif

			#ifdef MK_LightTransmission
				half4 lightTransmission;
				//A scaling could be implemented here: dot(vohld, scale) then saturate
				lightTransmission.a = FastPow4(lightData.VoLND);
				//Based on Colin Barre-Brisebois - GDC 2011 - Approximating Translucency for a Fast, Cheap and Convincing Subsurface-Scattering
				#ifdef MK_THICKNESS_MAP
					lightTransmission.a *= surface.thickness;
				#endif
				half sssAtten = light.distanceAttenuation;

				//Custom atten stylize not required, because shadows dont affect it
				#ifdef MK_LIGHT_TRANSMISSION_TRANSLUCENT
					sssAtten = lerp(0, sssAtten, pbsData.oneMinusReflectivity);
				#endif
				lightTransmission.a *= sssAtten;

				#ifdef MK_THRESHOLD_MAP
					lightTransmission.a -= _LightTransmissionThresholdOffset * surface.thresholdOffset;
					lightTransmission.a += _LightTransmissionThresholdOffset * THRESHOLD_OFFSET_NORMALIZER;
				#endif

				LIGHT_STYLE_RAW_2D(lightTransmission.a, light.distanceAttenuation, T_V, _LightTransmissionSmoothness * 0.5, _LightTransmissionSmoothness * 0.5, _LightTransmissionRamp, SAMPLER_CLAMPED_MAIN);
				ARTISTIC_RAW_ADDITIVE(lightTransmission.a);
				TRANSFER_SCALAR_TO_VECTOR(lightTransmission);

				#ifdef MK_LIGHT_TRANSMISSION_SUB_SURFACE_SCATTERING
					finalLightColor.rgb += lightTransmission.rgb * _LightTransmissionColor.rgb * light.color * pbsData.diffuseRadiance * _LightTransmissionIntensity;
				#else //Translucent
					finalLightColor.rgb += lightTransmission.rgb * _LightTransmissionColor.rgb * light.color * _LightTransmissionIntensity;
				#endif
			#endif

			#ifdef MK_LIGHTING_ALPHA
				#if defined(MK_ALPHA_LOOKUP)
					finalLightColor.a = dot(finalLightColor.rgb, REL_LUMA);
					//finalLightColor.a = (finalLightColor.r + finalLightColor.g + finalLightColor.b) * 0.33;
				#endif
				#if defined(MK_SURFACE_TYPE_OPAQUE)
					finalLightColor.a = 1;
				#endif
			#else
				finalLightColor.a = 1;
			#endif
		#endif
	}

	inline void LightingDirectAdditional(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData, in MKLight light, in MKLightData lightData, out half4 finalLightColor)
	{
		#ifdef MK_LIT
			half diffuseRaw = MKLightingDiffuse(surface, surfaceData, pbsData, light, lightData);
			half4 diffuse = half4(0, 0, 0, diffuseRaw);
			ARTISTIC_RAW(diffuse.a);
			//diffuse.a *= diffuseRaw;
			TRANSFER_SCALAR_TO_VECTOR(diffuse);
			
			MKLightingSFX(surface, surfaceData, pbsData, light, lightData, diffuse, finalLightColor);
		#endif
	}

	inline void LightingDirect(inout Surface surface, in MKSurfaceData surfaceData, in MKPBSData pbsData, in MKLight light, in MKLightData lightData, out half4 finalLightColor)
	{
		#ifdef MK_LIT
			half diffuseRaw = MKLightingDiffuse(surface, surfaceData, pbsData, light, lightData);
			half4 diffuse = half4(0, 0, 0, diffuseRaw);
			ARTISTIC_RAW(diffuse.a);
			TRANSFER_SCALAR_TO_VECTOR(diffuse);
			
			MKLightingSFX(surface, surfaceData, pbsData, light, lightData, diffuse, finalLightColor);

			#if defined(MK_RIM_SPLIT)
				#ifndef MK_ADDITIONAL_LIGHTS
					surface.rim += RimRawBright(diffuse.a, _RimSize, surfaceData.OneMinusVoN, _RimSmoothness * 0.5, surface, light);
				#endif
				//surface.rimDark = RimRawDark(1.0 - saturate(diffuse.a), _RimSize, surfaceData.OneMinusVoN, _RimSmoothness * 0.5, surface, light);
			#endif
		#endif
	}
#endif