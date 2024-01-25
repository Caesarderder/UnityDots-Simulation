#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using Pinwheel.Griffin.TextureTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [CustomEditor(typeof(GTextureStamper))]
    public class GTextureStamperInspector : Editor
    {
        private GTextureStamper instance;
        private Dictionary<string, RenderTexture> previewTextures;
        private MaterialPropertyBlock previewPropertyBlock;

        private const string HISTORY_PREFIX_COLOR = "Stamp AMS";
        private const string HISTORY_PREFIX_TEXTURE = "Stamp Splat";

        private readonly Vector3[] worldPoints = new Vector3[4];
        private readonly Vector2[] normalizedPoints = new Vector2[4];
        private readonly Vector3[] worldBox = new Vector3[8];

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;
            instance = target as GTextureStamper;
            Tools.hidden = true;

            instance.Internal_UpdateFalloffTexture();
            instance.Internal_UpdateLayerTransitionTextures();
            previewPropertyBlock = new MaterialPropertyBlock();
            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);

            GCommon.UpdateMaterials(instance.GroupId);
            UpdatePreview();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
            Tools.hidden = false;

            GCommon.UnregisterBeginRender(OnCameraRender);
            GCommon.UnregisterBeginRenderSRP(OnCameraRenderSRP);

            if (previewTextures != null)
            {
                foreach (string k in previewTextures.Keys)
                {
                    RenderTexture rt = previewTextures[k];
                    if (rt == null)
                        continue;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
            }
        }

        private void OnUndoRedo()
        {
            if (Selection.activeGameObject != instance.gameObject)
                return;
            if (string.IsNullOrEmpty(GUndoCompatibleBuffer.Instance.CurrentBackupName))
                return;
            GBackup.Restore(GUndoCompatibleBuffer.Instance.CurrentBackupName);
            UpdatePreview();
        }

        private class GBaseGUI
        {
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the terrain group which is edited by this tool");
            public static readonly GUIContent ENABLE_TERRAIN_MASK = new GUIContent("Enable Terrain Mask", "Use terrain mask (R) to lock a particular region from editing");
            public static readonly GUIContent SHOW_TERRAIN_MASK = new GUIContent("Show Terrain Mask", "Draw an overlay of the terrain mask in the scene view");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            instance.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GBaseGUI.GROUP_ID, instance.GroupId);
            instance.EnableTerrainMask = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TERRAIN_MASK, instance.EnableTerrainMask);
            if (instance.EnableTerrainMask)
            {
                GEditorSettings.Instance.stampTools.showTerrainMask = EditorGUILayout.Toggle(GBaseGUI.SHOW_TERRAIN_MASK, GEditorSettings.Instance.stampTools.showTerrainMask);
            }

            DrawInstructionGUI();
            DrawTransformGUI();
            DrawStampGUI();
            DrawStampLayersGUI();
            DrawGizmosGUI();
            DrawActionGUI();
            GEditorCommon.DrawBackupHelpBox();
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private class GInstructionGUI
        {
            public static readonly string LABEL = "Instruction";
            public static readonly string ID = "texture-stamper-instruction";

            public static readonly string INSTRUCTION = "Stamp texture or color onto the terrain surface.";
        }

        private void DrawInstructionGUI()
        {
            GEditorCommon.Foldout(GInstructionGUI.LABEL, true, GInstructionGUI.ID, () =>
            {
                EditorGUILayout.LabelField(GInstructionGUI.INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private class GTransformGUI
        {
            public static readonly string LABEL = "Transform";
            public static readonly string ID = "texture-stamper-transform";

            public static readonly GUIContent POSITION = new GUIContent("Position", "Position of the stamper");
            public static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the stamper");
            public static readonly GUIContent SCALE = new GUIContent("Scale", "Scale of the stamper");
        }

        private void DrawTransformGUI()
        {
            GEditorCommon.Foldout(GTransformGUI.LABEL, true, GTransformGUI.ID, () =>
            {
                instance.Position = GEditorCommon.InlineVector3Field(GTransformGUI.POSITION, instance.Position);
                instance.Rotation = Quaternion.Euler(GEditorCommon.InlineVector3Field(GTransformGUI.ROTATION, instance.Rotation.eulerAngles));
                instance.Scale = GEditorCommon.InlineVector3Field(GTransformGUI.SCALE, instance.Scale);

                Vector3 euler = instance.Rotation.eulerAngles;
                euler = new Vector3(0, euler.y, 0);
                instance.Rotation = Quaternion.Euler(euler);
            });
        }

        private class GStampGUI
        {
            public static readonly string LABEL = "Stamp";
            public static readonly string ID = "texture-stamper-stamp";

            public static readonly GUIContent MASK = new GUIContent("Mask", "A texture defines the opacity of the stamper");
            public static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Gradually decrease the mask intensity over its edge");
            public static readonly GUIContent CHANNEL = new GUIContent("Channel", "Choose the texture to stamp on, Albedo Metallic or Splat Controls");
        }

        private void DrawStampGUI()
        {
            GEditorCommon.Foldout(GStampGUI.LABEL, true, GStampGUI.ID, () =>
            {
                instance.Mask = EditorGUILayout.ObjectField(GStampGUI.MASK, instance.Mask, typeof(Texture2D), false) as Texture2D;

                EditorGUI.BeginChangeCheck();
                instance.Falloff = EditorGUILayout.CurveField(GStampGUI.FALLOFF, instance.Falloff, Color.red, new Rect(0, 0, 1, 1));
                if (EditorGUI.EndChangeCheck())
                {
                    instance.Internal_UpdateFalloffTexture();
                }
                instance.Channel = (GTextureStampChannel)EditorGUILayout.EnumPopup(GStampGUI.CHANNEL, instance.Channel);
            });
        }

        private class GLayerGUI
        {
            public static readonly string LABEL = "Layers";
            public static readonly string ID = "texture-stamper-layer";
            public static readonly string LAYER_ID_PREFIX = "texture-stamp-layer";

            public static readonly GUIContent ADD_LAYER = new GUIContent("Add Layer");

            public static readonly GUIContent MOVE_LAYER_UP = new GUIContent("Move Up");
            public static readonly GUIContent MOVE_LAYER_DOWN = new GUIContent("Move Down");
            public static readonly GUIContent REMOVE = new GUIContent("Remove");

            public static readonly string HEADER_GENERAL = "General";
            public static readonly GUIContent IGNORE = new GUIContent("Ignore", "Ignore this layer");
            public static readonly GUIContent NAME = new GUIContent("Name", "Display name of this layer");
            public static readonly GUIContent VISUALIZE_COLOR = new GUIContent("Visualize Color", "Color to draw a visualize mask in the scene view");

            public static readonly string HEADER_ALBEDO_METALLIC_SMOOTHNESS = "AMS";
            public static readonly GUIContent COLOR = new GUIContent("Color", "The albedo color to stamp");
            public static readonly GUIContent METALLIC = new GUIContent("Metallic", "The metallic value");
            public static readonly GUIContent SMOOTHNESS = new GUIContent("Smoothness", "The smoothness value");

            public static readonly string HEADER_SPLAT = "Splats";

            public static readonly string HEADER_HEIGHT_RULE = "Height Rule";
            public static readonly GUIContent BLEND_HEIGHT = new GUIContent("Enable", "Toggle terrain height rule");
            public static readonly GUIContent MIN_HEIGHT = new GUIContent("Min", "The minimum height to satisfy the rule");
            public static readonly GUIContent MAX_HEIGHT = new GUIContent("Max", "The maximum height to satisfy the rule");
            public static readonly GUIContent HEIGHT_TRANSITION = new GUIContent("Transition", "Fade factor for height rule");

            public static readonly string HEADER_SLOPE_RULE = "Slope Rule";
            public static readonly GUIContent BLEND_SLOPE = new GUIContent("Enable", "Toggle slope/steepness rule");
            public static readonly GUIContent NORMAL_MAP_MODE = new GUIContent("Mode", "Choose which normal map to read slope data from");
            public static readonly GUIContent MIN_SLOPE = new GUIContent("Min", "Minimum angle between surface normal and the up vector to satisfy the rule");
            public static readonly GUIContent MAX_SLOPE = new GUIContent("Max", "Maximum angle between surface normal and the up vector to satisfy the rule");
            public static readonly GUIContent SLOPE_TRANSITION = new GUIContent("Transition", "Fade factor for slope rule");

            public static readonly string HEADER_NOISE_RULE = "Noise Rule";
            public static readonly GUIContent BLEND_NOISE = new GUIContent("Enable", "Toggle noise rule");
            public static readonly GUIContent NOISE_ORIGIN = new GUIContent("Origin", "The origin point of the noise map");
            public static readonly GUIContent NOISE_FREQUENCY = new GUIContent("Frequency", "Frequency of the noise pattern, higher value gives thicker noise");
            public static readonly GUIContent NOISE_OCTAVES = new GUIContent("Octaves", "The number of noise layers to stack on top of each others");
            public static readonly GUIContent NOISE_LACUNARITY = new GUIContent("Lacunarity", "The change in frequency of each noise layer");
            public static readonly GUIContent NOISE_PERSISTENCE = new GUIContent("Persistence", "The change in amplitude of each noise layer");
            public static readonly GUIContent NOISE_REMAP = new GUIContent("Remap", "Remap the generated noise value");
        }

        private void DrawStampLayersGUI()
        {
            GEditorCommon.Foldout(GLayerGUI.LABEL, true, GLayerGUI.ID, () =>
            {
                List<GTextureStampLayer> layers = instance.Layers;
                for (int i = 0; i < layers.Count; ++i)
                {
                    DrawLayer(layers[i], i);
                }

                if (layers.Count > 0)
                {
                    GEditorCommon.Separator();
                }
                if (GUILayout.Button(GLayerGUI.ADD_LAYER))
                {
                    GTextureStampLayer layer = GTextureStampLayer.Create();
                    layers.Add(layer);
                }
            });
        }

        private void DrawLayer(GTextureStampLayer layer, int index)
        {
            string label = string.Format("Layer: {0} {1}",
                !string.IsNullOrEmpty(layer.Name) ? layer.Name : index.ToString(),
                layer.Ignore ? "[Ignored]" : string.Empty);
            string id = GLayerGUI.LAYER_ID_PREFIX + index;

            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GLayerGUI.MOVE_LAYER_UP,
                false,
                () =>
                {
                    int layerCount = instance.Layers.Count;
                    int swapIndex = Mathf.Clamp(index - 1, 0, layerCount - 1);

                    bool expand0 = GEditorCommon.GetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + index);
                    bool expand1 = GEditorCommon.GetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + swapIndex);
                    GEditorCommon.SetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + swapIndex, expand0);
                    GEditorCommon.SetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + index, expand1);

                    GUtilities.Swap(instance.Layers, index, swapIndex);
                    UpdatePreview();
                });
            menu.AddItem(
                GLayerGUI.MOVE_LAYER_DOWN,
                false,
                () =>
                {
                    int layerCount = instance.Layers.Count;
                    int swapIndex = Mathf.Clamp(index + 1, 0, layerCount - 1);

                    bool expand0 = GEditorCommon.GetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + index);
                    bool expand1 = GEditorCommon.GetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + swapIndex);
                    GEditorCommon.SetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + swapIndex, expand0);
                    GEditorCommon.SetFoldoutState(GLayerGUI.LAYER_ID_PREFIX + index, expand1);

                    GUtilities.Swap(instance.Layers, index, swapIndex);
                    UpdatePreview();
                });
            menu.AddSeparator(null);
            menu.AddItem(
                GLayerGUI.REMOVE,
                false,
                () =>
                {
                    ConfirmAndRemoveLayerAt(index);
                });

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                GEditorCommon.Header(GLayerGUI.HEADER_GENERAL);
                layer.Ignore = EditorGUILayout.Toggle(GLayerGUI.IGNORE, layer.Ignore);
                layer.Name = EditorGUILayout.TextField(GLayerGUI.NAME, layer.Name);

                if (instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
                {
                    GEditorCommon.Header(GLayerGUI.HEADER_ALBEDO_METALLIC_SMOOTHNESS);
                    layer.Color = EditorGUILayout.ColorField(GLayerGUI.COLOR, layer.Color);
                    layer.Metallic = EditorGUILayout.Slider(GLayerGUI.METALLIC, layer.Metallic, 0f, 1f);
                    layer.Smoothness = EditorGUILayout.Slider(GLayerGUI.SMOOTHNESS, layer.Smoothness, 0f, 1f);
                }
                else if (instance.Channel == GTextureStampChannel.Splat)
                {
                    GEditorCommon.Header(GLayerGUI.HEADER_SPLAT);
                    layer.SplatIndex = GEditorCommon.SplatSetSelectionGrid(instance.GroupId, layer.SplatIndex);
                }

                GEditorCommon.Header(GLayerGUI.HEADER_HEIGHT_RULE);
                layer.BlendHeight = EditorGUILayout.Toggle("Blend Height", layer.BlendHeight);
                if (layer.BlendHeight)
                {
                    EditorGUI.indentLevel += 1;
                    layer.MinHeight = EditorGUILayout.FloatField(GLayerGUI.MIN_HEIGHT, layer.MinHeight);
                    layer.MaxHeight = EditorGUILayout.FloatField(GLayerGUI.MAX_HEIGHT, layer.MaxHeight);
                    EditorGUI.BeginChangeCheck();
                    layer.HeightTransition = EditorGUILayout.CurveField(GLayerGUI.HEIGHT_TRANSITION, layer.HeightTransition, Color.red, new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        layer.UpdateCurveTextures();
                    }
                    EditorGUI.indentLevel -= 1;
                }

                GEditorCommon.Header(GLayerGUI.HEADER_SLOPE_RULE);
                layer.BlendSlope = EditorGUILayout.Toggle(GLayerGUI.BLEND_SLOPE, layer.BlendSlope);
                if (layer.BlendSlope)
                {
                    EditorGUI.indentLevel += 1;
                    layer.NormalMapMode = (GNormalMapMode)EditorGUILayout.EnumPopup(GLayerGUI.NORMAL_MAP_MODE, layer.NormalMapMode);
                    layer.MinSlope = EditorGUILayout.FloatField(GLayerGUI.MIN_SLOPE, layer.MinSlope);
                    layer.MaxSlope = EditorGUILayout.FloatField(GLayerGUI.MAX_SLOPE, layer.MaxSlope);
                    EditorGUI.BeginChangeCheck();
                    layer.SlopeTransition = EditorGUILayout.CurveField(GLayerGUI.SLOPE_TRANSITION, layer.SlopeTransition, Color.red, new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        layer.UpdateCurveTextures();
                    }
                    EditorGUI.indentLevel -= 1;
                }

                GEditorCommon.Header(GLayerGUI.HEADER_NOISE_RULE);
                layer.BlendNoise = EditorGUILayout.Toggle(GLayerGUI.BLEND_NOISE, layer.BlendNoise);
                if (layer.BlendNoise)
                {
                    EditorGUI.indentLevel += 1;
                    layer.NoiseOrigin = GEditorCommon.InlineVector2Field(GLayerGUI.NOISE_ORIGIN, layer.NoiseOrigin);
                    layer.NoiseFrequency = EditorGUILayout.FloatField(GLayerGUI.NOISE_FREQUENCY, layer.NoiseFrequency);
                    layer.NoiseLacunarity = EditorGUILayout.FloatField(GLayerGUI.NOISE_LACUNARITY, layer.NoiseLacunarity);
                    layer.NoisePersistence = EditorGUILayout.Slider(GLayerGUI.NOISE_PERSISTENCE, layer.NoisePersistence, 0.01f, 1f);
                    layer.NoiseOctaves = EditorGUILayout.IntSlider(GLayerGUI.NOISE_OCTAVES, layer.NoiseOctaves, 1, 4);
                    EditorGUI.BeginChangeCheck();
                    layer.NoiseRemap = EditorGUILayout.CurveField(GLayerGUI.NOISE_REMAP, layer.NoiseRemap, Color.red, new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        layer.UpdateCurveTextures();
                    }
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUI.indentLevel += 1;
            },
            menu);
        }

        private class GRemoveLayerDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";
        }

        private void ConfirmAndRemoveLayerAt(int index)
        {
            GTextureStampLayer layer = instance.Layers[index];
            string layerName = string.IsNullOrEmpty(layer.Name) ? ("Layer " + index) : ("Layer " + layer.Name);
            if (EditorUtility.DisplayDialog(
                GRemoveLayerDialogGUI.CONFIRM,
                "Remove " + layerName + "?",
                 GRemoveLayerDialogGUI.OK, GRemoveLayerDialogGUI.CANCEL))
            {
                instance.Layers.RemoveAt(index);
            }
        }

        private class GGizmosGUI
        {
            public static readonly string LABEL = "Gizmos";
            public static readonly string ID = "texture-stamper-gizmos";

            public static readonly GUIContent LIVE_PREVIEW = new GUIContent("Live Preview", "Draw a preview in the scene view");
            public static readonly GUIContent BOUNDS = new GUIContent("Bounds", "Show the stamper bounds in the scene view");
        }

        private void DrawGizmosGUI()
        {
            GEditorCommon.Foldout(GGizmosGUI.LABEL, true, GGizmosGUI.ID, () =>
            {
                GEditorSettings.Instance.stampTools.showLivePreview = EditorGUILayout.Toggle(GGizmosGUI.LIVE_PREVIEW, GEditorSettings.Instance.stampTools.showLivePreview);
                GEditorSettings.Instance.stampTools.showBounds = EditorGUILayout.Toggle(GGizmosGUI.BOUNDS, GEditorSettings.Instance.stampTools.showBounds);
            });
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "texture-stamper-action";
            public static readonly GUIContent SNAP_TO_TERRAIN = new GUIContent("Snap To Terrain", "Fit the stamper to the underneath terrain");
            public static readonly GUIContent SNAP_TO_LEVEL_BOUNDS = new GUIContent("Snap To Level Bounds", "Fit the stamper to cover all terrains in the level");
            public static readonly GUIContent APPLY = new GUIContent("Apply", "Stamp on the terrains");
        }

        private void DrawActionGUI()
        {
            GEditorCommon.Foldout(GActionGUI.LABEL, true, GActionGUI.ID, () =>
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(GActionGUI.SNAP_TO_TERRAIN))
                {
                    IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
                    while (terrains.MoveNext())
                    {
                        GStylizedTerrain t = terrains.Current;
                        Bounds b = t.Bounds;
                        Rect r = new Rect(new Vector2(b.min.x, b.min.z), new Vector2(b.size.x, b.size.z));
                        Vector2 p = new Vector2(instance.Position.x, instance.Position.z);
                        if (r.Contains(p))
                        {
                            instance.Position = new Vector3(r.center.x, b.min.y, r.center.y);
                            instance.Rotation = Quaternion.identity;
                            instance.Scale = new Vector3(b.size.x, b.size.y, b.size.z);
                            break;
                        }
                    }
                }
                if (GUILayout.Button(GActionGUI.SNAP_TO_LEVEL_BOUNDS))
                {
                    Bounds b = GCommon.GetLevelBounds();
                    instance.Position = new Vector3(b.center.x, b.min.y, b.center.z);
                    instance.Rotation = Quaternion.identity;
                    instance.Scale = new Vector3(b.size.x, b.size.y, b.size.z);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button(GActionGUI.APPLY))
                {
                    List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(instance.GroupId, instance.GetQuad()));
                    CreateInitialBackup(terrains);
                    ApplyStamp();
                    CreateAfterStampBackup(terrains);
                }
            });
        }

        private void CreateInitialBackup(List<GStylizedTerrain> terrains)
        {
            string historyPrefix =
                instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness ? HISTORY_PREFIX_COLOR :
                instance.Channel == GTextureStampChannel.Splat ? HISTORY_PREFIX_TEXTURE : "Unknown Action";
            List<GTerrainResourceFlag> resourcesFlag = new List<GTerrainResourceFlag>();
            if (instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
            {
                resourcesFlag.Add(GTerrainResourceFlag.AlbedoMap);
                resourcesFlag.Add(GTerrainResourceFlag.MetallicMap);
            }
            else if (instance.Channel == GTextureStampChannel.Splat)
            {
                resourcesFlag.Add(GTerrainResourceFlag.SplatControlMaps);
            }

            GBackupInternal.TryCreateAndMergeInitialBackup(historyPrefix, terrains, resourcesFlag, true);
        }
        private class GStampDialogGUI
        {
            public static readonly string TITLE = "Applying";
            public static readonly string INFO = "Stamping terrain geometry...";
        }

        private void ApplyStamp()
        {
            EditorUtility.DisplayProgressBar(GStampDialogGUI.TITLE, GStampDialogGUI.INFO, 1f);
            try
            {
                instance.Apply();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            EditorUtility.ClearProgressBar();
        }

        private void CreateAfterStampBackup(List<GStylizedTerrain> terrains)
        {
            string historyPrefix =
                instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness ? HISTORY_PREFIX_COLOR :
                instance.Channel == GTextureStampChannel.Splat ? HISTORY_PREFIX_TEXTURE : "Unknown Action";
            List<GTerrainResourceFlag> resourcesFlag = new List<GTerrainResourceFlag>();
            if (instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
            {
                resourcesFlag.Add(GTerrainResourceFlag.AlbedoMap);
                resourcesFlag.Add(GTerrainResourceFlag.MetallicMap);
            }
            else if (instance.Channel == GTextureStampChannel.Splat)
            {
                resourcesFlag.Add(GTerrainResourceFlag.SplatControlMaps);
            }

            GBackupInternal.TryCreateAndMergeBackup(historyPrefix, terrains, resourcesFlag, true);
        }

        private void DuringSceneGUI(SceneView sv)
        {
            EditorGUI.BeginChangeCheck();
            HandleEditingTransform();
            DrawStampBounds();
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }
        }

        private void HandleEditingTransform()
        {
            if (Tools.current == Tool.Move)
            {
                instance.Position = Handles.PositionHandle(instance.Position, instance.Rotation);
            }
            else if (Tools.current == Tool.Rotate)
            {
                instance.Rotation = Handles.RotationHandle(instance.Rotation, instance.Position);
                Vector3 euler = instance.Rotation.eulerAngles;
                euler = new Vector3(0, euler.y, 0);
                instance.Rotation = Quaternion.Euler(euler);
            }
            else if (Tools.current == Tool.Scale)
            {
                instance.Scale = Handles.ScaleHandle(instance.Scale, instance.Position, instance.Rotation, HandleUtility.GetHandleSize(instance.Position));
            }
        }

        private void OnCameraRender(Camera cam)
        {
            if (instance.Editor_ShowLivePreview)
                DrawLivePreview(cam);
            if (instance.EnableTerrainMask)
                DrawTerrainMask(cam);
        }

        private void DrawTerrainMask(Camera cam)
        {
            GCommon.ForEachTerrain(instance.GroupId, (t) =>
            {
                GLivePreviewDrawer.DrawTerrainMask(t, cam);
            });
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            OnCameraRender(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    continue;
                if (instance.GroupId >= 0 &&
                    instance.GroupId != t.GroupId)
                    continue;
                DrawLivePreview(t, cam);
            }
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            instance.GetQuad(worldPoints);
            for (int i = 0; i < normalizedPoints.Length; ++i)
            {
                normalizedPoints[i] = t.WorldPointToUV(worldPoints[i]);
            }

            Rect dirtyRect = GUtilities.GetRectContainsPoints(normalizedPoints);


            if (instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
            {
                int albedoResolution = t.TerrainData.Shading.AlbedoMapResolution;
                FilterMode albedoFilter = t.TerrainData.Shading.AlbedoMapOrDefault.filterMode;

                int metallicResolution = t.TerrainData.Shading.MetallicMapResolution;
                FilterMode metallicFilter = t.TerrainData.Shading.MetallicMapOrDefault.filterMode;

                Texture albedo = GetPreviewTexture(t, "albedo", albedoResolution, albedoFilter);
                Texture metallic = GetPreviewTexture(t, "metallic", metallicResolution, metallicFilter);
                GLivePreviewDrawer.DrawAMSLivePreview(t, cam, albedo, metallic, dirtyRect);
            }
            else if (instance.Channel == GTextureStampChannel.Splat)
            {
                int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
                Texture[] controls = new Texture[t.TerrainData.Shading.SplatControlMapCount];
                for (int i = 0; i < controls.Length; ++i)
                {
                    Texture currentControl = t.TerrainData.Shading.GetSplatControlOrDefault(i);
                    controls[i] = GetPreviewTexture(t, "splatControl" + i, controlMapResolution, currentControl.filterMode);
                }
                GLivePreviewDrawer.DrawSplatLivePreview(t, cam, controls, dirtyRect);
            }
        }

        private void UpdatePreview()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    continue;
                if (instance.GroupId >= 0 &&
                    instance.GroupId != t.GroupId)
                    continue;
                if (instance.Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
                {
                    SetupAlbedoMetallicSmoothnessPreview(t);
                }
                else if (instance.Channel == GTextureStampChannel.Splat)
                {
                    SetupSplatPreview(t);
                }
            }
        }

        private void SetupSplatPreview(GStylizedTerrain t)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            if (controlMapCount == 0)
                return;
            RenderTexture[] rtControls = new RenderTexture[controlMapCount];
            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D splatControl = t.TerrainData.Shading.GetSplatControlOrDefault(i);
                rtControls[i] = GetPreviewTexture(t, "splatControl" + i, controlMapResolution, splatControl.filterMode);
                GCommon.ClearRT(rtControls[i]);
            }
            instance.Internal_ApplySplat(t, rtControls);
            previewPropertyBlock.Clear();
        }

        private void SetupAlbedoMetallicSmoothnessPreview(GStylizedTerrain t)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int albedoResolution = t.TerrainData.Shading.AlbedoMapResolution;
            RenderTexture previewAlbedo = GetPreviewTexture(t, "albedo", albedoResolution, t.TerrainData.Shading.AlbedoMapOrDefault.filterMode);
            GCommon.ClearRT(previewAlbedo);

            int metallicResolution = t.TerrainData.Shading.MetallicMapResolution;
            RenderTexture previewMetallic = GetPreviewTexture(t, "metallic", metallicResolution, t.TerrainData.Shading.MetallicMapOrDefault.filterMode);
            GCommon.ClearRT(previewMetallic);
            instance.Internal_ApplyAlbedoMetallicSmoothness(t, previewAlbedo, previewMetallic);
        }

        private RenderTexture GetPreviewTexture(GStylizedTerrain t, string mapName, int resolution, FilterMode filter)
        {
            if (previewTextures == null)
            {
                previewTextures = new Dictionary<string, RenderTexture>();
            }

            string key = string.Format("{0}_{1}", t.GetInstanceID(), mapName);
            if (!previewTextures.ContainsKey(key) ||
                previewTextures[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                previewTextures[key] = rt;
            }
            else if (previewTextures[key].width != resolution || previewTextures[key].height != resolution)
            {
                previewTextures[key].Release();
                Object.DestroyImmediate(previewTextures[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                previewTextures[key] = rt;
            }

            previewTextures[key].filterMode = filter;
            return previewTextures[key];
        }

        private void DrawStampBounds()
        {
            if (!instance.Editor_ShowBounds)
                return;
            instance.GetBox(worldBox);

            Vector3[] b = worldBox;
            Handles.color = Color.yellow;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawLines(new Vector3[]
            {
                //bottom quad
                b[0], b[1],
                b[1], b[2],
                b[2], b[3],
                b[3], b[0],
                //top quad
                b[4], b[5],
                b[5], b[6],
                b[6], b[7],
                b[7], b[4],
                //vertical lines
                b[0], b[4],
                b[1], b[5],
                b[2], b[6],
                b[3], b[7]
            });
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        }
    }
}
#endif
