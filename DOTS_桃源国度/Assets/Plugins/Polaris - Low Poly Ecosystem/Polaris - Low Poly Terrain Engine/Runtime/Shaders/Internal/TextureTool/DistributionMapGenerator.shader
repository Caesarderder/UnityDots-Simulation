Shader "Hidden/Griffin/DistributionMapGenerator"
{
    Properties
    {
		_MainTex ("Main Texture", 2D) = "white" {}
		_Opacity ("Opacity", Float) = 1
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

	sampler2D _MainTex;
	float _Opacity;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float4 frag (v2f i) : SV_Target
    {
		float4 col = tex2D(_MainTex, i.uv);
		return float4(1, 1, 1, col.r*_Opacity);
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
