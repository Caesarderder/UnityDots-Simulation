#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Enum = System.Enum;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    [CustomEditor(typeof(GFoliagePainter))]
    public class GFoliagePainterInspector : Editor
    {
        private GFoliagePainter painter;
        private Rect addFilterButtonRect;

        private Vector3[] worldPoints = new Vector3[4];

        private void OnEnable()
        {
            GCommon.RegisterBeginRender(OnBeginRender);
            GCommon.RegisterBeginRenderSRP(OnBeginRenderSRP);
            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;
            painter = (GFoliagePainter)target;
            Tools.hidden = true;

            foreach (SceneView sv in SceneView.sceneViews)
            {
                sv.wantsMouseEnterLeaveWindow = true;
            }
        }

        private void OnDisable()
        {
            GCommon.UnregisterBeginRender(OnBeginRender);
            GCommon.UnregisterBeginRenderSRP(OnBeginRenderSRP);
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
            Tools.hidden = false;

            foreach (SceneView sv in SceneView.sceneViews)
            {
                sv.wantsMouseEnterLeaveWindow = false;
            }
        }

        private void OnUndoRedo()
        {
            if (Selection.activeGameObject != painter.gameObject)
                return;
            if (string.IsNullOrEmpty(GUndoCompatibleBuffer.Instance.CurrentBackupName))
                return;
            GBackup.Restore(GUndoCompatibleBuffer.Instance.CurrentBackupName);
        }

        private class GBaseGUI
        {
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the terrain group this painter will work on");
            public static readonly GUIContent ENABLE_HISTORY = new GUIContent("Enable History", "Enable history recording for undo. History recording may get slow when working with a large group of terrains");
            public static readonly GUIContent ENABLE_TERRAIN_MASK = new GUIContent("Enable Terrain Mask", "Use the terrain's Mask texture (R channel) to lock particular regions from being edited");
            public static readonly GUIContent SHOW_TERRAIN_MASK = new GUIContent("Show Terrain Mask", "Draw the terrain mask overlay in the scene view. Disable this toggle if you are experiencing some frame rate drop");
            public static readonly GUIContent NO_PAINTER_FOUND = new GUIContent("No painter found!");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            painter.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GBaseGUI.GROUP_ID, painter.GroupId);
            GEditorSettings.Instance.paintTools.enableHistory = EditorGUILayout.Toggle(GBaseGUI.ENABLE_HISTORY, GEditorSettings.Instance.paintTools.enableHistory);
            painter.EnableTerrainMask = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TERRAIN_MASK, painter.EnableTerrainMask);
            if (painter.EnableTerrainMask)
            {
                GEditorSettings.Instance.paintTools.showTerrainMask = EditorGUILayout.Toggle(GBaseGUI.SHOW_TERRAIN_MASK, GEditorSettings.Instance.paintTools.showTerrainMask);
            }

            DrawPaintMode();
            IGFoliagePainter p = painter.ActivePainter;
            if (p == null)
            {
                EditorGUILayout.LabelField(GBaseGUI.NO_PAINTER_FOUND, GEditorCommon.WordWrapItalicLabel);
            }
            else
            {
                DrawInstructionGUI();
                DrawBrushMaskGUI();
                if (painter.Mode == GFoliagePaintingMode.PaintTree ||
                    painter.Mode == GFoliagePaintingMode.ScaleTree)
                {
                    DrawTreeSelectionGUI();
                }
                else if (painter.Mode == GFoliagePaintingMode.PaintGrass ||
                    painter.Mode == GFoliagePaintingMode.ScaleGrass)
                {
                    DrawGrassSelectionGUI();
                }
                else
                {
                    DrawTreeSelectionGUI();
                    DrawGrassSelectionGUI();
                }
                DrawBrushGUI();
                DrawFilterGUI();
                GEditorCommon.DrawBackupHelpBox();
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUtilities.MarkCurrentSceneDirty();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private class GInstructionGUI
        {
            public static readonly string LABEL = "Instruction";
            public static readonly string ID = "foliage-painter-instruction";
        }

        private void DrawInstructionGUI()
        {
            GEditorCommon.Foldout(GInstructionGUI.LABEL, true, GInstructionGUI.ID, () =>
            {
                string text = null;
                try
                {
                    text = painter.ActivePainter.Instruction;
                }
                catch (System.Exception e)
                {
                    text = e.Message;
                }
                EditorGUILayout.LabelField(text, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private class GBrushMaskGUI
        {
            public static readonly string LABEL = "Brush Masks";
            public static readonly string ID = "foliage-painter-brush-masks";
            public static readonly GUIContent NEW_BRUSH = new GUIContent("New Brush");
            public static readonly GUIContent REFRESH = new GUIContent("Refresh");
            public static readonly string NEW_BRUSH_DIALOG_TITLE = "Info";
            public static readonly string NEW_BRUSH_DIALOG_INSTRUCTION = "To add a new brush, copy your brush texture to a Resources/PolarisBrushes/ folder, then Refresh.";
            public static readonly string OK = "OK";
        }

        private void DrawBrushMaskGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GBrushMaskGUI.NEW_BRUSH,
                false,
                DisplayNewBrushDialog);
            menu.AddItem(
                GBrushMaskGUI.REFRESH,
                false,
                painter.ReloadBrushMasks);

            GEditorCommon.Foldout(GBrushMaskGUI.LABEL, true, GBrushMaskGUI.ID, () =>
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.selectedIndex = painter.SelectedBrushMaskIndex;
                args.collection = painter.BrushMasks;
                args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
                args.itemPerRow = 4;
                args.drawPreviewFunction = DrawBrushMaskPreview;
                painter.SelectedBrushMaskIndex = GEditorCommon.SelectionGrid(args);
            },
            menu);
        }

        private void DisplayNewBrushDialog()
        {
            EditorUtility.DisplayDialog(
                GBrushMaskGUI.NEW_BRUSH_DIALOG_TITLE,
                GBrushMaskGUI.NEW_BRUSH_DIALOG_INSTRUCTION,
                GBrushMaskGUI.OK);
        }

        private void DrawBrushMaskPreview(Rect r, object o)
        {
            Texture2D tex = (Texture2D)o;
            if (tex != null)
            {
                EditorGUI.DrawPreviewTexture(r, tex);
            }
            else
            {
                EditorGUI.DrawRect(r, Color.black);
            }
        }

        private class GTreeGUI
        {
            public static readonly string LABEL = "Trees";
            public static readonly string ID = "foliage-painter-trees";
        }

        private void DrawTreeSelectionGUI()
        {
            GEditorCommon.Foldout(GTreeGUI.LABEL, true, GTreeGUI.ID, () =>
            {
                painter.SelectedTreeIndices = GEditorCommon.TreeSetMultiSelectionGrid(painter.GroupId, painter.SelectedTreeIndices);
            });
        }

        private class GGrassGUI
        {
            public static readonly string LABEL = "Grasses";
            public static readonly string ID = "foliage-painter-grasses";
        }

        private void DrawGrassSelectionGUI()
        {
            GEditorCommon.Foldout(GGrassGUI.LABEL, true, GGrassGUI.ID, () =>
            {
                painter.SelectedGrassIndices = GEditorCommon.GrassSetMultiSelectionGrid(painter.GroupId, painter.SelectedGrassIndices);
            });
        }

        private class GBrushGUI
        {
            public static readonly string LABEL = "Brush";
            public static readonly string ID = "texture-painter-brush";

            public static readonly GUIContent JITTER = new GUIContent("Jitter", "Randomness factor for the value");
            public static readonly GUIContent RADIUS = new GUIContent("Radius", "Radius of the brush stroke. Shortcut: - =");
            public static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the brush stroke. Shortcut: [ ]");
            public static readonly GUIContent DENSITY = new GUIContent("Density", "Number of instance to spawn of each stroke. Shortcut: ; '");
            public static readonly GUIContent SCATTER = new GUIContent("Scatter", "Pick a random position for the brush around your cursor position");
            public static readonly GUIContent ERASE_RATIO = new GUIContent("Erase Ratio", "Erasing propability, useful for thin out the area");
            public static readonly GUIContent SCALE_STRENGTH = new GUIContent("Scale Strength", "How fast an instance to scale up and down");
        }

        private void DrawBrushGUI()
        {
            GEditorCommon.Foldout(GBrushGUI.LABEL, true, GBrushGUI.ID, () =>
            {
                painter.BrushRadius = EditorGUILayout.FloatField(GBrushGUI.RADIUS, painter.BrushRadius);
                painter.BrushRadiusJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushRadiusJitter, 0f, 1f);
                GEditorCommon.Separator();

                painter.BrushRotation = EditorGUILayout.Slider(GBrushGUI.ROTATION, painter.BrushRotation, -360f, 360f);
                painter.BrushRotationJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushRotationJitter, 0f, 1f);
                GEditorCommon.Separator();

                painter.BrushDensity = EditorGUILayout.IntSlider(GBrushGUI.DENSITY, painter.BrushDensity, 1, 100);
                painter.BrushDensityJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushDensityJitter, 0f, 1f);
                GEditorCommon.Separator();

                painter.BrushScatter = EditorGUILayout.Slider(GBrushGUI.SCATTER, painter.BrushScatter, 0f, 1f);
                painter.BrushScatterJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushScatterJitter, 0f, 1f);
                GEditorCommon.Separator();

                painter.EraseRatio = EditorGUILayout.Slider(GBrushGUI.ERASE_RATIO, painter.EraseRatio, 0f, 1f);
                painter.ScaleStrength = EditorGUILayout.FloatField(GBrushGUI.SCALE_STRENGTH, painter.ScaleStrength);
            });
        }

        private void DuringSceneGUI(SceneView sv)
        {
            HandleTerrainEditingInSceneView();
            HandleBrushSettingsShortcuts();
            HandleFunctionKeys();
            if (Event.current != null && Event.current.type == EventType.MouseMove)
                SceneView.RepaintAll();
            if (Event.current != null && Event.current.type == EventType.MouseLeaveWindow)
            {
                GFoliagePainterArgs args = new GFoliagePainterArgs();
                args.ShouldCommitNow = true;
                painter.Paint(args);
            }
        }

        private void HandleBrushSettingsShortcuts()
        {
            if (Event.current != null && Event.current.isKey)
            {
                KeyCode k = Event.current.keyCode;
                if (k == KeyCode.Equals)
                {
                    painter.BrushRadius += GEditorSettings.Instance.paintTools.radiusStep;
                }
                else if (k == KeyCode.Minus)
                {
                    painter.BrushRadius -= GEditorSettings.Instance.paintTools.radiusStep;
                }
                else if (k == KeyCode.RightBracket)
                {
                    painter.BrushRotation += GEditorSettings.Instance.paintTools.rotationStep;
                }
                else if (k == KeyCode.LeftBracket)
                {
                    painter.BrushRotation -= GEditorSettings.Instance.paintTools.rotationStep;
                }
                else if (k == KeyCode.Quote)
                {
                    painter.BrushDensity += GEditorSettings.Instance.paintTools.densityStep;
                }
                else if (k == KeyCode.Semicolon)
                {
                    painter.BrushDensity -= GEditorSettings.Instance.paintTools.densityStep;
                }
            }
        }

        private void HandleFunctionKeys()
        {
            if (Event.current != null && Event.current.type == EventType.KeyDown)
            {
                KeyCode k = Event.current.keyCode;
                if (k >= KeyCode.F1 && k <= KeyCode.F12)
                {
                    if (painter.Mode != GFoliagePaintingMode.Custom &&
                        (k - KeyCode.F1) != (int)GTexturePaintingMode.Custom)
                    {
                        painter.Mode = (GFoliagePaintingMode)(k - KeyCode.F1);
                    }
                    else
                    {
                        painter.CustomPainterIndex = k - KeyCode.F1;
                    }
                }
            }
        }

        private void HandleTerrainEditingInSceneView()
        {
            if (!painter.enabled)
                return;

            if (Event.current == null)
                return;

            if (Event.current.alt == true)
                return;

            if (Event.current.type != EventType.Repaint &&
                Event.current.type != EventType.MouseDown &&
                Event.current.type != EventType.MouseDrag &&
                Event.current.type != EventType.MouseUp &&
                Event.current.type != EventType.KeyDown)
                return;

            int controlId = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    //Set the hot control to this tool, to disable marquee selection tool on mouse dragging
                    GUIUtility.hotControl = controlId;
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                GRuntimeSettings.Instance.isEditingGeometry = false;
                if (GUIUtility.hotControl == controlId)
                {
                    //Return the hot control back to Unity, use the default
                    GUIUtility.hotControl = 0;
                }
            }

            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (GStylizedTerrain.Raycast(r, out hit, float.MaxValue, painter.GroupId))
            {
                OnRaycast(true, hit);
            }
            else
            {
                OnRaycast(false, hit);
            }

            if (GGuiEventUtilities.IsLeftMouse)
            {
                Event.current.Use();
            }
        }

        private void OnRaycast(bool isHit, RaycastHit hitInfo)
        {
            if (isHit)
            {
                DrawHandleAtCursor(hitInfo);
                if (GGuiEventUtilities.IsLeftMouse)
                {
                    Paint(hitInfo);
                }
            }
        }

        private class GCursorHandleGUI
        {
            public static readonly int COLOR = Shader.PropertyToID("_Color");
            public static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
            public static readonly int WORLD_TO_CURSOR = Shader.PropertyToID("_WorldToCursorMatrix");
        }

        private void DrawHandleAtCursor(RaycastHit hit)
        {
            Color cursorColor =
                Event.current.shift ? GEditorSettings.Instance.paintTools.alternativeActionCursorColor :
                Event.current.control ? GEditorSettings.Instance.paintTools.negativeActionCursorColor :
                GEditorSettings.Instance.paintTools.normalActionCursorColor;

            cursorColor.a = cursorColor.a * Mathf.Lerp(0.5f, 1f, painter.BrushDensity * 1.0f / 100f);

            if (GEditorSettings.Instance.paintTools.useSimpleCursor)
            {
                Handles.color = cursorColor;
                Vector3[] corner = GCommon.GetBrushQuadCorners(hit.point, painter.BrushRadius, painter.BrushRotation);
                Handles.DrawAAPolyLine(5, corner[0], corner[1], corner[2], corner[3], corner[0]);
            }
            else
            {
                Matrix4x4 cursorToWorld = Matrix4x4.TRS(hit.point, Quaternion.Euler(0, painter.BrushRotation, 0), 2 * painter.BrushRadius * Vector3.one);
                worldPoints[0] = cursorToWorld.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
                worldPoints[1] = cursorToWorld.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
                worldPoints[2] = cursorToWorld.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
                worldPoints[3] = cursorToWorld.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));

                Material mat = GInternalMaterials.PainterCursorProjectorMaterial;
                mat.SetColor(GCursorHandleGUI.COLOR, cursorColor);
                mat.SetTexture(GCursorHandleGUI.MAIN_TEX, painter.BrushMasks[painter.SelectedBrushMaskIndex]);
                mat.SetMatrix(GCursorHandleGUI.WORLD_TO_CURSOR, cursorToWorld.inverse);
                mat.SetPass(0);

                List<GOverlapTestResult> overlapTest = GPaintToolUtilities.OverlapTest(painter.GroupId, hit.point, painter.BrushRadius, painter.BrushRotation);
                foreach (GOverlapTestResult test in overlapTest)
                {
                    if (test.IsOverlapped)
                    {
                        DrawCursorProjected(test.Terrain, test.IsChunkOverlapped);
                    }
                }
            }
        }

        private void DrawCursorProjected(GStylizedTerrain t, bool[] chunkCulling)
        {
            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                if (chunkCulling[i] == false)
                    continue;
                Mesh m = chunks[i].GetMesh(0);
                Graphics.DrawMeshNow(
                    m,
                    chunks[i].transform.localToWorldMatrix);
            }
        }

        private void Paint(RaycastHit hit)
        {
            GFoliagePainterArgs args = new GFoliagePainterArgs();
            args.HitPoint = hit.point;
            
            args.MouseEventType =
                Event.current.type == EventType.MouseDown ? GPainterMouseEventType.Down :
                Event.current.type == EventType.MouseDrag ? GPainterMouseEventType.Drag :
                GPainterMouseEventType.Up;
            args.ActionType =
                Event.current.shift ? GPainterActionType.Alternative :
                Event.current.control ? GPainterActionType.Negative :
                GPainterActionType.Normal;
            painter.Paint(args);
        }

        private class GFilterGUI
        {
            public static readonly string LABEL = "Filters";
            public static readonly string ID = "foliage-painter-filters";
            public static readonly GUIContent ADD_FILTER = new GUIContent("Add Filter");
            public static readonly GUIContent NO_SUITABLE_FILTER = new GUIContent("No suitable filter!");
        }

        private void DrawFilterGUI()
        {
            GEditorCommon.Foldout(GFilterGUI.LABEL, true, GFilterGUI.ID, () =>
            {
                List<Type> suitableFilterTypes = null;
                try
                {
                    suitableFilterTypes = painter.ActivePainter.SuitableFilterTypes;
                }
                catch (System.Exception)
                {
                    suitableFilterTypes = new List<Type>();
                }
                GSpawnFilter[] filters = painter.GetComponents<GSpawnFilter>();
                for (int i = 0; i < filters.Length; ++i)
                {
                    Type filterType = filters[i].GetType();
                    GUI.enabled = suitableFilterTypes.Contains(filterType);
                    EditorGUILayout.LabelField(GEditorCommon.GetClassDisplayName(filterType), GEditorCommon.ItalicLabel);
                    GUI.enabled = true;
                }

                if (filters.Length > 0)
                {
                    GEditorCommon.Separator();
                }

                Rect r = EditorGUILayout.GetControlRect();
                if (Event.current.type == EventType.Repaint)
                    addFilterButtonRect = r;
                if (GUI.Button(r, GFilterGUI.ADD_FILTER))
                {
                    GenericMenu menu = new GenericMenu();
                    if (suitableFilterTypes.Count == 0)
                    {
                        menu.AddDisabledItem(GFilterGUI.NO_SUITABLE_FILTER);
                    }
                    else
                    {
                        for (int i = 0; i < suitableFilterTypes.Count; ++i)
                        {
                            Type t = suitableFilterTypes[i];
                            menu.AddItem(
                                new GUIContent(GEditorCommon.GetClassDisplayName(t)),
                                false,
                                () =>
                                {
                                    AddFilter(t);
                                });
                        }
                    }
                    menu.DropDown(addFilterButtonRect);
                }
            });
        }

        private void AddFilter(Type t)
        {
            painter.gameObject.AddComponent(t);
        }

        private class GPaintModeGUI
        {
            public static readonly string LABEL = "Mode";
            public static readonly string ID = "texture-painter-mode";

            public static readonly GUIContent NO_CUSTOM_PAINTER = new GUIContent("No Custom Painter defined!");
            public static readonly GUIContent CUSTOM_ARGS = new GUIContent("Custom Args");
        }

        private void DrawPaintMode()
        {
            GEditorCommon.Foldout(GPaintModeGUI.LABEL, true, GPaintModeGUI.ID, () =>
            {
                ShowPaintModeAsGrid();
            });
        }

        private void ShowPaintModeAsGrid()
        {
            GSelectionGridArgs args0 = new GSelectionGridArgs();
            args0.selectedIndex = (int)painter.Mode;
            args0.collection = Enum.GetValues(typeof(GFoliagePaintingMode));
            args0.itemSize = GEditorCommon.selectionGridTileSizeWide;
            args0.itemPerRow = 2;
            args0.drawPreviewFunction = DrawModePreview;
            EditorGUI.BeginChangeCheck();
            painter.Mode = (GFoliagePaintingMode)GEditorCommon.SelectionGrid(args0);
            if (EditorGUI.EndChangeCheck())
            {
            }

            if (painter.Mode == GFoliagePaintingMode.Custom)
            {
                GEditorCommon.Separator();
                List<Type> customPainterTypes = GFoliagePainter.CustomPainterTypes;
                if (customPainterTypes.Count == 0)
                {
                    EditorGUILayout.LabelField(GPaintModeGUI.NO_CUSTOM_PAINTER, GEditorCommon.WordWrapItalicLabel);
                }
                else
                {
                    GSelectionGridArgs args1 = new GSelectionGridArgs();
                    args1.selectedIndex = painter.CustomPainterIndex;
                    args1.collection = customPainterTypes;
                    args1.itemSize = GEditorCommon.selectionGridTileSizeWide;
                    args1.itemPerRow = 2;
                    args1.drawPreviewFunction = DrawCustomMode;
                    painter.CustomPainterIndex = GEditorCommon.SelectionGrid(args1);
                    GEditorCommon.Separator();
                    painter.CustomPainterArgs = EditorGUILayout.TextField(GPaintModeGUI.CUSTOM_ARGS, painter.CustomPainterArgs);
                }
            }
        }

        private void DrawModePreview(Rect r, object o)
        {
            GFoliagePaintingMode mode = (GFoliagePaintingMode)o;
            Texture2D icon = GEditorSkin.Instance.GetTexture(mode.ToString() + "Icon");
            if (icon == null)
            {
                icon = GEditorSkin.Instance.GetTexture("GearIcon");
            }
            string label = ObjectNames.NicifyVariableName(mode.ToString());
            DrawMode(r, label, icon);
        }

        private void DrawCustomMode(Rect r, object o)
        {
            Type t = (Type)o;
            if (t != null)
            {
                string label = ObjectNames.NicifyVariableName(GEditorCommon.GetClassDisplayName(t));
                Texture2D icon = GEditorSkin.Instance.GetTexture("GearIcon");
                DrawMode(r, label, icon);
            }
        }

        private void DrawMode(Rect r, string label, Texture icon)
        {
            GUIStyle labelStyle = EditorStyles.miniLabel;
            Rect iconRect = new Rect(r.min.x, r.min.y, r.height, r.height);
            Rect labelRect = new Rect(r.min.x + r.height + 1, r.min.y, r.width - r.height, r.height);

            GEditorCommon.DrawBodyBox(r);
            GEditorCommon.DrawHeaderBox(iconRect);
            if (icon != null)
            {
                GUI.color = labelStyle.normal.textColor;
                RectOffset offset = new RectOffset(3, 3, 3, 3);
                GUI.DrawTexture(offset.Remove(iconRect), icon);
                GUI.color = Color.white;
            }

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            GUI.Label(labelRect, label, labelStyle);
            EditorGUI.indentLevel = indent;
        }

        private void OnBeginRender(Camera cam)
        {
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (painter.EnableTerrainMask && GEditorSettings.Instance.paintTools.showTerrainMask)
            {
                DrawTerrainMask(cam);
            }
        }

        private void DrawTerrainMask(Camera cam)
        {
            GCommon.ForEachTerrain(painter.GroupId, (t) =>
            {
                GLivePreviewDrawer.DrawTerrainMask(t, cam);
            });
        }

        private void OnBeginRenderSRP(ScriptableRenderContext context, Camera cam)
        {
            OnBeginRender(cam);
        }
    }
}
#endif
