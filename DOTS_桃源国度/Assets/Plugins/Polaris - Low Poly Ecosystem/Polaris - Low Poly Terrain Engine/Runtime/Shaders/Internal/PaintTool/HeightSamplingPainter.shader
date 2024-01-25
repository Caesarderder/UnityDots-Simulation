Shader "Hidden/Griffin/HeightSamplingPainter"
{
    Properties
    {
		_MainTex ("Main Texture", 2D) = "black" {}
		_Mask ("Mask", 2D) = "white" {}
		_Opacity ("Opacity", Float) = 1
		_TargetGray ("Target Gray", Float) = 0
		_TerrainMask ("Terrain Mask", 2D) = "black" {}
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
	sampler2D _Mask;
	float _Opacity;
	float _TargetGray;
	sampler2D _TerrainMask;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
    }

	fixed4 fragSetHeight(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;

		float4 currentColor = tex2D(_MainTex, i.localPos);
		float4 maskColor = tex2D(_Mask, i.uv);
		float f = maskColor.r*_Opacity*terrainMask;
		float current = GriffinDecodeFloatRG(currentColor.rg);
		float target = clamp(_TargetGray, 0, 1);
		float gray = max(0, min(0.999999, lerp(current, target, f)));
		float2 encodedHeight = GriffinEncodeFloatRG(gray);

		return saturate(float4(encodedHeight, currentColor.b, currentColor.a));
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		
        Pass
        {
			Name "Set Height"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragSetHeight
            ENDCG
        }
    }
}
