Shader "Hidden/Polaris/ErosionTexturer"
{
	Properties { }
	CGINCLUDE
	#pragma vertex vert

	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex: POSITION;
		float2 uv: TEXCOORD0;
	};

	struct v2f
	{
		float2 uv: TEXCOORD0;
		float4 vertex: SV_POSITION;
		float4 localPos: TEXCOORD1;
	};

	sampler2D _MainTex;
	sampler2D _ErosionMap;
	sampler2D _FalloffTexture;

	float4 _ErosionAlbedo;
	float _ErosionMetallic;
	float _ErosionSmoothness;
	float _ErosionChannelIndex;
	float _ErosionIntensity;
	float _ErosionExponent;

	float4 _DepositionAlbedo;
	float _DepositionMetallic;
	float _DepositionSmoothness;
	float _DepositionChannelIndex;
	float _DepositionIntensity;
	float _DepositionExponent;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Name "Splat"
			CGPROGRAM

			#pragma fragment fragSplat

			float4 fragSplat(v2f i): SV_Target
			{
				float f = saturate(2 * length(i.uv - float2(0.5, 0.5)));
				float falloff = tex2D(_FalloffTexture, float2(f, 0.5)).r;

				float4 currentColor = tex2D(_MainTex, i.localPos);
				float2 erosionData = tex2D(_ErosionMap, i.uv);

				float erosionStrength = saturate(pow(erosionData.r * _ErosionIntensity * falloff, _ErosionExponent));
				float4 erosionChannel = float4(_ErosionChannelIndex == 0, _ErosionChannelIndex == 1, _ErosionChannelIndex == 2, _ErosionChannelIndex == 3);
				currentColor = lerp(currentColor, erosionChannel, erosionStrength);

				float depositionStrength = saturate(pow(erosionData.g * _DepositionIntensity * falloff, _DepositionExponent));
				float4 depositionChannel = float4(_DepositionChannelIndex == 0, _DepositionChannelIndex == 1, _DepositionChannelIndex == 2, _DepositionChannelIndex == 3);
				currentColor = lerp(currentColor, depositionChannel, depositionStrength);

				return currentColor;
			}

			ENDCG
		}

		Pass
		{
			Name "Albedo"
			CGPROGRAM

			#pragma fragment fragAlbedo

			float4 fragAlbedo(v2f i): SV_Target
			{
				float f = saturate(2 * length(i.uv - float2(0.5, 0.5)));
				float falloff = tex2D(_FalloffTexture, float2(f, 0.5)).r;

				float4 currentColor = tex2D(_MainTex, i.localPos);
				float2 erosionData = tex2D(_ErosionMap, i.uv);

				float erosionStrength = saturate(pow(erosionData.r * _ErosionIntensity * falloff, _ErosionExponent));
				currentColor = lerp(currentColor, _ErosionAlbedo, erosionStrength);

				float depositionStrength = saturate(pow(erosionData.g * _DepositionIntensity * falloff, _DepositionExponent));
				currentColor = lerp(currentColor, _DepositionAlbedo, depositionStrength);

				return currentColor;
			}

			ENDCG
		}
		Pass
		{
			Name "Metallic Smoothness"
			CGPROGRAM

			#pragma fragment fragMetallicSmoothness

			float4 fragMetallicSmoothness(v2f i): SV_Target
			{
				float f = saturate(2 * length(i.uv - float2(0.5, 0.5)));
				float falloff = tex2D(_FalloffTexture, float2(f, 0.5)).r;

				float4 currentColor = tex2D(_MainTex, i.localPos);
				float2 erosionData = tex2D(_ErosionMap, i.uv);

				float erosionStrength = saturate(pow(erosionData.r * _ErosionIntensity * falloff, _ErosionExponent));
				float4 targetMS = float4(_ErosionMetallic, currentColor.g, currentColor.b, _ErosionSmoothness);
				currentColor = lerp(currentColor, targetMS, erosionStrength);

				float depositionStrength = saturate(pow(erosionData.g * _DepositionIntensity * falloff, _DepositionExponent));	
				targetMS = float4(_DepositionMetallic, currentColor.g, currentColor.b, _DepositionSmoothness);
				currentColor = lerp(currentColor, targetMS, depositionStrength);

				return currentColor;
			}

			ENDCG
		}
	}
}
