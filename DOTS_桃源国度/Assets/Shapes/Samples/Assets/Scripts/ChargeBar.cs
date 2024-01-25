using UnityEngine;

namespace Shapes {

	public class ChargeBar : MonoBehaviour {

		[Header( "Gameplay" )] [SerializeField] float chargeSpeed = 1;
		[SerializeField] float chargeDecaySpeed = 1;
		[System.NonSerialized] public bool isCharging = false;
		float charge;

		[Header( "Style" )] public Color tickColor = Color.white;
		public Gradient chargeFillGradient;
		[Range( 0, 0.1f )] public float tickSizeSmol = 0.1f;
		[Range( 0, 0.1f )] public float tickSizeLorge = 0.1f;
		[Range( 0, 0.05f )] public float tickTickness;
		[Range( 0, 0.5f )] public float fontSize = 0.1f;
		[Range( 0, 0.5f )] public float fontSizeLorge = 0.1f;
		[Range( 0, 0.1f )] public float percentLabelOffset = 0.1f;
		[Range( 0, 0.4f )] public float fontGrowRangePrev = 0.1f;
		[Range( 0, 0.4f )] public float fontGrowRangeNext = 0.1f;


		[Header( "Animation" )] public AnimationCurve chargeFillCurve;
		public AnimationCurve animChargeShakeMagnitude = AnimationCurve.Linear( 0, 0, 1, 1 );
		[Range( 0, 0.05f )] public float chargeShakeMagnitude = 0.1f;
		public float chargeShakeSpeed = 1;


		public void UpdateCharge() {
			if( isCharging )
				charge += chargeSpeed * Time.deltaTime;
			else
				charge -= chargeDecaySpeed * Time.deltaTime;
			charge = Mathf.Clamp01( charge );
		}

		public void DrawBar( FpsController fpsController, float barRadius ) {
			// get some data
			float barThickness = fpsController.ammoBarThickness;
			float ammoBarOutlineThickness = fpsController.ammoBarOutlineThickness;
			float angRadMin = -fpsController.ammoBarAngularSpanRad / 2;
			float angRadMax = fpsController.ammoBarAngularSpanRad / 2;
			float angRadMinLeft = angRadMin + ShapesMath.TAU / 2;
			float angRadMaxLeft = angRadMax + ShapesMath.TAU / 2;
			float outerRadius = barRadius + barThickness / 2;

			float chargeAnim = chargeFillCurve.Evaluate( charge );

			// charge bar shake:
			float chargeMag = animChargeShakeMagnitude.Evaluate( chargeAnim ) * chargeShakeMagnitude;
			Vector2 origin = fpsController.GetShake( chargeShakeSpeed, chargeMag ); // do shake here
			float chargeAngRad = Mathf.Lerp( angRadMaxLeft, angRadMinLeft, chargeAnim );
			Color chargeColor = chargeFillGradient.Evaluate( chargeAnim );
			Draw.Arc( origin, fpsController.ammoBarRadius, barThickness, angRadMaxLeft, chargeAngRad, chargeColor );

			Vector2 movingLeftPos = origin + ShapesMath.AngToDir( chargeAngRad ) * barRadius;
			Vector2 bottomLeftPos = origin + ShapesMath.AngToDir( angRadMaxLeft ) * barRadius;

			// bottom fill
			Draw.Disc( bottomLeftPos, barThickness / 2f, chargeColor );

			// ticks
			const int tickCount = 7;

			Draw.LineEndCaps = LineEndCap.None;
			for( int i = 0; i < tickCount; i++ ) {
				float t = i / ( tickCount - 1f );
				float angRad = Mathf.Lerp( angRadMaxLeft, angRadMinLeft, t );
				Vector2 dir = ShapesMath.AngToDir( angRad );
				Vector2 a = origin + dir * outerRadius;
				bool lorge = i % 3 == 0;
				Vector2 b = a + dir * ( lorge ? tickSizeLorge : tickSizeSmol );
				Draw.Line( a, b, tickTickness, tickColor );

				// scale based on distance to real value
				float chargeDelta = t - chargeAnim;
				float growRange = chargeDelta < 0 ? fontGrowRangePrev : fontGrowRangeNext;
				float tFontScale = 1f - ShapesMath.SmoothCos01( Mathf.Clamp01( Mathf.Abs( chargeDelta ) / growRange ) );
				float fontScale = ShapesMath.Eerp( fontSize, fontSizeLorge, tFontScale );
				Draw.FontSize = fontScale;
				Vector2 labelPos = a + dir * percentLabelOffset;
				string pct = Mathf.RoundToInt( t * 100 ) + "%";
				Quaternion rotation = Quaternion.Euler( 0, 0, ( angRad + ShapesMath.TAU / 2 ) * Mathf.Rad2Deg );
				Draw.Text( labelPos, rotation, pct, TextAlign.Right );
			}

			// moving dot
			Draw.Disc( movingLeftPos, barThickness / 2f + ammoBarOutlineThickness / 2f );
			Draw.Disc( movingLeftPos, barThickness / 2f - ammoBarOutlineThickness / 2f, chargeColor );

			FpsController.DrawRoundedArcOutline( origin, barRadius, barThickness, ammoBarOutlineThickness, angRadMinLeft, angRadMaxLeft );

			Draw.LineEndCaps = LineEndCap.Round;

			// glow
			Draw.BlendMode = ShapesBlendMode.Additive;
			Draw.Disc( movingLeftPos, barThickness * 2, DiscColors.Radial( chargeColor, Color.clear ) );
			Draw.BlendMode = ShapesBlendMode.Transparent;
		}
	}

}