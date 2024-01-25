#if GRIFFIN
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.HelpTool
{
    [CustomEditor(typeof(GHelpDatabase))]
    public class GHelpDatabaseInspector : Editor
    {
        private GHelpDatabase instance;
        private bool onlyShowLastTenItems = true;

        private GUIStyle textAreaStyle;
        private GUIStyle TextAreaStyle
        {
            get
            {
                if (textAreaStyle == null)
                {
                    textAreaStyle = new GUIStyle(EditorStyles.textArea);
                }
                textAreaStyle.wordWrap = true;
                return textAreaStyle;
            }
        }

        private void OnEnable()
        {
            instance = target as GHelpDatabase;
        }

        public override void OnInspectorGUI()
        {
            DrawStatistic();

            string prefPrefix = "helpdb" + instance.GetInstanceID();
            List<GHelpEntry> entries = instance.Entries;
            int startIndex = 0;
            if (onlyShowLastTenItems)
            {
                startIndex = Mathf.Max(0, entries.Count - 10);
            }

            for (int i = startIndex; i < entries.Count; ++i)
            {
                GHelpEntry e = entries[i];
                e.Id = i + 1;
                string label = string.IsNullOrEmpty(e.Question) ?
                    "[" + e.Id.ToString("000") + "]" :
                    GEditorCommon.Ellipsis(string.Format("[{0}] {1}", e.Id.ToString("000"), e.Question), 100);
                bool expanded = GEditorCommon.Foldout(label, prefPrefix + i, false);
                if (expanded)
                {
                    EditorGUI.indentLevel += 1;
                    EditorGUILayout.LabelField("Id: " + e.Id.ToString("000"));
                    EditorGUILayout.PrefixLabel("Category");
                    e.Category = (GCategory)EditorGUILayout.EnumPopup(e.Category);
                    EditorGUILayout.PrefixLabel("Question");
                    e.Question = EditorGUILayout.TextField(e.Question);
                    EditorGUILayout.PrefixLabel("Answer");
                    e.Answer = EditorGUILayout.TextArea(e.Answer, TextAreaStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight * 5));

                    for (int j = 0; j < e.Links.Count; ++j)
                    {
                        EditorGUILayout.PrefixLabel("Link " + j + " (Text>URL)");
                        GHelpLink link = e.Links[j];
                        EditorGUILayout.BeginHorizontal();
                        link.DisplayText = EditorGUILayout.TextField(link.DisplayText);
                        link.Link = EditorGUILayout.TextField(link.Link);
                        EditorGUILayout.EndHorizontal();
                        e.Links[j] = link;
                    }

                    EditorGUI.indentLevel -= 1;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("L+", GUILayout.Width(30)))
                    {
                        e.Links.Add(new GHelpLink());
                    }
                    GUI.enabled = e.Links.Count > 0;
                    if (GUILayout.Button("L-", GUILayout.Width(30)))
                    {
                        e.Links.RemoveAt(e.Links.Count - 1);
                    }
                    GUI.enabled = true;
                    if (GUILayout.Button("Delete Entry"))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Confirm",
                            "Delete this entry?",
                            "OK", "Cancel"))
                        {
                            entries.RemoveAt(i);
                            Event.current.Use();
                            continue;
                        }
                    }
                    if (GUILayout.Button("Duplicate"))
                    {
                        GHelpEntry newEntry = entries[i];
                        newEntry.Links = new List<GHelpLink>(entries[i].Links);
                        entries.Add(newEntry);

                        string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", prefPrefix + (entries.Count - 1));
                        EditorPrefs.SetBool(prefKey, true);

                        Event.current.Use();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }

                entries[i] = e;
            }

            if (GEditorCommon.RightAnchoredButton("Add"))
            {
                entries.Add(new GHelpEntry());
            }

            EditorUtility.SetDirty(instance);
        }

        private void DrawStatistic()
        {
            List<GHelpEntry> entries = instance.Entries;
            int total = entries.Count;

            Dictionary<GCategory, int> countPerCategory = new Dictionary<GCategory, int>();
            foreach (GCategory c in Enum.GetValues(typeof(GCategory)))
            {
                int count = entries.FindAll(e => e.Category == c).Count;
                countPerCategory.Add(c, count);
            }

            EditorGUILayout.LabelField("Total", total.ToString());
            EditorGUI.indentLevel += 1;
            foreach (GCategory c in Enum.GetValues(typeof(GCategory)))
            {
                int count = countPerCategory[c];
                EditorGUILayout.LabelField(c.ToString(), count.ToString());
            }
            EditorGUI.indentLevel -= 1;

            onlyShowLastTenItems = EditorGUILayout.Toggle("Only Show Last 10 Items", onlyShowLastTenItems);

            GEditorCommon.Separator();
        }
    }
}
#endif
