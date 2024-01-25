Shader "Hidden/Griffin/HeightMapFromMesh"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float depth01 : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth01 = -UnityObjectToViewPos( v.vertex.xyz ).z * _ProjectionParams.w;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float gray = saturate(1-i.depth01);
				return float4(gray, gray, gray, 1);
            }
            ENDCG
        }
    }
}
