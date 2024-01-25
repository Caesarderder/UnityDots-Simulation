// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(half4, _Color)
PROP_DEF(half4, _Rect)
#ifdef CORNER_RADIUS
    PROP_DEF(half4, _CornerRadii)
#endif
#ifdef BORDERED
    PROP_DEF(half, _Thickness)
	PROP_DEF(half, _ThicknessSpace)
	SHAPES_DASH_PROPERTIES
#endif
SHAPES_FILL_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#ifdef BORDERED
#include "../DashUtils.cginc"
#endif
#include "../FillUtils.cginc"

#define IP_uv0 intp0.xy
#define IP_nrmCoord intp0.zw
#define IP_rect intp1
#define IP_thickness intp2.x
#define IP_pxCoverage intp2.y
#define IP_pxPerMeter intp2.z

struct VertexInput {
    float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
	SHAPES_INTERPOLATOR_FILL(0)
    half4 intp0 : TEXCOORD1;
    half4 intp1 : TEXCOORD2;
    #if defined(BORDERED) || defined(CORNER_RADIUS)
        half3 intp2 : TEXCOORD3;
    #endif
	UNITY_FOG_COORDS(4)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
	half4 rect = PROP(_Rect);
	int scaleMode = PROP(_ScaleMode);
	half2 objScale = GetObjectScaleXY();
	bool useUniformScale = scaleMode == SCALE_MODE_UNIFORM;
	half2 rectScale = useUniformScale ? 1 : objScale;
	

	#ifdef BORDERED
		half scaleThickness = useUniformScale ? (objScale.x+objScale.y)/2 : 1;
		half thickness = PROP(_Thickness) * scaleThickness;
		half thicknessSpace = PROP(_ThicknessSpace);
	#else
		half thickness = 1; 
		half thicknessSpace = THICKN_SPACE_METERS;
	#endif

	// needed for LAA padding, even if not hollow
	float3 localCenter = float3( rect.xy + rect.zw/2, 0 );
	LineWidthData wd = GetScreenSpaceWidthDataSimple( LocalToWorldPos(localCenter), CAM_UP, thickness, thicknessSpace );
	
	#if defined(BORDERED)
		o.IP_pxPerMeter = wd.pxPerMeter;
		o.IP_pxCoverage = saturate(wd.thicknessPixelsTarget);
		o.IP_thickness = wd.thicknessMeters / scaleThickness;
	#endif
	
	#if defined(BORDERED) || defined(CORNER_RADIUS)
	    o.IP_nrmCoord = v.vertex.xy;
	#endif

	rect *= rectScale.xyxy;
	o.IP_rect = rect;

	half2 padScale = scaleMode == SCALE_MODE_UNIFORM ? objScale : 1;
	half2 padding = AA_PADDING_PX / (wd.pxPerMeter * padScale);

    o.IP_uv0 = Remap( half2(-1, -1), half2(1, 1), -rect.zw/2-padding.xy, rect.zw/2 + padding.xy, v.vertex.xy );
	v.vertex.xy = Remap( half2(-1, -1), half2(1, 1), rect.xy - padding.xy, rect.xy + rect.zw + padding.xy, v.vertex.xy );
	v.vertex.xy /= rectScale;
    o.pos = UnityObjectToClipPos( v.vertex );
	UNITY_TRANSFER_FOG(o,o.pos);
	o.fillCoords = GetFillCoords( v.vertex.xyz );
    return o;
}

#ifdef BORDERED

float2 QuadrantRotate( float2 v ) {
	if( v.x * v.y < 0 )
		v = float2( v.y, v.x );
	return float2( abs( v.x ), abs( v.y ) );
}

int GetQuadrant( float2 v ) {
	v = sign(v);
	return -v.y + ( -v.x * v.y + 3 ) / 2;
}

inline int GetLocalSector( float2 relVec ) {
	#ifdef CORNER_RADIUS
	if( relVec.x > 0 && relVec.y > 0 )
		return 1;
	else
	#endif
		return relVec.x - relVec.y > 0 ? 0 : 2;
}

