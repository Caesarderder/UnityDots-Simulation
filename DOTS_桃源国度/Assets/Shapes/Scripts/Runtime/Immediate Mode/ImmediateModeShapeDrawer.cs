using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ImmediateModeShapeDrawer : MonoBehaviour {

		/// <summary>Whether or not to only draw in cameras that can see the layer of this GameObject</summary>
		[Tooltip( "When enabled, shapes will only draw in cameras that can see the layer of this GameObject" )]
		public bool useCullingMasks = false;

		/// <summary>Override this to draw Shapes in immediate mode. This is called once per camera. You can draw using this code: using(Draw.Command(cam)){ // Draw here }</summary>
		/// <param name="cam">The camera that is currently rendering</param>
		public virtual void DrawShapes( Camera cam ) {
			// override this and draw shapes in immediate mode here
		}

		void OnCameraPreRender( Camera cam ) {
			switch( cam.cameraType ) {
				case CameraType.Preview:
				case CameraType.Reflection:
					return; // Don't render in preview windows or in reflection probes in case we run this script in the editor
			}
			if( useCullingMasks && ( cam.cullingMask & ( 1 << gameObject.layer ) ) == 0 )
				return; // scene & game view cameras should respect culling layer settings if you tell them to

			DrawShapes( cam );
		}

		#if (SHAPES_URP || SHAPES_HDRP)
			#if UNITY_2019_1_OR_NEWER
				public virtual void OnEnable() => RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
				public virtual void OnDisable() => RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
				void DrawShapesSRP( ScriptableRenderContext ctx, Camera cam ) => OnCameraPreRender( cam );
			#else
				public virtual void OnEnable() => Debug.LogWarning( "URP/HDRP immediate mode doesn't really work pre-Unity 2019.1, as there is no OnPreRender or beginCameraRendering callback" );
			#endif
		#else
		public virtual void OnEnable() => Camera.onPreRender += OnCameraPreRender;
		public virtual void OnDisable() => Camera.onPreRender -= OnCameraPreRender;
		#endif

	}

}