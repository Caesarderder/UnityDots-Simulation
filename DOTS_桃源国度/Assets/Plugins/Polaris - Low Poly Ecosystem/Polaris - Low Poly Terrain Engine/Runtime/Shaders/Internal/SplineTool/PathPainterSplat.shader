// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/Polaris/PathPainterAlbedo"
{
	Properties { }
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend Off
		ZWrite Off
		ZTest Always
		Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

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

			sampler2D _MainTex;
			sampler2D _Falloff;
			sampler2D _FalloffNoise;
			float4 _FalloffNoise_ST;
			sampler2D _TerrainMask;
			float4x4 _WorldToNormalized;
			int _ChannelIndex;

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
				float4 currentColor = tex2D(_MainTex, i.uv);
				float2 noiseUV = float2(i.positionWS.x * _FalloffNoise_ST.x, i.positionWS.z * _FalloffNoise_ST.y);
				float falloffNoise = tex2D(_FalloffNoise, noiseUV).r;
				float t = clamp(i.color.r, 0, 1);
				float falloffCurve = tex2D(_Falloff, float2(t, 0.5)).r;
				float falloff = (falloffCurve - falloffNoise) * (i.color.r < 1) + i.color.r * (i.color.r == 1);
				falloff = saturate(falloff);

				float terrainMask = 1 - tex2D(_TerrainMask, i.uv).r;
				float4 channel = float4(_ChannelIndex == 0, _ChannelIndex == 1, _ChannelIndex == 2, _ChannelIndex == 3);
				float4 desColor = channel;
				float4 result = lerp(currentColor, desColor, falloff);
				result = lerp(currentColor, result, terrainMask);
				return result;
			}
			ENDCG

		}
	}
}
