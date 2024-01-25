Shader "Hidden/Griffin/ErosionGeometryLivePreview"
{
	Properties
	{
		_Transparency ("Transparency", Range(0, 1)) = 0.5
		_FadeDistance ("Fade Distance", Float) = 0.5
		_SimulationData ("Sim Data", 2D) = "black" { }
		_FalloffTexture ("Falloff Texture", 2D) = "black" { }
		_HeightMap ("Height Map", 2D) = "black" { }
		_Height ("Height", Float) = 1
		_ErosionMap ("Erosion Map", 2D) = "black" { }
		_ErosionSplat ("Erosion Splat", 2D) = "black" { }
		_DepositionSplat ("Deposition Splat", 2D) = "black" { }
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
			#pragma shader_feature_local SHOW_TEXTURE
			#pragma shader_feature_local SHOW_COLOR

			#include "UnityCG.cginc"
			#include "LivePreviewCommon.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex: SV_POSITION;
				float4 worldPos: TEXCOORD2;
				float4 vertexColor: TEXCOORD3;
			};

			float _Transparency;
			float _FadeDistance;
			float4x4 _WorldToSim;
			float4x4 _WorldToTerrainUV;
			sampler2D _SimulationData;
			sampler2D _FalloffTexture;
			sampler2D _HeightMap;
			float _Height;

			sampler2D _ErosionMap;
			sampler2D _ErosionSplat;
			float4 _ErosionSplat_ST;
			float _ErosionIntensity;
			float4 _ErosionAlbedo;

			sampler2D _DepositionSplat;
			float4 _DepositionSplat_ST;
			float _DepositionIntensity;
			float4 _DepositionAlbedo;

			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 simPos = mul(_WorldToSim, float4(worldPos.xyz, 1));

				float2 terrainUV = mul(_WorldToTerrainUV, float4(worldPos.xyz, 1)).xz;
				float oldHeight = GriffinDecodeFloatRG(tex2Dlod(_HeightMap, float4(terrainUV, 0, 0)).rg) * _Height;
				
				float2 simUv = simPos.xz + float2(0.5, 0.5);
				float newHeight = tex2Dlod(_SimulationData, float4(simUv, 0, 0));

				float f = saturate(2 * length(simUv - float2(0.5, 0.5)));
				float falloff = tex2Dlod(_FalloffTexture, float4(f, 0.5, 0, 0)).r;
				newHeight = lerp(oldHeight, newHeight, falloff);

				v.vertex.y = newHeight;

				o.uv = simUv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertexColor = float4(
					1, 1, 1,
					clamp(abs(newHeight - oldHeight) / _FadeDistance, 0, 1)
				);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				int isInside = (i.uv.x >= 0) * (i.uv.x <= 1) * (i.uv.y >= 0) * (i.uv.y <= 1);
				if (isInside == 0)
				{
					discard;
				}

				float4 color = 0;
				#if SHOW_TEXTURE || SHOW_COLOR
					float4 simPos = mul(_WorldToSim, float4(i.worldPos.xyz, 1));
					float2 simUv = simPos.xz + float2(0.5, 0.5);
					float2 terrainUV = mul(_WorldToTerrainUV, float4(i.worldPos.xyz, 1)).xz;

					float f = saturate(2 * length(simUv - float2(0.5, 0.5)));
					float falloff = tex2Dlod(_FalloffTexture, float4(f, 0.5, 0, 0)).r;

					float2 erosionData = tex2D(_ErosionMap, simUv).rg;

					float4 erosionColor = 0;
					#if SHOW_TEXTURE
						erosionColor = tex2D(_ErosionSplat, terrainUV * _ErosionSplat_ST.xy + _ErosionSplat_ST.zw);
					#elif SHOW_COLOR
						erosionColor = _ErosionAlbedo;
					#endif
					color = lerp(float4(0, 0, 0, 0), erosionColor, saturate(_ErosionIntensity * erosionData.r));

					float4 depositionColor = 0;
					#if SHOW_TEXTURE
						depositionColor = tex2D(_DepositionSplat, terrainUV * _DepositionSplat_ST.xy + _DepositionSplat_ST.zw);
					#elif SHOW_COLOR
						depositionColor = _DepositionAlbedo;
					#endif
					color = lerp(color, depositionColor, saturate(_DepositionIntensity * erosionData.g));

					color.a = saturate(falloff * max(_ErosionIntensity * erosionData.r, _DepositionIntensity * erosionData.g));
				#else
					float3 dpdx = ddx(i.worldPos);
					float3 dpdy = ddy(i.worldPos);
					float3 normal = normalize(cross(dpdy, dpdx));
					
					color = float4((normal.x + 1) * 0.5, (normal.z + 1) * 0.5, (normal.y + 1) * 0.5, _Transparency * i.vertexColor.a);
				#endif
				UNITY_APPLY_FOG(i.fogCoord, col);
				return color;
			}
			ENDCG

		}
	}
}
