using UnityEngine;

namespace Shapes {

	public class Crosshair : MonoBehaviour {

		[Header( "Style" )] [Range( 0, 0.05f )] public float crosshairCrossInnerRad = 0.1f;
		[Range( 0, 0.05f )] public float crosshairCrossOuterRad = 0.3f;
		[Range( 0, 0.05f )] public float crosshairCrossThickness = 0.2f;
		[Range( 0, 0.05f )] public float crosshairHitCrossInnerRad = 0.1f;
		[Range( 0, 0.05f )] public float crosshairHitCrossOuterRad = 0.3f;
		[Range( 0, 0.05f )] public float crosshairHitCrossThickness = 0.2f;

		[Header( "Animation" )] [Range( 0, 1f )] public float scaleFire = 0.1f;
		public Decayer fireDecayer = new Decayer();
		public Decayer hitDecayer = new Decayer();

		public void Fire() => fireDecayer.SetT( 1 );
		public void FireHit() => hitDecayer.SetT( 1 );

		public void UpdateCrosshairDecay() {
			fireDecayer.Update();
			hitDecayer.Update();
		}

		public void DrawCrosshair() {
			Vector2[] dirsOrtho = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
			Vector2[] dirsDiag = {
				( Vector2.up + Vector2.right ).normalized,
				( Vector2.right + Vector2.down ).normalized,
				( Vector2.down + Vector2.left ).normalized,
				( Vector2.left + Vector2.up ).normalized
			};

			void DrawCross( Vector2[] dirs, float radInner, float radOuter, float thickness, float radialOffset, Color color ) {
				foreach( Vector2 dir in dirs ) {
					Vector2 a = dir * ( radInner + radialOffset );
					Vector2 b = dir * ( radOuter + radialOffset );
					Draw.Line( a, b, thickness, LineEndCap.Round, color );
				}
			}

			// draw things
			float thicc = crosshairCrossThickness * Mathf.Lerp( 1f, scaleFire, fireDecayer.t );
			DrawCross( dirsOrtho, crosshairCrossInnerRad, crosshairCrossOuterRad, thicc, fireDecayer.value, Color.white );
			DrawCross( dirsDiag, crosshairHitCrossInnerRad, crosshairHitCrossOuterRad, crosshairHitCrossThickness, hitDecayer.valueInv, new Color( 1, 0, 0, hitDecayer.t ) );
		}

	}

}