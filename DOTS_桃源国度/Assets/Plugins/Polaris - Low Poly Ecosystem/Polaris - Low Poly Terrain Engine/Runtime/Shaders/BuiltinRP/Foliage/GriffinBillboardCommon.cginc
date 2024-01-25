#ifndef GRIFFIN_BILLBOARD_COMMON_INCLUDED
#define GRIFFIN_BILLBOARD_COMMON_INCLUDED

#include "UnityCG.cginc"

struct Input
{
	float2 uv_MainTex; 
	float2 imageTexcoord;
	float face : VFACE;
};

fixed4 _ImageTexcoords[256];
int _ImageCount;

void GetImageTexcoord(appdata_full i, inout Input IN)
{
	fixed3 normal = normalize(UnityObjectToWorldNormal(i.normal));
	fixed dotZ = dot(normal, fixed3(0,0,1));
	fixed dotX = dot(normal, fixed3(1,0,0));
	fixed rad = atan2(dotZ, dotX);
	rad = (rad + UNITY_TWO_PI) % UNITY_TWO_PI;
	fixed f = rad/UNITY_TWO_PI - 0.5/_ImageCount;
	int imageIndex = f*_ImageCount;

	fixed4 rect = _ImageTexcoords[imageIndex];
	fixed2 min = rect.xy;
	fixed2 max = rect.xy + rect.zw;

	fixed2 result = fixed2(
		lerp(min.x, max.x, i.texcoord.x),
		lerp(min.y, max.y, i.texcoord.y));
	IN.imageTexcoord = result;
}

void TreeBillboardVert(inout appdata_full v, out Input o)
{      
	UNITY_INITIALIZE_OUTPUT(Input, o);

	//Calculate new billboard vertex position and normal;
	float3 upCamVec = float3(0, 1, 0);
	float3 forwardCamVec = -normalize(UNITY_MATRIX_V._m20_m21_m22);
	float3 rightCamVec = normalize(UNITY_MATRIX_V._m00_m01_m02);
	float4x4 rotationCamMatrix = float4x4(rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1);
	v.normal = mul(unity_WorldToObject, -forwardCamVec);
	v.tangent = mul(unity_WorldToObject, float4(rightCamVec, 0));
	v.vertex.x *= length(unity_ObjectToWorld._m00_m10_m20);
	v.vertex.y *= length(unity_ObjectToWorld._m01_m11_m21);
	v.vertex.z *= length(unity_ObjectToWorld._m02_m12_m22);
	v.vertex = mul(v.vertex, rotationCamMatrix);
	v.vertex.xyz += unity_ObjectToWorld._m03_m13_m23;
	//Need to nullify rotation inserted by generated surface shader;
	v.vertex = mul(unity_WorldToObject, v.vertex);

	GetImageTexcoord(v, o);
}

#endif //GRIFFIN_BILLBOARD_COMMON_INCLUDED