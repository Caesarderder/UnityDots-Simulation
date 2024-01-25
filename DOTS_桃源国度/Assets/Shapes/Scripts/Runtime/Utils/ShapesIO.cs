using System.Linq;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class ShapesIO {
		#if UNITY_EDITOR

		static string rootFolder = null;
		public static string RootFolder {
			get {
				if( rootFolder == null ) {
					string shapeAssetsPath = AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( "t:ShapesAssets" )[0] );
					int fileNameLen = "/Resources/Shapes Assets.asset".Length;
					rootFolder = shapeAssetsPath.Substring( 0, shapeAssetsPath.Length - fileNameLen );
				}

				return rootFolder;
			}
		}

		static string shaderFolder = null;
		public static string ShaderFolder => shaderFolder ?? ( shaderFolder = RootFolder + "/Shaders" );
		public static string CoreShaderFolder => ShaderFolder + "/Core";
		public static string GeneratedMaterialsFolder => ShaderFolder + "/Generated Materials";
		public static string GeneratedShadersFolder => ShaderFolder + "/Generated Shaders/Resources";
		public static string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";

		static string ConfigCsharpPath => RootFolder + "/Scripts/Runtime/ShapesConfig.cs";
		internal static string ConfigShadersPath => ShaderFolder + "/Shapes Config.cginc";

		public static void OpenConfigCsharp() => OpenAssetAtPath( ConfigCsharpPath );
		public static void OpenConfigShaders() => OpenAssetAtPath( ConfigShadersPath );
		public static TextAsset[] LoadCoreShaders() => LoadAllAssets<TextAsset>( CoreShaderFolder ).ToArray();

		static void OpenAssetAtPath( string path ) => AssetDatabase.OpenAsset( AssetDatabase.LoadAssetAtPath<Object>( path ) );
		internal static bool IsUsingVcWithCheckoutEnabled => Provider.enabled && Provider.hasCheckoutSupport;
		internal static bool AssetCanBeEdited( Object asset ) => AssetDatabase.IsOpenForEdit( asset, StatusQueryOptions.UseCachedIfPossible );
		internal static bool AssetCanBeEdited( string path ) => AssetDatabase.IsOpenForEdit( path, StatusQueryOptions.UseCachedIfPossible );
		internal static bool AssetsCanBeEdited( params Object[] assets ) => assets.All( AssetCanBeEdited );
		internal static bool AssetsCanBeEdited( params string[] assets ) => assets.All( AssetCanBeEdited );

		static bool TryMakeAssetsEditableByObject( Object[] assets ) {
			if( AssetsCanBeEdited( assets ) )
				return true;
			#if UNITY_2019_3_OR_NEWER
			string[] paths = assets.Select( AssetDatabase.GetAssetPath ).ToArray();
			return AssetDatabase.MakeEditable( paths, null );
			#else
			Task checkoutTask = Provider.Checkout( assets, CheckoutMode.Asset );
			checkoutTask.Wait();
			return checkoutTask.success;
			#endif
		}

		static bool TryMakeAssetsEditableByPath( string[] paths ) {
			if( AssetsCanBeEdited( paths ) )
				return true;
			#if UNITY_2019_3_OR_NEWER
			return AssetDatabase.MakeEditable( paths, null );
			#else
			Task checkoutTask = Provider.Checkout( paths, CheckoutMode.Asset );
			checkoutTask.Wait();
			return checkoutTask.success;
			#endif
		}

		internal static bool TryMakeAssetsEditable( params Object[] assets ) {
			if( TryMakeAssetsEditableByObject( assets ) )
				return true;
			DisplayAssetUnlockFailDialog( assets.Select( AssetDatabase.GetAssetPath ).ToArray() );
			return false;
		}

		internal static bool TryMakeAssetsEditable( params string[] paths ) {
			if( TryMakeAssetsEditableByPath( paths ) )
				return true;
			DisplayAssetUnlockFailDialog( paths );
			return false;
		}

		static void DisplayAssetUnlockFailDialog( string[] paths ) {
			bool multiple = paths.Length > 1;
			string files = $"file{( multiple ? "s" : "" )}";
			const string msg = "Shapes failed to access files, likely due to your version control system, please see the console for more info";
			EditorUtility.DisplayDialog( $"Failed to open {files} for editing", msg, "weird but ok" );
			string log = $"Shapes failed to access the following {files}:\n";
			paths.ForEach( x => log += x + "\n" );
			Debug.LogWarning( log );
		}

		static T LoadAssetWithGUID<T>( string guid ) where T : Object {
			string path = AssetDatabase.GUIDToAssetPath( guid );
			return AssetDatabase.LoadAssetAtPath<T>( path );
		}

		static string[] FindAllAssetGUIDs<T>() where T : Object => AssetDatabase.FindAssets( $"t:{typeof(T).Name}" );

		static string[] FindAllAssetGUIDs<T>( string path ) where T : Object {
			return AssetDatabase.FindAssets( $"t:{typeof(T).Name}", new[] { path } );
		}

		public static T TryLoadSingletonAsset<T>() where T : Object {
			string[] guids = FindAllAssetGUIDs<T>();
			return guids.Length > 0 ? LoadAssetWithGUID<T>( guids[0] ) : null;
		}

		public static IEnumerable<T> LoadAllAssets<T>( string path ) where T : Object => FindAllAssetGUIDs<T>( path ).Select( LoadAssetWithGUID<T> );
		static IEnumerable<T> LoadAllAssets<T>() where T : Object => FindAllAssetGUIDs<T>().Select( LoadAssetWithGUID<T> );

		#endif
	}

}