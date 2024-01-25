#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Linq;
#if SHAPES_URP
using UnityEditor.Rendering.Universal;
#if UNITY_2021_2_OR_NEWER
using URP_RND_DATA_EDITOR = UnityEditor.Rendering.Universal.UniversalRendererDataEditor;
#else
using URP_RND_DATA_EDITOR = UnityEditor.Rendering.Universal.ForwardRendererDataEditor;
#endif
#endif
#endif
using UnityEngine;
using UnityEngine.Rendering;
#if SHAPES_URP
using System.Reflection;
using UnityEngine.Rendering.Universal;
#if UNITY_2021_2_OR_NEWER
using URP_RND_DATA = UnityEngine.Rendering.Universal.UniversalRendererData;

#else
using URP_RND_DATA = UnityEngine.Rendering.Universal.ForwardRendererData;
#endif
#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShapesImportState : ScriptableObject {

		[Header( "Do not edit this~" )]
		[SerializeField]
		internal RenderPipeline currentShaderRP = RenderPipeline.Legacy;

		static ShapesImportState instance;
		public static ShapesImportState Instance => instance != null ? instance : instance = Resources.Load<ShapesImportState>( "Shapes Import State" );

		#if UNITY_EDITOR
		[DidReloadScripts( 1 )]
		public static void CheckRenderPipelineSoon() {
			if( ShapesConfig.Instance != null && ShapesConfig.Instance.autoConfigureRenderPipeline )
				EditorApplication.delayCall += AutoCheckRenderPipeline;
		}

		static void AutoCheckRenderPipeline() {
			RenderPipeline rpInUnity = UnityInfo.GetCurrentRenderPipelineInUse();
			ShapesImportState inst = Instance;
			if( inst == null ) {
				Debug.LogWarning( "Failed to get import state - Shapes will retry on the next script recompile" );
				return; // I guess some weird import order shenan happened? :c
			}

			// make sure we have a valid RP state
			bool valid = true;
			string error = "";
			RenderPipeline rpShaders = Instance.currentShaderRP;
			if( rpInUnity != rpShaders ) {
				valid = false;
				error += $" • Shape's shaders are compiled for {rpShaders.PrettyName()}\n";
			}

			if( TryGetPreprocessorRP( out RenderPipeline rpPreproc ) ) {
				if( rpInUnity != rpPreproc ) {
					valid = false;
					error += $" • The project keywords are set up for {rpPreproc.PrettyName()}\n";
				}
			} else {
				valid = false;
				error += $" • The project keywords are incorrectly set to both HDRP and URP\n";
			}

			if( valid == false ) {
				string desc = $"Shapes detected a mismatch in render pipeline state.\n" +
							  $"It looks like you are using {rpInUnity.PrettyName()}, but:\n{error}" +
							  $"Would you like to recompile Shapes for your render pipeline?\n(Shapes may not work if you don't)\n\n" +
							  $"Note: You disable this auto-checker in the Shapes settings";

				if( EditorUtility.DisplayDialog( "Render pipeline mismatch", desc, $"Recompile for {rpInUnity.PrettyName()}", "cancel" ) ) {
					ForceSetRP( rpInUnity );
				}
			}
		}

		internal static void ForceSetRP( RenderPipeline targetRP ) {
			Debug.Log( $"Shapes is recompiling for {targetRP.PrettyName()}..." );
			ForceSetRpFirstPass( targetRP );
			EditorApplication.delayCall += () => {
				ForceSetRpSecondPass( targetRP );
				Debug.Log( $"Shapes is done recompiling" );
			};
		}

		static void ForceSetRpFirstPass( RenderPipeline targetRP ) {
			// set up preprocessor defines, this will also require a second pass of this whole method
			SetPreprocessorRpSymbols( targetRP );
		}

		static void ForceSetRpSecondPass( RenderPipeline targetRP ) {
			// makes sure all shaders are compiled to a specific render pipeline
			RenderPipeline rpShapesShaders = Instance.currentShaderRP;
			if( rpShapesShaders != targetRP )
				CodegenShaders.GenerateShadersAndMaterials( targetRP );

			if( targetRP == RenderPipeline.URP ) {
				string msg = "In order for immediate mode drawing to work, URP render data needs the shapes render features. Would you like to open Shapes settings to make sure immediate mode drawing is supported?";
				if( EditorUtility.DisplayDialog( "URP render features", msg, "yes", "no, I don't need IM drawing" ) )
					MenuItems.OpenCsharpSettings();
			}

			// also on second pass
			MakeSureSampleMaterialsAreValid();
		}


		static void MakeSureSampleMaterialsAreValid() {
			#if SHAPES_URP || SHAPES_HDRP
			#if UNITY_2019_1_OR_NEWER
			Shader targetShader = GraphicsSettings.renderPipelineAsset.defaultShader;
			#else
			Shader targetShader = GraphicsSettings.renderPipelineAsset.GetDefaultShader();
			#endif
			#else
			Shader targetShader = UIAssets.Instance.birpDefaultShader;
			#endif

			bool changed = false;
			if( ShapesIO.TryMakeAssetsEditable( UIAssets.Instance.sampleMaterials ) ) { // ensures version control allows us to edit
				foreach( var mat in UIAssets.Instance.sampleMaterials ) {
					if( mat == null )
						continue; // samples were probably not imported into this project (or they were deleted) if this is null
					if( mat.shader != targetShader ) {
						Undo.RecordObject( mat, "Shapes update sample materials shaders" );
						Color color = GetMainColor( mat );
						mat.shader = targetShader;
						#if SHAPES_URP || SHAPES_HDRP
						mat.SetColor( ShapesMaterialUtils.propBaseColor, color );
						#else
							mat.SetColor( ShapesMaterialUtils.propColor, color );
						#endif
						changed = true;
					}
				}
			}

			if( changed )
				Debug.Log( "Shapes updated sample material shaders to match your current render pipeline" );
		}

		static Color GetMainColor( Material mat ) {
			if( mat.HasProperty( ShapesMaterialUtils.propColor ) ) return mat.GetColor( ShapesMaterialUtils.propColor );
			if( mat.HasProperty( ShapesMaterialUtils.propBaseColor ) ) return mat.GetColor( ShapesMaterialUtils.propBaseColor );
			return Color.white;
		}


		#if SHAPES_URP
		/* this is pretty cursed, I'm commenting this out for now.
		static class UrpRndFuncs {
			const BindingFlags bfs = BindingFlags.Instance | BindingFlags.NonPublic;
			public static readonly FieldInfo fRndDataList = typeof(UniversalRenderPipelineAsset).GetField( "m_RendererDataList", bfs );
			public static readonly MethodInfo fAddComponent = typeof(ScriptableRendererDataEditor).GetMethod( "AddComponent", bfs );
			public static readonly MethodInfo fOnEnable = typeof(ScriptableRendererDataEditor).GetMethod( "OnEnable", bfs );
			public static readonly bool successfullyLoaded = fRndDataList != null && fAddComponent != null && fOnEnable != null;

			public static readonly string failMessage = $"Unity's URP API seems to have changed. Failed to load: " +
														$"{( fRndDataList == null ? "UniversalRenderPipelineAsset.m_RendererDataList" : "" )} " +
														$"{( fAddComponent == null ? "ScriptableRendererDataEditor.AddComponent" : "" )} " +
														$"{( fOnEnable == null ? "ScriptableRendererDataEditor.OnEnable" : "" )}";
		}

		static void EnsureShapesPassExistsInTheUrpRenderer() {
			if( UrpRndFuncs.successfullyLoaded ) { // if our reflected members failed to load, we're kinda screwed :c
				if( GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset urpa ) { // find the URP asset
					ScriptableRendererData[] srd = (ScriptableRendererData[])UrpRndFuncs.fRndDataList.GetValue( urpa );
					foreach( var rndd in srd.Where( x => x is URP_RND_DATA ) ) { // only add to forward renderer
						if( rndd.rendererFeatures.Any( x => x is ShapesRenderFeature ) == false ) { // does it have Shapes?
							// does not contain the Shapes render feature, so, oh boy, here we go~
							if( ShapesIO.TryMakeAssetsEditable( urpa ) ) {
								URP_RND_DATA_EDITOR fwEditor = (URP_RND_DATA_EDITOR)Editor.CreateEditor( rndd );
								UrpRndFuncs.fOnEnable.Invoke( fwEditor, null ); // you ever just call OnEnable manually
								UrpRndFuncs.fAddComponent.Invoke( fwEditor, new[] { (object)nameof(ShapesRenderFeature) } );
								DestroyImmediate( fwEditor ); // luv 2 create temporary editors
								Debug.Log( $"Added Shapes renderer feature to {rndd.name}", rndd );
							}
						}
					}
				} else
					Debug.LogWarning( $"Shapes failed to load the URP pipeline asset to add the renderer feature. " +
									  $"You might have to add {nameof(ShapesRenderFeature)} to your renderer asset manually" );
			} else
				Debug.LogError( UrpRndFuncs.failMessage );
		}*/

		#endif

		public static List<string> GetCurrentKeywords() => PlayerSettings.GetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup ).Split( ';' ).ToList();
		static void SetCurrentKeywords( IEnumerable<string> keywords ) => PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, string.Join( ";", keywords ) );

		internal static bool TryGetPreprocessorRP( out RenderPipeline rp ) {
			List<string> keywords = GetCurrentKeywords();
			bool kwURP = keywords.Contains( RenderPipeline.URP.PreprocessorDefineName() );
			bool kwHDRP = keywords.Contains( RenderPipeline.HDRP.PreprocessorDefineName() );
			rp = default;
			if( kwURP && !kwHDRP )
				rp = RenderPipeline.URP;
			else if( kwHDRP && !kwURP )
				rp = RenderPipeline.HDRP;
			else if( !kwHDRP && !kwURP )
				rp = RenderPipeline.Legacy;
			else
				return false;
			return true;
		}


		static void SetPreprocessorRpSymbols( RenderPipeline rpTarget ) {
			Debug.Log( $"Setting preprocessor symbols for {rpTarget.PrettyName()}" );
			List<string> symbols = GetCurrentKeywords();

			bool changed = false;

			void CheckRpSymbol( RenderPipeline rp ) {
				bool on = rp == rpTarget;
				string ppName = rp.PreprocessorDefineName();
				if( on && symbols.Contains( ppName ) == false ) {
					symbols.Add( ppName );
					changed = true;
				} else if( on == false && symbols.Remove( ppName ) )
					changed = true;
			}

			CheckRpSymbol( RenderPipeline.URP );
			CheckRpSymbol( RenderPipeline.HDRP );

			if( changed && ShapesIO.TryMakeAssetsEditable( ShapesIO.projectSettingsPath ) ) {
				//Debug.Log( $"Shapes updated your project scripting define symbols since you seem to be using {rpTarget.PrettyName()}, I hope that's okay~" );
				SetCurrentKeywords( symbols );
			}
		}

		#endif
	}

}