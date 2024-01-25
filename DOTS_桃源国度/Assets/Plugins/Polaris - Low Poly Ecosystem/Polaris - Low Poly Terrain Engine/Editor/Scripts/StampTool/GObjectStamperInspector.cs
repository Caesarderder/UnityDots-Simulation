#if GRIFFIN
using Pinwheel.Griffin.TextureTool;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [CustomEditor(typeof(GObjectStamper))]
    public class GObjectStamperInspector : Editor
    {
        private GObjectStamper instance;
        private Dictionary<string, RenderTexture> previewTextures;

        private readonly Vector3[] worldBox = new Vector3[8];


        private void OnEnable()
        {
            instance = target as GObjectStamper;
            Tools.hidden = true;

            instance.Internal_UpdateFalloffTexture();
            instance.Internal_UpdateLayerTransitionTextures();
            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);
            SceneView.duringSceneGui += DuringSceneGUI;
            UpdatePreview();
        }

        private void OnDisable()
        {
            Tools.hidden = false;

            GCommon.UnregisterBeginRender(OnCameraRender);
            GCommon.UnregisterBeginRenderSRP(OnCameraRenderSRP);
            SceneView.duringSceneGui -= DuringSceneGUI;

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
            public static readonly string ID = "object-stamper-instruction";

            public static readonly string INSTRUCTION = "Stamp objects onto the terrain surface.";
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
            public static readonly string ID = "object-stamper-transform";

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
            public static readonly string ID = "object-stamper-stamp";

            public static readonly GUIContent MASK = new GUIContent("Mask", "A texture defines the spawn probability on the region");
            public static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Gradually decrease the mask intensity over its edge");
            public static readonly GUIContent MASK_RESOLUTION = new GUIContent("Mask Resolution", "Size of the combined mask for instance sampling");
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
                instance.MaskResolution = EditorGUILayout.DelayedIntField(GStampGUI.MASK_RESOLUTION, instance.MaskResolution);
            });
        }

        private class GLayerGUI
        {
            public static readonly string LABEL = "Layers";
            public static readonly string ID = "object-stamper-layer";
            public static readonly string LAYER_ID_PREFIX = "foliage-stamp-layer";

            public static readonly GUIContent ADD_LAYER = new GUIContent("Add Layer");

            public static readonly GUIContent MOVE_LAYER_UP = new GUIContent("Move Up");
            public static readonly GUIContent MOVE_LAYER_DOWN = new GUIContent("Move Down");
            public static readonly GUIContent REMOVE = new GUIContent("Remove");

            public static readonly string HEADER_GENERAL = "General";
            public static readonly GUIContent IGNORE = new GUIContent("Ignore", "Ignore this layer");
            public static readonly GUIContent NAME = new GUIContent("Name", "Display name of this layer");
            public static readonly GUIContent VISUALIZE_COLOR = new GUIContent("Visualize Color", "Color to draw a visualize mask in the scene view");

            public static readonly string HEADER_ROTATION_SCALE = "Rotation & Scale";
            public static readonly GUIContent MIN_ROTATION = new GUIContent("Min Rotation", "Minimum rotation of the spawned instances");
            public static readonly GUIContent MAX_ROTATION = new GUIContent("Max Rotation", "Maximum rotation of the spawned instances");
            public static readonly GUIContent MIN_SCALE = new GUIContent("Min Scale", "Minimum scale of the spawned instances");
            public static readonly GUIContent MAX_SCALE = new GUIContent("Max Scale", "Maximum scale of the spawned instances");

            public static readonly string HEADER_OBJECT = "Objects";
            public static readonly GUIContent INSTANCE_COUNT = new GUIContent("Instance Count Per Terrain", "Number of instance to spawn on each terrain. Note that final instance count might be different depends on mask intensity");

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

            public static readonly GUIContent ALIGN_TO_SURFACE = new GUIContent("Align To Surface", "Snap the spawned objects to surface normal vector");
            public static readonly GUIContent WORLD_RAYCAST_MASK = new GUIContent("World Raycast Mask", "Define the layers to perform raycast and snap object position");
        }

        private void DrawStampLayersGUI()
        {
            GEditorCommon.Foldout(GLayerGUI.LABEL, true, GLayerGUI.ID, () =>
            {
                List<GObjectStampLayer> layers = instance.Layers;
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
                    GObjectStampLayer layer = GObjectStampLayer.Create();
                    layers.Add(layer);
                }
            });
        }

        private void DrawLayer(GObjectStampLayer layer, int index)
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
                layer.VisualizeColor = EditorGUILayout.ColorField(GLayerGUI.VISUALIZE_COLOR, layer.VisualizeColor);

                GEditorCommon.Header(GLayerGUI.HEADER_ROTATION_SCALE);
                layer.MinRotation = EditorGUILayout.FloatField(GLayerGUI.MIN_ROTATION, layer.MinRotation);
                layer.MaxRotation = EditorGUILayout.FloatField(GLayerGUI.MAX_ROTATION, layer.MaxRotation);
                layer.MinScale = GEditorCommon.InlineVector3Field(GLayerGUI.MIN_SCALE, layer.MinScale);
                layer.MaxScale = GEditorCommon.InlineVector3Field(GLayerGUI.MAX_SCALE, layer.MaxScale);

                GEditorCommon.Header(GLayerGUI.HEADER_OBJECT);
                DrawObjectSelectorGUI(layer);
                layer.InstanceCount = EditorGUILayout.IntField(GLayerGUI.INSTANCE_COUNT, layer.InstanceCount);
                layer.WorldRaycastMask = GEditorCommon.LayerMaskField(GLayerGUI.WORLD_RAYCAST_MASK, layer.WorldRaycastMask);
                layer.AlignToSurface = EditorGUILayout.Toggle(GLayerGUI.ALIGN_TO_SURFACE, layer.AlignToSurface);

                GEditorCommon.Header(GLayerGUI.HEADER_HEIGHT_RULE);
                layer.BlendHeight = EditorGUILayout.Toggle(GLayerGUI.BLEND_HEIGHT, layer.BlendHeight);
                if (layer.BlendHeight)
                {
                    layer.MinHeight = EditorGUILayout.FloatField(GLayerGUI.MIN_HEIGHT, layer.MinHeight);
                    layer.MaxHeight = EditorGUILayout.FloatField(GLayerGUI.MAX_HEIGHT, layer.MaxHeight);
                    EditorGUI.BeginChangeCheck();
                    layer.HeightTransition = EditorGUILayout.CurveField(GLayerGUI.HEIGHT_TRANSITION, layer.HeightTransition, Color.red, new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        layer.UpdateCurveTextures();
                    }
                }

                GEditorCommon.Header(GLayerGUI.HEADER_SLOPE_RULE);
                layer.BlendSlope = EditorGUILayout.Toggle(GLayerGUI.BLEND_SLOPE, layer.BlendSlope);
                if (layer.BlendSlope)
                {
                    layer.NormalMapMode = (GNormalMapMode)EditorGUILayout.EnumPopup(GLayerGUI.NORMAL_MAP_MODE, layer.NormalMapMode);
                    layer.MinSlope = EditorGUILayout.FloatField(GLayerGUI.MIN_SLOPE, layer.MinSlope);
                    layer.MaxSlope = EditorGUILayout.FloatField(GLayerGUI.MAX_SLOPE, layer.MaxSlope);
                    EditorGUI.BeginChangeCheck();
                    layer.SlopeTransition = EditorGUILayout.CurveField(GLayerGUI.SLOPE_TRANSITION, layer.SlopeTransition, Color.red, new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        layer.UpdateCurveTextures();
                    }
                }

                GEditorCommon.Header(GLayerGUI.HEADER_NOISE_RULE);
                layer.BlendNoise = EditorGUILayout.Toggle(GLayerGUI.BLEND_NOISE, layer.BlendNoise);
                if (layer.BlendNoise)
                {
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
                }
                EditorGUI.indentLevel += 1;
            },
            menu);
        }
        private class GObjectSelectionGUI
        {
            public static readonly string NO_OBJECT_FOUND = "No Game Object found!";
            public static readonly string OBJECT_DROP_ZONE_MESSAGE_0 = "Drop a Game Object here!";
            public static readonly string OBJECT_DROP_ZONE_FILTER_0 = "t:GameObject";
            public static readonly string OBJECT_DROP_ZONE_MESSAGE_1 = "Drop a Prefab Prototype Group here!";
            public static readonly string OBJECT_DROP_ZONE_FILTER_1 = "t:GPrefabPrototypeGroup";

            public static readonly GUIContent REMOVE = new GUIContent("Remove");
        }

        private void DrawObjectSelectorGUI(GObjectStampLayer layer)
        {
            if (layer.Prototypes.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = layer.Prototypes;
                args.selectedIndices = layer.PrototypeIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
                args.itemPerRow = 4;
                args.drawPreviewFunction = GEditorCommon.DrawGameObjectPreview;
                args.contextClickFunction = OnObjectSelectorContextClick;
                layer.PrototypeIndices = GEditorCommon.MultiSelectionGrid(args);
            }
            else
            {
                EditorGUILayout.LabelField(GObjectSelectionGUI.NO_OBJECT_FOUND, GEditorCommon.WordWrapItalicLabel);
            }

            Rect r1 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GameObject prefab = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r1, GObjectSelectionGUI.OBJECT_DROP_ZONE_MESSAGE_0, GObjectSelectionGUI.OBJECT_DROP_ZONE_FILTER_0);
            if (prefab != null)
            {
                layer.Prototypes.AddIfNotContains(prefab);
            }

            GEditorCommon.SpacePixel(0);
            Rect r2 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GPrefabPrototypeGroup group = GEditorCommon.ObjectSelectorDragDrop<GPrefabPrototypeGroup>(r2, GObjectSelectionGUI.OBJECT_DROP_ZONE_MESSAGE_1, GObjectSelectionGUI.OBJECT_DROP_ZONE_FILTER_1);
            if (group != null)
            {
                layer.Prototypes.AddIfNotContains(group.Prototypes);
            }
        }

        private void OnObjectSelectorContextClick(Rect r, int index, ICollection collection)
        {
            List<GameObject> prototypes = (List<GameObject>)collection;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GObjectSelectionGUI.REMOVE,
                false,
                () =>
                {
                    prototypes.RemoveAt(index);
                });

            menu.ShowAsContext();
        }

        private class GRemoveLayerDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";
        }

        private void ConfirmAndRemoveLayerAt(int index)
        {
            GObjectStampLayer layer = instance.Layers[index];
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
            public static readonly string ID = "object-stamper-gizmos";

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
            public static readonly string ID = "object-stamper-action";
            public static readonly GUIContent SNAP_TO_TERRAIN = new GUIContent("Snap To Terrain", "Fit the stamper to the underneath terrain");
            public static readonly GUIContent SNAP_TO_LEVEL_BOUNDS = new GUIContent("Snap To Level Bounds", "Fit the stamper to cover all terrains in the level");
            public static readonly GUIContent CLEAR_OBJECT = new GUIContent("Clear Objects", "Remove all object instances inside the stamper bounding box");
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

                if (GUILayout.Button(GActionGUI.CLEAR_OBJECT))
                {
                    instance.ClearObjects();
                    Event.current.Use();
                }

                if (GUILayout.Button(GActionGUI.APPLY))
                {
                    ApplyStamp();
                    EditorGUIUtility.ExitGUI();
                }
            });
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
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.stampTools.showLivePreview)
                DrawLivePreview(cam);
            if (instance.EnableTerrainMask && GEditorSettings.Instance.stampTools.showTerrainMask)
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
            List<GOverlapTestResult> overlapTests = GCommon.OverlapTest(instance.GroupId, instance.GetQuad());
            foreach (GOverlapTestResult test in overlapTests)
            {
                if (test.IsOverlapped)
                {
                    DrawLivePreview(test.Terrain, cam, test.IsChunkOverlapped);
                }
            }
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;
            RenderTexture[] brushes = new RenderTexture[instance.Layers.Count];
            for (int i = 0; i < brushes.Length; ++i)
            {
                brushes[i] = GetPreviewTexture(t, "brush" + i.ToString(), instance.MaskResolution, FilterMode.Bilinear);
            }

            Color[] colors = new Color[instance.Layers.Count];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = instance.Layers[i].VisualizeColor;
            }

            GLivePreviewDrawer.DrawMasksLivePreview(t, cam, brushes, colors, chunkCulling);
        }

        private void UpdatePreview()
        {
            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GCommon.OverlapTest(instance.GroupId, instance.GetQuad()));
            foreach (GStylizedTerrain t in terrains)
            {
                UpdatePreview(t);
            }
        }

        private void UpdatePreview(GStylizedTerrain t)
        {
            RenderTexture[] brushes = new RenderTexture[instance.Layers.Count];
            for (int i = 0; i < brushes.Length; ++i)
            {
                brushes[i] = GetPreviewTexture(t, "brush" + i.ToString(), instance.MaskResolution, FilterMode.Bilinear);
            }

            Vector3[] worldPoints = instance.GetQuad();
            Vector2[] uvPoint = new Vector2[worldPoints.Length];
            for (int i = 0; i < uvPoint.Length; ++i)
            {
                uvPoint[i] = t.WorldPointToUV(worldPoints[i]);
            }
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvPoint);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return;
            instance.Internal_RenderBrushes(brushes, t, uvPoint);
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
            if (!GEditorSettings.Instance.stampTools.showBounds)
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
