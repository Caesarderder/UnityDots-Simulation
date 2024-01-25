Shader "Hidden/Griffin/InvertFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
		_InvertRed ("Invert Red", Int) = 1
		_InvertGreen ("Invert Green", Int) = 1
		_InvertBlue ("Invert Blue", Int) = 1
		_InvertAlpha ("Invert Alpha", Int) = 1
    }

	CGINCLUDE
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
	int _InvertRed;
	int _InvertGreen;
	int _InvertBlue;
	int _InvertAlpha;

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
		float r = _InvertRed*(1-col.r) + (1-_InvertRed)*col.r;
		float g = _InvertGreen*(1-col.g) + (1-_InvertGreen)*col.g;
		float b = _InvertBlue*(1-col.b) + (1-_InvertBlue)*col.b;
		float a = _InvertAlpha*(1-col.a) + (1-_InvertAlpha)*col.a;

		return float4(r, g, b, a);
	}

	ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
