using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Polyline shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Polyline" )]
	public class Polyline : ShapeRenderer {

		#if UNITY_EDITOR
		/// <summary>"Please use points instead of PolyPoints - this one is deprecated"</summary>
		[Obsolete( "Please use " + nameof(points) + " instead - this one is deprecated", error: true )]
		public List<PolylinePoint> PolyPoints => points;
		#endif

		/// <summary>IMPORTANT: if you modify this list, you need to set meshOutOfDate to true, otherwise your changes won't apply</summary>
		[FormerlySerializedAs( "polyPoints" )] [SerializeField] public List<PolylinePoint> points = new List<PolylinePoint>() {
			new PolylinePoint( new Vector3( 0, 1, 0 ), Color.white ),
			new PolylinePoint( new Vector3( 0.86602540378f, -.5f, 0 ), Color.white ),
			new PolylinePoint( new Vector3( -0.86602540378f, -.5f, 0 ), Color.white )
		};

		// also called alignment
		[SerializeField] PolylineGeometry geometry = PolylineGeometry.Flat2D;
		/// <summary>Get or set the geometry type to use for this polyline</summary>
		public PolylineGeometry Geometry {
			get => geometry;
			set {
				geometry = value;
				SetIntNow( ShapesMaterialUtils.propAlignment, (int)geometry );
				UpdateMaterial();
				ApplyProperties();
			}
		}

		[SerializeField] PolylineJoins joins = PolylineJoins.Miter;
		/// <summary>The type of joins to use in all corners of this polyline</summary>
		public PolylineJoins Joins {
			get => joins;
			set {
				joins = value;
				meshOutOfDate = true;
				UpdateMaterial();
			}
		}

		[SerializeField] bool closed = true;
		/// <summary>Whether or not this polyline should form a closed loop</summary>
		public bool Closed {
			get => closed;
			set {
				closed = value;
				meshOutOfDate = true;
			}
		}

		[SerializeField] float thickness = 0.125f;
		/// <summary>The thickness of this polyline in the given thickness space</summary>
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = value );
		}

		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		/// <summary>The space in which Thickness is defined</summary>
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}

		/// <summary>The number of points in this polyline</summary>
		public int Count => points.Count;

		/// <summary>Get or set a polyline point by index</summary>
		public PolylinePoint this[ int i ] {
			get => points[i];
			set {
				points[i] = value;
				meshOutOfDate = true;
			}
		}

		/// <summary>Set a polygon point position by index</summary>
		public void SetPointPosition( int index, Vector3 position ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.point = position;
			points[index] = pp;
			meshOutOfDate = true;
		}

		/// <summary>Set a polygon point color by index</summary>
		public void SetPointColor( int index, Color color ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.color = color;
			points[index] = pp;
			meshOutOfDate = true;
		}

		/// <summary>Set a polygon point thickness by index</summary>
		public void SetPointThickness( int index, float thickness ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.thickness = thickness;
			points[index] = pp;
			meshOutOfDate = true;
		}

		/// <summary>Sets all points and their corresponding colors for this polyline</summary>
		public void SetPoints( IReadOnlyCollection<Vector3> points, IReadOnlyCollection<Color> colors = null ) {
			this.points.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		/// <summary>Sets all points and their corresponding colors for this polyline</summary>
		public void SetPoints( IReadOnlyCollection<Vector2> points, IReadOnlyCollection<Color> colors = null ) {
			this.points.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		/// <summary>Sets all points of this polyline</summary>
		public void SetPoints( IEnumerable<PolylinePoint> points ) {
			this.points.Clear();
			AddPoints( points );
		}

		/// <summary>Adds a set of points to this polyline</summary>
		public void AddPoints( IEnumerable<PolylinePoint> points ) {
			this.points.AddRange( points );
			meshOutOfDate = true;
		}

		/// <summary>Adds a point to this polyline</summary>
		public void AddPoint( Vector3 position ) => AddPoint( new PolylinePoint( position ) );

		/// <summary>Adds a point to this polyline</summary>
		public void AddPoint( Vector3 position, Color color ) => AddPoint( new PolylinePoint( position, color ) );

		/// <summary>Adds a point to this polyline</summary>
		public void AddPoint( Vector3 position, Color color, float thickness ) => AddPoint( new PolylinePoint( position, color, thickness ) );

		/// <summary>Adds a point to this polyline</summary>
		public void AddPoint( Vector3 position, float thickness ) => AddPoint( new PolylinePoint( position, Color.white, thickness ) );

		/// <summary>Adds a point to this polyline</summary>
		public void AddPoint( PolylinePoint point ) {
			points.Add( point );
			meshOutOfDate = true;
		}

		private protected override bool UseCamOnPreCull => true;

		internal override void CamOnPreCull() {
			if( meshOutOfDate ) {
				meshOutOfDate = false;
				UpdateMesh( force: true );
			}
		}


		private protected override MeshUpdateMode MeshUpdateMode => MeshUpdateMode.SelfGenerated;
		private protected override void GenerateMesh() => ShapesMeshGen.GenPolylineMesh( Mesh, points, closed, joins, flattenZ: geometry == PolylineGeometry.Flat2D, useColors: true );

		private protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
		}

		private protected override void ShapeClampRanges() => thickness = Mathf.Max( 0f, thickness );

		private protected override Material[] GetMaterials() {
			if( joins.HasJoinMesh() )
				return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode], ShapesMaterialUtils.GetPolylineJoinsMat( joins )[BlendMode] };
			return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode] };
		}

		// todo: this doesn't take point thickness or thickness space into account
		private protected override Bounds GetBounds_Internal() {
			if( points.Count < 2 )
				return default;
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			foreach( Vector3 pt in points.Select( p => p.point ) ) {
				min = Vector3.Min( min, pt );
				max = Vector3.Max( max, pt );
			}

			if( geometry == PolylineGeometry.Flat2D )
				min.z = max.z = 0;

			return new Bounds( ( max + min ) * 0.5f, ( max - min ) + Vector3.one * ( thickness * 0.5f ) );
		}

	}

}