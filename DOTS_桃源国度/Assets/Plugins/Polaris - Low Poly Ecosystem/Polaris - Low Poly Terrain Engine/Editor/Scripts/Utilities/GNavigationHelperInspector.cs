#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GNavigationHelper))]
    public class GNavigationHelperInspector : Editor
    {
        private GNavigationHelper instance;

        public void OnEnable()
        {
            instance = target as GNavigationHelper;
        }

        public override void OnInspectorGUI()
        {
            instance.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", instance.GroupId);
            DrawInstructionGUI();
            DrawActionGUI();
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "instruction" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUILayout.LabelField("Create dummy game objects for Nav Mesh baking. These game objects should be removed in production.", GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawActionGUI()
        {
            string label = "Actions";
            string id = "actions" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                Rect createButtonRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(createButtonRect, "Create static obstacles"))
                {
                    instance.CreateStaticObstacles();
                }

                Rect deleteButtonRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(deleteButtonRect, "Delete static obstacles"))
                {
                    instance.DeleteStaticObstacles();
                }
            });
        }
    }
}
#endif
