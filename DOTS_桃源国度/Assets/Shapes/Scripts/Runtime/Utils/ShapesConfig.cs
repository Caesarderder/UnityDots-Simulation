using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {


	[CreateAssetMenu]
	public class ShapesConfig : ScriptableObject {

		public enum FragOutputPrecision {
			fixed4,
			half4,
			float4
		}

		public enum LocalAAQuality {
			Off,
			Medium,
			High
		}

		public enum QuadInterpolationQuality {
			Low,
			Medium,
			High2D,
			High
		}

		static ShapesConfig inst;
		public static ShapesConfig Instance {
			get {
				if( inst == null )
					inst = Resources.Load<ShapesConfig>( "Shapes Config" );
				return inst;
			}
		}

		[Tooltip( "Whether or not to use HDR color pickers throughout Shapes (This does not affect performance in any way)" )]
		public bool useHdrColorPickers = false;

		[Tooltip( "Whether or not to auto-detect and set up render pipeline" )]
		public bool autoConfigureRenderPipeline = true;

		[Tooltip( "GPU Instancing in immediate mode drawing means if you render lots of similar shapes consecutively, they will get batched into a single draw call. " +
				  "Generally you'll want this on, but there may be cases where the CPU and memory overhead of instancing isn't worth it, " +
				  "which might be the case if you never draw shapes of the same type consecutively" )]
		public bool useImmediateModeInstancing = true;

		[Tooltip( "Default point density for polyline arcs and beziers in points per full turn\n" +
				  "If set to 128, then it'll use 64 points for a 180° turn, 32 points for a 90° turn\n\n" +
				  "16 = curves are very jagged, clearly just a bunch of straight lines in a trenchcoat, except they forgot the trenchcoat\n" +
				  "32 = curves visibly have straight segments when looking close, but appear smooth at a quick glance. (trenchcoat is now on)\n" +
				  "64 = curves generally appear smooth, except at the very sharpest of turns. recommended value.\n" +
				  "128 = curves appear smooth in pretty much all cases, beyond this is pretty wild, but I mean, if you're a wild person then go for it\n" )]
		public float polylineDefaultPointsPerTurn = 64;

		[Tooltip( "Default accuracy when calculating point density of bezier curves.\n" +
				  "This is only used for bezier curves where you specify density rather than point count.\n" +
				  "If you have mostly very simple bezier curves, you can leave this at 3.\n" +
				  "If you have more complex curves, like those with widely separated control points squishing the curve,\n" +
				  "then you should use at least 5 samples\n" +
				  "\n" +
				  "1 = ~12% margin of error. this is the minimum value! works for the simplest curves, but generally inaccurate\n" +
				  "2 = ~4% margin of error. this is recommended, good balance between accuracy and speed\n" +
				  "3 = ~2% margin of error\n" +
				  "4 = ~1% margin of error" )]
		public int polylineBezierAngularSumAccuracy = 2;

		[Tooltip( "If this is on, static properties set inside of Draw.Command will apply only within that draw command. " +
				  "This is usually more intuitive and convenient, but it does come with a slight processing overhead, " +
				  "so if you are running something very performance sensitive you might want to turn this off" )]
		public bool pushPopStateInDrawCommands = true;

		public const string TOOLTIP_BOUNDS = "These settings are uh, very esoteric\n" +
											 "*if* you are having trouble with *many* shapes being drawn on screen at the same time,\n" +
											 "making the bounds smaller using this parameter might help you optimize your game\n" +
											 "\n" +
											 "This is like, super technical, so please read every word very carefully below:\n" +
											 "This value should be set so that *all* shapes using, for instance, the quad mesh (disc, line, rect, etc.),\n" +
											 "can use *these specific bounds*, so that the bounds would encapsulate the entire shape.\n" +
											 "Practically, this means that these bounds should be set so that it can encapsulate the largest\n" +
											 "shape you have in your project. If this is set too low, larger shapes will pop in/out of existence\n" +
											 "\n" +
											 "The purpose of this is to gain some benefit in culling, but still keep the benefits of instancing.\n" +
											 "By default, size is set to a large value of 1 << 16 (65536), practically \"turning off\" frustum culling";

		const float VERY_LORGE_BOUNDS = 1 << 16;

		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeQuad = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeTriangle = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeSphere = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeTorus = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeCuboid = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeCone = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeCylinder = VERY_LORGE_BOUNDS;
		[Tooltip( TOOLTIP_BOUNDS )] public float boundsSizeCapsule = VERY_LORGE_BOUNDS;


		// minimal/low/medium/high/extreme
		public int[] sphereDetail = { 1, 2, 5, 7, 12 }; // 0 = icosahedron

		public Vector2Int[] torusDivsMinorMajor = {
			new Vector2Int( 6, 8 ),
			new Vector2Int( 12, 16 ),
			new Vector2Int( 24, 32 ),
			new Vector2Int( 32, 48 ),
			new Vector2Int( 64, 128 )
		};

		public int[] coneDivs = { 8, 12, 32, 64, 128 };
		public int[] cylinderDivs = { 8, 12, 32, 64, 128 };
		public int[] capsuleDivs = { 2, 3, 8, 10, 32 }; // 1 divs = 4 sides w. octahedron caps

		// Shader settings
		[Tooltip( "Precision of the fragment shader output.\n" +
				  "\n" +
				  "[fixed4] 11 bit, cheap and very low precision output, range of –2 to +2 and 1/256th precision\n\n" +
				  "[half4] 16 bit, range of –60000 to +60000, with about 3 decimal digits of precision\n\n" +
				  "[float4] 32 bit, full floating point precision" )]
		public FragOutputPrecision FRAG_OUTPUT_V4 = FragOutputPrecision.half4;

		[Tooltip( "[Off] Turns off local anti-aliasing\n\n" +
				  "[Medium] Approximate, usually good enough. This uses the approximate partial derivative of fwidth for anti-aliasing\n\n" +
				  "[High] Higher quality, mathematically correct. Primarily handles diagonals better as it uses more precise partial derivative calculations" )]
		public LocalAAQuality LOCAL_ANTI_ALIASING_QUALITY = LocalAAQuality.High;

		[Tooltip( "[Low] Direct barycentric interpolation of colors per vertex\n" +
				  "  • super cheap\n" +
				  "  • prone to triangular artifacts\n" +
				  "  • playstation 1 energy\n" +
				  "\n[Medium] Barycentric interpolation of UVs, bilinear interpolation in the fragment shader\n" +
				  "  • this gets you like 80% there\n" +
				  "  • most games settle here\n" +
				  "  • only use quality above this if you really need to\n" +
				  "  • or if you are as pretentious as me with colors\n" +
				  "\n[High2D] 2D only, Z plane only, inverse barycentric interpolation in the fragment shader based on vertex positions.\n" +
				  "  • mathematically correct\n" +
				  "  • ...when restricted to the XY plane\n" +
				  "  • numerically unstable otherwise\n" +
				  "  • utterly and completely broken on the X plane or the Y plane. like, it goes invisible and I don't even know why. I think we're dividing by 0 or something idk\n" +
				  "\n[High] Full 3D inverse barycentric interpolation in the fragment shader based on vertex positions.\n" +
				  "  • mathematically correct method\n" +
				  "  • ...when all points are planar\n" +
				  "  • skew quads use a best-fit 2D projection\n" +
				  "  • the shader gets like way more expensive but the colors are nice and you can look at it and go \"nice\"" )]
		public QuadInterpolationQuality QUAD_INTERPOLATION_QUALITY = QuadInterpolationQuality.Medium;

		[Tooltip( "Noots is a unit, in addition to Meters and Pixels, useful for resolution-independent sizing\n" +
				  "A noot is proportional to the shortest dimension of your resolution (note: this is unrelated to physical size)\n\n" +
				  "Converting noots to pixels:\nmin(res.x,res.y)*(noots/NAS)\nres = screen resolution\nNAS = noots across screen\n\n" +
				  "You can specify how big a single noot is here, though, I recommended leaving it at the default value of 100\n\n" +
				  "1 = 1 noot is 100% of the screen\n" +
				  "50 = 1 noot is 50% of the screen\n" +
				  "100 = 1 noot is 1% of the screen (default)\n(100 is like vmin in CSS)" )]
		public int NOOTS_ACROSS_SCREEN = 100;

	}

}