#ifndef GRIFFIN_SPLAT_COMMON_INCLUDED
#define GRIFFIN_SPLAT_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

struct Input 
{
	float2 uv_Control0;
};

void Sample4Splats(
	sampler2D _Control0,
	sampler2D _Splat0, sampler2D _Splat1, sampler2D _Splat2, sampler2D _Splat3,
	fixed4 _Splat0_ST, fixed4 _Splat1_ST, fixed4 _Splat2_ST, fixed4 _Splat3_ST, 
	float2 uv, inout fixed4 control, inout fixed4 splat, inout fixed weight)
{
	control = tex2D(_Control0, uv);
	
	fixed2 uv_Splat0 = fixed2(uv.x*_Splat0_ST.x, uv.y*_Splat0_ST.y)+_Splat0_ST.zw;
	fixed2 uv_Splat1 = fixed2(uv.x*_Splat1_ST.x, uv.y*_Splat1_ST.y)+_Splat1_ST.zw;
	fixed2 uv_Splat2 = fixed2(uv.x*_Splat2_ST.x, uv.y*_Splat2_ST.y)+_Splat2_ST.zw;
	fixed2 uv_Splat3 = fixed2(uv.x*_Splat3_ST.x, uv.y*_Splat3_ST.y)+_Splat3_ST.zw;

	fixed4 splat0 = tex2D(_Splat0, uv_Splat0)*control.r;
	fixed4 splat1 = tex2D(_Splat1, uv_Splat1)*control.g;
	fixed4 splat2 = tex2D(_Splat2, uv_Splat2)*control.b;
	fixed4 splat3 = tex2D(_Splat3, uv_Splat3)*control.a;

	splat = splat0 + splat1 + splat2 + splat3;
	weight = control.r + control.g + control.b + control.a;
}

void Sample4Splats4Normals(
	sampler2D _Control0,
	sampler2D _Splat0, sampler2D _Splat1, sampler2D _Splat2, sampler2D _Splat3,
	fixed4 _Splat0_ST, fixed4 _Splat1_ST, fixed4 _Splat2_ST, fixed4 _Splat3_ST,
	sampler2D _Normal0, sampler2D _Normal1, sampler2D _Normal2, sampler2D _Normal3,
	float2 uv, inout fixed4 control, inout fixed4 splat, inout fixed3 normal, inout fixed weight)
{
	control = tex2D(_Control0, uv);
	
	fixed2 uv_Splat0 = fixed2(uv.x*_Splat0_ST.x, uv.y*_Splat0_ST.y)+_Splat0_ST.zw;
	fixed2 uv_Splat1 = fixed2(uv.x*_Splat1_ST.x, uv.y*_Splat1_ST.y)+_Splat1_ST.zw;
	fixed2 uv_Splat2 = fixed2(uv.x*_Splat2_ST.x, uv.y*_Splat2_ST.y)+_Splat2_ST.zw;
	fixed2 uv_Splat3 = fixed2(uv.x*_Splat3_ST.x, uv.y*_Splat3_ST.y)+_Splat3_ST.zw;

	fixed4 splat0 = tex2D(_Splat0, uv_Splat0)*control.r;
	fixed4 splat1 = tex2D(_Splat1, uv_Splat1)*control.g;
	fixed4 splat2 = tex2D(_Splat2, uv_Splat2)*control.b;
	fixed4 splat3 = tex2D(_Splat3, uv_Splat3)*control.a;

	splat = splat0 + splat1 + splat2 + splat3;
	weight = control.r + control.g + control.b + control.a;

	fixed4 normal0 = tex2D(_Normal0, uv_Splat0)*control.r;
	fixed4 normal1 = tex2D(_Normal1, uv_Splat1)*control.g;
	fixed4 normal2 = tex2D(_Normal2, uv_Splat2)*control.b;
	fixed4 normal3 = tex2D(_Normal3, uv_Splat3)*control.a;
	fixed3 nrm = UnpackNormal(normal0 + normal1 + normal2 + normal3);
	normal = lerp(normal, nrm, weight);
}

#endif //GRIFFIN_SPLAT_COMMON_INCLUDED