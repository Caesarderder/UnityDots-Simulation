// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

#ifndef SHAPES_INCLUDED_MATH // just to make compilers happy
#include "Shapes Math.cginc"
#endif

#define FILL_TYPE_NONE -1
#define FILL_TYPE_LINEAR 0
#define FILL_TYPE_RADIAL 1

#define FILL_SPACE_LOCAL 0
#define FILL_SPACE_WORLD 1

#define SHAPES_INTERPOLATOR_FILL(i) float3 fillCoords : TEXCOORD##i;


float3 GetFillCoords( float3 localPos ){

    int fillType = PROP( _FillType );
    int fillSpace = PROP( _FillSpace );
    float4 start = PROP( _FillStart ); 
    float3 end = PROP( _FillEnd );

    if( fillType == FILL_TYPE_NONE )
        return float3(0,0,0);
    
    // need coords
    float3 absoluteCoord = fillSpace == FILL_SPACE_LOCAL ? localPos : LocalToWorldPos( localPos );
    float3 relativeCoord = absoluteCoord - start.xyz;

    // radial has to send full coordinates
    if( fillType == FILL_TYPE_RADIAL )
        return relativeCoord;
    
    // linear needs only the interpolator
    // if( fillType == FILL_TYPE_LINEAR )
    half3 gradVec = end - start.xyz;
    half t = dot(gradVec, relativeCoord ) / dot(gradVec, gradVec);
    return float3( t, 0, 0 );
}

half GetFillGradientT( float3 coords, int fillType, float4 start ){
    switch( fillType ){
        case FILL_TYPE_LINEAR: {
            return saturate(coords.x); // interpolation is done in the vertex shader so shrug~
        }
        case FILL_TYPE_RADIAL: {
            half radius = start.w;
            return saturate( length( coords ) / radius ); // start.w = radius
        }
        default: { return 0; }
    }
}

half4 GetFillColor( float3 fillCoords ){
    int fillType = PROP( _FillType );
    half4 color = PROP( _Color );
    if( fillType == FILL_TYPE_NONE )
        return color;
    half t = GetFillGradientT( fillCoords, fillType, PROP(_FillStart) );
    return lerp( color, PROP(_ColorEnd), t );
}
