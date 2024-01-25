using UnityEngine;

namespace Shapes {

	public class AmmoBar : MonoBehaviour {

		public int totalBullets = 20;
		public int bullets = 15;

		[Header( "Style" )] [Range( 0, 1f )] public float bulletThicknessScale = 1f;
		[Range( 0, 0.5f )] public float bulletEjectScale = 0.5f;

		[Header( "Animation" )] public float bulletDisappearTime = 1f;
		[Range( 0, ShapesMath.TAU )] public float bulletEjectAngSpeed = 0.5f;
		[Range( 0, ShapesMath.TAU )] public float ejectRotSpeedVariance = 1f;
		public AnimationCurve bulletEjectX = AnimationCurve.Constant( 0, 1, 0 );
		public AnimationCurve bulletEjectY = AnimationCurve.Constant( 0, 1, 0 );

		Vector2 GetBulletEjectPos( Vector2 origin, float t ) {
			Vector2 ejectAnimPos = new Vector2( bulletEjectX.Evaluate( t ), bulletEjectY.Evaluate( t ) );
			return origin + ejectAnimPos * bulletEjectScale;
		}

		float[] bulletFireTimes;
		public bool HasBulletsLeft => bullets > 0;
		public void Fire() => bulletFireTimes[--bullets] = Time.time;
		public void Reload() => bullets = totalBullets;

		void Awake() => bulletFireTimes = new float[totalBullets];

		public void DrawBar( FpsController fpsController, float barRadius ) {
			float barThickness = fpsController.ammoBarThickness;
			float ammoBarOutlineThickness = fpsController.ammoBarOutlineThickness;
			float angRadMin = -fpsController.ammoBarAngularSpanRad / 2;
			float angRadMax = fpsController.ammoBarAngularSpanRad / 2;

			// draw bullets
			Draw.LineEndCaps = LineEndCap.Round;
			float innerRadius = barRadius - barThickness / 2;
			float bulletThickness = ( innerRadius * fpsController.ammoBarAngularSpanRad / totalBullets ) * bulletThicknessScale;
			for( int i = 0; i < totalBullets; i++ ) {
				float t = i / ( totalBullets - 1f );
				float angRad = Mathf.Lerp( angRadMin, angRadMax, t );
				Vector2 dir = ShapesMath.AngToDir( angRad );
				Vector2 origin = dir * barRadius;
				Vector2 offset = dir * ( barThickness / 2f - ammoBarOutlineThickness * 1.5f );

				float alpha = 1;
				bool hasBeenFired = i >= bullets;
				if( hasBeenFired && Application.isPlaying ) {
					float timePassed = Time.time - bulletFireTimes[i];
					float tFade = Mathf.Clamp01( timePassed / bulletDisappearTime );
					alpha = 1f - tFade;
					origin = GetBulletEjectPos( origin, tFade );
					float angle = timePassed * ( bulletEjectAngSpeed + Mathf.Cos( i * 92372.8f ) * ejectRotSpeedVariance );
					offset = ShapesMath.Rotate( offset, angle );
				}

				Vector2 a = origin + offset;
				Vector2 b = origin - offset;
				Draw.Line( a, b, bulletThickness, new Color( 1, 1, 1, alpha ) );
			}

			FpsController.DrawRoundedArcOutline( Vector2.zero, barRadius, barThickness, ammoBarOutlineThickness, angRadMin, angRadMax );
		}
	}

}