using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Shapes;
using UnityEditor;

public static class CodegenInterfaces {

	static CodeWriter code;

	public static void Generate() {
		// find all component-based Shapes with interfaces
		Assembly asm = Assembly.Load( "ShapesRuntime" );
		var allShapes = asm.DefinedTypes.Where(
			type => type.IsAbstract == false && ( type.BaseType == typeof(ShapeRenderer) || type.BaseType?.BaseType == typeof(ShapeRenderer) )
		).OrderBy( type => type.FullName );
		var shapesWithInterfaces = allShapes.Where( sh => sh.ImplementedInterfaces.Contains( typeof(IDashable) ) || sh.ImplementedInterfaces.Contains( typeof(IFillable) ) );

		// generate code
		code = new CodeWriter();
		using( code.MainScope( typeof(CodegenInterfaces), "UnityEngine" ) ) {
			foreach( var shape in shapesWithInterfaces ) {
				code.Spacer();
				using( code.Scope( $"public partial class {shape.Name}" ) ) {
					if( shape.ImplementedInterfaces.Contains( typeof(IDashable) ) )
						AppendDashLines( shape );
					if( shape.ImplementedInterfaces.Contains( typeof(IFillable) ) )
						AppendFillLines();
				}
			}
		}

		// write to file
		string path = ShapesIO.RootFolder + "/Scripts/Runtime/Interfaces/GeneratedInterfaceImplementations.cs";
		code.WriteTo( path );
	}

	static void Append( string s ) => code.Append( s );

	const string DD_LINE = nameof(DashStyle) + "." + nameof(DashStyle.defaultDashStyleLine);
	const string DD_RING = nameof(DashStyle) + "." + nameof(DashStyle.defaultDashStyleRing);
	static string GetDefaultDashStyle( TypeInfo type ) => type.AsType() == typeof(Line) ? DD_LINE : DD_RING;
	static string GetDashTypeCondition( TypeInfo type ) => type.AsType() == typeof(Line) ? "Geometry != LineGeometry.Volumetric3D" : "setType: true";


	static void AppendDashLines( TypeInfo type ) {
		Append( "[SerializeField] bool matchDashSpacingToSize = true;" );
		Append( "public bool MatchDashSpacingToSize {" );
		Append( "\tget => matchDashSpacingToSize;" );
		Append( "\tset {" );
		Append( "\t\tmatchDashSpacingToSize = value;" );
		Append( "\t\tSetAllDashValues( now: true );" );
		Append( "\t}" );
		Append( "}" );
		Append( "[SerializeField] bool dashed = false;" );
		Append( "public bool Dashed {" );
		Append( "\tget => dashed;" );
		Append( "\tset {" );
		Append( "\t\tdashed = value;" );
		Append( "\t\tSetAllDashValues( now: true );" );
		Append( "\t}" );
		Append( "}" );
		Append( $"[SerializeField] DashStyle dashStyle = {GetDefaultDashStyle( type )};" );
		Append( "public float DashSize {" );
		Append( "\tget => dashStyle.size;" );
		Append( "\tset {" );
		Append( "\t\tdashStyle.size = value;" );
		Append( "\t\tfloat netDashSize = dashStyle.GetNetAbsoluteSize( dashed, thickness );" );
		Append( "\t\tif( matchDashSpacingToSize )" );
		Append( "\t\t\tSetFloat( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing() );" );
		Append( "\t\tSetFloatNow( ShapesMaterialUtils.propDashSize, netDashSize );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public float DashSpacing {" );
		Append( "\tget => matchDashSpacingToSize ? dashStyle.size : dashStyle.spacing;" );
		Append( "\tset {" );
		Append( "\t\tdashStyle.spacing = value;" );
		Append( "\t\tSetFloatNow( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing() );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public float DashOffset {" );
		Append( "\tget => dashStyle.offset;" );
		Append( "\tset => SetFloatNow( ShapesMaterialUtils.propDashOffset, dashStyle.offset = value );" );
		Append( "}" );
		Append( "public DashSpace DashSpace {" );
		Append( "\tget => dashStyle.space;" );
		Append( "\tset {" );
		Append( "\t\tSetInt( ShapesMaterialUtils.propDashSpace, (int)( dashStyle.space = value ) );" );
		Append( "\t\tSetFloatNow( ShapesMaterialUtils.propDashSize, dashStyle.GetNetAbsoluteSize( dashed, thickness ) );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public DashSnapping DashSnap {" );
		Append( "\tget => dashStyle.snap;" );
		Append( "\tset => SetIntNow( ShapesMaterialUtils.propDashSnap, (int)( dashStyle.snap = value ) );" );
		Append( "}" );
		Append( "public DashType DashType {" );
		Append( "\tget => dashStyle.type;" );
		Append( "\tset => SetIntNow( ShapesMaterialUtils.propDashType, (int)( dashStyle.type = value ) );" );
		Append( "}" );
		Append( "public float DashShapeModifier {" );
		Append( "\tget => dashStyle.shapeModifier;" );
		Append( "\tset => SetFloatNow( ShapesMaterialUtils.propDashShapeModifier, dashStyle.shapeModifier = value );" );
		Append( "}" );
		Append( $"void SetAllDashValues( bool now ) => SetAllDashValues( dashStyle, Dashed, matchDashSpacingToSize, thickness, {GetDashTypeCondition( type )}, now );" );
		Append( "float GetNetDashSpacing() => GetNetDashSpacing( dashStyle, dashed, matchDashSpacingToSize, thickness );" );
	}


