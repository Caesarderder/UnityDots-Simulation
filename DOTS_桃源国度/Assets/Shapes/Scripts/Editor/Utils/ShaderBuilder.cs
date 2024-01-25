using System;
using System.Collections.Generic;
using System.Linq;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	struct ShaderTag {
		string key, value;
		public static implicit operator ShaderTag( (string key, string value) noot ) => new ShaderTag() { key = noot.key, value = noot.value };
		public static implicit operator string( ShaderTag st ) => st.ToString();
		public override string ToString() => $"\"{key}\" = \"{value}\"";
	}

	internal class ShaderBuilder {

		public string shader;

		// all multi_compiles are defined here
		public static Dictionary<string, MultiCompile[]> shaderKeywords = new Dictionary<string, MultiCompile[]> {
			{ "Disc", new[] { new MultiCompile( "INNER_RADIUS" ), new MultiCompile( "SECTOR" ) } },
			{ "Line 2D", new[] { new MultiCompile( "CAP_ROUND", "CAP_SQUARE" ) } },
			{ "Line 3D", new[] { new MultiCompile( "CAP_ROUND", "CAP_SQUARE" ) } },
			{ "Polyline 2D", new[] { new MultiCompile( "IS_JOIN_MESH" ), new MultiCompile( "JOIN_MITER", "JOIN_ROUND", "JOIN_BEVEL" ) /*, new MultiCompile( "CAP_ROUND", "CAP_SQUARE" )*/ } },
			{ "Rect", new[] { new MultiCompile( "CORNER_RADIUS" ), new MultiCompile( "BORDERED" ) } }
		};

		const char INDENT_STR = '\t';
		int indentLevel = 0;
		ShapesBlendMode blendMode;
		string shaderName;

		public struct BracketScope : IDisposable {
			ShaderBuilder builder;

			public BracketScope( ShaderBuilder builder, string line ) {
				this.builder = builder;
				builder.AppendLine( line + " {" );
				builder.indentLevel++;
			}

			public void Dispose() {
				builder.indentLevel--;
				builder.AppendLine( "}" );
			}
		}

		public BracketScope Scope( string line ) => new BracketScope( this, line );

		public ShaderBuilder( string name, ShapesBlendMode blendMode, RenderPipeline rp ) {
			this.blendMode = blendMode;
			this.shaderName = name;

			using( Scope( $"Shader \"Shapes/{name} {blendMode.ToString()}\"" ) ) {
				using( Scope( "Properties" ) ) {
					AppendLine( "[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest (\"Z Test\", int) = 4" );
					AppendLine( "_ZOffsetFactor (\"Z Offset Factor\", Float ) = 0" );
					AppendLine( "_ZOffsetUnits (\"Z Offset Units\", int ) = 0" );

					AppendLine( "[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp (\"Stencil Comparison\", int) = 8" );
					AppendLine( "[Enum(UnityEngine.Rendering.StencilOp)] _StencilOpPass (\"Stencil Operation Pass\", int) = 0" );
					AppendLine( "_StencilID (\"Stencil ID\", int) = 0" );
					AppendLine( "_StencilReadMask (\"Stencil Read Mask\", int) = 255" );
					AppendLine( "_StencilWriteMask (\"Stencil Write Mask\", int) = 255" );
					AppendLine( "_ColorMask (\"Color Mask\", int) = 15" );
					if( name == "Texture" )
						AppendLine( "_MainTex (\"Texture\", 2D) = \"white\" {}" );
				}

				using( Scope( "SubShader" ) ) {
					using( Scope( "Tags" ) ) { // subshader tags
						AppendLines( rp.GetSubshaderTags() );
						AppendLines( blendMode.GetSubshaderTags() );
					}

					AppendPass( ShaderPassType.Render, rp );
					if( rp != RenderPipeline.Legacy )
						AppendPass( ShaderPassType.DepthOnly, rp );
					AppendPass( ShaderPassType.Picking, rp );
					AppendPass( ShaderPassType.Selection, rp );
				}
			}
		}

		void AppendPass( ShaderPassType pass, RenderPipeline rp ) {
			using( Scope( "Pass" ) ) {
				// Name & LightMode
				bool isLegacyMainRenderPass = rp == RenderPipeline.Legacy && pass == ShaderPassType.Render;
				if( isLegacyMainRenderPass == false ) {
					( string passName, string lightMode ) = pass.NameAndLightMode( rp );
					AppendLine( $"Name \"{passName}\"" );
					AppendLine( "Tags { " + (ShaderTag)( "LightMode", lightMode ) + " }" );
				}

				using( Scope( "Stencil" ) ) {
					AppendLine( "Comp [_StencilComp]" );
					AppendLine( "Pass [_StencilOpPass]" );
					AppendLine( "Ref [_StencilID]" );
					AppendLine( "ReadMask [_StencilReadMask]" );
					AppendLine( "WriteMask [_StencilWriteMask]" );
				}

				// culling/blend mode etc
				if( pass == ShaderPassType.Render ) {
					AppendLines( blendMode.GetPassRenderStates() );
				} else
					AppendLine( "Cull Off" ); // todo: might be incorrect for DepthOnly

				// hlsl program
				AppendHlslProgram( pass, rp );
			}
		}


		void AppendHlslProgram( ShaderPassType passType, RenderPipeline rp ) {
			AppendLine( "HLSLPROGRAM" );
			indentLevel++;
			AppendLine( "#pragma vertex vert" );
			AppendLine( "#pragma fragment frag" );
			AppendLine( "#pragma multi_compile_fog" );
			AppendLine( "#pragma multi_compile_instancing" );
			if( rp != RenderPipeline.Legacy ) {
				AppendLine( "#pragma prefer_hlslcc gles" );
				AppendLine( "#pragma exclude_renderers d3d11_9x" );
				AppendLine( "#pragma target 2.0" );
			}

			if( shaderKeywords.ContainsKey( shaderName ) )
				AppendLines( shaderKeywords[shaderName].Select( x => x.ToString() ) );
			AppendLine( $"#define {blendMode.BlendShaderDefine()}" );
			if( passType == ShaderPassType.Picking || passType == ShaderPassType.Selection ) {
				AppendLine( "#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap" );
				if( passType == ShaderPassType.Picking )
					AppendLine( "#define SCENE_VIEW_PICKING" );
				else if( passType == ShaderPassType.Selection )
					AppendLine( "#define SCENE_VIEW_OUTLINE_MASK" );
			}

			AppendLine( $"#include \"../../Core/{shaderName} Core.cginc\"" );
			indentLevel--;
			AppendLine( "ENDHLSL" );
		}


		string GetIndentation() => new string( INDENT_STR, indentLevel );
		void AppendLine( string s ) => shader += $"{GetIndentation()}{s}\n";

		void AppendLines( IEnumerable<string> strings ) {
			foreach( string s in strings )
				AppendLine( s );
		}

		void BeginScope( string line ) {
			AppendLine( line );
			indentLevel++;
		}

		void EndScope() {
			indentLevel--;
			AppendLine( "}" );
		}


	}

}