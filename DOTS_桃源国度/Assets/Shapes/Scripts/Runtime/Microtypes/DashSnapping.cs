// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Snap modes for dashed shapes</summary>
	public enum DashSnapping {
		/// <summary>No snapping</summary>
		Off,
		/// <summary>This will snap so that the dash pattern will tile along the shape</summary>
		Tiling,
		/// <summary>This will snap so that there's a solid dash on each end</summary>
		EndToEnd
	}

}