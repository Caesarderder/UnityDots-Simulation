// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(half4, _Color)
SHAPES_FILL_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#include "../FillUtils.cginc"

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
	SHAPES_INTERPOLATOR_FILL(0)
    UNITY_FOG_COORDS(1)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = UnityObjectToClipPos( v.vertex );
    UNITY_TRANSFER_FOG(o,o.pos);
    o.fillCoords = GetFillCoords( v.vertex );
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    half4 fillColor = GetFillColor(i.fillCoords);
    return SHAPES_OUTPUT( fillColor, 1, i );
}