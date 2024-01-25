// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF( half4, _Color )
PROP_DEF( half4, _ColorB )
PROP_DEF( half4, _ColorC )
PROP_DEF( float3, _A )
PROP_DEF( float3, _B )
PROP_DEF( float3, _C )
PROP_DEF( half, _Roundness )
PROP_DEF( half, _Hollow )
PROP_DEF( half, _Thickness )
PROP_DEF( half, _ThicknessSpace )
PROP_DEF( half, _ScaleMode )
SHAPES_DASH_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#include "../DashUtils.cginc"

#define IP_A intp0.xy
#define IP_B intp0.zw
#define IP_C intp1.xy
#define IP_Pos intp1.zw
#define IP_AB intp2.x
#define IP_BC intp2.y
#define IP_CA intp2.z
#define IP_HALF_THICKNESS intp2.w
#define IP_DISTANCES intp2.xyz
#define IP_pxPerMeter intp3.x
#define IP_inradius intp3.y

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    half4 pos : SV_POSITION;
    half4 color : TEXCOORD0;
    half4 intp0 : TEXCOORD1;
    half4 intp1 : TEXCOORD2;
    half4 intp2 : TEXCOORD3;
    half2 intp3 : TEXCOORD4;
    UNITY_FOG_COORDS(5)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// constructs a matrix where A to B is the X axis direction
inline half3x3 GetTriangleToProjectionSpaceMatrix( half abDistance, half3 a, half3 b, half3 c ) {
    half3 xAxis = (b-a)/abDistance; // we don't calculate abDist in here as it's already needed outside of this function
    half3 zAxis = normalize(cross(b-a, c-a));
    half3 yAxis = cross(xAxis, zAxis); // AB normal
    return half3x3( xAxis, yAxis, zAxis );
}

inline half4 GetColor( VertexInput v, half3 weights ) {
    half4 colorA = PROP(_Color);
    half4 colorB = PROP(_ColorB);
    half4 colorC = PROP(_ColorC);
    return WeightedSum( weights, colorA, colorB, colorC );
}

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    half3 w = v.vertex.xyz; // vertex weights
    o.color = GetColor( v, w ); // colors

    // local space vertex positions
    int scaleMode = PROP(_ScaleMode);
    bool useUniformScale = PROP(_ScaleMode) == SCALE_MODE_UNIFORM;
    half3 objScale = GetObjectScale();
    half uniformScale = GetUniformScale(objScale);
    half scaleThickness = useUniformScale ? uniformScale : 1;
  
    half3 coordinateScaling = useUniformScale ? half3(1,1,1) : objScale; 
    half3 a = PROP(_A) * coordinateScaling;
    half3 b = PROP(_B) * coordinateScaling;
    half3 c = PROP(_C) * coordinateScaling;

    // construct a 2D space and project all vertices
    o.IP_AB = length(b-a); // distance from a to b. this distance is the same as in projection space
    half3x3 mtxLocalToProj = GetTriangleToProjectionSpaceMatrix( o.IP_AB, a, b, c );

    // triangle corners in projection space
    half2 aProj = mul((half2x3)mtxLocalToProj,a);
    half2 bProj = mul((half2x3)mtxLocalToProj,b);
    half2 cProj = mul((half2x3)mtxLocalToProj,c);

    // calculate incircle, then make projection coordinates relative to the incenter
    o.IP_BC = distance(bProj,cProj);
    o.IP_CA = distance(cProj,aProj);
    Circle incircle = GetIncirclePosRadius( aProj, bProj, cProj, o.IP_AB, o.IP_BC, o.IP_CA );
    aProj -= incircle.pos; 
    bProj -= incircle.pos;
    cProj -= incircle.pos;
    
    half roundness = PROP(_Roundness);
    half thickness = PROP(_Thickness) * scaleThickness;
    int thicknessSpace = PROP(_ThicknessSpace);

    half3 center = LocalToWorldPos( ((a+b+c)/3)/coordinateScaling );
    LineWidthData widthData = GetScreenSpaceWidthDataSimple(center, CAM_UP, thickness, thicknessSpace );
    o.IP_pxPerMeter = widthData.pxPerMeter;

    // thinness fade
    if(PROP(_Hollow) > 0)
        o.color.a *= saturate(widthData.thicknessPixelsTarget);
    o.IP_inradius = incircle.r;
    o.IP_HALF_THICKNESS = 0.5*widthData.thicknessMeters/(incircle.r*scaleThickness);

    half padding = 0;//hollow ? thickness/2 : 0;

    #if LOCAL_ANTI_ALIASING_QUALITY > 0
        half padScale = useUniformScale ? uniformScale : 1;
        padding += AA_PADDING_PX/(widthData.pxPerMeter*padScale); // extra padding for LAA
    #endif

    // outer vertices in projection space. with no padding, they equal aProj etc
    half2 aProjOuter = aProj;
    half2 bProjOuter = bProj;
    half2 cProjOuter = cProj;

    // handle extra padding for anti-aliasing
    if( padding > 0 ) {
        // edge normal directions in projection space 
        half2 nAB = half2(0,1); // projection space is already aligned with a to b
        half2 nBC = Rotate90Left((cProj-bProj)/o.IP_BC); // divide normalizes here
        half2 nCA = Rotate90Left((aProj-cProj)/o.IP_CA);
        
        // 2D miter offset directions
        half2 miterVecA = GetMiterOffsetDirFast( nCA, nAB, padding );
        half2 miterVecB = GetMiterOffsetDirFast( nAB, nBC, padding );
        half2 miterVecC = GetMiterOffsetDirFast( nBC, nCA, padding );

        // add local space padding
        half3x3 mtxProjToLocal = transpose(mtxLocalToProj);
        a += mul(mtxProjToLocal, half3(miterVecA,0));
        b += mul(mtxProjToLocal, half3(miterVecB,0));
        c += mul(mtxProjToLocal, half3(miterVecC,0));

        // calculate the post-padding projection coords
        aProjOuter = aProj + miterVecA;
        bProjOuter = bProj + miterVecB;
        cProjOuter = cProj + miterVecC;
    }

    o.IP_Pos = WeightedSum( w, aProjOuter, bProjOuter, cProjOuter ) / incircle.r; // scale by inverse inradius

    // scale by inverse inradius, then move inner points based on roundness in the frag shader (since we need tangents out there)
    o.IP_A = (aProj/incircle.r);
    o.IP_B = (bProj/incircle.r);
    o.IP_C = (cProj/incircle.r);
    o.IP_DISTANCES = (o.IP_DISTANCES/incircle.r); // rescale distances too
    
    // set local space positions
    v.vertex.xyz = WeightedSum( w, a, b, c ); 
    o.pos = UnityObjectToClipPos( v.vertex / coordinateScaling );
    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}

