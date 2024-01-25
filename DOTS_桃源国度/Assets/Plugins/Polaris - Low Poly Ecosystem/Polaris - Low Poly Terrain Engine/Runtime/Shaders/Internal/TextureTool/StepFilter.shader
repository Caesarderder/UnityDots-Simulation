Shader "Hidden/Griffin/StepFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
		_Count ("Count", Int) = 256
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
	int _Count;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float stepValue(float v)
	{
		float step = 1.0/_Count;
		return v-v%step;
	}

	float4 frag (v2f i) : SV_Target
    {
		float4 col = tex2D(_MainTex, i.uv);
		float r = stepValue(col.r);
		float g = stepValue(col.g);
		float b = stepValue(col.b);
		float a = stepValue(col.a);

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
