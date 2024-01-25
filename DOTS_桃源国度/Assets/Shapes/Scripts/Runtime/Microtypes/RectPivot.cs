using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public enum RectPivot {
		Corner,
		Center
	}

	public static class RectPivotExtensions {
		public static Rect GetRect( this RectPivot pivot, Vector2 size ) => pivot.GetRect( size.x, size.y );
		public static Rect GetRect( this RectPivot pivot, float w, float h ) => pivot == RectPivot.Corner ? new Rect( 0, 0, w, h ) : new Rect( -w / 2, -h / 2, w, h );
	}

}