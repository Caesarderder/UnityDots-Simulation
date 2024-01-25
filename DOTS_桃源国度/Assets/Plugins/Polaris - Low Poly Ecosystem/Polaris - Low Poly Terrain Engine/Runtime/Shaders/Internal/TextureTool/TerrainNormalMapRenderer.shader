Shader "Hidden/Griffin/TerrainNormalMapRenderer"
{
    Properties
    {
		_TangentSpace("Tangent Space", Int) = 0
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

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : TEXCOORD0; //normal vector send to this channel in GL code
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
            };

			int _TangentSpace;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float3 n = _TangentSpace*float3(i.normal.x, i.normal.z, i.normal.y) + (1-_TangentSpace)*i.normal;
                float3 normal = normalize(n);
				float3 col = float3(
					(normal.x + 1)/2,
					(normal.y + 1)/2,
					(normal.z + 1)/2);
				#ifdef UNITY_COLORSPACE_GAMMA
				return float4(col,1);
				#else
				return float4(GammaToLinearSpace(col.rgb), 1);
				#endif
            }
            ENDCG
        }
    }
}
