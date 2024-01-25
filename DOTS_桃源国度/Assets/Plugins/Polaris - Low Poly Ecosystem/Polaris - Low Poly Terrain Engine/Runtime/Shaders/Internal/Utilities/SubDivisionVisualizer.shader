Shader "Hidden/Griffin/SubDivisionVisualizer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Step ("Step", Float) = 0.1

		_Color0 ("C0", Color) = (1,0,0,1)
		_Color1 ("C1", Color) = (0,1,0,1)
		_Color2 ("C2", Color) = (0,0,1,1)
		_Color3 ("C3", Color) = (1,1,0,1)
		_Color4 ("C4", Color) = (0,1,1,1)
		_Color5 ("C5", Color) = (1,0,1,1)
		_Color6 ("C6", Color) = (1,1,1,1)
		_Color7 ("C7", Color) = (1,0.5,0,1)
		_Color8 ("C8", Color) = (0,1,0.5,1)
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Step;
			float4 _Color0;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;
			float4 _Color8;



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 layerColors[9] = {
					_Color0,
					_Color1,
					_Color2,
					_Color3,
					_Color4,
					_Color5,
					_Color6,
					_Color7,
					_Color8
				};
			
                fixed4 col = tex2D(_MainTex, i.uv);
				float gray = col.r;
				int colorIndex = (int)(gray/_Step);
				
                return layerColors[colorIndex];
            }
            ENDCG
        }
    }
}
