Shader "Unlit/Selection"
{
    Properties
    {
        _Color("Color", Color) = (0, 1, 0, 1)
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionSS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionSS = UnityObjectToClipPos(i.positionOS);
                o.uv = i.texcoord;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
