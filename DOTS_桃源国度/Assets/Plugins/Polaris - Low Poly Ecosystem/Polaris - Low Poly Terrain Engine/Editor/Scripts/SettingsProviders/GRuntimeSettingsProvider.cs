#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin
{
    public class GRuntimeSettingsProvider : SettingsProvider
    {
        public GRuntimeSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {

        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250;
            Editor editor = Editor.CreateEditor(GRuntimeSettings.Instance);
            editor.OnInspectorGUI();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            GRuntimeSettingsProvider provider = new GRuntimeSettingsProvider("Project/Polaris/Runtime", SettingsScope.Project);
            return provider;
        }
    }
}
#endif
