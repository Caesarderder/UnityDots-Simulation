using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentCircleShapeAuthoring))]
    class AgentCircleShapeEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Radius = EditorGUIUtility.TrTextContent("Radius", "The radius of the agent.");
            public static readonly Color32 Color = new Color32(255, 166, 89, 255);
        }

        SerializedProperty m_Radius;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Radius, Styles.Radius);

            if (serializedObject.ApplyModifiedProperties())
            {
                // Update all agents entities shape
                foreach (var target in targets)
                {
                    var authoring = target as AgentCircleShapeAuthoring;
                    if (authoring.HasEntityShape)
                        authoring.EntityShape = authoring.DefaultShape;
                }
            }
        }

        void OnSceneGUI()
        {
            var shape = target as AgentCircleShapeAuthoring;
            var transform = shape.transform;

            Handles.color = Styles.Color;
            Handles.DrawWireDisc(transform.position, Vector3.forward, shape.DefaultShape.Radius);
        }

        void OnEnable()
        {
            m_Radius = serializedObject.FindProperty("Radius");
        }
    }
}
