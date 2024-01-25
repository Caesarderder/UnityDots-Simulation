using UnityEngine;
using UnityEditor.IMGUI.Controls;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesHandles {

		public static Color GetHandleColor( Color shapeColor ) {
			return ShapesMath.Luminance( shapeColor ) > 0.7f ? Color.black : Color.white;
		}

		public static ArcHandle InitAngularHandle() {
			ArcHandle handle = new ArcHandle();
			handle.SetColorWithRadiusHandle( Color.white, 0f );
			handle.radiusHandleSizeFunction = pos => 0f; // no radius handle
			return handle;
		}

		public static ArcHandle InitRadialHandle() {
			ArcHandle handle = new ArcHandle();
			handle.SetColorWithRadiusHandle( Color.white, 0f );
			handle.angle = 360f;
			handle.angleHandleSizeFunction = pos => 0f; // no angle handle
			handle.angleHandleColor = Color.clear;
			return handle;
		}

		public static BoxBoundsHandle InitBoxHandle() {
			BoxBoundsHandle handle = new BoxBoundsHandle {
				axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
				handleColor = Color.white,
				wireframeColor = Color.white
			};
			return handle;
		}

		public static SphereBoundsHandle InitDiscHandle() {
			SphereBoundsHandle handle = new SphereBoundsHandle();
			handle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
			handle.handleColor = Color.white;
			handle.wireframeColor = Color.white;
			return handle;
		}

		public static SphereBoundsHandle InitSphereHandle() {
			SphereBoundsHandle handle = new SphereBoundsHandle();
			handle.handleColor = Color.white;
			handle.wireframeColor = Color.white;
			return handle;
		}

	}

}