using UnityEngine;

namespace Shapes {

	/// <summary>
	/// Color picker interaction, for the Shapes color picker example scene
	/// </summary>
	public class IMColorPickerInteraction : MonoBehaviour {

		enum ColorPickerElement {
			None,
			HueStrip,
			Rectangle
		}

		public IMColorPickerRenderer picker;
		ColorPickerElement currentInteraction;

		// Example interaction using the mouse
		void Update() {
			if( Camera.main != null ) {
				Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
				RaycastInteract( ray,
					Input.GetMouseButtonDown( 0 ),
					Input.GetMouseButton( 0 ),
					Input.GetMouseButtonUp( 0 )
				);
			}
		}

		void OnDisable() => currentInteraction = ColorPickerElement.None;

		public void RaycastInteract( Ray ray, bool onPress, bool whileHeld, bool onRelease ) {
			if( onPress || whileHeld ) {
				// first transform the ray to local space
				ray.origin = transform.InverseTransformPoint( ray.origin );
				ray.direction = transform.InverseTransformDirection( ray.direction );

				// raycast the local Z plane
				if( new Plane( Vector3.back, 0 ).Raycast( ray, out float dist ) ) {
					Vector2 pt = ray.GetPoint( dist ); // get point and discard Z
					if( onPress )
						currentInteraction = GetPickerElementAt( pt );
					if( whileHeld && currentInteraction != ColorPickerElement.None )
						UpdatePickerColor( pt );
				}
			}

			if( onRelease )
				currentInteraction = ColorPickerElement.None;
		}

		void UpdatePickerColor( Vector2 pt ) {
			if( currentInteraction == ColorPickerElement.HueStrip )
				picker.hue = IMColorPickerRenderer.VectorToHue( pt );
			else if( currentInteraction == ColorPickerElement.Rectangle ) {
				Vector2 sv = ShapesMath.InverseLerp( picker.QuadRect, pt );
				picker.saturation = Mathf.Clamp01( sv.x );
				picker.value = Mathf.Clamp01( sv.y );
			}
		}

		ColorPickerElement GetPickerElementAt( Vector2 pt ) {
			if( HueStripContains( pt ) )
				return ColorPickerElement.HueStrip;
			if( picker.QuadRect.Contains( pt ) )
				return ColorPickerElement.Rectangle;
			return ColorPickerElement.None;
		}

		bool HueStripContains( Vector2 pt ) {
			float ptRadius = pt.magnitude;
			return ptRadius >= picker.HueStripRadiusInner && ptRadius <= picker.HueStripRadiusOuter;
		}

	}

}