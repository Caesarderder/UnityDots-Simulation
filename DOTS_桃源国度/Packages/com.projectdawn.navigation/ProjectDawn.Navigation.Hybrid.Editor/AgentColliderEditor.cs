using UnityEngine;
using UnityEditor;
using Unity.Entities;
using ProjectDawn.Navigation;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentColliderAuthoring))]
    class AgentColliderEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent Layers = EditorGUIUtility.TrTextContent("Layers", "");
        }

        SerializedProperty m_Layers;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Layers, Styles.Layers);
            serializedObject.ApplyModifiedProperties();
            
            if (!serializedObject.isEditingMultipleObjects)
            {
                if (target is AgentColliderAuthoring collider && (collider.gameObject.GetComponent<Collider>() != null || collider.gameObject.GetComponent<Collider2D>()))
                    EditorGUILayout.HelpBox("This game object already contains physics collider!", MessageType.Warning);
            }
        }

        void OnEnable()
        {
            m_Layers = serializedObject.FindProperty("m_Layers");
        }
    }
}
