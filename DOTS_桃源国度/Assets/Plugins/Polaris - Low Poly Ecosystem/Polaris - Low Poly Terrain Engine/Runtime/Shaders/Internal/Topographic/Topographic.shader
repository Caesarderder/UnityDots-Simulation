Shader "Griffin/~Internal/Topographic"
{
	Properties
	{
		_Saturation("Saturation", Range(0,1)) = 1
		_Value("Value", Range(0,1)) = 1
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
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 localPos : TEXCOORD0;
				float3 normal : NORMAL;
			};

			float _Saturation;
			float _Value;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;
				o.normal = v.normal;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float f = frac(i.localPos.y / 360);

				float h = f * 359.0;
				float s = _Saturation;
				float v = _Value;

				float mod60 = h % 60;
				float ep60 = 0.5;
				float mod6 = h % 10;
				float ep6 = 0.25;

				if (h < 5)
				{

				}
				else if (mod60 <= ep60)
				{
					v *= 0.65;
				}
				else if (mod6 <= ep6)
				{
					v *= 0.85;
				}

				float c = s * v;
				float x = c * (1 - abs((h / 60) % 2 - 1));
				float m = v - c;
				float3 rgb0 =
						fixed3(c, x, 0) * (0 <= h) * (h < 60) +
						fixed3(x, c, 0) * (60 <= h) * (h < 120) +
						fixed3(0, c, x) * (120 <= h) * (h < 180) +
						fixed3(0, x, c) * (180 <= h) * (h < 240) +
						fixed3(x, 0, c) * (240 <= h) * (h < 300) +
						fixed3(c, 0, x) * (300 <= h) * (h < 360);
				float3 Albedo = rgb0 + float3(m, m, m);

				float3 viewDir = UNITY_MATRIX_V[2].xyz;
				float3 worldNormal = mul(unity_ObjectToWorld, i.normal.xyz);
				float nDotV = dot(worldNormal, viewDir);
				nDotV = lerp(0.5, 1, nDotV);
				Albedo *= nDotV;

				float fresnel = pow(1 - nDotV, 3);
				Albedo += fresnel;

				fixed4 col = float4(Albedo.rgb, 1);
				return col;
			}
			ENDCG
		}
	}
}
