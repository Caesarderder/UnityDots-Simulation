#ifndef CONDITIONAL_PAINTING_INCLUDED
	#define CONDITIONAL_PAINTING_INCLUDED

	#include "PaintToolCommon.cginc"

	//Include these lines in the main shader Pass code
	//#pragma shader_feature BLEND_HEIGHT
	//#pragma shader_feature BLEND_SLOPE
	//#pragma shader_feature BLEND_NOISE

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

	sampler2D _HeightMap;
	sampler2D _NormalMap;
	float4 _TerrainDimension;

	float ComputeRuleMask(float2 terrainUV)
	{
		float blendHeightValue = 1;
		float4 heightMapColor = tex2D(_HeightMap, terrainUV);
		float height = GriffinDecodeFloatRG(heightMapColor.rg);

		#if BLEND_HEIGHT
			float heightTransitionFactor = (height - _MinHeight) / (_MaxHeight - _MinHeight);
			float heightTransition = tex2D(_HeightTransition, float2(heightTransitionFactor, 0.5)).r;
			blendHeightValue = (height >= _MinHeight) * (height <= _MaxHeight) * heightTransition;
		#endif

		float blendSlopeValue = 1;
		#if BLEND_SLOPE
			float4 normalMapColor = tex2D(_NormalMap, terrainUV);
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

			float2 worldXZ = float2(_TerrainDimension.x + terrainUV.x * _TerrainDimension.z, _TerrainDimension.y + terrainUV.y * _TerrainDimension.w)/1000;

			for (int octave = 0; octave < 4; ++octave)
			{
				frequency = frequency * pow(_NoiseLacunarity, octave);
				amplitude = amplitude * pow(_NoisePersistence, octave);
				noisePos = _NoiseOrigin.xy + worldXZ * frequency;
				uv = float2(noisePos.x / refSize.x, noisePos.y / refSize.y);
				sampledNoise = GradientNoise(uv) * amplitude * (octave < _NoiseOctaves);
				noiseValue += sampledNoise;
			}

			float remapFactor = noiseValue * 0.5 + 0.5;
			blendNoiseValue = tex2D(_NoiseRemap, float2(remapFactor, 0.5)).a;
		#endif

		float result = blendHeightValue * blendSlopeValue * blendNoiseValue;
		return result;
	}

#endif // CONDITIONAL_PAINTING_INCLUDED