inline half SdfToMaskBordered( half sdf, bool hollow, half halfThickness, out half tRadial ) {
    if( hollow ) {
        tRadial = sdf/(2*halfThickness)+1;
        return StepAAExplicitPD( -abs(tRadial*2-1)+1, tRadial*2 );
    }
    tRadial = 0;
    return StepAA(-sdf);
}

#define BASICALLY_A_CIRCLE_ROUNDNESS 0.998

inline half GetRadialMask( VertexOutput i, out half tRadial ) {

    half roundness = PROP(_Roundness);
    bool hollow = PROP(_Hollow) > 0;

    // shift by roundness
    i.IP_A *= (1-roundness);
    i.IP_B *= (1-roundness);
    i.IP_C *= (1-roundness);
    i.IP_DISTANCES *= (1-roundness);

    // no rounding, use three planes instead
    if( roundness < 0.002 ) {
        #if LOCAL_ANTI_ALIASING_QUALITY > 0
            half sdfAB = -SdfLine( i.IP_Pos, i.IP_A, i.IP_B ) / i.IP_AB;      
            half sdfBC = -SdfLine( i.IP_Pos, i.IP_B, i.IP_C ) / i.IP_BC;
            half sdfCA = -SdfLine( i.IP_Pos, i.IP_C, i.IP_A ) / i.IP_CA;
            half sdf = max(sdfAB, max(sdfBC, sdfCA));
            return SdfToMaskBordered(sdf, hollow, i.IP_HALF_THICKNESS, /*out*/ tRadial );
        #else
            return 1; // no AA and no rounding, so, no sdfs needed
        #endif
    }

    // 100% rounded, just use an incircle disc
    if( roundness > BASICALLY_A_CIRCLE_ROUNDNESS ) 
        return SdfToMaskBordered( length( i.IP_Pos )-1, hollow, i.IP_HALF_THICKNESS, /*out*/ tRadial );

    // rounded triangle, use full triangle SDF
    return SdfToMaskBordered( SdfTriangle( i.IP_Pos, i.IP_A, i.IP_B, i.IP_C ) - roundness, hollow, i.IP_HALF_THICKNESS, /*out*/ tRadial );
}

inline float AngBetween( float2 a, float2 b ) {
    return acos(clamp(dot(a,b),-1,1));
}

