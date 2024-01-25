using TMPro;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal struct DrawStyle {

		const float DEFAULT_THICKNESS = 0.05f;
		const ThicknessSpace DEFAULT_THICKNESS_SPACE = ThicknessSpace.Meters;

		public static DrawStyle @default = new DrawStyle {
			color = Color.white,
			renderState = new RenderState {
				zTest = ShapeRenderer.DEFAULT_ZTEST,
				zOffsetFactor = ShapeRenderer.DEFAULT_ZOFS_FACTOR,
				zOffsetUnits = ShapeRenderer.DEFAULT_ZOFS_UNITS,
				colorMask = ShapeRenderer.DEFAULT_COLOR_MASK,
				stencilComp = ShapeRenderer.DEFAULT_STENCIL_COMP,
				stencilOpPass = ShapeRenderer.DEFAULT_STENCIL_OP,
				stencilRefID = ShapeRenderer.DEFAULT_STENCIL_REF_ID,
				stencilReadMask = ShapeRenderer.DEFAULT_STENCIL_MASK,
				stencilWriteMask = ShapeRenderer.DEFAULT_STENCIL_MASK
			},
			blendMode = ShapesBlendMode.Transparent,
			scaleMode = ScaleMode.Uniform,
			detailLevel = DetailLevel.Medium,

			// styling states
			useDashes = false,
			dashStyle = DashStyle.defaultDashStyle,
			useGradients = false,
			gradientFill = GradientFill.defaultFill,

			// shared states
			thickness = DEFAULT_THICKNESS,
			thicknessSpace = DEFAULT_THICKNESS_SPACE,
			radiusSpace = DEFAULT_THICKNESS_SPACE,
			sizeSpace = DEFAULT_THICKNESS_SPACE,
			radius = 1f,

			// line
			lineEndCaps = LineEndCap.Round,
			lineGeometry = LineGeometry.Billboard,

			// polygon
			polygonTriangulation = PolygonTriangulation.EarClipping,

			// polyline
			polylineGeometry = PolylineGeometry.Billboard,
			polylineJoins = PolylineJoins.Round,

			// disc
			discGeometry = DiscGeometry.Flat2D,

			// regular polygon
			regularPolygonSideCount = 6,
			regularPolygonGeometry = RegularPolygonGeometry.Flat2D,

			// text
			textStyle = TextStyle.defaultTextStyle
		};

		// globally shared render state styles
		public RenderState renderState;
		public Color color;
		public ShapesBlendMode blendMode; // technically a render state rather than property, but we swap shaders here instead
		public ScaleMode scaleMode;
		public DetailLevel detailLevel;

		// styling states
		public bool useDashes;
		public DashStyle dashStyle;
		public bool useGradients;
		public GradientFill gradientFill;

		// shared states
		public float radius;
		public float thickness;
		public ThicknessSpace thicknessSpace;
		public ThicknessSpace radiusSpace;
		public ThicknessSpace sizeSpace;

		// line states
		public LineEndCap lineEndCaps;
		public LineGeometry lineGeometry;

		// polygon states
		public PolygonTriangulation polygonTriangulation;

		// polyline states
		public PolylineGeometry polylineGeometry;
		public PolylineJoins polylineJoins;

		// disc & ring states
		public DiscGeometry discGeometry;

		// regular polygon states
		public int regularPolygonSideCount;
		public RegularPolygonGeometry regularPolygonGeometry;

		// text states
		public TextStyle textStyle;
	}

}