using UnityEngine;
using UnityEditor;
using System.Linq;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Rectangle) )]
	[CanEditMultipleObjects]
	public class RectangleEditor : ShapeRendererEditor {

		SerializedProperty propType = null;
		SerializedProperty propCornerRadii = null;
		SerializedProperty propCornerRadiusMode = null;
		SerializedProperty propWidth = null;
		SerializedProperty propHeight = null;
		SerializedProperty propPivot = null;
		SerializedProperty propThickness = null;
		SerializedProperty propThicknessSpace = null;
		SerializedProperty propFill = null;
		SerializedProperty propUseFill = null;
		SerializedProperty propDashStyle = null;
		SerializedProperty propDashed = null;
		SerializedProperty propMatchDashSpacingToSize = null;

		DashStyleEditor dashEditor;
		SceneFillEditor fillEditor;
		SceneRectEditor rectEditor;

		public override void OnEnable() {
			base.OnEnable();
			dashEditor = DashStyleEditor.GetDashEditor( propDashStyle, propMatchDashSpacingToSize, propDashed );
			rectEditor = new SceneRectEditor( this );
			fillEditor = new SceneFillEditor( this, propFill, propUseFill );
		}

		void OnSceneGUI() {
			Rectangle rect = target as Rectangle;
			bool changed = rectEditor.DoSceneHandles( rect );
			GradientFill fill = rect.Fill;
			changed |= fillEditor.DoSceneHandles( rect.UseFill, rect, ref fill, rect.transform );
			if( changed ) {
				rect.Fill = fill;
				rect.UpdateAllMaterialProperties();
			}
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();
			base.BeginProperties();
			bool multiEditing = serializedObject.isEditingMultipleObjects;

			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "Type" );
				ShapesUI.DrawTypeSwitchButtons( propType, UIAssets.RectTypeButtonContents );
			}

			EditorGUILayout.PropertyField( propPivot );
			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "Size" );
				using( ShapesUI.TempLabelWidth( 14 ) ) {
					EditorGUILayout.PropertyField( propWidth, new GUIContent( "X" ), GUILayout.MinWidth( 20 ) );
					EditorGUILayout.PropertyField( propHeight, new GUIContent( "Y" ), GUILayout.MinWidth( 20 ) );
				}
			}

			bool isBorder = ( (Rectangle)target ).IsBorder;
			using( new EditorGUI.DisabledScope( !multiEditing && isBorder == false ) )
				ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );

			bool hasRadius = ( (Rectangle)target ).IsRounded;

			using( new EditorGUI.DisabledScope( hasRadius == false ) ) {
				EditorGUILayout.PropertyField( propCornerRadiusMode, new GUIContent( "Radius Mode" ) );
				CornerRadiusProperties();
			}

			rectEditor.GUIEditButton();
			
			bool hasDashablesInSelection = targets.Any( x => ( x as Rectangle ).IsBorder );
			using( new ShapesUI.GroupScope() )
				using( new EditorGUI.DisabledScope( hasDashablesInSelection == false ) )
					dashEditor.DrawProperties();

			fillEditor.DrawProperties( this );

			base.EndProperties();
		}

		void CornerRadiusProperties() {
			Rectangle.RectangleCornerRadiusMode radiusMode = (Rectangle.RectangleCornerRadiusMode)propCornerRadiusMode.enumValueIndex;

			if( radiusMode == Rectangle.RectangleCornerRadiusMode.Uniform ) {
				using( var change = new EditorGUI.ChangeCheckScope() ) {
					EditorGUI.showMixedValue = propCornerRadii.hasMultipleDifferentValues;
					float newRadius = Mathf.Max( 0f, EditorGUILayout.FloatField( "Radius", propCornerRadii.vector4Value.x ) );
					EditorGUI.showMixedValue = false;
					if( change.changed && newRadius != propCornerRadii.vector4Value.x )
						propCornerRadii.vector4Value = new Vector4( newRadius, newRadius, newRadius, newRadius );
				}
			} else { // per-corner
				SerializedProperty[] components = propCornerRadii.GetVisibleChildren().ToArray();
				(int component, string label )[] corners = { ( 1, "↖" ), ( 2, "↗" ), ( 0, "↙" ), ( 3, "↘" ) };
				void CornerField( string label, int component ) => EditorGUILayout.PropertyField( components[component], new GUIContent( label ), GUILayout.Width( 64 ) );

				void RowFields( string label, int a, int b ) {
					using( ShapesUI.Horizontal ) {
						GUILayout.Label( label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
						using( ShapesUI.TempLabelWidth( 18 ) ) {
							CornerField( corners[a].label, corners[a].component );
							CornerField( corners[b].label, corners[b].component );
						}
					}
				}

				RowFields( "Radii", 0, 1 );
				RowFields( " ", 2, 3 );
			}
		}
	}

}