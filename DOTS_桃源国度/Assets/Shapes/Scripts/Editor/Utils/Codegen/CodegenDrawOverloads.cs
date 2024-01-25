using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class CodegenDrawOverloads {

		const int FILL_CUSTOM = GradientFill.FILL_NONE - 1;
		public static bool DEBUG_WRITE_EMPTY_BODIES = false; // debugging purposes

		static readonly (int, string)[] fillTypes = {
			( GradientFill.FILL_NONE, "" ),
			( FILL_CUSTOM, "Fill" ), // specify fill with the type thingy. also default
			( (int)FillType.LinearGradient, "FillLinear" ),
			( (int)FillType.RadialGradient, "FillRadial" )
		};

		static readonly (int, string)[] fillTypesShort = {
			( GradientFill.FILL_NONE, "" ),
			( FILL_CUSTOM, "Fill" ) // specify fill with the type thingy. also default
		};

		public enum TextDrawMode {
			Simple,
			RegionSizePivot,
			RegionRect
		}

		struct TextDrawInfo {

			public TextDrawMode mode;
			public string methodNameTarget;
			public string methodNameSelf;

			public bool IsRect => mode != TextDrawMode.Simple;

			public TextDrawInfo( TextDrawMode mode, string methodNameSelf, string methodNameTarget ) {
				this.mode = mode;
				this.methodNameTarget = methodNameTarget;
				this.methodNameSelf = methodNameSelf;
			}
		}

		static TextDrawInfo[] textDrawModes = {
			new TextDrawInfo( TextDrawMode.Simple, "Text", "Text_Internal" ),
			new TextDrawInfo( TextDrawMode.RegionSizePivot, "TextRect", "Text_Internal" ),
			new TextDrawInfo( TextDrawMode.RegionRect, "TextRect", "TextRect_Internal" )
		};

		public static void GenerateDrawOverloadsScript() {
			CodeWriter code = new CodeWriter();
			using( code.MainScope( typeof(CodegenDrawOverloads), "UnityEngine", "TMPro", "System.Runtime.CompilerServices" ) ) {
				using( code.Scope( "public static partial class " + nameof(Draw) ) )
					code.Append( GenerateAllOverloads() );
			}

			string path = ShapesIO.RootFolder + "/Scripts/Runtime/Immediate Mode/DrawOverloads.cs";
			code.WriteTo( path );
		}

		// documentation strings
		const string DOC_POS = "The position of the [OBJECTNAME]";
		const string DOC_ROT = "The orientation of the [OBJECTNAME]";
		const string DOC_NORMAL = "The normal direction of the [OBJECTNAME]";
		const string DOC_COLOR = "The color of the [OBJECTNAME]";
		const string DOC_THICKNESS = "The thickness of the [OBJECTNAME]";
		const string DOC_RADIUS = "The radius of this [OBJECTNAME]";
		const string DOC_ANG_START = "The start angle of the [OBJECTNAME], in radians";
		const string DOC_ANG_END = "The end angle of the [OBJECTNAME], in radians";
		const string DOC_DASH_STYLE = "The configuration of how to display the dashes";
		const string DOC_ROUNDNESS = "A value from 0 to 1 setting how rounded the corners should be. 0 means sharp, 1 means fully rounded";
		const string DOC_BORDER_THICKNESS = "The thickness of the border";

		const string DOC_BLENDMODE = "The type of blending to draw with";
		const string DOC_LINE_GEOMETRY = "The type of line geometry to use";
		const string DOC_THICKNESS_SPACE = "What space thickness is defined in";
		const string DOC_RADIUS_SPACE = "What space radius is defined in";

		const string DOC_LINE = "Draws a line from start to end";
		const string DOC_LINE_START = "The starting point of this line";
		const string DOC_LINE_END = "The endpoint of this line";
		const string DOC_LINE_THICKNESS = DOC_THICKNESS;
		const string DOC_LINE_ENDCAPS = "The type of end caps to use";
		const string DOC_LINE_COLOR_START = "The color at the starting point";
		const string DOC_LINE_COLOR_END = "The color at the endpoint";

		const string DOC_POLYLINE = "Draws a polyline, given an existing polyline path";
		const string DOC_POLYLINE_PATH = "The path to use when drawing. Note: be sure to call myPolylinePath.Dispose() when you are done with it, or use it inside of using-statements";
		const string DOC_POLYLINE_THICKNESS = DOC_THICKNESS;
		const string DOC_POLYLINE_CLOSED = "Whether or not this polyline should be a closed loop";
		const string DOC_POLYLINE_GEOMETRY = "Whether it should be flat on the local space XY plane or 3D billboarded";
		const string DOC_POLYLINE_JOINS = "What type of joins to use";

		const string DOC_POLYGON = "Draws a simple polygon, given an existing polygon path";
		const string DOC_POLYGON_PATH = "The path to use when drawing. Note: Be be sure to call myPolygonPath.Dispose() when you are done with it, or use it inside of using-statements";
		const string DOC_POLYGON_TRIANGULATION = "The triangulation method to use. Some of these are computationally faster than others, but only works for certain shapes";

		const string DOC_REGPOL = "Draws a regular polygon";
		const string DOC_REGPOL_HOLLOW = "Whether or not it should be hollowed out";
		const string DOC_REGPOL_SIDES = "Number of sides of this regular polygon. 3 = triangle, 5 = pentagon, 6 = hexagon, and so on!";
		const string DOC_REGPOL_RADIUS = "The radius from center to vertex";
		const string DOC_REGPOL_ANGLE = "Angular offset in radians";

		const string DOC_DISC = "Draws a solid filled disc";
		const string DOC_RING = "Draws a ring (circle)";
		const string DOC_PIE = "Draws a solid filled pie shape, using start/end angles";
		const string DOC_ARC = "Draws a circular arc, using start/end angles";
		const string DOC_ARC_ENDCAPS = "What type of end caps to use on the arc";
		const string DOC_DISC_COLOR_INNER = "The color of the inner side of the gradient";
		const string DOC_DISC_COLOR_OUTER = "The color of the outer side of the gradient";
		const string DOC_DISC_COLOR_START = "The color at the start angle of the gradient";
		const string DOC_DISC_COLOR_END = "The color at the end angle of the gradient";
		const string DOC_DISC_COLOR_INNER_START = "The color on the inner side at the start angle";
		const string DOC_DISC_COLOR_OUTER_START = "The color on the outer side at the start angle";
		const string DOC_DISC_COLOR_INNER_END = "The color on the inner side at the end angle";
		const string DOC_DISC_COLOR_OUTER_END = "The color on the outer side at the end angle";
		const string DOC_DISC_COLOR = "The color of the [OBJECTNAME]. This can be either a single color, or you can use DiscColor.Radial(), DiscColors.Angular() or DiscColors.Bilinear() for gradients";

		const string DOC_RECT = "Draws a rectangle with a given size";
		const string DOC_RECT_HOLLOW = "Whether or not this rectangle should be hollow";
		const string DOC_RECT_RECT = "The shape of the rectangle in local space";
		const string DOC_RECT_SIZE = "The size of the rectangle in local space";
		const string DOC_RECT_WIDTH = "The width of the rectangle in local space";
		const string DOC_RECT_HEIGHT = "The height of the rectangle in local space";
		const string DOC_RECT_PIVOT = "Set where the pivot of this rectangle should be";
		const string DOC_RECT_THICKNESS = "The thickness of the rectangular border";
		const string DOC_RECT_CORNER_RADIUS = "The radius of rounded corners";
		const string DOC_RECT_CORNER_RADII = "The radius for each corner when rounded. The order is clockwise from the bottom left";

		const string DOC_TRI = "Draws a triangle with arbitrary vertex positions and colors";
		const string DOC_QUAD = "Draws a quad with arbitrary vertex positions and colors";
		const string DOC_SPHERE = "Draws a 3D sphere with a given radius";
		const string DOC_CUBOID = "Draws a 3D cuboid with a given size";
		const string DOC_CUBE = "Draws a 3D cube with a given size";
		const string DOC_CUBE_SIZE = "";
		const string DOC_CUBOID_POS = "The center point of the [OBJECTNAME]";
		const string DOC_CUBOID_SIZE = "";

		const string DOC_A_POS = "The position of the first point";
		const string DOC_B_POS = "The position of the second point";
		const string DOC_C_POS = "The position of the third point";
		const string DOC_D_POS = "The position of the fourth point";
		const string DOC_A_COLOR = "The color of the first point";
		const string DOC_B_COLOR = "The color of the second point";
		const string DOC_C_COLOR = "The color of the third point";
		const string DOC_D_COLOR = "The color of the fourth point";

		const string DOC_CONE = "Draws a 3D cone with a given radius and length";
		const string DOC_CONE_POS = "The position of the center of the base";
		const string DOC_CONE_RADIUS = "The radius of the base of the cone";
		const string DOC_CONE_LENGTH = "The length/height of the cone";
		const string DOC_CONE_CAP = "Whether or not the base cap should be filled or hollow";

		const string DOC_TORUS = "Draws a 3D torus with a given radius and thickness";

		const string DOC_TEXT = "Draws text using Text Mesh Pro";
		const string DOC_TEXT_CONTENT = "The text to display";
		const string DOC_TEXT_ELEMENT = "The text element to use when drawing this text";
		const string DOC_TEXT_RECT = "The local space rectangle to display text within";
		const string DOC_TEXT_PIVOT = "The normalized pivot of the local space rectangle for text positioning, like Unity's RectTransform";
		const string DOC_TEXT_SIZE = "The local space size of the rectangle to draw text the in";
		const string DOC_TEXT_ALIGN = "The text alignment to use";
		const string DOC_TEXT_ANGLE = "The angular offset of the text, in radians";
		const string DOC_TEXT_FONT = "The font to use";
		const string DOC_TEXT_FONT_SIZE = "The text alignment to use";

		const string DOC_TEXTURE = "Draws a texture";
		const string DOC_TEXTURE_TEXTURE = "The texture to draw. Its alpha channel will be used as transparency/intensity";
		const string DOC_TEXTURE_RECT = "The region to draw the texture in";
		const string DOC_TEXTURE_UVS = "The UV coordinates to use. Position will offset the texture, size will scale the texture. Default UVs to fit the texture: (0,0,1,1)";
		const string DOC_TEXTURE_FILL_MODE = "How to place the texture within the rectangle";
		const string DOC_TEXTURE_CENTER = "The center of the texture";
		const string DOC_TEXTURE_SIZE = "The size of this texture";
		const string DOC_TEXTURE_SIZE_MODE = "How to interpret the size value";

		const string DOC_FILL = "The color fill style to use";
		const string DOC_FILL_SPACE = "Whether the gradient should be drawn in local space or world space";
		const string DOC_FILL_LIN_START = "The start position of the linear gradient";
		const string DOC_FILL_LIN_END = "The end position of the linear gradient";
		const string DOC_FILL_LIN_START_COLOR = "The start color of the linear gradient";
		const string DOC_FILL_LIN_END_COLOR = "The end color of the linear gradient";

		const string DOC_FILL_RAD_CENTER = "The center of the radial gradient";
		const string DOC_FILL_RAD_RADIUS = "The radius of the radial gradient";
		const string DOC_FILL_RAD_COLOR_START = "The inner color of the radial gradient";
		const string DOC_FILL_RAD_COLOR_END = "The outer color of the radial gradient";


		static List<string> GenerateAllOverloads() {
			List<string> lines = new List<string>();

			// find core draw functions as targets of the overloads
			Dictionary<string, TargetMethodCall> callTargets = typeof(Draw)
				.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
				.Where( prop => Attribute.IsDefined( prop, typeof(OvldGenCallTarget) ) )
				.Select( m => new TargetMethodCall( m ) )
				.ToDictionary( m => m.name, m => m );

			// shared things
			OverloadGenerator g;
			bool[] bools = { true, false };
			OrSelectorParams rotIdentityNormalQuat = new OrSelectorParams(
				null,
				new[] { new Param( $"Vector3 normal // {DOC_NORMAL}" ) { mtxFlags = Param.MtxFlags.Normal } },
				new[] { new Param( $"Quaternion rot // {DOC_ROT}" ) { mtxFlags = Param.MtxFlags.Rotation } }
			);

			// Line
			g = new OverloadGenerator( "Line", callTargets["Line_Internal"], DOC_LINE ) { objectName = "line" };
			g += $"Vector3 start // {DOC_LINE_START}";
			g += $"Vector3 end // {DOC_LINE_END}";
			g += new CombinatorialParams( $"float thickness // {DOC_LINE_THICKNESS}", $"LineEndCap endCaps // {DOC_LINE_ENDCAPS}" );
			g += new OrSelectorParams(
				null,
				new[] { new Param( $"Color color // {DOC_COLOR}", "colorStart", "colorEnd" ) },
				new Param[] { $"Color colorStart // {DOC_LINE_COLOR_START}", $"Color colorEnd // {DOC_LINE_COLOR_END}" }
			);
			g.GenerateAndAppend( lines );


			// Polyline
			g = new OverloadGenerator( "Polyline", callTargets["Polyline_Internal"], DOC_POLYLINE );
			g += $"PolylinePath path // {DOC_POLYLINE_PATH}";
			g += new CombinatorialParams(
				$"bool closed // {DOC_POLYLINE_CLOSED}",
				$"float thickness // {DOC_POLYLINE_THICKNESS}",
				$"PolylineJoins joins // {DOC_POLYLINE_JOINS}",
				$"Color color // {DOC_COLOR}"
			);
			g.GenerateAndAppend( lines );

			// Polygon
			g = new OverloadGenerator( "Polygon", callTargets["Polygon_Internal"], DOC_POLYGON ) { objectName = "polygon" };
			g += $"PolygonPath path // {DOC_POLYGON_PATH}";
			g += new CombinatorialParams( $"PolygonTriangulation triangulation // {DOC_POLYGON_TRIANGULATION}" );
			g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
			g.GenerateAndAppend( lines );

			// Regular Polygon
			foreach( ( bool hollow, string hollownessSuffix ) in new[] { ( false, "" ), ( true, "Border" ) } ) {
				foreach( bool usePositioning in bools ) {
					g = new OverloadGenerator( "RegularPolygon" + hollownessSuffix, callTargets["RegularPolygon_Internal"], DOC_REGPOL ) { objectName = "regular polygon" };
					g.constAssigns["hollow"] = hollow.ToString().ToLowerInvariant();
					if( usePositioning ) {
						g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
						g += rotIdentityNormalQuat;
					}

					g += new CombinatorialParams( new Param( $"int sideCount // {DOC_REGPOL_SIDES}" ) );

					Param rpRadius = new Param( $"float radius // {DOC_REGPOL_RADIUS}" );
					Param rpAngle = new Param( $"float angle // {DOC_REGPOL_ANGLE}" );
					Param rpRoundness = new Param( $"float roundness // {DOC_ROUNDNESS}" );

					if( hollow ) {
						Param rpThickness = new Param( $"float thickness // {DOC_BORDER_THICKNESS}" );
						g += new OrSelectorParams(
							null,
							new[] { rpRadius },
							new[] { rpRadius, rpThickness },
							new[] { rpRadius, rpThickness, rpAngle },
							new[] { rpRadius, rpThickness, rpAngle, rpRoundness }
						);
					} else {
						g += new OrSelectorParams(
							null,
							new[] { rpRadius },
							new[] { rpRadius, rpAngle },
							new[] { rpRadius, rpAngle, rpRoundness }
						);
					}

					g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
					g.GenerateAndAppend( lines );
				}
			}


			// Disc / Ring / Pie / Arc
			string[] discTypeNames = { "Disc", "Ring", "Pie", "Arc" };
			string[] shapeDoc = { DOC_DISC, DOC_RING, DOC_PIE, DOC_ARC };
			for( int idt = 0; idt < 4; idt++ ) {
				string discType = discTypeNames[idt];
				bool hollow = idt == 1 || idt == 3; // ring or arc
				bool angles = idt == 2 || idt == 3;
				foreach( bool usePositioning in bools ) {
					g = new OverloadGenerator( discType, callTargets[discType + "_Internal"], shapeDoc[idt] ) { objectName = discType };
					if( usePositioning ) {
						g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
						g += rotIdentityNormalQuat;
					}

					Param discRadius = new Param( $"float radius // {DOC_RADIUS}" );
					if( hollow ) {
						// we have to do this because we can't use both radius and thickness on their own as overloads
						g += new OrSelectorParams(
							null,
							new[] { discRadius },
							new[] { discRadius, new Param( $"float thickness // {DOC_THICKNESS}" ) }
						);
					} else {
						g += new CombinatorialParams( discRadius );
					}

					if( angles ) {
						g += $"float angleRadStart // {DOC_ANG_START}";
						g += $"float angleRadEnd // {DOC_ANG_END}";
					}

					if( angles && hollow ) // arc only
						g += new OrSelectorParams( null, new[] { new Param( $"{nameof(ArcEndCap)} endCaps // {DOC_ARC_ENDCAPS}" ) } );

					// colors, optional
					g += new CombinatorialParams( new[] { new Param( $"DiscColors colors // {DOC_DISC_COLOR}", "colors" ) } );

					g.GenerateAndAppend( lines );
				}
			}

			// Rectangle / RectangleBorder
			foreach( ( bool border, string borderSuffix ) in new[] { ( false, "" ), ( true, "Border" ) } ) {
				for( int mode = 0; mode < 4; mode++ ) {
					bool doRect = mode == 0 || mode == 2;
					bool posRot = mode == 0 || mode == 1 || mode == 3;
					bool sizePivotParams = mode == 1 || mode == 3;
					bool noPivot = mode == 1; // mode 1 is without optional pivot param

					g = new OverloadGenerator( "Rectangle" + borderSuffix, callTargets["Rectangle_Internal"], DOC_RECT ) { objectName = "rectangle" };
					g.constAssigns["hollow"] = border.ToString().ToLowerInvariant();

					if( posRot ) {
						g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
						g += rotIdentityNormalQuat;
					}

					if( doRect )
						g += $"Rect rect // {DOC_RECT_RECT}";
					if( sizePivotParams ) {
						string pivot = noPivot ? "RectPivot.Center" : "pivot";
						g += new OrSelectorParams( new[] {
							new[] { new Param( $"Vector2 size // {DOC_RECT_SIZE}", "rect" ) { methodCallStr = $"{pivot}.GetRect( size )" } },
							new[] {
								new Param( $"float width // {DOC_RECT_WIDTH}", "rect" ) { methodCallStr = $"{pivot}.GetRect( width, height )" },
								new Param( $"float height // {DOC_RECT_HEIGHT}" ) // a bit of a hack but it's fiiiine, this shouuld go ignored by the target call
							}
						} );
						if( noPivot == false )
							g += new Param( $"RectPivot pivot // {DOC_RECT_PIVOT}" );
					}

					if( border )
						g += $"float thickness // {DOC_RECT_THICKNESS}";
					g += new OrSelectorParams( new[] {
						null,
						new[] { new Param( $"float cornerRadius // {DOC_RECT_CORNER_RADIUS}", "cornerRadii" ) { methodCallStr = "new Vector4( cornerRadius, cornerRadius, cornerRadius, cornerRadius )" } },
						new[] { new Param( $"Vector4 cornerRadii // {DOC_RECT_CORNER_RADII}" ) },
					} );
					g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
					g.GenerateAndAppend( lines );
				}
			}

			// Triangle / TriangleBorder
			foreach( ( bool border, string borderSuffix ) in new[] { ( false, "" ), ( true, "Border" ) } ) {
				g = new OverloadGenerator( "Triangle" + borderSuffix, callTargets["Triangle_Internal"], DOC_TRI ) { objectName = "triangle" };
				g.constAssigns["hollow"] = border.ToString().ToLowerInvariant();

				g += $"Vector3 a // {DOC_A_POS}";
				g += $"Vector3 b // {DOC_B_POS}";
				g += $"Vector3 c // {DOC_C_POS}";

				Param rpRoundness = new Param( $"float roundness // {DOC_ROUNDNESS}" );

				if( border ) {
					Param rpThickness = new Param( $"float thickness // {DOC_BORDER_THICKNESS}" );
					g += new OrSelectorParams(
						null,
						new[] { rpThickness },
						new[] { rpThickness, rpRoundness }
					);
				} else {
					g += new OrSelectorParams(
						null,
						new[] { rpRoundness }
					);
				}

				g += new OrSelectorParams(
					null,
					new[] { new Param( $"Color color // {DOC_COLOR}", "colorA", "colorB", "colorC" ) },
					new Param[] { $"Color colorA // {DOC_A_COLOR}", $"Color colorB // {DOC_B_COLOR}", $"Color colorC // {DOC_C_COLOR}" }
				);
				g.GenerateAndAppend( lines );
			}

			// Quad
			g = new OverloadGenerator( "Quad", callTargets["Quad_Internal"], DOC_QUAD );
			g += $"Vector3 a // {DOC_A_POS}";
			g += $"Vector3 b // {DOC_B_POS}";
			g += $"Vector3 c // {DOC_C_POS}";
			g += new CombinatorialParams( $"Vector3 d // {DOC_D_POS}" );
			g += new OrSelectorParams(
				null,
				new[] { new Param( $"Color color // {DOC_COLOR}", "colorA", "colorB", "colorC", "colorD" ) },
				new Param[] { $"Color colorA // {DOC_A_COLOR}", $"Color colorB // {DOC_B_COLOR}", $"Color colorC // {DOC_C_COLOR}", $"Color colorD // {DOC_D_COLOR}" }
			);
			g.GenerateAndAppend( lines );

			// Sphere
			foreach( bool usePositioning in bools ) {
				g = new OverloadGenerator( "Sphere", callTargets["Sphere_Internal"], DOC_SPHERE );
				if( usePositioning )
					g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
				g += new CombinatorialParams( $"float radius // {DOC_RADIUS}", $"Color color // {DOC_COLOR}" );
				g.GenerateAndAppend( lines );
			}

			// Cuboid / Cube
			foreach( ( bool cube, string name ) in new[] { ( false, "Cuboid" ), ( true, "Cube" ) } ) {
				foreach( bool usePositioning in bools ) {
					g = new OverloadGenerator( name, callTargets["Cuboid_Internal"], cube ? DOC_CUBE : DOC_CUBOID );
					if( usePositioning ) {
						g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
						g += rotIdentityNormalQuat;
					}

					if( cube )
						g += new Param( $"float size // {DOC_CUBE_SIZE}" ) { methodCallStr = "new Vector3( size, size, size )" };
					else
						g += $"Vector3 size // {DOC_CUBOID_SIZE}";

					g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
					g.GenerateAndAppend( lines );
				}
			}

			// Cone
			foreach( bool usePositioning in bools ) {
				g = new OverloadGenerator( "Cone", callTargets["Cone_Internal"], DOC_CONE );
				if( usePositioning ) {
					g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
					g += rotIdentityNormalQuat;
				}

				g += $"float radius // {DOC_CONE_RADIUS}";
				g += $"float length // {DOC_CONE_LENGTH}";
				g += new CombinatorialParams( $"bool fillCap // {DOC_CONE_CAP}", $"Color color // {DOC_COLOR}" );
				g.GenerateAndAppend( lines );
			}

			// Torus
			bool[] optionalAngleParams = { false, true };
			foreach( bool useAngles in optionalAngleParams ) {
				foreach( bool usePositioning in bools ) {
					g = new OverloadGenerator( "Torus", callTargets["Torus_Internal"], DOC_TORUS );
					if( usePositioning ) {
						g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
						g += rotIdentityNormalQuat;
					}

					g += $"float radius // {DOC_RADIUS}";
					g += $"float thickness // {DOC_THICKNESS}";

					if( useAngles ) {
						g += $"float angleRadStart // {DOC_ANG_START}";
						g += $"float angleRadEnd // {DOC_ANG_END}";
					}

					g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
					g.GenerateAndAppend( lines );
				}
			}

			// Text
			foreach( TextDrawInfo textMode in textDrawModes ) {
				foreach( bool manualElement in bools ) {
					foreach( bool usePositioning in bools ) {
						if( usePositioning && textMode.mode == TextDrawMode.RegionRect )
							continue; // region rects don't use positioning overloads
						g = new OverloadGenerator( textMode.methodNameSelf, callTargets[textMode.methodNameTarget], DOC_TEXT );

						// TextRect_Internal doesn't have the isRect param
						if( textMode.mode != TextDrawMode.RegionRect )
							g.constAssigns["isRect"] = textMode.IsRect.ToString().ToLowerInvariant();

						if( manualElement )
							g += $"TextElement element // {DOC_TEXT_ELEMENT}";

						if( usePositioning && textMode.mode != TextDrawMode.RegionRect ) {
							g += new Param( $"Vector3 pos // {DOC_POS}" ) { mtxFlags = Param.MtxFlags.Position };
							g += new CombinatorialParams( new Param( $"Quaternion rot // {DOC_ROT}" ) { mtxFlags = Param.MtxFlags.Rotation } );
						}

						if( textMode.mode == TextDrawMode.RegionSizePivot ) {
							g += $"Vector2 pivot // {DOC_TEXT_PIVOT}";
							g += $"Vector2 size // {DOC_TEXT_SIZE}";
						} else if( textMode.mode == TextDrawMode.RegionRect ) {
							g += new Param( $"Rect rect // {DOC_TEXT_RECT}" );
						}

						g += $"string content // {DOC_TEXT_CONTENT}";
						g += new CombinatorialParams( $"TextAlign align // {DOC_TEXT_ALIGN}", $"float fontSize // {DOC_TEXT_FONT_SIZE}", $"TMP_FontAsset font // {DOC_TEXT_FONT}", $"Color color // {DOC_COLOR}" );
						g.GenerateAndAppend( lines );
					}
				}
			}

			// Texture
			const int TEX_RECTUVS = 0;
			const int TEX_RECTFILL = 1;
			const int TEX_POSSIZE = 2;
			string[] targetNames = { "Texture_Internal", "Texture_RectFill_Internal", "Texture_PosSize_Internal" };
			for( int i = 0; i < 3; i++ ) {
				g = new OverloadGenerator( "Texture", callTargets[targetNames[i]], DOC_TEXTURE );
				g += $"Texture texture // {DOC_TEXTURE_TEXTURE}";

				if( i == TEX_RECTUVS || i == TEX_RECTFILL ) {
					g += $"Rect rect // {DOC_TEXTURE_RECT}";
					if( i == TEX_RECTUVS )
						g += $"Rect uvs // {DOC_TEXTURE_UVS}";
					else if( i == TEX_RECTFILL )
						g += new CombinatorialParams( $"TextureFillMode fillMode // {DOC_TEXTURE_FILL_MODE}" );
				} else if( i == TEX_POSSIZE ) {
					g += $"Vector2 center // {DOC_TEXTURE_CENTER}";
					g += $"float size // {DOC_TEXTURE_SIZE}";
					g += new CombinatorialParams( $"TextureSizeMode sizeMode // {DOC_TEXTURE_SIZE_MODE}" );
				}

				g += new CombinatorialParams( $"Color color // {DOC_COLOR}" );
				g.GenerateAndAppend( lines );
			}

			return lines;
		}


	}

}