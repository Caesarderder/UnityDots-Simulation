
Shader "Polaris/BuiltinRP/Demo/WaterBasic" 
{
	Properties
	{
		_Color("Color", Color) = (0.0, 0.8, 1.0, 0.5)
		_Specular ("Specular Color", Color) = (0.1, 0.1, 0.1, 1)
		_Smoothness("Smoothness", Range(0.0,1.0)) = 1

		_DepthColor("Depth Color", Color) = (0.0, 0.45, 0.65, 0.85)
		_MaxDepth("Max Depth", Float) = 5

		_FoamColor("Foam Color", Color) = (1,1,1,1)
		_FoamDistance("Foam Distance", Float) = 1.2

		_FresnelStrength("Fresnel Strength", Range(0.0, 5.0)) = 1
		_FresnelBias("Fresnel Bias", Range(0.0, 1.0)) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"

		#pragma multi_compile_fog
		#pragma surface surfBasic StandardSpecular alpha:fade keepalpha nolightmap vertex:vertexFunction finalcolor:finalColorFunction  
  
  
		struct Input  
		{
			float4 vertexPos;
			float3 worldPos;
			float4 screenPos;
			float3 normal;
			float fogCoord;
		};

		uniform half4 _Color;
		uniform half4 _Specular;
		uniform half _Smoothness;

		uniform half4 _DepthColor;
		uniform half _MaxDepth;

		uniform half4 _FoamColor;
		uniform half _FoamDistance;

		uniform half _FresnelStrength;
		uniform half _FresnelBias;
		
		UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
		float4 _CameraDepthTexture_TexelSize;
		
		float InverseLerpUnclamped(float a, float b, float value)
		{
			//adding a==b check if needed
			return (value - a) / (b - a + 0.00000001);
		}

		half IsOrtho()
		{
			return unity_OrthoParams.w;
		}

		half GetNearPlane()
		{
			return _ProjectionParams.y;
		}

		half GetFarPlane()
		{
			return _ProjectionParams.z;
		}

		void CalculateScreenDepthEyeSpace(float4 vertexScreenPos, out float depth)
		{
			float4 screenPos = float4(vertexScreenPos.xyz, vertexScreenPos.w + 0.00000000001);
			float depth01 = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos));
			float perpsDepth = LinearEyeDepth(depth01);
	
			#if defined(UNITY_REVERSED_Z)
			depth01 = 1 - depth01;
			#endif
	
			float orthoDepth = lerp(GetNearPlane(), GetFarPlane(), depth01);
			depth = lerp(perpsDepth, orthoDepth, IsOrtho());
		}

		void CalculateSurfaceDepthEyeSpace(float4 vertexPos, out float depth)
		{
			depth = -UnityObjectToViewPos(vertexPos.xyz).z;
		}

		void CalculateDeepWaterColor(float4 vertexScreenPos, float4 vertexPos, float4 color, float4 depthColor, float maxDepth, out float4 waterColor)
		{
			float screenDepth;
			float surfaceDepth;
			CalculateScreenDepthEyeSpace(vertexScreenPos, screenDepth);
			CalculateSurfaceDepthEyeSpace(vertexPos, surfaceDepth);

			float waterDepth = screenDepth - surfaceDepth;
			float depthFade = saturate(InverseLerpUnclamped(0, maxDepth, waterDepth));

			waterColor = lerp(color, depthColor, depthFade);
		}

		void CalculateFresnelFactor(float3 worldPos, float3 worldNormal, float power, float bias, out float fresnel)
		{
			float3 worldViewDir = normalize(_WorldSpaceCameraPos.xyz  - worldPos);
			float vDotN = dot(worldViewDir, worldNormal);
			fresnel = saturate(pow(max(0, 1 - vDotN), power)) - bias;
		}

		void CalculateFoamColor(float4 vertexScreenPos, float4 vertexPos, float4 tint, float foamDistance, out float4 foamColor)
		{
			float screenDepth;
			float surfaceDepth;
			CalculateScreenDepthEyeSpace(vertexScreenPos, screenDepth);
			CalculateSurfaceDepthEyeSpace(vertexPos, surfaceDepth);

			float waterDepth = screenDepth - surfaceDepth;
			float depthClip = waterDepth <= foamDistance;
	
			foamColor = depthClip*tint;
		}

		void vertexFunction(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertexPos = v.vertex;
			o.normal = v.normal;

			UNITY_TRANSFER_FOG(o, UnityObjectToClipPos(v.vertex));
		}
		
		void surfBasic(Input i, inout SurfaceOutputStandardSpecular o)
		{ 
			float3 worldNormal = UnityObjectToWorldNormal(i.normal);
			float fresnel;
			CalculateFresnelFactor(i.worldPos, worldNormal, _FresnelStrength, _FresnelBias, fresnel);

			float4 tintColor = _Color;
			CalculateDeepWaterColor(i.screenPos, i.vertexPos, _Color, _DepthColor, _MaxDepth, tintColor); 

			float4 waterColor = lerp(_Color, tintColor, fresnel);
			waterColor = saturate(waterColor);
			o.Albedo = waterColor.rgb;
			o.Specular = _Specular; 
			o.Alpha = waterColor.a;
			o.Smoothness = _Smoothness; 
		}

		void finalColorFunction(Input i, SurfaceOutputStandardSpecular o, inout fixed4 color)
		{
			float4 foamColor = float4(0,0,0,0); 
			CalculateFoamColor(i.screenPos, i.vertexPos, _FoamColor, _FoamDistance, foamColor);
			color = lerp(color, foamColor, foamColor.a);

			UNITY_APPLY_FOG(i.fogCoord, color); // apply fog 
		}

		ENDCG
	}
	Fallback "Diffuse"
}
 