#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GRampMaker))]
    public class GRampMakerInspector : Editor
    {
        private GRampMaker instance;
        private Dictionary<GStylizedTerrain, RenderTexture> previewTextures;

        private static readonly string HISTORY_PREFIX = "Make Ramp";

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            instance = (GRampMaker)target;
            instance.Internal_UpdateFalloffTexture();

            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
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
        }

        public override void OnInspectorGUI()
        {
            DrawBaseGUI();
            if (instance.SplineCreator == null)
                return;
            DrawFalloffGUI();
            DrawRampGUI();
            DrawActionGUI();

#if GRIFFIN_VEGETATION_STUDIO_PRO
            GEditorCommon.DrawVspIntegrationGUI();
#endif
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
            GEditorSettings.Instance.splineTools.livePreview.rampMaker = EditorGUILayout.Toggle(GBaseGUI.LIVE_PREVIEW, GEditorSettings.Instance.splineTools.livePreview.rampMaker);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        private class GFalloffGUI
        {
            public static readonly string LABEL = "Falloff";
            public static readonly string ID = "spline-ramp-maker-falloff";

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

        private class GRampGUI
        {
            public static readonly string LABEL = "Ramp";
            public static readonly string ID = "spline-ramp-maker-ramp";

            public static readonly GUIContent ADDITIONAL_MESH_RESOLUTION = new GUIContent("Additional Mesh Resolution", "Add more mesh detail to the ramp");
            public static readonly GUIContent HEIGHT_OFFSET = new GUIContent("Height Offset", "Offset the ramp height on Y axis, useful for making rivers");
            public static readonly GUIContent STEP_COUNT = new GUIContent("Step Count", "Adding a quantize/step effect to the ramp");
            public static readonly GUIContent RAISE_HEIGHT = new GUIContent("Raise Height", "Toggle height raising");
            public static readonly GUIContent LOWER_HEIGHT = new GUIContent("Lower Height", "Toggle height lowering");
        }

        private void DrawRampGUI()
        {
            GEditorCommon.Foldout(GRampGUI.LABEL, false, GRampGUI.ID, () =>
            {
                instance.AdditionalMeshResolution = EditorGUILayout.IntField(GRampGUI.ADDITIONAL_MESH_RESOLUTION, instance.AdditionalMeshResolution);
                instance.HeightOffset = EditorGUILayout.FloatField(GRampGUI.HEIGHT_OFFSET, instance.HeightOffset);
                instance.StepCount = EditorGUILayout.IntField(GRampGUI.STEP_COUNT, instance.StepCount);
                instance.RaiseHeight = EditorGUILayout.Toggle(GRampGUI.RAISE_HEIGHT, instance.RaiseHeight);
                instance.LowerHeight = EditorGUILayout.Toggle(GRampGUI.LOWER_HEIGHT, instance.LowerHeight);
            });
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "spline-ramp-maker-action";

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
                    ApplyRamp();
                    CreateBackupAfterApplyRamp(terrains);
                }
            });
        }

        private void CreateInitialBackup(List<GStylizedTerrain> terrains)
        {
            GBackupInternal.TryCreateAndMergeInitialBackup(HISTORY_PREFIX, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
        }

        private void ApplyRamp()
        {
            EditorUtility.DisplayProgressBar("Applying", "Creating ramp...", 1f);
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

        private void CreateBackupAfterApplyRamp(List<GStylizedTerrain> terrains)
        {
            GBackupInternal.TryCreateAndMergeBackup(HISTORY_PREFIX, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
        }

        private void OnCameraRender(Camera cam)
        {
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.rampMaker)
                DrawLivePreview(cam);
        }

        private void OnCameraRenderSRP(ScriptableRenderContext context, Camera cam)
        {
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.rampMaker)
                DrawLivePreview(context, cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> sweepTests = instance.SplineCreator.SweepTest();
            for (int i = 0; i < sweepTests.Count; ++i)
            {
                GOverlapTestResult st = sweepTests[i];
                if (st.IsOverlapped)
                {
                    GStylizedTerrain t = st.Terrain;
                    if (t.transform.rotation != Quaternion.identity ||
                        t.transform.lossyScale != Vector3.one)
                        return;

                    RenderTexture rt = GetPreviewTexture(t);
                    instance.Internal_DrawOnTexture(t, rt);

                    GLivePreviewDrawer.DrawGeometryLivePreview(t, cam, rt, st.IsChunkOverlapped);
                }
            }
        }

        private void DrawLivePreview(ScriptableRenderContext context, Camera cam)
        {
            List<GOverlapTestResult> sweepTests = instance.SplineCreator.SweepTest();
            for (int i = 0; i < sweepTests.Count; ++i)
            {
                GOverlapTestResult st = sweepTests[i];
                if (st.IsOverlapped)
                {
                    GStylizedTerrain t = st.Terrain;
                    if (t.transform.rotation != Quaternion.identity ||
                        t.transform.lossyScale != Vector3.one)
                        return;

                    RenderTexture rt = GetPreviewTexture(t);
                    instance.Internal_DrawOnTexture(t, rt, context);

                    GLivePreviewDrawer.DrawGeometryLivePreview(t, cam, rt, st.IsChunkOverlapped);
                }
            }
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
    }
}
#endif
