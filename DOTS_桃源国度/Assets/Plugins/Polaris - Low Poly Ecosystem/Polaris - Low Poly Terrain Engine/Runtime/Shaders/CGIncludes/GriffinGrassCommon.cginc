#ifndef GRIFFIN_GRASS_COMMON_INCLUDED
#define GRIFFIN_GRASS_COMMON_INCLUDED

#include "UnityCG.cginc"

float _WaveDistance;
float4 _Wind;
fixed _BendFactor;
sampler2D _VectorField;
float4x4 _WorldToNormalized;

struct Input
{
	float2 uv_MainTex; 
    float2 texcoord;
	float4 vertexColor: COLOR;
};

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

void GrassVert(inout appdata_full i, out Input o)
{   
	i.normal = fixed3(0,1,0);
	UNITY_INITIALIZE_OUTPUT(Input, o);
	o.texcoord = i.texcoord;

	fixed4 worldPos = mul(unity_ObjectToWorld, i.vertex);

	fixed4 interactiveGrassOffset = 0;
	#if INTERACTIVE_GRASS
	fixed4 normPos = mul(_WorldToNormalized, worldPos);
	fixed4 bendVector = tex2Dlod(_VectorField, fixed4(normPos.x, normPos.z, 0, 0));
	bendVector = fixed4(bendVector.x*2-1, bendVector.y*2-1, bendVector.z*2-1, 0);
	interactiveGrassOffset = bendVector*i.color.a*_BendFactor;
	#endif

	fixed2 windDir = normalize(_Wind.xy);
	fixed2 sampleOrigin = fixed2(worldPos.x, worldPos.z) - windDir*_Wind.z*_Time.y;
	fixed2 samplePos = sampleOrigin/_Wind.w;
	fixed noiseValue = GradientNoise(samplePos);
	fixed multiplier = noiseValue*i.color.a;
	fixed4 windWorldOffset = multiplier*_WaveDistance*_BendFactor*float4(windDir.x, multiplier*0.5, windDir.y, 0);

	fixed4 offset = mul(unity_WorldToObject, windWorldOffset) + interactiveGrassOffset;
	i.vertex += offset;
	i.vertex.y = max(0, i.vertex.y);
}

#endif //GRIFFIN_GRASS_COMMON_INCLUDED