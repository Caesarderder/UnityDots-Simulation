using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ProjectDawn.Navigation.Editor
{
    [CustomEditor(typeof(AgentsNavigationSettings))]
    public class AgentsNavigationSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty m_SubSettings;

        AgentsNavigationSettings Settings => target as AgentsNavigationSettings;

        void OnEnable()
        {
            if (serializedObject == null)
                return;

            foreach (var type in AgentsNavigationSettings.FindTypesInAssemblies())
            {
                if (Settings.Contains(type))
                    continue;

                var subSettings = Activator.CreateInstance(type) as ISubSettings;
                Settings.SubSettings.Add(subSettings);
            }

            m_SubSettings = serializedObject.FindProperty("m_SubSettings");
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject == null)
                return;

            serializedObject.UpdateIfRequiredOrScript();
            for (int i = 0; i < m_SubSettings.arraySize; i++)
            {
                SerializedProperty subSettings = m_SubSettings.GetArrayElementAtIndex(i);
                string name = ObjectNames.NicifyVariableName(subSettings.type.Replace("managedReference<", String.Empty).Replace(">", String.Empty));
                EditorGUILayout.PropertyField(subSettings, EditorGUIUtility.TrTextContent(name), true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
