#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Type = System.Type;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GSplineCreator))]
    public class GSplineCreatorInspector : Editor
    {
        private List<Type> modifierTypes;
        public List<Type> ModifierTypes
        {
            get
            {
                if (modifierTypes == null)
                {
                    modifierTypes = new List<Type>();
                }
                return modifierTypes;
            }
            set
            {
                modifierTypes = value;
            }
        }

        private GSplineCreator instance;
        private int selectedAnchorIndex = -1;
        private int selectedSegmentIndex = -1;

        private Rect addModifierButtonRect;

        private GSplineEditingGUIDrawer splineEditingDrawer;

        private void OnEnable()
        {
            instance = (GSplineCreator)target;
            InitModifierClasses();
            Tools.hidden = true;
            splineEditingDrawer = new GSplineEditingGUIDrawer(instance);

            GCommon.RegisterBeginRender(OnBeginRender);
            GCommon.RegisterBeginRenderSRP(OnBeginRenderSRP);
            SceneView.duringSceneGui += DuringSceneGUI;

            GLayerInitializer.SetupSplineLayer();
        }

        private void OnDisable()
        {
            Tools.hidden = false;

            GCommon.UnregisterBeginRender(OnBeginRender);
            GCommon.UnregisterBeginRenderSRP(OnBeginRenderSRP);
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void InitModifierClasses()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            ModifierTypes = loadedTypes.FindAll(
                t => t.IsSubclassOf(typeof(GSplineModifier)));
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private class GBaseGUI
        {
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the terrain group this tool will work on");
            public static readonly GUIContent RAYCAST_LAYER = new GUIContent("Raycast Layers", "Game object layers to perform raycast when adding anchors");
            public static readonly GUIContent RAYCAST_LAYER_WARNING = new GUIContent("You can't edit the spline with Raycast Layers set to Nothing");
            public static readonly GUIContent AUTO_TANGENT = new GUIContent("Auto Tangent", "Smooth the spline automatically");
            public static readonly GUIContent ENABLE_TERRAIN_MASK = new GUIContent("Enable Terrain Mask", "Use the terrain Mask texture (R) to lock particular regions from being edited");
            public static readonly GUIContent ENABLE_TOPOGRAPHIC = new GUIContent("Enable Topographic", "Draw an overlay topographic view for better sense of elevation");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUI.enabled = !GEditorSettings.Instance.splineTools.showTransformGizmos;
            instance.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GBaseGUI.GROUP_ID, instance.GroupId);

            GEditorSettings.Instance.splineTools.raycastLayers = GEditorCommon.LayerMaskField(GBaseGUI.RAYCAST_LAYER, GEditorSettings.Instance.splineTools.raycastLayers);
            if (GEditorSettings.Instance.splineTools.raycastLayers == 0)
            {
                EditorGUILayout.LabelField(GBaseGUI.RAYCAST_LAYER_WARNING, GEditorCommon.WarningLabel);
            }

            GEditorSettings.Instance.splineTools.autoTangent = EditorGUILayout.Toggle(GBaseGUI.AUTO_TANGENT, GEditorSettings.Instance.splineTools.autoTangent);

            instance.EnableTerrainMask = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TERRAIN_MASK, instance.EnableTerrainMask);
            GEditorSettings.Instance.topographic.enable = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TOPOGRAPHIC, GEditorSettings.Instance.topographic.enable);

            DrawInstructionGUI();
            GUI.enabled = true;
            DrawTransformGUI();
            GUI.enabled = !GEditorSettings.Instance.splineTools.showTransformGizmos;
            DrawAnchorDefaultValueGUI();
            DrawSelectedAnchorGUI();
            DrawSegmentDefaultValueGUI();
            DrawSelectedSegmentGUI();
            //DrawGizmosGUI();
            DrawModifiersGUI();
            GEditorCommon.DrawBackupHelpBox();
            //DrawDebugGUI();
            if (EditorGUI.EndChangeCheck())
            {
                GSplineCreator.MarkSplineChanged(instance);
                GUtilities.MarkCurrentSceneDirty();
            }
        }

        private class GInstructionGUI
        {
            public static readonly string LABEL = "Instruction";
            public static readonly string ID = "spline-creator-instruction";

            public static readonly GUIContent INSTRUCTION = new GUIContent(
                string.Format(
                    "Create a edit bezier spline.\n" +
                    "   - Left Click to select element.\n" +
                    "   - Ctrl & Left Click to delete element.\n" +
                    "   - Shift & Left Click to add element.\n" +
                    "Use Add Modifier to do specific tasks with spline data."));
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
            public static readonly string ID = "spline-creator-transform";

            public static readonly GUIContent SET_PIVOT_TO_MEDIAN_POINT = new GUIContent("Set Pivot To Median Point");
            public static readonly GUIContent POSITION = new GUIContent("Position", "Position of the Spline object");
            public static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the Spline object");
            public static readonly GUIContent SCALE = new GUIContent("Scale", "Scale of the Spline object");
            public static readonly GUIContent SHOW_TRANSFORM_GIZMOS = new GUIContent("Show Transform Gizmos", "Show gizmos to move, rotate and scale the Spline object");
        }

        private void DrawTransformGUI()
        {
            if (GEditorSettings.Instance.splineTools.showTransformGizmos)
            {
                GEditorCommon.ExpandFoldout(GTransformGUI.ID);
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GTransformGUI.SET_PIVOT_TO_MEDIAN_POINT,
                false,
                () => { SetPivotToMedianPoint(); });

            GEditorCommon.Foldout(GTransformGUI.LABEL, true, GTransformGUI.ID, () =>
            {
                instance.transform.localPosition = GEditorCommon.InlineVector3Field(GTransformGUI.POSITION, instance.transform.localPosition);
                instance.transform.localRotation = GEditorCommon.InlineEulerRotationField(GTransformGUI.ROTATION, instance.transform.localRotation);
                instance.transform.localScale = GEditorCommon.InlineVector3Field(GTransformGUI.SCALE, instance.transform.localScale);

                GEditorSettings.Instance.splineTools.showTransformGizmos = EditorGUILayout.Toggle(GTransformGUI.SHOW_TRANSFORM_GIZMOS, GEditorSettings.Instance.splineTools.showTransformGizmos);
            },
            menu);
        }

        private class GAnchorDefaultGUI
        {
            public static readonly string LABEL = "Anchor Defaults";
            public static readonly string ID = "spline-creator-anchor-defaults";

            public static readonly GUIContent POSITION_OFFSET = new GUIContent("Position Offset", "Offset from the raycast point of the anchor");
            public static readonly GUIContent INITIAL_ROTATION = new GUIContent("Rotation", "Initial rotation of the new anchor");
            public static readonly GUIContent INITIAL_SCALE = new GUIContent("Scale", "Initial scale of the new anchor");
        }

        private void DrawAnchorDefaultValueGUI()
        {
            GEditorCommon.Foldout(GAnchorDefaultGUI.LABEL, true, GAnchorDefaultGUI.ID, () =>
            {
                instance.PositionOffset = GEditorCommon.InlineVector3Field(GAnchorDefaultGUI.POSITION_OFFSET, instance.PositionOffset);
                instance.InitialRotation = GEditorCommon.InlineEulerRotationField(GAnchorDefaultGUI.INITIAL_ROTATION, instance.InitialRotation);
                instance.InitialScale = GEditorCommon.InlineVector3Field(GAnchorDefaultGUI.INITIAL_SCALE, instance.InitialScale);
            });
        }

        private class GSelectedAnchorGUI
        {
            public static readonly string LABEL = "Selected Anchor";
            public static readonly string ID = "spline-creator-selected-anchor";

            public static readonly GUIContent POSITION = new GUIContent("Position", "Position of the anchor");
            public static readonly GUIContent ROTATION = new GUIContent("Rotation", "Rotation of the anchor, you can't change this if Auto Tangent is enable");
            public static readonly GUIContent SCALE = new GUIContent("Scale", "Scale of the anchor");

            public static readonly GUIContent NO_ANCHOR_SELECTED = new GUIContent("No Anchor selected!");
        }

        private void DrawSelectedAnchorGUI()
        {
            GEditorCommon.Foldout(GSelectedAnchorGUI.LABEL, true, GSelectedAnchorGUI.ID, () =>
            {
                selectedAnchorIndex = splineEditingDrawer.selectedAnchorIndex;
                if (selectedAnchorIndex >= 0 && selectedAnchorIndex < instance.Spline.Anchors.Count)
                {
                    GSplineAnchor a = instance.Spline.Anchors[selectedAnchorIndex];
                    EditorGUI.BeginChangeCheck();
                    a.Position = GEditorCommon.InlineVector3Field(GSelectedAnchorGUI.POSITION, a.Position);
                    GUI.enabled = !GEditorSettings.Instance.splineTools.autoTangent;
                    a.Rotation = GEditorCommon.InlineEulerRotationField(GSelectedAnchorGUI.ROTATION, a.Rotation);
                    GUI.enabled = true;
                    a.Scale = GEditorCommon.InlineVector3Field(GSelectedAnchorGUI.SCALE, a.Scale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (GEditorSettings.Instance.splineTools.autoTangent)
                        {
                            instance.Spline.SmoothTangents(selectedAnchorIndex);
                        }
                        List<int> segmentIndices = instance.Spline.FindSegments(selectedAnchorIndex);
                        instance.UpdateMesh(segmentIndices);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(GSelectedAnchorGUI.NO_ANCHOR_SELECTED, GEditorCommon.ItalicLabel);
                }
            });
        }

        private class GSegmentDefaultGUI
        {
            public static readonly string LABEL = "Segment Defaults";
            public static readonly string ID = "spline-creator-segment-defaults";

            public static readonly GUIContent SMOOTHNESS = new GUIContent("Smoothness", "Spline subdivision factor, larger number produces smoother spline");
            public static readonly GUIContent WIDTH = new GUIContent("Width", "Width of the inner part of the spline body where there is no falloff");
            public static readonly GUIContent FALLOFF_WIDTH = new GUIContent("Falloff Width", "Width of the outer part of the spline body where falloff takes into account");
        }

        private void DrawSegmentDefaultValueGUI()
        {
            GEditorCommon.Foldout(GSegmentDefaultGUI.LABEL, true, GSegmentDefaultGUI.ID, () =>
            {
                EditorGUIUtility.wideMode = true;
                EditorGUI.BeginChangeCheck();
                instance.Smoothness = EditorGUILayout.IntField(GSegmentDefaultGUI.SMOOTHNESS, instance.Smoothness);
                instance.Width = EditorGUILayout.FloatField(GSegmentDefaultGUI.WIDTH, instance.Width);
                instance.FalloffWidth = EditorGUILayout.FloatField(GSegmentDefaultGUI.FALLOFF_WIDTH, instance.FalloffWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    instance.UpdateMeshes();
                }
                EditorGUIUtility.wideMode = false;
            });
        }

        private class GSelectedSegmentGUI
        {
            public static readonly string LABEL = "Selected Segment";
            public static readonly string ID = "spline-creator-selected-segment";

            public static readonly GUIContent START_TANGENT = new GUIContent("Start Tangent", "Position of the first tangent, you can't change this if Auto Tangent is on");
            public static readonly GUIContent END_TANGENT = new GUIContent("End Tangent", "Position of the second tangent, you can't change this if Auto Tangent is on");
            public static readonly GUIContent FLIP_DIRECTION_BTN = new GUIContent("Flip Direction", "Flip the direction of this segment");
            public static readonly GUIContent NO_SEGMENT_SELECTED = new GUIContent("No Segment selected!");
        }

        private void DrawSelectedSegmentGUI()
        {
            GEditorCommon.Foldout(GSelectedSegmentGUI.LABEL, true, GSelectedSegmentGUI.ID, () =>
            {
                selectedSegmentIndex = splineEditingDrawer.selectedSegmentIndex;
                if (selectedSegmentIndex >= 0 && selectedSegmentIndex < instance.Spline.Segments.Count)
                {
                    GSplineSegment s = instance.Spline.Segments[selectedSegmentIndex];
                    GUI.enabled = !GEditorSettings.Instance.splineTools.autoTangent;
                    s.StartTangent = GEditorCommon.InlineVector3Field(GSelectedSegmentGUI.START_TANGENT, s.StartTangent);
                    s.EndTangent = GEditorCommon.InlineVector3Field(GSelectedSegmentGUI.END_TANGENT, s.EndTangent);
                    GUI.enabled = true;
                    if (GUILayout.Button(GSelectedSegmentGUI.FLIP_DIRECTION_BTN))
                    {
                        s.FlipDirection();
                        if (GEditorSettings.Instance.splineTools.autoTangent)
                        {
                            instance.Spline.SmoothTangents(s.StartIndex, s.EndIndex);
                        }
                        instance.UpdateMesh(selectedSegmentIndex);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(GSelectedSegmentGUI.NO_SEGMENT_SELECTED, GEditorCommon.ItalicLabel);
                }
            });
        }

        private class GGizmosGUI
        {

        }

        private void DrawGizmosGUI()
        {
            string label = "Gizmos";
            string id = "gizmos" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {

                EditorUtility.SetDirty(GEditorSettings.Instance);
            });
        }

        private class GModifiersGUI
        {
            public static readonly string LABEL = "Modifiers";
            public static readonly string ID = "spline-creator-modifiers";

            public static readonly GUIContent ADD_MODIFIER_BTN = new GUIContent("Add Modifier");
        }

        private void DrawModifiersGUI()
        {
            GEditorCommon.Foldout(GModifiersGUI.LABEL, true, GModifiersGUI.ID, () =>
            {
                if (ModifierTypes.Count == 0)
                    return;
                Rect r = EditorGUILayout.GetControlRect();
                if (Event.current.type == EventType.Repaint)
                    addModifierButtonRect = r;
                if (GUI.Button(r, GModifiersGUI.ADD_MODIFIER_BTN))
                {
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < ModifierTypes.Count; ++i)
                    {
                        Type t = ModifierTypes[i];
                        string menuLabel = string.Empty;
                        object[] alternativeClassNameAttributes = t.GetCustomAttributes(typeof(GDisplayName), false);
                        if (alternativeClassNameAttributes != null && alternativeClassNameAttributes.Length > 0)
                        {
                            GDisplayName att = alternativeClassNameAttributes[0] as GDisplayName;
                            if (att.DisplayName == null ||
                                att.DisplayName.Equals(string.Empty))
                                menuLabel = t.Name;
                            else
                                menuLabel = att.DisplayName;
                        }
                        else
                        {
                            menuLabel = t.Name;
                        }

                        menu.AddItem(
                            new GUIContent(ObjectNames.NicifyVariableName(menuLabel)),
                            false,
                            () =>
                            {
                                AddModifier(t);
                            });
                    }
                    menu.DropDown(addModifierButtonRect);
                }
            });
        }

        private void AddModifier(Type t)
        {
            GSplineModifier modifier = instance.gameObject.AddComponent(t) as GSplineModifier;
            modifier.SplineCreator = instance;
        }

        private void DuringSceneGUI(SceneView sv)
        {
            if (!instance.isActiveAndEnabled)
                return;
            EditorGUI.BeginChangeCheck();
            splineEditingDrawer.Draw();
            if (EditorGUI.EndChangeCheck())
            {
                GSplineCreator.MarkSplineChanged(instance);
                GUtilities.MarkCurrentSceneDirty();
            }
        }

        private void SetPivotToMedianPoint()
        {
            Vector3 localMedian = Vector3.zero;
            List<GSplineAnchor> anchors = instance.Spline.Anchors;
            if (anchors.Count == 0)
                return;

            for (int i = 0; i < anchors.Count; ++i)
            {
                localMedian += anchors[i].Position;
            }
            localMedian = localMedian / anchors.Count;

            Vector3 worldMedian = instance.transform.TransformPoint(localMedian);
            Matrix4x4 medianToLocal = Matrix4x4.TRS(
                worldMedian,
                instance.transform.rotation,
                instance.transform.lossyScale).inverse;
            Matrix4x4 localToWorld = instance.transform.localToWorldMatrix;
            Matrix4x4 transformationMatrix = medianToLocal * localToWorld;

            for (int i = 0; i < anchors.Count; ++i)
            {
                anchors[i].Position = transformationMatrix.MultiplyPoint(anchors[i].Position);
            }

            List<GSplineSegment> segments = instance.Spline.Segments;
            for (int i = 0; i < segments.Count; ++i)
            {
                segments[i].StartTangent = transformationMatrix.MultiplyPoint(segments[i].StartTangent);
                segments[i].EndTangent = transformationMatrix.MultiplyPoint(segments[i].EndTangent);
            }

            instance.transform.position = worldMedian;
            instance.UpdateMeshes();
            GSplineCreator.MarkSplineChanged(instance);
        }

        private void OnBeginRender(Camera cam)
        {
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (instance.EnableTerrainMask)
            {
                DrawTerrainMask(cam);
            }
        }

        private void OnBeginRenderSRP(ScriptableRenderContext context, Camera cam)
        {
            OnBeginRender(cam);
        }

        private void DrawTerrainMask(Camera cam)
        {
            GCommon.ForEachTerrain(instance.GroupId, (t) =>
            {
                GLivePreviewDrawer.DrawTerrainMask(t, cam);
            });
        }
    }
}
#endif
