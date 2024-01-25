Shader "Hidden/Griffin/HeightmapConverterEncodeRG"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

CGINCLUDE

			#include "UtilitiesCommon.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		float4 frag0(v2f i) : SV_Target
		{
			float4 data = tex2D(_MainTex, i.uv);
			float height = data.r;
			float subdiv = data.g;
			float visibility = data.a;

			height = max(0, min(0.999999, height));
			float2 encodedHeight = GriffinEncodeFloatRG(height);

			return float4(encodedHeight.xy, subdiv, visibility);
		}

			float4 frag1(v2f i) : SV_Target
		{
			float4 data = tex2D(_MainTex, i.uv);
			float height = data.r;

			height = max(0, min(0.999999, height));
			float2 encodedHeight = GriffinEncodeFloatRG(height);

			return float4(encodedHeight.xy, 0, 0);
		}

ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag0
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag1
			ENDCG
		}
	}
}
