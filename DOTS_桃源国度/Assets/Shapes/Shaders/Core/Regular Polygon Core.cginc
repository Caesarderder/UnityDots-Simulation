// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(int, _Alignment)
PROP_DEF(half4, _Color)
PROP_DEF(half, _Radius)
PROP_DEF(int, _RadiusSpace)
PROP_DEF(half, _Thickness)
PROP_DEF(int, _ThicknessSpace)
PROP_DEF(half, _Angle)
PROP_DEF(half, _Roundness)
PROP_DEF(int, _Hollow)
PROP_DEF(int, _Sides)
SHAPES_DASH_PROPERTIES
SHAPES_FILL_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#include "../DashUtils.cginc"
#include "../FillUtils.cginc"

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

#define IP_uv0 intp0.xy
#define IP_pxPerMeter intp0.z
#define IP_apothem intp0.w
#define IP_innerRadiusFraction intp1.x
#define IP_pxCoverage intp1.y
#define IP_angHalfSegment intp1.z
#define IP_halfSideLength intp1.w
#define IP_thicknessMeters intp2.x
#define IP_radiusMeters intp2.y
#define IP_uniformScale intp2.z

struct VertexInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    half4 intp0 : TEXCOORD0;
	half4 intp1 : TEXCOORD1;
	half3 intp2 : TEXCOORD2;
	SHAPES_INTERPOLATOR_FILL(3)
	UNITY_FOG_COORDS(4)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	half2 objScale = GetObjectScaleXY();
	o.IP_uniformScale = GetUniformScale(objScale);
	
	float radius = PROP(_Radius) * o.IP_uniformScale;
	float radiusSpace = PROP(_RadiusSpace);
    LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, radius*2, radiusSpace );
    o.IP_pxCoverage = widthDataRadius.thicknessPixelsTarget;

    o.IP_radiusMeters = widthDataRadius.thicknessMeters / 2; // actually, center radius

	bool hollow = PROP(_Hollow) == 1;
	if( hollow ) {
		o.IP_pxPerMeter = widthDataRadius.pxPerMeter;
		int scaleMode = PROP(_ScaleMode);
		half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? o.IP_uniformScale : 1;
		float thickness = PROP(_Thickness) * scaleThickness;
		float thicknessSpace = PROP(_ThicknessSpace);
		LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, thickness, thicknessSpace );
		half thicknessRadius = widthDataThickness.thicknessMeters / 2;
		o.IP_pxCoverage = widthDataThickness.thicknessPixelsTarget; // todo: this isn't properly handling coordinate scaling yet
		half radiusOuter = o.IP_radiusMeters + thicknessRadius;
		o.IP_innerRadiusFraction = (radiusOuter - thicknessRadius*2) / radiusOuter;
		o.IP_thicknessMeters = widthDataThickness.thicknessMeters;
		v.vertex.xy = v.uv0 * radiusOuter;
	} else {
		v.vertex.xy = v.uv0 * o.IP_radiusMeters;
	}

	v.vertex.xy = Rotate(v.vertex.xy, PROP(_Angle));

	if( PROP(_Alignment) == ALIGNMENT_BILLBOARD ) {
		half3 frw = WorldToLocalVec( -DirectionToNearPlanePos( OBJ_ORIGIN ) );
		half3 camRightLocal = WorldToLocalVec( CAM_RIGHT );
		half3 up = normalize( cross( frw, camRightLocal ) );
		half3 right = cross( up, frw ); // already normalized
		v.vertex.xyz = v.vertex.x * right + v.vertex.y * up;
	}

	float n = PROP(_Sides);
	half angSegment = TAU/n;
	o.IP_angHalfSegment = angSegment/2;
	o.IP_apothem = cos( o.IP_angHalfSegment );
	o.IP_halfSideLength = sin( o.IP_angHalfSegment );

	
	
	o.IP_uv0 = v.uv0;
	v.vertex.xy /= objScale;
    o.pos = UnityObjectToClipPos( v.vertex );
	UNITY_TRANSFER_FOG(o,o.pos);
	o.fillCoords = GetFillCoords( v.vertex );

    return o;
}

