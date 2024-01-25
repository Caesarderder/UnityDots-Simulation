Shader "Hidden/Griffin/MaskVisualizer"
{
    Properties
    {
		_Color ("Color", Color) = (1,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Channel ("Channel", Int) = 0
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

			fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _Channel;

            v2f vert (appdata v)
            {
                v2f o; 
                o.uv = v.uv;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
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
				fixed4 col = float4(_Color.rgb, _Color.a*texColor[_Channel]);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
