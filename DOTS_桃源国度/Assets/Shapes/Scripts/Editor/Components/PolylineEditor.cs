using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Polyline) )]
	[CanEditMultipleObjects]
	public class PolylineEditor : ShapeRendererEditor {

		SerializedProperty propPoints = null;
		SerializedProperty propGeometry = null;
		SerializedProperty propJoins = null;
		SerializedProperty propClosed = null;
		SerializedProperty propThickness = null;
		SerializedProperty propThicknessSpace = null;

		ReorderableList pointList;

		ScenePointEditor scenePointEditor;

		const int MANY_POINTS = 20;
		[SerializeField] bool hasManyPoints;
		[SerializeField] bool showPointList = true;
		bool showZ;

		public override void OnEnable() {
			base.OnEnable();

			pointList = new ReorderableList( serializedObject, propPoints, true, true, true, true ) {
				drawElementCallback = DrawPointElement,
				drawHeaderCallback = PointListHeader
			};

			if( pointList.count > MANY_POINTS ) {
				hasManyPoints = true;
				showPointList = false;
			}

			scenePointEditor = new ScenePointEditor( this ) { hasEditThicknessMode = true, hasEditColorMode = true };
		}

		public override void OnInspectorGUI() {
			base.BeginProperties();
			if( Event.current.type == EventType.Layout )
				showZ = targets.Any( x => ( (Polyline)x ).Geometry != PolylineGeometry.Flat2D );
			EditorGUILayout.PropertyField( propGeometry );
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( propJoins );
			ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );

			if( hasManyPoints ) { // to prevent lag when inspecting polylines with many points
				string foldoutLabel = showPointList ? "Hide" : "Show Points";
				showPointList = GUILayout.Toggle( showPointList, foldoutLabel, EditorStyles.foldout );
			}

			if( showPointList )
				pointList.DoLayoutList();

			scenePointEditor.GUIEditButton( "Edit Points in Scene" );

			base.EndProperties();
		}

		void OnSceneGUI() {
			Polyline p = target as Polyline;
			scenePointEditor.useFlatThicknessHandles = p.Geometry == PolylineGeometry.Flat2D;
			scenePointEditor.hasEditThicknessMode = p.ThicknessSpace == ThicknessSpace.Meters;
			bool changed = scenePointEditor.DoSceneHandles( p.Closed, p, p.points, p.transform, p.Thickness, p.Color );
			if( changed )
				p.UpdateMesh( force: true );
		}

		void PointListHeader( Rect r ) {
			const int checkboxSize = 14;
			const int closedSize = 50;

			Rect rLabel = r;
			rLabel.width = r.width - checkboxSize - closedSize;
			Rect rCheckbox = r;
			rCheckbox.x = r.xMax - checkboxSize;
			rCheckbox.width = checkboxSize;
			Rect rClosed = r;
			rClosed.x = rLabel.xMax;
			rClosed.width = closedSize;
			EditorGUI.LabelField( rLabel, "Points" );
			EditorGUI.LabelField( rClosed, "Closed" );
			EditorGUI.PropertyField( rCheckbox, propClosed, GUIContent.none );
		}

		// Draws the elements on the list
		void DrawPointElement( Rect r, int i, bool isActive, bool isFocused ) {
			r.yMin += 1;
			r.yMax -= 2;
			SerializedProperty prop = propPoints.GetArrayElementAtIndex( i );
			SerializedProperty pPoint = prop.FindPropertyRelative( nameof(PolylinePoint.point) );
			SerializedProperty pThickness = prop.FindPropertyRelative( nameof(PolylinePoint.thickness) );
			SerializedProperty pColor = prop.FindPropertyRelative( nameof(PolylinePoint.color) );

			using( var chChk = new EditorGUI.ChangeCheckScope() ) {
				ShapesUI.PosThicknessColorField( r, pPoint, pThickness, pColor, colorEnabled: true, showZ );
				if( chChk.changed )
					pThickness.floatValue = Mathf.Max( 0.001f, pThickness.floatValue ); // Make sure it's never 0 or under
			}
		}


	}

}