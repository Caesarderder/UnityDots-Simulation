using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class CodegenMpbs {

		static CodeWriter code;
		static Dictionary<string, string> propToVarName;

		const string SIMPLE_TRANSFER = "protected override void TransferShapeProperties() => _ = 0; // :>";

		static string[] excludedShaders = { "Texture Core" }; // manually set up

		public static void Generate() {
			// load core files
			TextAsset[] coreShaders = ShapesIO.LoadCoreShaders()
				.Where( t => excludedShaders.Contains( t.name ) == false )
				.OrderBy( x => x.name )
				.ToArray();

			// Dictionary for (_X) to (ShapesMaterialUtils.propX)
			propToVarName = LoadPropertyNames();

			// generate code
			code = new CodeWriter();
			using( code.MainScope( typeof(CodegenMpbs), "System.Collections.Generic", "UnityEngine" ) ) {
				// main shaders
				coreShaders.ForEach( GenerateMpbClass );
				// text is a special case
				code.Spacer();
				using( code.Scope( "internal class MpbText : MetaMpb" ) )
					code.Append( SIMPLE_TRANSFER );
			}

			// save
			string path = ShapesIO.RootFolder + "/Scripts/Runtime/Immediate Mode/MetaMpbShapes.cs";
			code.WriteTo( path );
		}

		static Dictionary<string, string> LoadPropertyNames() {
			var dict = new Dictionary<string, string>();
			string path = ShapesIO.RootFolder + "/Scripts/Runtime/Utils/ShapesMaterialUtils.cs";

			// manually parse this out I guess~
			foreach( string s in File.ReadAllLines( path ) ) {
				if( s.TrimStart().StartsWith( "public static readonly int prop" ) ) {
					string propNameVar = s.Between( "readonly int ", " = Shader.PropertyToID" );
					string propNameShader = s.Between( "ToID( \"", "\" );" );
					dict[propNameShader] = propNameVar;
				}
			}

			return dict;
		}

		// scuffed regex~
		static string Between( this string s, string pre, string post ) => s.Split( new[] { pre }, StringSplitOptions.None )[1].Split( new[] { post }, StringSplitOptions.None )[0].Trim();


		struct PropertyVar {
			public PropertyVar( string type, string nameShaderProp ) {
				this.type = type;
				this.nameShaderProp = nameShaderProp;
				this.nameCsVar = char.ToLower( nameShaderProp[1] ) + nameShaderProp.Substring( 2 );
				this.nameCsVarProp = propToVarName[nameShaderProp];
			}

			public string type; // Vector4
			public string nameShaderProp; // _Rect
			public string nameCsVar; // rect
			public string nameCsVarProp; // ShapesMaterialUtils.propRect
		}

		struct Property {
			public Property( string type, string name ) {
				this.type = type;
				this.name = name;
				if( type == "Single" )
					this.type = "float";
			}

			public string type; // Vector4
			public string name; // _Rect
		}

		static List<Property> GetInterfaceListProperties<T>() {
			return typeof(T).GetProperties()
				.Select( p => new Property( p.PropertyType.GenericTypeArguments[0].Name, p.Name ) )
				.OrderBy( p => p.name ).ToList();
		}

		static void GenerateMpbClass( TextAsset coreFile ) {
			string path = AssetDatabase.GetAssetPath( coreFile );
			string[] lines = File.ReadAllLines( path );

			bool filled = false;
			bool dashed = false;
			List<PropertyVar> properties = new List<PropertyVar>();
			List<Property> propertiesFill = GetInterfaceListProperties<IFillableMpb>();
			List<Property> propertiesDash = GetInterfaceListProperties<IDashableMpb>();

			// find all properties from the core shader files
			bool propStart = false;
			foreach( var s in lines ) {
				string sTrimmed = s.TrimStart();
				if( propStart ) {
					if( sTrimmed.StartsWith( "SHAPES_FILL_PROPERTIES" ) ) {
						filled = true;
					} else if( sTrimmed.StartsWith( "SHAPES_DASH_PROPERTIES" ) ) {
						dashed = true;
					} else if( sTrimmed.StartsWith( "PROP_DEF(" ) ) {
						string contents = s.Between( "_DEF(", ")" );
						string[] typeAndName = contents.Split( ',' );
						string type = ShaderTypeToCsType( typeAndName[0].Trim() );
						string name = typeAndName[1].Trim();
						if( name != "_Color" ) // skip global properties
							properties.Add( new PropertyVar( type, name ) );
					} else if( sTrimmed.StartsWith( "UNITY_INSTANCING_BUFFER_END" ) )
						break; // done
				} else if( sTrimmed.StartsWith( "UNITY_INSTANCING_BUFFER_START" ) )
					propStart = true;
			}

			string shapeName = coreFile.name.Substring( 0, coreFile.name.Length - " Core".Length );
			string className = "Mpb" + shapeName.Replace( " ", "" );
			string baseTypes = "MetaMpb";
			if( filled ) baseTypes += $", {nameof(IFillableMpb)}";
			if( dashed ) baseTypes += $", {nameof(IDashableMpb)}";

			properties = properties.OrderBy( p => p.nameCsVar ).ToList();

			code.Spacer();
			using( code.Scope( $"internal class {className} : {baseTypes}" ) ) {
				// shape vars
				if( properties.Count > 0 ) {
					code.Spacer();
					foreach( PropertyVar p in properties )
						code.Append( $"internal readonly List<{p.type}> {p.nameCsVar} = InitList<{p.type}>();" );
				}

				// fill/dash properties
				if( filled ) {
					code.Spacer();
					code.Comment( "fill boilerplate" );
					foreach( Property p in propertiesFill )
						code.Append( $"List<{p.type}> {nameof(IFillableMpb)}.{p.name} {{ get; }} = InitList<{p.type}>();" );
				}

				if( dashed ) {
					code.Spacer();
					code.Comment( "dash boilerplate" );
					foreach( Property p in propertiesDash )
						code.Append( $"List<{p.type}> {nameof(IDashableMpb)}.{p.name} {{ get; }} = InitList<{p.type}>();" );
				}

				// transfer function
				code.Spacer();
				if( properties.Count == 0 ) {
					code.Append( "protected override void TransferShapeProperties() => _ = 0; // :>" );
				} else {
					using( code.Scope( "protected override void TransferShapeProperties()" ) ) {
						foreach( PropertyVar p in properties )
							code.Append( $"Transfer( {nameof(ShapesMaterialUtils)}.{p.nameCsVarProp}, {p.nameCsVar} );" );
					}
				}

				code.Spacer();
			}
		}

		static string ShaderTypeToCsType( string t ) {
			switch( t ) {
				case "int":
				case "half":
				case "fixed":
				case "float":
					return "float"; // SetFloat()
				case "int2":
				case "half2":
				case "fixed2":
				case "float2":
				case "int3":
				case "half3":
				case "fixed3":
				case "float3":
				case "int4":
				case "half4":
				case "fixed4":
				case "float4":
					return "Vector4"; // SetVector()
			}

			throw new ArgumentException( "INVALID TYPE: " + t );
		}


	}

}