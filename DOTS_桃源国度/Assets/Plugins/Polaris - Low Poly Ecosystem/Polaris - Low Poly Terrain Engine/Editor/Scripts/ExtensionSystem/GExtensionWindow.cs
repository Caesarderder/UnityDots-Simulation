#if GRIFFIN
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.ExtensionSystem
{
    public class GExtensionWindow : EditorWindow
    {
        private Vector2 ScrollPos { get; set; }

        private string searchString;
        private string SearchString
        {
            get
            {
                if (searchString == null)
                {
                    searchString = string.Empty;
                }
                return searchString;
            }
            set
            {
                searchString = value;
            }
        }

        public static void ShowWindow()
        {
            GExtensionWindow window = GetWindow<GExtensionWindow>();
            window.titleContent = new GUIContent("Extension");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        public void OnEnable()
        {
            GExtensionManager.ReloadExtensions();
        }

        public void OnDisable()
        {
        }

        public void OnGUI()
        {
            DrawToolbar();
            DrawExtensions();
        }

        private void DrawToolbar()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            Rect r = EditorGUILayout.GetControlRect();
            RectOffset offset = new RectOffset(4, 4, 0, 0);
            GUI.Box(offset.Add(r), string.Empty, EditorStyles.toolbar);

            Rect searchBoxRect = new Rect(r.max.x - 200, r.min.y, 200, r.height);
            RectOffset searchBoxOffset = new RectOffset(0, 0, 2, 2);
            SearchString = EditorGUI.TextField(searchBoxOffset.Remove(searchBoxRect), SearchString, EditorStyles.toolbarTextField);
            GUI.enabled = true;
        }

        private void DrawExtensions()
        {
            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

            List<GExtensionInfo> extensions = GExtensionManager.Extensions;
            for (int i = 0; i < extensions.Count; ++i)
            {
                if (extensions[i].Publisher.Equals("Pinwheel Studio"))
                {
                    DrawExtension(extensions[i]);
                }
            }

            for (int i = 0; i < extensions.Count; ++i)
            {
                if (!extensions[i].Publisher.Equals("Pinwheel Studio"))
                {
                    DrawExtension(extensions[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawExtension(GExtensionInfo ex)
        {
            GUI.enabled = !EditorApplication.isCompiling;
            string filter = SearchString.ToLower();
            if (!ex.Name.ToLower().Contains(filter) &&
                !ex.Publisher.ToLower().Contains(filter) &&
                !ex.Description.ToLower().Contains(filter))
                return;

            string id = "griffin-extension" + ex.Name + ex.Publisher;
            string label = ex.Name;

            GEditorCommon.Foldout(label, false, id, () =>
            {
                try
                {
                    EditorGUILayout.LabelField("Publisher", ex.Publisher);
                    EditorGUILayout.LabelField("Version", ex.Version);
                    EditorGUILayout.LabelField("Description", ex.Description, GEditorCommon.WordWrapLeftLabel);
                    if (ex.OpenUserGuideMethod != null || ex.OpenSupportLinkMethod != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Link");
                        using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(-1))
                        {
                            Rect r = EditorGUILayout.GetControlRect();
                            List<string> linkLabels = new List<string>();
                            linkLabels.Add("Support");
                            if (ex.OpenUserGuideMethod != null)
                            {
                                linkLabels.Add("User Guide");
                            }

                            List<Rect> linkRects = EditorGUIUtility.GetFlowLayoutedRects(r, EditorStyles.label, 7, 0, linkLabels);
                            for (int i = 0; i < linkRects.Count; ++i)
                            {
                                EditorGUIUtility.AddCursorRect(linkRects[i], MouseCursor.Link);
                            }

                            if (GUI.Button(linkRects[0], "Support", EditorStyles.label))
                            {
                                ex.OpenSupportLinkMethod.Invoke(null, null);
                            }
                            GEditorCommon.DrawBottomLine(linkRects[0], EditorStyles.label.normal.textColor);

                            if (ex.OpenUserGuideMethod != null)
                            {
                                if (GUI.Button(linkRects[1], "User Guide", EditorStyles.label))
                                {
                                    ex.OpenUserGuideMethod.Invoke(null, null);
                                }
                                GEditorCommon.DrawBottomLine(linkRects[1], EditorStyles.label.normal.textColor);
                            }


                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (ex.ButtonMethods.Count > 0)
                    {
                        GEditorCommon.Separator();
                        for (int i = 0; i < ex.ButtonMethods.Count; ++i)
                        {
                            MethodInfo method = ex.ButtonMethods[i];
                            if (method == null)
                                continue;

                            string buttonLabel = ObjectNames.NicifyVariableName(method.Name.Substring(GExtensionInfo.BUTTON_METHOD_PREFIX.Length));
                            if (GUILayout.Button(buttonLabel))
                            {
                                method.Invoke(null, null);
                            }
                        }
                    }

                    if (ex.GuiMethod != null)
                    {
                        GEditorCommon.Separator();
                        ex.GuiMethod.Invoke(null, null);
                    }
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField(string.Format("<color=red>Error: {0}</color>", e.ToString()), GEditorCommon.RichTextLabel);
                    Debug.LogException(e);
                }
            });
            GUI.enabled = true;
        }
    }
}
#endif
