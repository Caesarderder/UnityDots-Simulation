using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class SceneFillEditor : SceneEditGizmos {

		SerializedProperty propUseFill;
		SerializedProperty propType;
		SerializedProperty propSpace;
		SerializedProperty propColorStart;
		SerializedProperty propColorEnd;
		SerializedProperty propLinearStart;
		SerializedProperty propLinearEnd;
		SerializedProperty propRadialOrigin;
		SerializedProperty propRadialRadius;

		SerializedObject soMain;

		SphereBoundsHandle sphereHandle = ShapesHandles.InitSphereHandle();

		static bool isEditing;
		protected override bool IsEditing {
			get => isEditing;
			set => isEditing = value;
		}

		public SceneFillEditor( Editor parentEditor, SerializedProperty propShapeFill, SerializedProperty propUseFill ) {
			this.parentEditor = parentEditor;
			soMain = propShapeFill.serializedObject;
			this.propUseFill = propUseFill;
			propType = propShapeFill.FindPropertyRelative( "type" );
			propSpace = propShapeFill.FindPropertyRelative( "space" );
			propColorStart = propShapeFill.FindPropertyRelative( "colorStart" );
			propColorEnd = propShapeFill.FindPropertyRelative( "colorEnd" );
			propLinearStart = propShapeFill.FindPropertyRelative( "linearStart" );
			propLinearEnd = propShapeFill.FindPropertyRelative( "linearEnd" );
			propRadialOrigin = propShapeFill.FindPropertyRelative( "radialOrigin" );
			propRadialRadius = propShapeFill.FindPropertyRelative( "radialRadius" );
		}


		// this runs per object selected
		public bool DoSceneHandles( bool useFill, Object component, ref GradientFill fill, Transform tf ) {
			CheckForCancelEditAction();

			if( useFill == false )
				return false;

			bool changed = false;
			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : tf.rotation;

			switch( fill.type ) {
				case FillType.LinearGradient:

					Vector3 linStart = fill.GetLinearStartWorld( tf );
					Vector3 linEnd = fill.GetLinearEndWorld( tf );
					if( isEditing && IsHoldingAlt == false ) {
						linStart = Handles.PositionHandle( linStart, handleRotation );
						if( GUI.changed ) {
							Undo.RecordObject( component, "edit fill start" );
							fill.SetLinearStartWorld( tf, linStart );
							changed = true;
						}

						linEnd = Handles.PositionHandle( linEnd, handleRotation );
						if( GUI.changed ) {
							Undo.RecordObject( component, "edit fill end" );
							fill.SetLinearEndWorld( tf, linEnd );
							changed = true;
						}
					}

					// line markers
					Handles.DrawAAPolyLine( linStart, linEnd );
					Vector3 camForward = SceneView.currentDrawingSceneView.camera.transform.forward;
					float nootLength = 0.1f * HandleUtility.GetHandleSize( Vector3.Lerp( linStart, linEnd, .5f ) );
					Vector3 lineTangent = ( linEnd - linStart ).normalized;
					Vector3 lineNormal = Vector3.Cross( camForward, lineTangent ).normalized;
					Vector3 nootOffset = lineNormal * nootLength;
					const int nootCount = 5;
					for( int i = 0; i < nootCount; i++ ) {
						float t = i / ( nootCount - 1f );
						Vector3 pos = Vector3.Lerp( linStart, linEnd, t );
						Handles.DrawAAPolyLine( pos + nootOffset, pos - nootOffset );
					}

					break;
				case FillType.RadialGradient:

					if( isEditing ) {
						using( new Handles.DrawingScope( Matrix4x4.TRS( fill.GetRadialOriginWorld( tf ), handleRotation, Vector3.one ) ) ) {
							sphereHandle.center = Vector3.zero;
							sphereHandle.radius = fill.GetRadialWorldRadius( tf );
							sphereHandle.DrawHandle();
							if( GUI.changed ) {
								float newWorldRadius = sphereHandle.radius;
								Undo.RecordObject( component, "edit fill radius" );
								fill.SetRadialWorldRadius( tf, newWorldRadius );
								changed = true;
							}
						}


						if( IsHoldingAlt == false ) {
							Vector3 newOrigin = Handles.PositionHandle( fill.GetRadialOriginWorld( tf ), handleRotation );
							if( GUI.changed ) {
								Undo.RecordObject( component, "edit fill origin" );
								fill.SetRadialOriginWorld( tf, newOrigin );
								changed = true;
							}
						}
					}

					break;
			}

			return changed;
		}

		bool ShowFillControls => propUseFill.boolValue == false || propUseFill.hasMultipleDifferentValues;

		public bool DrawProperties( Editor parentEditor ) {
			bool changed = soMain.ApplyModifiedProperties(); // this is a little cursed
			using( new ShapesUI.GroupScope() )
				using( ShapesUI.TempLabelWidth( 75 ) ) {
					PropertyHeader( ref changed );
					using( new EditorGUI.DisabledScope( ShowFillControls ) ) {
						FillModeProperties( ref changed );
						// edit button
						GUIEditButton( "Edit Fill in Scene" );
					}
				}

			return changed;
		}

		void PropertyHeader( ref bool changed ) {
			using( ShapesUI.Horizontal ) {
				soMain.Update();
				EditorGUILayout.PropertyField( propUseFill, new GUIContent( "Fill", "Whether or not to override the color fill of this shape" ), GUILayout.ExpandWidth( false ) );
				if( soMain.ApplyModifiedProperties() )
					changed = true;

				using( new EditorGUI.DisabledScope( ShowFillControls ) ) {
					GUILayout.Space( 5 );
					using( ShapesUI.TempLabelWidth( 45 ) ) {
						SpaceField( ref changed );
					}
				}
			}
		}

		void SpaceField( ref bool changed ) {
			using( var chChk = new EditorGUI.ChangeCheckScope() ) {
				SerializedObject[] targets = propSpace.serializedObject.targetObjects.Select( obj => new SerializedObject( obj ) ).ToArray();
				FillSpace[] prevSpaceStates = targets.Select( t => (FillSpace)t.FindProperty( "fill.space" ).enumValueIndex ).ToArray();
				// this blob of mess is because we want coordinates to stay at the same world space position when changing space
				soMain.Update();
				EditorGUILayout.PropertyField( propSpace, new GUIContent( "Space" ) );
				if( soMain.ApplyModifiedProperties() )
					changed = true;

				if( chChk.changed ) {
					FillSpace currentSpace = (FillSpace)propSpace.enumValueIndex;
					for( int i = 0; i < prevSpaceStates.Length; i++ ) {
						FillSpace prevSpace = prevSpaceStates[i];
						if( currentSpace == prevSpace )
							continue;

						SerializedProperty propLinStart = targets[i].FindProperty( "fill.linearStart" );
						SerializedProperty propLinEnd = targets[i].FindProperty( "fill.linearEnd" );
						SerializedProperty propRadOrigin = targets[i].FindProperty( "fill.radialOrigin" );
						SerializedProperty propRadRadius = targets[i].FindProperty( "fill.radialRadius" );
						Transform tf = ( (Component)( targets[i].targetObject ) ).transform;

						propLinStart.serializedObject.Update();
						if( currentSpace == FillSpace.World ) {
							// local to world
							propLinStart.vector3Value = tf.TransformPoint( propLinStart.vector3Value );
							propLinEnd.vector3Value = tf.TransformPoint( propLinEnd.vector3Value );
							propRadOrigin.vector3Value = tf.TransformPoint( propRadOrigin.vector3Value );
							propRadRadius.floatValue *= tf.lossyScale.x;
						} else {
							propLinStart.vector3Value = tf.InverseTransformPoint( propLinStart.vector3Value );
							propLinEnd.vector3Value = tf.InverseTransformPoint( propLinEnd.vector3Value );
							propRadOrigin.vector3Value = tf.InverseTransformPoint( propRadOrigin.vector3Value );
							propRadRadius.floatValue /= tf.lossyScale.x;
						}

						if( propLinStart.serializedObject.ApplyModifiedProperties() )
							changed = true;
					}
				}
			}
		}


		void FillModeProperties( ref bool changed ) {
			soMain.Update();

			EditorGUILayout.PropertyField( propType, new GUIContent( "Type" ) );
			if( propType.enumValueIndex == (int)FillType.LinearGradient ) {
				ShapesUI.PosColorField( "Start", propLinearStart, propColorStart );
				ShapesUI.PosColorField( "End", propLinearEnd, propColorEnd );
			} else if( propType.enumValueIndex == (int)FillType.RadialGradient ) {
				ShapesUI.PosColorField( "Pos", propRadialOrigin, propColorStart );
				using( ShapesUI.Horizontal ) {
					using( ShapesUI.TempLabelWidth( ShapesUI.POS_COLOR_FIELD_LABEL_WIDTH + 17 ) )
						EditorGUILayout.PropertyField( propRadialRadius, new GUIContent( "Radius" ) );
					//GUILayout.FlexibleSpace();
					EditorGUILayout.PropertyField( propColorEnd, GUIContent.none, GUILayout.Width( ShapesUI.POS_COLOR_FIELD_COLOR_WIDTH ) );
				}
			}

			if( soMain.ApplyModifiedProperties() )
				changed = true;
		}

	}

}