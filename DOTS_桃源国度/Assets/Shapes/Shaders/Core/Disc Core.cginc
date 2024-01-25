// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(int, _Alignment)
PROP_DEF(half4, _Color)
PROP_DEF(half4, _ColorOuterStart)
PROP_DEF(half4, _ColorInnerEnd)
PROP_DEF(half4, _ColorOuterEnd)
PROP_DEF(half, _Radius)
PROP_DEF(int, _RadiusSpace)
PROP_DEF(half, _Thickness)
PROP_DEF(int, _ThicknessSpace)
PROP_DEF(half, _AngleStart)
PROP_DEF(half, _AngleEnd)
PROP_DEF(int, _RoundCaps)
#ifdef INNER_RADIUS
SHAPES_DASH_PROPERTIES
#endif
UNITY_INSTANCING_BUFFER_END(Props)
#ifdef INNER_RADIUS
#include "../DashUtils.cginc"
#endif

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

#define IP_uv0 intp0.xy
#define IP_pxCoverage intp0.z
#define IP_innerRadiusFraction intp0.w
#define IP_centerRadiusMeters intp1.x
#define IP_thicknessMeters intp1.y
#define IP_pxPerMeter intp1.z
#define IP_uniformScale intp1.w

struct VertexInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    half4 intp0 : TEXCOORD0;
    #ifdef INNER_RADIUS
		half4 intp1 : TEXCOORD1;
    #endif
	UNITY_FOG_COORDS(2)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

// I hate C
inline void ApplyRadialMask( inout half mask, VertexOutput i, out half tRadial );
inline void ApplyAngularMask( inout half mask, VertexOutput i, out float tAngularFull, out float tAngular, out half2 coord, out float ang, out half angStart, out half angEnd, out bool useRoundCaps, out half sectorSize );
inline void ApplyEndCaps( inout half mask, VertexOutput i, half2 coord, half ang, half angStart, half angEnd, bool useRoundCaps );
inline void ApplyDashes( inout half mask, VertexOutput i, float t, half tRadial, half sectorSize );
inline half4 GetColor( half tRadial, half tAngular );
inline half4 GetBounds( bool centerAlign, half alignOffset, half angDelta, half innerFrac, half outerFrac );

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	half uniformScale = GetUniformScale();
	half radius = PROP(_Radius) * uniformScale;
	int radiusSpace = PROP(_RadiusSpace);
    LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, radius*2, radiusSpace );
    o.IP_pxCoverage = widthDataRadius.thicknessPixelsTarget;

	// padding correction
	half paddingMeters = AA_PADDING_PX/widthDataRadius.pxPerMeter;
    half radiusInMeters = widthDataRadius.thicknessMeters / 2; // actually, center radius
	half vertexRadius;
	half outerRadiusFraction;
	half innerRadiusFraction = 0;
	
	#ifdef INNER_RADIUS
        o.IP_pxPerMeter = widthDataRadius.pxPerMeter;
	    int scaleMode = PROP(_ScaleMode);
        half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? uniformScale : 1;
	    half thickness = PROP(_Thickness) * scaleThickness;
	    int thicknessSpace = PROP(_ThicknessSpace);
	    LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( OBJ_ORIGIN, CAM_RIGHT, thickness, thicknessSpace );
	    half thicknessRadius = widthDataThickness.thicknessMeters / 2;
	    o.IP_thicknessMeters = widthDataThickness.thicknessMeters;
	    o.IP_pxCoverage = widthDataThickness.thicknessPixelsTarget; // todo: this isn't properly handling coordinate scaling yet
	    half radiusOuter = radiusInMeters + thicknessRadius;
		vertexRadius = radiusOuter + paddingMeters;
		outerRadiusFraction = radiusOuter / vertexRadius;
	    o.IP_innerRadiusFraction = (radiusOuter - thicknessRadius*2) / radiusOuter;
		innerRadiusFraction = o.IP_innerRadiusFraction;
	    o.IP_centerRadiusMeters = radiusInMeters;
	    o.IP_uniformScale = uniformScale;
	#else
		vertexRadius = radiusInMeters + paddingMeters;
		outerRadiusFraction = radiusInMeters / vertexRadius;
	#endif
	

	// change the bounding box based on angles to save fill rate performance
	#ifdef SECTOR
		half angStartRaw = PROP(_AngleStart);
		half angEndRaw = PROP(_AngleEnd);
		half angStart = min(angStartRaw,angEndRaw);
		half angEnd = max(angStartRaw,angEndRaw);
	
		half angDeltaRaw = angEnd - angStart;
		half angDelta = clamp( angDeltaRaw, -TAU, TAU );
		half angSpan = abs(angDelta);

		half alignOffset = (angSpan-TAU/2)*0.5;
		bool centerAlign = true;
		#ifndef INNER_RADIUS
			if( angSpan <= TAU/2 ) { // don't center align pies until more than half a turn
				alignOffset = 0; // to corner-align pies better
				centerAlign = false;
			}
		#endif
	
		half4 bounds = GetBounds( centerAlign, alignOffset, angDelta, innerRadiusFraction, outerRadiusFraction );
		v.uv0 = Remap( half2(-1,-1), half2(1,1), bounds.xy, bounds.zw, v.uv0 );
		v.uv0 = Rotate(v.uv0, angStart + alignOffset);
		
	#endif
	
	v.vertex.xy = v.uv0 * vertexRadius;
	v.uv0 /= outerRadiusFraction; // padding correction
	

	if( PROP(_Alignment) == ALIGNMENT_BILLBOARD ) {
		half3 frw = WorldToLocalVec( -DirectionToNearPlanePos( OBJ_ORIGIN ) );
		half3 camRightLocal = WorldToLocalVec( CAM_RIGHT );
		half3 up = normalize( cross( frw, camRightLocal ) );
		half3 right = cross( up, frw ); // already normalized
		v.vertex.xyz = v.vertex.x * right + v.vertex.y * up;
	}
	
	o.IP_uv0 = v.uv0;
    o.pos = UnityObjectToClipPos( v.vertex / uniformScale );
	UNITY_TRANSFER_FOG(o,o.pos);

    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i); 

    half tRadial;
	float tAngular; // interpolators for radial & angular gradient
    float tAngularFull; // angular gradient 0 to 1, used for dashes
	float ang;
	half angStart, angEnd;
	bool useRoundCaps;
	half2 coord; // coordinates used for end caps, if applicable
	half sectorSize; // used to snap dash coords
	
	half mask = 1;
	ApplyRadialMask( /*inout*/ mask, i, /*out*/ tRadial );
	ApplyAngularMask( /*inout*/ mask, i,/*out*/ tAngularFull, /*out*/ tAngular, /*out*/ coord, /*out*/ ang, /*out*/ angStart, /*out*/ angEnd, /*out*/ useRoundCaps, /*out*/ sectorSize );
	#ifdef INNER_RADIUS
		ApplyEndCaps(/*inout*/ mask, i, coord, ang, angStart, angEnd, useRoundCaps);
	    ApplyDashes( /*inout*/ mask, i, tAngularFull, tRadial, sectorSize );
	#endif
	mask *= saturate(i.IP_pxCoverage); // pixel fade
	
    half4 color = GetColor( tRadial, tAngular );
	return SHAPES_OUTPUT( color, mask, i );
}


