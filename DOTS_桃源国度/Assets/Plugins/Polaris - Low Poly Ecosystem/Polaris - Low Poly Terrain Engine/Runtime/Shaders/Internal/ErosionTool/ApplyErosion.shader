Shader "Hidden/Polaris/ApplyErosion"
{
    Properties
    {
        _HeightMap ("Height Map", 2D) = "white" {}
        _SimulationData("Simulation Data", 2D) = "black"{}
        _FalloffTexture("Falloff", 2D) = "black"{}
        _Bounds("Bounds", Vector) = (1,1,1,1)
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

            #include "ErosionToolCommon.cginc"

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

            sampler2D _HeightMap;
            sampler2D _SimulationData;
            sampler2D _FalloffTexture;
            float4 _Bounds;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex;
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 data = tex2D(_HeightMap, i.localPos);

                float currentHeight = DecodeFloatRG(data.rg);
                float simHeight = tex2D(_SimulationData, i.uv).r/_Bounds.y;
                
		        float f = saturate(2*length(i.uv - float2(0.5,0.5)));
		        float falloff = tex2D(_FalloffTexture, float2(f, 0.5)).r;

                float h = lerp(currentHeight, simHeight, falloff);
                data.rg = EncodeFloatRG(min(0.999999, h));

                return data;
            }
            ENDCG
        }
    }
}