inline void GetPerimeterDistance( VertexOutput i, out float dist, out float distTotal ) {
    float2 p = i.IP_Pos;

    //float3 signs = sign(float3( Determinant( p, a ), Determinant( p, b ), Determinant( p, c ) ))*0.5+0.5;

    float roundness = PROP(_Roundness);
    float roundnessRadius = roundness; // linearly proportional to inradius, which is 1 here, and roundness

    uint3 signs = sign(half3( Determinant( p, i.IP_A ), Determinant( p, i.IP_B ), Determinant( p, i.IP_C ) ))*0.5+0.5;
    signs *= (1-signs).yzx;
    uint sector = signs.x*0+signs.y*1+signs.z*2; // convert from (1,0,0),(0,1,0),(0,0,1) to 0,1,2
    uint sectorNext = (sector+1)%3;
    
    float3 dists = float3(i.IP_AB,i.IP_BC,i.IP_CA);
    float2 v[3] = {i.IP_A,i.IP_B,i.IP_C};
    float2 tangents[3] = {
        (v[1]-v[0])/i.IP_AB,
        (v[2]-v[1])/i.IP_BC,
        (v[0]-v[2])/i.IP_CA
    };
    float3 angles = // also angles! same same
        acos(clamp( float3(
            dot(tangents[0], -tangents[2] ),
            dot(tangents[1], -tangents[0] ),
            dot(tangents[2], -tangents[1] )
        ),-1,1));
    float3 arcAngles = TAU/2-angles;
    float3 arcLengths = arcAngles * roundnessRadius;

    // scale by reverse roundness to offset verts for the roundness indent
    float invRoundness = 1-roundness;
    dists *= invRoundness;
    v[0] *= invRoundness;
    v[1] *= invRoundness;
    v[2] *= invRoundness;

    
    float2 vThis = v[sector];
    float2 vNext = v[sectorNext];

    // todo: distinguish between linear and actual curved dist when roundness is involved
    // this is linear dist along the edge, from the inner points! not actual triangle edges
    bool hasLinearDist = roundness <= BASICALLY_A_CIRCLE_ROUNDNESS;
    float linearDist = hasLinearDist ? dot(p-vThis, vNext-vThis)/max(0.00001,dists[sector]) : 0;
    float3 edgeDists = dists + (arcLengths+arcLengths.yzx)/2; // edge dists is dist + rounded sections
    float localDist = linearDist; // only if non-rounded


    // no roundness, linear
    // rounded, < circle
    // rounded, > circle

    if( roundness > 0 ) {
        // shit gets complicated oh boy
        float tLocal = hasLinearDist ? linearDist / max(0.00001,dists[sector]) : sign(dot(tangents[sector],p));

        if( tLocal < 0 || tLocal >= 1 ) { // >= here to include fully rounded sign == 1 case
            float2 pivotCorner = tLocal < 0 ? vThis : vNext;
            float2 relVec = p - pivotCorner;

            // we don't need to scale by inradius to get arc length here,
            // I think, since inradius == 1 in our coord system
            float ang = acos(clamp(Determinant(tangents[sector], normalize(relVec)),-1,1));
            float angArcLen = ang*roundnessRadius;
            if( tLocal < 0 )
                localDist = arcLengths[sector]/2 - angArcLen;
            else // if( tLocal > 1 )
                localDist = arcLengths[sector]/2 + dists[sector] + angArcLen;
        } else {
            localDist = arcLengths[sector]/2 + linearDist;
        }
            
    }
    
    bool snapEndToEnd = PROP(_DashSnap) == DASH_SNAP_ENDTOEND;
    if( snapEndToEnd ) {
        dist = localDist;
        distTotal = edgeDists[sector];
    } else {
        dist = localDist;
        dist += edgeDists[0] * (sector > 0); // add previous distances, if applicable
        dist += edgeDists[1] * (sector > 1);
        distTotal = (edgeDists[0]+edgeDists[1]+edgeDists[2]);
    }
        
    // scale back up to actual units instead of normalized inradius coords 
    distTotal *= i.IP_inradius;
    dist *= i.IP_inradius;
}

inline half GetAngularMask( VertexOutput i, half tRadial, out float tAngular ) {

    half mask = 1;
    tAngular = 0;
    if( IsDashed() && PROP(_Hollow) > 0 ) {
        float dist, distTotal;
        GetPerimeterDistance( i, /*out*/ dist, /*out*/ distTotal );
        DashConfig dash = GetDashConfig( 1, /*periodicEndToEnd = */ true ); // todo: uniform scale?
        DashCoordinates dashData = GetDashCoordinates( dash, dist, distTotal, i.IP_HALF_THICKNESS*2*i.IP_inradius, i.IP_pxPerMeter );
        ApplyDashMask( /*inout*/ mask, dashData, tRadial, dash.type, dash.modifier );
        tAngular = dist / distTotal;
    }
    
    return mask;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    half tRadial;
    float tAngular;
    half radialMask = GetRadialMask( i, /*out*/ tRadial );
    float angularMask = GetAngularMask( i, tRadial*2-1, /*out*/ tAngular );
    //return float4(tAngular,tRadial,0,1);
    return SHAPES_OUTPUT( i.color, radialMask*angularMask, i );
}