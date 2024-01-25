#if GRIFFIN
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.BillboardTool
{
    public class GBillboardEditor : GTwoPaneWindowWindow
    {
        private int previewPadding = 10;
        private bool alphaBlend = true;

        private GBillboardRenderMode mode;
        private GameObject target;
        private int row = 4;
        private int column = 4;
        private int cellSize = 256;
        private Vector3 cameraOffset = new Vector3(0, 0, 5);
        private float cameraSize = 1;
        private Material atlasMaterial;
        private Material normalMaterial;
        private string srcTextureProps = "_MainTex";
        private string desTextureProps = "_MainTex";
        private string srcColorProps = "_Color";
        private string desColorProps = "_Color";
        private int cellIndex = 0;
        private List<Vector2> vertices = new List<Vector2>(new Vector2[] { new Vector2(0.01f, 0.01f), new Vector2(0.01f, 0.99f), new Vector2(0.99f, 0.99f), new Vector2(0.99f, 0.01f) });
        private int selectedVertexIndex = -1;
        private float vertexClickDistance = 10;
        private ushort[] tris = new ushort[0];
        private float bottom = 0;
        private float width = 1;
        private float height = 2;
        private string saveFolder;

        private const int MAX_VERTICES = 10;

        private int[] rowColumnValues = new int[] { 1, 2, 4, 8, 16 };
        private string[] rowColumnLabels = new string[] { "1", "2", "4", "8", "16" };

        private int[] cellSizeValues = new int[] { 128, 256, 512 };
        private string[] cellSizeLabels = new string[] { "128", "256", "512" };

        private RenderTexture previewRt;

        private const string ATLAS_INSTRUCTION =
            "Preview billboard atlas. Image order from bottom-left to top-right.";

        private const string NORMAL_INSTRUCTION =
            "Preview billboard normal map. Image order from bottom-left to top-right";

        private const string FLIPBOOK_INSTRUCTION =
                "Preview billboard transition and edit billboard mesh.\n" +
                "   - Use Scrollwheel to iterate images.\n" +
                "   - Use Left Mouse to edit vertex position.\n" +
                "   - Use Ctrl & Left Mouse to remove vertex.\n" +
                "   - Use Shift & Left Mouse to add vertex.\n" +
                "For best rendering performance, billboard mesh is limited at maximum 10 vertices.";

        private const string KEY_BILLBOARDEDITOR = "billboardeditor";
        private const string KEY_MODE = "mode";
        private const string KEY_PREFAB = "prefab";
        private const string KEY_ROW = "row";
        private const string KEY_COLUMN = "column";
        private const string KEY_CELLSIZE = "cellsize";
        private const string KEY_CAM_X = "camx";
        private const string KEY_CAM_Y = "camy";
        private const string KEY_CAM_Z = "camz";
        private const string KEY_CAM_SIZE = "camsize";
        private const string KEY_ATLAS_MAT = "atlasmat";
        private const string KEY_NORMAL_MAT = "normalmat";
        private const string KEY_SRC_TEX_PROPS = "srctex";
        private const string KEY_DES_TEX_PROPS = "destex";
        private const string KEY_SRC_COLOR_PROPS = "srccolor";
        private const string KEY_DES_COLOR_PROPS = "descolor";
        private const string KEY_BOTTOM = "bottom";
        private const string KEY_WIDTH = "width";
        private const string KEY_HEIGHT = "height";
        private const string KEY_ALPHA_BLEND = "alphablend";
        private const string KEY_SAVE_FOLDER = "save-folder";

        private List<string> srcColorSuggestion;
        private List<string> SrcColorSuggestion
        {
            get
            {
                if (srcColorSuggestion == null)
                {
                    srcColorSuggestion = new List<string>();
                }
                return srcColorSuggestion;
            }
        }

        private List<string> srcTextureSuggestion;
        private List<string> SrcTextureSuggestion
        {
            get
            {
                if (srcTextureSuggestion == null)
                {
                    srcTextureSuggestion = new List<string>();
                }
                return srcTextureSuggestion;
            }
        }

        public static void ShowWindow()
        {
            GBillboardEditor window = EditorWindow.GetWindow<GBillboardEditor>();
            window.titleContent = new GUIContent("Billboard");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPrefs();
            wantsMouseMove = true;
            tris = GBillboardCreator.Triangulate(vertices.ToArray());
            RenderPreview();
            RefreshMaterialPropsSuggestions();
        }

        private void OnDisable()
        {
            SavePrefs();
        }

        private void SavePrefs()
        {
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_MODE), (int)mode);
            if (target != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(target);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_PREFAB), assetPath);
                }
            }
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_ROW), row);
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_COLUMN), column);
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CELLSIZE), cellSize);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_X), cameraOffset.x);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_Y), cameraOffset.y);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_Z), cameraOffset.z);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_SIZE), cameraSize);
            if (atlasMaterial != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(atlasMaterial);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_ATLAS_MAT), assetPath);
                }
            }
            if (normalMaterial != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(normalMaterial);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_NORMAL_MAT), assetPath);
                }
            }
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SRC_COLOR_PROPS), srcColorProps);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_DES_COLOR_PROPS), desColorProps);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SRC_TEX_PROPS), srcTextureProps);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_DES_TEX_PROPS), desTextureProps);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_WIDTH), width);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_HEIGHT), height);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_BOTTOM), bottom);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_ALPHA_BLEND), alphaBlend);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SAVE_FOLDER), saveFolder);
        }

        private void LoadPrefs()
        {
            mode = (GBillboardRenderMode)EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_MODE), 0);
            string targetAssetPath = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_PREFAB), string.Empty);
            target = AssetDatabase.LoadAssetAtPath<GameObject>(targetAssetPath);

            row = EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_ROW), 4);
            column = EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_COLUMN), 4);
            cellSize = EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CELLSIZE), 256);
            float camX = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_X), 0);
            float camY = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_Y), -2);
            float camZ = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_Z), 0);
            cameraOffset = new Vector3(camX, camY, camZ);
            cameraSize = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_CAM_SIZE), 2);

            atlasMaterial = GEditorSettings.Instance.billboardTools.atlasMaterial;
            normalMaterial = GEditorSettings.Instance.billboardTools.normalMaterial;

            srcColorProps = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SRC_COLOR_PROPS), "_Color");
            desColorProps = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_DES_COLOR_PROPS), "_Color");
            srcTextureProps = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SRC_TEX_PROPS), "_MainTex");
            desTextureProps = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_DES_TEX_PROPS), "_MainTex");

            width = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_WIDTH), 1);
            height = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_HEIGHT), 2);
            bottom = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_BOTTOM), 0.2f);
            alphaBlend = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_ALPHA_BLEND), true);
            saveFolder = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(KEY_BILLBOARDEDITOR, KEY_SAVE_FOLDER), "Assets/");
        }

        protected override void OnToolbarGUI(Rect r)
        {
            base.OnToolbarGUI(r);

            List<string> buttonLabels = new List<string>()
            {
                "Atlas",
                "Normal",
                "Flipbook",
                "",
                "A"
            };
            List<Rect> buttonRects = EditorGUIUtility.GetFlowLayoutedRects(r, EditorStyles.toolbarButton, 0, 0, buttonLabels);

            for (int i = 0; i < 3; ++i)
            {
                if (GUI.Button(buttonRects[i], buttonLabels[i], EditorStyles.toolbarButton))
                {
                    mode = (GBillboardRenderMode)i;
                    RenderPreview();
                }
                if (i == (int)mode)
                {
                    Color highlightColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.1f) : new Color(1, 1, 1, 0.3f);
                    EditorGUI.DrawRect(buttonRects[i], highlightColor);
                }
            }

            if (GUI.Button(buttonRects[4], buttonLabels[4], EditorStyles.toolbarButton))
            {
                alphaBlend = !alphaBlend;
            }
            if (alphaBlend)
            {

                Color highlightColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.1f) : new Color(1, 1, 1, 0.3f);
                EditorGUI.DrawRect(buttonRects[4], highlightColor);
            }

            Rect saveButtonRect = new Rect(
                r.max.x - 100 - 2,
                r.min.y,
                100,
                r.height);
            if (GUI.Button(saveButtonRect, "Save", EditorStyles.toolbarButton))
            {
                SaveAssets();
            }
        }

        private GBillboardCreatorArgs ConstructArgs()
        {
            GBillboardCreatorArgs args = new GBillboardCreatorArgs();
            args.Mode = mode;
            args.Target = target;
            args.Row = row;
            args.Column = column;
            args.CellSize = cellSize;
            args.CameraOffset = cameraOffset;
            args.CameraSize = cameraSize;
            args.AtlasMaterial = atlasMaterial;
            args.NormalMaterial = normalMaterial;
            args.SrcColorProps = srcColorProps;
            //args.DesColorProps = desColorProps;
            args.DesColorProps = "_Color";
            args.SrcTextureProps = srcTextureProps;
            //args.DesTextureProps = desTextureProps;
            args.DesTextureProps = "_MainTex";
            args.CellIndex = cellIndex % (args.Row * args.Column);
            args.Vertices = vertices.ToArray();
            args.Width = width;
            args.Height = height;
            args.Bottom = bottom;

            return args;
        }

        private void CopyArgs(GBillboardCreatorArgs args)
        {
            mode = args.Mode;
            target = args.Target;
            row = args.Row;
            column = args.Column;
            cellSize = args.CellSize;
            cameraOffset = args.CameraOffset;
            cameraSize = args.CameraSize;
            atlasMaterial = args.AtlasMaterial;
            normalMaterial = args.NormalMaterial;
            srcColorProps = args.SrcColorProps;
            desColorProps = args.DesColorProps;
            srcTextureProps = args.SrcTextureProps;
            desTextureProps = args.DesTextureProps;
            cellIndex = args.CellIndex;
            vertices = new List<Vector2>(args.Vertices);
            width = args.Width;
            height = args.Height;
            bottom = args.Bottom;
            tris = GBillboardCreator.Triangulate(vertices.ToArray());
        }

        protected override void OnLeftPaneGUI(Rect r)
        {
            base.OnLeftPaneGUI(r);

            Rect previewRect = new Rect();
            int size = Mathf.FloorToInt(Mathf.Min(LeftPaneRect.width, LeftPaneRect.height) - 2 * previewPadding);
            previewRect.size = new Vector2(size, size);
            previewRect.center = LeftPaneRect.center;

            if (previewRt != null)
            {
                if (alphaBlend)
                    EditorGUI.DrawTextureTransparent(previewRect, previewRt, ScaleMode.ScaleToFit);
                else
                    EditorGUI.DrawPreviewTexture(previewRect, previewRt, null, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawRect(previewRect, Color.black);
            }

            if (Event.current != null &&
                Event.current.isScrollWheel &&
                previewRect.Contains(Event.current.mousePosition) &&
                mode == GBillboardRenderMode.Flipbook)
            {
                cellIndex += (int)(Event.current.delta.y / 2f);
                if (cellIndex < 0)
                    cellIndex = row * column - 1;
                if (cellIndex >= row * column)
                    cellIndex = 0;
                RenderPreview();
            }

            if (mode == GBillboardRenderMode.Flipbook)
            {
                DrawBottomLine(previewRect);
                DrawTriangles(previewRect);
                DrawVertices(previewRect);
                Rect pageRect = new Rect(
                    previewRect.min.x, previewRect.max.y - 20,
                    previewRect.width, 20);
                EditorGUI.DropShadowLabel(pageRect, string.Format("{0}/{1}", cellIndex + 1, row * column), GEditorCommon.CenteredWhiteLabel);
            }
        }

        private void DrawTriangles(Rect previewRect)
        {
            if (tris == null)
                return;
            if (tris.Length % 3 != 0)
            {
                Debug.Log("Invalid tris array!");
                return;
            }

            int trisCount = tris.Length / 3;
            for (int i = 0; i < trisCount; ++i)
            {
                Vector2 v0 = vertices[tris[i * 3 + 0]];
                Vector2 v1 = vertices[tris[i * 3 + 1]];
                Vector2 v2 = vertices[tris[i * 3 + 2]];

                v0 = GUtilities.FlipY(v0);
                v1 = GUtilities.FlipY(v1);
                v2 = GUtilities.FlipY(v2);

                Vector2 p0 = Rect.NormalizedToPoint(previewRect, v0);
                Vector2 p1 = Rect.NormalizedToPoint(previewRect, v1);
                Vector2 p2 = Rect.NormalizedToPoint(previewRect, v2);

                GEditorCommon.DrawLine(p0, p1, Color.green);
                GEditorCommon.DrawLine(p1, p2, Color.green);
                GEditorCommon.DrawLine(p2, p0, Color.green);
            }
        }

        private void DrawVertices(Rect previewRect)
        {
            if (vertices == null)
            {
                vertices = new List<Vector2>();
                tris = new ushort[0];
            }
            for (int i = 0; i < vertices.Count; ++i)
            {
                HandleEditingVertices(previewRect, i);
            }
            HandleAddVertex(previewRect);
        }

        private void DrawBottomLine(Rect previewRect)
        {
            float h = bottom / (bottom - height);
            h = 1 - h;
            Vector2 p0 = Rect.NormalizedToPoint(previewRect, new Vector2(0, h));
            Vector2 p1 = Rect.NormalizedToPoint(previewRect, new Vector2(1, h));
            GEditorCommon.DrawLine(p0, p1, Color.red);
        }

        private void HandleEditingVertices(Rect previewRect, int vertIndex)
        {
            if (vertIndex >= vertices.Count)
                return;
            if (Event.current == null)
                return;
            Event e = Event.current;

            Vector2 uv = vertices[vertIndex];
            Vector2 point = Rect.NormalizedToPoint(previewRect, GUtilities.FlipY(uv));
            if (e.type == EventType.MouseDown)
            {
                selectedVertexIndex = -1;
                float d = Vector2.Distance(e.mousePosition, point);
                if (d <= vertexClickDistance)
                {
                    if (e.button == 0 && !e.control)
                    {
                        selectedVertexIndex = vertIndex;
                        e.Use();
                    }
                    else if (e.button == 0 && e.control)
                    {
                        vertices.RemoveAt(vertIndex);
                        tris = GBillboardCreator.Triangulate(vertices.ToArray());
                        e.Use();
                    }
                }
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (selectedVertexIndex == vertIndex)
                {
                    point = e.mousePosition;
                    uv = Rect.PointToNormalized(previewRect, point);
                    uv.Set(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
                    vertices[vertIndex] = GUtilities.FlipY(uv);
                    tris = GBillboardCreator.Triangulate(vertices.ToArray());
                    e.Use();
                }
            }

            Color handleColor = selectedVertexIndex == vertIndex ? Color.yellow : Color.green;
            float handleSize = 6;
            Rect handleRect = new Rect();
            handleRect.size = Vector2.one * handleSize;
            handleRect.center = point;
            EditorGUI.DrawRect(handleRect, handleColor);
        }

        private void HandleAddVertex(Rect previewRect)
        {
            if (vertices.Count >= MAX_VERTICES)
                return;
            if (Event.current == null)
                return;
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.shift)
            {
                if (!previewRect.Contains(e.mousePosition))
                    return;
                Vector2 uv = Rect.PointToNormalized(previewRect, e.mousePosition);
                vertices.Add(GUtilities.FlipY(uv));
                selectedVertexIndex = vertices.Count - 1;
                tris = GBillboardCreator.Triangulate(vertices.ToArray());
                e.Use();
            }
        }

        protected override void OnRightPaneScrollViewGUI()
        {
            base.OnRightPaneScrollViewGUI();

            EditorGUI.BeginChangeCheck();
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120;
            DrawInstruction();
            DrawTargetSettings();
            DrawCameraSettings();
            DrawAtlasSettings();
            DrawMaterialSettings();
            if (mode == GBillboardRenderMode.Flipbook)
            {
                DrawMeshSettings();
            }
            DrawUtilities();
            EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck())
            {
                RenderPreview();
            }
        }

        private void DrawInstruction()
        {
            string label = "Instruction";
            string id = "billboardeditor-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                string instruction =
                    mode == GBillboardRenderMode.Atlas ? ATLAS_INSTRUCTION :
                    mode == GBillboardRenderMode.Normal ? NORMAL_INSTRUCTION :
                    mode == GBillboardRenderMode.Flipbook ? FLIPBOOK_INSTRUCTION :
                    string.Empty;
                EditorGUILayout.LabelField(instruction, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawTargetSettings()
        {
            string label = "Target";
            string id = "billboardeditor-target";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.BeginChangeCheck();
                target = EditorGUILayout.ObjectField("Prefab", target, typeof(GameObject), true) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    GBillboardCreatorArgs args = GBillboardCreator.FitCameraToTarget(ConstructArgs());
                    CopyArgs(args);
                    RefreshMaterialPropsSuggestions();
                }
            });
        }

        private void DrawCameraSettings()
        {
            string label = "Camera";
            string id = "billboardeditor-camera";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUIUtility.wideMode = true;
                cameraOffset = EditorGUILayout.Vector3Field("Offset", cameraOffset);
                EditorGUIUtility.wideMode = false;
                cameraSize = EditorGUILayout.FloatField("Size", cameraSize);
                cameraSize = Mathf.Max(0, cameraSize);
            });
        }

        private void DrawAtlasSettings()
        {
            string label = "Atlas";
            string id = "billboardeditor-atlas";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                row = EditorGUILayout.IntPopup("Row", row, rowColumnLabels, rowColumnValues);
                column = EditorGUILayout.IntPopup("Column", column, rowColumnLabels, rowColumnValues);
                cellSize = EditorGUILayout.IntPopup("Cell Size", cellSize, cellSizeLabels, cellSizeValues);
                EditorGUILayout.LabelField("Resolution: ", string.Format("{0} x {1}", column * cellSize, row * cellSize));
            });
        }

        private void DrawMaterialSettings()
        {
            string label = "Materials";
            string id = "billboardeditor-materials";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GUI.enabled = false;
                atlasMaterial = EditorGUILayout.ObjectField("Atlas Material", atlasMaterial, typeof(Material), false) as Material;
                normalMaterial = EditorGUILayout.ObjectField("Normal Material", normalMaterial, typeof(Material), false) as Material;
                GUI.enabled = true;
                EditorGUILayout.BeginHorizontal();
                srcColorProps = EditorGUILayout.TextField("Source Color", srcColorProps);
                if (GUILayout.Button(GEditorCommon.contextIconText, EditorStyles.label, GUILayout.Width(14)))
                {
                    GenericMenu menu = new GenericMenu();
                    if (SrcColorSuggestion.Count > 0)
                    {
                        menu.AddDisabledItem(new GUIContent("Suggestion"));
                        menu.AddSeparator(null);
                        for (int i = 0; i < SrcColorSuggestion.Count; ++i)
                        {
                            string s = SrcColorSuggestion[i];
                            menu.AddItem(
                                new GUIContent(s),
                                false,
                                () => { srcColorProps = s; RenderPreview(); });
                        }
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("No suitable property found!"));
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                //desColorProps = EditorGUILayout.TextField("Des Color", desColorProps);

                EditorGUILayout.BeginHorizontal();
                srcTextureProps = EditorGUILayout.TextField("Source Texture", srcTextureProps);
                if (GUILayout.Button(GEditorCommon.contextIconText, EditorStyles.label, GUILayout.Width(14)))
                {
                    GenericMenu menu = new GenericMenu();
                    if (SrcTextureSuggestion.Count > 0)
                    {
                        menu.AddDisabledItem(new GUIContent("Suggestion"));
                        menu.AddSeparator(null);
                        for (int i = 0; i < SrcTextureSuggestion.Count; ++i)
                        {
                            string s = SrcTextureSuggestion[i];
                            menu.AddItem(
                                new GUIContent(s),
                                false,
                                () => { srcTextureProps = s; RenderPreview(); });
                        }
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("No suitable property found!"));
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                //desTextureProps = EditorGUILayout.TextField("Des Texture", desTextureProps);
            });
        }

        private void RenderPreview()
        {
            GBillboardCreatorArgs args = ConstructArgs();
            GBillboardCreator.PrepareRenderTexture(ref previewRt, args);
            GBillboardCreator.RenderPreview(previewRt, args);
        }

        private void DrawMeshSettings()
        {
            string label = "Mesh";
            string id = "billboardeditor-mesh";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                int trisCount = tris.Length / 3;
                EditorGUILayout.LabelField("Triangles", trisCount.ToString());
                EditorGUILayout.LabelField("Vertices", vertices.Count.ToString());
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < vertices.Count; ++i)
                {
                    Vector2 v = vertices[i];
                    EditorGUIUtility.wideMode = true;
                    v = EditorGUILayout.Vector2Field("Vertex " + i, v);
                    EditorGUIUtility.wideMode = false;
                    v.Set(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y));
                    vertices[i] = v;
                }
                EditorGUI.indentLevel -= 1;

                width = EditorGUILayout.FloatField("Width", width);
                width = Mathf.Max(0, width);
                height = EditorGUILayout.FloatField("Height", height);
                height = Mathf.Max(0, height);
                bottom = EditorGUILayout.FloatField("Bottom", bottom);
                bottom = Mathf.Min(bottom, 0);
            });
        }

        private void DrawUtilities()
        {
            string label = "Utilites";
            string id = "billboardeditor-utilities";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                Rect fitButtonRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(fitButtonRect, "Fit Camera"))
                {
                    GBillboardCreatorArgs args = GBillboardCreator.FitCameraToTarget(ConstructArgs());
                    CopyArgs(args);
                }
            });
        }

        private void SaveAssets()
        {
            if (string.IsNullOrEmpty(saveFolder))
            {
                saveFolder = "Assets/";
            }
            string folder = EditorUtility.OpenFolderPanel("Select Folder", saveFolder, "");
            if (string.IsNullOrEmpty(folder))
                return;
            try
            {
                saveFolder = folder;
                EditorUtility.DisplayProgressBar("Saving", "Saving assets...", 1f);
                GBillboardCreatorArgs args = ConstructArgs();
                BillboardAsset billboard = GBillboardCreator.CreateBillboardAsset(args);
                Texture2D atlas = GBillboardCreator.RenderAtlas(args);
                Texture2D normal = GBillboardCreator.RenderNormal(args);

                string billboardPath = Path.Combine(FileUtil.GetProjectRelativePath(saveFolder), billboard.name + ".asset");
                BillboardAsset billboardAsset = AssetDatabase.LoadAssetAtPath<BillboardAsset>(billboardPath);
                if (billboardAsset == null)
                {
                    AssetDatabase.CreateAsset(billboard, billboardPath);
                    billboardAsset = billboard;
                }
                else
                {
                    billboardAsset.SetVertices(billboard.GetVertices());
                    billboardAsset.SetIndices(billboard.GetIndices());
                    billboardAsset.SetImageTexCoords(billboard.GetImageTexCoords());
                    billboardAsset.width = billboard.width;
                    billboardAsset.height = billboard.height;
                    billboardAsset.bottom = billboard.bottom;
                    billboardAsset.name = billboard.name;
                }

                string atlasPath = Path.Combine(FileUtil.GetProjectRelativePath(saveFolder), atlas.name + ".png");
                byte[] atlasData = atlas.EncodeToPNG();
                File.WriteAllBytes(atlasPath, atlasData);
                GUtilities.DestroyObject(atlas);

                string normalPath = Path.Combine(FileUtil.GetProjectRelativePath(saveFolder), normal.name + ".png");
                byte[] normalData = normal.EncodeToPNG();
                File.WriteAllBytes(normalPath, normalData);
                GUtilities.DestroyObject(normal);

                AssetDatabase.Refresh();

                TextureImporter atlasImporter = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
                atlasImporter.wrapMode = TextureWrapMode.Clamp;
                atlasImporter.alphaIsTransparency = true;
                atlasImporter.SaveAndReimport();
                atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

                TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.wrapMode = TextureWrapMode.Clamp;
                normalImporter.SaveAndReimport();
                normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);


                Material mat = null;
                if (GCommon.CurrentRenderPipeline == GRenderPipelineType.Universal)
                {
                    mat = Object.Instantiate(GRuntimeSettings.Instance.foliageRendering.urpTreeBillboardMaterial);
                }
                else
                {
                    mat = Object.Instantiate(GRuntimeSettings.Instance.foliageRendering.treeBillboardMaterial);
                }
                if (mat != null)
                {
                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", atlas);
                    }
                    if (mat.HasProperty("_BumpMap"))
                    {
                        mat.SetTexture("_BumpMap", normal);
                    }
                    mat.name = args.Target.name + "_BillboardMaterial";
                    string matPath = Path.Combine(FileUtil.GetProjectRelativePath(saveFolder), mat.name + ".mat");
                    AssetDatabase.CreateAsset(mat, matPath);
                    billboardAsset.material = mat;
                }
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }

            EditorUtility.ClearProgressBar();
        }

        private void RefreshMaterialPropsSuggestions()
        {
            if (target == null)
            {
                SrcColorSuggestion.Clear();
                SrcColorSuggestion.Add("_Color");
                SrcTextureSuggestion.Clear();
                SrcTextureSuggestion.Add("_MainTex");
            }
            else
            {
                HashSet<Shader> shaders = new HashSet<Shader>();
                MeshRenderer[] mrs = target.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < mrs.Length; ++i)
                {
                    Material[] mats = mrs[i].sharedMaterials;
                    for (int j = 0; j < mats.Length; ++j)
                    {
                        shaders.Add(mats[j].shader);
                    }
                }

                HashSet<string> colorProps = new HashSet<string>();
                colorProps.Add("_Color");
                HashSet<string> textureProps = new HashSet<string>();
                textureProps.Add("_MainTex");
                IEnumerator<Shader> shaderIterator = shaders.GetEnumerator();
                while (shaderIterator.MoveNext())
                {
                    colorProps.UnionWith(GShaderParser.GetProperties(shaderIterator.Current, "Color"));
                    textureProps.UnionWith(GShaderParser.GetProperties(shaderIterator.Current, "2D"));
                }

                SrcColorSuggestion.Clear();
                SrcColorSuggestion.AddRange(colorProps);
                SrcTextureSuggestion.Clear();
                SrcTextureSuggestion.AddRange(textureProps);
            }
        }
    }
}
#endif
