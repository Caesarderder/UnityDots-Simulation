using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ScenePointEditor : SceneEditGizmos {

		static bool isEditing;

		public bool hasAddRemoveMode = true;
		public bool hasEditThicknessMode = false;
		public bool hasEditColorMode = false;

		public bool useFlatThicknessHandles = false;

		public event Action<ShapeRenderer, int> onValuesChanged = delegate {};

		public bool[] colorEnabledArray = null;
		public bool[] positionEnabledArray = null;

		EditMode currentEditMode = EditMode.PositionHandles;

		void GoToNextEditMode() {
			while( true ) {
				currentEditMode = (EditMode)( ( (int)currentEditMode + 1 ) % (int)EditMode.COUNT );
				if( CanEdit( currentEditMode ) == false ) continue;
				break;
			}
		}

		bool CanEdit( EditMode mode ) {
			switch( mode ) {
				case EditMode.AddRemovePoints: return hasAddRemoveMode;
				case EditMode.EditThickness:   return hasEditThicknessMode;
				case EditMode.EditColor:       return hasEditColorMode;
				default:                       return true;
			}
		}

		enum EditMode {
			PositionHandles,
			AddRemovePoints,
			EditThickness,
			EditColor,
			COUNT
		}

		SphereBoundsHandle discHandle = ShapesHandles.InitDiscHandle();

		public ScenePointEditor( Editor parentEditor ) => this.parentEditor = parentEditor;

		protected override bool IsEditing {
			get => isEditing;
			set => isEditing = value;
		}

		public bool DoSceneHandles( bool closed, Object component, List<PolylinePoint> points, Transform tf, float globalThicknessScale = 1f, Color globalColorTint = default ) {
			void SetPt( int i, Vector3 pt ) {
				PolylinePoint pp = points[i];
				pp.point = pt;
				points[i] = pp;
			}

			float GetThicknessWorld( int i ) {
				float localThickness = points[i].thickness * globalThicknessScale;
				return localThickness * tf.lossyScale.AvgComponentMagnitude();
			}

			void SetThicknessWorld( int i, float thicknessWorld ) {
				PolylinePoint pp = points[i];
				float localThickness = thicknessWorld / tf.lossyScale.AvgComponentMagnitude();
				pp.thickness = localThickness / globalThicknessScale;
				points[i] = pp;
			}


			Color GetNetColor( int i ) => points[i].color * globalColorTint;
			Color GetColor( int i ) => points[i].color;

			void SetColor( int i, Color color ) {
				PolylinePoint p = points[i];
				p.color = color;
				points[i] = p;
			}

			return DoSceneHandles( closed, component, points, tf, i => points[i].point, p => p.point, SetPt, PolylinePoint.Lerp, GetThicknessWorld, SetThicknessWorld, GetNetColor, GetColor, SetColor, () => points.Count );
		}

		public bool DoSceneHandles( bool closed, Object component, List<Vector2> points, Transform tf ) {
			return DoSceneHandles( closed, component, points, tf, i => points[i], p => p, ( i, p ) => points[i] = p, Vector2.LerpUnclamped );
		}

		public bool DoSceneHandles( bool closed, Object component, List<Vector3> points, Transform tf ) {
			return DoSceneHandles( closed, component, points, tf, i => points[i], p => p, ( i, p ) => points[i] = p, Vector3.LerpUnclamped );
		}

		public bool DoSceneHandles( bool closed, Object component, List<Vector3> points, List<Color> colors, Transform tf ) {
			void SetColor( int i, Color c ) {
				colors[i] = c;
				onValuesChanged( component as ShapeRenderer, i );
			}

			return DoSceneHandles( closed, component, points, tf, i => points[i], p => p, ( i, p ) => points[i] = p, Vector3.LerpUnclamped, null, null, i => colors[i], i => colors[i], SetColor, () => colors.Count );
		}

		bool TextureButton( Vector3 worldPos, Texture2D tex, float scale, bool fade = true ) {
			Rect r = new Rect( 0, 0, tex.width * scale, tex.height * scale );
			r.center = HandleUtility.WorldToGUIPoint( worldPos );

			Vector2 mousePos = Event.current.mousePosition;

			if( fade ) {
				float t = Mathf.InverseLerp( 200, 64, Vector2.Distance( mousePos, r.center ) );
				float a = Mathf.Lerp( 0.3f, 1f, t );
				GUI.color = new Color( 1, 1, 1, a );
			}

			bool pressed = GUI.Button( r, tex, GUIStyle.none );
			if( fade )
				GUI.color = Color.white;
			return pressed;
		}

		bool DoSceneHandles<T>( bool closed,
								Object component,
								List<T> points,
								Transform tf,
								Func<int, Vector3> GetPtAt,
								Func<T, Vector3> GetPt,
								Action<int, Vector3> SetPt,
								Func<T, T, float, T> Lerp,
								Func<int, float> GetThicknessWorld = null,
								Action<int, float> SetThicknessWorld = null,
								Func<int, Color> GetNetColor = null,
								Func<int, Color> GetColor = null,
								Action<int, Color> SetColor = null,
								Func<int> ColorCount = null ) {
			CheckForCancelEditAction();
			if( IsHoldingAlt )
				return false;

			bool changed = false;

			Vector3 GetWorldPt( int i ) => tf.TransformPoint( GetPtAt( i ) );

			if( isEditing ) {
				if( Event.current.isKey && Event.current.keyCode == KeyCode.Tab ) {
					if( Event.current.type == EventType.KeyDown )
						GoToNextEditMode();
					Event.current.Use();
				}

				// okay this is a bit of a hack but YKNOW it's fine
				// to prevent this from being drawn for every selected object
				if( Selection.gameObjects.Length > 0 && Selection.gameObjects[0] == ( (Component)component ).gameObject ) {
					if( Event.current.type == EventType.MouseMove )
						SceneView.lastActiveSceneView.Repaint();
					if( hasAddRemoveMode || hasEditThicknessMode || hasEditColorMode ) {
						Handles.BeginGUI();
						Vector2 mousePos = Event.current.mousePosition;
						Rect r = new Rect( mousePos.x + 32, mousePos.y, Screen.width, 128 );


						string label = "Press Tab to cycle modes:";

						void SelectLabel( string str, EditMode mode, bool exists = true ) {
							if( exists == false )
								return;
							if( mode == currentEditMode )
								label += "\n> " + str;
							else
								label += "\n  " + str;
						}

						SelectLabel( "postion", EditMode.PositionHandles );
						SelectLabel( "add/remove points", EditMode.AddRemovePoints, hasAddRemoveMode );
						SelectLabel( "thickness", EditMode.EditThickness, hasEditThicknessMode );
						SelectLabel( "color", EditMode.EditColor, hasEditColorMode );

						GUI.Label( r, label );
						Handles.EndGUI();
					}
				}

				bool DoAddButton( Vector3 pt, T newPoint, int insertIndex ) {
					if( TextureButton( pt, UIAssets.Instance.pointEditAdd, 0.5f ) ) {
						// add point
						changed = true;
						Undo.RecordObject( component, "add point" );
						points.Insert( insertIndex, newPoint );
						return true;
					}

					return false;
				}


				// add points before start and after end
				if( currentEditMode == EditMode.AddRemovePoints && closed == false && points.Count > 1 ) {
					void ExtrapolatedAddPoint( int end, int inner, int insertIndex ) {
						T newPtData = Lerp( points[inner], points[end], 2f );
						Vector3 ptWorld = tf.TransformPoint( GetPt( newPtData ) );
						Handles.EndGUI();
						Handles.DrawDottedLine( GetWorldPt( end ), ptWorld, 5f );
						Handles.BeginGUI(); // this is fine
						_ = DoAddButton( ptWorld, newPtData, insertIndex );
					}

					ExtrapolatedAddPoint( 0, 1, 0 );
					ExtrapolatedAddPoint( points.Count - 1, points.Count - 2, points.Count );
				}

				Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : tf.rotation;

				if( currentEditMode == EditMode.EditThickness ) {
					Transform camTf = SceneView.lastActiveSceneView.camera.transform;
					Vector3 camPos = camTf.position;
					Vector3 camUp = camTf.up;

					// thickness controls
					for( int i = 0; i < points.Count; i++ ) {
						discHandle.radius = GetThicknessWorld( i ) / 2f;
						discHandle.center = Vector3.zero;
						Vector3 discPos = GetWorldPt( i );
						Vector3 dirToCamera = discPos - camPos;
						Quaternion discRot = useFlatThicknessHandles ? tf.rotation : Quaternion.LookRotation( dirToCamera, camUp );
						Matrix4x4 mtx = Matrix4x4.TRS( discPos, discRot, Vector3.one );

						using( var chChk = new EditorGUI.ChangeCheckScope() ) {
							using( new Handles.DrawingScope( ShapesHandles.GetHandleColor( GetNetColor( i ) ), mtx ) )
								discHandle.DrawHandle();
							if( chChk.changed ) {
								changed = true;
								Undo.RecordObject( component, "edit thickness" );
								SetThicknessWorld( i, discHandle.radius * 2 );
								break;
							}
						}
					}
				} else if( currentEditMode == EditMode.AddRemovePoints ) {
					Handles.BeginGUI();
					for( int i = 0; i < points.Count; i++ ) {
						Vector3 ptWorld = GetWorldPt( i );
						if( TextureButton( ptWorld, UIAssets.Instance.pointEditRemove, 0.5f ) ) {
							// delete point
							changed = true;
							Undo.RecordObject( component, "delete point" );
							points.RemoveAt( i );
							break;
						}

						// if closed or !lastpoint
						if( closed || i != points.Count - 1 ) {
							Vector3 nextPt = GetWorldPt( ( i + 1 ) % points.Count );
							Vector3 midwayPt = ( nextPt + ptWorld ) / 2;
							T newPointData = Lerp( points[i], points[( i + 1 ) % points.Count], 0.5f );
							if( DoAddButton( midwayPt, newPointData, i + 1 ) )
								break; // added
						}
					}

					Handles.EndGUI();
				} else if( currentEditMode == EditMode.EditColor ) {
					Handles.BeginGUI();
					for( int i = 0; i < ColorCount(); i++ ) {
						if( colorEnabledArray != null && colorEnabledArray[i] == false )
							continue;
						Vector3 ptWorld = GetWorldPt( i );
						// Color newColor = EditorGUI.ColorField( r, GUIContent.none, GetColor( i ), true, true, ShapesConfig.USE_HDR_COLOR_PICKERS );

						Color col = GetColor( i );
						col.a = 1f;
						GUI.color = col;
						if( TextureButton( ptWorld, UIAssets.Instance.pointEditColor, 0.5f, fade: false ) ) {
							int noot = i;
							ShapesUI.ShowColorPicker( OnColorChanged, GetColor( i ) );

							void OnColorChanged( Color c ) {
								Undo.RecordObject( component, "modify color" );
								SetColor( noot, c );
								( component as ShapeRenderer )?.UpdateAllMaterialProperties();
								( component as ShapeRenderer )?.UpdateMesh( force: true );
								ShapesUI.RepaintAllSceneViews();
							}
						}
					}

					GUI.color = Color.white;
					Handles.EndGUI();
				} else if( currentEditMode == EditMode.PositionHandles ) {
					for( int i = 0; i < points.Count; i++ ) {
						if( positionEnabledArray != null && positionEnabledArray[i] == false )
							continue;
						Vector3 ptWorld = GetWorldPt( i );
						Vector3 newPosWorld = Handles.PositionHandle( ptWorld, handleRotation );
						if( GUI.changed ) {
							changed = true;
							Undo.RecordObject( component, "modify points" );
							SetPt( i, tf.InverseTransformPoint( newPosWorld ) );
						}
					}
				}
			}

			return changed;
		}

	}

}