inline half GetRadialMask( VertexOutput i, out float tLateral ){
	float n = PROP(_Sides);
	half angSegment = TAU/n;

	half2 coords = i.IP_uv0;
	half roundness = PROP(_Roundness);
	half roundnessInv = 1.0-roundness;
	half sdf = SdfNgon( angSegment, i.IP_apothem*roundnessInv, i.IP_halfSideLength*roundnessInv, coords );
	half mask = StepAA( sdf, roundness*i.IP_apothem ); // outer radius
	bool hollow = PROP(_Hollow) == 1;
	if( hollow ) {
		float sdfInner = sdf + 1-i.IP_innerRadiusFraction;
		tLateral = (sdf - roundness*i.IP_apothem)/(1-i.IP_innerRadiusFraction)+1;
		mask = min( mask, 1.0-StepAA( sdfInner, roundness*i.IP_apothem ) ); // inner radius
	}
	return mask;
}

void GetPerimeterDistance( VertexOutput i, out float dist, out float distTotal, float2 coord ) {
	
	int n = PROP(_Sides);
	bool snapEndToEnd = PROP(_DashSnap) == DASH_SNAP_ENDTOEND;
	int sector = floor( n * ( DirToAng( coord ) / TAU ) );
	if( sector < 0 )
		sector += n;

	float2 p = Rotate( coord,  -sector * TAU / n );
	float angHalfSegment = TAU / (2*n);
	float apothem = cos( angHalfSegment );
	float halfSideLength = sin( angHalfSegment );
	float2 normal = float2( apothem, halfSideLength );

	float sideLengthFlat = halfSideLength * 2;
	float2 scNormal = normal * sideLengthFlat;
	float pScNormDeterminant = Determinant( scNormal, p );
	bool isInSecondHalf = pScNormDeterminant > 0;
	if( isInSecondHalf )
		p = reflect( -p, normal );
	float projT = saturate( 0.5f - abs( pScNormDeterminant ) / ( sideLengthFlat * sideLengthFlat ) );
	float projectedDist = projT * sideLengthFlat;

	// radius = 1 during these calcs
	float roundness = PROP(_Roundness);
	float roundnessRadius = apothem * roundness;
	float roundnessStartOffset = halfSideLength * roundness;
	float localHalfArcLen = ( TAU / ( 2 * n ) ) * roundnessRadius;
	float sectorPerimeter = localHalfArcLen * 2 + ( sideLengthFlat - roundnessStartOffset * 2 );

	// full distance around the whole thing
	distTotal = snapEndToEnd ? sectorPerimeter : sectorPerimeter * n;
	dist = snapEndToEnd ? 0 : sectorPerimeter * sector; // add previous sectors

	float localPerimeterDist = 0;
	if( projectedDist < roundnessStartOffset ) {
		// inside the first arc bit
		float ang = DirToAng( p - float2( 1.0 - roundness, 0 ) );
		float arc = ang * roundnessRadius;
		localPerimeterDist += arc;
	} else {
		// after the arc, on the edge
		localPerimeterDist += localHalfArcLen;
		localPerimeterDist += projectedDist - roundnessStartOffset;
	}
	
	dist += isInSecondHalf ? sectorPerimeter - localPerimeterDist : localPerimeterDist; // add local perimeter progress

	// scale by radius
	half r = i.IP_radiusMeters;
	dist *= r;
	distTotal *= r;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	float tRadial = 0;
	half mask = GetRadialMask( i, /*out*/ tRadial );
	//return float4(tRadial.xxx,1);
	if( IsBetween( tRadial, -1, 2 ) && IsDashed() && PROP(_Hollow) > 0 ) {
		float dist, distTotal;
		GetPerimeterDistance( i, /*out*/ dist, /*out*/ distTotal, i.IP_uv0 );
		DashConfig dash = GetDashConfig( i.IP_uniformScale, /*periodicEndToEnd = */true );
		DashCoordinates dashData = GetDashCoordinates( dash, dist, distTotal, (1-i.IP_innerRadiusFraction)*i.IP_uniformScale, i.IP_pxPerMeter );
		ApplyDashMask( /*inout*/ mask, dashData, tRadial*2-1, dash.type, dash.modifier );
	}

	mask *= saturate(i.IP_pxCoverage); // pixel fade
	half4 fillColor = GetFillColor(i.fillCoords);
	return SHAPES_OUTPUT( fillColor, mask, i );
}


