using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(ShapeGroup) )]
	[CanEditMultipleObjects]
	public class ShapeGroupEditor : Editor {

		SerializedProperty propColor;

		void OnEnable() => propColor = serializedObject.FindProperty( "color" );

		public override void OnInspectorGUI() {
			serializedObject.Update();
			EditorGUILayout.PropertyField( propColor, new GUIContent("Color Tint") );
			serializedObject.ApplyModifiedProperties();
		}

	}

}