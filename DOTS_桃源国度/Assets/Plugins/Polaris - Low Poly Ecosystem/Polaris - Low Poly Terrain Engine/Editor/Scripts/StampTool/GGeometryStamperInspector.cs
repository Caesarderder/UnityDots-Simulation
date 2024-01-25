#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [CustomEditor(typeof(GGeometryStamper))]
    public class GGeometryStamperInspector : Editor
    {
        private GGeometryStamper instance;
        private Dictionary<GStylizedTerrain, RenderTexture> previewTextures;
        private static readonly string HISTORY_PREFIX = "Stamp Geometry";

        private readonly Vector3[] worldPoints = new Vector3[4];
        private readonly Vector3[] worldBox = new Vector3[8];

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            SceneView.duringSceneGui += DuringSceneGUI;
            instance = target as GGeometryStamper;
            Tools.hidden = true;

            instance.Internal_UpdateFalloffTexture();
            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);
            UpdatePreview();
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            Undo.undoRedoPerformed -= OnUndoRedo;
            SceneView.duringSceneGui -= DuringSceneGUI;
            GCommon.UnregisterBeginRender(OnCameraRender);
            GCommon.UnregisterBeginRenderSRP(OnCameraRenderSRP);
            if (previewTextures != null)
            {
                foreach (GStylizedTerrain t in previewTextures.Keys)
                {
                    RenderTexture rt = previewTextures[t];
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
            public static readonly GUIContent ENABLE_TOPOGRAPHIC = new GUIContent("Enable Topographic", "Draw topographic view over the terrain for better sense of altitude");
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

            GEditorSettings.Instance.topographic.enable = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TOPOGRAPHIC, GEditorSettings.Instance.topographic.enable);
            DrawInstructionGUI();
            DrawTransformGUI();
            DrawStampGUI();
            DrawGizmosGUI();
            DrawActionGUI();
#if GRIFFIN_VEGETATION_STUDIO_PRO
            GEditorCommon.DrawVspIntegrationGUI();
#endif
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
            public static readonly string ID = "geo-stamper-instruction";

            public static readonly string INSTRUCTION = "Stamp features onto the terrain surface.";
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
            public static readonly string ID = "geo-stamper-transform";

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
            public static readonly string ID = "geo-stamper-stamp";

            public static readonly GUIContent MASK = new GUIContent("Mask", "A texture defines the geometry feature to stamp, only R channel is used");
            public static readonly GUIContent CHANNEL = new GUIContent("Channel", "Choose whether to stamp to height or visibility data");
            public static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Gradually decrease the mask intensity over its edge");
            public static readonly GUIContent OPERATION = new GUIContent("Operation", "The math operation to perform on the terrain, please see the documentation for detail");
            public static readonly GUIContent LERP_FACTOR = new GUIContent("Lerp Factor", "The interpolation factor");
            public static readonly GUIContent ADDITIONAL_MESH_RESOLUTION = new GUIContent("Additional Mesh Resolution", "Add more polygons to the stamp area");
            public static readonly GUIContent INVERSE = new GUIContent("Inverse the stamp mask");
            public static readonly GUIContent BLEND_USING_FALLOFF = new GUIContent("Blend Using Falloff", "Blend the stamp result with the current terrain using Falloff curve");
        }

        private void DrawStampGUI()
        {
            GEditorCommon.Foldout(GStampGUI.LABEL, true, GStampGUI.ID, () =>
            {
                instance.Stamp = EditorGUILayout.ObjectField(GStampGUI.MASK, instance.Stamp, typeof(Texture2D), false) as Texture2D;
                instance.Channel = (GGeometryStamper.GStampChannel)EditorGUILayout.EnumPopup(GStampGUI.CHANNEL, instance.Channel);
                EditorGUI.BeginChangeCheck();
                instance.Falloff = EditorGUILayout.CurveField(GStampGUI.FALLOFF, instance.Falloff, Color.red, new Rect(0, 0, 1, 1));
                if (EditorGUI.EndChangeCheck())
                {
                    instance.Internal_UpdateFalloffTexture();
                }

                instance.Operation = (GStampOperation)EditorGUILayout.EnumPopup(GStampGUI.OPERATION, instance.Operation);
                if (instance.Operation == GStampOperation.Lerp)
                {
                    instance.LerpFactor = EditorGUILayout.Slider(GStampGUI.LERP_FACTOR, instance.LerpFactor, 0f, 1f);
                }
                instance.AdditionalMeshResolution = EditorGUILayout.IntSlider(GStampGUI.ADDITIONAL_MESH_RESOLUTION, instance.AdditionalMeshResolution, 0, 10);
                instance.InverseStamp = EditorGUILayout.Toggle(GStampGUI.INVERSE, instance.InverseStamp);
                instance.UseFalloffAsBlendFactor = EditorGUILayout.Toggle(GStampGUI.BLEND_USING_FALLOFF, instance.UseFalloffAsBlendFactor);
            });
        }

        private class GGizmosGUI
        {
            public static readonly string LABEL = "Gizmos";
            public static readonly string ID = "geo-stamper-gizmos";

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
            public static readonly string ID = "geo-stamper-action";
            public static readonly GUIContent SNAP_TO_TERRAIN = new GUIContent("Snap To Terrain","Fit the stamper to the underneath terrain");
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
            GBackupInternal.TryCreateAndMergeInitialBackup(HISTORY_PREFIX, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
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
            GBackupInternal.TryCreateAndMergeBackup(HISTORY_PREFIX, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
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
            {
                DrawMask(cam);
            }
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            OnCameraRender(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> overlapTest = GCommon.OverlapTest(instance.GroupId, instance.GetQuad());
            foreach (GOverlapTestResult test in overlapTest)
            {
                if (test.IsOverlapped)
                {
                    DrawLivePreview(test.Terrain, cam, test.IsChunkOverlapped);
                }
            }
        }

        private void DrawMask(Camera cam)
        {
            GCommon.ForEachTerrain(instance.GroupId, (t) =>
            {
                GLivePreviewDrawer.DrawTerrainMask(t, cam);
            });
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            RenderTexture rt = GetPreviewTexture(t);

            if (instance.Channel == GGeometryStamper.GStampChannel.Elevation)
            {
                GLivePreviewDrawer.DrawGeometryLivePreview(t, cam, rt, chunkCulling);
            }
            else if (instance.Channel == GGeometryStamper.GStampChannel.Visibility)
            {
                Matrix4x4 worldToMaskMatrix = Matrix4x4.TRS(
                worldPoints[0],
                instance.Rotation,
                instance.Scale).inverse;
                GLivePreviewDrawer.DrawVisibilityLivePreview(t, cam, rt, chunkCulling, instance.Stamp, worldToMaskMatrix);
            }
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
            RenderTexture rt = GetPreviewTexture(t);
            instance.Internal_DrawOnTexture(t, rt);
        }

        private RenderTexture GetPreviewTexture(GStylizedTerrain t)
        {
            if (previewTextures == null)
            {
                previewTextures = new Dictionary<GStylizedTerrain, RenderTexture>();
            }

            int resolution = t.TerrainData.Geometry.HeightMapResolution;
            if (!previewTextures.ContainsKey(t) ||
                previewTextures[t] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
                previewTextures[t] = rt;
            }
            else if (previewTextures[t].width != resolution ||
                previewTextures[t].height != resolution ||
                previewTextures[t].format != GGeometry.HeightMapRTFormat)
            {
                previewTextures[t].Release();
                Object.DestroyImmediate(previewTextures[t]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
                previewTextures[t] = rt;
            }

            previewTextures[t].wrapMode = TextureWrapMode.Clamp;

            return previewTextures[t];
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
