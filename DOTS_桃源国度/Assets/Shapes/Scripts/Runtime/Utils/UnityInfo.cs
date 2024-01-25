using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if SHAPES_URP
using System.Linq;
#if UNITY_2021_2_OR_NEWER
using URP_RND_DATA = UnityEngine.Rendering.Universal.ScriptableRendererData;

#else
using URP_RND_DATA = UnityEngine.Rendering.Universal.ForwardRendererData;
#endif
#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	internal static class UnityInfo {
		public static bool UsingSRP => GraphicsSettings.renderPipelineAsset != null;
		public const int INSTANCES_MAX = 1023;

		#if UNITY_EDITOR
		internal static RenderPipeline GetCurrentRenderPipelineInUse() {
			RenderPipelineAsset rpa = GraphicsSettings.renderPipelineAsset;
			if( rpa != null ) {
				switch( rpa.GetType().Name ) {
					case "UniversalRenderPipelineAsset": return RenderPipeline.URP;
					case "HDRenderPipelineAsset":        return RenderPipeline.HDRP;
				}
			}

			return RenderPipeline.Legacy;
		}

		#if SHAPES_URP
		internal static URP_RND_DATA[] LoadAllURPRenderData() => ShapesIO.LoadAllAssets<URP_RND_DATA>( "Assets/" ).ToArray();
		#endif

		#if SHAPES_URP || SHAPES_HDRP
		public const string ON_PRE_RENDER_NAME = "RenderPipelineManager.beginCameraRendering";
		#else
		public const string ON_PRE_RENDER_NAME = "Camera.onPreRender";
		#endif

		#endif
	}

}