using UnityEngine;

namespace Shapes {

	public class Compass : MonoBehaviour {

		public Vector2 position;
		public float width = 1f;
		[Range( 0, 0.01f )] public float lineThickness = 0.1f;
		[Range( 0.1f, 2f )] public float bendRadius = 1f;
		[Range( 0.05f, ShapesMath.TAU * 0.49f )] public float fieldOfView = ShapesMath.TAU / 4;

		[Header( "Ticks" )] public int ticksPerQuarterTurn = 12;
		[Range( 0, 0.2f )] public float tickSize = 0.1f;
		[Range( 0f, 1f )] public float tickEdgeFadeFraction = 0.1f;
		[Range( 0.01f, 0.26f )] public float fontSizeTickLabel = 1f;
		[Range( 0, 0.1f )] public float tickLabelOffset = 0.01f;

		[Header( "Degree Marker" )] [Range( 0.01f, 0.26f )] public float fontSizeLookLabel = 1f;
		public Vector2 lookAngLabelOffset;
		[Range( 0, 0.05f )] public float triangleNootSize = 0.1f;

		string[] directionLabels = { "S", "W", "N", "E" };

		public void DrawCompass( Vector3 worldDir ) {
			// prepare all variables
			Vector2 compArcOrigin = position + Vector2.down * bendRadius;
			float angUiMin = ShapesMath.TAU * 0.25f - ( width / 2 ) / bendRadius;
			float angUiMax = ShapesMath.TAU * 0.25f + ( width / 2 ) / bendRadius;
			Vector2 dirWorld = new Vector2( worldDir.x, worldDir.z ).normalized;
			float lookAng = ShapesMath.DirToAng( dirWorld );
			float angWorldMin = lookAng + fieldOfView / 2;
			float angWorldMax = lookAng - fieldOfView / 2;
			Vector2 labelPos = compArcOrigin + Vector2.up * ( bendRadius ) + lookAngLabelOffset * 0.1f;
			string lookLabel = Mathf.RoundToInt( -lookAng * Mathf.Rad2Deg + 180f ) + "°";

			// prepare draw state
			Draw.LineEndCaps = LineEndCap.Square;
			Draw.Thickness = lineThickness;

			// draw the horizontal line/arc of the compass
			Draw.Arc( compArcOrigin, bendRadius, lineThickness, angUiMin, angUiMax, ArcEndCap.Round );

			// draw the look angle label
			Draw.FontSize = fontSizeLookLabel;
			Draw.Text( labelPos, lookLabel, TextAlign.Center );

			// triangle arrow
			Vector2 trianglePos = compArcOrigin + Vector2.up * ( bendRadius + 0.01f );
			Draw.RegularPolygon( trianglePos, 3, triangleNootSize, -ShapesMath.TAU / 4 );

			// draw ticks
			int tickCount = ( ticksPerQuarterTurn - 1 ) * 4;
			for( int i = 0; i < tickCount; i++ ) {
				float t = i / ( (float)tickCount );
				float ang = ShapesMath.TAU * t;
				bool cardinal = i % ( tickCount / 4 ) == 0;

				string label = null;
				if( cardinal ) {
					int angInt = Mathf.RoundToInt( ( 1f - t ) * 4 );
					label = directionLabels[angInt % 4];
				}

				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, ang );
				if( tCompass < 1f && tCompass > 0f )
					DrawTick( ang, cardinal ? 0.8f : 0.5f, label );
			}

			void DrawTick( float worldAng, float size, string label = null ) {
				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, worldAng );
				float uiAng = Mathf.Lerp( angUiMin, angUiMax, tCompass );
				Vector2 uiDir = ShapesMath.AngToDir( uiAng );
				Vector2 a = compArcOrigin + uiDir * bendRadius;
				Vector2 b = compArcOrigin + uiDir * ( bendRadius - size * tickSize );
				float fade = Mathf.InverseLerp( 0, tickEdgeFadeFraction, ( 1f - Mathf.Abs( tCompass * 2 - 1 ) ) );
				Draw.Line( a, b, LineEndCap.None, new Color( 1, 1, 1, fade ) );
				if( label != null ) {
					Draw.FontSize = fontSizeTickLabel;
					Quaternion rotation = Quaternion.Euler( 0, 0, ( uiAng - ShapesMath.TAU / 4f ) * Mathf.Rad2Deg );
					Draw.Text( b - uiDir * tickLabelOffset, rotation, label, TextAlign.Center, new Color( 1, 1, 1, fade ) );
				}
			}
		}

	}

}