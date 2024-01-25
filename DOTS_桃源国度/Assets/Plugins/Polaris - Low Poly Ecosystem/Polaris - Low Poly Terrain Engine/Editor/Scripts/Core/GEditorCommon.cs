#if GRIFFIN
#if UNITY_EDITOR
using Pinwheel.Griffin.BackupTool;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Action = System.Action;
using Type = System.Type;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
#if GRIFFIN_VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationStudio;
using Pinwheel.Griffin.VegetationStudioPro;
#endif
using UnityEditorInternal;
using System.Reflection;

namespace Pinwheel.Griffin
{
    public static partial class GEditorCommon
    {
        public static string[] DOT_ANIM = new string[4] { "   ", "●  ", "●● ", "●●●" };
        public static string caretRight = "►";
        public static string caretDown = "▼";
        public static string contextIconText = "≡";
        public static string dot = "●";

        public static float standardHeight = EditorGUIUtility.singleLineHeight;
        public static float tinyHeight = EditorGUIUtility.singleLineHeight;

        public static float standardWidth = 100;
        public static float standardWidthExtent = 200;
        public static float tinyWidth = EditorGUIUtility.singleLineHeight;
        public static float indentSpace = 11;
        public static float objectSelectorDragDropHeight = 55;
        public static Vector2 selectionGridTileSizeSmall = new Vector2(50, 50);
        public static Vector2 selectionGridTileSizeMedium = new Vector2(75, 75);
        public static Vector2 selectionGridTileSizeLarge = new Vector2(100, 100);
        public static Vector2 selectionGridTileSizeWide = new Vector2(110, 18);

        public static Color oddItemColor = EditorGUIUtility.isProSkin ? new Color32(55, 55, 55, 255) : new Color32(190, 190, 190, 255);
        public static Color evenItemColor = EditorGUIUtility.isProSkin ? new Color32(50, 50, 50, 255) : new Color32(180, 180, 180, 255);
        public static Color selectedItemColor = EditorGUIUtility.isProSkin ? new Color32(62, 95, 150, 255) : new Color32(62, 125, 231, 255);
        public static Color criticalItemColor = new Color32(220, 40, 0, 255);
        public static Color lightSkinInspectorHeaderBackground = new Color32(221, 221, 221, 255);
        public static Color lightGrey = new Color32(176, 176, 176, 255);
        public static Color midGrey = new Color32(128, 128, 128, 255);
        public static Color darkGrey = new Color32(64, 64, 64, 255);
        public static Color assetPreviewGrey = new Color32(82, 82, 82, 255);
        public static Color linkColor = new Color32(60, 150, 200, 255);

        public static Color boxBorderColor = EditorGUIUtility.isProSkin ? new Color32(36, 36, 36, 255) : new Color32(127, 127, 127, 255);
        public static Color boxHeaderBg = EditorGUIUtility.isProSkin ? new Color32(53, 53, 53, 255) : new Color32(182, 182, 182, 255);
        public static Color boxBodyBg = EditorGUIUtility.isProSkin ? new Color32(65, 65, 65, 255) : new Color32(200, 200, 200, 255);

        public static RectOffset boxOffset = new RectOffset(2, 2, 2, 2);

        private static GUIStyle centeredMiniLabel;
        public static GUIStyle CenteredMiniLabel
        {
            get
            {
                if (centeredMiniLabel == null)
                {
                    centeredMiniLabel = new GUIStyle(EditorStyles.miniLabel);
                    centeredMiniLabel.alignment = TextAnchor.MiddleCenter;
                    //centeredMiniLabel.fontSize = 8;
                }

                return centeredMiniLabel;
            }
        }

        private static GUIStyle wordWrapLeftLabel;
        public static GUIStyle WordWrapLeftLabel
        {
            get
            {
                if (wordWrapLeftLabel == null)
                {
                    wordWrapLeftLabel = new GUIStyle(EditorStyles.label);
                    wordWrapLeftLabel.alignment = TextAnchor.MiddleLeft;
                    wordWrapLeftLabel.wordWrap = true;
                }

                return wordWrapLeftLabel;
            }
        }

        private static GUIStyle wordWrapItalicLabel;
        public static GUIStyle WordWrapItalicLabel
        {
            get
            {
                if (wordWrapItalicLabel == null)
                {
                    wordWrapItalicLabel = new GUIStyle(EditorStyles.label);
                    wordWrapItalicLabel.alignment = TextAnchor.MiddleLeft;
                    wordWrapItalicLabel.wordWrap = true;
                    wordWrapItalicLabel.fontStyle = FontStyle.Italic;
                }

                return wordWrapItalicLabel;
            }
        }

        private static GUIStyle warningLabel;
        public static GUIStyle WarningLabel
        {
            get
            {
                if (warningLabel == null)
                {
                    warningLabel = new GUIStyle(EditorStyles.label);
                    warningLabel.alignment = TextAnchor.MiddleLeft;
                    warningLabel.wordWrap = true;
                    warningLabel.fontStyle = FontStyle.Italic;
                    warningLabel.richText = true;
                    warningLabel.normal.textColor = EditorGUIUtility.isProSkin ?
                        new Color(1, 0.7f, 0.3f, 1) :
                        new Color(1, 0.5f, 0.2f, 1);
                }
                return warningLabel;
            }
        }

        private static GUIStyle centeredLabel;
        public static GUIStyle CenteredLabel
        {
            get
            {
                if (centeredLabel == null)
                {
                    centeredLabel = new GUIStyle(EditorStyles.label);
                    centeredLabel.alignment = TextAnchor.MiddleCenter;
                    centeredLabel.wordWrap = true;
                }

                return centeredLabel;
            }
        }

        private static GUIStyle centeredBoldLabel;
        public static GUIStyle CenteredBoldLabel
        {
            get
            {
                if (centeredBoldLabel == null)
                {
                    centeredBoldLabel = new GUIStyle(EditorStyles.label);
                    centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
                    centeredBoldLabel.wordWrap = true;
                    centeredBoldLabel.fontStyle = FontStyle.Bold;
                }

                return centeredBoldLabel;
            }
        }

        private static GUIStyle centeredWhiteLabel;
        public static GUIStyle CenteredWhiteLabel
        {
            get
            {
                if (centeredWhiteLabel == null)
                {
                    centeredWhiteLabel = new GUIStyle();
                    centeredWhiteLabel.alignment = TextAnchor.MiddleCenter;
                    centeredWhiteLabel.normal.textColor = Color.white;
                    centeredWhiteLabel.wordWrap = true;
                }

                return centeredWhiteLabel;
            }
        }

        private static GUIStyle linkLabel;
        public static GUIStyle LinkLabel
        {
            get
            {
                if (linkLabel == null)
                {
                    linkLabel = new GUIStyle(CenteredLabel);
                    linkLabel.normal.textColor = linkColor;
                }
                return linkLabel;
            }
        }

        public static GUIStyle BoldLabel
        {
            get
            {
                return EditorStyles.boldLabel;
            }
        }

        private static GUIStyle italicLabel;
        public static GUIStyle ItalicLabel
        {
            get
            {
                if (italicLabel == null)
                {
                    italicLabel = new GUIStyle(EditorStyles.label);
                    italicLabel.fontStyle = FontStyle.Italic;
                    italicLabel.alignment = TextAnchor.MiddleLeft;
                }
                return italicLabel;
            }
        }

        private static GUIStyle boldHeader;
        public static GUIStyle BoldHeader
        {
            get
            {
                if (boldHeader == null)
                {
                    boldHeader = new GUIStyle(EditorStyles.boldLabel);
                    boldHeader.fontStyle = FontStyle.Bold;
                    boldHeader.alignment = TextAnchor.MiddleLeft;
                }
                return boldHeader;
            }
        }

        private static GUIStyle rightAlignedItalicLabel;
        public static GUIStyle RightAlignedItalicLabel
        {
            get
            {
                if (rightAlignedItalicLabel == null)
                {
                    rightAlignedItalicLabel = new GUIStyle(ItalicLabel);
                    rightAlignedItalicLabel.alignment = TextAnchor.MiddleRight;
                }
                return rightAlignedItalicLabel;
            }
        }

        private static GUIStyle rightAlignedItalicWhiteLabel;
        public static GUIStyle RightAlignedItalicWhiteLabel
        {
            get
            {
                if (rightAlignedItalicWhiteLabel == null)
                {
                    rightAlignedItalicWhiteLabel = new GUIStyle();
                    rightAlignedItalicWhiteLabel.fontStyle = FontStyle.Italic;
                    rightAlignedItalicWhiteLabel.fontSize = 12;
                    rightAlignedItalicWhiteLabel.alignment = TextAnchor.MiddleRight;
                    rightAlignedItalicWhiteLabel.normal.textColor = Color.white;
                }
                return rightAlignedItalicWhiteLabel;
            }
        }

        private static GUIStyle rightAlignedWhiteTinyLabel;
        public static GUIStyle RightAlignedWhiteTinyLabel
        {
            get
            {
                if (rightAlignedWhiteTinyLabel == null)
                {
                    rightAlignedWhiteTinyLabel = new GUIStyle(EditorStyles.whiteMiniLabel);
                    rightAlignedWhiteTinyLabel.alignment = TextAnchor.MiddleRight;
                    rightAlignedWhiteTinyLabel.normal.textColor = Color.white;
                }
                return rightAlignedWhiteTinyLabel;
            }
        }

