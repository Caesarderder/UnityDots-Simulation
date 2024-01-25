// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

#define DASH_TYPE_BASIC 0
#define DASH_TYPE_ANGLED 1
#define DASH_TYPE_ROUND 2

#define DASH_SPACE_FIXED_COUNT -2
#define DASH_SPACE_RELATIVE -1
#define DASH_SPACE_METERS 0

#define DASH_SNAP_OFF 0
#define DASH_SNAP_TILING 1
#define DASH_SNAP_ENDTOEND 2
#define DASH_SNAP_ENDTOEND_PERIODIC 3 // note: only used in shader code to branch behavior, not in C#

// all the dash properties
struct DashConfig {
	half size;
	half spacing;
	half offset;
	int space;
	int thicknessSpace;
	int snap;
	int type;
	half modifier;
};

// coordinates used when rendering dashes in the fragment shader
struct DashCoordinates{
	half coord;
	half spacePerPeriod;
	half thicknessPerPeriod;
};

DashConfig GetDashConfig( half uniformScale, bool periodicEndToEnd = false ) {
	DashConfig d;
	d.size = PROP( _DashSize );
	d.spacing = PROP( _DashSpacing );
	d.space = PROP( _DashSpace );
	d.offset = PROP( _DashOffset );
	d.thicknessSpace = PROP( _ThicknessSpace );
	d.snap = PROP( _DashSnap );
	if( d.snap == DASH_SNAP_ENDTOEND && periodicEndToEnd )
		d.snap = DASH_SNAP_ENDTOEND_PERIODIC;
	d.type = PROP( _DashType );
	d.modifier = PROP( _DashShapeModifier );

	// scale size/spacing when using uniform scaling w. relative/meters
	int scaleMode = PROP(_ScaleMode);
	if( scaleMode == SCALE_MODE_UNIFORM && d.space != DASH_SPACE_FIXED_COUNT ) {
		d.size *= uniformScale;
		d.spacing *= uniformScale;
	}
	
	return d;
}

// this works in normalized space, repeating integers for every period
inline void ApplyDashMask( inout half shape_mask, DashCoordinates dashCoords, half coordAcross, int type, half dashModifier ){
    
    half spacePerPeriod = dashCoords.spacePerPeriod;

	if( spacePerPeriod >= 1.0-VERY_SMOL ){
		shape_mask = 0; // pretty much just space, make invisible
		// the flipside of this (only dashes, no space) 
		// is actually not gapless for rounded dashes by design,
		// so we can't just return solid/no masking naively
    } else {
        half thicknessPerPeriod = dashCoords.thicknessPerPeriod;
        half2 coord = half2( coordAcross, dashCoords.coord );
        
        if( type == DASH_TYPE_ANGLED )
            coord.y += 0.5*coord.x*thicknessPerPeriod*dashModifier; // 45° angle skewing        
		half dashSdf = abs(frac(coord.y) * 2 - 1); // triangle wave
		dashSdf = InverseLerp( spacePerPeriod, 1, dashSdf ); // convert to SDF matching dash ratio
		
		half dashMask;
		if( type == DASH_TYPE_ROUND ){
            half dashPerPeriod = 1.0-spacePerPeriod;
            half dashPerThickness = dashPerPeriod/thicknessPerPeriod;
		    half lenCoord = 1.0 - dashSdf*dashPerThickness;
		    half2 roundnoot = half2(saturate(lenCoord), coord.x);
            half sdfRound = length(half2(lenCoord, coord.x))-1;
            half maskRounds = 1.0-saturate( sdfRound / fwidth(sdfRound) + 0.5 ); // 0.5 offset to center on pixel bounds
            half maskFill = saturate( (dashSdf - 1/dashPerThickness) / fwidth(dashSdf) + 0.5 ); // 0.5 offset to center on pixel bounds
            dashMask = max(maskFill, maskRounds);
		} else {
            dashMask = saturate( dashSdf / fwidth(dashSdf) + 0.5 ); // 0.5 offset to center on pixel bounds		
		}
		
		shape_mask = min(shape_mask, dashMask);
	}
}

// returns snapped period count
inline half DashSnap( half periodCount, half spacePerPeriod, int snapMode ){
	switch( snapMode ){
		case DASH_SNAP_OFF:
			return periodCount;
		case DASH_SNAP_TILING:
		case DASH_SNAP_ENDTOEND_PERIODIC:
			return max( 1, round( periodCount ) );
		case DASH_SNAP_ENDTOEND:
			return max( 1, round( periodCount + spacePerPeriod ) ) - spacePerPeriod;
		default:
			return 0;
	}
}

inline DashCoordinates GetDashCoordinates( DashConfig dash, float dist, float distTotal, half thickness, half pxPerMeter ) {
	DashCoordinates dashCoords;
    
	// dist and dist total are both in meters, so we need to convert them to match if we're using relative coords
	if( dash.space == DASH_SPACE_RELATIVE ) {
		dist = MetersToOtherSpace( dist, pxPerMeter, dash.thicknessSpace );
		distTotal = MetersToOtherSpace( distTotal, pxPerMeter, dash.thicknessSpace );
		thickness = MetersToOtherSpace( thickness, pxPerMeter, dash.thicknessSpace );
	}
    
	// first, convert to dash count, to leave non-normalized space land asap because we hate it
	bool fixedCount = dash.space == DASH_SPACE_FIXED_COUNT;
	float periodCount, spacePerPeriod;
	if( fixedCount ){
		spacePerPeriod = dash.spacing;
		periodCount = DashSnap( dash.size, spacePerPeriod, dash.snap );
	} else {
		float rawPeriod = (dash.size + dash.spacing);
		spacePerPeriod = dash.spacing / rawPeriod;
		periodCount = DashSnap( distTotal / rawPeriod, spacePerPeriod, dash.snap );
	}
	float dashPerPeriod = 1-spacePerPeriod;
	
	if( dash.snap == DASH_SNAP_ENDTOEND_PERIODIC )
		dash.offset -= dashPerPeriod*0.5; // offset by half a dash distance to center

    
	dashCoords.spacePerPeriod = spacePerPeriod;
	dashCoords.thicknessPerPeriod = (thickness*periodCount) / (distTotal);
    
	float t = dist / distTotal; // normalized longitudinal coord
	
	dashCoords.coord = t * periodCount - dash.offset - dashPerPeriod/2;
    
	return dashCoords;
}

inline bool IsDashed() {
	return PROP( _DashSize ) > VERY_SMOL;
}