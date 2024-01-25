using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesMath {
		// for a similar bunch of extra math functions,
		// check out Mathfs at https://github.com/FreyaHolmer/Mathfs!

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		public const float TAU = 6.28318530718f;
		[MethodImpl( INLINE )] public static float Frac( float x ) => x - Mathf.Floor( x );
		[MethodImpl( INLINE )] public static float Eerp( float a, float b, float t ) => Mathf.Pow( a, 1 - t ) * Mathf.Pow( b, t );
		[MethodImpl( INLINE )] public static float SmoothCos01( float x ) => Mathf.Cos( x * Mathf.PI ) * -0.5f + 0.5f;
		[MethodImpl( INLINE )] public static Vector2 AngToDir( float angRad ) => new Vector2( Mathf.Cos( angRad ), Mathf.Sin( angRad ) );
		[MethodImpl( INLINE )] public static float DirToAng( Vector2 dir ) => Mathf.Atan2( dir.y, dir.x );
		[MethodImpl( INLINE )] public static Vector2 Rotate90CW( Vector2 v ) => new Vector2( v.y, -v.x );
		[MethodImpl( INLINE )] public static Vector2 Rotate90CCW( Vector2 v ) => new Vector2( -v.y, v.x );
		[MethodImpl( INLINE )] public static Vector4 AtLeast0( Vector4 v ) => new Vector4( Mathf.Max( 0, v.x ), Mathf.Max( 0, v.y ), Mathf.Max( 0, v.z ), Mathf.Max( 0, v.w ) );
		[MethodImpl( INLINE )] public static float MaxComp( Vector4 v ) => Mathf.Max( Mathf.Max( Mathf.Max( v.y, v.x ), v.z ), v.w );
		[MethodImpl( INLINE )] public static bool HasNegativeValues( Vector4 v ) => v.x < 0 || v.y < 0 || v.z < 0 || v.w < 0;
		[MethodImpl( INLINE )] public static float Determinant( Vector2 a, Vector2 b ) => a.x * b.y - a.y * b.x;
		[MethodImpl( INLINE )] public static float Luminance( Color c ) => c.r * 0.2126f + c.g * 0.7152f + c.b * 0.0722f;

		public static float GetLineSegmentProjectionT( Vector3 a, Vector3 b, Vector3 p ) {
			Vector3 disp = b - a;
			return Vector3.Dot( p - a, disp ) / Vector3.Dot( disp, disp );
		}

		[MethodImpl( INLINE )] public static PolylinePoint WeightedSum( Vector4 w, PolylinePoint a, PolylinePoint b, PolylinePoint c, PolylinePoint d ) => w.x * a + w.y * b + w.z * c + w.w * d;
		[MethodImpl( INLINE )] public static Vector3 WeightedSum( Vector4 w, Vector3 a, Vector3 b, Vector3 c, Vector3 d ) => w.x * a + w.y * b + w.z * c + w.w * d;
		[MethodImpl( INLINE )] public static Vector2 WeightedSum( Vector4 w, Vector2 a, Vector2 b, Vector2 c, Vector2 d ) => w.x * a + w.y * b + w.z * c + w.w * d;
		[MethodImpl( INLINE )] public static Color WeightedSum( Vector4 w, Color a, Color b, Color c, Color d ) => w.x * a + w.y * b + w.z * c + w.w * d;

		public static bool PointInsideTriangle( Vector2 a, Vector2 b, Vector2 c, Vector2 point, float aMargin = 0f, float bMargin = 0f, float cMargin = 0f ) {
			float d0 = Determinant( Dir( a, b ), Dir( a, point ) );
			float d1 = Determinant( Dir( b, c ), Dir( b, point ) );
			float d2 = Determinant( Dir( c, a ), Dir( c, point ) );
			bool b0 = d0 < cMargin;
			bool b1 = d1 < aMargin;
			bool b2 = d2 < bMargin;
			return b0 == b1 && b1 == b2; // on the same side of all halfspaces, this can only happen inside
		}

		/// <summary>Direction from a to b, skipping intermediate Vector2s for speed</summary>
		[MethodImpl( INLINE )] internal static Vector2 Dir( Vector2 a, Vector2 b ) {
			float dx = b.x - a.x;
			float dy = b.y - a.y;
			float mag = Mathf.Sqrt( dx * dx + dy * dy );
			return new Vector2( dx / mag, dy / mag );
		}

		public static float PolygonSignedArea( List<Vector2> pts ) {
			int count = pts.Count;
			float sum = 0f;
			for( int i = 0; i < count; i++ ) {
				Vector2 a = pts[i];
				Vector2 b = pts[( i + 1 ) % count];
				sum += ( b.x - a.x ) * ( b.y + a.y );
			}

			return sum;
		}

		public static Vector2 Rotate( Vector2 v, float angRad ) {
			float ca = Mathf.Cos( angRad );
			float sa = Mathf.Sin( angRad );
			return new Vector2( ca * v.x - sa * v.y, sa * v.x + ca * v.y );
		}

		static float DeltaAngleRad( float a, float b ) => Mathf.Repeat( b - a + Mathf.PI, TAU ) - Mathf.PI;

		public static float InverseLerpAngleRad( float a, float b, float v ) {
			float angBetween = DeltaAngleRad( a, b );
			b = a + angBetween; // removes any a->b discontinuity
			float h = a + angBetween * 0.5f; // halfway angle
			v = h + DeltaAngleRad( h, v ); // get offset from h, and offset by h
			return Mathf.InverseLerp( a, b, v );
		}


		[MethodImpl( INLINE )] static Vector2 Lerp( Vector2 a, Vector2 b, Vector2 t ) => new Vector2( Mathf.Lerp( a.x, b.x, t.x ), Mathf.Lerp( a.y, b.y, t.y ) );
		[MethodImpl( INLINE )] public static Vector2 Lerp( Rect r, Vector2 t ) => new Vector2( Mathf.Lerp( r.xMin, r.xMax, t.x ), Mathf.Lerp( r.yMin, r.yMax, t.y ) );
		[MethodImpl( INLINE )] static Vector2 InverseLerp( Vector2 a, Vector2 b, Vector2 v ) => ( v - a ) / ( b - a );
		[MethodImpl( INLINE )] public static Vector2 InverseLerp( Rect r, Vector2 pt ) => new Vector2( Mathf.InverseLerp( r.xMin, r.xMax, pt.x ), Mathf.InverseLerp( r.yMin, r.yMax, pt.y ) );
		[MethodImpl( INLINE )] static Vector2 Remap( Vector2 iMin, Vector2 iMax, Vector2 oMin, Vector2 oMax, Vector2 value ) => Lerp( oMin, oMax, InverseLerp( iMin, iMax, value ) );
		[MethodImpl( INLINE )] public static Vector2 Remap( Rect iRect, Rect oRect, Vector2 iPos ) => Remap( iRect.min, iRect.max, oRect.min, oRect.max, iPos );

		public static Vector3 Abs( Vector3 v ) => new Vector3( Mathf.Abs( v.x ), Mathf.Abs( v.y ), Mathf.Abs( v.z ) );

		// source: https://answers.unity.com/questions/421968/normal-distribution-random.html
		public static float RandomGaussian( float min = 0.0f, float max = 1.0f ) {
			float u;
			float s;
			do {
				u = 2f * Random.value - 1f;
				float v = 2f * Random.value - 1f;
				s = u * u + v * v;
			} while( s >= 1f );

			float std = u * Mathf.Sqrt( -2.0f * Mathf.Log( s ) / s );
			float mean = ( min + max ) / 2.0f;
			float sigma = ( max - mean ) / 3.0f;
			return Mathf.Clamp( std * sigma + mean, min, max );
		}

		public static Vector3 GetRandomPerpendicularVector( Vector3 a ) {
			Vector3 b;
			do b = Random.onUnitSphere;
			while( Mathf.Abs( Vector3.Dot( a, b ) ) > 0.98f );

			return b;
		}

		// arc utils
		public static IEnumerable<PolylinePoint> GetArcPoints( PolylinePoint a, PolylinePoint b, Vector3 normA, Vector3 normB, Vector3 center, float radius, int count ) {
			count = Mathf.Max( 2, count ); // at least 2

			PolylinePoint DirToPt( Vector3 dir, float t ) {
				PolylinePoint p = t <= 0 ? a : ( t >= 1 ? b : PolylinePoint.Lerp( a, b, t ) );
				p.point = center + dir * radius;
				return p;
			}

			yield return DirToPt( normA, 0 );
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return DirToPt( Vector3.Slerp( normA, normB, t ), t );
			}

			yield return DirToPt( normB, 1 );
		}

		public static IEnumerable<Vector3> GetArcPoints( Vector3 normA, Vector3 normB, Vector3 center, float radius, int count ) {
			count = Mathf.Max( 2, count ); // at least 2
			Vector3 DirToPt( Vector3 dir ) => center + dir * radius;
			yield return DirToPt( normA );
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return DirToPt( Vector3.Slerp( normA, normB, t ) );
			}

			yield return DirToPt( normB );
		}

		public static IEnumerable<Vector2> GetArcPoints( Vector2 normA, Vector2 normB, Vector2 center, float radius, int count ) {
			count = Mathf.Max( 2, count ); // at least 2
			Vector2 DirToPt( Vector2 dir ) => center + dir * radius;
			yield return DirToPt( normA );
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return DirToPt( Vector3.Slerp( normA, normB, t ) ); // todo: vec2 slerp?
			}

			yield return DirToPt( normB );
		}

		// bezier utils
		public static IEnumerable<PolylinePoint> CubicBezierPointsSkipFirst( PolylinePoint a, PolylinePoint b, PolylinePoint c, PolylinePoint d, int count ) {
			// skip first point
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return CubicBezier( a, b, c, d, t );
			}

			yield return d;
		}

		public static IEnumerable<PolylinePoint> CubicBezierPointsSkipFirstMatchStyle( PolylinePoint style, Vector3 a, Vector3 b, Vector3 c, Vector3 d, int count ) {
			// skip first point
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				PolylinePoint pp = style;
				pp.point = CubicBezier( a, b, c, d, t );
				yield return pp;
			}

			PolylinePoint ppEnd = style;
			ppEnd.point = d;
			yield return ppEnd;
		}

		public static IEnumerable<Vector3> CubicBezierPointsSkipFirst( Vector3 a, Vector3 b, Vector3 c, Vector3 d, int count ) {
			// skip first point
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return CubicBezier( a, b, c, d, t );
			}

			yield return d;
		}

		public static IEnumerable<Vector2> CubicBezierPointsSkipFirst( Vector2 a, Vector2 b, Vector2 c, Vector2 d, int count ) {
			// skip first point
			for( int i = 1; i < count - 1; i++ ) {
				float t = i / ( count - 1f );
				yield return CubicBezier( a, b, c, d, t );
			}

			yield return d;
		}

		public static Vector4 GetCubicBezierWeights( float t ) {
			float omt = 1f - t;
			float omt2 = omt * omt;
			float t2 = t * t;
			return new Vector4(
				omt2 * omt, // (1-t)³
				3f * omt2 * t, // 3(1-t)²t
				3f * omt * t2, // 3(1-t)t²
				t2 * t // t³
			);
		}

		public static PolylinePoint CubicBezier( PolylinePoint a, PolylinePoint b, PolylinePoint c, PolylinePoint d, float t ) {
			if( t <= 0f ) return a;
			if( t >= 1f ) return d;
			return WeightedSum( GetCubicBezierWeights( t ), a, b, c, d );
		}

		public static Vector3 CubicBezier( Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t ) {
			if( t <= 0f ) return a;
			if( t >= 1f ) return d;
			return WeightedSum( GetCubicBezierWeights( t ), a, b, c, d );
		}

		public static Vector2 CubicBezier( Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t ) {
			if( t <= 0f ) return a;
			if( t >= 1f ) return d;
			return WeightedSum( GetCubicBezierWeights( t ), a, b, c, d );
		}

		public static Vector3 CubicBezierDerivative( Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t ) {
			float omt = 1f - t;
			float omt2 = omt * omt;
			float t2 = t * t;
			return
				a * ( -3 * omt2 ) +
				b * ( 9 * t2 - 12 * t + 3 ) +
				c * ( 6 * t - 9 * t2 ) +
				d * ( 3 * t2 );
		}

		public static float GetApproximateCurveSum( Vector3 a, Vector3 b, Vector3 c, Vector3 d, int vertCount ) {
			Vector2[] tangents = new Vector2[vertCount];
			for( int i = 0; i < vertCount; i++ ) {
				float t = i / ( vertCount - 1f );
				tangents[i] = CubicBezierDerivative( a, b, c, d, t );
			}

			float angSum = 0f;
			for( int i = 0; i < vertCount - 1; i++ )
				angSum += Vector2.Angle( tangents[i], tangents[i + 1] );

			return angSum;
		}


	}

}