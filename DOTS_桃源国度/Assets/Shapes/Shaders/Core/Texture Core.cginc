// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

// no instancing for textures anyway
sampler2D _MainTex;
half4 _Color;
half4 _Rect; // xy = pos, zw = size
half4 _Uvs;  // xy = pos, zw = size

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
    float4 pos : SV_POSITION;
    half2 uv : TEXCOORD1;
    UNITY_FOG_COORDS(2)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

inline half2 UvToRect(half2 uv01, half4 rect) {
    return rect.xy + uv01*rect.zw;
}

VertexOutput vert(VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    half2 uv01 = v.vertex.xy*0.5+0.5;
    o.uv = UvToRect( uv01, _Uvs );
    v.vertex.xy = UvToRect( uv01, _Rect );
    o.pos = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}

FRAG_OUTPUT_V4 frag(VertexOutput i) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    half4 shape_color = tex2D(_MainTex, i.uv) * _Color;
    return SHAPES_OUTPUT(shape_color, 1, i);
}
