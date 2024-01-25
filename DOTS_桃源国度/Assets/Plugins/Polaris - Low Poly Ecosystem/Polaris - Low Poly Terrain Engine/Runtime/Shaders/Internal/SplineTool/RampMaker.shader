Shader "Hidden/Griffin/RampMaker"
{
	Properties 
	{
		_HeightMap("Height Map", 2D) = "black"{}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "SplineToolCommon.cginc"

	struct appdata
	{
		float4 vertex: POSITION;
		float4 color: COLOR;
	};

	struct v2f
	{
		float4 vertex: SV_POSITION;
		float4 color: COLOR;
		float3 positionNormalized: TEXCOORD0;
		float4 positionWS: TEXCOORD1;
	};

	sampler2D _HeightMap;
	sampler2D _Falloff;
	sampler2D _FalloffNoise;
	float4 _FalloffNoise_ST;
	float _HeightOffset;
	int _LowerHeight;
	int _RaiseHeight;
	float _AdditionalMeshResolution;
	sampler2D _TerrainMask;
	int _StepCount;
	float4x4 _WorldToNormalized;

	float stepValue(float v, int stepCount)
	{
		float step = 1.0 / stepCount;
		return v - v % step;
	}

	v2f vert(appdata v)
	{
		v2f o;
		v.vertex.y += _HeightOffset;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.color = v.color;
		o.positionWS = mul(unity_ObjectToWorld, v.vertex);
		o.positionNormalized = mul(_WorldToNormalized, o.positionWS);
		return o;
	}

	fixed4 fragRamp(v2f i): SV_Target
	{
		float4 heightMapColor = tex2D(_HeightMap, i.positionNormalized.xz);
		float currentHeight = GriffinDecodeFloatRG(heightMapColor.rg);
		float splineHeight = clamp(i.positionNormalized.y, 0, 1);
		float delta = splineHeight - currentHeight;
		float targetHeight = currentHeight + (delta < 0) * _LowerHeight * delta + (delta >= 0) * _RaiseHeight * saturate(delta);

		float2 noiseUV = float2(i.positionWS.x * _FalloffNoise_ST.x, i.positionWS.z * _FalloffNoise_ST.y);
		float falloffNoise = tex2D(_FalloffNoise, noiseUV).r;
		float t = clamp(i.color.r, 0, 1);
		float falloffCurve = tex2D(_Falloff, float2(t, 0.5)).r;
		float falloff = (falloffCurve - falloffNoise) * (i.color.r < 1) + i.color.r * (i.color.r == 1);
		falloff = saturate(falloff);

		float terrainMask = 1 - tex2D(_TerrainMask, i.positionNormalized.xz).r;
		float h = lerp(currentHeight, targetHeight, falloff);
		h = lerp(currentHeight, h, terrainMask);
		h = stepValue(h, _StepCount);
		h = max(0, min(0.999999, h));

		float2 encodedHeight = GriffinEncodeFloatRG(h);

		float addRes = lerp(0, _AdditionalMeshResolution, terrainMask);
		return saturate(float4(encodedHeight.rg, addRes, heightMapColor.a));
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Cull Off

		Pass
		{
			Name "Ramp"
			Blend One Zero
			BlendOp Add
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment fragRamp
			ENDCG

		}
	}
}
