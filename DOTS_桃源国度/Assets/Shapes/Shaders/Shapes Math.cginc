// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

#define SHAPES_INCLUDED_MATH

// iOS metal and vulkan seems to convert half to float,
// so overloads error out as redefinitions of the float functions
#define SUPPORTS_LOWP_OVERLOADS (defined(SHADER_API_METAL) == false && defined(SHADER_API_VULKAN) == false)

// constants
#define TAU 6.28318530718
#define VERY_SMOL 0.00006103515625 // smallest positive normal number for half precision floats

#define CAM_POS        _WorldSpaceCameraPos
#define CAM_RIGHT      UNITY_MATRIX_I_V._m00_m10_m20
#define CAM_UP         UNITY_MATRIX_I_V._m01_m11_m21
#define CAM_FORWARD    UNITY_MATRIX_I_V._m02_m12_m22
#define OBJ_ORIGIN     UNITY_MATRIX_M._m03_m13_m23

#define THICKN_SPACE_METERS 0
#define THICKN_SPACE_PIXELS 1
#define THICKN_SPACE_NOOTS 2

#define SCALE_MODE_UNIFORM 0
#define SCALE_MODE_COORDINATE 1

#if LOCAL_ANTI_ALIASING_QUALITY == 0
    #define AA_PADDING_PX 0
#else
    #define AA_PADDING_PX 2
#endif

// remap functions
inline float InverseLerp( float a, float b, float v ) {     return (v - a) / (b - a); }
inline float2 InverseLerp( float2 a, float2 b, float2 v ) { return (v - a) / (b - a); }
inline float3 InverseLerp( float3 a, float3 b, float v ) {  return (v - a) / (b - a); }
float2 Remap( float2 iMin, float2 iMax, float2 oMin, float2 oMax, float2 v ) {
    float2 t = InverseLerp( iMin, iMax, v );
    return lerp( oMin, oMax, t );
}
half Remap( half iMin, half iMax, half oMin, half oMax, half v ) {
    half t = InverseLerp( iMin, iMax, v );
    return lerp( oMin, oMax, t );
}

inline bool IsBetween( half v, half min, half max ) {
    return v > min && v < max;
}

// iOS metal seem to convert half to float, so these overloads error out as redefinitions of the above functions
#if SUPPORTS_LOWP_OVERLOADS 
inline half InverseLerp( half a, half b, half v ) {         return (v - a) / (b - a); }
inline half2 InverseLerp( half2 a, half2 b, half2 v ) {     return (v - a) / (b - a); }
half2 Remap( half2 iMin, half2 iMax, half2 oMin, half2 oMax, half2 v ) {
    half2 t = InverseLerp( iMin, iMax, v );
    return lerp( oMin, oMax, t );
}
#endif


inline float Round( float a, float divs ){
	return round(a*divs)/divs;
}

