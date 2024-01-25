Shader "Hidden/Griffin/TextureStamperBrush"
{
	Properties
	{
		_Mask ("Mask", 2D) = "white" { }
		_Falloff ("Falloff", 2D) = "black" { }
		_MinHeight ("Min Height", Range(0.0, 1.0)) = 0
		_MaxHeight ("Max Height", Range(0.0, 1.0)) = 1
		_HeightTransition ("Height Transition", 2D) = "white" { }
		_MinSlope ("Min Slope", Range(0.0, 1.0)) = 0
		_MaxSlope ("Max Slope", Range(0.0, 1.0)) = 1
		_SlopeTransition ("Slope Transition", 2D) = "white" { }
		_NoiseOrigin ("Noise Origin", Vector) = (0, 0, 0, 0)
		_NoiseFrequency ("Noise Frequency", Float) = 1
		_NoiseOctaves ("Noise Octaves", Int) = 1
		_NoiseLacunarity ("Noise Lacunarity", Float) = 2
		_NoisePersistence ("Noise Persistence", Float) = 0.5
		_NoiseRemap ("Noise Remap", 2D) = "gray" { }

		_HeightMap ("Height Map", 2D) = "black" { }
		_NormalMap ("Normal Map", 2D) = "gray" { }

		_TerrainMask ("Terrain Mask", 2D) = "black" { }
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Blend One Zero

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature BLEND_HEIGHT
			#pragma shader_feature BLEND_SLOPE
			#pragma shader_feature BLEND_NOISE

			#include "UnityCG.cginc"
			#include "StampToolCommon.cginc"

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 normalizePos: TEXCOORD1;
			};

			sampler2D _Mask;
			sampler2D _Falloff;
			float _MinHeight;
			float _MaxHeight;
			sampler2D _HeightTransition;
			float _MinSlope;
			float _MaxSlope;
			sampler2D _SlopeTransition;
			float4 _NoiseOrigin;
			float _NoiseFrequency;
			int _NoiseOctaves;
			float _NoiseLacunarity;
			float _NoisePersistence;
			sampler2D _NoiseRemap;

			float _StamperMinHeight;
			float _StamperMaxHeight;

			sampler2D _HeightMap;
			sampler2D _NormalMap;
			float4x4 _LocalToWorld;

			sampler2D _TerrainMask;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normalizePos = v.vertex;
				return o;
			}

			float4 frag(v2f i): SV_Target
			{
				float blendHeightValue = 1;
				float4 heightMapColor = tex2D(_HeightMap, i.normalizePos);
				float height = GriffinDecodeFloatRG(heightMapColor.rg);

				#if BLEND_HEIGHT
					float heightTransitionFactor = (height - _MinHeight) / (_MaxHeight - _MinHeight);
					float heightTransition = tex2D(_HeightTransition, float2(heightTransitionFactor, 0.5)).r;
					blendHeightValue = (height >= _MinHeight) * (height <= _MaxHeight) * heightTransition;
				#endif

				float blendSlopeValue = 1;
				#if BLEND_SLOPE
					float4 normalMapColor = tex2D(_NormalMap, i.normalizePos);
					float3 normalVector = normalMapColor * 2 - float3(1, 1, 1);
					float cosine = abs(normalVector.y);
					float slopeAngle = acos(cosine);
					float slopeTransitionFactor = (slopeAngle - _MinSlope) / (_MaxSlope - _MinSlope);
					float slopeTransition = tex2D(_SlopeTransition, float2(slopeTransitionFactor, 0.5f)).r;
					blendSlopeValue = (slopeAngle >= _MinSlope) * (slopeAngle <= _MaxSlope) * slopeTransition;
				#endif

				float blendNoiseValue = 1;
				#if BLEND_NOISE
					float2 refSize = float2(1, 1);
					float noiseValue = 0;
					float frequency = _NoiseFrequency;
					float amplitude = 1;
					float2 noisePos;
					float2 uv;
					float sampledNoise;

					for (int octave = 0; octave < 4; ++octave)
					{
						frequency = frequency * pow(_NoiseLacunarity, octave);
						amplitude = amplitude * pow(_NoisePersistence, octave);
						noisePos = _NoiseOrigin.xy + i.uv * frequency;
						uv = float2(noisePos.x / refSize.x, noisePos.y / refSize.y);
						sampledNoise = GradientNoise(uv) * amplitude * (octave < _NoiseOctaves);
						noiseValue += sampledNoise;
					}

					float remapFactor = noiseValue * 0.5 + 0.5;
					blendNoiseValue = tex2D(_NoiseRemap, float2(remapFactor, 0.5)).a;
				#endif

				float4 maskColor = tex2D(_Mask, i.uv);
				float maskValue = maskColor.r;

				float falloffFactor = saturate(2 * length(i.uv - float2(0.5, 0.5)));
				float4 falloffColor = tex2D(_Falloff, float2(falloffFactor, 0.5));
				float falloffValue = falloffColor.r;
				float insideStamperValue = (height >= _StamperMinHeight) * (height <= _StamperMaxHeight);
				float result = blendHeightValue * blendSlopeValue * blendNoiseValue * maskValue * falloffValue * insideStamperValue;

				float terrainMask = 1 - tex2D(_TerrainMask, i.normalizePos).r;
				result = lerp(0, result, terrainMask);

				return result;
			}
			ENDCG

		}
	}
}
