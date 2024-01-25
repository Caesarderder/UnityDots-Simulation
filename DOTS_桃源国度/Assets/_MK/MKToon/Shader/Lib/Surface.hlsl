//////////////////////////////////////////////////////
// MK Toon Surface		       						//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_SURFACE
	#define MK_TOON_SURFACE

	#include "Core.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Surface Data
	/////////////////////////////////////////////////////////////////////////////////////////////
	//Dynamic precalc struct
	struct MKSurfaceData
	{
		float4 svPositionClip;
		#ifdef MK_VFACE
			half vFace;
		#endif
		#ifdef MK_NORMAL
			half3 vertexNormalWorld;
		#endif
		#if defined(MK_DEPTH_NORMALS_PASS) || defined(MK_LIT)
			half3 normalWorld;
		#endif
		#ifdef MK_LIT
			#ifdef MK_LIGHTMAP_UV
				float4 lightmapUV;
			#endif
			#ifdef MK_VERTEX_LIGHTING
				half3 vertexLighting;
			#endif
			#ifdef MK_TBN
				half3 tangentWorld;
				half3 bitangentWorld;
			#endif
			#ifdef MK_PARALLAX
				half height;
			#endif
		#endif

		#if defined(MK_URP_2020_2_Or_Newer) && defined(MK_LIT)
			half4 shadowMask;
		#endif

		#if defined(MK_SCREEN_SPACE_OCCLUSION) && defined(MK_URP_2020_2_Or_Newer)
			AmbientOcclusionFactor ambientOcclusion;
		#endif

		#ifdef MK_VD
			half3 viewWorld;
		#endif
		#ifdef MK_VD_O
			half3 viewTangent;
		#endif

		#ifdef MK_POS_WORLD
			float3 positionWorld;
		#endif
		#ifdef MK_FOG
			float fogFactor;
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			half4 vertexColor;
		#endif

		#if defined(MK_TCM) || defined(MK_TCD)
			float2 rawUV;
		#endif
		#if defined(MK_TCM)
			float2 baseUV;
		#endif
		#if defined(MK_TCD)
			float2 detailUV;
		#endif
		#ifdef MK_OCCLUSION_UV_SECOND
			float2 secondUV;
		#endif
		#ifdef MK_THRESHOLD_MAP
			float2 thresholdUV;
		#endif
		#if defined(MK_SCREEN_UV)
			float4 screenUV;
		#endif
		#if defined(MK_REFRACTION)
			float2 refractionUV;
		#endif
		#ifdef MK_ARTISTIC
			float2 artisticUV;
		#endif
		#ifdef MK_FLIPBOOK
			float3 flipbookUV;
		#endif
		#ifdef MK_DISSOLVE
			half dissolveClip;
		#endif

		#ifdef MK_V_DOT_N
			half VoN;
			half OneMinusVoN;
		#endif
		#ifdef MK_MV_REF_N
			half3 MVrN;
		#endif
	};

	struct MKPBSData
	{
		half reflectivity;
		half oneMinusReflectivity;
		half roughness;
		#if defined(MK_ENVIRONMENT_REFLECTIONS) || defined(MK_SPECULAR) || defined(MK_DIFFUSE_OREN_NAYAR) || defined(MK_DIFFUSE_MINNAERT)
			half roughnessPow2;
		#endif
		#if defined(MK_ENVIRONMENT_REFLECTIONS) || defined(MK_SPECULAR)
			half roughnessPow4;
		#endif
		half smoothness;
		half metallic;
		half3 specular;
		half3 diffuseRadiance;
		half3 specularRadiance;
		#ifdef MK_FRESNEL_HIGHLIGHTS
			half3 fresnel;
		#endif
	};

	//dynamic surface struct
	struct Surface
	{
		half4 final;
		half3 albedo;
		#ifdef MK_DETAIL_MAP
			half4 detail;
		#endif
		half alpha;
		#ifdef MK_REFRACTION
			half3 refraction;
		#endif

		// RGB - RAW
		#if defined(MK_LIT)
			half indirectFade;
			#ifdef MK_THRESHOLD_MAP
				half thresholdOffset;
			#endif
			half4 goochBright;
			half4 goochDark;
			half2 occlusion;
			#ifdef MK_EMISSION
				half3 emission;
			#endif

			#ifdef MK_INDIRECT
				half3 indirect;
			#endif
			half4 direct;

			#ifdef MK_ARTISTIC
				#if defined(MK_ARTISTIC_DRAWN) || defined(MK_ARTISTIC_SKETCH)
					half artistic0;
				#elif defined(MK_ARTISTIC_HATCHING)
					half3 artistic0;
					half3 artistic1;
				#endif
			#endif
			#ifdef MK_THICKNESS_MAP
				half thickness;
			#endif
			#if defined(MK_RIM)
				half4 rim;
			#endif
			#ifdef MK_IRIDESCENCE
				half4 iridescence;
			#endif
		#endif
	};

	half3 FresnelSchlickGGXIBL(half oneMinusVoN, half3 f0, half smoothness)
	{
		return FastPow4(oneMinusVoN) * (max(smoothness, f0) - f0) + f0;
	} 

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Header macros
	/////////////////////////////////////////////////////////////////////////////////////////////
	#ifdef MK_PBS
		#define PASS_BLENDING_ARG(surface, surfaceData, pbsData) surface, surfaceData, pbsData
	#else
		#define PASS_BLENDING_ARG(surface, surfaceData, pbsData) surface, surfaceData
	#endif

	#ifdef MK_FLIPBOOK
		#define SURFACE_FLIPBOOK_UV surfaceData.flipbookUV
	#else
		#define SURFACE_FLIPBOOK_UV 0
	#endif

	#ifdef MK_POS_WORLD
		#define PASS_POSITION_WORLD_ARG(positionWorld) positionWorld
	#else
		#define PASS_POSITION_WORLD_ARG(positionWorld) 0
	#endif

	#ifdef MK_FOG
		#define PASS_FOG_FACTOR_WORLD_ARG(fogFacor) ,fogFacor
	#else
		#define PASS_FOG_FACTOR_WORLD_ARG(fogFacor)
	#endif

	#if defined(MK_TCM) || defined(MK_TCD)
		#define PASS_BASE_UV_ARG(baseUV) ,baseUV
	#else
		#define PASS_BASE_UV_ARG(baseUV)
	#endif

	#ifdef MK_VERTEX_COLOR_REQUIRED
		#define PASS_VERTEX_COLOR_ARG(vertexColor) ,vertexColor
	#else
		#define PASS_VERTEX_COLOR_ARG(vertexColor)
	#endif

	#if defined(MK_NORMAL)
		#define PASS_NORMAL_WORLD_ARG(normalWorld) ,normalWorld
	#else
		#define PASS_NORMAL_WORLD_ARG(normalWorld)
	#endif

	#if defined(MK_VERTEX_LIGHTING)
		#define PASS_VERTEX_LIGHTING_ARG(vertexLighting) ,vertexLighting
	#else
		#define PASS_VERTEX_LIGHTING_ARG(vertexLighting)
	#endif

	#if defined(MK_TBN)
		#define PASS_TANGENT_WORLD_ARG(tangentWorld) ,tangentWorld
		#define PASS_BITANGENT_WORLD_ARG(bitangentWorld) ,bitangentWorld
	#else
		#define PASS_TANGENT_WORLD_ARG(bitangentWorld)
		#define PASS_BITANGENT_WORLD_ARG(bitangentWorld)
	#endif

	#ifdef MK_LIGHTMAP_UV
		#define PASS_LIGHTMAP_UV_ARG(lightmapUV) ,lightmapUV
	#else
		#define PASS_LIGHTMAP_UV_ARG(lightmapUV)
	#endif

	#if defined(MK_PARALLAX)
		#define PASS_VIEW_TANGENT_ARG(viewTangent) ,viewTangent
	#else
		#define PASS_VIEW_TANGENT_ARG(viewTangent)
	#endif

	#ifdef MK_BARYCENTRIC_POS_CLIP
		#define PASS_BARYCENTRIC_POSITION_CLIP_ARG(barycentricPositionClip) ,barycentricPositionClip
	#else
		#define PASS_BARYCENTRIC_POSITION_CLIP_ARG(barycentricPositionClip)
	#endif

	#ifdef MK_POS_NULL_CLIP
		#define PASS_NULL_CLIP_ARG(nullClip) ,nullClip
	#else
		#define PASS_NULL_CLIP_ARG(nullClip)
	#endif

	#ifdef MK_FLIPBOOK
		#define PASS_FLIPBOOK_UV_ARG(flipbookUV) ,flipbookUV
	#else
		#define PASS_FLIPBOOK_UV_ARG(flipbookUV)
	#endif

	//Texture color
	inline void SurfaceColor(out half3 albedo, out half alpha, DECLARE_SAMPLER_2D_ARGS(albedoMap), float2 uv, float3 blendUV, half4 color)
	{
		half4 c;
		#ifdef MK_OUTLINE_PASS
			c = half4(color.rgb, SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap, uv, blendUV).a * color.a);
		#else
			c = SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap, uv, blendUV);
			c *= color;
		#endif
		albedo = c.rgb;
		#if defined(MK_ALPHA_LOOKUP)
			alpha = c.a;
		#else
			alpha = 1.0h;
		#endif
	}

	inline void SurfaceColor(out half3 albedo, out half alpha, DECLARE_SAMPLER_2D_ARGS(albedoMap), DECLARE_SAMPLER_2D_ARGS(albedoMap1), DECLARE_SAMPLER_2D_ARGS(albedoMap2), DECLARE_SAMPLER_2D_ARGS(albedoMap3), float2 uv, half4 blendColor, float3 blendUV, half4 color)
	{
		half4 c0, c1, c2, c3;
		#ifdef MK_OUTLINE_PASS
			c0 = half4(color.rgb, SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap, uv, blendUV).a * color.a);
			c1 = half4(color.rgb, SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap1, uv, blendUV).a) * blendColor.g;
			c2 = half4(color.rgb, SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap2, uv, blendUV).a) * blendColor.b;
			c3 = half4(color.rgb, SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap3, uv, blendUV).a) * blendColor.a;
		#else
			c0 = SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap, uv, blendUV) * color;
			c1 = SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap1, uv, blendUV) * blendColor.y;
			c2 = SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap2, uv, blendUV) * blendColor.z;
			c3 = SAMPLE_SAMPLER2D_FLIPBOOK(albedoMap3, uv, blendUV) * blendColor.w;
		#endif

		half4 mixedColor = lerp(lerp(lerp(c0, c1, blendColor.g), c2, blendColor.b), c3, blendColor.a);

		albedo = mixedColor.rgb;
		#if defined(MK_ALPHA_LOOKUP)
			alpha = mixedColor.a;
		#else
			alpha = 1.0h;
		#endif
	}
	
	//Non texture color
	inline void SurfaceColor(out half3 albedo, out half alpha, half4 vertexColor, half4 color)
	{
		half4 c = vertexColor * color;
		albedo = c.rgb;
		#if defined(MK_ALPHA_LOOKUP)
			alpha = c.a;
		#else
			alpha = 1.0h;
		#endif
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// Initialize
	/////////////////////////////////////////////////////////////////////////////////////////////
	inline MKSurfaceData ComputeSurfaceData
	(
		in float4 svPositionClip
		//#ifdef MK_POS_WORLD
			, in float3 positionWorld
		//#endif
		#ifdef MK_FOG
			, in float fogFactor
		#endif
		#if defined(MK_TCM) || defined(MK_TCD)
			, in float4 baseUV
		#endif
		#ifdef MK_LIGHTMAP_UV
			, in float4 lightmapUV
		#endif
		#ifdef MK_VERTEX_COLOR_REQUIRED
			, in half4 vertexColor
		#endif
		#ifdef MK_NORMAL
			, in half3 normalWorld
		#endif
		#ifdef MK_VERTEX_LIGHTING
			, in half3 vertexLighting
		#endif
		#if defined(MK_TBN)
			, in half3 tangentWorld
		#endif
		#if defined(MK_PARALLAX)
			, in half3 viewTangent
		#endif
		#if defined(MK_TBN)
			, in half3 bitangentWorld
		#endif
		#ifdef MK_BARYCENTRIC_POS_CLIP
			, in float4 barycentricPositionClip
		#endif
		#ifdef MK_POS_NULL_CLIP
			, in float4 nullClip
		#endif
		#ifdef MK_FLIPBOOK
			, in float3 flipbookUV
		#endif
		#ifdef MK_VFACE
			, half vFace
		#endif
	)
	{
		MKSurfaceData surfaceData;
		INITIALIZE_STRUCT(MKSurfaceData, surfaceData);

		surfaceData.svPositionClip = svPositionClip;
		#ifdef MK_POS_WORLD
			surfaceData.positionWorld = positionWorld;
		#endif
		#ifdef MK_FOG
			surfaceData.fogFactor = fogFactor;
		#endif

		#if defined(MK_URP_2020_2_Or_Newer) && defined(MK_LIT)
			#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				surfaceData.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);
			#elif !defined (LIGHTMAP_ON)
				surfaceData.shadowMask = unity_ProbesOcclusion;
			#else
				surfaceData.shadowMask = half4(1, 1, 1, 1);
			#endif
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			surfaceData.vertexColor = vertexColor;
		#endif

		#if defined(MK_TCM)
			surfaceData.baseUV = 0;
		#endif
		#if defined(MK_TCD)
			surfaceData.detailUV = 0;
		#endif
		#if defined(MK_OCCLUSION_UV_SECOND)
			surfaceData.secondUV = 0;
		#endif
		#if defined(MK_TCM) || defined(MK_TCD)
			surfaceData.rawUV = 0;
		#endif

		#if defined(MK_TCM) || defined(MK_TCD)
			surfaceData.rawUV = baseUV.xy;
		#endif
		#if defined(MK_TCM)
			surfaceData.baseUV.xy = surfaceData.rawUV * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
		#endif
		#if defined(MK_TCM) && defined(MK_OCCLUSION_UV_SECOND)
			surfaceData.secondUV = baseUV.zw * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw;
		#endif
		#if defined(MK_TCD)
			surfaceData.detailUV = surfaceData.rawUV * _DetailMap_ST.xy + _DetailMap_ST.zw;
		#endif

		#ifdef MK_FLIPBOOK
			SURFACE_FLIPBOOK_UV = flipbookUV;
		#endif

		#ifdef MK_VD
			surfaceData.viewWorld = MKSafeNormalize(CAMERA_POSITION_WORLD - surfaceData.positionWorld);
		#endif

		#ifdef MK_VD_O
			surfaceData.viewTangent = half3(0, 0, 0);
		#endif

		#ifdef MK_PARALLAX
			surfaceData.viewTangent = MKSafeNormalize(viewTangent);
		#endif

		#ifdef MK_PARALLAX
			surfaceData.height = SAMPLE_TEX2D_FLIPBOOK(_HeightMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).r;
			float2 parallaxUVOffset = Parallax(surfaceData.viewTangent, surfaceData.height, _Parallax, 0.42);

			#if defined(MK_TCM)
				surfaceData.baseUV.xy += parallaxUVOffset;
			#endif
			#if defined(MK_TCD)
				surfaceData.detailUV += parallaxUVOffset;
			#endif
		#endif

		#ifdef MK_THRESHOLD_MAP
			surfaceData.thresholdUV = surfaceData.baseUV.xy * _ThresholdMapScale;
		#endif

		#if defined(MK_SCREEN_UV)
			surfaceData.screenUV = ComputeNDC(barycentricPositionClip);
		#endif

		#if defined(MK_SCREEN_SPACE_OCCLUSION) && defined(MK_URP_2020_2_Or_Newer)
			surfaceData.ambientOcclusion = GetScreenSpaceAmbientOcclusion(surfaceData.screenUV.xy);
		#endif

		#if defined(MK_ARTISTIC)
			#if defined(MK_ARTISTIC_PROJECTION_SCREEN_SPACE)
				#ifndef MK_LEGACY_SCREEN_SCALING
					half orthoViewScale = ScaleToFitOrthographicUV(barycentricPositionClip.w);
				#else
					half orthoViewScale = 1;
				#endif
			#endif
			#if defined(MK_ARTISTIC_DRAWN)
				#if defined(MK_ARTISTIC_PROJECTION_SCREEN_SPACE)
					surfaceData.artisticUV = orthoViewScale * ComputeNormalizedScreenUV(surfaceData.screenUV, ComputeNDC(nullClip), _DrawnMapScale);
				#else
					surfaceData.artisticUV = surfaceData.baseUV.xy * _DrawnMapScale;
				#endif
			#elif defined(MK_ARTISTIC_HATCHING)
				#if defined(MK_ARTISTIC_PROJECTION_SCREEN_SPACE)
					surfaceData.artisticUV = orthoViewScale * ComputeNormalizedScreenUV(surfaceData.screenUV, ComputeNDC(nullClip), _HatchingMapScale);
				#else
					surfaceData.artisticUV = surfaceData.baseUV.xy * _HatchingMapScale;
				#endif
			#elif defined(MK_ARTISTIC_SKETCH)
				#if defined(MK_ARTISTIC_PROJECTION_SCREEN_SPACE)
					surfaceData.artisticUV = orthoViewScale * ComputeNormalizedScreenUV(surfaceData.screenUV, ComputeNDC(nullClip), _SketchMapScale);
				#else
					surfaceData.artisticUV = surfaceData.baseUV.xy * _SketchMapScale;
				#endif
			#endif
			
			#ifdef MK_ARTISTIC_ANIMATION_STUTTER
				surfaceData.artisticUV.xy += Stutter(MK_TIME.y, _ArtisticFrequency);
			#endif
		#endif

		//dissolve could be moved above the screen uv to safe some instructions while clipping
		#ifdef MK_DISSOLVE
			float2 dissolveUV;
			#ifdef MK_DISSOLVE_PROJECTION_SCREEN_SPACE
				dissolveUV = ComputeNormalizedScreenUV(surfaceData.screenUV, ComputeNDC(nullClip), _DissolveMapScale);
			#else
				dissolveUV = surfaceData.baseUV.xy * _DissolveMapScale;
			#endif
			surfaceData.dissolveClip = SAMPLE_TEX2D_FLIPBOOK(_DissolveMap, SAMPLER_REPEAT_MAIN, dissolveUV, SURFACE_FLIPBOOK_UV).r - _DissolveAmount;
			Clip0(surfaceData.dissolveClip);
		#endif

		#ifdef MK_NORMAL
			surfaceData.vertexNormalWorld = MKSafeNormalize(normalWorld);
		#endif
		#ifdef MK_TBN
			surfaceData.tangentWorld = MKSafeNormalize(tangentWorld);
			surfaceData.bitangentWorld = MKSafeNormalize(bitangentWorld);
		#endif
		#if defined(MK_DEPTH_NORMALS_PASS) || defined(MK_LIT)
			//get normal direction
			#ifdef MK_TBN
				//Normalmap extraction
				#if defined(MK_NORMAL_MAP) && !defined(MK_DETAIL_NORMAL_MAP)
					surfaceData.normalWorld = NormalMappingWorld(PASS_TEXTURE_2D(_NormalMap, SAMPLER_REPEAT_MAIN), surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV, _NormalMapIntensity, half3x3(surfaceData.tangentWorld, surfaceData.bitangentWorld, surfaceData.vertexNormalWorld));
				#elif defined(MK_NORMAL_MAP) && defined(MK_DETAIL_NORMAL_MAP)
					surfaceData.normalWorld = NormalMappingWorld(PASS_TEXTURE_2D(_NormalMap, SAMPLER_REPEAT_MAIN), surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV, _NormalMapIntensity, PASS_TEXTURE_2D(_DetailNormalMap, SAMPLER_REPEAT_MAIN), surfaceData.detailUV, _DetailNormalMapIntensity, half3x3(surfaceData.tangentWorld, surfaceData.bitangentWorld, surfaceData.vertexNormalWorld));
				#elif !defined(MK_NORMAL_MAP) && defined(MK_DETAIL_NORMAL_MAP)
					surfaceData.normalWorld = NormalMappingWorld(PASS_TEXTURE_2D(_DetailNormalMap, SAMPLER_REPEAT_MAIN), surfaceData.detailUV, SURFACE_FLIPBOOK_UV, _DetailNormalMapIntensity, half3x3(surfaceData.tangentWorld, surfaceData.bitangentWorld, surfaceData.vertexNormalWorld));
				#else
					surfaceData.normalWorld = surfaceData.vertexNormalWorld;
				#endif

				#ifdef MK_SPECULAR_ANISOTROPIC
					//tangent for normal mapping has to be perpendicular
					//recalculate tangent for aniso orthonormal
					surfaceData.bitangentWorld = ComputeBitangentWorld(surfaceData.normalWorld, surfaceData.tangentWorld, 1.0);
					surfaceData.tangentWorld = MKSafeNormalize(cross(surfaceData.bitangentWorld, surfaceData.normalWorld));
				#endif
			#else
				surfaceData.normalWorld = surfaceData.vertexNormalWorld;
			#endif

			#ifdef MK_VERTEX_LIGHTING
				surfaceData.vertexLighting = vertexLighting;
			#endif
			#ifdef MK_LIGHTMAP_UV
				surfaceData.lightmapUV = lightmapUV;
			#endif
		#endif

		#if defined(MK_REFRACTION)
			#ifdef MK_INDEX_OF_REFRACTION
				float3 ssY = float3(UNITY_MATRIX_V[1][0], UNITY_MATRIX_V[1][1], UNITY_MATRIX_V[1][2]);
				float3 ssX = normalize( cross(surfaceData.viewWorld, ssY));

				float4x4 ssView = float4x4
				(
					ssX.x, ssX.y, ssX.z, 0,
					ssY.x, ssY.y, ssY.z, 0,
					surfaceData.viewWorld, 0,
					0,0,0,1
				);

				float2 ssIOR = mul(ssView, float4(surfaceData.vertexNormalWorld, 0.0)).xy;
				ssIOR.x *= SafeDivide(_ScreenParams.y, _ScreenParams.x);
				ssIOR *= (1.0 - saturate(dot(surfaceData.vertexNormalWorld, surfaceData.viewWorld))) * _IndexOfRefraction;
				surfaceData.refractionUV = surfaceData.screenUV.xy - ssIOR;
			#else
				surfaceData.refractionUV = surfaceData.screenUV.xy;
			#endif
		#endif

		#ifndef _DBUFFER
			#ifdef MK_V_DOT_N
				surfaceData.VoN = saturate(dot(surfaceData.viewWorld, surfaceData.normalWorld));
				surfaceData.OneMinusVoN = 1.0 - surfaceData.VoN;
			#endif
			#ifdef MK_MV_REF_N
				surfaceData.MVrN = reflect(-surfaceData.viewWorld, surfaceData.normalWorld);
			#endif
		#else
			#ifdef MK_V_DOT_N
				surfaceData.VoN = 0;
				surfaceData.OneMinusVoN = 0;
			#endif
			#ifdef MK_MV_REF_N
				surfaceData.MVrN = 0;
			#endif
		#endif

		#ifdef MK_VFACE
			surfaceData.vFace = vFace;
		#endif
		return surfaceData;
	}

	inline void ComputeBlending
	(
		inout Surface surface
		, in MKSurfaceData surfaceData
		#ifdef MK_PBS
	 		, inout MKPBSData pbsData
		#endif
	)
	{
		#if defined(MK_BLEND_PREMULTIPLY) || defined(MK_BLEND_ADDITIVE)
			#if defined(MK_PBS)
				pbsData.diffuseRadiance *= surface.alpha;
				half premulGISpec;
				#ifdef MK_FRESNEL_HIGHLIGHTS
					premulGISpec = dot(pbsData.fresnel, REL_LUMA);
				#else
					premulGISpec = dot(pbsData.specularRadiance, REL_LUMA);
				#endif
				surface.alpha = surface.alpha * pbsData.oneMinusReflectivity + premulGISpec;
			#else
				surface.albedo *= surface.alpha;
			#endif
		#elif defined(MK_BLEND_MULTIPLY)
			#if defined(MK_PBS)
				surface.albedo = lerp(HALF3_ONE, surface.albedo, surface.alpha);
				pbsData.diffuseRadiance = lerp(HALF3_ONE, pbsData.diffuseRadiance, surface.alpha * pbsData.oneMinusReflectivity + pbsData.reflectivity);
			#else
				surface.albedo = lerp(HALF3_ONE, surface.albedo, surface.alpha);
			#endif
		#endif

		#ifdef MK_SOFT_FADE
			#if defined(MK_BLEND_PREMULTIPLY) || defined(MK_BLEND_ADDITIVE)
				surface.albedo *= SoftFade(_SoftFadeNearDistance, _SoftFadeFarDistance, surfaceData.svPositionClip, surfaceData.screenUV);
			#else
				surface.alpha *= SoftFade(_SoftFadeNearDistance, _SoftFadeFarDistance, surfaceData.svPositionClip, surfaceData.screenUV);
			#endif
		#endif

		#ifdef MK_CAMERA_FADE
			#if defined(MK_BLEND_PREMULTIPLY) || defined(MK_BLEND_ADDITIVE)
				surface.albedo *= CameraFade(_CameraFadeNearDistance, _CameraFadeFarDistance, surfaceData.screenUV);
			#else
				surface.alpha *= CameraFade(_CameraFadeNearDistance, _CameraFadeFarDistance, surfaceData.screenUV);
			#endif
		#endif

		#ifdef MK_LIT
			#if defined(MK_ALPHA_LOOKUP)
				surface.goochBright.a *= surface.alpha;
				surface.goochDark.a *= surface.alpha;
			#endif
		#endif
	}

	inline MKPBSData ComputePBSData(inout Surface surface, inout MKSurfaceData surfaceData)
	{
		MKPBSData pbsData;
		INITIALIZE_STRUCT(MKPBSData, pbsData);

		half4 pbsInput;
		
		#if defined(MK_WORKFLOW_METALLIC)
			#ifdef MK_PBS_MAP_0
				pbsInput = SAMPLE_TEX2D_FLIPBOOK(_MetallicMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).rrra;
				pbsInput.a *= _Smoothness;
			#else
				pbsInput.rgb = _Metallic;
				pbsInput.a = _Smoothness;
			#endif
		#elif defined(MK_WORKFLOW_ROUGHNESS)
			#ifdef MK_PBS_MAP_0
				pbsInput.rgb = SAMPLE_TEX2D_FLIPBOOK(_MetallicMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).rrr;
			#else
				pbsInput.rgb = _Metallic;
			#endif
			#ifdef MK_PBS_MAP_1
				pbsInput.a = 1.0 - SAMPLE_TEX2D_FLIPBOOK(_RoughnessMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).r;
			#else
				pbsInput.a = 1.0 - _Roughness;
			#endif
		#else //MK_WORKFLOW_SPECULAR / Simple
			#ifdef MK_PBS_MAP_0
				pbsInput = SAMPLE_TEX2D_FLIPBOOK(_SpecularMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV);
				pbsInput.a *= _Smoothness;
			#else
				pbsInput.rgb = _SpecularColor.rgb;
				pbsInput.a = _Smoothness;
			#endif
		#endif

		#if defined(MK_WORKFLOW_METALLIC)
			pbsData.smoothness = pbsInput.a;
			pbsData.metallic = pbsInput.r;
			pbsData.specular = 0;
		#elif defined(MK_WORKFLOW_ROUGHNESS)
			pbsData.smoothness = pbsInput.a;
			pbsData.metallic = pbsInput.r;
			pbsData.specular = 0;
		#elif defined(MK_WORKFLOW_SPECULAR)
			pbsData.smoothness = pbsInput.a;
			pbsData.metallic = 0;
			pbsData.specular = pbsInput.rgb;
		#else //Simple
			pbsData.metallic = 0;
			pbsData.specular = pbsInput.rgb;
			pbsData.smoothness = pbsInput.a;
		#endif

		#ifdef _DBUFFER
			#ifdef MK_LIT
				#if defined(MK_WORKFLOW_SPECULAR) || defined(MK_SIMPLE)
					half metallic = 0;
					ApplyDecal
					(
						surfaceData.svPositionClip,
						surface.albedo,
						pbsData.specular,
						surfaceData.normalWorld,
						metallic,
						surface.occlusion.r,
						pbsData.smoothness
					);
				#else
					half3 specular = 0;
					ApplyDecal
					(	
						surfaceData.svPositionClip,
						surface.albedo,
						specular,
						surfaceData.normalWorld,
						pbsData.metallic,
						surface.occlusion.r,
						pbsData.smoothness
					);
				#endif

				#ifdef MK_V_DOT_N
					surfaceData.VoN = saturate(dot(surfaceData.viewWorld, surfaceData.normalWorld));
					surfaceData.OneMinusVoN = 1.0 - surfaceData.VoN;
				#endif
				#ifdef MK_MV_REF_N
					surfaceData.MVrN = reflect(-surfaceData.viewWorld, surfaceData.normalWorld);
				#endif
			#else
				ApplyDecalToBaseColor(surfaceData.svPositionClip, surface.albedo);
			#endif
		#endif

		#if defined(MK_WORKFLOW_METALLIC)
			pbsData.oneMinusReflectivity = K_SPEC_DIELECTRIC_MAX - pbsData.metallic * K_SPEC_DIELECTRIC_MAX;
			pbsData.reflectivity = 1.0 - pbsData.oneMinusReflectivity;
			pbsData.specularRadiance = lerp(K_SPEC_DIELECTRIC_MIN, surface.albedo, pbsData.metallic);
			pbsData.diffuseRadiance = surface.albedo * (pbsData.oneMinusReflectivity * (1.0 - pbsData.specularRadiance));//surface.albedo * pbsData.oneMinusReflectivity;
		#elif defined(MK_WORKFLOW_ROUGHNESS)
			pbsData.oneMinusReflectivity = K_SPEC_DIELECTRIC_MAX - pbsData.metallic * K_SPEC_DIELECTRIC_MAX;
			pbsData.reflectivity = 1.0 - pbsData.oneMinusReflectivity;
			pbsData.specularRadiance = lerp(K_SPEC_DIELECTRIC_MIN, surface.albedo, pbsData.metallic);
			pbsData.diffuseRadiance = surface.albedo * ((1.0 - pbsData.specularRadiance) * pbsData.oneMinusReflectivity);
		#elif defined(MK_WORKFLOW_SPECULAR)
			pbsData.reflectivity = max(max(pbsData.specular.r, pbsData.specular.g), pbsData.specular.b);
			pbsData.oneMinusReflectivity = 1.0 - pbsData.reflectivity;
			pbsData.specularRadiance = pbsData.specular.rgb;
			pbsData.diffuseRadiance = surface.albedo * (half3(1.0, 1.0, 1.0) - pbsData.specular.rgb);
		#else //Simple
			pbsData.reflectivity = 0;
			pbsData.oneMinusReflectivity = 1.0 - pbsData.reflectivity;
			pbsData.diffuseRadiance = surface.albedo;
			pbsData.specularRadiance = pbsData.specular.rgb;
		#endif

		pbsData.roughness = 1.0 - pbsData.smoothness;
		#if defined(MK_ENVIRONMENT_REFLECTIONS) || defined(MK_SPECULAR) || defined(MK_DIFFUSE_OREN_NAYAR) || defined(MK_DIFFUSE_MINNAERT)
			pbsData.roughnessPow2 = FastPow2(pbsData.roughness);
		#endif
		#if defined(MK_ENVIRONMENT_REFLECTIONS) || defined(MK_SPECULAR)
			pbsData.roughnessPow4 = FastPow2(pbsData.roughnessPow2);
		#endif

		#ifdef MK_FRESNEL_HIGHLIGHTS
			pbsData.fresnel = FresnelSchlickGGXIBL(surfaceData.OneMinusVoN, pbsData.specularRadiance, pbsData.smoothness);
		#endif

		#ifdef MK_PBS
			ComputeBlending(PASS_BLENDING_ARG(surface, surfaceData, pbsData));
		#endif

		return pbsData;
	}

	inline Surface InitSurface(inout MKSurfaceData surfaceData, DECLARE_SAMPLER_2D_ARGS(samp2D), in half4 albedoTint, in float4 pC)
	{
		//Init Surface
		Surface surface;
		INITIALIZE_STRUCT(Surface, surface);

		#ifdef MK_ARTISTIC
			#if defined(MK_ARTISTIC_DRAWN)
				surface.artistic0 = 1.0 - SampleTex2D(PASS_TEXTURE_2D(_DrawnMap, SAMPLER_REPEAT_MAIN), surfaceData.artisticUV).r;
			#elif defined(MK_ARTISTIC_SKETCH)
				surface.artistic0 = SampleTex2D(PASS_TEXTURE_2D(_SketchMap, SAMPLER_REPEAT_MAIN), surfaceData.artisticUV).r;
			#elif defined(MK_ARTISTIC_HATCHING)
				surface.artistic0 = SampleTex2D(PASS_TEXTURE_2D(_HatchingDarkMap, SAMPLER_REPEAT_MAIN), surfaceData.artisticUV).rgb;
				surface.artistic1 = SampleTex2D(PASS_TEXTURE_2D(_HatchingBrightMap, SAMPLER_REPEAT_MAIN), surfaceData.artisticUV).rgb;
			#endif
		#endif

		//init surface color
		#if defined(MK_TEXCLR)
			#if defined(MK_PARTICLES) || defined(MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP)
				albedoTint *= surfaceData.vertexColor;
			#endif
			SurfaceColor(surface.albedo, surface.alpha, PASS_SAMPLER_2D(_AlbedoMap), surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV, albedoTint);
		#elif defined(MK_POLYBRUSH)
			SurfaceColor(surface.albedo, surface.alpha, PASS_SAMPLER_2D(_AlbedoMap), PASS_SAMPLER_2D(_AlbedoMap1), PASS_SAMPLER_2D(_AlbedoMap2), PASS_SAMPLER_2D(_AlbedoMap3), surfaceData.baseUV.xy, surfaceData.vertexColor, SURFACE_FLIPBOOK_UV, albedoTint);
		#else
			SurfaceColor(surface.albedo, surface.alpha, surfaceData.vertexColor, albedoTint);
		#endif

		//add detail
		#ifdef MK_DETAIL_MAP
			surface.detail = SAMPLE_TEX2D_FLIPBOOK(_DetailMap, SAMPLER_REPEAT_MAIN, surfaceData.detailUV, SURFACE_FLIPBOOK_UV);
			surface.detail.rgb *= _DetailColor.rgb;
			MixAlbedoDetail(surface.albedo, surface.detail);
		#endif

		#if defined(MK_ALPHA_CLIPPING)
			Clip0(surface.alpha - _AlphaCutoff);
		#endif

		#ifdef MK_REFRACTION
			half2 refractionDir;

			#ifdef MK_REFRACTION_DISTORTION_MAP
				refractionDir = (_RefractionDistortion * REFRACTION_DISTORTION_SCALE * surface.alpha) * UnpackDudv(PASS_TEXTURE_2D(_RefractionDistortionMap, SAMPLER_REPEAT_MAIN), surfaceData.baseUV.xy * _RefractionDistortionMapScale, SURFACE_FLIPBOOK_UV);
			#else
				//currently disabled if no normal mapping is set, could be optimized using a procedural noise
				refractionDir = 0;
			#endif
			
			surface.refraction = SampleRefraction(surfaceData.refractionUV + refractionDir);
			surface.albedo.rgb = lerp(surface.refraction.rgb, surface.albedo.rgb, saturate(surface.alpha - _RefractionDistortionFade));
		#endif

		#ifdef MK_COLOR_GRADING_ALBEDO
			surface.albedo = ColorGrading(surface.albedo, _Brightness, _Saturation, _Contrast);
		#endif

		#ifdef MK_LIT
			surface.indirectFade = _IndirectFade;
			#ifdef MK_THRESHOLD_MAP
				surface.thresholdOffset = SAMPLE_TEX2D_FLIPBOOK(_ThresholdMap, SAMPLER_REPEAT_MAIN, surfaceData.thresholdUV, SURFACE_FLIPBOOK_UV).r;
			#endif
			surface.direct = 0;

			surface.goochBright = _GoochBrightColor;
			#ifdef MK_LEGACY_RP
				#ifdef USING_DIRECTIONAL_LIGHT
					surface.goochDark = _GoochDarkColor;
				#else
					surface.goochDark = 0;
				#endif
			#else
				surface.goochDark = _GoochDarkColor;
			#endif

			#if defined(MK_GOOCH_BRIGHT_MAP)
				surface.goochBright *= SAMPLE_TEX2D_FLIPBOOK(_GoochBrightMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV);
			#endif
			#if defined(MK_GOOCH_DARK_MAP)
				#ifdef MK_LEGACY_RP
					#ifdef USING_DIRECTIONAL_LIGHT
						surface.goochDark *= SAMPLE_TEX2D_FLIPBOOK(_GoochDarkMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV);
					#endif
				#else
					surface.goochDark *= SAMPLE_TEX2D_FLIPBOOK(_GoochDarkMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV);
				#endif
			#endif

			#ifdef MK_INDIRECT
				surface.indirect = 0;
			#endif
			#ifdef MK_THICKNESS_MAP
				surface.thickness = SAMPLE_TEX2D_FLIPBOOK(_ThicknessMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).r;
			#endif
			#if defined(MK_RIM)
				surface.rim = 0;
			#endif

			#ifdef MK_IRIDESCENCE
				surface.iridescence = 0;
			#endif

			#ifdef MK_OCCLUSION_MAP
				float2 occlusionUV;
				#if defined(MK_TCM) && defined(MK_OCCLUSION_UV_SECOND)
					occlusionUV = surfaceData.secondUV;
				#else
					occlusionUV = surfaceData.baseUV;
				#endif
				surface.occlusion = (1.0 - _OcclusionMapIntensity) + SAMPLE_TEX2D_FLIPBOOK(_OcclusionMap, SAMPLER_REPEAT_MAIN, occlusionUV, SURFACE_FLIPBOOK_UV).rg * _OcclusionMapIntensity;
			#else
				surface.occlusion = 1.0;
			#endif

			#ifdef MK_EMISSION
				#if defined(MK_EMISSION_MAP)
					surface.emission = _EmissionColor * SAMPLE_TEX2D_FLIPBOOK(_EmissionMap, SAMPLER_REPEAT_MAIN, surfaceData.baseUV.xy, SURFACE_FLIPBOOK_UV).rgb;
				#else
					surface.emission = _EmissionColor;
				#endif
			#endif
		#endif

		#ifdef MK_COLOR
			#if defined(MK_COLOR_BLEND_ADDITIVE)
				surface.albedo = surface.albedo + surfaceData.vertexColor.rgb;
				surface.alpha *= surfaceData.vertexColor.a;
			#elif defined(MK_COLOR_BLEND_SUBTRACTIVE)
				surface.albedo = surface.albedo + surfaceData.vertexColor * (-1.0h);
				surface.alpha *= surfaceData.vertexColor.a;
			#elif defined(MK_COLOR_BLEND_OVERLAY)
				surface.albedo = lerp(1 - 2 * (1 - surface.albedo) * (1 - surface.albedo), 2 * surface.albedo * surfaceData.vertexColor.rgb, step(surface.albedo, 0.5));
				surface.alpha *= surfaceData.vertexColor.a;
			#elif defined(MK_COLOR_BLEND_COLOR)
				half3 aHSL = RGBToHSV(surface.albedo);
				half3 bHSL = RGBToHSV(surfaceData.vertexColor.rgb);
				half3 rHSL = half3(bHSL.x, bHSL.y, aHSL.z);
				surface.albedo = HSVToRGB(rHSL);
				surface.alpha = surface.alpha * surfaceData.vertexColor.a;
			#elif defined(MK_COLOR_BLEND_DIFFERENCE)
				surface.albedo =  abs(surface.albedo + surfaceData.vertexColor * (-1.0h));
				surface.alpha *= surfaceData.vertexColor.a;
			#else
				surface.albedo *= surfaceData.vertexColor.rgb;
				surface.alpha *= surfaceData.vertexColor.a;
			#endif
		#endif

		//avoid not initialized value
		surface.final = 0;

		#ifndef MK_PBS
			ComputeBlending(PASS_BLENDING_ARG(surface, surfaceData, 0));
		#endif

		return surface;
	}
#endif