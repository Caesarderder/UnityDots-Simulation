#ifndef EROSION_COMMON_INCLUDED
#define EROSION_COMMON_INCLUDED

#define M2CM 100
#define CM2M 0.01
#define DT 1.0/30.0
#define G 9.8
#define FLOW_CONST 0.5

#define LEFT 0
#define TOP 1
#define RIGHT 2
#define BOTTOM 3

#define LEFT_TOP 0
#define TOP_RIGHT 1
#define RIGHT_BOTTOM 2
#define BOTTOM_LEFT 3

int To1DIndex(int x, int z, int width)
{
	return z * width + x;
}

float4 SampleTextureBilinear(Texture2D<float4> buffer, float width, float height, float2 uv)
{
	float4 value = 0;
	float2 pixelCoord = float2(lerp(0, width - 1, uv.x), lerp(0, height - 1, uv.y));
	//apply a bilinear filter
	int xFloor = floor(pixelCoord.x);
	int xCeil = ceil(pixelCoord.x);
	int yFloor = floor(pixelCoord.y);
	int yCeil = ceil(pixelCoord.y);

	float4 f00 = buffer[uint2(xFloor, yFloor)];
	float4 f01 = buffer[uint2(xFloor, yCeil)];

	float4 f10 = buffer[uint2(xCeil, yFloor)];
	float4 f11 = buffer[uint2(xCeil, yCeil)];

	float2 unitCoord = float2(pixelCoord.x - xFloor, pixelCoord.y - yFloor);

	value = f00 * (1 - unitCoord.x) * (1 - unitCoord.y) + f01 * (1 - unitCoord.x) * unitCoord.y + f10 * unitCoord.x * (1 - unitCoord.y) + f11 * unitCoord.x * unitCoord.y;

	return value;
}

float4 SampleTextureBilinear(RWTexture2D<float4> buffer, float width, float height, float2 pixelCoord)
{
	float4 value = 0;
	pixelCoord.x = clamp(pixelCoord.x, 0, width - 1);
	pixelCoord.y = clamp(pixelCoord.y, 0, height - 1);
	//apply a bilinear filter
	int xFloor = floor(pixelCoord.x);
	int xCeil = ceil(pixelCoord.x);
	int yFloor = floor(pixelCoord.y);
	int yCeil = ceil(pixelCoord.y);

	float4 f00 = buffer[uint2(xFloor, yFloor)];
	float4 f01 = buffer[uint2(xFloor, yCeil)];

	float4 f10 = buffer[uint2(xCeil, yFloor)];
	float4 f11 = buffer[uint2(xCeil, yCeil)];

	float2 unitCoord = float2(pixelCoord.x - xFloor, pixelCoord.y - yFloor);

	value = f00 * (1 - unitCoord.x) * (1 - unitCoord.y) + f01 * (1 - unitCoord.x) * unitCoord.y + f10 * unitCoord.x * (1 - unitCoord.y) + f11 * unitCoord.x * unitCoord.y;

	return value;
}

float DecodeFloatRG(float2 enc)
{
	float2 kDecodeDot = float2(1.0, 1 / 255.0);
	return dot(enc, kDecodeDot);
}

float2 EncodeFloatRG(float v)
{
    float2 kEncodeMul = float2(1.0, 255.0);
    float kEncodeBit = 1.0 / 255.0;
    float2 enc = kEncodeMul * v;
    enc = frac(enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}

float RandomValue (float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
}

float InverseLerpUnclamped(float a, float b, float value)
{
	//adding a==b check if needed
	return (value - a) / (b - a + 0.0000001);
}

#endif //GRIFFIN_GRASS_COMMON_INCLUDED