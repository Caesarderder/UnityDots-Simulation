Shader "Hidden/Griffin/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 texel = _MainTex_TexelSize.xy;
				float4 avgColor = float4(0,0,0,0);
				float sampleCount = 0;
				for (int x0=-25; x0<=25; ++x0)
				{
					for (int y0=-25; y0<=25; ++y0)
					{
						avgColor += tex2D(_MainTex, i.uv + float2(x0*texel.x, y0*texel.y));
						sampleCount +=1;
					}
				}
				avgColor = avgColor/sampleCount;

                return avgColor;
            }
            ENDCG
        }
    }
}
