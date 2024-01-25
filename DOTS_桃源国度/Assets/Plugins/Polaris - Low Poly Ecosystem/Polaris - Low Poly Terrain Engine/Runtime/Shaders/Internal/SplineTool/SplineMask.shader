Shader "Hidden/Griffin/SplineMask"
{
	Properties
	{
		_Falloff ("Falloff", 2D) = "white" { }
		_FalloffNoise ("Falloff Noise", 2D) = "white" { }
		_TerrainMask ("TerrainMask", 2D) = "black" { }
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex: POSITION;
		float4 color: COLOR;
	};

	struct v2f
	{
		float4 vertex: SV_POSITION;
		float4 color: COLOR;
		float2 uv: TEXCOORD0;
		float4 positionWS: TEXCOORD1;
	};

	sampler2D _Falloff;
	sampler2D _FalloffNoise;
	float4 _FalloffNoise_ST;
	sampler2D _TerrainMask;
	float4x4 _WorldToNormalized;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.color = v.color;
		o.positionWS = mul(unity_ObjectToWorld, v.vertex);
		o.uv = mul(_WorldToNormalized, o.positionWS).xz;
		return o;
	}

	fixed4 frag(v2f i): SV_Target
	{
		float2 noiseUV = float2(i.positionWS.x * _FalloffNoise_ST.x, i.positionWS.z * _FalloffNoise_ST.y);
		float falloffNoise = tex2D(_FalloffNoise, noiseUV).r;
		float t = clamp(i.color.r, 0, 1);
		float falloffCurve = tex2D(_Falloff, float2(t, 0.5)).r;
		float falloff = (falloffCurve - falloffNoise) * (i.color.r < 1) + i.color.r * (i.color.r == 1);
		falloff = saturate(falloff);

		float terrainMask = 1 - tex2D(_TerrainMask, i.uv).r;
		float4 overlayColor = float4(falloff, falloff, falloff, falloff) * terrainMask;

		return overlayColor;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Cull Off

		Pass
		{
			Blend One Zero
			BlendOp Add
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			ENDCG

		}
	}
}
