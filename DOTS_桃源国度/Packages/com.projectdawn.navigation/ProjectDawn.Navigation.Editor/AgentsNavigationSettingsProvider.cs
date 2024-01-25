using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace ProjectDawn.Navigation.Editor
{
    class AgentsNavigationSettingsProvider : SettingsProvider
    {
        static class Styles
        {
            public static readonly GUIContent SonarTimeHorizon = EditorGUIUtility.TrTextContent("Sonar Time Horizon", "Changes sonar avoidance radius to be based on velocity and also navmesh collision velocity accounts collision.");
            public static readonly GUIContent UseRegularUpdate = EditorGUIUtility.TrTextContent("Use Regular Update", "The enabled agents will use regular updates instead of fixed ones. Fixed updates provide a more deterministic motion and a lower possibility for agents to pass through.");
            public static readonly GUIStyle lineStyle = new GUIStyle();
            public static readonly GUIStyle centerStyle = new GUIStyle();

            static Styles()
            {
                centerStyle.alignment = TextAnchor.MiddleCenter;

                // Initialize the line style
                lineStyle = new GUIStyle();
                lineStyle.normal.background = EditorGUIUtility.whiteTexture; // Use a white texture as the line color
                lineStyle.margin = new RectOffset(0, 0, 4, 4); // Add some margin to the line
            }
        }

        AgentsNavigationSettings m_Settings;
        AgentsNavigationSettingsEditor m_Editor;

        public AgentsNavigationSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            label = "Agents Navigation";
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_Settings = AgentsNavigationSettings.Instance;
            m_Editor = UnityEditor.Editor.CreateEditor(m_Settings) as AgentsNavigationSettingsEditor;
        }

        public override void OnDeactivate()
        {
            if (m_Editor != null)
                Object.DestroyImmediate(m_Editor);
            m_Editor = null;
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            m_Settings = (AgentsNavigationSettings)EditorGUILayout.ObjectField(m_Settings, typeof(AgentsNavigationSettings), false);
            if (EditorGUI.EndChangeCheck())
            {
                AgentsNavigationSettings.Instance = m_Settings;
                if (m_Editor != null)
                    Object.DestroyImmediate(m_Editor);
                m_Editor = UnityEditor.Editor.CreateEditor(m_Settings) as AgentsNavigationSettingsEditor;
            }

            GUILayout.Space(10);

            if (m_Settings != null)
            {
                DrawHorizontalLine();

                using (new EditorGUI.DisabledScope(Application.isPlaying))
                {
                    m_Editor?.OnInspectorGUI();
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"Settings asset can be created by clicking menu item Assets>Create>AI>Agents Navigation Settings.", MessageType.Info);
            }

            GUILayout.Space(10);

            DrawHorizontalLine();

            ScriptingDefineToggleField.Draw(Styles.SonarTimeHorizon, "EXPERIMENTAL_SONAR_TIME");
            Rect controlRect = EditorGUILayout.GetControlRect();
            controlRect.height = 20;
            EditorGUI.HelpBox(controlRect, $"This feature should result better sonar avoidance, but for now it is experimental! Make sure commit changes, before turning it on.", MessageType.Info);

            ScriptingDefineToggleField.Draw(Styles.UseRegularUpdate, "AGENTS_NAVIGATION_REGULAR_UPDATE");

            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
        }

        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider() => new AgentsNavigationSettingsProvider("Project/AgentsNavigation", SettingsScope.Project);

        static void DrawHorizontalLine()
        {
            Rect lineRect = GUILayoutUtility.GetRect(GUIContent.none, Styles.lineStyle, GUILayout.Height(1)); // Set the height of the line to 1

            // Check the current skin and adjust the line color accordingly
            if (EditorGUIUtility.isProSkin)
                GUI.color = new Color(0.10196f, 0.10196f, 0.10196f, 1); // For light skin, use a darker gray color
            else
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f); // For dark skin, use a slightly darker gray color

            // Draw the line
            GUI.Box(lineRect, GUIContent.none, Styles.lineStyle);

            // Reset the GUI color
            GUI.color = Color.white;
        }

        static string ExtractComponentName(System.Type componentType)
        {
            var attributes = componentType.GetCustomAttributes(typeof(AddComponentMenu), true);

            if (attributes.Length > 0)
            {
                var addComponentMenuAttribute = attributes[0] as AddComponentMenu;
                if (addComponentMenuAttribute != null)
                {
                    string path = addComponentMenuAttribute.componentMenu;
                    string[] menuItems = path.Split('/');
                    string componentName = menuItems.LastOrDefault();

                    return componentName;
                }
            }

            return ObjectNames.NicifyVariableName(componentType.Name);
        }
    }
}
