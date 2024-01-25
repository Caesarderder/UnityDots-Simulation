using UnityEngine;
using UnityEditor;
using System;

namespace ProjectDawn.Navigation.Hybrid.Editor
{
    [Obsolete("This class is obsolete, please use new settings workflow https://lukaschod.github.io/agents-navigation-docs/manual/settings.html.")]
    [CustomEditor(typeof(SettingsBehaviour), true)]
    internal class AgentPartitioningSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script")
                    continue;

                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
