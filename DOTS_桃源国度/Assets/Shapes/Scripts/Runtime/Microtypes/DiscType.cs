// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	public enum DiscType {
		Disc,
		Pie,
		Ring,
		Arc
	}

	internal static class DiscTypeExtensions {
		public static bool HasThickness( this DiscType type ) => type == DiscType.Ring || type == DiscType.Arc;
		public static bool HasSector( this DiscType type ) => type == DiscType.Pie || type == DiscType.Arc;
	}

}