#ifndef GRASS_COMMON_URP
#define GRASS_COMMON_URP

float4x4 _WorldToNormalized;

void GetWorldToNormalizedMatrix_float(out float4x4 WorldToNormalized)
{
	WorldToNormalized = _WorldToNormalized;
}

#endif