// minX, minY, maxX, maxY
inline half4 GetBounds( bool centerAlign, half alignOffset, half angDelta, half innerFrac, half outerFrac ) {
	half laaPadding = -(1-outerFrac);
	half yMinRootRef = laaPadding; // padded for LAA
	half aSpan = abs(angDelta);
	half2 dir = AngToDir(aSpan - alignOffset);
	half2 tipMin = min(0,dir);
	half tipMaxY = dir.y;
	#ifdef INNER_RADIUS
		bool roundCaps = PROP( _RoundCaps );
		if(roundCaps){
			half uvRingRadius = (1-innerFrac)*0.5-laaPadding;
			half2 dirSclCnt = dir*(1-uvRingRadius);
			yMinRootRef -= uvRingRadius;
			tipMin = dirSclCnt-uvRingRadius.xx;
			tipMaxY = dirSclCnt.y+uvRingRadius;
		} else {
			half2 dirSclInner = dir*innerFrac+laaPadding;
			tipMin = min(dirSclInner, dir);
		}
	#endif

	half2 minC, maxC;
	if( centerAlign ){ // arc center always at the top
		minC.x = aSpan > TAU*0.50 ? -1 : tipMin.x;
		minC.y = tipMin.y;
		maxC.x = -minC.x;
		maxC.y = 1;
	} else {
		minC.x = aSpan > TAU*0.50 ? -1 : tipMin.x;
		half y0 = aSpan > TAU*0.75 ? tipMin.y : min(tipMin.y, yMinRootRef);
		half y1 = aSpan > TAU*0.25 ? 1 : tipMaxY;
		minC.y = min(y0,y1);
		maxC.y = max(y0,y1);
		maxC.x = 1; // start angle always fully to the right
	}

	return half4(minC, maxC);
}


inline half ArcLengthToAngle( half radius, half arcLength ){
    return arcLength / radius;
}

#ifdef INNER_RADIUS
inline void ApplyDashes( inout half mask, VertexOutput i, float t, half tRadial, half angularSpan ){
	if( IsDashed() ) {
		half radiusMeters = i.IP_centerRadiusMeters;
		float dist = t * radiusMeters * TAU; // arc length in meters right now
		float distTotal = radiusMeters * angularSpan;
		DashConfig dash = GetDashConfig( i.IP_uniformScale );
		DashCoordinates dashCoords = GetDashCoordinates( dash, dist, distTotal, i.IP_thicknessMeters, i.IP_pxPerMeter );
		ApplyDashMask( /*inout*/ mask, dashCoords, tRadial*2-1, dash.type, dash.modifier );
	}
}
#endif