        private static GUIStyle sceneViewBoxContent;
        public static GUIStyle SceneViewBoxContent
        {
            get
            {
                if (sceneViewBoxContent == null)
                {
                    sceneViewBoxContent = new GUIStyle(EditorStyles.label);
                    sceneViewBoxContent.padding = new RectOffset(10, 10, 10, 10);
                }
                return sceneViewBoxContent;
            }
        }

        private static Texture2D sliderBackgroundTexture;
        public static Texture2D SliderBackgroundTexture
        {
            get
            {
                if (sliderBackgroundTexture == null)
                {
                    //sliderBackgroundTexture = EditorGUIUtility.Load("icons/sv_icon_dot0_sml.png") as Texture2D;
                    sliderBackgroundTexture = Texture2D.whiteTexture;
                }
                return sliderBackgroundTexture;
            }
        }

        private static Texture2D warningIcon;
        public static Texture2D WarningIcon
        {
            get
            {
                if (warningIcon == null)
                {
                    warningIcon = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
                }
                return warningIcon;
            }
        }

        private static Texture2D oddItemTexture1x1;
        public static Texture2D OddItemTexture1x1
        {
            get
            {
                if (oddItemTexture1x1 == null)
                {
                    oddItemTexture1x1 = new Texture2D(1, 1);
                    oddItemTexture1x1.SetPixels(new Color[] { oddItemColor });
                    oddItemTexture1x1.Apply();
                }
                return oddItemTexture1x1;
            }
        }

        private static Texture2D oddItemHoveredTexture1x1;
        public static Texture2D OddItemHoveredTexture1x1
        {
            get
            {
                if (oddItemHoveredTexture1x1 == null)
                {
                    oddItemHoveredTexture1x1 = new Texture2D(1, 1);
                    oddItemHoveredTexture1x1.SetPixels(new Color[] { oddItemColor * 1.2f });
                    oddItemHoveredTexture1x1.Apply();
                }
                return oddItemHoveredTexture1x1;
            }
        }

        private static Texture2D oddItemClickedTexture1x1;
        public static Texture2D OddItemClickedTexture1x1
        {
            get
            {
                if (oddItemClickedTexture1x1 == null)
                {
                    oddItemClickedTexture1x1 = new Texture2D(1, 1);
                    oddItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    oddItemClickedTexture1x1.Apply();
                }
                return oddItemClickedTexture1x1;
            }
        }

        private static Texture2D evenItemTexture1x1;
        public static Texture2D EvenItemTexture1x1
        {
            get
            {
                if (evenItemTexture1x1 == null)
                {
                    evenItemTexture1x1 = new Texture2D(1, 1);
                    evenItemTexture1x1.SetPixels(new Color[] { evenItemColor });
                    evenItemTexture1x1.Apply();
                }
                return evenItemTexture1x1;
            }
        }

        private static Texture2D evenItemHoveredTexture1x1;
        public static Texture2D EvenItemHoveredTexture1x1
        {
            get
            {
                if (evenItemHoveredTexture1x1 == null)
                {
                    evenItemHoveredTexture1x1 = new Texture2D(1, 1);
                    evenItemHoveredTexture1x1.SetPixels(new Color[] { evenItemColor * 1.2f });
                    evenItemHoveredTexture1x1.Apply();
                }
                return evenItemHoveredTexture1x1;
            }
        }

        private static Texture2D evenItemClickedTexture1x1;
        public static Texture2D EvenItemClickedTexture1x1
        {
            get
            {
                if (evenItemClickedTexture1x1 == null)
                {
                    evenItemClickedTexture1x1 = new Texture2D(1, 1);
                    evenItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    evenItemClickedTexture1x1.Apply();
                }
                return evenItemClickedTexture1x1;
            }
        }

        private static Texture2D selectedItemTexture1x1;
        public static Texture2D SelectedItemTexture1x1
        {
            get
            {
                if (selectedItemTexture1x1 == null)
                {
                    selectedItemTexture1x1 = new Texture2D(1, 1);
                    selectedItemTexture1x1.SetPixels(new Color[] { selectedItemColor });
                    selectedItemTexture1x1.Apply();
                }
                return selectedItemTexture1x1;
            }
        }

