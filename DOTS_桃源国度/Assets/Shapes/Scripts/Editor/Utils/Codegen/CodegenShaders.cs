using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public struct MultiCompile {
		string[] keywords;
		public int Count => keywords.Length + 1; // include 0th;
		public string this[ int i ] => i == 0 ? "" : keywords[i - 1];
		public MultiCompile( params string[] keywords ) => this.keywords = keywords;
		public override string ToString() => $"#pragma multi_compile __ {string.Join( " ", keywords )}";

		public IEnumerable<string> Enumerate() {
			for( int i = 0; i < Count; i++ )
				yield return this[i];
		}
	}

	public static class CodegenShaders {

		internal static class StaticInit {
			public static string[] shaderNames = ShapesIO.LoadCoreShaders().Select( x => x.name.Substring( 0, x.name.Length - " Core".Length ) ).ToArray();
		}

		class PathContent {
			public string path;
			public string content;
			public PathContent( string path, string content ) => ( this.path, this.content ) = ( path, content );
		}

		class PathMaterial {
			public string path;
			public Material mat;
			public Shader shader;
			public string[] keywords;
			public PathMaterial( string path, Material mat, Shader shader, string[] keywords ) => ( this.path, this.mat, this.shader, this.keywords ) = ( path, mat, shader, keywords );

			public void GenerateOrUpdate() {
				if( mat != null ) {
					EditorUtility.SetDirty( mat );
					mat.shader = shader;
					mat.hideFlags = HideFlags.HideInInspector;
					TrySetKeywordsAndDefaultProperties( keywords, mat );
				} else {
					Debug.Log( "creating material " + path );
					mat = new Material( shader ) { enableInstancing = true, hideFlags = HideFlags.HideInInspector };
					TrySetKeywordsAndDefaultProperties( keywords, mat );
					AssetDatabase.CreateAsset( mat, path );
				}
			}
		}

		static int blendModeCount = System.Enum.GetNames( typeof(ShapesBlendMode) ).Length;

		internal static void GenerateShadersAndMaterials( RenderPipeline targetRP ) {
			Debug.Log( $"Regenerating shaders for {targetRP.PrettyName()}" );

			// check if we need to update rp state
			bool writeTargetRpToImportState = ShapesImportState.Instance.currentShaderRP != targetRP;

			// generate all shader paths & content
			List<PathContent> shaderPathContents = GetShaderPathContents();
			List<PathMaterial> pathMaterials = GetMaterialPathContents();

			// tally up all files we need to edit to make version control happy
			List<string> filesToUnlock = new List<string>();
			if( writeTargetRpToImportState ) filesToUnlock.Add( AssetDatabase.GetAssetPath( ShapesImportState.Instance ) );
			filesToUnlock.AddRange( shaderPathContents.Select( x => x.path ) );
			filesToUnlock.AddRange( pathMaterials.Where( x => x.mat != null ).Select( x => x.path ) );

			// try to make sure they can be edited, then write all data
			if( ShapesIO.TryMakeAssetsEditable( filesToUnlock.ToArray() ) ) {
				shaderPathContents.ForEach( pc => File.WriteAllText( pc.path, pc.content ) ); // write all shaders
				AssetDatabase.Refresh( ImportAssetOptions.Default ); // reimport all assets to load newly generated shaders
				pathMaterials.ForEach( x => x.GenerateOrUpdate() ); // generate all materials
				if( writeTargetRpToImportState ) { // update the current shader state
					ShapesImportState.Instance.currentShaderRP = targetRP;
					EditorUtility.SetDirty( ShapesImportState.Instance );
				}

				AssetDatabase.Refresh( ImportAssetOptions.Default ); // reimport stuff
			}
		}

		static List<PathContent> GetShaderPathContents() {
			RenderPipeline rp = UnityInfo.GetCurrentRenderPipelineInUse();
			List<PathContent> pcs = new List<PathContent>();
			foreach( string name in StaticInit.shaderNames ) {
				for( int i = 0; i < blendModeCount; i++ ) {
					ShapesBlendMode blendMode = (ShapesBlendMode)i;
					string path = $"{ShapesIO.GeneratedShadersFolder}/{name} {blendMode}.shader";
					string shaderContents = new ShaderBuilder( name, blendMode, rp ).shader;
					pcs.Add( new PathContent( path, shaderContents ) );
				}
			}

			return pcs;
		}

		static List<PathMaterial> GetMaterialPathContents() {
			List<PathMaterial> pathMaterials = new List<PathMaterial>();
			foreach( string name in StaticInit.shaderNames ) {
				for( int i = 0; i < blendModeCount; i++ ) {
					ShapesBlendMode blendMode = (ShapesBlendMode)i;
					string nameWithBlendMode = ShapesMaterials.GetMaterialName( name, blendMode.ToString() );
					Shader shader = Shader.Find( $"Shapes/{nameWithBlendMode}" );
					if( shader == null ) {
						Debug.LogError( "missing shader " + $"Shapes/{nameWithBlendMode}" );
						continue;
					}

					if( ShaderBuilder.shaderKeywords.ContainsKey( name ) ) {
						// create all permutations
						MultiCompile[] multis = ShaderBuilder.shaderKeywords[name];
						List<string> keywordPermutations = new List<string>();
						foreach( IEnumerable<string> perm in GetPermutations( multis.Select( m => m.Enumerate() ) ) ) {
							string[] validKeywords = perm.Where( p => string.IsNullOrEmpty( p ) == false ).ToArray();
							string kws = $" [{string.Join( "][", validKeywords )}]";
							if( kws.Contains( "[]" ) ) // this means it has no permutations
								kws = "";
							pathMaterials.Add( GetPathMaterial( nameWithBlendMode + kws, shader, validKeywords ) );
						}
					} else {
						pathMaterials.Add( GetPathMaterial( nameWithBlendMode, shader ) );
					}
				}
			}

			return pathMaterials;
		}

		static PathMaterial GetPathMaterial( string fullMaterialName, Shader shader, string[] keywords = null ) {
			string savePath = $"{ShapesIO.GeneratedMaterialsFolder}/{fullMaterialName}.mat";
			Material mat = AssetDatabase.LoadAssetAtPath<Material>( savePath );
			return new PathMaterial( savePath, mat, shader, keywords );
		}

		static void TrySetKeywordsAndDefaultProperties( IEnumerable<string> keywords, Material mat ) {
			if( keywords != null )
				foreach( string keyword in keywords )
					mat.EnableKeyword( keyword );

			ShapesMaterials.ApplyDefaultGlobalProperties( mat );
		}

		// magic wand wau ✨
		public static IEnumerable<IEnumerable<string>> GetPermutations( IEnumerable<IEnumerable<string>> inputData ) {
			IEnumerable<IEnumerable<string>> emptyProduct = new[] { Enumerable.Empty<string>() };
			return inputData.Aggregate( emptyProduct, ( accumulator, sequence ) => accumulator.SelectMany( accseq => sequence, ( accseq, item ) => accseq.Concat( new[] { item } ) ) );
		}

	}

}