inline void ApplyRadialMask( inout half mask, VertexOutput i, out half tRadial ){
    half len = length( i.IP_uv0 );
    mask = min( mask, StepAA( len, 1 ) ); // outer radius
	#ifdef INNER_RADIUS
		mask = min( mask, 1.0-StepAA( len, i.IP_innerRadiusFraction ) ); // inner radius
		tRadial = saturate(InverseLerp( i.IP_innerRadiusFraction, 1, len ) );
	#else
	    tRadial = saturate(len);
	#endif
}

inline void ApplyAngularMask( inout half mask, VertexOutput i, out float tAngularFull, out float tAngular, out half2 coord, out float ang, out half angStart, out half angEnd, out bool useRoundCaps, out half sectorSize ){

    #ifdef SECTOR
        angStart = PROP(_AngleStart);
		angEnd = PROP(_AngleEnd);
	
	    // Rotate so that the -pi/pi seam is opposite of the visible segment
		// 0 is the center of the segment post-rotate
		half angOffset = -(angEnd + angStart) * 0.5;
	    coord = Rotate( i.IP_uv0, angOffset );
	    angStart += angOffset;
	    angEnd += angOffset;
	#else
	    // required for angular gradients on rings and discs
		angStart = 0;
        angEnd = TAU;
        coord = -i.IP_uv0;
	#endif
	
	half angDelta = clamp( angEnd - angStart, -TAU, TAU );
	coord.y *= sign(angDelta); // since start/end is flipped for reversed situations
	ang = atan2( coord.y, coord.x ); // -pi to pi
	
	sectorSize = abs(angDelta);
	tAngular = saturate(ang/sectorSize + 0.5); // angular interpolator for color
	
	
	#ifdef SECTOR
	
	    useRoundCaps = PROP( _RoundCaps );
	        
	    float segmentMask;
		#if LOCAL_ANTI_ALIASING_QUALITY == 0
		    segmentMask = StepAA( abs(ang), sectorSize*0.5 );
		#else
		    // if arc
		    #ifdef INNER_RADIUS
		        if( useRoundCaps ){ // arcs with round caps hide the border anyway, so use cheap version
                    segmentMask = step( abs(ang), sectorSize*0.5 );
                } else {
		    #endif
		    
            float2 pdCoordSpace = float2( -coord.y, coord.x ) / dot( coord, coord );
            segmentMask = StepAAManualPD( coord, abs(ang), sectorSize*0.5, pdCoordSpace );
            
            // if arc
            #ifdef INNER_RADIUS
                } // this is a little cursed I know I'm sorry~
		    #endif
		    
		#endif
		
		// Adjust if close to 0 or TAU radians, fade in or out completely
		half THRESH_INVIS = 0.001;
		half THRESH_VIS = 0.002;
		half fadeInMask = saturate( InverseLerp( TAU - THRESH_VIS, TAU - THRESH_INVIS, sectorSize ) );
		half fadeOutMask = saturate( InverseLerp( THRESH_INVIS, THRESH_VIS, sectorSize ) );
		mask *= lerp( segmentMask * fadeOutMask, 1, fadeInMask );
	#else 
	    // SECTOR not defined
	    useRoundCaps = false;
	#endif
	
	// used for dashes
	#ifdef INNER_RADIUS
	    tAngularFull = (ang + sectorSize/2)/TAU;
	#else
	    tAngularFull = 0;
	#endif

}

inline void ApplyEndCaps( inout half mask, VertexOutput i, half2 coord, half ang, half angStart, half angEnd, bool useRoundCaps ){
    #if defined(INNER_RADIUS) && defined(SECTOR)
        if( useRoundCaps ){
            half halfThickness = (1-i.IP_innerRadiusFraction)/2;
            half distToCenterOfRing = 1-halfThickness;
            half angToA = abs( ang - angStart );
            half angToB = abs( ang - angEnd );
            half capAng = (angToA < angToB) ? angStart : angEnd;
            half2 capCenter = AngToDir(capAng) * distToCenterOfRing;
            half endCapMask = StepAA( length(coord - capCenter), halfThickness );
            mask = max(mask, endCapMask);
        }
    #endif
}

inline half4 GetColor( half tRadial, half tAngular ){
    half4 colInnerStart = PROP(_Color);
    half4 colOuterStart = PROP(_ColorOuterStart);
    half4 colInnerEnd = PROP(_ColorInnerEnd);
    half4 colOuterEnd = PROP(_ColorOuterEnd);
	half4 colorStart = lerp( colInnerStart, colOuterStart, tRadial );
	half4 colorEnd = lerp( colInnerEnd, colOuterEnd, tRadial );
	return lerp( colorStart, colorEnd, tAngular );
} 