// vector utils
float Determinant( in float2 a, in float2 b ) {
    return a.x * b.y - a.y * b.x;
}
float2 Rotate( float2 v, float ang ){
	float2 a = float2( cos(ang), sin(ang) );
	return float2(
		a.x * v.x - a.y * v.y,
		a.y * v.x + a.x * v.y
	);
}
inline half2 AngToDir( half ang ){
    return half2(cos(ang),sin(ang));
}
inline half DirToAng( half2 dir ){
    return atan2( dir.y, dir.x );
}
inline float2 Rotate90Left( in float2 v ){
	return float2( -v.y, v.x );
}
inline float2 Rotate90Right( in float2 v ){
	return float2( v.y, -v.x );
}
inline float3 Rotate90Left( in float3 v ){
	return float3( -v.y, v.x, 0 );
}
inline float3 Rotate90Right( in float3 v ){
	return float3( v.y, -v.x, 0 );
}
inline half Repeat(float v, float len) {
    return clamp( v - floor( v / len ) * len, 0, len );
}
inline half DeltaAngle(half a, half b) {
    return Repeat( b - a + TAU*0.5, TAU ) - TAU*0.5;
}
void GetDirMag( in float2 v, out float2 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}
void GetDirMag( in float3 v, out float3 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}
inline half2 ClampedVecReject( in half2 p, in half2 v ) {
    return p - v*saturate( dot(p,v)/dot(v,v) );
}
void GetMiterOffset( out float3 dir, out float len, float3 aNormal, float3 bNormal, float radius ) {
    float dotVal = dot(aNormal, bNormal);
    if (dotVal < -0.99) {
        dir = float3(0, 0, 0);
        len = 0;
    } else {
        dir = normalize( aNormal + bNormal );
        len = radius / max(0.0001,dot( dir, bNormal ));
    }
}
void GetMiterOffsetFast( out half3 dir, out half len, half3 aNormal, half3 bNormal, half radius ) {
    dir = normalize( aNormal + bNormal );
    len = radius / max(0.0001,dot( dir, bNormal ));
}
struct MiterOffset2D {
    half2 dir;
    half len;
};
inline MiterOffset2D GetMiterOffsetFast( half2 aNormal, half2 bNormal, half radius ) {
    MiterOffset2D m;
    m.dir = normalize( aNormal + bNormal );
    m.len = radius / max(0.0001,dot( m.dir, bNormal ));
    return m;
}
inline half2 GetMiterOffsetDirFast( half2 aNormal, half2 bNormal, half radius ) {
    MiterOffset2D m = GetMiterOffsetFast( aNormal, bNormal, radius );
    return m.dir * m.len;
}
inline half4 WeightedSum( half3 w, half4 a, half4 b, half4 c ){
    return a*w.x + b*w.y + c*w.z;
}
inline half3 WeightedSum( half3 w, half3 a, half3 b, half3 c ){
    return a*w.x + b*w.y + c*w.z;
}
inline half2 WeightedSum( half3 w, half2 a, half2 b, half2 c ){
    return a*w.x + b*w.y + c*w.z;
}

// color/value utils
inline float4 GammaCorrectVertexColorsIfNeeded( in float4 color ){
    #ifdef UNITY_COLORSPACE_GAMMA
        return color;
    #else
        return float4( GammaToLinearSpace(color.rgb), color.a ); 
    #endif
}
float PxNoise( float2 uv ){
    float2 s = uv + 0.2127+uv.x*0.3713*uv.y;
    float2 r = 4.789*sin(489.123*s);
    return frac(r.x*r.y*(1+s.x));
}



// modified version of http://iquilezles.org/www/articles/ibilinear/ibilinear.htm
float2 InvBilinear( in float2 p, in float2 a, in float2 b, in float2 c, in float2 d ) {
    float2 res = float2(-1, -1);

    float2 e = d-a;
    float2 f = b-a;
    float2 g = a-d+c-b;
    float2 h = p-a;
        
    float k2 = Determinant( g, f );
    float k1 = Determinant( e, f ) + Determinant( h, g );
    float k0 = Determinant( h, e );
    
    // if edges are parallel, this is a linear equation. Do not this test here though, do
    // it in the user code
    if( abs(k2)<0.001 ) {
        float v = -k0/k1;
        float u  = (h.x*k1+f.x*k0) / ((e.x*k1-g.x*k0));
        /*if( v>0.0 && v<1.0 && u>0.0 && u<1.0 )*/  res = float2( u, v );
    } else {
        // otherwise, it's a quadratic
        float w = sqrt(max(0, k1*k1 - 4.0*k0*k2));

        float ik2 = 0.5/k2;
        float v = (-k1 - w)*ik2;
        if( v<0.0 || v>1.0 )
            v = (-k1 + w)*ik2;
        float u = (h.x - f.x*v)/(e.x + g.x*v);
        res = saturate(float2( u, v ));
    }
    
    return res;
}

// partial derivative shenanigans
#if LOCAL_ANTI_ALIASING_QUALITY != 0
inline float PD( float value ){
	#if LOCAL_ANTI_ALIASING_QUALITY == 2
		float2 pd = float2(ddx(value), ddy(value));
		return sqrt( dot( pd, pd ) );
	#endif
	#if LOCAL_ANTI_ALIASING_QUALITY == 1
		return fwidth( value );
	#endif
}
#endif

