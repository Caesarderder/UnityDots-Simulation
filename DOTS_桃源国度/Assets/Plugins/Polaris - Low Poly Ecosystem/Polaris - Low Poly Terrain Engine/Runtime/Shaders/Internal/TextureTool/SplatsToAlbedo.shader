Shader "Hidden/Griffin/SplatsToAlbedo"
{
    Properties
    {
		_Control0 ("Control0 (RGBA)", any) = "black" {}
		_Splat0 ("Splat0 (R)", 2D) = "white" {}
		_Splat1 ("Splat1 (G)", 2D) = "white" {}
		_Splat2 ("Splat2 (B)", 2D) = "white" {}
		_Splat3 ("Splat3 (A)", 2D) = "white" {}
    }

	CGINCLUDE
    #include "UnityCG.cginc"
#include "TextureToolCommon.cginc"
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

	sampler2D _Control0;
	sampler2D _Splat0;
	sampler2D _Splat1;
	sampler2D _Splat2;
	sampler2D _Splat3;
	fixed4 _Splat0_ST;
	fixed4 _Splat1_ST;
	fixed4 _Splat2_ST;
	fixed4 _Splat3_ST;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	fixed4 frag (v2f i) : SV_Target
    {
		fixed4 control;
		fixed4 splat;
		fixed weight;
		Sample4Splats(
			_Control0,
			_Splat0, _Splat1, _Splat2, _Splat3,
			_Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST,
			i.uv, control, splat, weight);
		return splat;
	}
	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		
        Pass
        {
			Blend One One
			BlendOp Add
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
