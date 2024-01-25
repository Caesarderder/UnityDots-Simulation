// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Type of geometry positioning to use for polylines</summary>
	public enum PolylineGeometry {
		/// <summary>Flat against the local space XY plane</summary>
		Flat2D,

		/// <summary>Billboarded and aligning to the camera in 3D space, useful to fake 3D lines</summary>
		Billboard
	}

}