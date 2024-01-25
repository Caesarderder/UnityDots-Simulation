// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Geometry types for Line shapes</summary>
	public enum LineGeometry {
		/// <summary>Flat 2D lines on the XY plane in local space</summary>		
		Flat2D,
		/// <summary>Flat Billboarded lines, aligning with the camera in 3D</summary>
		Billboard,
		/// <summary>Polygonal 3D lines</summary>
		Volumetric3D
	}

}