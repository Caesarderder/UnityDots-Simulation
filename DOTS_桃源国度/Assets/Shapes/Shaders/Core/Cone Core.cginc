// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(float4, _Color)
PROP_DEF(float, _Radius)
PROP_DEF(float, _Length)
PROP_DEF(int, _SizeSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	float pxCoverage : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
    float radiusTarget = PROP(_Radius);
    float lengthTarget = PROP(_Length);
    int sizeSpace = PROP(_SizeSpace);
    
	LineWidthData widthDataLength = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, lengthTarget, sizeSpace );
	float length = widthDataLength.thicknessMeters;
	
	LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, radiusTarget*2, sizeSpace );
	float radius = widthDataRadius.thicknessMeters / 2;
	
	o.pxCoverage = min( widthDataLength.thicknessPixelsTarget, widthDataRadius.thicknessPixelsTarget );
	float scaleBranched = (v.vertex.z > 0.5 ? length : radius);
	float3 localPos = v.vertex.xyz * scaleBranched;
	o.pos = LocalToClipPos( localPos );
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	float4 color = PROP(_Color);
	return SHAPES_OUTPUT( color, saturate(i.pxCoverage), i ); 
}