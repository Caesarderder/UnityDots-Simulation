// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Various spaces to define dash lengths in</summary>
	public enum DashSpace {
		/// <summary>Allows you to set the number of dashes along the shape, rather than the size of individual dashes and spaces</summary>
		FixedCount = -2,

		/// <summary>Dash and space lengths are relative to the width, where 1 unit = width of the shape</summary>
		Relative = -1,

		/// <summary>Dash and space lengths are in meters</summary>
		Meters = 0 // this enum matches the thickness space enum, just, in case I add support for px/noot sizing here in the future
	}

	public static class DashExtensions {
		public static int GetIndex( this DashSpace noot ) => (int)noot + 2;
	}

}