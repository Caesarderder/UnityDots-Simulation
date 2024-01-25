Shader "Hidden/Polaris/FetchWorldData"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_Bounds ("Bounds", Vector) = (1, 1, 1, 1)
		_EnableTerrainMask ("Enable Terrain Mask", Float) = 0
	}
	CGINCLUDE
	#include "ErosionToolCommon.cginc"

	struct appdata
	{
		float4 vertex: POSITION;
		float2 uv: TEXCOORD0;
	};

	struct v2f
	{
		float2 uv: TEXCOORD0;
		float4 vertex: SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _Bounds;
	float _EnableTerrainMask;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			

			float4 frag(v2f i): SV_Target
			{
				float2 enc = tex2D(_MainTex, i.uv).rg;
				float h = DecodeFloatRG(enc) * _Bounds.y;
				float4 color = float4(h, 0, h, 0);

				return color;
			}
			ENDCG

		}
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			

			float4 frag(v2f i): SV_Target
			{
				float4 mask = tex2D(_MainTex, i.uv);
				float4 color = float4(mask.b, 1, 0, 1 - mask.r * _EnableTerrainMask); //water source, rain, unused, erosion strength

				return color;
			}
			ENDCG

		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			

			float4 frag(v2f i): SV_Target
			{
				return float4(0, 0, 0, 0);
			}
			ENDCG

		}
	}
}
