// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(half4, _Color)
PROP_DEF(half, _Thickness)
PROP_DEF(int, _ThicknessSpace)
PROP_DEF(int, _Alignment)
UNITY_INSTANCING_BUFFER_END(Props)

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

#define IP_nrmCoordLat intp0.x
#define IP_nrmCoordLong intp0.y
#define IP_radius intp0.z
#define IP_pxCoverage intp0.w
 
struct VertexInput {
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION; // current
    float4 uv0 : TEXCOORD0; // uvs (XY) endpoint (Z) thickness (W)
    float3 uv1 : TEXCOORD1; // prev
    float3 uv2 : TEXCOORD2; // next
    float4 color : COLOR;
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    half4 color : TEXCOORD0;
    half4 intp0 : TEXCOORD1;
    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        float3 worldPos : TEXCOORD2;
        float3 origin : TEXCOORD3;
    #endif
    UNITY_FOG_COORDS(4)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void GetSimpleOffset( out float3 dir, out float len, float3 aNormal, float3 bNormal, float radius ){
    float dotVal = dot(aNormal, bNormal);
    if (dotVal < -0.99) {
        dir = float3(0, 0, 0);
        len = 0;
    } else {
        dir = normalize( aNormal + bNormal );
        len = radius;
    }
}

VertexOutput vert (VertexInput v) {
    VertexOutput o = (VertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    // alignment
    int alignment = PROP(_Alignment);

    // points
    float3 ptWorld = 0;
    float3 pt = 0;
    float3 ptPrev = 0;
    float3 ptNext = 0;
    half3 dirToCam = 0;
    switch( alignment ){
        case ALIGNMENT_FLAT: {
            // local space
            pt = float3( v.vertex.xy, 0 );
            ptWorld = LocalToWorldPos( pt );
            ptPrev = float3( v.uv1.xy, 0 );
            ptNext = float3( v.uv2.xy, 0 );
            break;
        }
        case ALIGNMENT_BILLBOARD: {
            pt = LocalToWorldPos( v.vertex.xyz );
            ptWorld = pt;
            ptPrev = LocalToWorldPos( v.uv1 );
            ptNext = LocalToWorldPos( v.uv2 );
            dirToCam = DirectionToNearPlanePos( pt );
            break;
        }
    }
    

    // tangents & normals
    half3 tangentPrev = normalize( pt - ptPrev );
    half3 tangentNext = normalize( ptNext - pt );
    half3 normalPrev = 0;
    half3 normalNext = 0;
    switch( alignment ){
        case ALIGNMENT_FLAT: {
            // local space
            normalPrev = half3( tangentPrev.y, -tangentPrev.x, 0 );
            normalNext = half3( tangentNext.y, -tangentNext.x, 0 );
            break;
        }
        case ALIGNMENT_BILLBOARD: {
            // world space
            normalPrev = normalize(cross( dirToCam, tangentPrev ));
            normalNext = normalize(cross( dirToCam, tangentNext ));
            break;
        }
    }
    
    int scaleMode = PROP(_ScaleMode);
    half uniformScale = GetUniformScale();
	half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? 1 : 1.0/uniformScale;
    
    // thickness stuff
    half thickness = PROP(_Thickness) * v.uv0.w * scaleThickness; // global thickness * per-point thickness * transform scaling
    
    // radius calc
    int thicknessSpace = PROP(_ThicknessSpace);
	LineWidthData widthData = GetScreenSpaceWidthData( ptWorld, CAM_RIGHT, thickness, thicknessSpace );
	o.IP_pxCoverage = widthData.thicknessPixelsTarget / scaleThickness; // hack: this is a lil weird honestly - I think we're mixing local vs world somehere before this
	half vertexRadius = widthData.thicknessMeters * 0.5;
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
        half paddingWorldSpace = (0.5 * AA_PADDING_PX / widthData.pxPerMeter);
        bool isEndpoint = abs(v.uv0.z) > 0;
        half endpointExtrude = isEndpoint ? paddingWorldSpace : 0;
    #else
        half endpointExtrude = 0;
    #endif
    o.IP_radius = vertexRadius / widthData.aaPaddingScale; // actual radius of the visuals

    // miter point calc
    half turnDirection = sign(dot( tangentPrev, normalNext ));
    #if defined(IS_JOIN_MESH) // only draw joins
        half tSide = (-turnDirection*v.uv0.y + 1)/2;
        half tExtVsMiter = (v.uv0.x + 1)/2;
        half3 normalPrevFlip = turnDirection*normalPrev;
        half3 normalNextFlip = turnDirection*normalNext;
        half3 sideExtrudeDir = lerp( normalPrevFlip, normalNextFlip, tSide );
        half3 centerNormalDir = normalize( normalPrevFlip + normalNextFlip );
        half3 miterDir;
        half miterLength;
        GetMiterOffset( /*out*/ miterDir, /*out*/ miterLength, sideExtrudeDir, centerNormalDir, vertexRadius );
        half3 miterOffset = miterDir * miterLength;
        half3 vertOffset = lerp( miterOffset, sideExtrudeDir * vertexRadius, tExtVsMiter ) * abs(v.uv0.x);
        float3 vertPos = pt + vertOffset;
    #else // only draw line segments
    
        half3 miterDir;
        half miterLength;
        
        // all these boys use proper miters
        #if defined(JOIN_MITER) || defined(JOIN_ROUND) || defined(JOIN_BEVEL) 
            GetMiterOffset( /*out*/ miterDir, /*out*/ miterLength, normalPrev, normalNext, vertexRadius );
            // make sure miter length doesn't overshoot line length
            // or not you know because it turns out~ this is more complicated than this
            // (because it changes thickness) so, uh, nevermind for now
            // miterLength = lerp(miterLength, min(miterLength, min(length(pt - ptPrev),length(ptNext - pt)) ),(-v.uv0.x*turnDirection + 1)/2 );
            //float3 midpt = (ptNext + ptPrev)/2;
            //miterLength = min(miterLength, min( distance( pt, ptNext ), distance( pt, ptPrev ) ) );
        #else
            // simple joins
            GetSimpleOffset( /*out*/ miterDir, /*out*/ miterLength, normalPrev, normalNext, vertexRadius );
        #endif
        
        
        half3 scaledMiterNormal = miterDir * miterLength;
        #if defined(JOIN_ROUND) || defined(JOIN_BEVEL)
            half3 extrude = lerp(normalPrev, normalNext, (v.uv0.y + 1)/2 ) * vertexRadius;
            extrude = lerp( extrude, scaledMiterNormal, (-v.uv0.x*turnDirection + 1)/2 );
            float3 vertPos = pt + extrude * v.uv0.x;
        #else
            float3 vertPos = pt + scaledMiterNormal * v.uv0.x; // float3(v.tangent.xy * v.uv0.x * vertexRadius,0);
        #endif
        #if LOCAL_ANTI_ALIASING_QUALITY > 0
            vertPos += tangentPrev * endpointExtrude * v.uv0.z; // is 0 if not an endpoint
            half distToPrev = distance(pt, ptPrev);
            v.uv0.z *= (distToPrev + endpointExtrude)/(distToPrev); // ratio for extrude dist compensation 
        #endif
    #endif

    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        o.worldPos = vertPos;
        o.origin = pt;
    #endif

    o.color = v.color * PROP(_Color);

    // todo: this causes *bad things* with anti-aliasing
    // it's complicated - this doesn't work due to how barycentric interpolation skews coordinates.
    // just scaling the UVs overscales those skew regions leading to dents in the AA region
    o.IP_nrmCoordLat = v.uv0.x * widthData.aaPaddingScale; // scale compensate for fading
    o.IP_nrmCoordLong = v.uv0.z;

    //float depth = unity_ObjectToWorld[2][3];
    switch( alignment ){
        case ALIGNMENT_FLAT: {
            o.pos = LocalToClipPos( float4( vertPos, 1 ) );
            break;
        }
        case ALIGNMENT_BILLBOARD: {
            o.pos = WorldToClipPos( float4( vertPos, 1 ) );
            break;
        }
    }
    UNITY_TRANSFER_FOG(o,o.pos);
    
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    
    // debug padding 
    // float2 paddingDebug = abs(float2( i.uv0.x, i.uv0.z )) >= 1 ? 1 : 0;
    //return ShapesOutput( float4(uv,0,1), shape_mask );
    half shape_mask = 1;
    
    // Round joins
    #if defined(IS_JOIN_MESH) && defined(JOIN_ROUND)
        shape_mask = SdfToMask( distance( i.worldPos, i.origin ) / i.IP_radius, 1 );
    #endif
        
	// used for line segments and bevel joins
	#if LOCAL_ANTI_ALIASING_QUALITY > 0 && ( defined(IS_JOIN_MESH) == false || (defined(IS_JOIN_MESH) && defined(JOIN_BEVEL)) )
        half maskEdges = GetLineLocalAA( i.IP_nrmCoordLat, i.IP_pxCoverage );
        half maskEdgesCap = GetLineLocalAA( i.IP_nrmCoordLong, i.IP_pxCoverage );
        shape_mask = min( shape_mask, min( maskEdges, maskEdgesCap ) );
    #endif

    shape_mask *= saturate( i.IP_pxCoverage );
    
    return SHAPES_OUTPUT( i.color, shape_mask, i );
    
}