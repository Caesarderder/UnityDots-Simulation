#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GPathPainter))]
    public class GPathPainterInspector : Editor
    {
        private GPathPainter instance;
        private Dictionary<string, RenderTexture> previewTextures;
        private bool isUrp;
        private ScriptableRenderContext urpContext;

        private static readonly string HISTORY_PREFIX_ALBEDO_METALLIC = "Make Path Albedo Metallic";
        private static readonly string HISTORY_PREFIX_SPLAT = "Make Path Splat";

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            instance = (GPathPainter)target;
            instance.Internal_UpdateFalloffTexture();
            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);
            GCommon.UpdateMaterials(instance.SplineCreator.GroupId);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
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
        }

        public override void OnInspectorGUI()
        {
            DrawBaseGUI();
            if (instance.SplineCreator == null)
                return;

            DrawFalloffGUI();
            DrawPathGUI();
            DrawActionGUI();
        }

        private class GBaseGUI
        {
            public static readonly GUIContent SPLINE_CREATOR = new GUIContent("Spline Creator", "The Spline Creator component this modifier belongs to");
            public static readonly GUIContent LIVE_PREVIEW = new GUIContent("Live Preview", "Draw a preview over the terrain");
        }

        private void DrawBaseGUI()
        {
            instance.SplineCreator = EditorGUILayout.ObjectField(GBaseGUI.SPLINE_CREATOR, instance.SplineCreator, typeof(GSplineCreator), true) as GSplineCreator;
            EditorGUI.BeginChangeCheck();
            GEditorSettings.Instance.splineTools.livePreview.pathPainter = EditorGUILayout.Toggle(GBaseGUI.LIVE_PREVIEW, GEditorSettings.Instance.splineTools.livePreview.pathPainter);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        private class GFalloffGUI
        {
            public static readonly string LABEL = "Falloff";
            public static readonly string ID = "spline-path-painter-falloff";

            public static readonly GUIContent FALL_OFF = new GUIContent("Falloff", "Falloff factor of the spline effect, from the center to the edge");
            public static readonly GUIContent FALL_OFF_NOISE = new GUIContent("Falloff Noise", "Noise map in world space to blend with the falloff curve");
            public static readonly GUIContent FALL_OFF_NOISE_SIZE = new GUIContent("Falloff Noise Size", "Size of the falloff noise in world space");
        }

        private void DrawFalloffGUI()
        {
            GEditorCommon.Foldout(GFalloffGUI.LABEL, false, GFalloffGUI.ID, () =>
            {
                EditorGUI.BeginChangeCheck();
                instance.Falloff = EditorGUILayout.CurveField(GFalloffGUI.FALL_OFF, instance.Falloff, Color.red, new Rect(0, 0, 1, 1));
                if (EditorGUI.EndChangeCheck())
                {
                    instance.Internal_UpdateFalloffTexture();
                }
                instance.FalloffNoise = EditorGUILayout.ObjectField(GFalloffGUI.FALL_OFF_NOISE, instance.FalloffNoise, typeof(Texture2D), false) as Texture2D;
                if (instance.FalloffNoise != null)
                    instance.FalloffNoiseSize = GEditorCommon.InlineVector2Field(GFalloffGUI.FALL_OFF_NOISE_SIZE, instance.FalloffNoiseSize);
            });
        }

        private class GPathGUI
        {
            public static readonly string LABEL = "Path";
            public static readonly string ID = "spline-path-painter-path";

            public static readonly GUIContent CHANNEL = new GUIContent("Channel", "The target texture to paint the path on");
            public static readonly GUIContent COLOR = new GUIContent("Color", "Color of the path");
            public static readonly GUIContent METALLIC = new GUIContent("Metallic", "Metallic value of the path");
            public static readonly GUIContent SMOOTHNESS = new GUIContent("Smoothness", "Smoothness value of the path");
            public static readonly GUIContent AMS_NOTE = new GUIContent("Use a material that utilizes Albedo & Metallic map to see the result!");
            public static readonly GUIContent SPLAT_PROTOTYPE = new GUIContent("Prototype");
            public static readonly GUIContent SPLAT_NOTE = new GUIContent("Use a material that utilizes Splat map to see the result!");
        }

        private void DrawPathGUI()
        {
            GEditorCommon.Foldout(GPathGUI.LABEL, false, GPathGUI.ID, () =>
            {
                instance.Channel = (GPathPainter.PaintChannel)EditorGUILayout.EnumPopup(GPathGUI.CHANNEL, instance.Channel);

                if (instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic)
                {
                    instance.Color = EditorGUILayout.ColorField(GPathGUI.COLOR, instance.Color);
                    instance.Metallic = EditorGUILayout.Slider(GPathGUI.METALLIC, instance.Metallic, 0f, 1f);
                    instance.Smoothness = EditorGUILayout.Slider(GPathGUI.SMOOTHNESS, instance.Smoothness, 0f, 1f);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(GPathGUI.AMS_NOTE, GEditorCommon.WordWrapItalicLabel);
                }
                else if (instance.Channel == GPathPainter.PaintChannel.Splat)
                {
                    EditorGUILayout.LabelField(GPathGUI.SPLAT_PROTOTYPE);
                    instance.SplatIndex = GEditorCommon.SplatSetSelectionGrid(instance.SplineCreator.GroupId, instance.SplatIndex);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(GPathGUI.SPLAT_NOTE, GEditorCommon.WordWrapItalicLabel);
                }
            });
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "spline-path-painter-action";

            public static readonly GUIContent APPLY_BTN = new GUIContent("Apply");
        }

        private void DrawActionGUI()
        {
            GEditorCommon.ExpandFoldout(GActionGUI.ID);
            GEditorCommon.Foldout(GActionGUI.LABEL, true, GActionGUI.ID, () =>
            {
                if (GUILayout.Button(GActionGUI.APPLY_BTN))
                {
                    List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(instance.SplineCreator.SweepTest());
                    CreateInitialBackup(terrains);
                    ApplyPath();
                    CreateBackupAfterApplyPath(terrains);
                }
            });

        }

        private void CreateInitialBackup(List<GStylizedTerrain> terrains)
        {
            string historyPrefix =
                instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic ? HISTORY_PREFIX_ALBEDO_METALLIC :
                instance.Channel == GPathPainter.PaintChannel.Splat ? HISTORY_PREFIX_SPLAT : "Unknown Action";
            List<GTerrainResourceFlag> resourceFlag = new List<GTerrainResourceFlag>();
            if (instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic)
            {
                resourceFlag.Add(GTerrainResourceFlag.AlbedoMap);
                resourceFlag.Add(GTerrainResourceFlag.MetallicMap);
            }
            else if (instance.Channel == GPathPainter.PaintChannel.Splat)
            {
                resourceFlag.Add(GTerrainResourceFlag.SplatControlMaps);
            }

            GBackupInternal.TryCreateAndMergeInitialBackup(historyPrefix, terrains, resourceFlag, true);
        }

        private void ApplyPath()
        {
            EditorUtility.DisplayProgressBar("Applying", "Creating path...", 1f);
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

        private void CreateBackupAfterApplyPath(List<GStylizedTerrain> terrains)
        {
            string historyPrefix =
                instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic ? HISTORY_PREFIX_ALBEDO_METALLIC :
                instance.Channel == GPathPainter.PaintChannel.Splat ? HISTORY_PREFIX_SPLAT : "Unknown Action";
            List<GTerrainResourceFlag> resourceFlag = new List<GTerrainResourceFlag>();
            if (instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic)
            {
                resourceFlag.Add(GTerrainResourceFlag.AlbedoMap);
                resourceFlag.Add(GTerrainResourceFlag.MetallicMap);
            }
            else if (instance.Channel == GPathPainter.PaintChannel.Splat)
            {
                resourceFlag.Add(GTerrainResourceFlag.SplatControlMaps);
            }

            GBackupInternal.TryCreateAndMergeBackup(historyPrefix, terrains, resourceFlag, true);
        }

        private void OnCameraRender(Camera cam)
        {
            isUrp = false;
            urpContext = default;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.pathPainter)
                DrawLivePreview(cam);
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            isUrp = true;
            urpContext = context; 
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.pathPainter)
                DrawLivePreview(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> sweepTest = instance.SplineCreator.SweepTest();
            for (int i = 0; i < sweepTest.Count; ++i)
            {
                GOverlapTestResult st = sweepTest[i];
                if (!st.IsOverlapped)
                    continue;
                DrawLivePreview(st.Terrain, cam, st.IsChunkOverlapped);
            }
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            if (instance.Channel == GPathPainter.PaintChannel.AlbedoAndMetallic)
            {
                SetupAlbedoMetallicPreview(t, cam, chunkCulling);
            }
            else if (instance.Channel == GPathPainter.PaintChannel.Splat)
            {
                SetupSplatPreview(t, cam, chunkCulling);
            }
        }

        private void SetupAlbedoMetallicPreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            Material mat = t.TerrainData.Shading.MaterialToRender;
            if (mat == null)
                return;
            int albedoResolution = t.TerrainData.Shading.AlbedoMapResolution;
            FilterMode albedoFilter = t.TerrainData.Shading.AlbedoMapOrDefault.filterMode;
            RenderTexture rtAlbedo = GetPreviewTexture(t, "albedo", albedoResolution, albedoFilter);
            if (isUrp)
            {
                instance.Internal_ApplyAlbedo(t, rtAlbedo, urpContext);
            }
            else
            {
                instance.Internal_ApplyAlbedo(t, rtAlbedo);
            }

            int metallicResolution = t.TerrainData.Shading.MetallicMapResolution;
            FilterMode metallicFilter = t.TerrainData.Shading.MetallicMapOrDefault.filterMode;
            RenderTexture rtMetallic = GetPreviewTexture(t, "metallic", metallicResolution, metallicFilter);
            if (isUrp)
            {
                instance.Internal_ApplyMetallic(t, rtMetallic, urpContext);
            }
            else
            {
                instance.Internal_ApplyMetallic(t, rtMetallic);
            }

            GLivePreviewDrawer.DrawAMSLivePreview(t, cam, rtAlbedo, rtMetallic, chunkCulling);
        }

        private void SetupSplatPreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
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
                rtControls[i] = GetPreviewTexture(t, "splatControl" + i, controlMapResolution, t.TerrainData.Shading.GetSplatControlOrDefault(i).filterMode);
            }
            if (isUrp)
            {
                instance.Internal_ApplySplat(t, rtControls, urpContext);
            }
            else
            {
                instance.Internal_ApplySplat(t, rtControls);
            }
            GLivePreviewDrawer.DrawSplatLivePreview(t, cam, rtControls, chunkCulling);
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
    }
}
#endif
