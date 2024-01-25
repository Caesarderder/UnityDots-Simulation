Shader "Unlit/LifeBar"
{
    Properties
    {
        _Color("Color", Color) = (0, 1, 0, 1)
        _MainTex("Texture", 2D) = "white" {}
        _Width("Width", Float) = 1
        _Height("Height", Float) = 1
        _Border("Border", Float) = 0.1
        _Split("Split", Integer) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

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
            float _Width;
            float _Height;
            float _Progress;
            float _Border;
            int _Split;

            Varyings vert(Attributes i)
            {
                Varyings o;

                o.positionSS = mul(UNITY_MATRIX_P, 
                    mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
                    + float4(i.positionOS.x, i.positionOS.y, 0.0, 0.0)
                    * float4(_Width, _Height, 1.0, 1.0));

                o.uv = i.texcoord;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;

                bool isBlack = false;

                // If health is below treshold
                isBlack = isBlack || (i.uv.x >= _Progress);

                // If borders
                float aspectRatio = _Height / _Width;
                float2 border = float2(_Border * aspectRatio, _Border);
                isBlack = isBlack || any(i.uv < border);
                isBlack = isBlack || any(i.uv > 1 - border);

                // If split
                float stride = 1.0 / (_Split + 1.0);
                float split = 0;
                float2 uv = i.uv;
                for (int i = 0; i < _Split; ++i)
                {
                    split += stride;
                    isBlack = isBlack || ((split - border.x) < uv.x && uv.x < (split + border.x));
                }

                if (isBlack)
                    col = float4(0, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}
