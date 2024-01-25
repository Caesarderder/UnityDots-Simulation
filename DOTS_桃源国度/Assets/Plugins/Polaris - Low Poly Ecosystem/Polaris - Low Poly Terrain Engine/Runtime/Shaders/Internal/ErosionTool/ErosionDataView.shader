Shader "Hidden/Polaris/ErosionDataView"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_Scale ("Scale", float) = 1
		_Channel ("Channel", float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
			};

			sampler2D _MainTex;
			float _Scale;
			float _Channel;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i): SV_Target
			{
				float4 data = tex2D(_MainTex, i.uv);
				float value = data[_Channel] * _Scale;
				
				float4 tint = float4(1, 0, 0, 1) * (_Channel == 0) + float4(0, 1, 0, 1) * (_Channel == 1) + float4(0, 0, 1, 1) * (_Channel == 2) + float4(1, 1, 1, 1) * (_Channel == 3);

				return tint * value;
			}
			ENDCG

		}
	}
}
