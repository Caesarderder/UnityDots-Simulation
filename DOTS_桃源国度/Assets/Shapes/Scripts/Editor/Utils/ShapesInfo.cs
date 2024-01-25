using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {


	public static class ShapesInfo {

		public static string FILE_HEADER_COMMENT_A = "// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/";
		public static string FILE_HEADER_COMMENT_B = "// Website & Documentation - https://acegikmo.com/shapes/";
		
		public static string LINK_WEBSITE = "https://acegikmo.com/shapes";
		public static string LINK_DOCS = "https://acegikmo.com/shapes/docs";
		public static string LINK_CHANGELOG = "https://acegikmo.com/shapes/changelog";
		public static string LINK_LATEST_VERSION = "https://acegikmo.com/shapes/changelog/latestversion.php";
		public static string LINK_FEEDBACK = "https://shapes.userecho.com/";
		public static string LINK_QUIRE = "https://quire.io/w/Shapes/";
		public static string LINK_TWITTER = "https://twitter.com/FreyaHolmer";
		public static string LINK_PATREON = "https://www.patreon.com/acegikmo";

		static string version = null;
		public static string Version => version ?? ( version = JsonUtility.FromJson<ParsedPackageJson>( ShapesAssets.Instance.packageJson.text ).version );

		struct ParsedPackageJson {
			// this *is* actually used, silly IDEs~
			#pragma warning disable 649
			public string version;
			#pragma warning restore 649
		}

	}

}