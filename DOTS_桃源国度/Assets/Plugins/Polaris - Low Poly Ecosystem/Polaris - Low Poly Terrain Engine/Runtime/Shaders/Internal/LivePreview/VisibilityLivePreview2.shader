Shader "Hidden/Griffin/VisibilityLivePreview2" 
{
	Properties 
	{
		_Color ("Line Color", Color) = (1,1,1,1)

		_HeightMap ("Height Map", 2D) = "black" {}
		_Dimension ("Dimension", Vector) = (0,0,0,0)

		_SubdivMap ("Subdiv Map", 2D) = "black" {}
		_SubdivRange("Subdiv Range", Vector) = (0,0,0,0)

		_Mask ("Mask", 2D) = "white" {}
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="AlphaTest" "Queue"="Overlay" }

			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off
			ZTest Always
			LOD 200
			
			CGPROGRAM
				#include "UnityCG.cginc"
				#include "LivePreviewCommon.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ USE_MASK

				struct v2f
				{
					float4	pos		: POSITION;
					float2  heightMapUv : TEXCOORD1;
					float4  worldPos : TEXCOORD2;
				};

				float4 _Color;
				sampler2D _HeightMap;
				float4 _Dimension;			
				float4 _BoundMin;
				float4 _BoundMax;
				sampler2D _SubdivMap;
				float4 _SubdivRange;
				sampler2D _Mask;
				float4x4 _WorldPointToMask;

				v2f vert(appdata_base v)
				{
					v2f output;

					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					float4 heightMapUv = float4(
						(worldPos.x-_BoundMin.x)/(_BoundMax.x-_BoundMin.x),
						(worldPos.z-_BoundMin.z)/(_BoundMax.z-_BoundMin.z),
						0, 0
					);
					float newHeight = GriffinDecodeFloatRG(tex2Dlod(_HeightMap, heightMapUv).rg)*_Dimension.y;
					v.vertex.y = newHeight;
					
					output.pos =  UnityObjectToClipPos(v.vertex);
					output.heightMapUv = heightMapUv;
					output.worldPos = worldPos;

					return output;
				}

				float4 frag(v2f input) : COLOR
				{	
					#if USE_MASK
					float2 maskUv = mul(_WorldPointToMask, input.worldPos).xz;
					if (maskUv.x < 0) discard;
					if (maskUv.x > 1) discard;
					if (maskUv.y < 0) discard;
					if (maskUv.y > 1) discard;
					#endif

					float visibilityValue = tex2D(_HeightMap, input.heightMapUv.xy).a;
					if (visibilityValue >= 0.5) 
						discard;

					float subdivValue = tex2D(_SubdivMap, input.heightMapUv.xy).r;

					#if USE_MASK
					float maskValue = tex2D(_Mask, maskUv).r;
					#else
					float maskValue = 1;
					#endif

					float4 col = _Color;
					col.a *= col.a*maskValue*(subdivValue >= _SubdivRange.x)*(subdivValue <= _SubdivRange.y);

					return col;
				}
			
			ENDCG
		}
	} 
}
