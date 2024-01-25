using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class IMMaterialPool {

		public static Dictionary<RenderState, Material> pool = new Dictionary<RenderState, Material>();

		static IMMaterialPool() {
			// materials magically go null here so we have to flush if any scenes are unloaded 
			SceneManager.sceneUnloaded += scene => FlushAllMaterials();
			#if UNITY_EDITOR
			EditorSceneManager.sceneClosed += scene => FlushAllMaterials();
			EditorSceneManager.activeSceneChangedInEditMode += ( sceneA, sceneB ) => FlushAllMaterials();
			EditorSceneManager.newSceneCreated += ( scene, setup, mode ) => FlushAllMaterials();
			AssemblyReloadEvents.beforeAssemblyReload += FlushAllMaterials;
			#endif
		}

		#if UNITY_EDITOR
		static bool wasPlaying;
		#endif

		internal static Material GetMaterial( ref RenderState state ) {
			#if UNITY_EDITOR
			// very cursed, but we can't use the "leaving play mode" events for this because Unity destroys play mode materials at a fucky time
			if( wasPlaying != EditorApplication.isPlaying ) {
				FlushAllMaterials();
				wasPlaying = EditorApplication.isPlaying;
			}
			#endif

			if( pool.TryGetValue( state, out Material mat ) == false )
				pool.Add( state, mat = state.CreateMaterial() );

			return mat;
		}

		static void FlushAllMaterials() {
			foreach( Material mat in pool.Values ) {
				if( mat != null )
					mat.DestroyBranched();
			}

			pool.Clear();
		}

	}

}