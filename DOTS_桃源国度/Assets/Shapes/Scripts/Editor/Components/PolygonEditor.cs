using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(Polygon) )]
	[CanEditMultipleObjects]
	public class PolygonEditor : ShapeRendererEditor {

		SerializedProperty propPoints = null;
		SerializedProperty propTriangulation = null;
		SerializedProperty propFill = null;
		SerializedProperty propUseFill = null;

		SceneFillEditor fillEditor;
		ScenePointEditor scenePointEditor;
		ReorderableList pointList;

		const int MANY_POINTS = 20;
		[SerializeField] bool hasManyPoints;
		[SerializeField] bool showPointList = true;

		public override void OnEnable() {
			base.OnEnable();

			fillEditor = new SceneFillEditor( this, propFill, propUseFill );
			scenePointEditor = new ScenePointEditor( this );

			pointList = new ReorderableList( serializedObject, propPoints, true, true, true, true ) {
				drawElementCallback = DrawPointElement,
				drawHeaderCallback = PointListHeader
			};

			if( pointList.count > MANY_POINTS ) {
				hasManyPoints = true;
				showPointList = false;
			}
		}

		public override void OnInspectorGUI() {
			base.BeginProperties();
			EditorGUILayout.PropertyField( propTriangulation );

			bool changed = fillEditor.DrawProperties( this );

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
			Polygon p = target as Polygon;
			GradientFill fill = p.Fill;
			bool changed = fillEditor.DoSceneHandles( p.UseFill, p, ref fill, p.transform );
			changed |= scenePointEditor.DoSceneHandles( closed: true, p, p.points, p.transform );
			if( changed ) {
				p.Fill = fill;
				p.UpdateMesh( true );
				p.UpdateAllMaterialProperties();
			}
		}

		void PointListHeader( Rect r ) => EditorGUI.LabelField( r, "Points" );

		// Draws the elements on the list
		void DrawPointElement( Rect r, int i, bool isActive, bool isFocused ) {
			r.yMin += 1;
			r.yMax -= 2;
			SerializedProperty prop = propPoints.GetArrayElementAtIndex( i );
			EditorGUI.PropertyField( r, prop, GUIContent.none );
		}


	}

}