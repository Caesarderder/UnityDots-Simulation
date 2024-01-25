using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Quad) )]
	[CanEditMultipleObjects]
	public class QuadEditor : ShapeRendererEditor {

		SerializedProperty propA = null;
		SerializedProperty propB = null;
		SerializedProperty propC = null;
		SerializedProperty propD = null;
		SerializedProperty propColorMode = null;
		SerializedProperty propColorB = null;
		SerializedProperty propColorC = null;
		SerializedProperty propColorD = null;
		SerializedProperty propAutoSetD = null;

		ScenePointEditor scenePointEditor;
		List<Color> colors = new List<Color>() { default, default, default, default };

		public override void OnEnable() {
			base.OnEnable();
			scenePointEditor = new ScenePointEditor( this ) { hasAddRemoveMode = false, hasEditColorMode = true };
			scenePointEditor.onValuesChanged += UpdateColors;
		}

		void OnSceneGUI() {
			Quad quad = target as Quad;
			List<Vector3> pts = new List<Vector3>() { quad.A, quad.B, quad.C, quad.IsUsingAutoD ? quad.DAuto : quad.D };
			colors[0] = quad.ColorA;
			colors[1] = quad.ColorB;
			colors[2] = quad.ColorC;
			colors[3] = quad.ColorD;


			scenePointEditor.positionEnabledArray = new[] {
				true, true, true, quad.IsUsingAutoD == false
			};
			UpdateHandleColors( quad );

			bool changed = scenePointEditor.DoSceneHandles( false, quad, pts, colors, quad.transform );
			if( changed ) {
				( quad.A, quad.B, quad.C ) = ( pts[0], pts[1], pts[2] );
				if( quad.IsUsingAutoD == false ) quad.D = pts[3];
				( quad.ColorA, quad.ColorB, quad.ColorC, quad.ColorD ) = ( colors[0], colors[1], colors[2], colors[3] );
				quad.UpdateAllMaterialProperties();
			}
		}

		void UpdateColors( ShapeRenderer shape, int changeIndex ) {
			Quad quad = shape as Quad;

			Color newColor = colors[changeIndex];

			switch( (Quad.QuadColorMode)propColorMode.enumValueIndex ) {
				case Quad.QuadColorMode.Single:
					colors[0] = newColor;
					colors[1] = newColor;
					colors[2] = newColor;
					colors[3] = newColor;
					break;
				case Quad.QuadColorMode.Horizontal:
					if( changeIndex == 0 || changeIndex == 1 ) {
						colors[0] = newColor;
						colors[1] = newColor;
					} else {
						colors[2] = newColor;
						colors[3] = newColor;
					}

					break;
				case Quad.QuadColorMode.Vertical:
					if( changeIndex == 0 || changeIndex == 3 ) {
						colors[0] = newColor;
						colors[3] = newColor;
					} else {
						colors[1] = newColor;
						colors[2] = newColor;
					}

					break;
			}


			( quad.ColorA, quad.ColorB, quad.ColorC, quad.ColorD ) = ( colors[0], colors[1], colors[2], colors[3] );
		}

		void UpdateHandleColors( Quad quad ) {
			switch( (Quad.QuadColorMode)propColorMode.enumValueIndex ) {
				case Quad.QuadColorMode.Single:
					colors[0] = quad.ColorA;
					colors[1] = quad.ColorA;
					colors[2] = quad.ColorA;
					colors[3] = quad.ColorA;
					break;
				case Quad.QuadColorMode.Horizontal:
					colors[0] = quad.ColorA;
					colors[1] = quad.ColorA;
					colors[2] = quad.ColorC;
					colors[3] = quad.ColorC;
					break;
				case Quad.QuadColorMode.Vertical:
					colors[0] = quad.ColorD;
					colors[1] = quad.ColorB;
					colors[2] = quad.ColorB;
					colors[3] = quad.ColorD;
					break;
				case Quad.QuadColorMode.PerCorner:
					colors[0] = quad.ColorA;
					colors[1] = quad.ColorB;
					colors[2] = quad.ColorC;
					colors[3] = quad.ColorD;
					break;
			}
		}


		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.BeginProperties( showColor: false );
			EditorGUILayout.PropertyField( propColorMode );

			bool dEnabled = propAutoSetD.boolValue == false;
			Vector3 dAuto = ( target as Quad ).DAuto;

			switch( (Quad.QuadColorMode)propColorMode.enumValueIndex ) {
				case Quad.QuadColorMode.Single:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, base.propColor, false );
					ShapesUI.PosColorField( "C", propC, base.propColor, false );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, base.propColor, false, dEnabled );
					break;
				case Quad.QuadColorMode.Horizontal:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, base.propColor, false );
					ShapesUI.PosColorField( "C", propC, propColorC );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorC, false, dEnabled );
					break;
				case Quad.QuadColorMode.Vertical:
					ShapesUI.PosColorField( "A", propA, propColorD );
					ShapesUI.PosColorField( "B", propB, propColorB );
					ShapesUI.PosColorField( "C", propC, propColorB, false );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorD, false, dEnabled );
					break;
				case Quad.QuadColorMode.PerCorner:
					ShapesUI.PosColorField( "A", propA, base.propColor );
					ShapesUI.PosColorField( "B", propB, propColorB );
					ShapesUI.PosColorField( "C", propC, propColorC );
					ShapesUI.PosColorFieldSpecialOffState( "D", propD, dAuto, propColorD, true, dEnabled );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			using( new EditorGUILayout.HorizontalScope() ) {
				GUILayout.Label( GUIContent.none, GUILayout.Width( ShapesUI.POS_COLOR_FIELD_LABEL_WIDTH ) );
				EditorGUILayout.PropertyField( propAutoSetD, GUIContent.none, GUILayout.Width( 16 ) );
				GUILayout.Label( "Auto-set D" );
			}

			scenePointEditor.GUIEditButton( "Edit Points in Scene" );

			base.EndProperties();
		}

	}

}