Shader "Hidden/Griffin/CurveFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
		_MasterCurve ("Master Curve", 2D) = "black" {}
		_RedCurve ("Red Curve", 2D) = "black" {}
		_GreenCurve ("Green Curve", 2D) = "black" {}
		_BlueCurve ("Blue Curve", 2D) = "black" {}
		_AlphaCurve ("Alpha Curve", 2D) = "black" {}
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
	sampler2D _MasterCurve;
	sampler2D _RedCurve;
	sampler2D _GreenCurve;
	sampler2D _BlueCurve;
	sampler2D _AlphaCurve;

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
		float r = tex2D(_RedCurve, col.r).r;
		float g = tex2D(_GreenCurve, col.g).r;
		float b = tex2D(_BlueCurve, col.b).r;
		float a = tex2D(_AlphaCurve, col.a).r;

		r = tex2D(_MasterCurve, r).r;
		g = tex2D(_MasterCurve, g).r;
		b = tex2D(_MasterCurve, b).r;
		a = tex2D(_MasterCurve, a).r;

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
