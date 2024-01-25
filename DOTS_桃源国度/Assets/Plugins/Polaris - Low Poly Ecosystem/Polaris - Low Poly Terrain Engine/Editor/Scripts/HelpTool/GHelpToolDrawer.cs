#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.HelpTool
{
    public static class GHelpToolDrawer
    {
        private static string searchText;
        private static Vector2 scrollPos;

        private static int suggestionIndex;
        private static string[] searchSuggestions = new string[]
        {
            "Create a new terrain",
            "Paint texture on terrain",
            "Convert Unity Terrain to low poly",
            "Paint tree and grass on terrain",
            "Upgrade to LWRP",
            "Create ramp, path, river using Spline Creator",
            "Using stamper tools",
            "Baking nav mesh",
            "Import and export data",
            "What's new in this version"
        };

        private static List<int> searchResult;
        private static List<int> SearchResult
        {
            get
            {
                if (searchResult == null)
                {
                    searchResult = new List<int>();
                }
                return searchResult;
            }
            set
            {
                searchResult = value;
            }
        }

        private static List<float> hitCount;
        private static List<float> HitCount
        {
            get
            {
                if (hitCount == null)
                {
                    hitCount = new List<float>();
                }
                return hitCount;
            }
            set
            {
                hitCount = value;
            }
        }

        private static bool[] expandedFlags;

        private static GUIStyle linkButtonStyle;
        private static GUIStyle LinkButtonStyle
        {
            get
            {
                if (linkButtonStyle == null)
                {
                    linkButtonStyle = new GUIStyle(EditorStyles.label);
                }
                linkButtonStyle.normal.textColor = GEditorCommon.selectedItemColor;
                linkButtonStyle.alignment = TextAnchor.UpperLeft;
                return linkButtonStyle;
            }
        }

        private static GUIStyle richTextStyle;
        private static GUIStyle RichTextStyle
        {
            get
            {
                if (richTextStyle == null)
                {
                    richTextStyle = new GUIStyle(EditorStyles.label);
                }
                richTextStyle.richText = true;
                richTextStyle.wordWrap = true;
                return richTextStyle;
            }
        }

        public static void DrawGUI()
        {
            try
            {
                DrawIcon();
                DrawSearchBox();
                DrawSearchResult();
            }
            catch
            {
                searchText = string.Empty;
                suggestionIndex = 0;
                SearchResult.Clear();
                HitCount.Clear();
                expandedFlags = null;
                OnSearchTextChanged();
            }
        }

        private static void DrawIcon()
        {
            string iconName = EditorGUIUtility.isProSkin ? "IconWhite" : "IconBlack";
            Texture2D icon = GEditorSkin.Instance.GetTexture(iconName);
            if (icon == null)
                return;

            EditorGUILayout.Space();
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(64));
            GUI.DrawTexture(r, icon, ScaleMode.ScaleToFit, true, 1);
            GEditorCommon.Space();
        }

        private static void DrawSearchBox()
        {
            EditorGUI.BeginChangeCheck();
            searchText = EditorGUILayout.TextField(searchText);
            if (EditorGUI.EndChangeCheck())
            {
                OnSearchTextChanged();
            }
            GEditorCommon.Separator();
        }

        private static void OnSearchTextChanged()
        {
            suggestionIndex = Random.Range(0, searchSuggestions.Length);
            Search();
        }

        private static void Search()
        {
            SearchResult.Clear();
            HitCount.Clear();

            if (searchText.Equals("?"))
            {
                for (int i = 0; i < GHelpDatabase.Instance.Entries.Count; ++i)
                {
                    SearchResult.Add(i);
                    HitCount.Add(1);
                    expandedFlags = new bool[SearchResult.Count];
                }
                return;
            }

            string text = searchText;
            if (text.Length < 3)
                return;

            string[] split = text.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return;
            for (int i = 0; i < split.Length; ++i)
            {
                split[i] = split[i].ToLower();
            }

            List<GHelpEntry> entries = GHelpDatabase.Instance.Entries;
            for (int i = 0; i < entries.Count; ++i)
            {
                float count = 0;
                GHelpEntry e = entries[i];
                for (int j = 0; j < split.Length; ++j)
                {
                    count += e.Question.ToLower().Contains(split[j]) ? 1 : 0;
                    count += e.Answer.ToLower().Contains(split[j]) ? 0.5f : 0;
                }

                SearchResult.Add(i);
                HitCount.Add(count);
            }

            SearchResult.Sort((i0, i1) => -HitCount[i0].CompareTo(HitCount[i1]));
            HitCount.Sort((i0, i1) => -i0.CompareTo(i1));

            expandedFlags = new bool[SearchResult.Count];
        }

        private static void DrawSearchResult()
        {
            GUtilities.EnsureArrayLength<bool>(ref expandedFlags, SearchResult.Count);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (string.IsNullOrEmpty(searchText))
            {
                DrawSearchSuggestions();
            }
            else
            {
                EditorGUI.indentLevel += 1;
                List<GHelpEntry> entries = GHelpDatabase.Instance.Entries;
                int resultCount = searchText.Equals("?") ? SearchResult.Count : Mathf.Min(10, SearchResult.Count);
                for (int i = 0; i < resultCount; ++i)
                {
                    if (HitCount[i] == 0)
                        break;
                    int index = SearchResult[i];
                    GHelpEntry e = entries[index];
                    string label = string.Format("[{0}] {1}", e.Id.ToString("000"), e.Question);
                    expandedFlags[i] = EditorGUILayout.Foldout(expandedFlags[i], label);
                    if (expandedFlags[i])
                    {
                        EditorGUI.indentLevel += 1;
                        EditorGUILayout.LabelField(e.Answer, RichTextStyle);

                        if (e.Links.Count > 0)
                        {
                            EditorGUILayout.LabelField("Link" + (e.Links.Count >= 2 ? "s" : ""));
                            for (int j = 0; j < e.Links.Count; ++j)
                            {
                                if (LinkButton("- " + e.Links[j].DisplayText, e.Links[j].Link))
                                {
                                    if (e.Links[j].Link.StartsWith("~"))
                                    {
                                        searchText = e.Links[j].Link.Substring(1);
                                        EditorGUI.FocusTextInControl(null);
                                        OnSearchTextChanged();
                                        Event.current.Use();
                                        return;
                                    }
                                    else
                                    {
                                        Application.OpenURL(e.Links[j].Link);
                                    }
                                }
                            }
                        }
                        EditorGUI.indentLevel -= 1;
                        EditorGUILayout.Space();
                    }
                }
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSearchSuggestions()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Try the following: ", GUILayout.Width(100));
            string suggestion = searchSuggestions[suggestionIndex];
            Rect r0 = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUIUtility.AddCursorRect(r0, MouseCursor.Link);
            if (GUI.Button(r0, suggestion, LinkButtonStyle))
            {
                searchText = suggestion;
                EditorGUI.FocusTextInControl(null);
                OnSearchTextChanged();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Can't find your answer? ", GUILayout.Width(137));
            Rect r1 = EditorGUILayout.GetControlRect();
            EditorGUIUtility.AddCursorRect(r1, MouseCursor.Link);
            if (GUI.Button(r1, "Ask me directly", LinkButtonStyle))
            {
                GEditorCommon.OpenEmailEditor(
                    "customer@pinwheel.studio",
                    "[POLARIS V2 - HELP] SHORT_QUESTION_HERE",
                    "YOUR_QUESTION_IN_DETAIL");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static bool LinkButton(string label, string tooltip = "")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUI.indentLevel * GEditorCommon.indentSpace));
            Rect r = EditorGUILayout.GetControlRect();
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            bool clicked = GUI.Button(r, new GUIContent(label, tooltip), LinkButtonStyle);
            EditorGUILayout.EndHorizontal();
            return clicked;
        }
    }
}
#endif
