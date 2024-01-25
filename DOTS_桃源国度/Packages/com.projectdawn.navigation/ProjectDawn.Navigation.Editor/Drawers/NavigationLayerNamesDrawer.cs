using UnityEditor;
using UnityEngine;

namespace ProjectDawn.Navigation.Editor
{
    [CustomPropertyDrawer(typeof(NavigationLayerNames))]
    class NavigationLayerNamesDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var layerNames = property.FindPropertyRelative("m_Names");
            return EditorGUI.GetPropertyHeight(layerNames, label);
        }

        public override void OnGUI(Rect controlRect, SerializedProperty property, GUIContent label)
        {
            var layerNames = property.FindPropertyRelative("m_Names");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(controlRect, layerNames, label, true);
            if (EditorGUI.EndChangeCheck() && layerNames.arraySize > 32)
            {
                layerNames.arraySize = 32;
            }
        }
    }
}
