// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
PROP_DEF(int, _ScaleMode)
PROP_DEF(half4, _Color)
PROP_DEF(half4, _ColorEnd)
PROP_DEF(float3, _PointStart)
PROP_DEF(float3, _PointEnd)
PROP_DEF(half, _Thickness)
PROP_DEF(int, _ThicknessSpace)
PROP_DEF(int, _Alignment)
SHAPES_DASH_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#include "../DashUtils.cginc"

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

#define IP_dash_coord intp0.x
#define IP_dash_spacePerPeriod intp0.y
#define IP_dash_thicknessPerPeriod intp0.z
#define IP_pxCoverage intp0.w
#define IP_uv0 intp1.xy
#define IP_tColor intp1.z
#define IP_capLengthRatio intp1.w

struct VertexInput {
	float4 vertex : POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half4 intp0 : TEXCOORD0;
	half4 intp1 : TEXCOORD1;
	UNITY_FOG_COORDS(2)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    half3 aLocal = PROP(_PointStart);
    half3 bLocal = PROP(_PointEnd);
    int alignment = PROP(_Alignment);
    aLocal.z *= saturate(alignment); // flatten Z if _Alignment == ALIGNMENT_FLAT
    bLocal.z *= saturate(alignment);
	float3 a = LocalToWorldPos( aLocal );
	float3 b = LocalToWorldPos( bLocal );
	float3 vertOrigin = v.vertex.y < 0 ? a : b;

	half lineLengthVisual;
	half3 tangent;
	GetDirMag(b - a, /*out*/ tangent, /*out*/ lineLengthVisual);

    half3 normal;
    switch( alignment ){
        case ALIGNMENT_FLAT: {
            half3 localZ = normalize( half3( UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z ) );
            normal = cross( tangent, localZ );
			break;
	    }
        case ALIGNMENT_BILLBOARD: {
            half3 camForward = -DirectionToNearPlanePos( vertOrigin );
            normal = normalize(cross(tangent,camForward));   
			break;
        }
    	default: {
			normal = 0;
        	break;
	    }
    }
    
    int scaleMode = PROP(_ScaleMode);
    half uniformScale = GetUniformScale();
	half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? uniformScale : 1;
	
	
	
	half thickness = PROP(_Thickness) * scaleThickness;
	int thicknessSpace = PROP(_ThicknessSpace);
	LineWidthData widthData = GetScreenSpaceWidthData( vertOrigin, normal, thickness, thicknessSpace );
	
	o.IP_uv0 = v.vertex;
	half verticalPaddingTotal = AA_PADDING_PX / widthData.pxPerMeter;
	
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
	    o.IP_uv0.x *= widthData.aaPaddingScale; // scale compensate width
	    o.IP_uv0.y *= (lineLengthVisual + verticalPaddingTotal ) / lineLengthVisual; // scale compensate height
	#endif
	
	o.IP_pxCoverage = widthData.thicknessPixelsTarget;
	half radiusVtx = widthData.thicknessMeters / 2;
	
	#if defined(CAP_ROUND) || defined(CAP_SQUARE)
	    float3 vertPos = vertOrigin + (normal * v.vertex.x + tangent * v.vertex.y) * radiusVtx;
	#else
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
		    float3 vertPos = vertOrigin + normal * (v.vertex.x * radiusVtx) + tangent * (v.vertex.y * verticalPaddingTotal * 0.5);
		#else
		    float3 vertPos = vertOrigin + normal * v.vertex.x * radiusVtx;
		#endif
	#endif
	
    half radiusVisuals = 0.5*widthData.thicknessPixelsTarget / widthData.pxPerMeter;
    #if defined(CAP_ROUND) || defined(CAP_SQUARE)
        half endToEndLength = lineLengthVisual + radiusVisuals * 2;
    #else
        half endToEndLength = lineLengthVisual;
    #endif
    o.IP_capLengthRatio = (2*radiusVisuals)/endToEndLength;
    
	// dashes
	if( IsDashed() ) {
		float projDist = dot( tangent, vertPos - a ); // distance along line
		DashConfig dash = GetDashConfig(uniformScale);
		DashCoordinates dashCoords = GetDashCoordinates( dash, projDist, lineLengthVisual, 2*radiusVisuals, widthData.pxPerMeter );
		o.IP_dash_coord = dashCoords.coord;
		o.IP_dash_spacePerPeriod = dashCoords.spacePerPeriod;
		o.IP_dash_thicknessPerPeriod = dashCoords.thicknessPerPeriod;
	}
	
	// color
	#if defined(CAP_ROUND) || defined(CAP_SQUARE)
	    half k = 2 * radiusVtx / lineLengthVisual + 1;
        half m = -radiusVtx / lineLengthVisual;
        o.IP_tColor = k * (v.vertex.y/2+0.5) + m;
    #else
	    o.IP_tColor = v.vertex.y/2+0.5;
	#endif

	o.pos = WorldToClipPos( vertPos.xyz );
	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
} 

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
 
	UNITY_SETUP_INSTANCE_ID(i);
	
	float4 colorStart = PROP(_Color);
    float4 colorEnd = PROP(_ColorEnd);
	float4 shape_color = lerp(colorStart, colorEnd, saturate( i.IP_tColor ) );

	half shape_mask = 1;
	
	// edge masking
	#if LOCAL_ANTI_ALIASING_QUALITY > 0
	    half maskEdges = GetLineLocalAA( i.IP_uv0.x, i.IP_pxCoverage );
		shape_mask = min( shape_mask, maskEdges );
	#endif
	
    // cap masking
	#ifdef CAP_ROUND
		half2 uv = i.IP_uv0.xy;
		uv = abs(uv);
		uv.y = (uv.y-1)/i.IP_capLengthRatio + 1;
		half maskRound = StepAA(length(max(0,uv)),1);
		half useMaskRound = saturate(i.IP_pxCoverage/2); // only use LineLocalAA when very thin
		shape_mask = min( shape_mask, lerp( 1, maskRound, useMaskRound)); 
	#else
	    // if cap == square or no caps, also do uv.y masking for caps
	    #if LOCAL_ANTI_ALIASING_QUALITY > 0
	        shape_mask = min( shape_mask, GetLineLocalAA( i.IP_uv0.y, i.IP_pxCoverage ) );
	    #endif
	#endif

    int dashType = PROP(_DashType);
	half dashModifier = PROP(_DashShapeModifier);
	DashCoordinates dashCoords;
	dashCoords.coord = i.IP_dash_coord;
	dashCoords.spacePerPeriod = i.IP_dash_spacePerPeriod;
	dashCoords.thicknessPerPeriod = i.IP_dash_thicknessPerPeriod;
	ApplyDashMask( /*inout*/ shape_mask, dashCoords, i.IP_uv0.x, dashType, dashModifier );
    
	shape_mask *= saturate( i.IP_pxCoverage );
	return SHAPES_OUTPUT( shape_color, shape_mask, i );
}