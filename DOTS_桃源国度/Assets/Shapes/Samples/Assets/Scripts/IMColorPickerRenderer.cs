using UnityEngine;

namespace Shapes {

	/// <summary>
	/// Color picker renderer for the Shapes color picker example scene
	/// </summary>
	[ExecuteAlways] public class IMColorPickerRenderer : ImmediateModeShapeDrawer {

		[Header( "Color value" )]
		[Range( 0, 1 )] public float hue = 0;
		[Range( 0, 1 )] public float saturation = 1;
		[Range( 0, 1 )] public float value = 1;

		[Header( "Styling" )]
		[Range( 0, 0.3f )] public float hueStripThickness;
		[Range( 0, 0.1f )] public float outline;
		[Range( 0, 0.1f )] public float quadMargin;
		[Range( 0, 1.5f )] public float hueDotScale;
		public Vector2 labelSize;

		// state
		PolylinePath hueStripPath; // cached polyline for extra performance

		// properties & utility functions
		public Color CurrentPureColor => Color.HSVToRGB( hue, 1, 1 );
		public Color CurrentColor => Color.HSVToRGB( hue, saturation, value );
		public float QuadScale => ( 1f - hueStripThickness / 2 - quadMargin ) / Mathf.Sqrt( 2 );
		public Rect QuadRect => new Rect( default, Vector2.one * QuadScale * 2 ) { center = default };
		public float HueStripRadiusOuter => 1 + hueStripThickness / 2 + outline;
		public float HueStripRadiusInner => 1 - hueStripThickness / 2 - outline;
		public static Vector2 HueToVector( float hue ) => ShapesMath.AngToDir( hue * ShapesMath.TAU );
		public static float VectorToHue( Vector2 v ) => ShapesMath.Frac( ( ShapesMath.DirToAng( v ) / ShapesMath.TAU ) );

		public override void OnEnable() {
			base.OnEnable();
			ConstructHueStripPolyline();
		}

		public override void OnDisable() {
			base.OnDisable();
			hueStripPath.Dispose(); // important to dispose mesh data when destroying this object or leaving this scene
		}

		// Main drawing function. Called every time a camera wants to render this
		public override void DrawShapes( Camera cam ) {
			using( Draw.Command( cam ) ) {
				// make drawing relative to this transform
				Draw.Matrix = transform.localToWorldMatrix;

				// hue strip
				Draw.Ring( Vector3.zero, 1f, hueStripThickness + outline, Color.black ); // outline/background
				Draw.PolylineJoins = PolylineJoins.Simple;
				Draw.PolylineGeometry = PolylineGeometry.Flat2D;
				Draw.Polyline( hueStripPath, closed: true, hueStripThickness );

				// color rectangle
				float quadScale = QuadScale;
				Draw.Rectangle( Vector3.zero, Vector2.one * ( ( quadScale * 2 ) + outline ), Color.black ); // outline/background
				using( Draw.MatrixScope ) {
					Draw.Scale( quadScale );
					Draw.Quad(
						new Vector2( -1, -1 ), new Vector2( 1, -1 ), new Vector2( 1, 1 ), new Vector2( -1, 1 ),
						Color.black, Color.black, CurrentPureColor, Color.white
					);
				}

				// label
				Rect labelRect = new Rect( -labelSize.x / 2, -quadScale - labelSize.y, labelSize.x, labelSize.y );
				Draw.Rectangle( labelRect, 0.1f, Color.black ); // background
				string hexColor = "#" + ColorUtility.ToHtmlStringRGB( CurrentColor );
				Draw.FontSize = labelSize.y * 8.5f;
				Draw.TextAlign = TextAlign.Center;
				Draw.TextRect( labelRect, hexColor );

				// hue dot
				float dotRadius = ( hueStripThickness / 2 ) * hueDotScale;
				Vector2 hueDotPos = HueToVector( hue );
				Draw.Disc( hueDotPos, dotRadius + outline / 2, Color.black );
				Draw.Disc( hueDotPos, dotRadius, CurrentPureColor );

				// saturation/value dot
				Vector2 satValDot = ShapesMath.Lerp( QuadRect, new Vector2( saturation, value ) );
				Draw.Disc( satValDot, dotRadius + outline / 2, Color.black );
				Draw.Disc( satValDot, dotRadius, CurrentColor );
			}
		}

		void ConstructHueStripPolyline() {
			hueStripPath = new PolylinePath();
			const int DETAIL = 100;
			for( int i = 0; i < DETAIL; i++ ) {
				float tHue = i / (float)DETAIL;
				Color color = Color.HSVToRGB( tHue, 1, 1 );
				Vector3 pt = HueToVector( tHue );
				hueStripPath.AddPoint( pt, color );
			}
		}

	}

}

