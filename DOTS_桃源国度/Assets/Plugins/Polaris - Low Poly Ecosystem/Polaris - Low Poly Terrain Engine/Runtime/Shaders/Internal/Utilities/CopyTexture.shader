Shader "Hidden/Griffin/CopyTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_StartUV ("Start UV", Vector) = (0,0,0,0)
		_EndUV ("End UV", Vector) = (1,1,1,1)
		_DefaultColor ("Default Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
			float4 _StartUV;
			float4 _EndUV;
			float4 _DefaultColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			float2 TransformUV(float2 uv)
			{
				return lerp(_StartUV, _EndUV, uv);
			}

			float IsInRange01(float2 uv)
			{
				return uv.x>=0 && uv.x<=1 && uv.y>=0 && uv.y<=1;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                float2 transformedUV = TransformUV(i.uv);
				float4 texColor = tex2D(_MainTex, transformedUV);
				float inRange = IsInRange01(transformedUV);
				float4 result = texColor*inRange + _DefaultColor*(1-inRange);
                return result;
            }
            ENDCG
        }
    }
}