inline float StepThresholdPD( float value, float pd ) {
    return saturate( value / max( 0.00001, pd ) + 0.5 ); // sooooooooooooo this is complicated, whether it should be +0 or +.5
}
inline float StepThresholdPDAAOffset( float value, float pd, float aaOffset ) {
    return saturate( value / max( 0.00001, pd ) + aaOffset );
}
inline float StepAAExplicitPD( float value, float pdValue ){
    #if LOCAL_ANTI_ALIASING_QUALITY == 0
    return step(0,value);
    #else
    return StepThresholdPD( value, PD( pdValue ) );
    #endif
}
inline float StepAA( float value ) {
    return StepAAExplicitPD( value, value );
}
inline float StepAA( float thresh, float value ){
	return StepAA( value - thresh );
}


inline float SdfToMask( float value, float thresh ){
    float sdf = value - thresh;
    return 1-StepAA( sdf );
}

#if LOCAL_ANTI_ALIASING_QUALITY != 0
float StepAAPdThresh( float thresh, float value ){
    float pd = PD(thresh);
    value = value * pd;
    return StepAA( thresh, value );
}
float StepAAManualPD( float2 coords, float sdf, float thresh, float2 pdCoordSpace ){
	float2 pdScreenSpace = pdCoordSpace * PD( coords ); // Transform uv to screen space (does not support rotations, I think)
	float pdMag = length( pdScreenSpace ); // Get the magnitude of change
	float sub = sdf - thresh;
	return 1.0-saturate( sub / pdMag );
}
inline float GetLineLocalAA( float coord, float pxCoverage, half pxOffset = 0 ){
    float ddxy = PD(coord);
    float sdf = abs(coord)-1;
    float aaOffset = saturate(InverseLerp( 0, 1.1, pxCoverage )) * 0.5; // the 1.1 here is very much a hack but it looks good okay THIN LINES ARE HARD
    return 1.0-StepThresholdPDAAOffset( sdf, ddxy, aaOffset + pxOffset );
}
#endif

struct Circle {
    half2 pos;
    half r;
};

Circle GetIncirclePosRadius( half2 a, half2 b, half2 c, half ab, half bc, half ca ) {
    half sideSum = bc + ca + ab;
    half s = sideSum * 0.5;
    Circle circle;
    circle.pos = ( bc * a + ca * b + ab * c ) / sideSum;
    circle.r = sqrt( ( s - bc ) * ( s - ca ) * ( s - ab ) / s );
    return circle;
}

Circle GetIncirclePosRadius( half2 a, half2 b, half2 c ) {
    half ab = distance( a, b );
    half bc = distance( b, c );
    half ca = distance( c, a );
    return GetIncirclePosRadius(a,b,c,ab,bc,ca);
}

// sdfs
inline float SdfBox( float2 coord, float2 size ) {
    float2 q = abs(coord) - size;
    return length(max(0,q)) + min(0,max(q.x,q.y));
}

// The MIT License (for the function below)
// Copyright © 2018 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// (modified to align with a vertex on the right, have constants as inputs and using some of my helper functions)
half SdfNgon( half tauOverN, half apothem, half halfSideLength, half2 p ) {
    half halfAng = tauOverN/2;
    half pAng = DirToAng(p)-halfAng;
    half bn = tauOverN*floor( (pAng+halfAng)/tauOverN );
    half2 cs = AngToDir(bn+halfAng);
    p = mul(p, half2x2(cs.x,-cs.y,cs.y,cs.x));
    return length(p-half2(apothem,clamp(p.y,-halfSideLength,halfSideLength)))*sign(p.x-apothem);
}

// The MIT License (for most of the function below)
// Copyright © 2018 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// This is modified to use my helper functions and made cheaper because we can assume correct handedness
half SdfTriangle( half2 p, half2 p0, half2 p1, half2 p2 ) {
    half2 e0 = p1 - p0;
    half2 e1 = p2 - p1;
    half2 e2 = p0 - p2;

    half2 v0 = p - p0;
    half2 v1 = p - p1;
    half2 v2 = p - p2;

    half2 pq0 = ClampedVecReject(v0, e0);
    half2 pq1 = ClampedVecReject(v1, e1);
    half2 pq2 = ClampedVecReject(v2, e2);
    
    half2 d = min( min( half2( dot( pq0, pq0 ), Determinant(v0,e0) ),
                       half2( dot( pq1, pq1 ), Determinant(v1,e1) )),
                       half2( dot( pq2, pq2 ), Determinant(v2,e2) ));

    return -sqrt(d.x)*sign(d.y);
}

