Shader "Hidden/Griffin/Mask4Channels"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		ZTest LEqual
        Offset -1, -1
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "LivePreviewCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o; 
                o.uv = v.uv;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 Blend(fixed4 src, fixed4 des)
            {
                return src*src.a + des*(1-src.a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenPos = i.screenPos.xy / i.screenPos.w;
                float2 pixel = screenPos.xy * _ScreenParams.xy;

                if (floor(pixel.x) % 2 != 0 || floor(pixel.y) % 2 != 0)
                {
                    discard;
                }

				fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 baseColor = fixed4(0,0,0,0);
                fixed4 red = fixed4(1,0,0,texColor.r);
                fixed4 green = fixed4(0,1,0,texColor.g);
                fixed4 blue = fixed4(0,0,1,texColor.b);
                fixed4 alpha = fixed4(0.8, 0.8, 0.8, texColor.a);
                
                fixed4 rgb=baseColor;
                rgb = Blend(red, rgb);
                rgb = Blend(green,rgb);
                rgb = Blend(blue, rgb);
                rgb = Blend(alpha, rgb);
                rgb.a = max(max(red.a, green.a), max(blue.a, alpha.a));
                
                UNITY_APPLY_FOG(i.fogCoord, rgb);
                return rgb;
            }
            ENDCG
        }
    }
}
