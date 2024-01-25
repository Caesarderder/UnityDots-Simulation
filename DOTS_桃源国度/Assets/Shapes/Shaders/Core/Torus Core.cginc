// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(half4, _Color)
PROP_DEF(half, _Radius)
PROP_DEF(half, _RadiusSpace)
PROP_DEF(half, _Thickness)
PROP_DEF(half, _ThicknessSpace)
PROP_DEF(half, _AngleStart)
PROP_DEF(half, _AngleEnd)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half3 uvAndPxCoverage : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	int scaleMode = PROP(_ScaleMode);
    half uniformScale = GetUniformScale();
    half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? uniformScale : 1;
    
    half radiusMajorTarget = PROP(_Radius) * uniformScale;
    int radiusMajorSpace = PROP(_RadiusSpace);
    int thicknessSpace = PROP(_ThicknessSpace);
    half thicknessTarget = PROP(_Thickness) * scaleThickness;
    
    // calc radius
	LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, radiusMajorTarget * 2 /*to thickness*/, radiusMajorSpace );
	float radiusMajor = widthDataRadius.thicknessMeters / 2;
	
	// calc thickness
	LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, thicknessTarget, thicknessSpace );
	o.uvAndPxCoverage.z = widthDataThickness.thicknessPixelsTarget;
	float thicknessRadius = widthDataThickness.thicknessMeters * 0.5;

	// local space pos
    half3 dirFromCenter = normalize( half3( v.vertex.xy, 0 ) );
    half3 tubeCenter = dirFromCenter * radiusMajor;
	half3 localPos = tubeCenter + v.normal * thicknessRadius;
	o.uvAndPxCoverage.xy = localPos.xy;
	o.pos = LocalToClipPos( localPos / uniformScale );
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half angStartRaw = PROP(_AngleStart);
	half angEndRaw = PROP(_AngleEnd);
	half angStart = min(angStartRaw,angEndRaw);
	half angEnd = max(angStartRaw,angEndRaw);
	half angDeltaRaw = angEnd - angStart;
	half angDelta = clamp( angDeltaRaw, -TAU, TAU );
	if( abs(angDeltaRaw) < TAU-VERY_SMOL ) {
		half2 uv = Rotate( i.uvAndPxCoverage.xy, -angStart+ TAU/2 );
		half ang = DirToAng(uv)+TAU/2;
		clip((ang<angDelta)-0.5);
	}
	
	half4 color = PROP(_Color);
	return SHAPES_OUTPUT( color, saturate(i.uvAndPxCoverage.z), i );
}