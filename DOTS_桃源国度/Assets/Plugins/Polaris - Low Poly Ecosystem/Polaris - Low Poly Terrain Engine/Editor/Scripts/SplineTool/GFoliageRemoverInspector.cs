#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GFoliageRemover))]
    public class GFoliageRemoverInspector : Editor
    {
        private GFoliageRemover instance;
        private Dictionary<GStylizedTerrain, RenderTexture> previewTextures;

        private bool isUrp;
        private ScriptableRenderContext urpContext;

        private const string HISTORY_PREFIX = "Remove Foliage Along Path";

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            instance = target as GFoliageRemover;
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
            DrawMaskGUI();
            DrawTreesGUI();
            DrawGrassesGUI();
            DrawActionGUI();
        }


        private class GBaseGUI
        {
            public static readonly GUIContent SPLINE_CREATOR = new GUIContent("Spline Creator", "The Spline Creator component which this modifier belongs to");
            public static readonly GUIContent LIVE_PREVIEW = new GUIContent("Live Preview", "Draw a preview on the terrain");
        }

        private void DrawBaseGUI()
        {
            instance.SplineCreator = EditorGUILayout.ObjectField(GBaseGUI.SPLINE_CREATOR, instance.SplineCreator, typeof(GSplineCreator), true) as GSplineCreator;
            EditorGUI.BeginChangeCheck();
            GEditorSettings.Instance.splineTools.livePreview.foliageRemover = EditorGUILayout.Toggle(GBaseGUI.LIVE_PREVIEW, GEditorSettings.Instance.splineTools.livePreview.foliageRemover);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        private class GFalloffGUI
        {
            public static readonly string LABEL = "Falloff";
            public static readonly string ID = "spline-foliage-remover-falloff";

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

        private class GMaskGUI
        {
            public static readonly string LABEL = "Mask";
            public static readonly string ID = "spline-foliage-remover-mask";
            public static readonly GUIContent MASK_RESOLUTION = new GUIContent("Mask Resolution", "Resolution of the mask which is rendered by the spline, this mask will be used for sample foliage instances");
        }

        private void DrawMaskGUI()
        {
            GEditorCommon.Foldout(GMaskGUI.LABEL, false, GMaskGUI.ID, () =>
            {
                instance.MaskResolution = EditorGUILayout.DelayedIntField(GMaskGUI.MASK_RESOLUTION, instance.MaskResolution);
            });
        }

        private class GTreesGUI
        {
            public static readonly string LABEL = "Trees";
            public static readonly string ID = "spline-foliage-remover-trees";

            public static readonly GUIContent REMOVE_TREES = new GUIContent("Remove Trees", "Toggle trees removing along the spline");
            public static readonly GUIContent PROTOTYPES = new GUIContent("Prototypes", "The tree types to remove");
        }

        private void DrawTreesGUI()
        {
            GEditorCommon.Foldout(GTreesGUI.LABEL, false, GTreesGUI.ID, () =>
            {
                instance.RemoveTrees = EditorGUILayout.Toggle(GTreesGUI.REMOVE_TREES, instance.RemoveTrees);
                if (instance.RemoveTrees)
                {
                    EditorGUILayout.LabelField(GTreesGUI.PROTOTYPES);
                    instance.TreePrototypeIndices = GEditorCommon.TreeSetMultiSelectionGrid(instance.SplineCreator.GroupId, instance.TreePrototypeIndices);
                }
            });
        }


        private class GGrassesGUI
        {
            public static readonly string LABEL = "Grasses";
            public static readonly string ID = "spline-foliage-remover-grasses";

            public static readonly GUIContent REMOVE_GRASSES = new GUIContent("Remove Grasses", "Toggle grasses removing along the spline");
            public static readonly GUIContent PROTOTYPES = new GUIContent("Prototypes", "The grass types to remove");
        }

        private void DrawGrassesGUI()
        {
            GEditorCommon.Foldout(GGrassesGUI.LABEL, false, GGrassesGUI.ID, () =>
            {
                instance.RemoveGrasses = EditorGUILayout.Toggle(GGrassesGUI.REMOVE_GRASSES, instance.RemoveGrasses);
                if (instance.RemoveGrasses)
                {
                    EditorGUILayout.LabelField(GGrassesGUI.PROTOTYPES);
                    instance.GrassPrototypeIndices = GEditorCommon.GrassSetMultiSelectionGrid(instance.SplineCreator.GroupId, instance.GrassPrototypeIndices);
                }
            });
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "spline-foliage-remover-action";

            public static readonly GUIContent REMOVE_TREES = new GUIContent("Remove Trees");
            public static readonly GUIContent REMOVE_GRASSES = new GUIContent("Remove Grasses");
            public static readonly GUIContent REMOVE_TREES_GRASSES = new GUIContent("Remove Trees & Grasses");
        }

        private void DrawActionGUI()
        {
            GEditorCommon.ExpandFoldout(GActionGUI.ID);
            GEditorCommon.Foldout(GActionGUI.LABEL, true, GActionGUI.ID, () =>
            {
                GUIContent btnLabel;
                if (instance.RemoveTrees && instance.RemoveGrasses)
                {
                    btnLabel = GActionGUI.REMOVE_TREES_GRASSES;
                }
                else if (instance.RemoveTrees && !instance.RemoveGrasses)
                {
                    btnLabel = GActionGUI.REMOVE_TREES;
                }
                else if (!instance.RemoveTrees && instance.RemoveGrasses)
                {
                    btnLabel = GActionGUI.REMOVE_GRASSES;
                }
                else
                {
                    btnLabel = GActionGUI.REMOVE_TREES_GRASSES;
                }

                GUI.enabled = instance.RemoveTrees || instance.RemoveGrasses;
                if (GUILayout.Button(btnLabel))
                {
                    List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(instance.SplineCreator.SweepTest());
                    CreateInitialBackup(terrains);
                    Apply();
                    CreateBackupAfterApply(terrains);
                }
                GUI.enabled = true;
            });
        }

        private void CreateInitialBackup(List<GStylizedTerrain> terrains)
        {
            GBackupInternal.TryCreateAndMergeInitialBackup(HISTORY_PREFIX, terrains, GCommon.FoliageInstancesResourceFlags, true);
        }

        private void Apply()
        {
            EditorUtility.DisplayProgressBar("Applying", "Removing foliage...", 1f);
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

        private void CreateBackupAfterApply(List<GStylizedTerrain> terrains)
        {
            GBackupInternal.TryCreateAndMergeBackup(HISTORY_PREFIX, terrains, GCommon.FoliageInstancesResourceFlags, true);
        }

        private void OnCameraRender(Camera cam)
        {
            isUrp = false;
            urpContext = default;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.foliageRemover)
                DrawLivePreview(cam);
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            isUrp = true;
            urpContext = context;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.foliageRemover)
                DrawLivePreview(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> sweepTests = instance.SplineCreator.SweepTest();
            foreach (GOverlapTestResult st in sweepTests)
            {
                if (!st.IsOverlapped)
                    continue;
                DrawLivePreview(st.Terrain, cam, st.IsChunkOverlapped);
            }
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            RenderTexture rt = GetPreviewTexture(t);
            if (isUrp)
            {
                instance.Internal_Apply(t, rt, urpContext);
            }
            else
            {
                instance.Internal_Apply(t, rt);
            }

            GLivePreviewDrawer.DrawMasksLivePreview(
                t, cam,
                new Texture[] { rt },
                new Color[] { GEditorSettings.Instance.splineTools.negativeHighlightColor },
                chunkCulling);
        }

        private RenderTexture GetPreviewTexture(GStylizedTerrain t)
        {
            if (previewTextures == null)
            {
                previewTextures = new Dictionary<GStylizedTerrain, RenderTexture>();
            }

            int resolution = instance.MaskResolution;
            if (!previewTextures.ContainsKey(t) ||
                previewTextures[t] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                previewTextures[t] = rt;
            }
            else if (previewTextures[t].width != resolution || previewTextures[t].height != resolution)
            {
                previewTextures[t].Release();
                Object.DestroyImmediate(previewTextures[t]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                previewTextures[t] = rt;
            }

            previewTextures[t].wrapMode = TextureWrapMode.Clamp;
            return previewTextures[t];
        }
    }
}
#endif
