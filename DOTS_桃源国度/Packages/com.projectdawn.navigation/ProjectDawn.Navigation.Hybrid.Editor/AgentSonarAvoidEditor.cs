using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentAvoidAuthoring))]
    class AgentSonarAvoidEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The maximum distance at which agent will attempt to avoid nearby agents.");
            public static readonly GUIContent Angle = EditorGUIUtility.TrTextContent("Velocity Obstacle Angle", "The angle of obstacle inserted behind agent velocity.");
            public static readonly GUIContent MaxAngle = EditorGUIUtility.TrTextContent("Max Angle", "The maximum angle at which agent will attempt to nearby agents.");
            public static readonly GUIContent Mode = EditorGUIUtility.TrTextContent("Mode", "Mode that modifies avoidance behaviour.");
            public static readonly GUIContent BlockedStop = EditorGUIUtility.TrTextContent("Blocked Stop", "Whenever agent should stop, if all directions are blocked.");
            public static readonly GUIContent UseWalls = EditorGUIUtility.TrTextContent("Use Walls", "Should avoidance account for static obstacles. Having this option enable will cost more performance.");
            public static readonly GUIContent Layers = EditorGUIUtility.TrTextContent("Layers", "Should avoidance account for static obstacles. Having this option enable will cost more performance.");
        }

        SerializedProperty m_Radius;
        SerializedProperty m_Angle;
        SerializedProperty m_MaxAngle;
        SerializedProperty m_Mode;
        SerializedProperty m_BlockedStop;
        SerializedProperty m_UseWalls;
        SerializedProperty m_Layers;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Radius, Styles.Radius);
            EditorGUILayout.PropertyField(m_Angle, Styles.Angle);
            EditorGUILayout.PropertyField(m_MaxAngle, Styles.MaxAngle);
#if EXPERIMENTAL_SONAR_TIME
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(m_Mode, Styles.Mode);
                Rect controlRect = EditorGUILayout.GetControlRect();
                controlRect.height = 20;
                EditorGUI.HelpBox(controlRect, "This option is skipped with `Sonar Time Horizon` enabled!", MessageType.Warning);
            }
#else
            EditorGUILayout.PropertyField(m_Mode, Styles.Mode);
#endif
            EditorGUILayout.PropertyField(m_BlockedStop, Styles.BlockedStop);
            if (!serializedObject.isEditingMultipleObjects)
            {
                var authoring = target as AgentAvoidAuthoring;
                bool hasNavMesh = authoring.GetComponent<AgentNavMeshAuthoring>();
                using (new EditorGUI.DisabledScope(!hasNavMesh))
                    EditorGUILayout.PropertyField(m_UseWalls, Styles.UseWalls);
                if (!hasNavMesh)
                    EditorGUILayout.HelpBox("Use Walls property requires AgentNavMeshAuthoring component!", MessageType.Info);
            }
            EditorGUILayout.PropertyField(m_Layers, Styles.Layers);

            if (serializedObject.ApplyModifiedProperties())
            {
                // Update entities
                foreach (var target in targets)
                {
                    var authoring = target as AgentAvoidAuthoring;
                    if (authoring.HasEntityAvoid)
                        authoring.EntityAvoid = authoring.DefaultAvoid;
                }
            }
        }

        void OnEnable()
        {
            m_Radius = serializedObject.FindProperty("Radius");
            m_Angle = serializedObject.FindProperty("Angle");
            m_MaxAngle = serializedObject.FindProperty("MaxAngle");
            m_Mode = serializedObject.FindProperty("Mode");
            m_BlockedStop = serializedObject.FindProperty("BlockedStop");
            m_UseWalls = serializedObject.FindProperty("UseWalls");
            m_Layers = serializedObject.FindProperty("m_Layers");
        }
    }
}
