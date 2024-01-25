Shader "Hidden/Griffin/NoisePainter"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
		_Mask("Mask", 2D) = "white" {}
		_NoiseMap("Noise Map", 2D) = "white" {}
		_Opacity("Opacity", Float) = 1
		_TerrainMask("Terrain Mask", 2D) = "black" {}
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "PaintToolCommon.cginc"
#pragma multi_compile _ USE_WORLD_SPACE

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float4 localPos : TEXCOORD1;
	};

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	sampler2D _Mask;
	sampler2D _NoiseMap;
	float _Opacity;
	sampler2D _TerrainMask;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}

	fixed4 fragRaise(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;

		float4 maskColor = tex2D(_Mask, i.uv);
		#if USE_WORLD_SPACE
		float4 noiseColor = tex2D(_NoiseMap, i.localPos);
		#else
		float4 noiseColor = tex2D(_NoiseMap, i.uv);
		#endif
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float currentHeight = GriffinDecodeFloatRG(currentColor.rg);
		float desHeight = currentHeight + maskColor.r * noiseColor.r * _Opacity * terrainMask;
		desHeight = max(0, min(0.999999, desHeight));
		float2 encodedHeight = GriffinEncodeFloatRG(desHeight);

		return saturate(float4(encodedHeight.rg, currentColor.b, currentColor.a));
	}


		fixed4 fragLower(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;

		float4 maskColor = tex2D(_Mask, i.uv);
		#if USE_WORLD_SPACE
		float4 noiseColor = tex2D(_NoiseMap, i.localPos);
		#else
		float4 noiseColor = tex2D(_NoiseMap, i.uv);
		#endif
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float currentHeight = GriffinDecodeFloatRG(currentColor.rg);
		float desHeight = currentHeight - maskColor.r * noiseColor.r * _Opacity * terrainMask;
		desHeight = max(0, min(0.999999, desHeight));
		float2 encodedHeight = GriffinEncodeFloatRG(desHeight);

		return saturate(float4(encodedHeight.rg, currentColor.b, currentColor.a));
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Transparent" }

			Pass
		{
			Name "Raise"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragRaise
			ENDCG
		}

			Pass
		{
			Name "Lower"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragLower
			ENDCG
		}
	}
}
