Shader "Hidden/Griffin/PainterCursorProjector"
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
		float3 normal : NORMAL;
    };

    struct v2f
    {
		float4 vertex : SV_POSITION;
		float4 worldPos : TEXCOORD1;
		float3 normal : NORMAL;
    };

	float4 _Color;
    sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4x4 _WorldToCursorMatrix;

	v2f vert (appdata v)
    {
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.normal = UnityObjectToWorldNormal(v.normal);
		return o;
    }

	fixed4 frag (v2f i) : SV_Target
    {
		float4 cursorSpacePos = mul(_WorldToCursorMatrix, i.worldPos);
		clip(0.5 - abs(cursorSpacePos.x));
		clip(0.5 - abs(cursorSpacePos.z));

		float2 uv = float2(cursorSpacePos.x + 0.5, cursorSpacePos.z + 0.5);
		float texGray = tex2D(_MainTex, uv).r;
		float fade = lerp(0.5, 1, i.normal.y);
		float4 color = float4(fade*_Color.rgb, _Color.a*texGray);

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
			Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
