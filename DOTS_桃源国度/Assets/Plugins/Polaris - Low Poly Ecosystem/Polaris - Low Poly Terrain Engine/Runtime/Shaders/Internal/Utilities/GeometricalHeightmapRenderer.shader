Shader "Hidden/Griffin/GeometricalHeightMapRenderer"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
		Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UtilitiesCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 localPos : TEXCOORD1;
            };
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.localPos = v.vertex;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float height = saturate(i.localPos.z);
                float2 enc = GriffinEncodeFloatRG(height);
				return float4(enc.x, enc.y, 0, 1);
            }
            ENDCG
        }
    }
}
