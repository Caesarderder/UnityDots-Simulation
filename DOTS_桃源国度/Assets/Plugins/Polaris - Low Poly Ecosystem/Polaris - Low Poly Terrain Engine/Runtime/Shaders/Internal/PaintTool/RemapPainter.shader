Shader "Hidden/Griffin/RemapPainter"
{
    Properties
    {
		_MainTex ("MainTex", 2D) = "black" {}
		_Mask ("Mask", 2D) = "white" {}
		_Opacity ("Opacity", Float) = 1
		_RemapTex ("Remap Texture", 2D) = "black" {}
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
	float4 _MainTex_TexelSize;
	sampler2D _Mask;
	float _Opacity;
	sampler2D _RemapTex;
	sampler2D _TerrainMask;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.localPos = v.vertex;
		return o;
    }

	fixed4 frag(v2f i) : SV_Target
	{
		float terrainMask = 1 - tex2D(_TerrainMask, i.localPos).r;

		float4 maskColor = tex2D(_Mask, i.uv);
		float4 currentColor = tex2D(_MainTex, i.localPos);
		float currentHeight = GriffinDecodeFloatRG(currentColor.rg);
		float desHeight = tex2D(_RemapTex, float2(currentHeight, 0.5));

		float height = max(0, min(0.999999, lerp(currentHeight, desHeight, maskColor.r*_Opacity*terrainMask)));
		float2 encodedHeight = GriffinEncodeFloatRG(height);

		return saturate(float4(encodedHeight.rg, currentColor.b, currentColor.a));
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		
        Pass
        {
			Name "Raise"
			Blend One Zero
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
