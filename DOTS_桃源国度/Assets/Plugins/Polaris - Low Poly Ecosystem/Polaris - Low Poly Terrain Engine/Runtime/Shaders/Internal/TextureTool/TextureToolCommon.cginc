#ifndef TEXTURE_TOOL_COMMON_INCLUDED
#define TEXTURE_TOOL_COMMON_INCLUDED

float2 GradientNoise_dir(float2 p)
{
	p = p % 289;
	float x = (34 * p.x + 1) * p.x % 289 + p.y;
	x = (34 * x + 1) * x % 289;
	x = frac(x / 41) * 2 - 1;
	return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float GradientNoise(float2 p)
{
	float2 ip = floor(p);
	float2 fp = frac(p);
	float d00 = dot(GradientNoise_dir(ip), fp);
	float d01 = dot(GradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
	float d10 = dot(GradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
	float d11 = dot(GradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
	fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
	return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}

float InverseLerpUnclamped(float a, float b, float value)
{
	//adding a==b check if needed
	return (value - a) / (b - a + 0.0000001);
}

float RandomValue(float seed)
{
	return frac(sin(dot(float2(seed, seed+1), float2(12.9898, 78.233)))*43758.5453);
}

float RandomValue(float u, float v)
{
	return frac(sin(dot(float2(u, v), float2(12.9898, 78.233)))*43758.5453);
}

float2 VoronoiRandomVector (float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

float Voronoi(float2 UV, float AngleOffset, float CellDensity)
{
    float2 g = floor(UV * CellDensity);
    float2 f = frac(UV * CellDensity);
    float t = 8.0;
    float3 res = float3(8.0, 0.0, 0.0);
	float noiseValue = 0;

    for(int y=-1; y<=1; y++)
    {
        for(int x=-1; x<=1; x++)
        {
            float2 lattice = float2(x,y);
            float2 offset = VoronoiRandomVector(lattice + g, AngleOffset);
            float d = distance(lattice + offset, f);
            if(d < res.x)
            {
                res = float3(d, offset.x, offset.y);
                noiseValue = res.x;
            }
        }
    }

	return noiseValue;
}

inline float ValueNoiseInterpolate (float a, float b, float t)
{
    return (1.0-t)*a + (t*b);
}

inline float ValueNoise (float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = RandomValue(c0.x, c0.y);
    float r1 = RandomValue(c1.x, c1.y);
    float r2 = RandomValue(c2.x, c2.y);
    float r3 = RandomValue(c3.x, c3.y);

    float bottomOfGrid = ValueNoiseInterpolate(r0, r1, f.x);
    float topOfGrid = ValueNoiseInterpolate(r2, r3, f.x);
    float t = ValueNoiseInterpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}


float4 MoveTowards(float4 current, float4 target, float maxDistanceDelta)
{
	float4 a = target - current;
	float magnitude = length(a);
	if (magnitude <= maxDistanceDelta || magnitude == 0)
	{
		return target;
	}
	return current + a / magnitude * maxDistanceDelta;
}

// Encoding/decoding [0..1) floats into 8 bit/channel RG. Note that 1.0 will not be encoded properly.
inline float2 GriffinEncodeFloatRG(float v)
{
    float2 kEncodeMul = float2(1.0, 255.0);
    float kEncodeBit = 1.0 / 255.0;
    float2 enc = kEncodeMul * v;
    enc = frac(enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}

inline float GriffinDecodeFloatRG(float2 enc)
{
    float2 kDecodeDot = float2(1.0, 1 / 255.0);
    return dot(enc, kDecodeDot);
}

void Sample4Splats(
    sampler2D _Control0,
    sampler2D _Splat0, sampler2D _Splat1, sampler2D _Splat2, sampler2D _Splat3,
    fixed4 _Splat0_ST, fixed4 _Splat1_ST, fixed4 _Splat2_ST, fixed4 _Splat3_ST,
    float2 uv, inout fixed4 control, inout fixed4 splat, inout fixed weight)
{
    control = tex2D(_Control0, uv);

    fixed2 uv_Splat0 = fixed2(uv.x * _Splat0_ST.x, uv.y * _Splat0_ST.y) + _Splat0_ST.zw;
    fixed2 uv_Splat1 = fixed2(uv.x * _Splat1_ST.x, uv.y * _Splat1_ST.y) + _Splat1_ST.zw;
    fixed2 uv_Splat2 = fixed2(uv.x * _Splat2_ST.x, uv.y * _Splat2_ST.y) + _Splat2_ST.zw;
    fixed2 uv_Splat3 = fixed2(uv.x * _Splat3_ST.x, uv.y * _Splat3_ST.y) + _Splat3_ST.zw;

    fixed4 splat0 = tex2D(_Splat0, uv_Splat0) * control.r;
    fixed4 splat1 = tex2D(_Splat1, uv_Splat1) * control.g;
    fixed4 splat2 = tex2D(_Splat2, uv_Splat2) * control.b;
    fixed4 splat3 = tex2D(_Splat3, uv_Splat3) * control.a;

    splat = splat0 + splat1 + splat2 + splat3;
    weight = control.r + control.g + control.b + control.a;
}
#endif //GRIFFIN_CG_INCLUDED