using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class PolylinePath : PointPath<PolylinePoint> {

		const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

		bool lastUsedClosed = false;
		PolylineJoins lastUsedJoins = PolylineJoins.Miter;

		public PolylinePath() => _ = 0;

		#region accessors and setters by index

		public void SetPoint( int index, Vector3 point ) {
			PolylinePoint p = path[index];
			p.point = point;
			SetPoint( index, p );
		}

		public void SetPoint( int index, Vector2 point ) {
			PolylinePoint p = path[index];
			p.point = point;
			SetPoint( index, p );
		}

		public void SetColor( int index, Color color ) {
			PolylinePoint p = path[index];
			p.color = color;
			SetPoint( index, p );
		}

		#endregion

		#region point adding

		[MethodImpl( INLINE )] public void AddPoint( float x, float y ) => AddPoint( new PolylinePoint( new Vector3( x, y, 0f ), Color.white ) );
		[MethodImpl( INLINE )] public void AddPoint( float x, float y, float z ) => AddPoint( new PolylinePoint( new Vector3( x, y, z ), Color.white ) );
		[MethodImpl( INLINE )] public void AddPoint( float x, float y, Color color ) => AddPoint( new PolylinePoint( new Vector3( x, y, 0f ), color ) );
		[MethodImpl( INLINE )] public void AddPoint( float x, float y, float z, Color color ) => AddPoint( new PolylinePoint( new Vector3( x, y, z ), color ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector3 pos ) => AddPoint( new PolylinePoint( pos, Color.white ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector3 pos, Color color ) => AddPoint( new PolylinePoint( pos, color ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector3 pos, float thickness ) => AddPoint( new PolylinePoint( pos, Color.white, thickness ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector3 pos, float thickness, Color color ) => AddPoint( new PolylinePoint( pos, color, thickness ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector2 pos ) => AddPoint( new PolylinePoint( pos, Color.white ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector2 pos, Color color ) => AddPoint( new PolylinePoint( pos, color ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector2 pos, float thickness ) => AddPoint( new PolylinePoint( pos, Color.white, thickness ) );
		[MethodImpl( INLINE )] public void AddPoint( Vector2 pos, float thickness, Color color ) => AddPoint( new PolylinePoint( pos, color, thickness ) );

		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector3> pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		[MethodImpl( INLINE )] public void AddPoints( params Vector3[] pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector2> pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		[MethodImpl( INLINE )] public void AddPoints( params Vector2[] pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector3> pts, Color color ) => AddPoints( pts.Select( point => new PolylinePoint( point, color ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector2> pts, Color color ) => AddPoints( pts.Select( point => new PolylinePoint( point, color ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<float> thicknesses ) => AddPoints( pts.Zip( thicknesses, ( p, t ) => new PolylinePoint( p, Color.white, t ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<float> thicknesses ) => AddPoints( pts.Zip( thicknesses, ( p, t ) => new PolylinePoint( p, Color.white, t ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<float> thicknesses, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, thicknesses, ( p, c, t ) => new PolylinePoint( p, c, t ) ) );
		[MethodImpl( INLINE )] public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<float> thicknesses, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, thicknesses, ( p, c, t ) => new PolylinePoint( p, c, t ) ) );

		#endregion

		#region BezierTo, ArcTo

		// VECTOR INPUTS ONLY:
		/// <summary>A cubic bezier curve, using the previous point as the starting point</summary>
		[MethodImpl( INLINE )] public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end ) => BezierTo( startTangent, endTangent, end, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		/// <summary>A cubic bezier curve, using the previous point as the starting point. Number of points is given by density in number of points per full 360° turn</summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, float pointsPerTurn ) {
			if( CheckCanAddContinuePoint() ) return;
			int pointCount = CalcBezierPointCount( LastPoint.point, startTangent, endTangent, end, pointsPerTurn );
			BezierTo( startTangent, endTangent, end, pointCount );
		}

		/// <summary> Adds points of a cubic bezier curve, using the previous point as the starting point</summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, int pointCount ) {
			if( CheckCanAddContinuePoint() ) return;
			AddPoints( ShapesMath.CubicBezierPointsSkipFirstMatchStyle( LastPoint, LastPoint.point, startTangent, endTangent, end, pointCount ) );
		}

		// POLYLINEPOINT ENDPOINTS:
		/// <summary>A cubic bezier curve, using the previous point as the starting point. Color and thickness etc. will blend toward the end point values</summary>
		[MethodImpl( INLINE )] public void BezierTo( Vector3 startTangent, Vector3 endTangent, PolylinePoint end ) => BezierTo( startTangent, endTangent, end, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		/// <summary>A cubic bezier curve, using the previous point as the starting point. Number of points is given by density in number of points per full 360° turn. Color and thickness etc. will blend toward the end point values</summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, PolylinePoint end, float pointsPerTurn ) {
			if( CheckCanAddContinuePoint() ) return;
			int pointCount = CalcBezierPointCount( LastPoint.point, startTangent, endTangent, end.point, pointsPerTurn );
			BezierTo( startTangent, endTangent, end, pointCount );
		}

		/// <summary> Adds points of a cubic bezier curve, using the previous point as the starting point</summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, PolylinePoint end, int pointCount ) {
			if( CheckCanAddContinuePoint() ) return;
			PolylinePoint ppB = PolylinePoint.Lerp( LastPoint, end, 1f / 3f ); // blend all other properties, assume thirds
			ppB.point = startTangent;
			PolylinePoint ppC = PolylinePoint.Lerp( LastPoint, end, 2f / 3f );
			ppC.point = endTangent;
			BezierTo( ppB, ppC, end, pointCount );
		}

		// POLYLINEPOINT INPUT ONLY:
		/// <summary>A cubic bezier curve, using the previous point as the starting point. Color and thickness etc. will blend across the point values</summary>
		[MethodImpl( INLINE )] public void BezierTo( PolylinePoint startTangent, PolylinePoint endTangent, PolylinePoint end ) => BezierTo( startTangent, endTangent, end, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		/// <summary>A cubic bezier curve, using the previous point as the starting point. Number of points is given by density in number of points per full 360° turn. Color and thickness etc. will blend across the point values</summary>
		public void BezierTo( PolylinePoint startTangent, PolylinePoint endTangent, PolylinePoint end, float pointsPerTurn ) {
			if( CheckCanAddContinuePoint() ) return;
			int pointCount = CalcBezierPointCount( LastPoint.point, startTangent.point, endTangent.point, end.point, pointsPerTurn );
			BezierTo( startTangent, endTangent, end, pointCount );
		}

		/// <summary> Adds points of a cubic bezier curve, using the previous point as the starting point. Color and thickness etc. will blend across the point values</summary>
		public void BezierTo( PolylinePoint startTangent, PolylinePoint endTangent, PolylinePoint end, int pointCount ) {
			if( CheckCanAddContinuePoint() ) return;
			AddPoints( ShapesMath.CubicBezierPointsSkipFirst( LastPoint, startTangent, endTangent, end, pointCount ) );
		}

		static int CalcBezierPointCount( Vector3 a, Vector3 b, Vector3 c, Vector3 d, float pointsPerTurn ) {
			int sampleCount = ShapesConfig.Instance.polylineBezierAngularSumAccuracy * 2 + 1;
			float curveSumDeg = ShapesMath.GetApproximateCurveSum( a, b, c, d, sampleCount );
			float angSpanTurns = curveSumDeg / 360f;
			return Mathf.Max( 2, Mathf.RoundToInt( angSpanTurns * pointsPerTurn ) );
		}

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point count</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, Vector3 next, float radius, int pointCount ) => AddArcPoints( corner, next, radius, useDensity: false, pointCount, 0 );

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point count. Color and thickness etc. will blend from start to end</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, PolylinePoint next, float radius, int pointCount ) => AddArcPoints( corner, next, radius, useDensity: false, pointCount, 0 );

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, Vector3 next, float radius ) => AddArcPoints( corner, next, radius, useDensity: true, 0, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next. Color and thickness etc. will blend from start to end</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, PolylinePoint next, float radius ) => AddArcPoints( corner, next, radius, useDensity: true, 0, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point density in number of points per full 360° turn</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, Vector3 next, float radius, float pointsPerTurn ) => AddArcPoints( corner, next, radius, useDensity: true, 0, pointsPerTurn );

		/// <summary>Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point density in number of points per full 360° turn. Color and thickness etc. will blend from start to end</summary>
		[MethodImpl( INLINE )] public void ArcTo( Vector3 corner, PolylinePoint next, float radius, float pointsPerTurn ) => AddArcPoints( corner, next, radius, useDensity: true, 0, pointsPerTurn );

		void AddArcPoints( Vector3 corner, Vector3 next, float radius, bool useDensity, int targetPointCount, float pointsPerTurn ) {
			if( CheckCanAddContinuePoint() ) return;
			PolylinePoint ppNext = LastPoint;
			ppNext.point = next;
			AddArcPoints( corner, ppNext, radius, useDensity, targetPointCount, pointsPerTurn );
		}

		void AddArcPoints( Vector3 corner, PolylinePoint next, float radius, bool useDensity, int targetPointCount, float pointsPerTurn ) {
			if( CheckCanAddContinuePoint() ) return;

			PolylinePoint prev = LastPoint;

			Vector3 tangentA = ( corner - prev.point ).normalized;
			Vector3 tangentB = ( next.point - corner ).normalized;
			Vector3 cross = Vector3.Cross( tangentA, tangentB );

			if( cross.TaxicabMagnitude() <= 0.001f ) {
				// this means it's a straight line, a few things happen:
				// 1. there's a sharp color change where the corner would project onto the line from start to end
				// 2. we need to include two points there to have continuity with how the colors are applied
				float tCenter = ShapesMath.GetLineSegmentProjectionT( prev.point, next.point, corner );
				float tA = Mathf.Clamp01( tCenter - 0.0001f ); // nudge to make sharp discontinuity
				float tB = Mathf.Clamp01( tCenter + 0.0001f ); // nudge to make sharp discontinuity
				PolylinePoint ppA = prev;
				PolylinePoint ppB = next;
				ppA.point = Vector3.Lerp( prev.point, next.point, tA );
				ppB.point = Vector3.Lerp( prev.point, next.point, tB );
				AddPoint( ppA );
				AddPoint( ppB );
				return; // pretty much just a straight line. only add the corner point
			}

			Vector3 axis = cross.normalized;
			Vector3 normA = Vector3.Cross( axis, tangentA ); // normalized
			Vector3 normB = Vector3.Cross( axis, tangentB );
			Vector3 cornerDir = ( normA + normB ).normalized;
			float cornerBDot = Vector3.Dot( cornerDir, normB );
			radius = Mathf.Max( radius, 0.0001f ); // make sure radius isn't degenerate
			Vector3 center = corner + cornerDir * ( ( radius / cornerBDot ) );
			// calc count here if density based
			if( useDensity ) {
				float angTurn = Vector3.Angle( normA, normB ) / 360f;
				targetPointCount = Mathf.RoundToInt( angTurn * pointsPerTurn );
			}

			AddPoints( ShapesMath.GetArcPoints( prev, next, -normA, -normB, center, radius, targetPointCount ) );
		}

		#endregion

		public bool EnsureMeshIsReadyToRender( bool closed, PolylineJoins renderJoins, out Mesh outMesh ) {
			if( meshDirty == false ) {
				// polyline itself didn't change, but the render state might force us to update
				if( renderJoins != lastUsedJoins || closed != lastUsedClosed )
					meshDirty = true;
			}

			return base.EnsureMeshIsReadyToRender( out outMesh, () => { TryUpdateMesh( closed, renderJoins ); } );
		}

		void TryUpdateMesh( bool closed, PolylineJoins joins ) {
			lastUsedClosed = closed;
			lastUsedJoins = joins;
			// todo: be smarter about this, maybe don't mesh.clear but check point count and whatnot
			ShapesMeshGen.GenPolylineMesh( base.mesh, path, closed, joins, flattenZ: false, useColors: true );
		}


		#region Obsolete

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void ArcTo( Vector3 corner, Vector3 next, float radius, int pointCount, Color color ) => AddArcPoints( corner, next, radius, useDensity: false, pointCount, 0 );

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void ArcTo( Vector3 corner, Vector3 next, float radius, Color color ) => AddArcPoints( corner, next, radius, useDensity: true, 0, ShapesConfig.Instance.polylineDefaultPointsPerTurn );

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void ArcTo( Vector3 corner, Vector3 next, float radius, float pointsPerTurn, Color color ) => AddArcPoints( corner, next, radius, useDensity: true, 0, pointsPerTurn );

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, float pointsPerTurn, Color color ) => _ = 0;

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, int pointCount, Color color ) => _ = 0;

		[System.Obsolete( "This function no longer exists - either use the overload without a color, where the color will match the previous point, or the one with a PolylinePoint endpoint, where the color will blend between previous point and the target point", true )]
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, Color color ) => _ = 0;

		#endregion


	}


}