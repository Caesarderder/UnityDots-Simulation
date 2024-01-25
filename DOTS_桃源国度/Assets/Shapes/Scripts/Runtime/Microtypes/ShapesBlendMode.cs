// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System.ComponentModel;

namespace Shapes {

	/// <summary>Various types of blending modes, changing how shapes render on top of the existing colors</summary>
	public enum ShapesBlendMode {
		/// <summary>No partial transparency support, but sorts and blends correctly as it writes to the depth buffer, unlike all other blending modes.
		/// Note that when MSAA is enabled you get low-resolution fading by means of alpha-to-coverage blending
		/// (though this will only work in the game view with MSAA on)</summary>
		[Description( "Opaque" )] Opaque = 0,

		/// <summary>The classic transparent/alpha-blended mode, where the alpha channel controls opacity. Note that this mode is order dependent, and can't sort correctly when intersecting other transparent shapes</summary>
		[Description( "Transparent_" )] Transparent = 1,

		/// <summary>Adds color linearly, good for glowing/brightening effects against dark backgrounds</summary>
		[Description( "Linear Dodge (Additive)" )] Additive = 2,

		/// <summary>A harsher non-linear version of additive that goes even brighter when blending with bright colors.
		/// Has somewhat unintuitive behaviors when blending with pure black</summary>
		[Description( "Color Dodge" )] ColorDodge = 9,

		/// <summary>Softly brightens towards white.
		/// Also called "soft additive", this is the opposite operation of multiply,
		/// softly blending toward white instead of black
		/// Lighten outputs the lightest of the shape and its background</summary>
		[Description( "Screen" )] Screen = 4,

		/// <summary>Outputs the lightest color per-channel of the shape and its background</summary>
		[Description( "Lighten_" )] Lighten = 7,

		/// <summary>Darkens linearly. This is the opposite operation of additive.
		/// Unlike multiply, linear burn has a tendency to introduce hue shifting</summary>
		[Description( "Linear Burn" )] LinearBurn = 6,

		/// <summary>A harsher non-linear version of linear burn that goes even darker when blending with dark colors.
		/// Has somewhat unintuitive behaviors when blending with pure white</summary>
		[Description( "Color Burn" )] ColorBurn = 10,

		/// <summary>Is good for tinting/darkening effects against bright backgrounds. It will softly darken toward black, which is the opposite of screen blending which softly brighens to white</summary>
		[Description( "Multiply" )] Multiplicative = 3,

		/// <summary>Outputs the darkest color per-channel of the shape and its background</summary>
		[Description( "Darken_" )] Darken = 8,

		/// <summary>Subtracts the shape color from the background. It's like linear burn but the colors are inverted and it's annoying to work with</summary>
		[Description( "Subtract" )] Subtractive = 5,
	}

}