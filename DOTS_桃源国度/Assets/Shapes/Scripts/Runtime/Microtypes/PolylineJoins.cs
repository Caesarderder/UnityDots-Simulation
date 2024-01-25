using System;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>Various corner join types for polylines</summary>
	public enum PolylineJoins {
		/// <summary>Very cheap joins, useful when having many points in smooth curves</summary>
		Simple,

		/// <summary>Miter joins look the most natural for hard corners, but looks weird when you have very sharp corners</summary>
		Miter,

		/// <summary>Soft and cute corners~</summary>
		Round,

		/// <summary>Like miter joins but cut off for some reason</summary>
		Bevel
	}

	internal static class PolylineJoinsExtensions {

		public static bool HasJoinMesh( this PolylineJoins join ) {
			switch( join ) {
				case PolylineJoins.Simple: return false;
				case PolylineJoins.Miter:  return false;
				case PolylineJoins.Round:  return true;
				case PolylineJoins.Bevel:  return true;
				default:                   throw new ArgumentOutOfRangeException( nameof(join), join, null );
			}
		}

		public static bool HasSimpleJoin( this PolylineJoins join ) {
			switch( join ) {
				case PolylineJoins.Simple: return false;
				case PolylineJoins.Miter:  return false;
				case PolylineJoins.Round:  return false;
				case PolylineJoins.Bevel:  return true;
				default:                   throw new ArgumentOutOfRangeException( nameof(join), join, null );
			}
		}

	}

}