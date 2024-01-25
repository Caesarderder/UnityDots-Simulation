Shader "Hidden/Griffin/PainterCursor"
{
    Properties
    {
		_Color ("Color", Color) = (0,1,1,1)
		_MainTex ("Main Texture", 2D) = "black" {}
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

	float4 _Color;
    sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float Stripe(in float x, in float stripeX, in float pixelWidth)
	{
		// compute derivatives to get ddx / pixel
		float2 derivatives = float2(ddx(x), ddy(x));
		float derivLen = length(derivatives);
		float sharpen = 1.0f / max(derivLen, 0.00001f);
		return saturate(0.5f + 0.5f * (0.5f * pixelWidth - sharpen * abs(x - stripeX)));
	}

	fixed4 frag (v2f i) : SV_Target
    {
		float texGray = tex2D(_MainTex, i.uv).r;

		float stripeWidth = 2.0f;       // pixels
        float stripeLocation = 0.05f;    // at 5% alpha
        float brushStripe = Stripe(texGray, stripeLocation, stripeWidth);
		float4 color = _Color;
		color.a = _Color.a*texGray*(brushStripe<0.5) + _Color*(brushStripe>=0.5);
		
		return color;
	}

	ENDCG

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
		
        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			BlendOp Add
			ZTest Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
