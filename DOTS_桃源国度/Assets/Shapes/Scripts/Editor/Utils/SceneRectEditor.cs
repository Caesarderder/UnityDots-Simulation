using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class SceneRectEditor : SceneEditGizmos {

		static bool isEditing;

		BoxBoundsHandle boxHandle = ShapesHandles.InitBoxHandle();

		public SceneRectEditor( Editor parentEditor ) => this.parentEditor = parentEditor;

		protected override bool IsEditing {
			get => isEditing;
			set => isEditing = value;
		}

		public bool DoSceneHandles( Rectangle rect ) {
			if( IsEditing == false )
				return false;

			bool holdingAlt = ( Event.current.modifiers & EventModifiers.Alt ) != 0;
			bool showControls = holdingAlt == false;

			// set up matrix
			Matrix4x4 gizmoToWorld = Matrix4x4.TRS( default, rect.transform.rotation, Vector3.one );
			Matrix4x4 worldToGizmo = gizmoToWorld.inverse;

			Vector2 GizmoToRect( Vector3 gizmoPt ) {
				Vector3 wPos = gizmoToWorld.MultiplyPoint( gizmoPt );
				return rect.transform.InverseTransformPoint( wPos );
			}

			Vector3 RectToGizmo( Vector2 rectPt ) {
				Vector3 wPos = rect.transform.TransformPoint( rectPt );
				return worldToGizmo.MultiplyPoint( wPos );
			}

			Vector2 GizmoToRectVec( Vector3 gizmoPt ) {
				Vector3 wPos = gizmoToWorld.MultiplyVector( gizmoPt );
				return rect.transform.InverseTransformVector( wPos );
			}

			Vector3 RectToGizmoVec( Vector2 rectPt ) {
				Vector3 wPos = rect.transform.TransformVector( rectPt );
				return worldToGizmo.MultiplyVector( wPos );
			}

			Vector2 GetCenter() => GizmoToRect( boxHandle.center );
			Vector2 GetSize() => GizmoToRectVec( boxHandle.size );
			Vector2 GetBottomLeft() => GetCenter() - GetSize() / 2;

			Vector2 RectCenter() => rect.Pivot == RectPivot.Center ? Vector2.zero : RectSize() / 2;
			Vector2 RectSize() => new Vector2( rect.Width, rect.Height );
			Vector2 RectBottomLeft() => RectCenter() - RectSize() / 2;

			using( new Handles.DrawingScope( ShapesHandles.GetHandleColor( rect.Color ), gizmoToWorld ) ) {
				boxHandle.size = RectToGizmoVec( new Vector2( rect.Width, rect.Height ) );
				boxHandle.center = RectToGizmo( RectCenter() );
				Vector2 prevBottomLeft = GetBottomLeft();

				using( var chchk = new EditorGUI.ChangeCheckScope() ) {
					boxHandle.DrawHandle();
					if( chchk.changed ) {
						Undo.RecordObject( rect, "edit rectangle" );
						Vector2 newSize = GetSize();
						rect.Width = newSize.x;
						rect.Height = newSize.y;
						if( rect.Pivot == RectPivot.Corner ) {
							Transform rtf = rect.transform;
							Undo.RecordObject( rtf, "edit rectangle" );
							Vector2 newBottomLeft = GetBottomLeft();
							Vector2 delta = newBottomLeft - prevBottomLeft;
							Transform parent = rtf.parent;
							Vector3 deltaWorld = rtf.TransformVector( delta );
							Vector3 deltaParent = parent != null ? parent.InverseTransformVector( deltaWorld ) : deltaWorld;
							rtf.localPosition += deltaParent;
						}

						return true;
					}
				}

				if( rect.IsRounded ) {
					Vector3 gizUp = RectToGizmoVec( Vector3.up );
					Vector3 gizRight = RectToGizmoVec( Vector3.right );
					Vector3 gizLeft = -gizRight;
					Vector3 gizDown = -gizUp;

					( Vector3 a, Vector3 b )[] gizDirs = {
						( gizRight, gizUp ),
						( gizDown, gizRight ),
						( gizLeft, gizDown ),
						( gizUp, gizLeft )
					};

					Vector2 rectBl = RectBottomLeft();
					Vector3[] gizCorners = {
						RectToGizmo( rectBl ),
						RectToGizmo( rectBl + Vector2.up * rect.Height ),
						RectToGizmo( rectBl + Vector2.up * rect.Height + Vector2.right * rect.Width ),
						RectToGizmo( rectBl + Vector2.right * rect.Width )
					};

					float maxRadius = Mathf.Min( rect.Width, rect.Height ) / 2f;

					for( int i = 0; i < 4; i++ ) {
						float prevRadius = rect.CornerRadiusMode == Rectangle.RectangleCornerRadiusMode.Uniform ? rect.CornerRadius : rect.CornerRadii[i];
						float radiusGizmoSpace = RectToGizmoVec( prevRadius * Vector2.one ).x;
						Vector3 cornerGizmoSpace = gizCorners[i];

						// markers
						Vector3 innerCornerGizmoSpace = cornerGizmoSpace + ( gizDirs[i].a + gizDirs[i].b ) * radiusGizmoSpace;
						Handles.DrawWireArc( innerCornerGizmoSpace, Vector3.forward, -gizDirs[i].a, 90f, radiusGizmoSpace );
						using( new Handles.DrawingScope( new Color( Handles.color.r, Handles.color.g, Handles.color.b, 0.5f ) ) ) {
							Handles.DrawLine( innerCornerGizmoSpace, cornerGizmoSpace + gizDirs[i].a * radiusGizmoSpace );
							Handles.DrawLine( innerCornerGizmoSpace, cornerGizmoSpace + gizDirs[i].b * radiusGizmoSpace );
						}

						Handles.DrawLine( innerCornerGizmoSpace, innerCornerGizmoSpace - ( gizDirs[i].a + gizDirs[i].b ) * radiusGizmoSpace / Mathf.Sqrt( 2 ) );

						//Handles.DrawWireDisc( cornerGizmoSpace + (gizDirs[i].a + gizDirs[i].b ) * radiusGizmoSpace, Vector3.forward, prevRadius );

						if( showControls == false )
							continue;
						using( var chchk = new EditorGUI.ChangeCheckScope() ) {
							Vector3 handlePos = innerCornerGizmoSpace;
							float size = HandleUtility.GetHandleSize( handlePos ) * 0.15f;
							Vector3 gizDir = gizDirs[i].a + gizDirs[i].b; // diagonal
							Vector3 newPosGizmoSpace = Handles.Slider( handlePos, gizDir, size, Handles.CubeHandleCap, 0f );
							if( chchk.changed ) {
								Undo.RecordObject( rect, "edit rectangle corner radius" );
								Vector3 deltaGizmoSpace = newPosGizmoSpace - cornerGizmoSpace;
								float newRadius = Mathf.Abs( GizmoToRectVec( deltaGizmoSpace ).x );
								if( rect.CornerRadiusMode == Rectangle.RectangleCornerRadiusMode.Uniform ) {
									rect.CornerRadius = Mathf.Min( newRadius, maxRadius );
								} else {
									Vector4 radii = rect.CornerRadii;
									radii[i] = Mathf.Min( newRadius, maxRadius );
									rect.CornerRadii = radii;
								}

								return true;
							}
						}
					}
				}
			}

			return false;
		}


	}

}