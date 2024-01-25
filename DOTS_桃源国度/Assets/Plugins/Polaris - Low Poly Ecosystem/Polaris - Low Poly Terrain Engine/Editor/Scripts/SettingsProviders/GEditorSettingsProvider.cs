#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin
{
    public class GEditorSettingsProvider : SettingsProvider
    {
        public GEditorSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {

        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250;
            Editor editor = Editor.CreateEditor(GEditorSettings.Instance);
            editor.OnInspectorGUI();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            GEditorSettingsProvider provider = new GEditorSettingsProvider("Project/Polaris/Editor", SettingsScope.Project);
            return provider;
        }
    }
}
#endif