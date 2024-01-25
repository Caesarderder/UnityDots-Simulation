Shader "Hidden/Griffin/AlbedoPainter"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "black" { }
		_MainTex_Left ("MainTex_Left", 2D) = "black" { }
		_MainTex_TopLeft ("MainTex_TopLeft", 2D) = "black" { }
		_MainTex_Top ("MainTex_Top", 2D) = "black" { }
		_MainTex_TopRight ("MainTex_TopRight", 2D) = "black" { }
		_MainTex_Right ("MainTex_Right", 2D) = "black" { }
		_MainTex_BottomRight ("MainTex_BottomRight", 2D) = "black" { }
		_MainTex_Bottom ("MainTex_Bottom", 2D) = "black" { }
		_MainTex_BottomLeft ("MainTex_BottomLeft", 2D) = "black" { }

		_Mask ("Mask", 2D) = "white" { }
		_Opacity ("Opacity", Float) = 1
		_TerrainMask ("Terrain Mask", 2D) = "black" { }
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "ConditionalPainting.cginc"
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

	float4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _MainTex_Left;
	sampler2D _MainTex_TopLeft;
	sampler2D _MainTex_Top;
	sampler2D _MainTex_TopRight;
	sampler2D _MainTex_Right;
	sampler2D _MainTex_BottomRight;
	sampler2D _MainTex_Bottom;
	sampler2D _MainTex_BottomLeft;

	sampler2D _Mask;
	float _Opacity;
	sampler2D _TerrainMask;

	sampler2D textureGrid[3][3];

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
	}

	fixed4 fragPaint(v2f i): SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float4 maskColor = tex2D(_Mask, i.uv);
		float maskGray = maskColor.r;
		float conditionalMask = ComputeRuleMask(i.localPos);
		float4 overlayColor = float4(_Color.rgb, _Color.a * maskGray * _Opacity * terrainMask * conditionalMask);
		float4 currentColor = tex2D(_MainTex, i.localPos);

		float4 desColor = overlayColor * overlayColor.a + currentColor * (1 - overlayColor.a);
		float alpha = 1 - (1 - overlayColor.a) * (1 - currentColor.a);

		return float4(desColor.rgb, alpha);
	}

	fixed4 fragErase(v2f i): SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float4 backgroundColor = float4(0, 0, 0, 0);
		float4 maskColor = tex2D(_Mask, i.uv);
		float maskGray = maskColor.r;
		float conditionalMask = ComputeRuleMask(i.localPos);
		float f = maskGray * _Opacity * terrainMask * conditionalMask;
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 dir = normalize(backgroundColor - currentColor) * f;
		float4 desColor = saturate(currentColor + dir);

		return desColor;
	}

	fixed4 fragSmooth(v2f i): SV_Target
	{
		textureGrid[0][0] = _MainTex_BottomLeft;
		textureGrid[1][0] = _MainTex_Bottom;
		textureGrid[2][0] = _MainTex_BottomRight;

		textureGrid[0][1] = _MainTex_Left;
		textureGrid[1][1] = _MainTex;
		textureGrid[2][1] = _MainTex_Right;

		textureGrid[0][2] = _MainTex_TopLeft;
		textureGrid[1][2] = _MainTex_Top;
		textureGrid[2][2] = _MainTex_TopRight;

		float2 texel = _MainTex_TexelSize.xy;
		float4 avgColor = float4(0, 0, 0, 0);
		float sampleCount = 0;

		int indexX = 0;
		int indexY = 0;
		float2 uv = float2(0, 0);
		float4 c = float4(0, 0, 0, 0);

		for (int x0 = -3; x0 <= 3; ++x0)
		{
			for (int y0 = -3; y0 <= 3; ++y0)
			{
				uv = i.localPos + float2(x0 * texel.x, y0 * texel.y);

				indexX = lerp(lerp(1, 2, uv.x > 1), 0, uv.x < 0);
				indexY = lerp(lerp(1, 2, uv.y > 1), 0, uv.y < 0);

				uv.x = lerp(lerp(uv.x, uv.x - 1, uv.x > 1), uv.x + 1, uv.x < 0);
				uv.y = lerp(lerp(uv.y, uv.y - 1, uv.y > 1), uv.y + 1, uv.y < 0);

				if (indexX == 0)
				{
					if (indexY == 0)
					{
						c = tex2D(textureGrid[0][0], uv);
					}
					if (indexY == 1)
					{
						c = tex2D(textureGrid[0][1], uv);
					}
					if (indexY == 2)
					{
						c = tex2D(textureGrid[0][2], uv);
					}
				}
				if (indexX == 1)
				{
					if (indexY == 0)
					{
						c = tex2D(textureGrid[1][0], uv);
					}
					if (indexY == 1)
					{
						c = tex2D(textureGrid[1][1], uv);
					}
					if (indexY == 2)
					{
						c = tex2D(textureGrid[1][2], uv);
					}
				}
				if (indexX == 2)
				{
					if (indexY == 0)
					{
						c = tex2D(textureGrid[2][0], uv);
					}
					if (indexY == 1)
					{
						c = tex2D(textureGrid[2][1], uv);
					}
					if (indexY == 2)
					{
						c = tex2D(textureGrid[2][2], uv);
					}
				}
				avgColor += c;
				sampleCount += 1;
			}
		}
		avgColor = avgColor / sampleCount;

		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 maskColor = tex2D(_Mask, i.uv);
		float maskValue = maskColor.r;
		float conditionalMask = ComputeRuleMask(i.localPos);
		float4 desColor = lerp(currentColor, avgColor, maskValue * _Opacity * terrainMask * conditionalMask);
		return desColor;
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Transparent" }

		Pass
		{
			Name "Paint"
			Blend One Zero
			BlendOp Add
			CGPROGRAM

			#pragma shader_feature BLEND_HEIGHT
			#pragma shader_feature BLEND_SLOPE
			#pragma shader_feature BLEND_NOISE
			#pragma vertex vert
			#pragma fragment fragPaint
			ENDCG

		}

		Pass
		{
			Name "Erase"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			
			#pragma shader_feature BLEND_HEIGHT
			#pragma shader_feature BLEND_SLOPE
			#pragma shader_feature BLEND_NOISE
			#pragma vertex vert
			#pragma fragment fragErase
			ENDCG

		}

		Pass
		{
			Name "Smooth"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			
			#pragma shader_feature BLEND_HEIGHT
			#pragma shader_feature BLEND_SLOPE
			#pragma shader_feature BLEND_NOISE
			#pragma vertex vert
			#pragma fragment fragSmooth
			ENDCG

		}
	}
}
