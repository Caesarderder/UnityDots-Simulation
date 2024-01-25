// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	/// <summary>Scale modes for shapes</summary>
	public enum ScaleMode {
		/// <summary>Uniform is traditional scaling, where scaling an object affects everything</summary>
		Uniform,

		/// <summary>Coordinate scaling affects only the major features, while thickness values and small scale features remain unaffected</summary>
		Coordinate
	}

}