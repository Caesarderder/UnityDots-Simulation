using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentReciprocalAvoidAuthoring))]
    class AgentReciprocalAvoidEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The maximum distance at which agent will attempt to avoid nearby agents.");
            public static readonly GUIContent Layers = EditorGUIUtility.TrTextContent("Layers", "");
        }

        SerializedProperty m_Radius;
        SerializedProperty m_Layers;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Radius, Styles.Radius);
            EditorGUILayout.PropertyField(m_Layers, Styles.Layers);
            if (serializedObject.ApplyModifiedProperties())
            {
                // Update entities
                foreach (var target in targets)
                {
                    var authoring = target as AgentReciprocalAvoidAuthoring;
                    if (authoring.HasEntityAvoid)
                        authoring.EntityAvoid = authoring.DefaultAvoid;
                }
            }

            EditorGUILayout.HelpBox("This is experimental feature. Not everything is set to work and will change in the future. Use at your own risk.", MessageType.Warning);
        }

        void OnEnable()
        {
            m_Radius = serializedObject.FindProperty("Radius");
            m_Layers = serializedObject.FindProperty("m_Layers");
        }
    }
}