inline half SdfLine( half2 coord, half2 a, half2 b ) {
    return Determinant( coord-a, b-a );
}

// smoothing and tweening
inline float Smooth( float x ) { // Cubic
	return x * x * (3.0 - 2.0 * x);
}
inline float Smooth2( float x ) { // Quartic
	return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}
inline float EaseOutBack( float x, float p ){
	return lerp( x, pow( x, p )+p*(1-x), x );
}
inline float EaseOutBack2( float x ){
	return x * (3 + x*(x-3) );
}
inline float EaseOutBack3( float x ){
	return x * (4 + x*(x*x-4) );
}
inline float EaseOutBack4( float x ){
	return x * (5 + x*( x*x*x-5 ) );
}
inline float EaseOutBack5( float x ){
	return x * (6 + x*( x*x*x*x-6 ) );
}

#ifdef SCENE_VIEW_PICKING
    // long story short - when picking in the scene view, it does so by rendering a tiny 4x4 RT
    // but it does not write to _ScreenParams, so any pixel-sized objects get very incorrect values
    #define SCREEN_PARAMS float4( 4, 4, 1.25, 1.25 )
#else
    #define SCREEN_PARAMS _ScreenParams
#endif 

// space utils
inline float4 WorldToClipPos( in float3 worldPos ) {
    return mul( UNITY_MATRIX_VP, float4( worldPos, 1 ) );
}
inline float4 ViewToClipPos( in float3 viewPos ) {
    return mul( UNITY_MATRIX_P, float4( viewPos, 1 ) );
}
inline float4 LocalToClipPos( in float3 localPos ) {
    return UnityObjectToClipPos( float4( localPos, 1 ) );
}
inline float3 LocalToWorldPos( in float3 localPos ){
    return mul( UNITY_MATRIX_M, float4( localPos, 1 )).xyz; 
}
/*inline float3 LocalToViewPos( in float3 localPos ){
    return UnityObjectToViewPos( localPos ); // mul( UNITY_MATRIX_MV, float4( localPos, 1 )).xyz; 
}
inline float3 LocalToViewVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_MV, localVec ).xyz; // Unity stop warning about this pls this is valid :c
}*/

inline float3 WorldToLocalVec( in float3 worldVec ) {
    return mul((float3x3)unity_WorldToObject, worldVec);
}
inline float3 LocalToWorldVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_M, localVec ); 
}
inline float3 CameraToWorldVec( float3 camVec ){
    return mul( (float3x3)UNITY_MATRIX_I_V, camVec );
}
float2 WorldToScreenSpace( float3 worldPos ){
    float4 clipSpace = UnityObjectToClipPos( float4( worldPos, 1 ) );
    float2 normalizedScreenspace = clipSpace.xy / clipSpace.w;
    return 0.5*(normalizedScreenspace+1.0) * SCREEN_PARAMS.xy;
}
float CalcPixelsPerMeter( float3 worldPos, float3 worldDir ){
    float w = dot( UNITY_MATRIX_VP._m30_m31_m32_m33, float4(worldPos,1) );
    float2 clipVec = float2(
        dot( UNITY_MATRIX_VP._m00_m01_m02, worldDir )/w,
        dot( UNITY_MATRIX_VP._m10_m11_m12, worldDir )/w
    );
    return length(clipVec * SCREEN_PARAMS.xy)/2;
}

inline float NootsToPixels( in float noots ){
    return min( SCREEN_PARAMS.x, SCREEN_PARAMS.y ) * ( noots / NOOTS_ACROSS_SCREEN );
}
inline float PixelsToNoots( in float pixels ){
    return (NOOTS_ACROSS_SCREEN * pixels) / min( SCREEN_PARAMS.x, SCREEN_PARAMS.y );
}

// camera stuff
inline bool IsOrthographic(){
    return unity_OrthoParams.w == 1;
}

