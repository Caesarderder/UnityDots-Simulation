using UnityEngine;

namespace Shapes {

	// I recommend drawing in onPreRender, which ImmediateModeShapeDrawer will handle for you, so let's inherit from that
	[ExecuteAlways] public class SpinningColorDiscs : ImmediateModeShapeDrawer {
		
		// Now, it *is* possible to run iummediate mode in Update() as well,
		// but keep in mind that these draw commands are only removed when a camera is done rendering.
		// This means that if Update() runs while the camera *isn't* rendering,
		// you'll accumulate/add lots of rendering commands that aren't rendered and removed until the next render.
		// This is actually very common - if you switch to the scene view when the game view isn't visible in editor, this will happen.
		// TLDR:
		// • use Camera.onPreRender or RenderPipelineManager.beginCameraRendering
		// • ImmediateModeShapeDrawer does this for you
		// • Update() is fine as long as you make sure to only run this command when the camera is rendering, which is harder than you think

		[Range( 3, 32 )] public int discCount = 24;
		[Range( 0f, 1f )] public float discRadius = 0.1f;

		// This method is called by ImmediateModeShapeDrawer - equivalent to OnPreRender of all cameras (except preview windows and probe cameras)
		public override void DrawShapes( Camera cam ) {
			
			// Draw.Command enqueues a set of draw commands to render in the given camera
			using( Draw.Command( cam ) ) { // all immediate mode drawing should happen within these using-statements
				Draw.ResetAllDrawStates(); // this makes sure no static draw states "leak" over to this scene
				Draw.Matrix = transform.localToWorldMatrix; // this makes it draw in the local space of this transform
				for( int i = 0; i < discCount; i++ ) {
					float t = i / (float)discCount;
					Color color = Color.HSVToRGB( t, 1, 1 );
					Vector2 pos = GetDiscPosition( t );
					Draw.Disc( pos, discRadius, color ); // This is the actual Shapes draw command
				}
			}
		}

		Vector2 GetDiscPosition( float t ) {
			float ang = t * ShapesMath.TAU; // base angle
			ang += ShapesMath.TAU * Time.time * 0.25f; // add constant spin rate
			ang += Mathf.Cos( ang * 2 + Time.time * ShapesMath.TAU * 0.5f ) * 0.16f; // add wave offsets~
			return ShapesMath.AngToDir( ang ); // convert angle to a direction/position
		}

	}

}