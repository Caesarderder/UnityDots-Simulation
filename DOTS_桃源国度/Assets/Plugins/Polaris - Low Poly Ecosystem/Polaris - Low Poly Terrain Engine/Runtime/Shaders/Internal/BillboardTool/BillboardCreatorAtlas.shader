Shader "Griffin/~Internal/Billboard Creator/Atlas"
{
    Properties
    {
		_Color ("_Color", Color) = (1,1,1,1)
        _MainTex ("_Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest+0"} 
        LOD 100
        Pass
        {
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			//ZWrite Off
			//ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
				clip(col.a-0.5);
                return col;
            }
            ENDCG
        }
    }
}
