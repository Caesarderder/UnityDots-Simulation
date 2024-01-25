using System.Linq;
using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Disc) )]
	[CanEditMultipleObjects]
	public class DiscEditor : ShapeRendererEditor {

		SerializedProperty propType = null;
		SerializedProperty propColorMode = null;
		SerializedProperty propColorOuterStart = null;
		SerializedProperty propColorInnerEnd = null;
		SerializedProperty propColorOuterEnd = null;
		SerializedProperty propGeometry = null;
		SerializedProperty propAngRadiansStart = null;
		SerializedProperty propAngRadiansEnd = null;
		SerializedProperty propAngUnitInput = null;
		SerializedProperty propRadius = null;
		SerializedProperty propRadiusSpace = null;
		SerializedProperty propThickness = null;
		SerializedProperty propThicknessSpace = null;
		SerializedProperty propArcEndCaps = null;
		SerializedProperty propDashStyle = null;
		SerializedProperty propDashed = null;
		SerializedProperty propMatchDashSpacingToSize = null;

		DashStyleEditor dashEditor;
		SceneDiscEditor discEditor;

		public override void OnEnable() {
			base.OnEnable();
			dashEditor = DashStyleEditor.GetDashEditor( propDashStyle, propMatchDashSpacingToSize, propDashed );
			discEditor = new SceneDiscEditor( this );
		}

		void OnSceneGUI() {
			Disc disc = target as Disc;
			bool changed = discEditor.DoSceneHandles( disc );
		}

		public override void OnInspectorGUI() {
			base.BeginProperties( showColor: false );

			EditorGUILayout.PropertyField( propGeometry );

			// Color properties
			EditorGUILayout.PropertyField( propColorMode );
			switch( (Disc.DiscColorMode)propColorMode.enumValueIndex ) {
				case Disc.DiscColorMode.Single:
					base.PropertyFieldColor();
					break;
				case Disc.DiscColorMode.Radial:
					base.PropertyFieldColor( "Inner" );
					EditorGUILayout.PropertyField( propColorOuterStart, new GUIContent( "Outer" ) );
					break;
				case Disc.DiscColorMode.Angular:
					base.PropertyFieldColor( "Start" );
					EditorGUILayout.PropertyField( propColorInnerEnd, new GUIContent( "End" ) );
					break;
				case Disc.DiscColorMode.Bilinear:
					base.PropertyFieldColor( "Inner Start" );
					EditorGUILayout.PropertyField( propColorOuterStart, new GUIContent( "Outer Start" ) );
					EditorGUILayout.PropertyField( propColorInnerEnd, new GUIContent( "Inner End" ) );
					EditorGUILayout.PropertyField( propColorOuterEnd, new GUIContent( "Outer End" ) );
					break;
			}

			using( new EditorGUILayout.HorizontalScope() ) {
				EditorGUILayout.PrefixLabel( "Type" );
				ShapesUI.DrawTypeSwitchButtons( propType, UIAssets.DiscTypeButtonContents );
			}

			DiscType selectedType = (DiscType)propType.enumValueIndex;

			if( propType.enumValueIndex == (int)DiscType.Arc )
				ShapesUI.EnumToggleProperty( propArcEndCaps, "Round Caps" );
			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );
			using( new EditorGUI.DisabledScope( selectedType.HasThickness() == false && serializedObject.isEditingMultipleObjects == false ) )
				ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			DrawAngleProperties( selectedType );

			bool canEditInSceneView = propRadiusSpace.hasMultipleDifferentValues || propRadiusSpace.enumValueIndex == (int)ThicknessSpace.Meters;
			using( new EditorGUI.DisabledScope( canEditInSceneView == false ) )
				discEditor.GUIEditButton();

			bool hasDashablesInSelection = targets.Any( x => ( x as Disc ).HasThickness );
			using( new ShapesUI.GroupScope() )
				using( new EditorGUI.DisabledScope( hasDashablesInSelection == false ) )
					dashEditor.DrawProperties();

			base.EndProperties();
		}


		public static GUILayoutOption[] angLabelLayout = { GUILayout.Width( 50 ) };

		void DrawAngleProperties( DiscType selectedType ) {
			using( new EditorGUI.DisabledScope( selectedType.HasSector() == false && serializedObject.isEditingMultipleObjects == false ) ) {
				ShapesUI.AngleProperty( propAngRadiansStart, "Angle start", propAngUnitInput, angLabelLayout );
				ShapesUI.AngleProperty( propAngRadiansEnd, "Angle end", propAngUnitInput, angLabelLayout );
				ShapesUI.DrawAngleSwitchButtons(propAngUnitInput);
			}
		}

	}

}