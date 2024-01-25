Shader "Hidden/Griffin/SteepnessMapGenerator"
{
    Properties
    {
		_BumpMap ("Bump Map", 2D) = "bump" {}
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

	sampler2D _BumpMap;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
    }

	float4 frag (v2f i) : SV_Target
    {
		float4 col = tex2D(_BumpMap, i.uv);
		float3 normalY = col.g*2 - 1;
		float steepness = 1 - normalY;

		return float4(steepness, steepness, steepness, 1);
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
