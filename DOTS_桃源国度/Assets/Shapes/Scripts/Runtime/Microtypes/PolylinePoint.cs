using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[Serializable]
	public struct PolylinePoint {
		/// <summary>Position of this point</summary>
		public Vector3 point;

		/// <summary>The color tint of this point</summary>
		[ShapesColorField( true )] public Color color;

		/// <summary>The thickness multiplier for this point</summary>
		public float thickness;

		public static PolylinePoint operator +( PolylinePoint a, PolylinePoint b ) => new PolylinePoint( a.point + b.point, a.color + b.color, a.thickness + b.thickness );
		public static PolylinePoint operator *( PolylinePoint a, float b ) => new PolylinePoint( a.point * b, a.color * b, a.thickness * b );
		public static PolylinePoint operator *( float b, PolylinePoint a ) => a * b;

		public static PolylinePoint Lerp( PolylinePoint a, PolylinePoint b, float t ) =>
			new PolylinePoint {
				point = Vector3.LerpUnclamped( a.point, b.point, t ),
				color = Color.LerpUnclamped( a.color, b.color, t ),
				thickness = Mathf.LerpUnclamped( a.thickness, b.thickness, t )
			};


		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		public PolylinePoint( Vector3 point ) {
			this.point = point;
			this.color = Color.white;
			this.thickness = 1;
		}

		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		public PolylinePoint( Vector2 point ) {
			this.point = point;
			this.color = Color.white;
			this.thickness = 1;
		}

		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		/// <param name="color">The color of this point</param>
		public PolylinePoint( Vector3 point, Color color ) {
			this.point = point;
			this.color = color;
			this.thickness = 1;
		}

		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		/// <param name="color">The color of this point</param>
		public PolylinePoint( Vector2 point, Color color ) {
			this.point = point;
			this.color = color;
			this.thickness = 1;
		}

		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		/// <param name="color">The color tint of this point</param>
		/// <param name="thickness">The thickness multiplier of this point</param>
		public PolylinePoint( Vector3 point, Color color, float thickness ) {
			this.point = point;
			this.color = color;
			this.thickness = thickness;
		}

		/// <summary>Creates a polyline point</summary>
		/// <param name="point">The position of this point</param>
		/// <param name="color">The color tint of this point</param>
		/// <param name="thickness">The thickness multiplier of this point</param>
		public PolylinePoint( Vector2 point, Color color, float thickness ) {
			this.point = point;
			this.color = color;
			this.thickness = thickness;
		}

	}

}