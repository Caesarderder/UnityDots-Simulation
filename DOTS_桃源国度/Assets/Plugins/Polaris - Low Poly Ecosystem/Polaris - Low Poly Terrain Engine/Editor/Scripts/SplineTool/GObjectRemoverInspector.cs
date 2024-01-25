#if GRIFFIN
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GObjectRemover))]
    public class GObjectRemoverInspector : Editor
    {
        private GObjectRemover instance;
        private Dictionary<GStylizedTerrain, RenderTexture> previewTextures;

        private bool isUrp;
        private ScriptableRenderContext urpContext;

        private const string HISTORY_PREFIX = "Remove Foliage Along Path";

        private void OnEnable()
        {
            instance = target as GObjectRemover;
            instance.Internal_UpdateFalloffTexture();

            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);
        }

        private void OnDisable()
        {
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

        public override void OnInspectorGUI()
        {
            DrawBaseGUI();
            if (instance.SplineCreator == null)
                return;
            DrawFalloffGUI();
            DrawMaskGUI();
            DrawObjectGUI();
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
            GEditorSettings.Instance.splineTools.livePreview.objectRemover = EditorGUILayout.Toggle(GBaseGUI.LIVE_PREVIEW, GEditorSettings.Instance.splineTools.livePreview.objectRemover);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
        }

        private class GFalloffGUI
        {
            public static readonly string LABEL = "Falloff";
            public static readonly string ID = "spline-object-remover-falloff";

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
            public static readonly string ID = "spline-object-remover-mask";
            public static readonly GUIContent MASK_RESOLUTION = new GUIContent("Mask Resolution", "Resolution of the mask which is rendered by the spline, this mask will be used for sample object instances");
        }

        private void DrawMaskGUI()
        {
            GEditorCommon.Foldout(GMaskGUI.LABEL, false, GMaskGUI.ID, () =>
            {
                instance.MaskResolution = EditorGUILayout.DelayedIntField(GMaskGUI.MASK_RESOLUTION, instance.MaskResolution);
            });
        }


        private class GObjectGUI
        {
            public static readonly string LABEL = "Objects";
            public static readonly string ID = "spline-object-remover-objects";

            public static readonly GUIContent MIN_ROTATION = new GUIContent("Min Rotation", "Minimum rotation of each instance, in degree, on Y axis");
            public static readonly GUIContent MAX_ROTATION = new GUIContent("Max Rotation", "Maximum rotation of each instance, in degree, on Y axis");
            public static readonly GUIContent MIN_SCALE = new GUIContent("Min Scale", "Minimum scale of each instance");
            public static readonly GUIContent MAX_SCALE = new GUIContent("Max Scale", "Maximum scale of each instance");
            public static readonly GUIContent RAYCAST_LAYER = new GUIContent("Raycast Layer", "Object layer to perform raycast for snapping");
            public static readonly GUIContent ALIGN_TO_SURFACE = new GUIContent("Align To Surface", "Align the object for terrain surface");
            public static readonly GUIContent DENSITY = new GUIContent("Density", "Number of object instances per a meter-squared");
        }

        private void DrawObjectGUI()
        {
            GEditorCommon.Foldout(GObjectGUI.LABEL, false, GObjectGUI.ID, () =>
            {
                DrawObjectSelectorGUI();
            });
        }


        private void DrawObjectSelectorGUI()
        {
            if (instance.Prototypes.Count > 0)
            {
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = instance.Prototypes;
                args.selectedIndices = instance.PrototypeIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
                args.itemPerRow = 4;
                args.drawPreviewFunction = GEditorCommon.DrawGameObjectPreview;
                args.contextClickFunction = OnObjectSelectorContextClick;
                instance.PrototypeIndices = GEditorCommon.MultiSelectionGrid(args);
            }
            else
            {
                EditorGUILayout.LabelField("No Game Object found!", GEditorCommon.WordWrapItalicLabel);
            }
            GEditorCommon.Separator();

            Rect r1 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GameObject prefab = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r1, "Drop a Game Object here!", "t:GameObject");
            if (prefab != null)
            {
                instance.Prototypes.AddIfNotContains(prefab);
            }

            GEditorCommon.SpacePixel(0);
            Rect r2 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GPrefabPrototypeGroup group = GEditorCommon.ObjectSelectorDragDrop<GPrefabPrototypeGroup>(r2, "Drop a Prefab Prototype Group here!", "t:GPrefabPrototypeGroup");
            if (group != null)
            {
                instance.Prototypes.AddIfNotContains(group.Prototypes);
            }
        }

        private void OnObjectSelectorContextClick(Rect r, int index, ICollection collection)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Remove"),
                false,
                () =>
                {
                    instance.Prototypes.RemoveAt(index);
                });

            menu.ShowAsContext();
        }

        private class GActionGUI
        {
            public static readonly string LABEL = "Action";
            public static readonly string ID = "spline-object-remover-action";

            public static readonly GUIContent SPAWN = new GUIContent("Remove");
        }

        private void DrawActionGUI()
        {
            GEditorCommon.ExpandFoldout(GActionGUI.ID);
            GEditorCommon.Foldout(GActionGUI.LABEL, true, GActionGUI.ID, () =>
            {
                if (GUILayout.Button(GActionGUI.SPAWN))
                {
                    Apply();
                }
            });
        }


        private void Apply()
        {
            EditorUtility.DisplayProgressBar("Applying", "Removing objects...", 1f);
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

        private void OnCameraRender(Camera cam)
        {
            isUrp = false;
            urpContext = default;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.objectRemover)
                DrawLivePreview(cam);
        }

        private void OnCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            isUrp = true;
            urpContext = context;
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.splineTools.livePreview.objectRemover)
                DrawLivePreview(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> sweepTests = instance.SplineCreator.SweepTest();
            foreach (GOverlapTestResult st in sweepTests)
            {
                if (!st.IsOverlapped)
                {
                    continue;
                }

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
