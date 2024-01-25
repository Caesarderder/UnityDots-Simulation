Shader "Hidden/Griffin/ChannelToGrayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ChannelIndex ("Channel Index", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
		ZTest Always
		ZWrite Off

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
			float _ChannelIndex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
				float value = col.r*(_ChannelIndex==0) + col.g*(_ChannelIndex==1) + col.b*(_ChannelIndex==2) + col.a*(_ChannelIndex==3);

				float4 result = float4(value, value, value, 1);
                return result;
            }
            ENDCG
        }
    }
}
