using System.Linq;
using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(RegularPolygon) )]
	[CanEditMultipleObjects]
	public class RegularPolygonEditor : ShapeRendererEditor {

		SerializedProperty propGeometry = null;
		SerializedProperty propSides = null;
		SerializedProperty propBorder = null;
		SerializedProperty propAngle = null;
		SerializedProperty propRoundness = null;
		SerializedProperty propAngUnitInput = null;
		SerializedProperty propRadius = null;
		SerializedProperty propRadiusSpace = null;
		SerializedProperty propThickness = null;
		SerializedProperty propThicknessSpace = null;
		SerializedProperty propFill = null;
		SerializedProperty propUseFill = null;
		SerializedProperty propDashStyle = null;
		SerializedProperty propDashed = null;
		SerializedProperty propMatchDashSpacingToSize = null;

		DashStyleEditor dashEditor;
		SceneFillEditor fillEditor;
		SceneDiscEditor discEditor; // todo: polygonal version

		public override void OnEnable() {
			base.OnEnable();
			dashEditor = DashStyleEditor.GetDashEditor( propDashStyle, propMatchDashSpacingToSize, propDashed );
			fillEditor = new SceneFillEditor( this, propFill, propUseFill );
			discEditor = new SceneDiscEditor( this );
		}

		void OnSceneGUI() {
			RegularPolygon rp = target as RegularPolygon;
			GradientFill fill = rp.Fill;
			bool changed = discEditor.DoSceneHandles( rp );
			changed |= fillEditor.DoSceneHandles( rp.UseFill, rp, ref fill, rp.transform );
			if( changed ) {
				rp.Fill = fill;
				rp.UpdateAllMaterialProperties();
			}
		}

		int[] indexToPolygonPreset = { 3, 4, 5, 6, 8 };
		
		GUIContent[] sideCountTypes;
		GUIContent[] SideCountTypes =>
			sideCountTypes ?? ( sideCountTypes = new[] {
				new GUIContent( UIAssets.Instance.GetRegularPolygonIcon( 3 ), "Triangle" ),
				new GUIContent( UIAssets.Instance.GetRegularPolygonIcon( 4 ), "Square" ),
				new GUIContent( UIAssets.Instance.GetRegularPolygonIcon( 5 ), "Pentagon" ),
				new GUIContent( UIAssets.Instance.GetRegularPolygonIcon( 6 ), "Hexagon" ),
				new GUIContent( UIAssets.Instance.GetRegularPolygonIcon( 8 ), "Octagon" )
			} );


		public override void OnInspectorGUI() {
			base.BeginProperties( showColor: true );

			EditorGUILayout.PropertyField( propGeometry );
			ShapesUI.DrawTypeSwitchButtons( propSides, SideCountTypes, indexToPolygonPreset );
			//ShapesUI.EnumButtonRow(); // todo
			EditorGUILayout.PropertyField( propSides );
			EditorGUILayout.PropertyField( propRoundness );

			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );

			EditorGUILayout.PropertyField( propBorder );
			bool hasBordersInSelection = targets.Any( x => ( x as RegularPolygon ).Border );
			using( new EditorGUI.DisabledScope( hasBordersInSelection == false ) )
				ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			ShapesUI.AngleProperty( propAngle, "Angle", propAngUnitInput, angLabelLayout );
			ShapesUI.DrawAngleSwitchButtons( propAngUnitInput );

			bool canEditInSceneView = propRadiusSpace.hasMultipleDifferentValues || propRadiusSpace.enumValueIndex == (int)ThicknessSpace.Meters;
			using( new EditorGUI.DisabledScope( canEditInSceneView == false ) )
				discEditor.GUIEditButton();
			
			
			using( new ShapesUI.GroupScope() )
				using( new EditorGUI.DisabledScope( hasBordersInSelection == false ) )
					dashEditor.DrawProperties();

			fillEditor.DrawProperties( this );

			base.EndProperties();
		}


		static GUILayoutOption[] angLabelLayout = { GUILayout.Width( 50 ) };


	}

}