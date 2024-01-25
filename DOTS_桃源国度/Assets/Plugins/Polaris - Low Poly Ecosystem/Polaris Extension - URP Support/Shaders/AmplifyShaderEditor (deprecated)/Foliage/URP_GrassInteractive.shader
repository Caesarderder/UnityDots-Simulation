// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polaris/Deprecated/URP/Foliage/GrassInteractive"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_NoiseTex("_NoiseTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Cutoff("Cutoff", Range( 0 , 1)) = 0.5
		_Occlusion("Occlusion", Range( 0 , 1)) = 0.2
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.25
		[HideInInspector]_BendFactor("BendFactor", Float) = 1
		_WaveDistance("WaveDistance", Float) = 0.1
		_Wind("Wind", Vector) = (1,1,4,8)
		_VectorField("VectorField", 2D) = "gray" {}

	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero , One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#define ASE_NEEDS_VERT_POSITION


			sampler2D _NoiseTex;
			sampler2D _VectorField;
			float4x4 _WorldToNormalized;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Wind;
			float _WaveDistance;
			float _BendFactor;
			float4 _Color;
			float4 _MainTex_ST;
			float _Occlusion;
			float _Smoothness;
			float _Cutoff;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 _VertexPos3_g6 = v.vertex;
				float4 transform15_g6 = mul(GetObjectToWorldMatrix(),_VertexPos3_g6);
				float2 appendResult22_g6 = (float2(transform15_g6.x , transform15_g6.z));
				float2 worldPosXZ21_g6 = appendResult22_g6;
				float _WindDirX11 = _Wind.x;
				float _WindDirX5_g6 = _WindDirX11;
				float _WindDirY12 = _Wind.y;
				float _WindDirY7_g6 = _WindDirY12;
				float2 appendResult19_g6 = (float2(_WindDirX5_g6 , _WindDirY7_g6));
				float _WindSpeed13 = _Wind.z;
				float _WindSpeed9_g6 = _WindSpeed13;
				float _WindSpread14 = _Wind.w;
				float _WindSpread10_g6 = _WindSpread14;
				float2 noisePos32_g6 = ( ( worldPosXZ21_g6 - ( appendResult19_g6 * _WindSpeed9_g6 * _TimeParameters.x ) ) / _WindSpread10_g6 );
				float2 uv059_g6 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_35_0_g6 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g6, 0, 0.0) ).r * uv059_g6.y );
				float _WaveDistance9 = _WaveDistance;
				float _WaveDistance12_g6 = _WaveDistance9;
				float _BendFactor71 = _BendFactor;
				float _BendFactor38_g6 = _BendFactor71;
				float4 appendResult42_g6 = (float4(_WindDirX5_g6 , ( temp_output_35_0_g6 * 0.5 ) , _WindDirY7_g6 , 0.0));
				float4 transform47_g6 = mul(GetWorldToObjectMatrix(),( temp_output_35_0_g6 * _WaveDistance12_g6 * _BendFactor38_g6 * appendResult42_g6 ));
				float4 vertexOffset48_g6 = transform47_g6;
				float4 windVertexOffset62 = vertexOffset48_g6;
				float4x4 myLocalVar55_g7 = _WorldToNormalized;
				float4x4 _WorldToNormalized8_g7 = myLocalVar55_g7;
				float4 _VertexPos3_g7 = v.vertex;
				float4 transform12_g7 = mul(GetObjectToWorldMatrix(),_VertexPos3_g7);
				float4 break28_g7 = mul( _WorldToNormalized8_g7, transform12_g7 );
				float4 appendResult29_g7 = (float4(break28_g7.x , break28_g7.z , 0.0 , 0.0));
				float4 vectorFieldUV30_g7 = appendResult29_g7;
				float4 bendVector33_g7 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g7.xy, 0, 0.0) );
				float4 break36_g7 = bendVector33_g7;
				float4 appendResult43_g7 = (float4(( ( break36_g7.r * 2.0 ) - 1.0 ) , ( ( break36_g7.g * 2.0 ) - 1.0 ) , ( ( break36_g7.b * 2.0 ) - 1.0 ) , 0.0));
				float4 remappedBendVector44_g7 = appendResult43_g7;
				float2 uv056_g7 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float _BendFactor51_g7 = _BendFactor71;
				float4 vertexOffset52_g7 = ( remappedBendVector44_g7 * uv056_g7.y * _BendFactor51_g7 );
				float4 touchBendingOffset83 = vertexOffset52_g7;
				float4 break87 = ( windVertexOffset62 + touchBendingOffset83 );
				float4 appendResult88 = (float4(break87.x , max( break87.y , 0.0 ) , break87.z , break87.w));
				float4 totalVertexOffset93 = appendResult88;
				
				float3 vertexNormal55 = float3(0,1,0);
				
				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = totalVertexOffset93.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = vertexNormal55;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				float3 WorldNormal = normalize( IN.tSpace0.xyz );
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				#if SHADER_HINT_NICE_QUALITY
					WorldViewDirection = SafeNormalize( WorldViewDirection );
				#endif

				float4 _Color5 = _Color;
				float2 uv0_MainTex = IN.ase_texcoord7.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv0_MainTex ) );
				float _Occlusion18 = _Occlusion;
				float2 uv026 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult33 = lerp( 0.0 , _Occlusion18 , ( ( 1.0 - uv026.y ) * ( 1.0 - uv026.y ) ));
				float4 albedoColor40 = ( temp_output_24_0 - float4( ( 0.5 * float3(1,1,1) * lerpResult33 ) , 0.0 ) );
				
				float _Smoothness49 = _Smoothness;
				
				float alpha45 = temp_output_24_0.a;
				
				float _Cutoff16 = _Cutoff;
				
				float3 Albedo = albedoColor40.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = _Smoothness49;
				float Occlusion = 1;
				float Alpha = alpha45;
				float AlphaClipThreshold = _Cutoff16;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				
				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal )));
				#else
					#if !SHADER_HINT_NICE_QUALITY
						inputData.normalWS = WorldNormal;
					#else
						inputData.normalWS = normalize( WorldNormal );
					#endif
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 / ( ScreenPos.z + 1.0 ) ) * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					float2 cameraRefraction = float2( refractionOffset.x, -( refractionOffset.y * _ProjectionParams.x ) );
					projScreenPos.xy += cameraRefraction;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _NoiseTex;
			sampler2D _VectorField;
			float4x4 _WorldToNormalized;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Wind;
			float _WaveDistance;
			float _BendFactor;
			float4 _Color;
			float4 _MainTex_ST;
			float _Occlusion;
			float _Smoothness;
			float _Cutoff;
			CBUFFER_END


			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			float3 _LightDirection;

			VertexOutput ShadowPassVertex( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 _VertexPos3_g6 = v.vertex;
				float4 transform15_g6 = mul(GetObjectToWorldMatrix(),_VertexPos3_g6);
				float2 appendResult22_g6 = (float2(transform15_g6.x , transform15_g6.z));
				float2 worldPosXZ21_g6 = appendResult22_g6;
				float _WindDirX11 = _Wind.x;
				float _WindDirX5_g6 = _WindDirX11;
				float _WindDirY12 = _Wind.y;
				float _WindDirY7_g6 = _WindDirY12;
				float2 appendResult19_g6 = (float2(_WindDirX5_g6 , _WindDirY7_g6));
				float _WindSpeed13 = _Wind.z;
				float _WindSpeed9_g6 = _WindSpeed13;
				float _WindSpread14 = _Wind.w;
				float _WindSpread10_g6 = _WindSpread14;
				float2 noisePos32_g6 = ( ( worldPosXZ21_g6 - ( appendResult19_g6 * _WindSpeed9_g6 * _TimeParameters.x ) ) / _WindSpread10_g6 );
				float2 uv059_g6 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_35_0_g6 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g6, 0, 0.0) ).r * uv059_g6.y );
				float _WaveDistance9 = _WaveDistance;
				float _WaveDistance12_g6 = _WaveDistance9;
				float _BendFactor71 = _BendFactor;
				float _BendFactor38_g6 = _BendFactor71;
				float4 appendResult42_g6 = (float4(_WindDirX5_g6 , ( temp_output_35_0_g6 * 0.5 ) , _WindDirY7_g6 , 0.0));
				float4 transform47_g6 = mul(GetWorldToObjectMatrix(),( temp_output_35_0_g6 * _WaveDistance12_g6 * _BendFactor38_g6 * appendResult42_g6 ));
				float4 vertexOffset48_g6 = transform47_g6;
				float4 windVertexOffset62 = vertexOffset48_g6;
				float4x4 myLocalVar55_g7 = _WorldToNormalized;
				float4x4 _WorldToNormalized8_g7 = myLocalVar55_g7;
				float4 _VertexPos3_g7 = v.vertex;
				float4 transform12_g7 = mul(GetObjectToWorldMatrix(),_VertexPos3_g7);
				float4 break28_g7 = mul( _WorldToNormalized8_g7, transform12_g7 );
				float4 appendResult29_g7 = (float4(break28_g7.x , break28_g7.z , 0.0 , 0.0));
				float4 vectorFieldUV30_g7 = appendResult29_g7;
				float4 bendVector33_g7 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g7.xy, 0, 0.0) );
				float4 break36_g7 = bendVector33_g7;
				float4 appendResult43_g7 = (float4(( ( break36_g7.r * 2.0 ) - 1.0 ) , ( ( break36_g7.g * 2.0 ) - 1.0 ) , ( ( break36_g7.b * 2.0 ) - 1.0 ) , 0.0));
				float4 remappedBendVector44_g7 = appendResult43_g7;
				float2 uv056_g7 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float _BendFactor51_g7 = _BendFactor71;
				float4 vertexOffset52_g7 = ( remappedBendVector44_g7 * uv056_g7.y * _BendFactor51_g7 );
				float4 touchBendingOffset83 = vertexOffset52_g7;
				float4 break87 = ( windVertexOffset62 + touchBendingOffset83 );
				float4 appendResult88 = (float4(break87.x , max( break87.y , 0.0 ) , break87.z , break87.w));
				float4 totalVertexOffset93 = appendResult88;
				
				float3 vertexNormal55 = float3(0,1,0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = totalVertexOffset93.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = vertexNormal55;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			half4 ShadowPassFragment(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 _Color5 = _Color;
				float2 uv0_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv0_MainTex ) );
				float alpha45 = temp_output_24_0.a;
				
				float _Cutoff16 = _Cutoff;
				
				float Alpha = alpha45;
				float AlphaClipThreshold = _Cutoff16;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			sampler2D _NoiseTex;
			sampler2D _VectorField;
			float4x4 _WorldToNormalized;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Wind;
			float _WaveDistance;
			float _BendFactor;
			float4 _Color;
			float4 _MainTex_ST;
			float _Occlusion;
			float _Smoothness;
			float _Cutoff;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 _VertexPos3_g6 = v.vertex;
				float4 transform15_g6 = mul(GetObjectToWorldMatrix(),_VertexPos3_g6);
				float2 appendResult22_g6 = (float2(transform15_g6.x , transform15_g6.z));
				float2 worldPosXZ21_g6 = appendResult22_g6;
				float _WindDirX11 = _Wind.x;
				float _WindDirX5_g6 = _WindDirX11;
				float _WindDirY12 = _Wind.y;
				float _WindDirY7_g6 = _WindDirY12;
				float2 appendResult19_g6 = (float2(_WindDirX5_g6 , _WindDirY7_g6));
				float _WindSpeed13 = _Wind.z;
				float _WindSpeed9_g6 = _WindSpeed13;
				float _WindSpread14 = _Wind.w;
				float _WindSpread10_g6 = _WindSpread14;
				float2 noisePos32_g6 = ( ( worldPosXZ21_g6 - ( appendResult19_g6 * _WindSpeed9_g6 * _TimeParameters.x ) ) / _WindSpread10_g6 );
				float2 uv059_g6 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_35_0_g6 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g6, 0, 0.0) ).r * uv059_g6.y );
				float _WaveDistance9 = _WaveDistance;
				float _WaveDistance12_g6 = _WaveDistance9;
				float _BendFactor71 = _BendFactor;
				float _BendFactor38_g6 = _BendFactor71;
				float4 appendResult42_g6 = (float4(_WindDirX5_g6 , ( temp_output_35_0_g6 * 0.5 ) , _WindDirY7_g6 , 0.0));
				float4 transform47_g6 = mul(GetWorldToObjectMatrix(),( temp_output_35_0_g6 * _WaveDistance12_g6 * _BendFactor38_g6 * appendResult42_g6 ));
				float4 vertexOffset48_g6 = transform47_g6;
				float4 windVertexOffset62 = vertexOffset48_g6;
				float4x4 myLocalVar55_g7 = _WorldToNormalized;
				float4x4 _WorldToNormalized8_g7 = myLocalVar55_g7;
				float4 _VertexPos3_g7 = v.vertex;
				float4 transform12_g7 = mul(GetObjectToWorldMatrix(),_VertexPos3_g7);
				float4 break28_g7 = mul( _WorldToNormalized8_g7, transform12_g7 );
				float4 appendResult29_g7 = (float4(break28_g7.x , break28_g7.z , 0.0 , 0.0));
				float4 vectorFieldUV30_g7 = appendResult29_g7;
				float4 bendVector33_g7 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g7.xy, 0, 0.0) );
				float4 break36_g7 = bendVector33_g7;
				float4 appendResult43_g7 = (float4(( ( break36_g7.r * 2.0 ) - 1.0 ) , ( ( break36_g7.g * 2.0 ) - 1.0 ) , ( ( break36_g7.b * 2.0 ) - 1.0 ) , 0.0));
				float4 remappedBendVector44_g7 = appendResult43_g7;
				float2 uv056_g7 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float _BendFactor51_g7 = _BendFactor71;
				float4 vertexOffset52_g7 = ( remappedBendVector44_g7 * uv056_g7.y * _BendFactor51_g7 );
				float4 touchBendingOffset83 = vertexOffset52_g7;
				float4 break87 = ( windVertexOffset62 + touchBendingOffset83 );
				float4 appendResult88 = (float4(break87.x , max( break87.y , 0.0 ) , break87.z , break87.w));
				float4 totalVertexOffset93 = appendResult88;
				
				float3 vertexNormal55 = float3(0,1,0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = totalVertexOffset93.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = vertexNormal55;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 _Color5 = _Color;
				float2 uv0_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv0_MainTex ) );
				float alpha45 = temp_output_24_0.a;
				
				float _Cutoff16 = _Cutoff;
				
				float Alpha = alpha45;
				float AlphaClipThreshold = _Cutoff16;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION


			sampler2D _NoiseTex;
			sampler2D _VectorField;
			float4x4 _WorldToNormalized;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Wind;
			float _WaveDistance;
			float _BendFactor;
			float4 _Color;
			float4 _MainTex_ST;
			float _Occlusion;
			float _Smoothness;
			float _Cutoff;
			CBUFFER_END


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float4 _VertexPos3_g6 = v.vertex;
				float4 transform15_g6 = mul(GetObjectToWorldMatrix(),_VertexPos3_g6);
				float2 appendResult22_g6 = (float2(transform15_g6.x , transform15_g6.z));
				float2 worldPosXZ21_g6 = appendResult22_g6;
				float _WindDirX11 = _Wind.x;
				float _WindDirX5_g6 = _WindDirX11;
				float _WindDirY12 = _Wind.y;
				float _WindDirY7_g6 = _WindDirY12;
				float2 appendResult19_g6 = (float2(_WindDirX5_g6 , _WindDirY7_g6));
				float _WindSpeed13 = _Wind.z;
				float _WindSpeed9_g6 = _WindSpeed13;
				float _WindSpread14 = _Wind.w;
				float _WindSpread10_g6 = _WindSpread14;
				float2 noisePos32_g6 = ( ( worldPosXZ21_g6 - ( appendResult19_g6 * _WindSpeed9_g6 * _TimeParameters.x ) ) / _WindSpread10_g6 );
				float2 uv059_g6 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_35_0_g6 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g6, 0, 0.0) ).r * uv059_g6.y );
				float _WaveDistance9 = _WaveDistance;
				float _WaveDistance12_g6 = _WaveDistance9;
				float _BendFactor71 = _BendFactor;
				float _BendFactor38_g6 = _BendFactor71;
				float4 appendResult42_g6 = (float4(_WindDirX5_g6 , ( temp_output_35_0_g6 * 0.5 ) , _WindDirY7_g6 , 0.0));
				float4 transform47_g6 = mul(GetWorldToObjectMatrix(),( temp_output_35_0_g6 * _WaveDistance12_g6 * _BendFactor38_g6 * appendResult42_g6 ));
				float4 vertexOffset48_g6 = transform47_g6;
				float4 windVertexOffset62 = vertexOffset48_g6;
				float4x4 myLocalVar55_g7 = _WorldToNormalized;
				float4x4 _WorldToNormalized8_g7 = myLocalVar55_g7;
				float4 _VertexPos3_g7 = v.vertex;
				float4 transform12_g7 = mul(GetObjectToWorldMatrix(),_VertexPos3_g7);
				float4 break28_g7 = mul( _WorldToNormalized8_g7, transform12_g7 );
				float4 appendResult29_g7 = (float4(break28_g7.x , break28_g7.z , 0.0 , 0.0));
				float4 vectorFieldUV30_g7 = appendResult29_g7;
				float4 bendVector33_g7 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g7.xy, 0, 0.0) );
				float4 break36_g7 = bendVector33_g7;
				float4 appendResult43_g7 = (float4(( ( break36_g7.r * 2.0 ) - 1.0 ) , ( ( break36_g7.g * 2.0 ) - 1.0 ) , ( ( break36_g7.b * 2.0 ) - 1.0 ) , 0.0));
				float4 remappedBendVector44_g7 = appendResult43_g7;
				float2 uv056_g7 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float _BendFactor51_g7 = _BendFactor71;
				float4 vertexOffset52_g7 = ( remappedBendVector44_g7 * uv056_g7.y * _BendFactor51_g7 );
				float4 touchBendingOffset83 = vertexOffset52_g7;
				float4 break87 = ( windVertexOffset62 + touchBendingOffset83 );
				float4 appendResult88 = (float4(break87.x , max( break87.y , 0.0 ) , break87.z , break87.w));
				float4 totalVertexOffset93 = appendResult88;
				
				float3 vertexNormal55 = float3(0,1,0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = totalVertexOffset93.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = vertexNormal55;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 _Color5 = _Color;
				float2 uv0_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv0_MainTex ) );
				float _Occlusion18 = _Occlusion;
				float2 uv026 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult33 = lerp( 0.0 , _Occlusion18 , ( ( 1.0 - uv026.y ) * ( 1.0 - uv026.y ) ));
				float4 albedoColor40 = ( temp_output_24_0 - float4( ( 0.5 * float3(1,1,1) * lerpResult33 ) , 0.0 ) );
				
				float alpha45 = temp_output_24_0.a;
				
				float _Cutoff16 = _Cutoff;
				
				
				float3 Albedo = albedoColor40.rgb;
				float3 Emission = 0;
				float Alpha = alpha45;
				float AlphaClipThreshold = _Cutoff16;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero , One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 999999

			#pragma enable_d3d11_debug_symbols
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_VERT_POSITION


			sampler2D _NoiseTex;
			sampler2D _VectorField;
			float4x4 _WorldToNormalized;
			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _Wind;
			float _WaveDistance;
			float _BendFactor;
			float4 _Color;
			float4 _MainTex_ST;
			float _Occlusion;
			float _Smoothness;
			float _Cutoff;
			CBUFFER_END


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float4 _VertexPos3_g6 = v.vertex;
				float4 transform15_g6 = mul(GetObjectToWorldMatrix(),_VertexPos3_g6);
				float2 appendResult22_g6 = (float2(transform15_g6.x , transform15_g6.z));
				float2 worldPosXZ21_g6 = appendResult22_g6;
				float _WindDirX11 = _Wind.x;
				float _WindDirX5_g6 = _WindDirX11;
				float _WindDirY12 = _Wind.y;
				float _WindDirY7_g6 = _WindDirY12;
				float2 appendResult19_g6 = (float2(_WindDirX5_g6 , _WindDirY7_g6));
				float _WindSpeed13 = _Wind.z;
				float _WindSpeed9_g6 = _WindSpeed13;
				float _WindSpread14 = _Wind.w;
				float _WindSpread10_g6 = _WindSpread14;
				float2 noisePos32_g6 = ( ( worldPosXZ21_g6 - ( appendResult19_g6 * _WindSpeed9_g6 * _TimeParameters.x ) ) / _WindSpread10_g6 );
				float2 uv059_g6 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_35_0_g6 = ( tex2Dlod( _NoiseTex, float4( noisePos32_g6, 0, 0.0) ).r * uv059_g6.y );
				float _WaveDistance9 = _WaveDistance;
				float _WaveDistance12_g6 = _WaveDistance9;
				float _BendFactor71 = _BendFactor;
				float _BendFactor38_g6 = _BendFactor71;
				float4 appendResult42_g6 = (float4(_WindDirX5_g6 , ( temp_output_35_0_g6 * 0.5 ) , _WindDirY7_g6 , 0.0));
				float4 transform47_g6 = mul(GetWorldToObjectMatrix(),( temp_output_35_0_g6 * _WaveDistance12_g6 * _BendFactor38_g6 * appendResult42_g6 ));
				float4 vertexOffset48_g6 = transform47_g6;
				float4 windVertexOffset62 = vertexOffset48_g6;
				float4x4 myLocalVar55_g7 = _WorldToNormalized;
				float4x4 _WorldToNormalized8_g7 = myLocalVar55_g7;
				float4 _VertexPos3_g7 = v.vertex;
				float4 transform12_g7 = mul(GetObjectToWorldMatrix(),_VertexPos3_g7);
				float4 break28_g7 = mul( _WorldToNormalized8_g7, transform12_g7 );
				float4 appendResult29_g7 = (float4(break28_g7.x , break28_g7.z , 0.0 , 0.0));
				float4 vectorFieldUV30_g7 = appendResult29_g7;
				float4 bendVector33_g7 = tex2Dlod( _VectorField, float4( vectorFieldUV30_g7.xy, 0, 0.0) );
				float4 break36_g7 = bendVector33_g7;
				float4 appendResult43_g7 = (float4(( ( break36_g7.r * 2.0 ) - 1.0 ) , ( ( break36_g7.g * 2.0 ) - 1.0 ) , ( ( break36_g7.b * 2.0 ) - 1.0 ) , 0.0));
				float4 remappedBendVector44_g7 = appendResult43_g7;
				float2 uv056_g7 = v.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float _BendFactor51_g7 = _BendFactor71;
				float4 vertexOffset52_g7 = ( remappedBendVector44_g7 * uv056_g7.y * _BendFactor51_g7 );
				float4 touchBendingOffset83 = vertexOffset52_g7;
				float4 break87 = ( windVertexOffset62 + touchBendingOffset83 );
				float4 appendResult88 = (float4(break87.x , max( break87.y , 0.0 ) , break87.z , break87.w));
				float4 totalVertexOffset93 = appendResult88;
				
				float3 vertexNormal55 = float3(0,1,0);
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = totalVertexOffset93.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = vertexNormal55;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 _Color5 = _Color;
				float2 uv0_MainTex = IN.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_output_24_0 = ( _Color5 * tex2D( _MainTex, uv0_MainTex ) );
				float _Occlusion18 = _Occlusion;
				float2 uv026 = IN.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult33 = lerp( 0.0 , _Occlusion18 , ( ( 1.0 - uv026.y ) * ( 1.0 - uv026.y ) ));
				float4 albedoColor40 = ( temp_output_24_0 - float4( ( 0.5 * float3(1,1,1) * lerpResult33 ) , 0.0 ) );
				
				float alpha45 = temp_output_24_0.a;
				
				float _Cutoff16 = _Cutoff;
				
				
				float3 Albedo = albedoColor40.rgb;
				float Alpha = alpha45;
				float AlphaClipThreshold = _Cutoff16;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}
		
	}
	
	Fallback "Universal Render Pipeline/Lit"
	
}
/*ASEBEGIN
Version=18000
1244;197;1906;885;3771.438;-1197.073;1.625107;True;False
Node;AmplifyShaderEditor.CommentaryNode;19;-2317.74,-2166.847;Inherit;False;701.778;1625.974;;21;49;48;16;15;9;12;13;14;71;11;70;10;8;5;18;4;17;7;6;80;81;Properties;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-2168.615,-1464.77;Float;False;Property;_WaveDistance;WaveDistance;10;0;Create;True;0;0;False;0;0.1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;80;-2192.371,-1718.896;Float;True;Property;_VectorField;VectorField;12;0;Create;True;0;0;False;0;None;None;False;gray;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector4Node;10;-2169.615,-1363.771;Float;False;Property;_Wind;Wind;11;0;Create;True;0;0;False;0;1,1,4,8;1,1,7,7;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;-2157.813,-704.1543;Float;False;Property;_BendFactor;BendFactor;9;1;[HideInInspector];Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;75;-2991.674,1324.012;Inherit;False;1377.313;1322.777;;24;93;88;90;89;92;91;87;86;85;84;83;82;79;55;62;54;74;72;68;65;63;64;66;109;Vertex Animation, Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1882.614,-1281.771;Float;False;_WindDirY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;41;-3517.229,-256.6267;Inherit;False;1899.109;1469.351;;6;40;39;37;38;44;45;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;13;-1882.614,-1199.772;Float;False;_WindSpeed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1882.614,-1356.771;Float;False;_WindDirX;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-1884.614,-1118.772;Float;False;_WindSpread;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1884.813,-702.1543;Float;False;_BendFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-1880.192,-1716.869;Float;False;_VectorField;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1881.614,-1463.77;Float;False;_WaveDistance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-2932.674,1801.739;Inherit;False;14;_WindSpread;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;-2922.274,1547.266;Inherit;False;11;_WindDirX;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-2927.726,2100.926;Inherit;False;81;_VectorField;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-2924.846,2193.444;Inherit;False;71;_BendFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;37;-3415.318,290.2566;Inherit;False;1270.362;876.2831;;7;35;34;33;30;32;31;36;Occlusion;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-2928.674,1706.739;Inherit;False;13;_WindSpeed;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-2941.674,1887.739;Inherit;False;9;_WaveDistance;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-2918.674,1627.739;Inherit;False;12;_WindDirY;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-2924.889,1976.733;Inherit;False;71;_BendFactor;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;6;-2190.384,-1929.848;Float;True;Property;_MainTex;MainTex;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;74;-2537.48,1578.475;Inherit;True;GrassWindAnimation;0;;6;8d39a13fc2a7a164fa1708057ff071d3;0;7;1;FLOAT4;0,0,0,0;False;51;FLOAT;1;False;52;FLOAT;1;False;53;FLOAT;7;False;54;FLOAT;7;False;55;FLOAT;0.2;False;56;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;36;-3395.857,845.9573;Inherit;False;711;293;;4;26;27;28;29;Occlusion factor;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;109;-2208.612,2157.495;Inherit;False;GrassTouchBending;3;;7;de9a62c6bacea6b4888ce2cdd0a3d3f8;0;3;2;FLOAT4;0,0,0,0;False;4;SAMPLER2D;;False;50;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-2263.971,-909.8423;Float;False;Property;_Occlusion;Occlusion;7;0;Create;True;0;0;False;0;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;38;-3109.156,-135.5909;Inherit;False;957;392;;5;21;22;23;25;24;Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;62;-2186.276,1573.266;Float;False;windVertexOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-1876.468,2157.643;Float;False;touchBendingOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;7;-1884.383,-1928.848;Float;False;_MainTex;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;26;-3345.857,910.957;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;85;-2955.059,2406.24;Inherit;False;83;touchBendingOffset;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;4;-2174.384,-2115.848;Float;False;Property;_Color;Color;2;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;27;-3060.857,895.9569;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;28;-3065.857,1028.956;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;-3011.407,8.565916;Inherit;False;7;_MainTex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-1886.246,-907.3323;Float;False;_Occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-2943.238,2316.919;Inherit;False;62;windVertexOffset;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2847.918,578.3406;Float;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-2853.856,953.9564;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-1885.383,-2116.848;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-2931.117,686.2404;Inherit;False;18;_Occlusion;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;86;-2612.219,2353.699;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-3059.156,100.409;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;32;-2659.255,452.257;Float;False;Constant;_Vector1;Vector 1;6;0;Create;True;0;0;False;0;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;25;-2606.156,-85.59092;Inherit;False;5;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2634.255,340.2565;Float;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;-2645.116,634.2404;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;21;-2705.05,8.684111;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;87;-2462.538,2350.519;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;90;-2151.156,2325.397;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;89;-2180.767,2380.926;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2321.157,-8.590945;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;92;-2148.115,2512.909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-2289.256,500.2567;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;91;-2147.102,2557.506;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;44;-2110.054,-11.27011;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;88;-2025.692,2349.507;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;54;-2400.55,1374.709;Float;False;Constant;_Up;Up;7;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;15;-2263.971,-1012.744;Float;False;Property;_Cutoff;Cutoff;6;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;39;-2025.438,246.9899;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-2180.177,1374.012;Float;False;vertexNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;-1844.054,53.72989;Float;False;alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-1847.459,246.99;Float;False;albedoColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;-1884.992,-1011.489;Float;False;_Cutoff;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-1861.78,2343.466;Float;False;totalVertexOffset;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;-342.0536,261.7299;Inherit;False;16;_Cutoff;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2262.407,-810.6655;Float;False;Property;_Smoothness;Smoothness;8;0;Create;True;0;0;False;0;0.25;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-367.3995,-0.407074;Inherit;False;40;albedoColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-377.9375,93.27258;Inherit;False;49;_Smoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-342.2595,180.5466;Inherit;False;45;alpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;-373.3654,450.1091;Inherit;False;55;vertexNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-1886.407,-808.6654;Float;False;_Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-371.1169,354.3228;Inherit;False;93;totalVertexOffset;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;110;0,0;Half;False;True;-1;2;;0;2;Polaris/Deprecated/URP/Foliage/GrassInteractive;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;14;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Universal Render Pipeline/Lit;0;0;Standard;14;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Cast Shadows;1;Receive Shadows;1;GPU Instancing;0;LOD CrossFade;0;Built-in Fog;1;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;Vertex Position,InvertActionOnDeselection;1;0;6;False;True;True;True;True;True;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;114;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;111;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;112;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;113;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;115;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;12;0;10;2
WireConnection;13;0;10;3
WireConnection;11;0;10;1
WireConnection;14;0;10;4
WireConnection;71;0;70;0
WireConnection;81;0;80;0
WireConnection;9;0;8;0
WireConnection;74;51;63;0
WireConnection;74;52;64;0
WireConnection;74;53;65;0
WireConnection;74;54;66;0
WireConnection;74;55;68;0
WireConnection;74;56;72;0
WireConnection;109;4;79;0
WireConnection;109;50;82;0
WireConnection;62;0;74;0
WireConnection;83;0;109;0
WireConnection;7;0;6;0
WireConnection;27;0;26;2
WireConnection;28;0;26;2
WireConnection;18;0;17;0
WireConnection;29;0;27;0
WireConnection;29;1;28;0
WireConnection;5;0;4;0
WireConnection;86;0;84;0
WireConnection;86;1;85;0
WireConnection;23;2;22;0
WireConnection;33;0;34;0
WireConnection;33;1;35;0
WireConnection;33;2;29;0
WireConnection;21;0;22;0
WireConnection;21;1;23;0
WireConnection;87;0;86;0
WireConnection;90;0;87;0
WireConnection;89;0;87;1
WireConnection;24;0;25;0
WireConnection;24;1;21;0
WireConnection;92;0;87;2
WireConnection;30;0;31;0
WireConnection;30;1;32;0
WireConnection;30;2;33;0
WireConnection;91;0;87;3
WireConnection;44;0;24;0
WireConnection;88;0;90;0
WireConnection;88;1;89;0
WireConnection;88;2;92;0
WireConnection;88;3;91;0
WireConnection;39;0;24;0
WireConnection;39;1;30;0
WireConnection;55;0;54;0
WireConnection;45;0;44;3
WireConnection;40;0;39;0
WireConnection;16;0;15;0
WireConnection;93;0;88;0
WireConnection;49;0;48;0
WireConnection;110;0;42;0
WireConnection;110;4;50;0
WireConnection;110;6;46;0
WireConnection;110;7;43;0
WireConnection;110;8;73;0
WireConnection;110;10;59;0
ASEEND*/
//CHKSM=5915946F71D7444F74F0DCFA42484A6502F658B4