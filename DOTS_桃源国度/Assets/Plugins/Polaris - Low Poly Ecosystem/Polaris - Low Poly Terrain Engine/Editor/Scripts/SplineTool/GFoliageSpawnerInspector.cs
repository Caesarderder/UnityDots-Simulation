#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GFoliageSpawner))]
    public class GFoliageSpawnerInspector : Editor
    {
        private GFoliageSpawner instance;
        private Dictionary<GStylizedTerrain, RenderTexture> previewTextures;
        private MaterialPropertyBlock previewPropertyBlock;

        private bool isUrp;
        private ScriptableRenderContext urpContext;

        private const string HISTORY_PREFIX = "Spawn Foliage Along Path";

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            instance = target as GFoliageSpawner;
            instance.Internal_UpdateFalloffTexture();
            previewPropertyBlock = new MaterialPropertyBlock();

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
            DrawRotationScaleGUI();
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
            GEditorSettings.Instance.splineTools.livePreview.foliageSpawner = EditorGUILayout.Toggle(GBaseGUI.LIVE_PREVIEW, GEditorSettings.Instance.splineTools.livePreview.foliageSpawner);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        private class GFalloffGUI
        {
            public static readonly string LABEL = "Falloff";
            public static readonly string ID = "spline-foliage-spawner-falloff";

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
            public static readonly string ID = "spline-foliage-spawner-mask";
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
            public static readonly string ID = "spline-foliage-spawner-trees";

            public static readonly GUIContent SPAWN_TREES = new GUIContent("Spawn Trees", "Toggle trees spawning along the spline");
            public static readonly GUIContent PROTOTYPES = new GUIContent("Prototypes", "The tree types to spawn");
            public static readonly GUIContent DENSITY = new GUIContent("Density", "Number of instances per a meter-squared");
        }

        private void DrawTreesGUI()
        {
            GEditorCommon.Foldout(GTreesGUI.LABEL, false, GTreesGUI.ID, () =>
            {
                instance.SpawnTrees = EditorGUILayout.Toggle(GTreesGUI.SPAWN_TREES, instance.SpawnTrees);
                if (instance.SpawnTrees)
                {
                    EditorGUILayout.LabelField(GTreesGUI.PROTOTYPES);
                    instance.TreePrototypeIndices = GEditorCommon.TreeSetMultiSelectionGrid(instance.SplineCreator.GroupId, instance.TreePrototypeIndices);
                    instance.TreeDensity = EditorGUILayout.FloatField(GTreesGUI.DENSITY, instance.TreeDensity);
                }
            });

        }

        private class GGrassesGUI
        {
            public static readonly string LABEL = "Grasses";
            public static readonly string ID = "spline-foliage-spawner-grasses";

            public static readonly GUIContent SPAWN_GRASSES = new GUIContent("Spawn Grasses", "Toggle grasses spawning along the spline");
            public static readonly GUIContent PROTOTYPES = new GUIContent("Prototypes", "The grass types to spawn");
            public static readonly GUIContent DENSITY = new GUIContent("Density", "Number of instances per a meter-squared");
        }

        private void DrawGrassesGUI()
        {
            GEditorCommon.Foldout(GGrassesGUI.LABEL, false, GGrassesGUI.ID, () =>
            {
                instance.SpawnGrasses = EditorGUILayout.Toggle(GGrassesGUI.SPAWN_GRASSES, instance.SpawnGrasses);
                if (instance.SpawnGrasses)
                {
                    EditorGUILayout.LabelField(GGrassesGUI.PROTOTYPES);
                    instance.GrassPrototypeIndices = GEditorCommon.GrassSetMultiSelectionGrid(instance.SplineCreator.GroupId, instance.GrassPrototypeIndices);
                    instance.GrassDensity = EditorGUILayout.FloatField(GGrassesGUI.DENSITY, instance.GrassDensity);
                }
            });

        }

        private class GRotationScaleGUI
        {
            public static readonly string LABEL = "Rotation & Scale";
            public static readonly string ID = "spline-foliage-spawner-rotation-scale";

            public static readonly GUIContent MIN_ROTATION = new GUIContent("Min Rotation", "Minimum rotation of each instance, in degree, on Y axis");
            public static readonly GUIContent MAX_ROTATION = new GUIContent("Max Rotation", "Maximum rotation of each instance, in degree, on Y axis");
            public static readonly GUIContent MIN_SCALE = new GUIContent("Min Scale", "Minimum scale of each instance");
            public static readonly GUIContent MAX_SCALE = new GUIContent("Max Scale", "Maximum scale of each instance");
        }

        private void DrawRotationScaleGUI()
        {
            GEditorCommon.Foldout(GRotationScaleGUI.LABEL, false, GRotationScaleGUI.ID, () =>
            {
                instance.MinRotation = EditorGUILayout.FloatField(GRotationScaleGUI.MIN_ROTATION, instance.MinRotation);
                instance.MaxRotation = EditorGUILayout.FloatField(GRotationScaleGUI.MAX_ROTATION, instance.MaxRotation);
                instance.MinScale = GEditorCommon.InlineVector3Field(GRotationScaleGUI.MIN_SCALE, instance.MinScale);
                instance.MaxScale = GEditorCommon.InlineVector3Field(GRotationScaleGUI.MAX_SCALE, instance.MaxScale);
            });
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "spline-foliage-spawner-action";

            public static readonly GUIContent SPAWN_TREES = new GUIContent("Spawn Trees");
            public static readonly GUIContent SPAWN_GRASSES = new GUIContent("Spawn Grasses");
            public static readonly GUIContent SPAWN_TREES_GRASSES = new GUIContent("Spawn Trees & Grasses");
        }

        private void DrawActionGUI()
        {
            GEditorCommon.ExpandFoldout(GActionGUI.ID);
            GEditorCommon.Foldout(GActionGUI.LABEL, true, GActionGUI.ID, () =>
            {
                GUIContent btnLabel;
                if (instance.SpawnTrees && instance.SpawnGrasses)
                {
                    btnLabel = GActionGUI.SPAWN_TREES_GRASSES;
                }
                else if (instance.SpawnTrees && !instance.SpawnGrasses)
                {
                    btnLabel = GActionGUI.SPAWN_TREES;
                }
                else if (!instance.SpawnTrees && instance.SpawnGrasses)
                {
                    btnLabel = GActionGUI.SPAWN_GRASSES;
                }
                else
                {
                    btnLabel = GActionGUI.SPAWN_TREES_GRASSES;
                }

                GUI.enabled = instance.SpawnTrees || instance.SpawnGrasses;
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
            EditorUtility.DisplayProgressBar("Applying", "Spawning foliage...", 1f);
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
            if (GEditorSettings.Instance.splineTools.livePreview.foliageSpawner)
                DrawLivePreview(cam);
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            isUrp = true;
            urpContext = context;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.foliageSpawner)
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
                new Color[] { GEditorSettings.Instance.splineTools.positiveHighlightColor },
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
