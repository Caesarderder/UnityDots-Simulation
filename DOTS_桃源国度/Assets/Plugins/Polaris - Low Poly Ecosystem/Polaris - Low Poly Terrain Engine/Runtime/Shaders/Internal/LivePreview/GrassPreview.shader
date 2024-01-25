Shader "Hidden/Griffin/~Internal/GrassPreview"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 0, 0.5)
		//_MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
		ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			//UNITY_INSTANCING_BUFFER_START(Props)
                //UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				//UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
            //UNITY_INSTANCING_BUFFER_END(Props)

			//CBUFFER_START(UnityPerMaterial)
            float4 _Color;
			//CBUFFER_END
			//sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                //UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//UNITY_SETUP_INSTANCE_ID(i); 
				//float4 texColor = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex), i.uv);
				///float4 col = float4(texColor.rgb*_Color.rgb, texColor.a*_Color.a);
                //return col;
				return _Color;
				//return float4(0,1,0,0.5);
            }
            ENDCG
        }
    }
}
