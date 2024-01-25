#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.HelpTool
{
    public class GHelpEditor : EditorWindow
    {
        public static GHelpEditor Instance { get; set; }

        public static void ShowWindow()
        {
            GHelpEditor window = EditorWindow.GetWindow<GHelpEditor>();
            window.minSize = new Vector2(300, 300);
            window.titleContent = new GUIContent("Help");
            window.Show();
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void OnGUI()
        {
            GHelpToolDrawer.DrawGUI();
        }
    }
}
#endif
