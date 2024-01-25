Shader "Hidden/Griffin/WarpFilter"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}
		_Mask("Mask", 2D) = "black" {}
		_Strength("Strength", Float) = 1
	}

		CGINCLUDE
#pragma shader_feature MASK_IS_NORMAL
#include "UnityCG.cginc"
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
		float4 _MainTex_TexelSize;
		sampler2D _Mask;
		float4 _Mask_TexelSize;
		float _Strength;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		float2 warp(float2 uv)
		{
			float3 normal = float4(0, 0, 1, 1);
			float multiplier = 1;
#if MASK_IS_NORMAL
			normal = tex2D(_Mask, uv).rgb;
			multiplier = 1 - normal.z;
#else
			float2 texel = _Mask_TexelSize.xy;
			float2 uvLeft = uv - float2(texel.x, 0);
			float2 uvUp = uv + float2(0, texel.y);
			float2 uvRight = uv + float2(texel.x, 0);
			float2 uvDown = uv - float2(0, texel.y);
			float2 uvCenter = uv;
			float2 uvLeftUp = uv - float2(texel.x, 0) + float2(0, texel.y);
			float2 uvUpRight = uv + float2(0, texel.y) + float2(texel.x, 0);
			float2 uvRightDown = uv + float2(texel.x, 0) - float2(0, texel.y);
			float2 uvDownLeft = uv - float2(0, texel.y) - float2(texel.x, 0);

			float leftHeight = tex2D(_Mask, uvLeft).r;
			float upHeight = tex2D(_Mask, uvUp).r;
			float rightHeight = tex2D(_Mask, uvRight).r;
			float downHeight = tex2D(_Mask, uvDown).r;
			float centerHeight = tex2D(_Mask, uvCenter).r;
			float leftUpHeight = tex2D(_Mask, uvLeftUp).r;
			float upRightHeight = tex2D(_Mask, uvUpRight).r;
			float rightDownHeight = tex2D(_Mask, uvRightDown).r;
			float downLeftHeight = tex2D(_Mask, uvDownLeft).r;

			float3 left = float3(uvLeft.x, leftHeight, uvLeft.y);
			float3 up = float3(uvUp.x, leftHeight, uvUp.y);
			float3 right = float3(uvRight.x, leftHeight, uvRight.y);
			float3 down = float3(uvDown.x, leftHeight, uvDown.y);
			float3 center = float3(uvCenter.x, centerHeight, uvCenter.y);
			float3 leftUp = float3(uvLeftUp.x, leftUpHeight, uvLeftUp.y);
			float3 upRight = float3(uvUpRight.x, upRightHeight, uvUpRight.y);
			float3 rightDown = float3(uvRightDown.x, rightDownHeight, uvRightDown.y);
			float3 downLeft = float3(uvDownLeft.x, downLeftHeight, uvDownLeft.y);

			float3 n0 = cross(left - center, leftUp - center);
			float3 n1 = cross(up - center, upRight - center);
			float3 n2 = cross(right - center, rightDown - center);
			float3 n3 = cross(down - center, downLeft - center);

			float3 n4 = cross(leftUp - center, up - center);
			float3 n5 = cross(upRight - center, right - center);
			float3 n6 = cross(rightDown - center, down - center);
			float3 n7 = cross(downLeft - center, left - center);

			float3 nc = (n0 + n1 + n2 + n3 + n4 + n5 + n6 + n7) / 8;

			float3 n = float3(nc.x, nc.z, nc.y);
			normal = normalize(n);
			multiplier = centerHeight * (1 - normal.z);
#endif

			float2 dir = float2(normal.r * 2 - 1, normal.g * 2 - 1);

			return uv - _Strength * multiplier * dir * _MainTex_TexelSize;
		}

		float4 frag(v2f i) : SV_Target
		{
			float2 uv = warp(i.uv);
			float4 col = tex2D(_MainTex, uv);
			return col;
		}

			ENDCG

			SubShader
		{
			Tags{ "RenderType" = "Opaque" }

				Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				ENDCG
			}
		}
}
