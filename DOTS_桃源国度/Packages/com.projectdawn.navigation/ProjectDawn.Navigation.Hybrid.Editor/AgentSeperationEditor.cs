using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentSeparationAuthoring))]
    class AgentSeparationEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The radius of the separation.");
            public static readonly GUIContent Weight = EditorGUIUtility.TrTextContent("Weight", "The weight of the separation force.");
            public static readonly GUIContent Layers = EditorGUIUtility.TrTextContent("Layers", "The weight of the separation force.");
            public static readonly Color32 Color = new Color32(255, 0, 0, 255);
        }

        SerializedProperty m_Radius;
        SerializedProperty m_Weight;
        SerializedProperty m_Layers;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Radius, Styles.Radius);
            EditorGUILayout.PropertyField(m_Layers, Styles.Layers);
            if (serializedObject.ApplyModifiedProperties())
            {
                // Update all agents entities shape
                foreach (var target in targets)
                {
                    var authoring = target as AgentSeparationAuthoring;
                    if (authoring.HasEntitySeparation)
                        authoring.EntitySeparation = authoring.DefaulSeparation;
                }
            }
        }

        void OnSceneGUI()
        {
            var authoring = target as AgentSeparationAuthoring;
            var transform = authoring.transform;

            Handles.color = Styles.Color;
            Handles.DrawWireDisc(transform.position, Vector3.forward, authoring.DefaulSeparation.Radius);
        }

        void OnEnable()
        {
            m_Radius = serializedObject.FindProperty("Radius");
            m_Weight = serializedObject.FindProperty("Weight");
            m_Layers = serializedObject.FindProperty("m_Layers");
        }
    }
}
