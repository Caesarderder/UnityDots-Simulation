using UnityEngine;
using UnityEditor;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentSmartStopAuthoring))]
    class AgentSmartStopEditor : UnityEditor.Editor
    {
        static class Styles
        {
            public static readonly GUIContent HiveMindStop = EditorGUIUtility.TrTextContent("Hive Mind Stop", "This option allows agent to do smarter stop decision than moving in group. It works under assumption that by reaching nearby agent that is already idle and have similar destination it can stop as destination is reached.");
            public static readonly GUIContent GiveUpStop = EditorGUIUtility.TrTextContent("Give Up Stop", "This option allows agent to do smarter stop decision than moving in group. It works under assumption that by reaching nearby agent that is already idle and have similar destination it can stop as destination is reached.");
        }

        SerializedProperty m_HiveMindStop;
        SerializedProperty m_GiveUpStop;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_HiveMindStop, Styles.HiveMindStop);
            EditorGUILayout.PropertyField(m_GiveUpStop, Styles.GiveUpStop);
            if (serializedObject.ApplyModifiedProperties())
            {
                // Update all agents entities
                foreach (var target in targets)
                {
                    var authoring = target as AgentSmartStopAuthoring;
                    if (authoring.HasEntitySmartStop)
                        authoring.EntitySmartStop = authoring.DefaulSmartStop;
                }
            }
        }

        void OnEnable()
        {
            m_HiveMindStop = serializedObject.FindProperty("m_HiveMindStop");
            m_GiveUpStop = serializedObject.FindProperty("m_GiveUpStop");
        }
    }
}
