#ifndef RUKHANKA_BONE_RENDERER_HLSL_
#define RUKHANKA_BONE_RENDERER_HLSL_

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

/////////////////////////////////////////////////////////////////////////////////

struct VertexInput
{
	float3 pos: POSITION;
	uint vertexID: SV_VertexID;
	uint instanceID: SV_InstanceID;
};

struct VertexToPixel
{
	float4 pos: SV_Position;
	float4 color: COLOR0;
};

struct BoneData
{
	float3 pos0, pos1;
	float4 colorTri, colorLines;
};

StructuredBuffer<BoneData> boneDataBuf;
int isLines;
float4x4 unity_MatrixVP;

/////////////////////////////////////////////////////////////////////////////////

float3 GetStableTangent(float3 v)
{
	float3 aV = abs(v);
	float3 rv = float3(-v.y, v.z, 0);

	if (aV.x <= aV.y && aV.x <= aV.z)		rv = float3(0, -v.z, v.y);
	else if (aV.y <= aV.y && aV.y <= aV.z)	rv = float3(-v.z, 0, v.x);

	return rv;
}

/////////////////////////////////////////////////////////////////////////////////

float3 GetCameraRelativePositionWS(float3 positionWS)
{
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
	positionWS -= _WorldSpaceCameraPos.xyz;
#endif
	return positionWS;
}

/////////////////////////////////////////////////////////////////////////////////

VertexToPixel VS(VertexInput i)
{
	VertexToPixel o = (VertexToPixel)0;
	BoneData bd = boneDataBuf[i.instanceID];

	float3 worldPos = 0;

	switch (i.vertexID)
	{
		case 0: worldPos = bd.pos0; break;
		case 5: worldPos = bd.pos1; break;
		default:
		{
			float3 boneVec = bd.pos0 - bd.pos1;
			float l = length(boneVec);
			if (l != 0)
			{
				float3 boneVecNrm = boneVec / l;
				float3 tangent = GetStableTangent(boneVecNrm);
				float3 t = normalize(cross(boneVecNrm, tangent));
				float3 n = normalize(cross(boneVecNrm, t));

				float3 offsetVec = t * i.pos.x - n * i.pos.z;
				worldPos = bd.pos1 + boneVec * 0.3f + offsetVec * l;
			}
		}
		break;
	}

	worldPos = GetCameraRelativePositionWS(worldPos);
	o.pos = mul(unity_MatrixVP, float4(worldPos, 1));
	o.color = isLines ? bd.colorLines : bd.colorTri;
	return o;
}

/////////////////////////////////////////////////////////////////////////////////

float4 PS(VertexToPixel i): SV_Target0
{
	return i.color;
}

#endif
