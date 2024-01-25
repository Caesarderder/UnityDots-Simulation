using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FlockGroupAuthoring))]
    class FlockGroupEditor : UnityEditor.Editor
    {
        SerializedProperty m_Leader;
        SerializedProperty m_Radius;
        SerializedProperty m_Cohesion;
        SerializedProperty m_Alignment;
        SerializedProperty m_Agents;
        SerializedProperty m_IncludeHierachy;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Leader);
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_Cohesion);
            EditorGUILayout.PropertyField(m_Alignment);
            EditorGUILayout.PropertyField(m_Agents);
            EditorGUILayout.PropertyField(m_IncludeHierachy);
            if (serializedObject.ApplyModifiedProperties())
            {
                // Update all agents entities shape
                foreach (var target in targets)
                {
                    var authoring = target as FlockGroupAuthoring;
                }
            }

            EditorGUILayout.HelpBox("This is experimental feature. Not everything is set to work and will change in the future. Use at your own risk.", MessageType.Warning);
        }

        void OnEnable()
        {
            m_Leader = serializedObject.FindProperty("Leader");
            m_Radius = serializedObject.FindProperty("Radius");
            m_Cohesion = serializedObject.FindProperty("Cohesion");
            m_Alignment = serializedObject.FindProperty("Alignment");
            m_Agents = serializedObject.FindProperty("Agents");
            m_IncludeHierachy = serializedObject.FindProperty("m_IncludeHierachy");
        }
    }
}
