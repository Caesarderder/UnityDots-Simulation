#ifndef TREE_BILLBOARD_COMMON_URP
#define TREE_BILLBOARD_COMMON_URP

float4 _ImageTexcoords[256];
int _ImageCount;

void GetImageTexcoord_float(float2 UV0, float3 WorldNormal, out float2 BillboardUV)
{
	float dotZ = dot(WorldNormal, float3(0, 0, 1));
	float dotX = dot(WorldNormal, float3(1, 0, 0));
	float rad = atan2(dotZ, dotX);
	rad = (rad + TWO_PI) % TWO_PI;
	float f = rad / TWO_PI;
	int imageIndex = f * _ImageCount;

	float4 rect = _ImageTexcoords[imageIndex];
	float2 min = rect.xy;
	float2 max = rect.xy + rect.zw;

	float2 result = float2(
		lerp(min.x, max.x, UV0.x),
		lerp(min.y, max.y, UV0.y));
	BillboardUV = result;
}

#endif