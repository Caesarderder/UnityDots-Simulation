using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentCylinderShapeAuthoring))]
    class AgentCylinderShapeEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The radius of the agent.");
            public static readonly GUIContent Height = EditorGUIUtility.TrTextContent("Height", "The height of the agent for purposes of passing under obstacles, etc.");
            public static readonly Color32 Color = new Color32(255, 166, 89, 255);
        }

        SerializedProperty m_Radius;
        SerializedProperty m_Height;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Radius, Styles.Radius);
            EditorGUILayout.PropertyField(m_Height, Styles.Height);
            if (serializedObject.ApplyModifiedProperties())
            {
                // Update all agents entities shape
                foreach (var target in targets)
                {
                    var authoring = target as AgentCylinderShapeAuthoring;
                    if (authoring.HasEntityShape)
                        authoring.EntityShape = authoring.DefaultShape;
                }
            }
        }

        void OnSceneGUI()
        {
            var shape = target as AgentCylinderShapeAuthoring;
            var transform = shape.transform;
            float height = shape.DefaultShape.Height;
            float radius = shape.DefaultShape.Radius;
            float start = 0;
            float end = height;

            Handles.color = Styles.Color;

            Handles.DrawWireDisc(transform.position + Vector3.up * end, Vector3.up, radius);
            Handles.DrawWireDisc(transform.position + Vector3.up * start, Vector3.up, radius);

            Handles.DrawLine(transform.position + new Vector3(radius, end, 0), transform.position + new Vector3(radius, start, 0));
            Handles.DrawLine(transform.position + new Vector3(-radius, end, 0), transform.position + new Vector3(-radius, start, 0));
            Handles.DrawLine(transform.position + new Vector3(0, end, radius), transform.position + new Vector3(0, start, radius));
            Handles.DrawLine(transform.position + new Vector3(0, end, -radius), transform.position + new Vector3(0, start, -radius));
        }

        void OnEnable()
        {
            m_Radius = serializedObject.FindProperty("Radius");
            m_Height = serializedObject.FindProperty("Height");
        }
    }
}
