using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static partial class Draw {

		internal static DrawStyle style;

		/// <summary><para>Resets draw style states, including color, but not the drawing matrix</para><para>See <see cref="Draw.ResetAllDrawStates()"/> to reset everything</para></summary>
		[MethodImpl( INLINE )] public static void ResetStyle() => style = DrawStyle.@default;

		/// <summary>using( StyleScope ){ /*code*/ } lets you modify the draw style state within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static StyleStack StyleScope => new StyleStack( Draw.style );

		/// <summary>Pushes the current draw style state onto the style stack. Calling <see cref="Draw.PopStyle()"/> will restore this state</summary>
		[MethodImpl( INLINE )] public static void PushStyle() => StyleStack.Push( Draw.style );

		/// <summary>Restores the draw style state to the previously pushed state from the stack</summary>
		[MethodImpl( INLINE )] public static void PopStyle() => StyleStack.Pop();

		/// <summary>using( ColorScope ){ /*code*/ } lets you modify the color state within that scope, automatically restoring the previously saved color once you leave the scope</summary>
		public static ColorStack ColorScope => new ColorStack( Draw.style.color );

		/// <summary>Pushes the current draw color onto the matrix stack. Calling <see cref="Draw.PopColor()"/> will restore this state</summary>
		[MethodImpl( INLINE )] public static void PushColor() => ColorStack.Push( Draw.style.color );

		/// <summary>Restores the draw color state to the previously pushed state from the stack</summary>
		[MethodImpl( INLINE )] public static void PopColor() => ColorStack.Pop();

		/// <summary>using( DashedScope ){ /*code*/ } temporarily enables dashes within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static DashStack DashedScope() {
			DashStack stack = new DashStack( Draw.UseDashes, Draw.DashStyle );
			Draw.UseDashes = true;
			return stack;
		}

		/// <summary>using( DashedScope ){ /*code*/ } temporarily enables and overrides dashes within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static DashStack DashedScope( DashStyle dashStyle ) {
			DashStack stack = new DashStack( Draw.UseDashes, Draw.DashStyle );
			Draw.UseDashes = true;
			Draw.DashStyle = dashStyle;
			return stack;
		}

		/// <summary>using( GradientFillScope ){ /*code*/ } temporarily enables gradient fill within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static GradientFillStack GradientFillScope() {
			GradientFillStack stack = new GradientFillStack( Draw.UseGradientFill, Draw.GradientFill );
			Draw.UseGradientFill = true;
			return stack;
		}

		/// <summary>using( GradientFillScope ){ /*code*/ } temporarily enables and overrides gradient fill within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static GradientFillStack GradientFillScope( GradientFill fill ) {
			GradientFillStack stack = new GradientFillStack( Draw.UseGradientFill, Draw.GradientFill ); // pushes
			Draw.UseGradientFill = true;
			Draw.GradientFill = fill;
			return stack;
		}

		#region Stencil & Depth Testing

		/// <inheritdoc cref="RenderState.zTest"/>
		public static CompareFunction ZTest {
			[MethodImpl( INLINE )] get => style.renderState.zTest;
			[MethodImpl( INLINE )] set => style.renderState.zTest = value;
		}
		/// <inheritdoc cref="RenderState.zOffsetFactor"/>
		public static float ZOffsetFactor {
			[MethodImpl( INLINE )] get => style.renderState.zOffsetFactor;
			[MethodImpl( INLINE )] set => style.renderState.zOffsetFactor = value;
		}
		/// <inheritdoc cref="RenderState.zOffsetUnits"/>
		public static int ZOffsetUnits {
			[MethodImpl( INLINE )] get => style.renderState.zOffsetUnits;
			[MethodImpl( INLINE )] set => style.renderState.zOffsetUnits = value;
		}
		/// <inheritdoc cref="RenderState.colorMask"/>
		public static ColorWriteMask ColorMask {
			[MethodImpl( INLINE )] get => style.renderState.colorMask;
			[MethodImpl( INLINE )] set => style.renderState.colorMask = value;
		}
		/// <inheritdoc cref="RenderState.colorMask"/>
		public static CompareFunction StencilComp {
			[MethodImpl( INLINE )] get => style.renderState.stencilComp;
			[MethodImpl( INLINE )] set => style.renderState.stencilComp = value;
		}
		/// <inheritdoc cref="RenderState.stencilComp"/>
		public static StencilOp StencilOpPass {
			[MethodImpl( INLINE )] get => style.renderState.stencilOpPass;
			[MethodImpl( INLINE )] set => style.renderState.stencilOpPass = value;
		}
		/// <inheritdoc cref="RenderState.stencilOpPass"/>
		public static byte StencilRefID {
			[MethodImpl( INLINE )] get => style.renderState.stencilRefID;
			[MethodImpl( INLINE )] set => style.renderState.stencilRefID = value;
		}
		/// <inheritdoc cref="RenderState.stencilRefID"/>
		public static byte StencilReadMask {
			[MethodImpl( INLINE )] get => style.renderState.stencilReadMask;
			[MethodImpl( INLINE )] set => style.renderState.stencilReadMask = value;
		}
		/// <inheritdoc cref="RenderState.stencilReadMask"/>
		public static byte StencilWriteMask {
			[MethodImpl( INLINE )] get => style.renderState.stencilWriteMask;
			[MethodImpl( INLINE )] set => style.renderState.stencilWriteMask = value;
		}

		#endregion

		#region Common shared properties

		/// <summary>The color to use when drawing. The alpha channel is used for opacity/intensity in all blend modes</summary>
		public static Color Color {
			[MethodImpl( INLINE )] get => style.color;
			[MethodImpl( INLINE )] set => style.color = value;
		}

		/// <summary>The opacity/intensity of the color. This is just a wrapper function for the alpha channel of Color. It's equivalent to getting/setting Color.a</summary>
		public static float Opacity {
			[MethodImpl( INLINE )] get => Color.a;
			[MethodImpl( INLINE )] set {
				Color c = Color;
				c.a = value;
				Color = c;
			}
		}

		/// <summary>What blending mode to use</summary>
		public static ShapesBlendMode BlendMode {
			[MethodImpl( INLINE )] get => style.blendMode;
			[MethodImpl( INLINE )] set => style.blendMode = value;
		} // technically a render state, but we swap shaders here instead

		/// <summary>Sets how shapes should behave when scaled</summary>
		public static ScaleMode ScaleMode {
			[MethodImpl( INLINE )] get => style.scaleMode;
			[MethodImpl( INLINE )] set => style.scaleMode = value;
		}

		/// <summary>What detail level to use for 3D primitives (3D Lines/Sphere/Torus/Cone)</summary>
		public static DetailLevel DetailLevel {
			[MethodImpl( INLINE )] get => style.detailLevel;
			[MethodImpl( INLINE )] set => style.detailLevel = value;
		}

		/// <summary>The thickness of all shapes with a thickness (line, polyline, ring, arc, regular polygon, torus etc.), in the given <c>ThicknessSpace</c></summary>
		public static float Thickness {
			[MethodImpl( INLINE )] get => style.thickness;
			[MethodImpl( INLINE )] set => style.thickness = value;
		}

		/// <summary>The radius of all shapes with a radius (disc, ring, pie, arc, regular polygon, torus, cone etc.), in the given <c>RadiusSpace</c></summary>
		public static float Radius {
			[MethodImpl( INLINE )] get => style.radius;
			[MethodImpl( INLINE )] set => style.radius = value;
		}

		/// <summary>The thickness space of all shapes with a thickness (line, ring, arc, polyline, torus etc.)</summary>
		public static ThicknessSpace ThicknessSpace {
			[MethodImpl( INLINE )] get => style.thicknessSpace;
			[MethodImpl( INLINE )] set => style.thicknessSpace = value;
		}

		/// <summary>The radius space of all shapes with a radius (disc, ring, arc, pie, torus, cone, torus, etc.)</summary>
		public static ThicknessSpace RadiusSpace {
			[MethodImpl( INLINE )] get => style.radiusSpace;
			[MethodImpl( INLINE )] set => style.radiusSpace = value;
		}

		/// <summary>The size space of all shapes with a non-radius based size dimension (cuboid, cone, etc.)</summary>
		public static ThicknessSpace SizeSpace {
			[MethodImpl( INLINE )] get => style.sizeSpace;
			[MethodImpl( INLINE )] set => style.sizeSpace = value;
		}

		#endregion

		#region Gradient Fill

		/// <summary>Whether or not to use gradient fill on shapes (note: this is not supported on all shapes)</summary>
		public static bool UseGradientFill {
			[MethodImpl( INLINE )] get => style.useGradients;
			[MethodImpl( INLINE )] set => style.useGradients = value;
		}

		/// <summary>The gradient fill style to use on all fillable shapes</summary>
		public static GradientFill GradientFill {
			[MethodImpl( INLINE )] get => style.gradientFill;
			[MethodImpl( INLINE )] set => style.gradientFill = value;
		}

		/// <summary>The type of color gradient to use (Linear vs Radial)</summary>
		public static FillType GradientFillType {
			[MethodImpl( INLINE )] get => style.gradientFill.type;
			[MethodImpl( INLINE )] set => style.gradientFill.type = value;
		}

		/// <summary>The space to draw the shape fill gradients in (Local or World)</summary>
		public static FillSpace GradientFillSpace {
			[MethodImpl( INLINE )] get => style.gradientFill.space;
			[MethodImpl( INLINE )] set => style.gradientFill.space = value;
		}

		/// <summary>The start color of shape fill gradients. For radial gradients, this is the inner color</summary>
		public static Color GradientFillColorStart {
			[MethodImpl( INLINE )] get => style.gradientFill.colorStart;
			[MethodImpl( INLINE )] set => style.gradientFill.colorStart = value;
		}

		/// <summary>The end color of shape fill gradients. For radial gradients, this is the outer color</summary>
		public static Color GradientFillColorEnd {
			[MethodImpl( INLINE )] get => style.gradientFill.colorEnd;
			[MethodImpl( INLINE )] set => style.gradientFill.colorEnd = value;
		}

		/// <summary>The starting point of shape fill linear gradients, in the given GradientFillSpace</summary>
		public static Vector3 GradientFillLinearStart {
			[MethodImpl( INLINE )] get => style.gradientFill.linearStart;
			[MethodImpl( INLINE )] set => style.gradientFill.linearStart = value;
		}

		/// <summary>The endpoint of shape fill linear gradients, in the given GradientFillSpace</summary>
		public static Vector3 GradientFillLinearEnd {
			[MethodImpl( INLINE )] get => style.gradientFill.linearEnd;
			[MethodImpl( INLINE )] set => style.gradientFill.linearEnd = value;
		}

		/// <summary>The origin of shape fill radial gradients, in the given GradientFillSpace</summary>
		public static Vector3 GradientFillRadialOrigin {
			[MethodImpl( INLINE )] get => style.gradientFill.radialOrigin;
			[MethodImpl( INLINE )] set => style.gradientFill.radialOrigin = value;
		}

		/// <summary>The radius of shape fill radial gradients, in the given GradientFillSpace</summary>
		public static float GradientFillRadialRadius {
			[MethodImpl( INLINE )] get => style.gradientFill.radialRadius;
			[MethodImpl( INLINE )] set => style.gradientFill.radialRadius = value;
		}

		#endregion

		#region Dashes

		/// <summary>Whether or not to dash shapes (note: this is not supported on all shapes)</summary>
		public static bool UseDashes {
			[MethodImpl( INLINE )] get => style.useDashes;
			[MethodImpl( INLINE )] set => style.useDashes = value;
		}

		/// <summary>The full dash style to use for all dashable shapes</summary>
		public static DashStyle DashStyle {
			[MethodImpl( INLINE )] get => style.dashStyle;
			[MethodImpl( INLINE )] set => style.dashStyle = value;
		}

		/// <summary>The type of dash to use</summary>
		public static DashType DashType {
			[MethodImpl( INLINE )] get => style.dashStyle.type;
			[MethodImpl( INLINE )] set => style.dashStyle.type = value;
		}

		/// <summary>The space in which dashes are defined</summary>
		public static DashSpace DashSpace {
			[MethodImpl( INLINE )] get => style.dashStyle.space;
			[MethodImpl( INLINE )] set => style.dashStyle.space = value;
		}

		/// <summary>What snapping type to use</summary>
		public static DashSnapping DashSnap {
			[MethodImpl( INLINE )] get => style.dashStyle.snap;
			[MethodImpl( INLINE )] set => style.dashStyle.snap = value;
		}

		/// <summary>Size of dashes in the specified dash space. When using DashSpace.FixedCount, this is the number of dashes</summary>
		public static float DashSize {
			[MethodImpl( INLINE )] get => style.dashStyle.size;
			[MethodImpl( INLINE )] set => style.dashStyle.size = value;
		}

		/// <summary>Sets both dash size and spacing to the same value. When using DashSpace.FixedCount, it will set spacing to 0.5, which makes the dash length the same as space length</summary>
		public static float DashSizeUniform {
			[MethodImpl( INLINE )] get => style.dashStyle.size; // a lil weird but it's okay
			[MethodImpl( INLINE )] set {
				style.dashStyle.size = value;
				style.dashStyle.spacing = style.dashStyle.space == DashSpace.FixedCount ? 0.5f : value;
			}
		}

		/// <summary>Size of spacing between each dash, in the specified dash space. When using DashSpace.FixedCount, this is the dash:space ratio</summary>
		public static float DashSpacing {
			[MethodImpl( INLINE )] get => style.dashStyle.spacing;
			[MethodImpl( INLINE )] set => style.dashStyle.spacing = value;
		}

		/// <summary>Sets the offset of dashes. An offset of 1 is the size of a whole dash+space period, meaning it will repeat at every integer value</summary>
		public static float DashOffset {
			[MethodImpl( INLINE )] get => style.dashStyle.offset;
			[MethodImpl( INLINE )] set => style.dashStyle.offset = value;
		}

		/// <summary>-1 to 1 modifier that allows you to tweak or mirror certain dash types</summary>
		public static float DashShapeModifier {
			[MethodImpl( INLINE )] get => style.dashStyle.shapeModifier;
			[MethodImpl( INLINE )] set => style.dashStyle.shapeModifier = value;
		}

		#endregion

		/// <summary>End caps of lines</summary>
		public static LineEndCap LineEndCaps {
			[MethodImpl( INLINE )] get => style.lineEndCaps;
			[MethodImpl( INLINE )] set => style.lineEndCaps = value;
		}

		/// <summary>Type of geometry for lines</summary>
		public static LineGeometry LineGeometry {
			[MethodImpl( INLINE )] get => style.lineGeometry;
			[MethodImpl( INLINE )] set => style.lineGeometry = value;
		}

		/// <summary>The triangulation method to use. Some of these are computationally faster than others, but only works for certain shapes</summary>
		public static PolygonTriangulation PolygonTriangulation {
			[MethodImpl( INLINE )] get => style.polygonTriangulation;
			[MethodImpl( INLINE )] set => style.polygonTriangulation = value;
		}

		/// <summary>Type of geometry for polylines</summary>
		public static PolylineGeometry PolylineGeometry {
			[MethodImpl( INLINE )] get => style.polylineGeometry;
			[MethodImpl( INLINE )] set => style.polylineGeometry = value;
		}

		/// <summary>The joins to use for polylines</summary>
		public static PolylineJoins PolylineJoins {
			[MethodImpl( INLINE )] get => style.polylineJoins;
			[MethodImpl( INLINE )] set => style.polylineJoins = value;
		}

		/// <summary>Whether or not discs, rings, pies &amp; arcs should be billboarded</summary>
		public static DiscGeometry DiscGeometry {
			[MethodImpl( INLINE )] get => style.discGeometry;
			[MethodImpl( INLINE )] set => style.discGeometry = value;
		}

		/// <summary>The number of sides on regular polygons</summary>
		public static int RegularPolygonSideCount {
			[MethodImpl( INLINE )] get => style.regularPolygonSideCount;
			[MethodImpl( INLINE )] set => style.regularPolygonSideCount = value;
		}

		/// <summary>Whether or not regular polygons should be billboarded</summary>
		public static RegularPolygonGeometry RegularPolygonGeometry {
			[MethodImpl( INLINE )] get => style.regularPolygonGeometry;
			[MethodImpl( INLINE )] set => style.regularPolygonGeometry = value;
		}


		#region Text

		/// <summary>The full text style to use when drawing text</summary>
		public static TextStyle TextStyle {
			[MethodImpl( INLINE )] get => style.textStyle;
			[MethodImpl( INLINE )] set => style.textStyle = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.font"/>
		public static TMP_FontAsset Font {
			[MethodImpl( INLINE )] get => style.textStyle.font;
			[MethodImpl( INLINE )] set => style.textStyle.font = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.size"/>
		public static float FontSize {
			[MethodImpl( INLINE )] get => style.textStyle.size;
			[MethodImpl( INLINE )] set => style.textStyle.size = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.style"/>
		public static FontStyles FontStyle {
			[MethodImpl( INLINE )] get => style.textStyle.style;
			[MethodImpl( INLINE )] set => style.textStyle.style = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.alignment"/>
		public static TextAlign TextAlign {
			[MethodImpl( INLINE )] get => style.textStyle.alignment;
			[MethodImpl( INLINE )] set => style.textStyle.alignment = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.characterSpacing"/>
		public static float TextCharacterSpacing {
			[MethodImpl( INLINE )] get => style.textStyle.characterSpacing;
			[MethodImpl( INLINE )] set => style.textStyle.characterSpacing = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.wordSpacing"/>
		public static float TextWordSpacing {
			[MethodImpl( INLINE )] get => style.textStyle.wordSpacing;
			[MethodImpl( INLINE )] set => style.textStyle.wordSpacing = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.lineSpacing"/>
		public static float TextLineSpacing {
			[MethodImpl( INLINE )] get => style.textStyle.lineSpacing;
			[MethodImpl( INLINE )] set => style.textStyle.lineSpacing = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.paragraphSpacing"/>
		public static float TextParagraphSpacing {
			[MethodImpl( INLINE )] get => style.textStyle.paragraphSpacing;
			[MethodImpl( INLINE )] set => style.textStyle.paragraphSpacing = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.margins"/>
		public static Vector4 TextMargins {
			[MethodImpl( INLINE )] get => style.textStyle.margins;
			[MethodImpl( INLINE )] set => style.textStyle.margins = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.wrap"/>
		public static bool TextWrap {
			[MethodImpl( INLINE )] get => style.textStyle.wrap;
			[MethodImpl( INLINE )] set => style.textStyle.wrap = value;
		}

		/// <inheritdoc cref="Shapes.TextStyle.overflow"/>
		public static TextOverflowModes TextOverflow {
			[MethodImpl( INLINE )] get => style.textStyle.overflow;
			[MethodImpl( INLINE )] set => style.textStyle.overflow = value;
		}

		#endregion

		#region Deprecated

		[System.Obsolete( "All shapes now use the same static " + nameof(Thickness) + " property", true )]
		public static float LineThickness {
			get => style.thickness;
			set => style.thickness = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace LineThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(DashStyle) + " property by default, when " + nameof(UseDashes) + " is enabled", true )]
		public static DashStyle LineDashStyle {
			get => style.dashStyle;
			set => style.dashStyle = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Radius) + " property", true )]
		public static float DiscRadius {
			get => style.radius;
			set => style.radius = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(DashStyle) + " property by default, when " + nameof(UseDashes) + " is enabled", true )]
		public static DashStyle RingDashStyle {
			get => style.dashStyle;
			set => style.dashStyle = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(GradientFill) + " property by default. If you want to override shape fill per shape, use the draw overload with a fill input", true )]
		public static GradientFill PolygonShapeFill {
			get => style.gradientFill;
			set => style.gradientFill = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(GradientFill) + " property by default. If you want to override shape fill per shape, use the draw overload with a fill input", true )]
		public static GradientFill RegularPolygonShapeFill {
			get => style.gradientFill;
			set => style.gradientFill = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(GradientFill) + " property by default. If you want to override shape fill per shape, use the draw overload with a fill input", true )]
		public static GradientFill RectangleShapeFill {
			get => style.gradientFill;
			set => style.gradientFill = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Thickness) + " property", true )]
		public static float RingThickness {
			get => style.thickness;
			set => style.thickness = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace RingThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(RadiusSpace) + " property", true )]
		public static ThicknessSpace DiscRadiusSpace {
			get => style.radiusSpace;
			set => style.radiusSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Radius) + " property", true )]
		public static float RegularPolygonRadius {
			get => style.radius;
			set => style.radius = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Thickness) + " property", true )]
		public static float RegularPolygonThickness {
			get => style.thickness;
			set => style.thickness = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace RegularPolygonThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(RadiusSpace) + " property", true )]
		public static ThicknessSpace RegularPolygonRadiusSpace {
			get => style.radiusSpace;
			set => style.radiusSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Thickness) + " property", true )]
		public static float RectangleThickness {
			get => style.thickness;
			set => style.thickness = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace RectangleThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Thickness) + " property", true )]
		public static float TriangleThickness {
			get => style.thickness;
			set => style.thickness = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace TriangleThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(Radius) + " property", true )]
		public static float SphereRadius {
			get => style.radius;
			set => style.radius = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(RadiusSpace) + " property", true )]
		public static ThicknessSpace SphereRadiusSpace {
			get => style.radiusSpace;
			set => style.radiusSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(SizeSpace) + " property", true )]
		public static ThicknessSpace CuboidSizeSpace {
			get => style.sizeSpace;
			set => style.sizeSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(ThicknessSpace) + " property", true )]
		public static ThicknessSpace TorusThicknessSpace {
			get => style.thicknessSpace;
			set => style.thicknessSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(RadiusSpace) + " property", true )]
		public static ThicknessSpace TorusRadiusSpace {
			get => style.radiusSpace;
			set => style.radiusSpace = value;
		}

		[System.Obsolete( "All shapes now use the same static " + nameof(SizeSpace) + " property", true )]
		public static ThicknessSpace ConeSizeSpace {
			get => style.sizeSpace;
			set => style.sizeSpace = value;
		}

		#endregion

	}

}