        private static Texture2D selectedItemHoveredTexture1x1;
        public static Texture2D SelectedItemHoveredTexture1x1
        {
            get
            {
                if (selectedItemHoveredTexture1x1 == null)
                {
                    selectedItemHoveredTexture1x1 = new Texture2D(1, 1);
                    selectedItemHoveredTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.2f });
                    selectedItemHoveredTexture1x1.Apply();
                }
                return selectedItemHoveredTexture1x1;
            }
        }

        private static Texture2D selectedItemClickedTexture1x1;
        public static Texture2D SelectedItemClickedTexture1x1
        {
            get
            {
                if (selectedItemClickedTexture1x1 == null)
                {
                    selectedItemClickedTexture1x1 = new Texture2D(1, 1);
                    selectedItemClickedTexture1x1.SetPixels(new Color[] { selectedItemColor * 1.4f });
                    selectedItemClickedTexture1x1.Apply();
                }
                return selectedItemClickedTexture1x1;
            }
        }

        private static GUIStyle oddFlatButton;
        public static GUIStyle OddFlatButton
        {
            get
            {
                if (oddFlatButton == null)
                {
                    oddFlatButton = new GUIStyle();
                    oddFlatButton.alignment = TextAnchor.MiddleCenter;
                    oddFlatButton.normal.background = OddItemTexture1x1;
                    oddFlatButton.hover.background = OddItemHoveredTexture1x1;
                    oddFlatButton.active.background = OddItemClickedTexture1x1;
                }
                return oddFlatButton;
            }
        }

        private static GUIStyle evenFlatButton;
        public static GUIStyle EvenFlatButton
        {
            get
            {
                if (evenFlatButton == null)
                {
                    evenFlatButton = new GUIStyle();
                    evenFlatButton.alignment = TextAnchor.MiddleCenter;
                    evenFlatButton.normal.background = EvenItemTexture1x1;
                    evenFlatButton.hover.background = EvenItemHoveredTexture1x1;
                    evenFlatButton.active.background = EvenItemClickedTexture1x1;
                }
                return evenFlatButton;
            }
        }

        private static GUIStyle selectedFlatButton;
        public static GUIStyle SelectedFlatButton
        {
            get
            {
                if (selectedFlatButton == null)
                {
                    selectedFlatButton = new GUIStyle();
                    selectedFlatButton.alignment = TextAnchor.MiddleCenter;
                    selectedFlatButton.normal.background = SelectedItemTexture1x1;
                    selectedFlatButton.hover.background = SelectedItemHoveredTexture1x1;
                    selectedFlatButton.active.background = SelectedItemClickedTexture1x1;
                }
                return selectedFlatButton;
            }
        }

        private static Texture2D criticalItemTexture1x1;
        public static Texture2D CriticalItemTexture1x1
        {
            get
            {
                if (criticalItemTexture1x1 == null)
                {
                    criticalItemTexture1x1 = new Texture2D(1, 1);
                    criticalItemTexture1x1.SetPixels(new Color[] { criticalItemColor });
                    criticalItemTexture1x1.Apply();
                }
                return criticalItemTexture1x1;
            }
        }

        private static Texture2D criticalItemHoveredTexture1x1;
        public static Texture2D CriticalItemHoveredTexture1x1
        {
            get
            {
                if (criticalItemHoveredTexture1x1 == null)
                {
                    criticalItemHoveredTexture1x1 = new Texture2D(1, 1);
                    criticalItemHoveredTexture1x1.SetPixels(new Color[] { criticalItemColor * 1.2f });
                    criticalItemHoveredTexture1x1.Apply();
                }
                return criticalItemHoveredTexture1x1;
            }
        }

        private static Texture2D criticalItemClickedTexture1x1;
        public static Texture2D CriticalItemClickedTexture1x1
        {
            get
            {
                if (criticalItemClickedTexture1x1 == null)
                {
                    criticalItemClickedTexture1x1 = new Texture2D(1, 1);
                    criticalItemClickedTexture1x1.SetPixels(new Color[] { criticalItemColor * 1.4f });
                    criticalItemClickedTexture1x1.Apply();
                }
                return criticalItemClickedTexture1x1;
            }
        }

        private static GUIStyle criticalFlatButton;
        public static GUIStyle CriticalFlatButton
        {
            get
            {
                if (criticalFlatButton == null)
                {
                    criticalFlatButton = new GUIStyle();
                    criticalFlatButton.alignment = TextAnchor.MiddleCenter;
                    criticalFlatButton.normal.background = CriticalItemTexture1x1;
                    criticalFlatButton.hover.background = CriticalItemHoveredTexture1x1;
                    criticalFlatButton.active.background = CriticalItemClickedTexture1x1;
                    criticalFlatButton.normal.textColor = Color.white;
                    criticalFlatButton.hover.textColor = Color.white;
                    criticalFlatButton.active.textColor = Color.white;
                    criticalFlatButton.fontStyle = FontStyle.Bold;
                }
                return criticalFlatButton;
            }
        }

        private static GUIStyle richTextLabel;
        public static GUIStyle RichTextLabel
        {
            get
            {
                if (richTextLabel == null)
                {
                    richTextLabel = new GUIStyle(EditorStyles.label);
                }
                richTextLabel.richText = true;
                richTextLabel.alignment = TextAnchor.MiddleLeft;
                richTextLabel.wordWrap = true;
                return richTextLabel;
            }
        }

        private static GUIStyle warningIconStyle;
        public static GUIStyle WarningIconStyle
        {
            get
            {
                if (warningIconStyle == null)
                {
                    warningIconStyle = new GUIStyle(EditorStyles.boldLabel);
                }
                warningIconStyle.normal.textColor = new Color(1, 0.7f, 0.3f, 1);
                return warningIconStyle;
            }
        }

        private static string projectName;
        public static string ProjectName
        {
            get
            {
                if (string.IsNullOrEmpty(projectName))
                {
                    string[] s = Application.dataPath.Split('/', '\\');
                    projectName = s[s.Length - 2];
                }
                return projectName;
            }

        }

        public static string GetUserDisplayName()
        {
            try
            {
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                object uc = assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
                // Cache type of UnityConnect.
                Type t = uc.GetType();
                // Get user info object from UnityConnect.
                var userInfo = t.GetProperty("userInfo").GetValue(uc, null);
                // Retrieve user id from user info.
                Type userInfoType = userInfo.GetType();
                string displayName = userInfoType.GetProperty("displayName").GetValue(userInfo, null) as string;
                return displayName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetProjectRelatedEditorPrefsKey(params string[] keyElements)
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder(ProjectName);
            for (int i = 0; i < keyElements.Length; ++i)
            {
                b.Append("-").Append(keyElements[i]);
            }
            return b.ToString();
        }

        /// <summary>
        /// Draw a tab view and return the currently selected tab
        /// </summary>
        /// <param name="current"></param>
        /// <param name="tabLabels"></param>
        /// <returns></returns>
        public static int Tabs(int current, string[] tabLabels)
        {
            if (tabLabels.Length == 0)
                return current;
            int padding = 40;
            int tabHeight = 20;
            RectOffset offset = new RectOffset(2, 2, 0, 2);

            int selectedTab = current;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.GetControlRect(GUILayout.Width(padding));
            for (int i = 0; i < tabLabels.Length; ++i)
            {
                Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(tabHeight), GUILayout.MaxWidth(150));

                if (selectedTab == i)
                {
                    DrawBodyBox(offset.Add(r));
                    GUI.Label(r, tabLabels[i], CenteredBoldLabel);
                }
                else
                {
                    DrawHeaderBox(offset.Add(r));
                    GUI.Label(r, tabLabels[i], CenteredLabel);
                }


                if (GUI.Button(offset.Add(r), string.Empty, GUIStyle.none))
                {
                    selectedTab = i;
                }
            }

            EditorGUILayout.GetControlRect(GUILayout.Width(padding));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            SpacePixel(0);
            return selectedTab;
        }

        /// <summary>
        /// Add space between controls
        /// </summary>
        /// <param name="amount"></param>
        public static void Space(int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
            {
                EditorGUILayout.Space();
            }
        }

        public static void SpacePixel(int pixel)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(pixel));
        }

        /// <summary>
        /// Draw a horizontal line
        /// </summary>
        public static void Separator(bool useIndent = false)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(3));
            if (useIndent)
            {
                r = EditorGUI.IndentedRect(r);
            }
            Vector2 start = new Vector2(r.min.x, (r.min.y + r.max.y) / 2);
            Vector2 end = new Vector2(r.max.x, (r.min.y + r.max.y) / 2);
            Handles.BeginGUI();
            Handles.color = boxBorderColor;
            Handles.DrawLine(start, end);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a button which anchors to the right edge of the window
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool RightAnchoredButton(string label)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool btnClicked = GUILayout.Button(label, GUILayout.Width(standardWidth), GUILayout.Height(standardHeight));
            EditorGUILayout.EndHorizontal();
            return btnClicked;
        }

        public static bool RightAnchoredButton(string label, float width, float height)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool btnClicked = GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
            return btnClicked;
        }

        public static bool Button(string label)
        {
            bool btnClicked = GUILayout.Button(label, GUILayout.Height(standardHeight));
            return btnClicked;
        }

        public static bool Button(string label, GUIStyle style)
        {
            bool btnClicked = GUILayout.Button(label, style, GUILayout.Height(standardHeight));
            return btnClicked;
        }

        /// <summary>
        /// Draw a tiny square button
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool TinyButton(string label)
        {
            return GUILayout.Button(label, GUILayout.Width(tinyWidth), GUILayout.Height(tinyHeight));
        }

        /// <summary>
        /// Draw a folder browser
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="result">The currently selected folder, this will be modified after user selects another folder</param>
        public static void BrowseFolder(string label, ref string result)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(label, result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("Browse", GUILayout.Width(standardWidth), GUILayout.Height(standardHeight)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                    result = FileUtil.GetProjectRelativePath(selectedFolder);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void BrowseFolder(GUIContent label, ref string result)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(label);
            //EditorGUILayout.LabelField(label, result, ItalicLabel, GUILayout.MinWidth(50));
            EditorGUILayout.LabelField(label, new GUIContent(result), ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("Browse", GUILayout.Width(standardWidth), GUILayout.Height(standardHeight)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                    result = FileUtil.GetProjectRelativePath(selectedFolder);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void BrowseFolderMiniButton(string label, ref string result)
        {
            int lastIndex = EditorGUI.indentLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField(result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("●", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                    result = FileUtil.GetProjectRelativePath(selectedFolder);
            }
            EditorGUI.indentLevel = lastIndex;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a file browser
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="result">The currently selected file path, this will be modified after selecting another file</param>
        /// <param name="filter">File extension filter</param>
        public static void BrowseFile(string label, ref string result, params string[] filter)
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel(label);
            EditorGUILayout.LabelField(label, result, ItalicLabel, GUILayout.MinWidth(50));
            if (GUILayout.Button("Browse", GUILayout.Width(standardWidth), GUILayout.Height(standardHeight)))
            {
                string selectedFile = EditorUtility.OpenFilePanelWithFilters("Select file", Application.dataPath, filter);
                if (!string.IsNullOrEmpty(selectedFile))
                    result = selectedFile;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a toggle group in horizontal layout, only one item can be selected at a time
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="selected">Currently selected item index</param>
        /// <param name="toggleWidth">Width of each toggle</param>
        /// <param name="toggleLabels">Label for each toggle</param>
        /// <returns>Index of the selected item</returns>
        public static int ToggleGroup(string label, int selected, float toggleWidth, params string[] toggleLabels)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect r = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(r.x, r.y, toggleWidth, r.height);

            for (int i = 0; i < toggleLabels.Length; ++i)
            {
                toggleRect.position = r.position + new Vector2(i * toggleWidth, 0);
                if (EditorGUI.ToggleLeft(toggleRect, toggleLabels[i], i == selected))
                {
                    selected = i;
                }
            }

            EditorGUILayout.EndHorizontal();
            return selected;
        }

        /// <summary>
        /// Draw a toggle group in vertical layout, only one item can be selected at a time
        /// </summary>
        /// <param name="label">The prefix label</param>
        /// <param name="selected">Currently selected item index</param>
        /// <param name="toggleHeight">Height of each toggle</param>
        /// <param name="toggleLabels">Label for each toggle</param>
        /// <returns>Index of the selected item</returns>
        public static int ToggleGroupVertical(string label, int selected, float toggleHeight, params string[] toggleLabels)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            Rect r = EditorGUILayout.GetControlRect(false, toggleHeight * toggleLabels.Length);
            Rect toggleRect = new Rect(r.x, r.y, r.width, toggleHeight);

            for (int i = 0; i < toggleLabels.Length; ++i)
            {
                toggleRect.position = r.position + new Vector2(0, i * toggleHeight);
                if (EditorGUI.ToggleLeft(toggleRect, toggleLabels[i], i == selected))
                {
                    selected = i;
                }
            }

            EditorGUILayout.EndHorizontal();
            return selected;
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static bool LinkButton(string label, float width, float height)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));
            return LinkButton(r, label);
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool LinkButton(string label)
        {
            return LinkButton(label, standardWidth, standardHeight);
        }

        /// <summary>
        /// Draw a button looks like a hyperlink
        /// </summary>
        /// <param name="r"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool LinkButton(Rect r, string label)
        {
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            return GUI.Button(r, label, LinkLabel);
        }

        public static void DrawOutlineBox(Rect r, Color c)
        {
            Handles.BeginGUI();
            using (var scope = new Handles.DrawingScope(c))
            {
                Vector2 p1 = new Vector2(r.xMin, r.yMin);
                Vector2 p2 = new Vector2(r.xMax, r.yMin);
                Vector2 p3 = new Vector2(r.xMax, r.yMax);
                Vector2 p4 = new Vector2(r.xMin, r.yMax);
                Handles.DrawLines(new Vector3[]
                {
                    p1,p2,
                    p2,p3,
                    p3,p4,
                    p4,p1
                });
            }
            Handles.EndGUI();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color c)
        {
            Handles.BeginGUI();
            using (var scope = new Handles.DrawingScope(c))
            {
                Handles.DrawLine(start, end);
            }
            Handles.EndGUI();
        }

        public static void DrawBottomLine(Rect r, Color c)
        {
            DrawLine(
                new Vector2(r.min.x, r.max.y),
                new Vector2(r.max.x, r.max.y),
                c);
        }

        public static void DrawPlus(Rect r, Color c, float thickness)
        {
            Rect r0 = new Rect();
            r0.size = new Vector2(thickness, r.height);
            r0.center = r.center;
            EditorGUI.DrawRect(r0, c);

            Rect r1 = new Rect();
            r1.size = new Vector2(r.width, thickness);
            r1.center = r.center;
            EditorGUI.DrawRect(r1, c);
        }

        public static void SetTerrainDataDirty(GTerrainData data)
        {
            EditorUtility.SetDirty(data);
        }

        public static int ActiveTerrainGroupPopup(string label, int selected)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>(GStylizedTerrain.ActiveTerrains);
            HashSet<int> groupId = new HashSet<int>();
            for (int i = 0; i < terrains.Count; ++i)
            {
                groupId.Add(terrains[i].GroupId);
            }

            List<int> values = new List<int>(groupId);
            values.Sort();
            string[] valueLabels = new string[values.Count];
            for (int i = 0; i < valueLabels.Length; ++i)
            {
                valueLabels[i] = string.Format("Group {0}", values[i].ToString());
            }

            return EditorGUILayout.IntPopup(label, selected, valueLabels, values.ToArray());
        }

        public static int ActiveTerrainGroupPopupWithAllOption(string label, int selected)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>(GStylizedTerrain.ActiveTerrains);
            HashSet<int> groupId = new HashSet<int>();
            for (int i = 0; i < terrains.Count; ++i)
            {
                groupId.Add(terrains[i].GroupId);
            }

            List<int> values = new List<int>(groupId);
            values.Sort();
            values.Insert(0, -1);
            string[] valueLabels = new string[values.Count];
            valueLabels[0] = "All";
            for (int i = 1; i < valueLabels.Length; ++i)
            {
                valueLabels[i] = string.Format("Group {0}", values[i].ToString());
            }

            return EditorGUILayout.IntPopup(label, selected, valueLabels, values.ToArray());
        }

        public static int ActiveTerrainGroupPopupWithAllOption(GUIContent label, int selected)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>(GStylizedTerrain.ActiveTerrains);
            HashSet<int> groupId = new HashSet<int>();
            for (int i = 0; i < terrains.Count; ++i)
            {
                groupId.Add(terrains[i].GroupId);
            }

            List<int> values = new List<int>(groupId);
            values.Sort();
            values.Insert(0, -1);
            GUIContent[] valueLabels = new GUIContent[values.Count];
            valueLabels[0] = new GUIContent("All");
            for (int i = 1; i < valueLabels.Length; ++i)
            {
                valueLabels[i] = new GUIContent(string.Format("Group {0}", values[i].ToString()));
            }

            return EditorGUILayout.IntPopup(label, selected, valueLabels, values.ToArray());
        }

        public static void SingleLineToggleIntField(string label, ref bool toggle, ref int value)
        {
            EditorGUILayout.BeginHorizontal();
            toggle = EditorGUILayout.Toggle(toggle);
            value = EditorGUILayout.DelayedIntField(label, value);
            EditorGUILayout.EndHorizontal();
        }

        public static int _SelectionGrid(_GSelectionGridArgs args)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUI.indentLevel * GEditorCommon.indentSpace));
            int selectedIndex = args.selectedIndex;
            if (args.categorizeFunction != null)
            {
                selectedIndex = _SelectionGridCategorized(args);
            }
            else
            {
                selectedIndex = _SelectionGridUncategorized(args);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            return selectedIndex;
        }

        private static int _SelectionGridCategorized(_GSelectionGridArgs args)
        {
            int lastIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            int selectedIndex = args.selectedIndex;
            List<object> items = new List<object>();
            IEnumerator iterator = args.collection.GetEnumerator();
            while (iterator.MoveNext())
            {
                items.Add(iterator.Current);
            }
            if (items.Count <= 0)
                return -1;
            EditorGUILayout.BeginVertical();
            List<object> itemsInCategory = new List<object>();
            itemsInCategory.Add(items[0]);
            object currentCategory = args.categorizeFunction(items[0]);
            object lastCategory = currentCategory;
            int categoryStartIndex = 0;
            for (int i = 1; i <= items.Count; ++i)
            {
                currentCategory = i == items.Count ? null : args.categorizeFunction(items[i]);
                if (currentCategory == null || !currentCategory.Equals(lastCategory))
                {

                    string header = lastCategory != null ? lastCategory.ToString() : string.Empty;
                    EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                    _GSelectionGridArgs tmpArgs = new _GSelectionGridArgs();
                    tmpArgs.selectedIndex = selectedIndex;
                    tmpArgs.collection = itemsInCategory;
                    tmpArgs.tileSize = args.tileSize;
                    tmpArgs.drawPreviewFunction = args.drawPreviewFunction;
                    tmpArgs.drawLabelFunction = args.drawLabelFunction;
                    tmpArgs.tooltipFunction = args.tooltipFunction;
                    tmpArgs.customDrawFunction = args.customDrawFunction;
                    selectedIndex = _SelectionGridInternal(tmpArgs, categoryStartIndex);

                    EditorGUILayout.Space();

                    if (i < items.Count)
                    {
                        itemsInCategory.Clear();
                        itemsInCategory.Add(items[i]);
                        categoryStartIndex = i;
                    }
                }
                else if (currentCategory.Equals(lastCategory))
                {
                    itemsInCategory.Add(items[i]);
                }
                lastCategory = currentCategory;
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = lastIndent;
            return selectedIndex;
        }

        private static int _SelectionGridUncategorized(_GSelectionGridArgs args)
        {
            return _SelectionGridInternal(args, 0);
        }

        private static int _SelectionGridInternal(_GSelectionGridArgs args, int startIndex = 0)
        {
            EditorGUILayout.BeginVertical();
            float width = EditorGUIUtility.currentViewWidth - 2 * (EditorGUI.indentLevel) * EditorGUIUtility.singleLineHeight;
            Vector2 itemSize = args.tileSize;
            if (args.drawLabelFunction != null)
                itemSize += new Vector2(0, EditorGUIUtility.singleLineHeight);
            int itemPerRow = Mathf.RoundToInt(width / (itemSize.x + EditorGUIUtility.standardVerticalSpacing)) - 1;
            itemPerRow = Mathf.Clamp(itemPerRow, 1, args.collection.Count);
            int numberOfRow = Mathf.CeilToInt(args.collection.Count * 1.0f / itemPerRow);
            IEnumerator iterator = args.collection.GetEnumerator();
            int selectedIndex = args.selectedIndex;
            int currentItemIndex = startIndex;
            for (int i = 0; i < itemPerRow * numberOfRow; ++i)
            {
                if (i % itemPerRow == 0)
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                if (i < args.collection.Count)
                {
                    iterator.MoveNext();
                    Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(itemSize.x), GUILayout.Height(itemSize.y));

                    string tooltip = args.tooltipFunction != null ? args.tooltipFunction(iterator.Current) : string.Empty;
                    GUIContent content = new GUIContent(string.Empty, tooltip);
                    if (GUI.Button(buttonRect, content, GUIStyle.none))
                    {
                        selectedIndex = currentItemIndex;
                    }

                    if (args.selectedIndex == currentItemIndex)
                    {
                        Rect highlightRect = new RectOffset(2, 2, 2, 2).Add(buttonRect);
                        EditorGUI.DrawRect(highlightRect, GEditorCommon.selectedItemColor);
                    }
                    if (args.drawPreviewFunction != null)
                    {
                        Rect previewRect = new Rect(buttonRect.position, itemSize);
                        args.drawPreviewFunction(previewRect, iterator.Current);
                    }
                    if (args.drawLabelFunction != null)
                    {
                        Rect labelRect = new RectOffset(0, 0, (int)args.tileSize.y, 0).Remove(buttonRect);
                        args.drawLabelFunction(labelRect, iterator.Current);
                    }
                    if (args.customDrawFunction != null)
                    {
                        Rect customDrawRect = new Rect(buttonRect.position, itemSize);
                        args.customDrawFunction(customDrawRect, iterator.Current);
                    }

                    currentItemIndex += 1;
                }

                if ((i + 1) % itemPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            return selectedIndex;
        }

        public static T ObjectSelectorDragDrop<T>(Rect r, string message, string filter = "", bool allowSceneObject = false) where T : class
        {
            r = EditorGUI.IndentedRect(r);
            int controlId = EditorGUIUtility.GetControlID(FocusType.Passive, r);
            T objectToReturn = default(T);

            DrawBodyBox(r);

            Rect messageRect = new Rect();
            messageRect.size = new Vector2(r.width, 12);
            messageRect.center = r.center - Vector2.up * (messageRect.size.y * 0.5f + 2);
            EditorGUI.LabelField(messageRect, message, GEditorCommon.CenteredLabel);

            Rect buttonRect = new Rect();
            buttonRect.size = new Vector2(47, 15);
            buttonRect.center = r.center + Vector2.up * (buttonRect.size.y * 0.5f + 2);
            if (GUI.Button(buttonRect, "Browse", GEditorCommon.CenteredLabel))
            {
                EditorGUIUtility.ShowObjectPicker<Object>(null, allowSceneObject, filter, controlId);
            }
            GEditorCommon.DrawLine(
                new Vector2(buttonRect.min.x, buttonRect.max.y),
                new Vector2(buttonRect.max.x, buttonRect.max.y),
                CenteredLabel.normal.textColor);
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);

            Event e = Event.current;
            if (e != null &&
                r.Contains(e.mousePosition) &&
                (e.type == EventType.DragUpdated ||
                e.type == EventType.Repaint))
            {
                Object[] draggedObject = DragAndDrop.objectReferences;
                if (draggedObject.Length == 1 &&
                    draggedObject[0] is T)
                {
                    DragAndDrop.AcceptDrag();
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = controlId;
                    EditorGUI.DrawRect(r, GUtilities.GetColor(GEditorCommon.selectedItemColor, 0.5f));
                    GEditorCommon.DrawOutlineBox(r, GEditorCommon.selectedItemColor);
                }
            }
            else if (e != null && e.type == EventType.DragPerform && r.Contains(e.mousePosition))
            {
                Object[] draggedObject = DragAndDrop.objectReferences;
                if (draggedObject.Length == 1 &&
                    draggedObject[0] is T)
                {
                    objectToReturn = draggedObject[0] as T;
                }
            }
            else if (e != null && e.type == EventType.ExecuteCommand && e.commandName.Equals("ObjectSelectorClosed"))
            {
                int id = EditorGUIUtility.GetObjectPickerControlID();
                Object o = EditorGUIUtility.GetObjectPickerObject();
                if (id == controlId && o != null)
                {
                    objectToReturn = o as T;
                }
            }

            return objectToReturn;
        }

        //public static int SplatSetSelectionGrid(int groupId, int selectedIndex)
        //{
        //    List<GSplatPrototypeGroup> splatSet = GetSplatsSetOnActiveTerrainGroup(groupId);
        //    GSplatPrototypeGroup set = null;
        //    if (splatSet.Count == 0 || (splatSet.Count == 1 && splatSet[0] == null))
        //    {
        //        EditorGUILayout.LabelField("No Splat Set found. Assign a Splat Prototype Group to the terrain or terrain group.", WordWrapItalicLabel);
        //    }
        //    else if (splatSet.Count > 1)
        //    {
        //        EditorGUILayout.LabelField("Splat Set config in the current terrain group is not consistent.\nTaking the largest set for preview!", WordWrapItalicLabel);
        //        splatSet.RemoveAll(s => s == null);
        //        splatSet.Sort((s0, s1) => { return -s0.Prototypes.Count.CompareTo(s1.Prototypes.Count); });
        //        set = splatSet[0];
        //    }
        //    else
        //    {
        //        set = splatSet[0];
        //    }

        //    if (set == GGriffinSettings.Instance.TerrainDataDefault.Shading.Splats)
        //    {
        //        EditorGUILayout.LabelField("This is a demo Splat set, consider creating your own one!", WordWrapItalicLabel);
        //    }

        //    GUI.enabled = false;
        //    EditorGUILayout.ObjectField("Splats", set, typeof(GSplatPrototypeGroup), false);
        //    GUI.enabled = true;

        //    if (set != null && set.Prototypes.Count > 0)
        //    {
        //        GSelectionGridArgs args = new GSelectionGridArgs();
        //        args.collection = set.Prototypes;
        //        args.selectedIndex = selectedIndex;
        //        args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
        //        args.itemPerRow = 4;
        //        args.drawPreviewFunction = DrawSplatPreview;
        //        selectedIndex = GEditorCommon.SelectionGrid(args);
        //    }
        //    else
        //    {
        //        EditorGUILayout.LabelField("No Splat found in this set!", WordWrapItalicLabel);
        //    }

        //    if (set != null && set.Prototypes.Count > 4)
        //    {
        //        EditorGUILayout.LabelField("Splat Set with more than 4 prototypes need appropriate material to be rendered correctly, ex: Griffin/Terrain/Lambert8Splats", WordWrapItalicLabel);
        //    }

        //    return selectedIndex;
        //}

        public static int SplatSetSelectionGrid(int groupId, int selectedIndex)
        {
            List<GSplatPreviewTextures> previews = GetSplatPreviewTextures(groupId);

            if (previews != null && previews.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = previews;
                args.selectedIndex = selectedIndex;
                args.itemSize = GEditorCommon.selectionGridTileSizeLarge;
                args.itemPerRow = 4;
                args.drawPreviewFunction = DrawSplatPreview2;
                selectedIndex = GEditorCommon.SelectionGrid(args);
            }
            else
            {
                EditorGUILayout.LabelField("No Splat found in this set!", WordWrapItalicLabel);
            }

            return selectedIndex;
        }

        public static List<int> SplatSetMultiSelectionGrid(int groupId, List<int> selectedIndices)
        {
            List<GSplatPreviewTextures> previews = GetSplatPreviewTextures(groupId);

            if (previews != null && previews.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = previews;
                args.selectedIndices = selectedIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeLarge;
                args.itemPerRow = 4;
                args.drawPreviewFunction = DrawSplatPreview2;
                selectedIndices = GEditorCommon.MultiSelectionGrid(args);
            }
            else
            {
                EditorGUILayout.LabelField("No Splat found in this set!", WordWrapItalicLabel);
            }

            return selectedIndices;
        }

        private static void DrawSplatPreview2(Rect r, object o)
        {
            GSplatPreviewTextures preview = (GSplatPreviewTextures)o;
            if (preview != null)
            {
                List<Texture> texToDraw = new List<Texture>();
                for (int i = 0; i < preview.TextureArray.Count; ++i)
                {
                    texToDraw.Add(preview.TextureArray[i]);
                }
                for (int i = 0; i < preview.Textures.Count; ++i)
                {
                    texToDraw.Add(preview.Textures[i]);
                }
                texToDraw.RemoveAll((t) => t == null);
                texToDraw = new List<Texture>(texToDraw.Distinct(new GObjectComparer<Texture>()));

                if (texToDraw.Count == 0)
                {
                    EditorGUI.DrawRect(r, Color.black);
                }
                else if (texToDraw.Count == 1)
                {
                    Texture t = texToDraw[0];
                    DrawSplatPreviewTile(r, t, preview.Index);
                }
                else if (texToDraw.Count > 1)
                {
                    Rect tileRect;
                    Texture t;
                    float w = r.width * 0.5f - 1;
                    float h = r.height * 0.5f - 1;

                    tileRect = new Rect(r.min.x, r.min.y, w, h);
                    t = texToDraw[0];
                    DrawSplatPreviewTile(tileRect, t, preview.Index);

                    tileRect = new Rect(r.center.x + 1, r.min.y, w, h);
                    t = texToDraw[1];
                    DrawSplatPreviewTile(tileRect, t, preview.Index);

                    if (texToDraw.Count > 2)
                    {
                        tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                        t = texToDraw[2];
                        DrawSplatPreviewTile(tileRect, t, preview.Index);
                    }
                    else
                    {
                        tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                        EditorGUI.DrawTextureTransparent(tileRect, Texture2D.blackTexture);
                    }

                    if (texToDraw.Count > 3)
                    {
                        tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                        t = texToDraw[3];
                        DrawSplatPreviewTile(tileRect, t, preview.Index);
                    }
                    else
                    {
                        tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                        EditorGUI.DrawTextureTransparent(tileRect, Texture2D.blackTexture);
                    }
                }
            }
        }

        private static void DrawSplatPreviewTile(Rect r, Texture t, int sliceIndex)
        {
            if (t is Texture2D)
            {
                EditorGUI.DrawPreviewTexture(r, t);
            }
            else if (t is Texture2DArray)
            {
                Material mat = GInternalMaterials.DrawTex2DArraySliceMaterial;
                mat.SetInt("_SliceIndex", sliceIndex);
                EditorGUI.DrawPreviewTexture(r, t, mat);

                Texture2D texArrayIcon = GEditorSkin.Instance.GetTexture("TextureArrayIcon");
                if (texArrayIcon != null)
                {
                    GUIContent icon = new GUIContent(texArrayIcon, "This layer is packed in a Texture Array.");
                    Rect iconRect = new Rect(r.max.x - 22, r.max.y - 22, 20, 20);
                    GUI.Box(iconRect, icon);
                }
            }
        }

        public static void DrawSplatPreview(Rect r, object o)
        {
            GSplatPrototype p = (GSplatPrototype)o;
            if (p != null)
            {
                if (p.Texture != null)
                {
                    EditorGUI.DrawPreviewTexture(r, p.Texture);
                }
                else
                {
                    EditorGUI.DrawRect(r, Color.black);
                }
            }
        }

        private static List<GSplatPrototypeGroup> GetSplatsSetOnActiveTerrainGroup(int groupId)
        {
            HashSet<GSplatPrototypeGroup> set = new HashSet<GSplatPrototypeGroup>();
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (groupId < 0 ||
                    (groupId >= 0 && t.GroupId == groupId))
                {
                    if (t.TerrainData != null)
                        set.Add(t.TerrainData.Shading.Splats);
                }
            }
            return new List<GSplatPrototypeGroup>(set);
        }

        private static List<GSplatPreviewTextures> GetSplatPreviewTextures(int groupId)
        {
            int maxLayer = 0;
            GCommon.ForEachTerrain(-1, (t) =>
            {
                if (t.TerrainData == null)
                {
                    return;
                }

                bool shaderUseTextureArray = false;
                Material mat = t.TerrainData.Shading.CustomMaterial;
                if (mat != null && mat.HasProperty("_Diffuse"))
                {
                    Texture2DArray texArray = mat.GetTexture("_Diffuse") as Texture2DArray;
                    if (texArray != null)
                    {
                        maxLayer = Mathf.Max(maxLayer, texArray.depth);
                        shaderUseTextureArray = true;
                    }
                }

                if (!shaderUseTextureArray)
                {
                    if (t.TerrainData.Shading.Splats != null)
                    {
                        maxLayer = Mathf.Max(maxLayer, t.TerrainData.Shading.Splats.Prototypes.Count);
                    }
                }
            });

            List<GSplatPreviewTextures> previews = new List<GSplatPreviewTextures>();
            for (int i = 0; i < maxLayer; ++i)
            {
                GSplatPreviewTextures p = new GSplatPreviewTextures(i);
                previews.Add(p);
                GCommon.ForEachTerrain(-1, (t) =>
                {
                    if (t.TerrainData == null)
                    {
                        return;
                    }

                    bool shaderUseTextureArray = false;
                    Material mat = t.TerrainData.Shading.CustomMaterial;
                    if (mat != null && mat.HasProperty("_Diffuse"))
                    {
                        Texture2DArray texArray = mat.GetTexture("_Diffuse") as Texture2DArray;
                        if (texArray != null)
                        {
                            p.TextureArray.Add(texArray);
                            shaderUseTextureArray = true;
                        }
                    }

                    if (!shaderUseTextureArray)
                    {
                        GSplatPrototypeGroup splats = t.TerrainData.Shading.Splats;
                        if (splats != null &&
                            splats.Prototypes.Count > i &&
                            splats.Prototypes[i] != null)
                        {
                            p.Textures.Add(t.TerrainData.Shading.Splats.Prototypes[i].Texture);
                        }
                    }
                });
                p.TextureArray.Distinct(new GObjectComparer<Texture>());
                p.Textures.Distinct(new GObjectComparer<Texture>());
            }

            return previews;
        }

        public static List<int> TreeSetMultiSelectionGrid(int groupId, List<int> selectedIndices)
        {
            List<GSelectionGridPreviews> previews = GetTreePreviews(groupId);

            if (previews.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = previews;
                args.selectedIndices = selectedIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeLarge;
                args.itemPerRow = 4;
                args.drawPreviewFunction = DrawTreePreviewTile;
                selectedIndices = GEditorCommon.MultiSelectionGrid(args);
                return selectedIndices;
            }
            else
            {
                EditorGUILayout.LabelField("No Tree Prototype found in this terrain group.", WordWrapItalicLabel);
                return new List<int>();
            }
        }

        private static List<GSelectionGridPreviews> GetTreePreviews(int groupId)
        {
            int maxPrototypeCount = 0;
            GCommon.ForEachTerrain(groupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                if (t.TerrainData.Foliage.Trees == null)
                    return;
                maxPrototypeCount = Mathf.Max(maxPrototypeCount, t.TerrainData.Foliage.Trees.Prototypes.Count);
            });

            List<GSelectionGridPreviews> previews = new List<GSelectionGridPreviews>();
            for (int i = 0; i < maxPrototypeCount; ++i)
            {
                previews.Add(new GSelectionGridPreviews());
            }

            GCommon.ForEachTerrain(groupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                if (t.TerrainData.Foliage.Trees == null)
                    return;
                List<GTreePrototype> prototypes = t.TerrainData.Foliage.Trees.Prototypes;
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    previews[i].Add(prototypes[i]);
                }
            });

            return previews;
        }

        private static void DrawTreePreviewTile(Rect r, object o)
        {
            GSelectionGridPreviews preview = (GSelectionGridPreviews)o;
            if (preview == null)
                return;
            List<Texture> texToDraw = new List<Texture>(preview.Textures);

            texToDraw.RemoveAll((t) => t == null);
            texToDraw = new List<Texture>(texToDraw.Distinct(new GObjectComparer<Texture>()));

            if (texToDraw.Count == 0)
            {
                EditorGUI.DrawRect(r, assetPreviewGrey);
                EditorGUI.LabelField(r, "...", GEditorCommon.CenteredWhiteLabel);
            }
            else if (texToDraw.Count == 1)
            {
                Texture t = texToDraw[0];
                EditorGUI.DrawPreviewTexture(r, t, null, ScaleMode.ScaleToFit);
            }
            else if (texToDraw.Count > 1)
            {
                Rect tileRect;
                Texture t;
                float w = r.width * 0.5f - 1;
                float h = r.height * 0.5f - 1;

                tileRect = new Rect(r.min.x, r.min.y, w, h);
                t = texToDraw[0];
                EditorGUI.DrawPreviewTexture(tileRect, t, null, ScaleMode.ScaleToFit);

                tileRect = new Rect(r.center.x + 1, r.min.y, w, h);
                t = texToDraw[1];
                EditorGUI.DrawPreviewTexture(tileRect, t, null, ScaleMode.ScaleToFit);

                if (texToDraw.Count > 2)
                {
                    tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                    t = texToDraw[2];
                    EditorGUI.DrawPreviewTexture(tileRect, t, null, ScaleMode.ScaleToFit);
                }
                else
                {
                    tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                }

                if (texToDraw.Count > 3)
                {
                    tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                    t = texToDraw[3];
                    EditorGUI.DrawPreviewTexture(tileRect, t, null, ScaleMode.ScaleToFit);
                }
                else
                {
                    tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                }
            }
        }

        public static void DrawTreePreviewSingle(Rect r, object o)
        {
            GTreePrototype t = (GTreePrototype)o;
            if (t != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(t.Prefab);
                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(r, preview, null, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(r, assetPreviewGrey);
                    EditorGUI.LabelField(r, "...", GEditorCommon.CenteredWhiteLabel);
                }
            }
        }

        public static void DrawGameObjectPreview(Rect r, object o)
        {
            GameObject g = (GameObject)o;
            if (g != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(g);
                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(r, preview, null, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(r, assetPreviewGrey);
                    EditorGUI.LabelField(r, "...", GEditorCommon.CenteredWhiteLabel);
                }
            }
        }

        public static List<int> GrassSetMultiSelectionGrid(int groupId, List<int> selectedIndices)
        {
            List<GSelectionGridPreviews> previews = GetGrassPreviews(groupId);
            if (previews.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = previews;
                args.selectedIndices = selectedIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeLarge;
                args.itemPerRow = 4;
                args.drawPreviewFunction = DrawGrassPreviewTile;
                selectedIndices = GEditorCommon.MultiSelectionGrid(args);
                return selectedIndices;
            }
            else
            {
                EditorGUILayout.LabelField("No Grass Prototype found in this terrain group.", WordWrapItalicLabel);
                return new List<int>();
            }
        }

        private static List<GSelectionGridPreviews> GetGrassPreviews(int groupId)
        {
            int maxPrototypeCount = 0;
            GCommon.ForEachTerrain(groupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                if (t.TerrainData.Foliage.Grasses == null)
                    return;
                maxPrototypeCount = Mathf.Max(maxPrototypeCount, t.TerrainData.Foliage.Grasses.Prototypes.Count);
            });

            List<GSelectionGridPreviews> previews = new List<GSelectionGridPreviews>();
            for (int i = 0; i < maxPrototypeCount; ++i)
            {
                previews.Add(new GSelectionGridPreviews());
            }

            GCommon.ForEachTerrain(groupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                if (t.TerrainData.Foliage.Grasses == null)
                    return;
                List<GGrassPrototype> prototypes = t.TerrainData.Foliage.Grasses.Prototypes;
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    previews[i].Add(prototypes[i]);
                }
            });

            return previews;
        }

        public static void DrawGrassPreviewTile(Rect r, object o)
        {
            GSelectionGridPreviews preview = (GSelectionGridPreviews)o;
            if (preview == null)
                return;
            List<Color> colors = preview.Colors;
            List<Texture> textures = preview.Textures;
            MergeGrassPreview(colors, textures);

            if (textures.Count == 0)
            {
                EditorGUI.DrawRect(r, assetPreviewGrey);
                EditorGUI.LabelField(r, "...", GEditorCommon.CenteredWhiteLabel);
            }
            else if (textures.Count == 1)
            {
                Texture t = textures[0];
                EditorGUI.DrawRect(r, assetPreviewGrey);
                GUI.DrawTexture(r, t, ScaleMode.ScaleToFit, true, 1, colors[0], 0, 0);
            }
            else if (textures.Count > 1)
            {
                Rect tileRect;
                Texture t;
                float w = r.width * 0.5f - 1;
                float h = r.height * 0.5f - 1;

                tileRect = new Rect(r.min.x, r.min.y, w, h);
                t = textures[0];
                EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                GUI.DrawTexture(tileRect, t, ScaleMode.ScaleToFit, true, 1, colors[0], 0, 0);

                tileRect = new Rect(r.center.x + 1, r.min.y, w, h);
                t = textures[1];
                EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                GUI.DrawTexture(tileRect, t, ScaleMode.ScaleToFit, true, 1, colors[1], 0, 0);

                if (textures.Count > 2)
                {
                    tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                    t = textures[2];
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                    GUI.DrawTexture(tileRect, t, ScaleMode.ScaleToFit, true, 1, colors[2], 0, 0);
                }
                else
                {
                    tileRect = new Rect(r.min.x, r.center.y + 1, w, h);
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                }

                if (textures.Count > 3)
                {
                    tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                    t = textures[3];
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                    GUI.DrawTexture(tileRect, t, ScaleMode.ScaleToFit, true, 1, colors[3], 0, 0);
                }
                else
                {
                    tileRect = new Rect(r.center.x + 1, r.center.y + 1, w, h);
                    EditorGUI.DrawRect(tileRect, assetPreviewGrey);
                }
            }
        }

        private static void MergeGrassPreview(List<Color> colors, List<Texture> textures)
        {
            int count = textures.Count;
            List<GSelectionGridPreviews.ColorTexturePair> tmpList = new List<GSelectionGridPreviews.ColorTexturePair>();
            for (int i = 0; i < count; ++i)
            {
                tmpList.Add(new GSelectionGridPreviews.ColorTexturePair(colors[i], textures[i]));
            }
            tmpList = new List<GSelectionGridPreviews.ColorTexturePair>(tmpList.Distinct());

            colors.Clear();
            textures.Clear();
            for (int i = 0; i < tmpList.Count; ++i)
            {
                colors.Add(tmpList[i].color);
                textures.Add(tmpList[i].texture);
            }
        }

        public static void DrawGrassPreview(Rect r, object o)
        {
            GGrassPrototype t = (GGrassPrototype)o;
            if (t != null)
            {
                if (t.Shape == GGrassShape.DetailObject)
                {
                    DrawPreview(r, t.Prefab, t.Color);
                }
                else
                {
                    DrawPreview(r, t.Texture, t.Color);
                }
            }
        }

        private static List<GGrassPrototypeGroup> GetGrassSetOnActiveTerrainGroup(int groupId)
        {
            HashSet<GGrassPrototypeGroup> set = new HashSet<GGrassPrototypeGroup>();
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (groupId < 0 ||
                    (groupId >= 0 && t.GroupId == groupId))
                {
                    if (t.TerrainData != null)
                        set.Add(t.TerrainData.Foliage.Grasses);
                }
            }
            return new List<GGrassPrototypeGroup>(set);
        }

        public static Vector2 InlineVector2Field(string label, Vector2 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector2Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector2 InlineVector2Field(GUIContent label, Vector2 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector2Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector3 InlineVector3Field(string label, Vector3 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector3Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector3 InlineVector3Field(GUIContent label, Vector3 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector3Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Vector4 InlineVector4Field(string label, Vector4 value)
        {
            EditorGUIUtility.wideMode = true;
            value = EditorGUILayout.Vector4Field(label, value);
            EditorGUIUtility.wideMode = false;
            return value;
        }

        public static Quaternion InlineEulerRotationField(string label, Quaternion rotation)
        {
            return InlineEulerRotationField(new GUIContent(label), rotation);
        }

        public static Quaternion InlineEulerRotationField(GUIContent label, Quaternion rotation)
        {
            EditorGUIUtility.wideMode = true;
            rotation = Quaternion.Euler(EditorGUILayout.Vector3Field(label, rotation.eulerAngles));
            EditorGUIUtility.wideMode = false;
            return rotation;
        }

        public static Texture2D InlineTexture2DField(string label, Texture2D value, int indentScope = 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(indentScope))
            {
                value = EditorGUILayout.ObjectField(value, typeof(Texture2D), false) as Texture2D;
            }
            EditorGUILayout.EndHorizontal();
            return value;
        }

        private class GBackupHelpboxGUI
        {
            public static readonly string LABEL = "Tip";
            public static readonly string ID = "backup-help-box";

            public static readonly GUIContent TEXT = new GUIContent("Backup System is a great way to manage your Editing History!");
            public static readonly GUIContent OPEN_EDITOR_BTN = new GUIContent("Open Backup Editor");
        }

        public static void DrawBackupHelpBox()
        {
            if (GBackupEditor.Instance == null)
            {
                Foldout(GBackupHelpboxGUI.LABEL, true, GBackupHelpboxGUI.ID, () =>
                {
                    EditorGUILayout.LabelField(
                       GBackupHelpboxGUI.TEXT,
                       WordWrapItalicLabel);
                    if (GUILayout.Button(GBackupHelpboxGUI.OPEN_EDITOR_BTN))
                    {
                        GBackupEditor.ShowWindow();
                    }
                });
            }
        }

        public static void OpenEmailEditor(string receiver, string subject, string body)
        {
            string url = string.Format(
                "mailto:{0}" +
                "?subject={1}" +
                "&body={2}",
                receiver,
                subject.Replace(" ", "%20"),
                body.Replace(" ", "%20"));

            Application.OpenURL(url);
        }

        public static string GetClassDisplayName(Type t)
        {
            string label = string.Empty;
            object[] alternativeClassNameAttributes = t.GetCustomAttributes(typeof(GDisplayName), false);
            if (alternativeClassNameAttributes != null && alternativeClassNameAttributes.Length > 0)
            {
                GDisplayName att = alternativeClassNameAttributes[0] as GDisplayName;
                if (att.DisplayName == null ||
                    att.DisplayName.Equals(string.Empty))
                    label = t.Name;
                else
                    label = att.DisplayName;
            }
            else
            {
                label = t.Name;
            }
            return label;
        }

        public static bool Foldout(string label, string prefkeys, bool defaultValue = true)
        {
            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", prefkeys);
            bool expanded = EditorPrefs.GetBool(prefKey, defaultValue);
            expanded = EditorGUILayout.Foldout(expanded, label);
            EditorPrefs.SetBool(prefKey, expanded);

            return expanded;
        }

        public static string Ellipsis(string text, int length)
        {
            if (text.Length <= length)
                return text;
            string substring = text.Substring(0, length);
            return substring + "...";
        }

        public static Rect BeginGroup()
        {
            Rect r = EditorGUILayout.BeginVertical();
            GUI.Box(GEditorCommon.boxOffset.Add(r), "");
            return r;
        }

        public static void EndGroup()
        {
            EditorGUILayout.EndVertical();
        }

        public static void ExpandFoldout(string id)
        {
            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            SessionState.SetBool(prefKey, true);
        }

        public static bool GetFoldoutState(string id)
        {
            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            return SessionState.GetBool(prefKey, false);
        }

        public static void SetFoldoutState(string id, bool value)
        {
            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            SessionState.SetBool(prefKey, value);
        }

        public static void DrawHeaderBox(Rect r)
        {
            EditorGUI.DrawRect(r, boxHeaderBg);
            DrawOutlineBox(r, boxBorderColor);
            //GUI.Box(r, "");
            //if (EditorGUIUtility.isProSkin)
            //    GUI.Box(r, "");
        }

        public static void DrawBodyBox(Rect r, bool shadow = true)
        {
            EditorGUI.DrawRect(r, boxBodyBg);
            DrawOutlineBox(r, boxBorderColor);

            if (shadow)
            {
                Vector2 start = new Vector2(r.min.x + 1, r.max.y + 1);
                Vector2 end = new Vector2(r.max.x - 1, r.max.y + 1);
                Color32 color = boxBorderColor;
                color.a = EditorGUIUtility.isProSkin ? (byte)100 : (byte)135;
                DrawLine(start, end, color);
            }

            //GUI.Box(r, "");
        }

        public static bool Foldout(string label, bool defaultExpanded, string id, Action innerGUI, GenericMenu context = null, string headerWarning = null)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));

            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            bool expanded = SessionState.GetBool(prefKey, defaultExpanded);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(indentSpace * EditorGUI.indentLevel));
            EditorGUILayout.BeginVertical();

            Rect headerRect = EditorGUILayout.BeginHorizontal();
            RectOffset headerBoxOffset = new RectOffset(2, 2, 1, 1);
            DrawHeaderBox(headerBoxOffset.Add(headerRect));

            Rect caretRect = EditorGUILayout.GetControlRect(GUILayout.Width(indentSpace));
            GUI.Label(caretRect, expanded ? caretDown : caretRight, EditorStyles.miniLabel);
            Rect labelRect = EditorGUILayout.GetControlRect();
            GUI.Label(labelRect, label, EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(headerWarning) || context != null)
            {
                GUILayout.FlexibleSpace();
            }

            if (!string.IsNullOrEmpty(headerWarning))
            {
                Rect warningRect = EditorGUILayout.GetControlRect(GUILayout.Width(20));
                EditorGUI.LabelField(warningRect, new GUIContent("[!]", headerWarning), WarningIconStyle);
            }

            if (context != null && context.GetItemCount() > 0)
            {
                Rect contextButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
                if (GUI.Button(contextButtonRect, contextIconText, EditorStyles.label))
                {
                    context.ShowAsContext();
                }
            }

            if (GUI.Button(headerRect, "", GUIStyle.none))
            {
                expanded = !expanded;
            }
            EditorGUILayout.EndHorizontal();

            SessionState.SetBool(prefKey, expanded);

            if (expanded)
            {
                Rect bodyRect = EditorGUILayout.BeginVertical();
                RectOffset bodyBoxOffset = new RectOffset(2, 2, 2, 2);
                DrawBodyBox(bodyBoxOffset.Add(bodyRect));
            }

            if (expanded)
            {
                EditorGUI.indentLevel += 1;
                if (innerGUI != null)
                    innerGUI.Invoke();
                EditorGUI.indentLevel -= 1;
                SpacePixel(1);
            }

            if (expanded)
            {
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUI.indentLevel - 1));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            return expanded;
        }

        public static void DrawPreview(Rect r, Object o)
        {
            DrawPreview(r, o, Color.white);
        }

        public static void DrawPreview(Rect r, Object o, Color tint)
        {
            EditorGUI.DrawRect(r, GEditorCommon.assetPreviewGrey);
            Texture2D preview = AssetPreview.GetAssetPreview(o);
            if (preview != null)
            {
                GUI.DrawTexture(r, preview, ScaleMode.ScaleToFit, true, 1, tint, 0, 0);
            }
            else
            {
                if (o != null && AssetPreview.IsLoadingAssetPreview(o.GetInstanceID()))
                {
                    EditorGUI.LabelField(r, "Loading preview...", GEditorCommon.CenteredWhiteLabel);
                }
                else
                {
                    EditorGUI.LabelField(r, "Preview not available!", GEditorCommon.CenteredWhiteLabel);
                }
            }
        }

        public static void Box(string content)
        {
            Rect r = EditorGUILayout.BeginVertical();
            RectOffset offset = new RectOffset((int)indentSpace * EditorGUI.indentLevel, 0, 0, 0);
            GUI.Box(offset.Remove(r), "");
            EditorGUILayout.LabelField(content, WordWrapItalicLabel);
            EditorGUILayout.EndVertical();
        }

        public static List<int> MultiSelectionGrid(GSelectionGridArgs args)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUI.indentLevel * GEditorCommon.indentSpace));
            List<int> selected = MultiSelectionGridInternal(args);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            return selected;
        }

        private static List<int> MultiSelectionGridInternal(GSelectionGridArgs args)
        {
            IEnumerator iter = args.collection.GetEnumerator();
            List<int> selected = args.selectedIndices;
            int maxWeight = 0;
            Dictionary<int, int> weights = new Dictionary<int, int>();
            for (int i = 0; i < selected.Count; ++i)
            {
                int index = selected[i];
                if (!weights.ContainsKey(index))
                {
                    weights.Add(index, 0);
                }
                weights[index] += 1;
                maxWeight = Mathf.Max(maxWeight, weights[index]);
            }

            int currentIndex = 0;
            int loopGuard = 100;
            int loop = 0;
            EditorGUILayout.BeginVertical();
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            while (currentIndex < args.collection.Count)
            {
                loop += 1;
                if (loop > loopGuard)
                {
                    Debug.Log("Loop error");
                    break;
                }

                EditorGUILayout.BeginHorizontal();
                int itemPerRow = Mathf.Max(1, args.itemPerRow);
                for (int i = 0; i < itemPerRow; ++i)
                {
                    if (iter.MoveNext())
                    {
                        Rect itemRect = EditorGUILayout.GetControlRect(GUILayout.Width(args.itemSize.x), GUILayout.Height(args.itemSize.y));
                        if (Event.current.type == EventType.MouseUp && itemRect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 0)
                            {
                                if (Event.current.shift && !args.simpleMode)
                                {
                                    selected.Add(currentIndex);
                                }
                                else if (Event.current.control && !args.simpleMode)
                                {
                                    selected.Remove(currentIndex);
                                }
                                else if (selected.Contains(currentIndex))
                                {
                                    selected.RemoveAll(index => index == currentIndex);
                                }
                                else
                                {
                                    selected.Add(currentIndex);
                                }
                                GUI.changed = true;
                                Event.current.Use();
                            }
                            else if (Event.current.button == 1)
                            {
                                if (args.contextClickFunction != null)
                                {
                                    args.contextClickFunction.Invoke(itemRect, currentIndex, args.collection);
                                }
                                GUI.changed = true;
                                Event.current.Use();
                            }
                        }

                        Rect previewRect = itemRect;
                        if (selected.Contains(currentIndex) && weights.ContainsKey(currentIndex))
                        {
                            Color c = GEditorCommon.selectedItemColor;
                            float w = weights[currentIndex];
                            float f = Mathf.InverseLerp(0, maxWeight, w);
                            c.a = Mathf.Lerp(0.5f, 1f, f);
                            EditorGUI.DrawRect(itemRect, c);
                            previewRect = new RectOffset(2, 2, 2, 2).Remove(previewRect);
                        }

                        if (args.drawPreviewFunction != null)
                        {
                            args.drawPreviewFunction(previewRect, iter.Current);
                        }

                        if (weights.ContainsKey(currentIndex) && !args.simpleMode)
                        {
                            Rect weightLabelRect = new Rect(itemRect.x, itemRect.max.y - 18, itemRect.width, 18);
                            weightLabelRect = new RectOffset(2, 2, 2, 2).Remove(weightLabelRect);
                            EditorGUI.LabelField(weightLabelRect, weights[currentIndex].ToString(), RightAlignedWhiteTinyLabel);
                        }
                        currentIndex += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
                SpacePixel(0);
            }
            if (!args.simpleMode)
            {
                EditorGUILayout.LabelField("Use Shift/Ctrl & Left Click to set probabilities.", GEditorCommon.WordWrapItalicLabel);
            }

            EditorGUI.indentLevel = indent;
            EditorGUILayout.EndVertical();
            return selected;
        }

        public static int SelectionGrid(GSelectionGridArgs args)
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUI.indentLevel * GEditorCommon.indentSpace));
            int selected = SelectionGridInternal(args);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            return selected;
        }

        private static int SelectionGridInternal(GSelectionGridArgs args)
        {
            IEnumerator iter = args.collection.GetEnumerator();
            int selected = args.selectedIndex;
            int currentIndex = 0;
            int loopGuard = 100;
            int loop = 0;
            EditorGUILayout.BeginVertical();
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            while (currentIndex < args.collection.Count)
            {
                loop += 1;
                if (loop > loopGuard)
                {
                    Debug.Log("Loop error");
                    break;
                }

                EditorGUILayout.BeginHorizontal();
                int itemPerRow = Mathf.Max(1, args.itemPerRow);
                for (int i = 0; i < itemPerRow; ++i)
                {
                    if (iter.MoveNext())
                    {
                        Rect itemRect = EditorGUILayout.GetControlRect(GUILayout.Width(args.itemSize.x), GUILayout.Height(args.itemSize.y));
                        if (Event.current.type == EventType.MouseUp && itemRect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 0)
                            {
                                if (selected != currentIndex)
                                {
                                    GUI.changed = true;
                                }
                                selected = currentIndex;
                                Event.current.Use();
                            }
                            else if (Event.current.button == 1)
                            {
                                if (args.contextClickFunction != null)
                                {
                                    args.contextClickFunction.Invoke(itemRect, currentIndex, args.collection);
                                }
                                Event.current.Use();
                            }
                        }

                        Rect previewRect = itemRect;
                        if (selected == currentIndex)
                        {
                            EditorGUI.DrawRect(itemRect, GEditorCommon.selectedItemColor);
                            previewRect = new RectOffset(2, 2, 2, 2).Remove(previewRect);
                        }

                        if (args.drawPreviewFunction != null)
                        {
                            args.drawPreviewFunction(previewRect, iter.Current);
                        }

                        currentIndex += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
                SpacePixel(0);
            }

            EditorGUI.indentLevel = indent;
            EditorGUILayout.EndVertical();
            return selected;
        }

        public static void Header(string text)
        {
            SpacePixel(2);
            EditorGUILayout.LabelField(text, BoldLabel);
        }

        public static void SceneViewBox(GUIContent content)
        {
            Handles.BeginGUI();
            int offsetX = 5;
            int offsetY = 45;
            Vector2 size = SceneViewBoxContent.CalcSize(content);
            Rect r = new Rect(Screen.width - size.x - offsetX, Screen.height - size.y - offsetY, size.x, size.y);
            EditorGUI.DrawRect(r, boxBodyBg);
            GUI.Label(r, content, SceneViewBoxContent);
            Handles.EndGUI();
        }

        public static void SceneViewMouseMessage(GUIContent content)
        {
            Handles.BeginGUI();
            float offsetX = Event.current.mousePosition.x + 15;
            float offsetY = Event.current.mousePosition.y + 20;
            Vector2 size = SceneViewBoxContent.CalcSize(content);
            Rect r = new Rect(offsetX, offsetY, size.x, size.y);
            EditorGUI.DrawRect(r, boxBodyBg);
            GUI.Label(r, content, SceneViewBoxContent);
            Handles.EndGUI();
        }

#if GRIFFIN_VEGETATION_STUDIO_PRO
        private class GVSPIntegrationGUI
        {
            public static readonly string LABEL = "VSP Integration";
            public static readonly string ID = "vsp-integration";

            public static readonly GUIContent REFRESH_GEOMETRY = new GUIContent("Refresh Geometry");
            public static readonly GUIContent REFRESH_VEGETATION = new GUIContent("Refresh Vegetation");
        }

        public static void DrawVspIntegrationGUI()
        {
            Foldout(GVSPIntegrationGUI.LABEL, true, GVSPIntegrationGUI.ID, () =>
            {
                if (GUILayout.Button(GVSPIntegrationGUI.REFRESH_GEOMETRY))
                {
                    GCommon.ForEachTerrain(-1,
                        (t) =>
                        {
                            GVSPPolarisTerrain polarisTerrain = t.GetComponent<GVSPPolarisTerrain>();
                            if (polarisTerrain != null)
                            {
                                polarisTerrain.GenerateMeshTerrain();
                            }
                        });
                }

                if (GUILayout.Button(GVSPIntegrationGUI.REFRESH_VEGETATION))
                {
                    VegetationStudioManager.RefreshVegetationSystem();
                }
            });
        }
#endif

        public static LayerMask LayerMaskField(string label, LayerMask mask)
        {
            return LayerMaskField(new GUIContent(label), mask);
        }

        public static LayerMask LayerMaskField(GUIContent label, LayerMask mask)
        {
            LayerMask tempMask = EditorGUILayout.MaskField(label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(mask), InternalEditorUtility.layers);
            mask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            return mask;
        }

        private class AffLinkGUI
        {
            private static GUIStyle linkButtonStyle;
            public static GUIStyle LinkButtonStyle
            {
                get
                {
                    if (linkButtonStyle == null)
                    {
                        linkButtonStyle = new GUIStyle(EditorStyles.miniLabel);
                    }
                    linkButtonStyle.wordWrap = false;
                    linkButtonStyle.alignment = TextAnchor.MiddleLeft;
                    linkButtonStyle.normal.textColor = linkColor;
                    return linkButtonStyle;
                }
            }
        }

        public static void DrawAffLinks(string text, params string[] urls)
        {
            if (GEditorSettings.Instance.general.enableAffiliateLinks)
            {
                Rect r = EditorGUILayout.GetControlRect();
                r = EditorGUI.IndentedRect(r);
                EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                GUIContent content = new GUIContent(
                    text + "→",
                    "This button contains affiliate links and will bring you to the Unity Asset Store.\n" +
                    "You won't have to pay an extra but I can earn a little more which can help me a lot.\n" +
                    "Thanks!");

                if (GUI.Button(r, content, AffLinkGUI.LinkButtonStyle))
                {
                    foreach (string s in urls)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            Application.OpenURL(s + "?aid=1100l3QbW&pubref=polaris-editor");
                        }
                    }
                }
            }
        }
    }
}
#endif
#endif