inline void GetPerimeterDistance( VertexOutput i, out float perimeterDistance, out float totalPerimeter, half4 radii, float2 pt, half thickness ) {
	// rect specific things
	half borderPivot = 0; // PIVOT: 0 = outer, 0.5 = center, 1 = inner
	half borderOffset = borderPivot * thickness;
	bool snapEndToEnd = PROP(_DashSnap) == DASH_SNAP_ENDTOEND;
	radii = radii.zyxw - borderOffset; // todo: this is gross I hate this pls breaking change to fix maybe?
	uint quadrant = GetQuadrant( pt );
	uint quadrantPrev = (quadrant+3)%4;
	float2 p = QuadrantRotate( pt );
	float2 size = (( quadrant % 2 == 1 ) ? i.IP_rect.wz : i.IP_rect.zw) - borderOffset*2;
	float4 arcLengths = radii * TAU * 0.25; // arc lengths of each corner
	float4 fullSideLengths = i.IP_rect.zwzw - borderOffset*2;
	float4 edgePerimeters = (arcLengths + arcLengths.yzwx)/2 + fullSideLengths - radii - radii.yzwx;
	float4 quadrPerimeters = ( size.x + size.y ) / 2 + arcLengths - 2 * radii;


	float r = radii[quadrant];
	float2 arcOrigin = size / 2 - r;
	float2 relVec = p - arcOrigin;
	int localSector = GetLocalSector( relVec );

	totalPerimeter = 0; // just to suppress warnings etc I guess
	perimeterDistance = 0;
	if( snapEndToEnd ) {
		if( localSector == 0 ) {
			totalPerimeter = edgePerimeters[quadrantPrev]; // "previous" edge
			perimeterDistance = arcLengths[quadrantPrev]/2 + size.y/2 - radii[quadrantPrev] + p.y;
		} else if( localSector == 2 ) {
			totalPerimeter = edgePerimeters[quadrant];
			perimeterDistance = arcLengths[quadrant]/2 - r + size.x/2 - p.x;
		}
		#ifdef CORNER_RADIUS
		else if( localSector == 1 ) {
			// if x < y, then, 0 at arc center + edge dist
			// if x > y, then, 1 at arc center + previous arc+edge dist
			if( relVec.x < relVec.y ) {
				totalPerimeter = edgePerimeters[quadrant];
				perimeterDistance = r * (DirToAng( relVec ) - TAU/8); // start of a new segment
			} else {
				totalPerimeter = edgePerimeters[quadrantPrev]; // "previous" edge
				perimeterDistance = totalPerimeter - arcLengths[quadrant]/2 + DirToAng( relVec ) * r;
			}
		}
		#endif
	} else {
		totalPerimeter = quadrPerimeters.x + quadrPerimeters.y + quadrPerimeters.z + quadrPerimeters.w;
		perimeterDistance = snapEndToEnd ? 0 : dot( quadrant >= uint3(1,2,3), quadrPerimeters.xyz ); // all previous quadrants
		if(localSector == 0)
			perimeterDistance += p.y;
		else if( localSector == 2 )
			perimeterDistance += quadrPerimeters[quadrant] - p.x;
		#ifdef CORNER_RADIUS
		else if(localSector == 1)
			perimeterDistance += size.y / 2 - r + DirToAng( relVec ) * r; // add max y dist + arcLen
		#endif
	}
	
}

inline void ApplyDashes( inout half mask, VertexOutput i, half4 radii, half tRadial ){
	if( IsDashed() ) {
		float dist, distTotal;
		GetPerimeterDistance( i, /*out*/ dist, /*out*/ distTotal, radii, i.IP_uv0, i.IP_thickness );
		DashConfig dash = GetDashConfig( 1, /*periodicEndToEnd:*/ true ); // todo: uniform scale?
		DashCoordinates dashData = GetDashCoordinates( dash, dist, distTotal, i.IP_thickness, i.IP_pxPerMeter );
		ApplyDashMask( /*inout*/ mask, dashData, tRadial*2-1, dash.type, dash.modifier );		
	}
}
#endif

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	
	#ifdef CORNER_RADIUS
	    half4 cornerRadii = PROP(_CornerRadii);
	    fixed2 sgn = sign(i.IP_nrmCoord);
	    half maxRadius = min(i.IP_rect.z, i.IP_rect.w) / 2;
	    cornerRadii = min( cornerRadii, maxRadius ); // clamp all radii
		int rComp = sgn.x-0.5*sgn.x*sgn.y+1.5; // thanks @khyperia <3
	    half cornerRadius = cornerRadii[rComp];
    #else
		half4 cornerRadii = half4(0,0,0,0);
	#endif
	
	// base sdf
	#ifdef CORNER_RADIUS
        half2 indentBoxSize = (i.IP_rect.zw - cornerRadius.xx*2);
        half boxSdf = SdfBox( i.IP_uv0.xy, indentBoxSize/2 ) - cornerRadius;
    #else
        half boxSdf = SdfBox( i.IP_uv0.xy, i.IP_rect.zw/2 );
    #endif
    
    // apply border to sdf
    #ifdef BORDERED
	    half thickness = i.IP_thickness;
        half halfthick = thickness / 2;
		half boxSdfPreAbs = boxSdf;
		boxSdf = abs(boxSdf + halfthick) - halfthick;
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
            half boxSdfPd = PD( boxSdf ); // todo: this has minor artifacts on inner corners, might want to separate masks by axis
            half shape_mask = 1.0-StepThresholdPD( boxSdf, boxSdfPd );
        #else
            half shape_mask = 1-StepAA(boxSdf);
        #endif

		// DASHES
		ApplyDashes(/*inout*/ shape_mask, i, cornerRadii, 1+boxSdfPreAbs/thickness );
	
		shape_mask *= i.IP_pxCoverage;
	#else
        half shape_mask = 1.0-StepAA( boxSdf );
    #endif
    
	half4 shape_color = GetFillColor( i.fillCoords );
	
	return SHAPES_OUTPUT( shape_color, shape_mask, i );
}