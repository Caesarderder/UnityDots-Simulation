// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF( half4, _Color)
PROP_DEF( half4, _ColorB)
PROP_DEF( half4, _ColorC)
PROP_DEF( half4, _ColorD)
PROP_DEF( float3, _A)
PROP_DEF( float3, _B)
PROP_DEF( float3, _C)
PROP_DEF( float3, _D)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    #if QUAD_INTERPOLATION_QUALITY == 0
        half4 color : TEXCOORD0;
    #elif QUAD_INTERPOLATION_QUALITY == 1
        half2 uv : TEXCOORD2;
    #elif QUAD_INTERPOLATION_QUALITY == 2
        half2 localPos : TEXCOORD1;
    #elif QUAD_INTERPOLATION_QUALITY == 3
        half2 localPos : TEXCOORD1;
        half4 posAB : TEXCOORD2;
        half4 posCD : TEXCOORD3;
    #endif
    UNITY_FOG_COORDS(4)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

half2 PlanarProject( half3 pos3D, half3 dirX, half3 dirY ){
    return half2( dot( pos3D, dirX ), dot( pos3D, dirY ) );
}

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    float3 A = PROP(_A);
    float3 B = PROP(_B);
    float3 C = PROP(_C);
    float3 D = PROP(_D);
    v.vertex.xyz = A * v.color.r + B * v.color.g + C * v.color.b + D * v.color.a;
    
    #if QUAD_INTERPOLATION_QUALITY == 0
        // per-vertex version, which doesn't do a real bilinear version
        half4 colorA = PROP(_Color);
        half4 colorB = PROP(_ColorB);
        half4 colorC = PROP(_ColorC);
        half4 colorD = PROP(_ColorD);
        o.color = colorA * v.color.r + colorB * v.color.g + colorC * v.color.b + colorD * v.color.a;
    #elif QUAD_INTERPOLATION_QUALITY == 1
        o.uv = v.uv * 0.5 + 0.5;
    #elif QUAD_INTERPOLATION_QUALITY == 2
        o.localPos = v.vertex.xy; // assume 2D coordinates only
    #elif QUAD_INTERPOLATION_QUALITY == 3
        // construct best fit plane from average face normals
        half3 ab = B - A;
        half3 ac = C - A;
        half3 ad = D - A;
        half3 normalA = normalize(cross( ab, ac ));
        half3 normalB = normalize(cross( ab, ac ));
        half3 normal = normalize(normalA + normalB);
        half3 tangent = normalize(ab);
        half3 bitangent = cross( normal, tangent );
        
        // project all coordinates
        o.posAB.xy = PlanarProject( A, tangent, bitangent );
        o.posAB.zw = PlanarProject( B, tangent, bitangent );
        o.posCD.xy = PlanarProject( C, tangent, bitangent );
        o.posCD.zw = PlanarProject( D, tangent, bitangent );
        o.localPos = PlanarProject( v.vertex, tangent, bitangent );
    #endif
    
    o.pos = UnityObjectToClipPos( v.vertex );
    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    #if QUAD_INTERPOLATION_QUALITY == 0
        return ShapesOutput( i.color, 1 );
    #else
        #if QUAD_INTERPOLATION_QUALITY == 1
            half2 uv = i.uv;
        #elif QUAD_INTERPOLATION_QUALITY == 2
            float3 A = PROP(_A);
            float3 B = PROP(_B);
            float3 C = PROP(_C);
            float3 D = PROP(_D);
            half2 uv = InvBilinear( i.localPos.xy, A.xy, B.xy, C.xy, D.xy );
        #elif QUAD_INTERPOLATION_QUALITY == 3
            half2 uv = InvBilinear( i.localPos.xy, i.posAB.xy, i.posAB.zw, i.posCD.xy, i.posCD.zw );
        #endif
        half4 colorA = PROP(_Color);
        half4 colorB = PROP(_ColorB);
        half4 colorC = PROP(_ColorC);
        half4 colorD = PROP(_ColorD);
        half4 left = lerp(colorA, colorB, uv.y );
        half4 right = lerp(colorD, colorC, uv.y );
        half4 color = lerp( left, right, uv.x );
        return SHAPES_OUTPUT( color, 1, i );
    #endif
}



