// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(float4, _Color)
PROP_DEF(float3, _Size)
PROP_DEF(int, _SizeSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half pxCoverage : TEXCOORD0;
	UNITY_FOG_COORDS(1)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
    //float radiusTarget = 1;//PROP(_Size);
    float3 size = PROP(_Size);
    int radiusSpace = PROP(_SizeSpace);
    
	LineWidthData widthData = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, 1, radiusSpace );
    half radius = widthData.thicknessMeters * 0.5;
    o.pxCoverage = widthData.thicknessPixelsTarget;
    
	half3 localPos = v.vertex.xyz * size.xyz * radius;
	o.pos = LocalToClipPos( localPos );
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half4 color = PROP(_Color);
	return SHAPES_OUTPUT( color, saturate( i.pxCoverage ), i ); 
}