	static void AppendFillLines() {
		Append( "[SerializeField] private protected GradientFill fill = GradientFill.defaultFill;" );
		Append( "[SerializeField] private protected bool useFill;" );
		Append( "public GradientFill Fill {" );
		Append( "\tget => fill;" );
		Append( "\tset {" );
		Append( "\t\tfill = value;" );
		Append( "\t\tSetFillProperties();" );
		Append( "\t}" );
		Append( "}" );
		Append( "public bool UseFill {" );
		Append( "\tget => useFill;" );
		Append( "\tset {" );
		Append( "\t\tuseFill = value;" );
		Append( "\t\tSetIntNow( ShapesMaterialUtils.propFillType, fill.GetShaderFillTypeInt( useFill ) );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public FillType FillType {" );
		Append( "\tget => fill.type;" );
		Append( "\tset {" );
		Append( "\t\tfill.type = value;" );
		Append( "\t\tSetIntNow( ShapesMaterialUtils.propFillType, fill.GetShaderFillTypeInt( useFill ) );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public FillSpace FillSpace {" );
		Append( "\tget => fill.space;" );
		Append( "\tset => SetIntNow( ShapesMaterialUtils.propFillSpace, (int)( fill.space = value ) );" );
		Append( "}" );
		Append( "public Vector3 FillRadialOrigin {" );
		Append( "\tget => fill.radialOrigin;" );
		Append( "\tset {" );
		Append( "\t\tfill.radialOrigin = value;" );
		Append( "\t\tSetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public float FillRadialRadius {" );
		Append( "\tget => fill.radialRadius;" );
		Append( "\tset {" );
		Append( "\t\tfill.radialRadius = value;" );
		Append( "\t\tSetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public Vector3 FillLinearStart {" );
		Append( "\tget => fill.linearStart;" );
		Append( "\tset {" );
		Append( "\t\tfill.linearStart = value;" );
		Append( "\t\tSetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );" );
		Append( "\t}" );
		Append( "}" );
		Append( "public Vector3 FillLinearEnd {" );
		Append( "\tget => fill.linearEnd;" );
		Append( "\tset => SetVector3Now( ShapesMaterialUtils.propFillEnd, fill.linearEnd = value );" );
		Append( "}" );
		Append( "public Color FillColorStart {" );
		Append( "\tget => fill.colorStart;" );
		Append( "\tset => SetColorNow( ShapesMaterialUtils.propColor, fill.colorStart = value );" );
		Append( "}" );
		Append( "public Color FillColorEnd {" );
		Append( "\tget => fill.colorEnd;" );
		Append( "\tset => SetColorNow( ShapesMaterialUtils.propColorEnd, fill.colorEnd = value );" );
		Append( "}" );
		Append( "" );
		Append( "private void SetFillProperties() {" );
		Append( "\tif( useFill ) {" );
		Append( "\t\tSetInt( ShapesMaterialUtils.propFillSpace, (int)fill.space );" );
		Append( "\t\tSetVector4( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );" );
		Append( "\t\tSetVector3( ShapesMaterialUtils.propFillEnd, fill.linearEnd );" );
		Append( "\t\tSetColor( ShapesMaterialUtils.propColor, fill.colorStart );" );
		Append( "\t\tSetColor( ShapesMaterialUtils.propColorEnd, fill.colorEnd );" );
		Append( "\t}" );
		Append( "" );
		Append( "\tSetInt( ShapesMaterialUtils.propFillType, fill.GetShaderFillTypeInt( useFill ) );" );
		Append( "}" );
	}

}