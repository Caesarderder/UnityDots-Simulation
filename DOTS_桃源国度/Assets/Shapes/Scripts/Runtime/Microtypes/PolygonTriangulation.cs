// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>The different triangulation algorithms to use. Some are faster than others, but can only handle specific shapes</summary>
	public enum PolygonTriangulation {
		/// <summary>A fast triangulation algorithm that can only handle convex polygons</summary>
		FastConvexOnly,

		/// <summary>A slower triangulation algorithm that can handle any simple (non-self intersecting) polygon</summary>
		EarClipping
	}

}