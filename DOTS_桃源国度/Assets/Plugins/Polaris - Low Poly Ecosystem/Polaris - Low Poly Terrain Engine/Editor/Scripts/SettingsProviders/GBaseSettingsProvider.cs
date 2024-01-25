using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin
{
    public class GBaseSettingsProvider : SettingsProvider
    {
        public GBaseSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {

        }

        private class GBaseGUI
        {
            public static readonly string INFO = "Here you can find the settings for Polaris in Editor and in Runtime";
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            EditorGUILayout.LabelField(GBaseGUI.INFO);
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            GBaseSettingsProvider provider = new GBaseSettingsProvider("Project/Polaris", SettingsScope.Project);
            return provider;
        }
    }
}
