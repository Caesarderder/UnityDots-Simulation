Shader "Hidden/Griffin/VisibilityPainter"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {}
		_Mask("Mask", 2D) = "white" {}
		_Opacity("Opacity", Float) = 1
		_TerrainMask("Terrain Mask", 2D) = "black" {}
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "PaintToolCommon.cginc"

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

	fixed4 fragAdd(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float4 maskColor = tex2D(_Mask, i.uv);
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 desColor = currentColor + maskColor.rrrr * _Opacity * terrainMask;
		float value = desColor.a;

		return saturate(float4(currentColor.r, currentColor.g, currentColor.b, value));
	}


		fixed4 fragSub(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;
		float4 maskColor = tex2D(_Mask, i.uv);
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 desColor = currentColor - maskColor.rrrr * _Opacity * terrainMask;
		float value = desColor.a;

		return saturate(float4(currentColor.r, currentColor.g, currentColor.b, value));
	}
		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Transparent" }

			Pass
		{
			//normal painting mode, alpha below 0.5 is visible, otherwise invisible, so subtract pass should have index of 0
			Name "Sub"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragSub
			ENDCG
		}

			Pass
		{
			Name "Add"
			Blend One Zero
			BlendOp Add
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragAdd
			ENDCG
		}
	}
}
