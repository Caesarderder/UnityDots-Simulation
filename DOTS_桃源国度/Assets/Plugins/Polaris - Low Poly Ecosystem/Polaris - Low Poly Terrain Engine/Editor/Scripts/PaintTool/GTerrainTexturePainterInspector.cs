#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Enum = System.Enum;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    [CustomEditor(typeof(GTerrainTexturePainter))]
    public class GTerrainPainterInspector : Editor
    {
        private GTerrainTexturePainter painter;
        private Vector3[] worldPoints = new Vector3[4];
        private bool isColorSamplingEnable;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;
            painter = (GTerrainTexturePainter)target;
            Tools.hidden = true;

            GCommon.UpdateMaterials(painter.GroupId);
            GCommon.RegisterBeginRender(OnBeginRender);
            GCommon.RegisterBeginRenderSRP(OnBeginRenderSRP);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
            Tools.hidden = false;

            GCommon.UnregisterBeginRender(OnBeginRender);
            GCommon.UnregisterBeginRenderSRP(OnBeginRenderSRP);
            GTerrainTexturePainter.Internal_ReleaseRenderTextures();
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
            public static readonly GUIContent FORCE_UPDATE_GEOMETRY = new GUIContent("Force Update Geometry", "Update terrain geometry even when the painter doesn't modify the height map. Turn this on when you're using Albedo To Vertex Color");
            public static readonly GUIContent ENABLE_HISTORY = new GUIContent("Enable History", "Enable history recording for undo. History recording may get slow when working with a large group of terrains");
            public static readonly GUIContent ENABLE_LIVE_PREVIEW = new GUIContent("Enable Live Preview", "Draw a preview on the terrain surface showing the painter effect");
            public static readonly GUIContent ENABLE_TERRAIN_MASK = new GUIContent("Enable Terrain Mask", "Use the terrain's Mask texture (R channel) to lock particular regions from being edited");
            public static readonly GUIContent SHOW_TERRAIN_MASK = new GUIContent("Show Terrain Mask", "Draw the terrain mask overlay in the scene view. Disable this toggle if you are experiencing some frame rate drop");
            public static readonly GUIContent ENABLE_TOPOGRAPHIC = new GUIContent("Enable Topographic", "Render a topographic view for better sense of elevation change");
            public static readonly GUIContent NO_PAINTER_FOUND = new GUIContent("No painter found!");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            painter.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GBaseGUI.GROUP_ID, painter.GroupId);
            painter.ForceUpdateGeometry = EditorGUILayout.Toggle(GBaseGUI.FORCE_UPDATE_GEOMETRY, painter.ForceUpdateGeometry);

            Color txtColor = GUI.contentColor;
            Rect r0 = EditorGUILayout.GetControlRect();
            if (!GEditorSettings.Instance.paintTools.enableHistory)
            {
                GUI.contentColor = Color.red*1.5f;
            }
            GEditorSettings.Instance.paintTools.enableHistory = EditorGUI.Toggle(r0, GBaseGUI.ENABLE_HISTORY, GEditorSettings.Instance.paintTools.enableHistory);
            GUI.contentColor = txtColor;

            GEditorSettings.Instance.paintTools.enableLivePreview = EditorGUILayout.Toggle(GBaseGUI.ENABLE_LIVE_PREVIEW, GEditorSettings.Instance.paintTools.enableLivePreview);

            bool isMaskPainterInUsed = painter.ActivePainter is GMaskPainter;
            if (!isMaskPainterInUsed)
            {
                painter.EnableTerrainMask = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TERRAIN_MASK, painter.EnableTerrainMask);
                if (painter.EnableTerrainMask)
                {
                    GEditorSettings.Instance.paintTools.showTerrainMask = EditorGUILayout.Toggle(GBaseGUI.SHOW_TERRAIN_MASK, GEditorSettings.Instance.paintTools.showTerrainMask);
                }
            }

            GEditorSettings.Instance.topographic.enable = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TOPOGRAPHIC, GEditorSettings.Instance.topographic.enable);

            DrawPaintMode();
            IGTexturePainter p = painter.ActivePainter;
            if (p == null)
            {
                EditorGUILayout.LabelField(GBaseGUI.NO_PAINTER_FOUND, GEditorCommon.WordWrapItalicLabel);
            }
            else
            {
                DrawInstructionGUI();
                DrawBrushMaskGUI();
                if (painter.Mode == GTexturePaintingMode.Splat || painter.Mode == GTexturePaintingMode.Custom)
                    DrawSplatGUI();
                DrawBrushGUI();
                if (p is IConditionalPainter)
                    DrawRulesGUI();
                if (p is IGTexturePainterWithCustomParams)
                {
                    IGTexturePainterWithCustomParams activePainter = painter.ActivePainter as IGTexturePainterWithCustomParams;
                    activePainter.Editor_DrawCustomParamsGUI();
                }

#if GRIFFIN_VEGETATION_STUDIO_PRO
                GEditorCommon.DrawVspIntegrationGUI();
#endif

                GEditorCommon.DrawBackupHelpBox();
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUtilities.MarkCurrentSceneDirty();
                EditorUtility.SetDirty(painter);
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private class GInstructionGUI
        {
            public static readonly string LABEL = "Instruction";
            public static readonly string ID = "texture-painter-instruction";
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
            public static readonly string ID = "texture-painter-brush-masks";
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

        private class GSplatGUI
        {
            public static readonly string LABEL = "Splats";
            public static readonly string ID = "texture-painter-splats";
            public static readonly GUIContent MULTI_SELECTION = new GUIContent("Multi Selection", "Select multiple splats by setting their probability");
        }

        private void DrawSplatGUI()
        {
            GEditorCommon.Foldout(GSplatGUI.LABEL, true, GSplatGUI.ID, () =>
            {
                if (GEditorSettings.Instance.paintTools.useMultiSplatsSelector)
                {
                    painter.SelectedSplatIndices = GEditorCommon.SplatSetMultiSelectionGrid(painter.GroupId, painter.SelectedSplatIndices);
                }
                else
                {
                    painter.SelectedSplatIndex = GEditorCommon.SplatSetSelectionGrid(painter.GroupId, painter.SelectedSplatIndex);
                }
                GEditorSettings.Instance.paintTools.useMultiSplatsSelector = EditorGUILayout.Toggle(GSplatGUI.MULTI_SELECTION, GEditorSettings.Instance.paintTools.useMultiSplatsSelector);
                EditorUtility.SetDirty(GEditorSettings.Instance);
                EditorGUILayout.Space();
            });
        }

        private class GBrushGUI
        {
            public static readonly string LABEL = "Brush";
            public static readonly string ID = "texture-painter-brush";

            public static readonly GUIContent JITTER = new GUIContent("Jitter", "Randomness factor for the value");
            public static readonly GUIContent RADIUS = new GUIContent("Radius", "Radius of the brush stroke. Shortcut: - =");
            public static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the brush stroke. Shortcut: [ ]");
            public static readonly GUIContent OPACITY = new GUIContent("Opacity", "Opacity of the brush stroke. Shortcut: ; '");
            public static readonly GUIContent TARGET_STRENGTH = new GUIContent("Target Strength", "Maximum intensity of the brush");
            public static readonly GUIContent SCATTER = new GUIContent("Scatter", "Pick a random position for the brush around your cursor position");
            public static readonly GUIContent COLOR = new GUIContent("Color", "Color of the brush. Mainly used for Albedo, Metallic (R) and Smoothness (A) painter");
            public static readonly GUIContent SAMPLE_POINT = new GUIContent("Sample Point", "A point in the scene for sample height. Mainly use for Height Sampling (Y) painter.");

            public static readonly GUIContent TRIANGLE_SAMPLER = new GUIContent(GEditorSkin.Instance.GetTexture("TriangleSamplerIcon"), "Sample terrain Albedo color, Metallic or Smoothness value based on the current paint mode");
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
                
                Color txtColor = GUI.contentColor;
                Rect r0 = EditorGUILayout.GetControlRect();
                if (painter.BrushOpacity <= 0 || painter.BrushOpacity >= 1)
                {
                    GUI.contentColor = Color.red * 1.5f;
                }
                painter.BrushOpacity = EditorGUI.Slider(r0, GBrushGUI.OPACITY, painter.BrushOpacity, 0f, 1f);
                GUI.contentColor = txtColor;

                painter.BrushOpacityJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushOpacityJitter, 0f, 1f);

                Rect r1 = EditorGUILayout.GetControlRect();
                if (painter.BrushTargetStrength <= 0)
                {
                    GUI.contentColor = Color.red * 1.5f;
                }
                painter.BrushTargetStrength = EditorGUI.Slider(r1, GBrushGUI.TARGET_STRENGTH, painter.BrushTargetStrength, 0f, 1f); 
                GUI.contentColor = txtColor;
                GEditorCommon.Separator();

                painter.BrushScatter = EditorGUILayout.Slider(GBrushGUI.SCATTER, painter.BrushScatter, 0f, 1f);
                painter.BrushScatterJitter = EditorGUILayout.Slider(GBrushGUI.JITTER, painter.BrushScatterJitter, 0f, 1f);
                GEditorCommon.Separator();

                EditorGUILayout.BeginHorizontal();
                painter.BrushColor = EditorGUILayout.ColorField(GBrushGUI.COLOR, painter.BrushColor);
                GUI.enabled =
                    painter.Mode == GTexturePaintingMode.Albedo ||
                    painter.Mode == GTexturePaintingMode.Metallic ||
                    painter.Mode == GTexturePaintingMode.Smoothness;
                GUI.color = isColorSamplingEnable ? Color.white * 1.5f : Color.white;
                if (GUILayout.Button(GBrushGUI.TRIANGLE_SAMPLER, GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    isColorSamplingEnable = !isColorSamplingEnable;
                }
                GUI.enabled = true;
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                painter.SamplePoint = GEditorCommon.InlineVector3Field(GBrushGUI.SAMPLE_POINT, painter.SamplePoint);
            });
        }

        private class GRulesGUI
        {
            public static readonly string LABEL = "Rules";
            public static readonly string ID = "texture-painter-rules";

            public static readonly string HEIGHT_RULE_HEADER = "Height";
            public static readonly GUIContent ENABLE_HEIGHT_RULE = new GUIContent("Enable", "Enable height rule");
            public static readonly GUIContent MIN_HEIGHT = new GUIContent("Min", "Minimum height in meter for the painter to take effect");
            public static readonly GUIContent MAX_HEIGHT = new GUIContent("Max", "Maximum height in meter for the painter to take effect");
            public static readonly GUIContent HEIGHT_TRANSITION = new GUIContent("Transition", "Intensity of the height blend in the range of [min,max]");

            public static readonly string SLOPE_RULE_HEADER = "Slope";
            public static readonly GUIContent ENABLE_SLOPE_RULE = new GUIContent("Enable", "Enable slope rule");
            public static readonly GUIContent NORMAL_MAP_MODE = new GUIContent("Mode", "Slope mode to use");
            public static readonly GUIContent MIN_SLOPE = new GUIContent("Min", "Minimum angle between surface normal and the up vector for the painter to take effect");
            public static readonly GUIContent MAX_SLOPE = new GUIContent("Max", "Maximum angle between surface normal and the up vector for the painter to take effect");
            public static readonly GUIContent SLOPE_TRANSITION = new GUIContent("Transition", "Intensity of the slope blend in the range of [min,max]");

            public static readonly string NOISE_RULE_HEADER = "Noise";
            public static readonly GUIContent ENABLE_NOISE_RULE = new GUIContent("Enable", "Enable noise rule");
            public static readonly GUIContent NOISE_ORIGIN = new GUIContent("Origin", "The origin point of the noise map");
            public static readonly GUIContent NOISE_FREQUENCY = new GUIContent("Frequency", "Frequency of the noise pattern, higher value gives thicker noise");
            public static readonly GUIContent NOISE_OCTAVES = new GUIContent("Octaves", "The number of noise layers to stack on top of each others");
            public static readonly GUIContent NOISE_LACUNARITY = new GUIContent("Lacunarity", "The change in frequency of each noise layer");
            public static readonly GUIContent NOISE_PERSISTENCE = new GUIContent("Persistence", "The change in amplitude of each noise layer");
            public static readonly GUIContent NOISE_REMAP = new GUIContent("Remap", "Remap the generated noise value");
        }

        private void DrawRulesGUI()
        {
            GEditorCommon.Foldout(GRulesGUI.LABEL, false, GRulesGUI.ID, () =>
            {
                GConditionalPaintingConfigs configs = painter.ConditionalPaintingConfigs;

                GEditorCommon.Header(GRulesGUI.HEIGHT_RULE_HEADER);
                configs.BlendHeight = EditorGUILayout.Toggle(GRulesGUI.ENABLE_HEIGHT_RULE, configs.BlendHeight);
                configs.MinHeight = EditorGUILayout.FloatField(GRulesGUI.MIN_HEIGHT, configs.MinHeight);
                configs.MaxHeight = EditorGUILayout.FloatField(GRulesGUI.MAX_HEIGHT, configs.MaxHeight);
                EditorGUI.BeginChangeCheck();
                configs.HeightTransition = EditorGUILayout.CurveField(GRulesGUI.HEIGHT_TRANSITION, configs.HeightTransition, Color.red, GCommon.UnitRect);
                if (EditorGUI.EndChangeCheck())
                {
                    configs.UpdateCurveTextures();
                }

                GEditorCommon.Header(GRulesGUI.SLOPE_RULE_HEADER);
                configs.BlendSlope = EditorGUILayout.Toggle(GRulesGUI.ENABLE_SLOPE_RULE, configs.BlendSlope);
                configs.NormalMapMode = (GNormalMapMode)EditorGUILayout.EnumPopup(GRulesGUI.NORMAL_MAP_MODE, configs.NormalMapMode);
                configs.MinSlope = EditorGUILayout.FloatField(GRulesGUI.MIN_SLOPE, configs.MinSlope);
                configs.MaxSlope = EditorGUILayout.FloatField(GRulesGUI.MAX_SLOPE, configs.MaxSlope);
                EditorGUI.BeginChangeCheck();
                configs.SlopeTransition = EditorGUILayout.CurveField(GRulesGUI.SLOPE_TRANSITION, configs.SlopeTransition, Color.red, GCommon.UnitRect);
                if (EditorGUI.EndChangeCheck())
                {
                    configs.UpdateCurveTextures();
                }

                GEditorCommon.Header(GRulesGUI.NOISE_RULE_HEADER);
                configs.BlendNoise = EditorGUILayout.Toggle(GRulesGUI.ENABLE_NOISE_RULE, configs.BlendNoise);
                configs.NoiseOrigin = GEditorCommon.InlineVector3Field(GRulesGUI.NOISE_ORIGIN, configs.NoiseOrigin);
                configs.NoiseFrequency = EditorGUILayout.FloatField(GRulesGUI.NOISE_FREQUENCY, configs.NoiseFrequency);
                configs.NoiseLacunarity = EditorGUILayout.FloatField(GRulesGUI.NOISE_LACUNARITY, configs.NoiseLacunarity);
                configs.NoisePersistence = EditorGUILayout.Slider(GRulesGUI.NOISE_PERSISTENCE, configs.NoisePersistence, 0.01f, 1f);
                configs.NoiseOctaves = EditorGUILayout.IntSlider(GRulesGUI.NOISE_OCTAVES, configs.NoiseOctaves, 1, 4);
                EditorGUI.BeginChangeCheck();
                configs.NoiseRemap = EditorGUILayout.CurveField(GRulesGUI.NOISE_REMAP, configs.NoiseRemap, Color.red, GCommon.UnitRect);
                if (EditorGUI.EndChangeCheck())
                {
                    configs.UpdateCurveTextures();
                }
            });
        }

        private void DuringSceneGUI(SceneView sv)
        {
            HandleTerrainEditingInSceneView();
            HandleBrushSettingsShortcuts();
            HandleFunctionKeys();
            if (Event.current != null && Event.current.type == EventType.MouseMove)
                SceneView.RepaintAll();
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
                    painter.BrushOpacity += GEditorSettings.Instance.paintTools.opacityStep;
                }
                else if (k == KeyCode.Semicolon)
                {
                    painter.BrushOpacity -= GEditorSettings.Instance.paintTools.opacityStep;
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
                    if (painter.Mode != GTexturePaintingMode.Custom &&
                        (k - KeyCode.F1) != (int)GTexturePaintingMode.Custom)
                    {
                        painter.Mode = (GTexturePaintingMode)(k - KeyCode.F1);
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
                if (GEditorSettings.Instance.paintTools.enableLivePreview &&
                    painter.ActivePainter != null &&
                    painter.ActivePainter is IGTexturePainterWithLivePreview)
                {

                }
                else
                {
                    if (!isColorSamplingEnable)
                    {
                        DrawHandleAtCursor(hitInfo);
                    }
                }

                if (GGuiEventUtilities.IsLeftMouseUp && isColorSamplingEnable)
                {
                    SampleColor(hitInfo);
                    isColorSamplingEnable = false;
                }

                if (GGuiEventUtilities.IsLeftMouse && !isColorSamplingEnable)
                {
                    Paint(hitInfo);
                }
            }
        }

        private void SampleColor(RaycastHit hit)
        {
            GStylizedTerrain t = hit.collider.GetComponentInParent<GStylizedTerrain>();
            if (t == null)
                return;
            if (t.TerrainData == null)
                return;

            Texture2D textureToSample =
                painter.Mode == GTexturePaintingMode.Albedo ? t.TerrainData.Shading.AlbedoMapOrDefault :
                painter.Mode == GTexturePaintingMode.Metallic ? t.TerrainData.Shading.MetallicMapOrDefault :
                painter.Mode == GTexturePaintingMode.Smoothness ? t.TerrainData.Shading.MetallicMapOrDefault :
                null;
            if (textureToSample == null)
                return;

            painter.BrushColor = textureToSample.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y).gamma;
        }

        private GTexturePainterArgs CreateBasicArgs(RaycastHit hit)
        {
            GTexturePainterArgs args = new GTexturePainterArgs();
            args.HitPoint = hit.point;
            
            args.MouseEventType =
                Event.current.type == EventType.MouseDown ? GPainterMouseEventType.Down :
                Event.current.type == EventType.MouseDrag ? GPainterMouseEventType.Drag :
                GPainterMouseEventType.Up;
            args.ActionType =
                Event.current.shift ? GPainterActionType.Alternative :
                Event.current.control ? GPainterActionType.Negative :
                GPainterActionType.Normal;

            return args;
        }

        private void Paint(RaycastHit hit)
        {
            GTexturePainterArgs args = CreateBasicArgs(hit);
            painter.Paint(args);
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

            cursorColor.a = cursorColor.a * Mathf.Lerp(0.5f, 1f, painter.BrushOpacity);

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
            args0.collection = Enum.GetValues(typeof(GTexturePaintingMode));
            args0.itemSize = GEditorCommon.selectionGridTileSizeWide;
            args0.itemPerRow = 2;
            args0.drawPreviewFunction = DrawModePreview;
            EditorGUI.BeginChangeCheck();
            painter.Mode = (GTexturePaintingMode)GEditorCommon.SelectionGrid(args0);
            if (EditorGUI.EndChangeCheck())
            {
            }

            if (painter.Mode == GTexturePaintingMode.Custom)
            {
                GEditorCommon.Separator();
                List<Type> customPainterTypes = GTerrainTexturePainter.GetCustomPainterTypes();
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
            GTexturePaintingMode mode = (GTexturePaintingMode)o;

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

            if (isColorSamplingEnable)
                return;

            if (GEditorSettings.Instance.paintTools.enableLivePreview)
            {
                bool canDrawLivePreview = painter.ActivePainter != null && painter.ActivePainter is IGTexturePainterWithLivePreview;
                if (canDrawLivePreview)
                {
                    Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hit;
                    if (GStylizedTerrain.Raycast(r, out hit, float.MaxValue, painter.GroupId))
                    {
                        DrawLivePreview(cam, hit);
                    }
                }
            }

            if (painter.EnableTerrainMask)
            {
                DrawMask(cam);
            }
        }

        private void OnBeginRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            OnBeginRender(cam);
        }

        private void DrawLivePreview(Camera cam, RaycastHit hitInfo)
        {
            bool canDrawLivePreview = painter.ActivePainter != null && painter.ActivePainter is IGTexturePainterWithLivePreview;
            if (!canDrawLivePreview)
                return;

            IGTexturePainterWithLivePreview activePainter = painter.ActivePainter as IGTexturePainterWithLivePreview;
            GTexturePainterArgs args = CreateBasicArgs(hitInfo);
            painter.FillArgs(ref args, true);

            List<GOverlapTestResult> overlapTest = GPaintToolUtilities.OverlapTest(painter.GroupId, hitInfo.point, painter.BrushRadius, painter.BrushRotation);
            foreach (GOverlapTestResult test in overlapTest)
            {
                if (test.IsOverlapped)
                {
                    activePainter.Editor_DrawLivePreview(test.Terrain, args, cam);
                }
            }
        }

        private void DrawMask(Camera cam)
        {
            GCommon.ForEachTerrain(painter.GroupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                if (painter.ActivePainter is GMaskPainter)
                {
                    if (GTexturePainterCustomParams.Instance.Mask.Visualize)
                    {
                        GLivePreviewDrawer.DrawMask4ChannelsLivePreview(t, cam, t.TerrainData.Mask.MaskMapOrDefault, GCommon.UnitRect);
                    }
                }
                else
                {
                    if (GEditorSettings.Instance.paintTools.showTerrainMask)
                        GLivePreviewDrawer.DrawTerrainMask(t, cam);
                }
            });
        }
    }
}
#endif
