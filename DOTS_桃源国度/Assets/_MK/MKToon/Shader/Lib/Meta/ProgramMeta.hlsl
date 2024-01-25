//////////////////////////////////////////////////////
// MK Toon Meta Program			       				//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#ifndef MK_TOON_META
	#define MK_TOON_META

	#if defined(MK_URP)
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
	#elif defined(MK_LWRP)
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
	#else
		#include "UnityMetaPass.cginc"
	#endif
	
	#include "../Core.hlsl"
	#include "Common.hlsl"
	#include "Data.hlsl"
	#include "../Surface.hlsl"

	/////////////////////////////////////////////////////////////////////////////////////////////
	// VERTEX SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	VertexOutputMeta MetaVert(VertexInputMeta vertexInput)
	{
		VertexOutputMeta vertexOutput;
		INITIALIZE_STRUCT(VertexOutputMeta, vertexOutput);
		//vertexposition
		vertexOutput.svPositionClip = ComputeMetaPosition(vertexInput.vertex, vertexInput.staticLightmapUV.xy, vertexInput.dynamicLightmapUV.xy, unity_LightmapST, unity_DynamicLightmapST);
		
		//texcoords
		#if defined(MK_TCM) || defined(MK_TCD)
			vertexOutput.uv = 0;
		#endif
		#if defined(MK_TCM)
			vertexOutput.uv.xy = vertexInput.texcoord0.xy;
		#endif
		#if defined(MK_TCD)
			#if defined(MK_OCCLUSION_UV_SECOND)
				vertexOutput.uv.zw = vertexInput.texcoord0.zw;
			#else
				vertexOutput.uv.zw = 0;
			#endif
		#endif

		#ifdef MK_VERTEX_COLOR_REQUIRED
			vertexOutput.color = vertexInput.color;
		#endif

		#ifdef EDITOR_VISUALIZATION
			vertexOutput.vizUV = 0;
			vertexOutput.lightCoords = 0;
			if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
				vertexOutput.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, vertexInput.texcoord0.xy, vertexInput.staticLightmapUV.xy, vertexInput.dynamicLightmapUV.xy, unity_EditorViz_Texture_ST);
			else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
			{
				vertexOutput.vizUV = vertexInput.staticLightmapUV.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				vertexOutput.lightCoords = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(vertexInput.vertex.xyz, 1)));
			}
		#endif

		#if defined(MK_PARALLAX)
			vertexOutput.viewTangent = ComputeViewTangent(ComputeViewObject(vertexInput.vertex.xyz), vertexInput.normal, vertexInput.tangent.xyz, cross(vertexInput.normal, vertexInput.tangent.xyz) * vertexInput.tangent.w * unity_WorldTransformParams.w);
		#endif

		return vertexOutput;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// FRAGMENT SHADER
	/////////////////////////////////////////////////////////////////////////////////////////////
	half4 MetaFrag (VertexOutputMeta vertexOutput) : SV_Target
	{
		MKMetaData mkMetaData;
		INITIALIZE_STRUCT(MKMetaData, mkMetaData);

		MKSurfaceData surfaceData = ComputeSurfaceData
		(
			vertexOutput.svPositionClip,
			PASS_POSITION_WORLD_ARG(0)
			PASS_FOG_FACTOR_WORLD_ARG(0)
			PASS_BASE_UV_ARG(vertexOutput.uv)
			PASS_LIGHTMAP_UV_ARG(0)
			PASS_VERTEX_COLOR_ARG(vertexOutput.color)
			PASS_NORMAL_WORLD_ARG(1)
			PASS_VERTEX_LIGHTING_ARG(0)
			PASS_TANGENT_WORLD_ARG(1)
			PASS_VIEW_TANGENT_ARG(vertexOutput.viewTangent)
			PASS_BITANGENT_WORLD_ARG(1)
			PASS_BARYCENTRIC_POSITION_CLIP_ARG(0)
			PASS_NULL_CLIP_ARG(0)
			PASS_FLIPBOOK_UV_ARG(0)
		);
		Surface surface = InitSurface(surfaceData, PASS_SAMPLER_2D(_AlbedoMap), _AlbedoColor, vertexOutput.svPositionClip);

		#ifdef MK_LIT
			MKPBSData mkPBSData;
			mkPBSData = ComputePBSData(surface, surfaceData);

			#ifdef EDITOR_VISUALIZATION
				mkMetaData.albedo = mkPBSData.diffuseRadiance;
				mkMetaData.vizUV = vertexOutput.vizUV;
				mkMetaData.lightCoords = vertexOutput.lightCoords;
			#else
				mkMetaData.albedo = mkPBSData.diffuseRadiance;
			#endif
			mkMetaData.specular = mkPBSData.specularRadiance * mkPBSData.roughness * 0.5;
		#else
			#ifdef EDITOR_VISUALIZATION
				mkMetaData.albedo = surface.albedo;
				mkMetaData.vizUV = vertexOutput.vizUV;
				mkMetaData.lightCoords = vertexOutput.lightCoords;
			#else
				mkMetaData.albedo = surface.albedo;
			#endif
			mkMetaData.specular = 0;
		#endif
		
		#ifdef MK_EMISSION
			#if defined(MK_EMISSION_MAP)
				mkMetaData.emission = _EmissionColor * SampleTex2D(PASS_TEXTURE_2D(_EmissionMap, SAMPLER_REPEAT_MAIN), vertexOutput.uv).rgb;
			#else
				mkMetaData.emission = _EmissionColor;
			#endif
		#else
			mkMetaData.emission = 0;
		#endif

		return ComputeMetaOutput(mkMetaData);
	}
#endif