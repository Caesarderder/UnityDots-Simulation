Shader "Hidden/Polaris/SplineGizmos"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest LEqual
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float4 color: COLOR;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float4 color: COLOR;
			};

			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				return float4(_Color.rgb, _Color.a * i.color.a);
			}
			ENDCG

		}
	}
}