inline half3 DirectionToNearPlanePos( float3 pt ){
    if( IsOrthographic() ){
        return -CAM_FORWARD;
    } else {
        return normalize( _WorldSpaceCameraPos - pt );
    }
}

// scale
inline half3 GetObjectScale(){
    half3x3 m = (half3x3)UNITY_MATRIX_M;
    return half3(
        length( half3( m[0][0], m[1][0], m[2][0] ) ),
        length( half3( m[0][1], m[1][1], m[2][1] ) ),
        length( half3( m[0][2], m[1][2], m[2][2] ) )
    );
}
inline half2 GetObjectScaleXY(){
    half3x3 m = (half3x3)UNITY_MATRIX_M;
    return half2(
        length( half3( m[0][0], m[1][0], m[2][0] ) ),
        length( half3( m[0][1], m[1][1], m[2][1] ) )
    );
}
inline half GetUniformScale( half3 s ){
    return ( s.x + s.y + s.z ) / 3;
}
inline half GetUniformScale( half2 s ){
    return ( s.x + s.y ) / 2;
}
inline half GetUniformScale(){
    return GetUniformScale(GetObjectScale());
}

// line utils
inline void ConvertToPixelThickness( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace, out float pxPerMeter, out float pxWidth ){

    // calculate pixels per meter
	pxPerMeter = CalcPixelsPerMeter( vertOrigin, normal ); // 1 unit in world space
	
	// figure out target width in pixels
	switch( thicknessSpace ){
	    case THICKN_SPACE_METERS: {
	        pxWidth = thickness*pxPerMeter; // this specifically should not have the + extraWidth
	        break;
	    }
	    case THICKN_SPACE_PIXELS: {
	        pxWidth = thickness;
	        break;
	    }
        case THICKN_SPACE_NOOTS: {
            pxWidth = NootsToPixels( thickness );
            break;
        }
        default: {
            pxWidth = 0;
            break;
        }
    }
}

inline half OtherSpaceToMeters( half value, half pxPerMeter, int thicknessSpace ){
    switch( thicknessSpace ){
	    case THICKN_SPACE_METERS:
	        return value;
	    case THICKN_SPACE_PIXELS:
	        return value / pxPerMeter;
        case THICKN_SPACE_NOOTS:
            return NootsToPixels( value ) / pxPerMeter;
        default:
            return 0;
    }
}

inline half MetersToOtherSpace( half meters, half pxPerMeter, int thicknessSpace ){
    switch( thicknessSpace ){
	    case THICKN_SPACE_METERS:
	        return meters;
	    case THICKN_SPACE_PIXELS:
	        return meters * pxPerMeter;
        case THICKN_SPACE_NOOTS:
            return PixelsToNoots( meters * pxPerMeter );
        default:
            return 0;
    }
}


struct LineWidthData{
    half thicknessPixelsTarget; // 1 when thicker than 1 px, px thickness when smaller
    half thicknessMeters; // vertex position thickness. includes LAA padding
    half aaPaddingScale; // multiplier used to correct UVs for LAA padding
    half pxPerMeter; // might be useful idk
};


inline void GetPaddingData( half thicknessPixelsTarget, out half aaPaddingScale, out half pxWidthVert ){
    // for vertex width, we need to clamp at 1px wide to prevent wandering ants and we don't want ants now do we
    pxWidthVert = max( 1, thicknessPixelsTarget+AA_PADDING_PX);
    aaPaddingScale = pxWidthVert / max( VERY_SMOL, thicknessPixelsTarget ); // how much extra we got from the padding, as a multiplier
}


inline LineWidthData GetScreenSpaceWidthData( float3 vertOrigin, half3 normal, half thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
	
	half pxWidthVert;
	GetPaddingData( data.thicknessPixelsTarget, /*out*/ data.aaPaddingScale, /*out*/ pxWidthVert );
	
	// when using pixel size, scale to match pixels
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    
    return data;
}

inline LineWidthData GetScreenSpaceWidthDataSimple( float3 vertOrigin, half3 normal, half thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
    half pxWidthVert = max( 1, data.thicknessPixelsTarget );
    data.aaPaddingScale = 1; 
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    return data;
}

