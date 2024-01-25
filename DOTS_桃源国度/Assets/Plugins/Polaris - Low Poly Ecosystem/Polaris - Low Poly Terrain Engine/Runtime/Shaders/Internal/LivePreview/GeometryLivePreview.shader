Shader "Hidden/Griffin/GeometryLivePreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_OldHeightMap ("Old Height Map", 2D) = "black" {}
		_NewHeightMap ("New Height Map", 2D) = "black" {}
		_Height ("Height", Float) = 1
		_Transparency ("Transparency", Range(0,1)) = 0.5
		_BoundMin ("Bound Min", Vector) = (0,0,0,1)
		_BoundMax ("Bound Max", Vector) = (1,1,1,1)
		_FadeDistance ("Fade Distance", Float) = 2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Off
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD2;
				float4 vertexColor : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _OldHeightMap;
			sampler2D _NewHeightMap;
			float _Height;
			float _Transparency;
			float4 _BoundMin;
			float4 _BoundMax;
			float _FadeDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 heightMapUv = float4(
					(worldPos.x-_BoundMin.x)/(_BoundMax.x-_BoundMin.x),
					(worldPos.z-_BoundMin.z)/(_BoundMax.z-_BoundMin.z),
					0, 0
				);
				float oldHeight = GriffinDecodeFloatRG(tex2Dlod(_OldHeightMap, heightMapUv).rg)*_Height;
				float newHeight = GriffinDecodeFloatRG(tex2Dlod(_NewHeightMap, heightMapUv).rg)*_Height;
				v.vertex.y = newHeight;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertexColor = float4(
					1,1,1,
					clamp(abs(newHeight-oldHeight)/_FadeDistance,0,1)
				);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 dpdx = ddx(i.worldPos);
				float3 dpdy = ddy(i.worldPos);
				float3 normal = normalize(cross(dpdy, dpdx));
				
                float4 col = float4(
					(normal.x + 1)*0.5,
					(normal.z + 1)*0.5,
					(normal.y + 1)*0.5,
					_Transparency*i.vertexColor.a);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
