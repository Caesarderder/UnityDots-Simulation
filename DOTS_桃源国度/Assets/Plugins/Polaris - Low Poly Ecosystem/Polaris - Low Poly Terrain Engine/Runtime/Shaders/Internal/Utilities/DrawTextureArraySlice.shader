Shader "Hidden/Griffin/DrawTextureArraySlice"
{
    Properties
    {
        _MainTex ("Texture", 2DArray) = "" {}
        _SliceIndex ("Slice Index", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            float4 _MainTex_ST;
            int _SliceIndex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.z = _SliceIndex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
