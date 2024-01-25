#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GEditorSettings))]
    public class GEditorSettingsInspector : Editor
    {
        private GEditorSettings instance;
        private void OnEnable()
        {
            instance = target as GEditorSettings;
        }

        public override void OnInspectorGUI()
        {
            DrawGeneralSettings();
            DrawLivePreviewSettings();
            DrawPaintToolsSettings();
            DrawSplineToolsSettings();
            DrawBillboardToolsSettings();
            DrawStampToolsSettings();
            DrawWizardToolsSettings();
            DrawRenderPipelinesSettings();
            DrawTopographicSettings();
            DrawLayersSettings();
            DrawDemoAssetSettings();

            EditorUtility.SetDirty(instance);
        }

        private void DrawGeneralSettings()
        {
            string label = "General";
            string id = "editor-settings-general";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.general.enableAnalytics = EditorGUILayout.Toggle("Allow Anonymous Editor Analytic", instance.general.enableAnalytics);
                instance.general.enableAffiliateLinks = EditorGUILayout.Toggle("Allow Affiliate Links", instance.general.enableAffiliateLinks);
                instance.general.debugMode = EditorGUILayout.Toggle("Debug Mode", instance.general.debugMode);
                EditorGUI.BeginChangeCheck();
                instance.general.showGeometryChunkInHierarchy = EditorGUILayout.Toggle("Show Geometry Chunks In Hierarchy", instance.general.showGeometryChunkInHierarchy);
                if (EditorGUI.EndChangeCheck())
                {
                    IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
                    while (terrains.MoveNext())
                    {
                        GStylizedTerrain t = terrains.Current;
                        Transform chunkRoot = t.GetOrCreateChunkRoot();
                        chunkRoot.gameObject.hideFlags = instance.general.showGeometryChunkInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy;
                    }
                    GUtilities.MarkCurrentSceneDirty();
                }
            });
        }

        private void DrawLivePreviewSettings()
        {
            string label = "Live Preview";
            string id = "editor-settings-live-preview";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                int count = GCommon.MAX_MESH_RESOLUTION + 1;

                GEditorCommon.Header("Triangle Meshes");
                if (instance.livePreview.triangleMeshes == null || instance.livePreview.triangleMeshes.Length != count)
                {
                    instance.livePreview.triangleMeshes = new Mesh[count];
                }
                for (int i = 0; i < count; ++i)
                {
                    instance.livePreview.triangleMeshes[i] = EditorGUILayout.ObjectField("LOD " + i, instance.livePreview.triangleMeshes[i], typeof(Mesh), false) as Mesh;
                }

                GEditorCommon.Header("Wireframe Meshes");
                if (instance.livePreview.wireframeMeshes == null || instance.livePreview.wireframeMeshes.Length != count)
                {
                    instance.livePreview.wireframeMeshes = new Mesh[count];
                }
                for (int i = 0; i < count; ++i)
                {
                    instance.livePreview.wireframeMeshes[i] = EditorGUILayout.ObjectField("LOD " + i, instance.livePreview.wireframeMeshes[i], typeof(Mesh), false) as Mesh;
                }
            });
        }

        public void DrawPaintToolsSettings()
        {
            string label = "Paint Tools";
            string id = "editor-settings-paint-tools";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.paintTools.useSimpleCursor = EditorGUILayout.Toggle("Simple Cursor", instance.paintTools.useSimpleCursor);
                instance.paintTools.normalActionCursorColor = EditorGUILayout.ColorField("Normal Color", instance.paintTools.normalActionCursorColor);
                instance.paintTools.negativeActionCursorColor = EditorGUILayout.ColorField("Negative Color", instance.paintTools.negativeActionCursorColor);
                instance.paintTools.alternativeActionCursorColor = EditorGUILayout.ColorField("Alternative Color", instance.paintTools.alternativeActionCursorColor);
                instance.paintTools.radiusStep = EditorGUILayout.FloatField("Radius Step", instance.paintTools.radiusStep);
                instance.paintTools.rotationStep = EditorGUILayout.FloatField("Rotation Step", instance.paintTools.rotationStep);
                instance.paintTools.opacityStep = EditorGUILayout.FloatField("Opacity Step", instance.paintTools.opacityStep);
                instance.paintTools.densityStep = EditorGUILayout.IntField("Density Step", instance.paintTools.densityStep);
            });
        }

        public void DrawSplineToolsSettings()
        {
            string label = "Spline Tools";
            string id = "editor-settings-spline-tools";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.splineTools.anchorColor = EditorGUILayout.ColorField("Anchor Color", instance.splineTools.anchorColor);
                instance.splineTools.segmentColor = EditorGUILayout.ColorField("Segment Color", instance.splineTools.segmentColor);
                instance.splineTools.meshColor = EditorGUILayout.ColorField("Mesh Color", instance.splineTools.meshColor);
                instance.splineTools.selectedElementColor = EditorGUILayout.ColorField("Selected Color", instance.splineTools.selectedElementColor);
                instance.splineTools.positiveHighlightColor = EditorGUILayout.ColorField("Positive Highlight Color", instance.splineTools.positiveHighlightColor);
                instance.splineTools.negativeHighlightColor = EditorGUILayout.ColorField("Negative Highlight Color", instance.splineTools.negativeHighlightColor);
                instance.splineTools.raycastLayers = GEditorCommon.LayerMaskField("Raycast Layers", instance.splineTools.raycastLayers);
                instance.splineTools.showTransformGizmos = EditorGUILayout.Toggle("Show Transform Gizmos", instance.splineTools.showTransformGizmos);
                instance.splineTools.autoTangent = EditorGUILayout.Toggle("Auto Tangent", instance.splineTools.autoTangent);

                GEditorCommon.Header("Live Preview");
                instance.splineTools.livePreview.foliageSpawner = EditorGUILayout.Toggle("Foliage Spawner", instance.splineTools.livePreview.foliageSpawner);
            });
        }

        public void DrawBillboardToolsSettings()
        {
            string label = "Billboard Tools";
            string id = "editor-settings-billboard-tools";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.billboardTools.atlasMaterial = EditorGUILayout.ObjectField("Atlas Material", instance.billboardTools.atlasMaterial, typeof(Material), false) as Material;
                instance.billboardTools.normalMaterial = EditorGUILayout.ObjectField("Normal Material", instance.billboardTools.normalMaterial, typeof(Material), false) as Material;
            });
        }

        public void DrawStampToolsSettings()
        {
            string label = "Stamp Tools";
            string id = "editor-settings-stamp-tools";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.stampTools.visualizeColor = EditorGUILayout.ColorField("Visualize Color", instance.stampTools.visualizeColor);
                instance.stampTools.minRotation = EditorGUILayout.FloatField("Default Min Rotation", instance.stampTools.minRotation);
                instance.stampTools.maxRotation = EditorGUILayout.FloatField("Default Max Rotation", instance.stampTools.maxRotation);
                instance.stampTools.minScale = GEditorCommon.InlineVector3Field("Default Min Scale", instance.stampTools.minScale);
                instance.stampTools.maxScale = GEditorCommon.InlineVector3Field("Default Max Scale", instance.stampTools.maxScale);
            });
        }

        public void DrawWizardToolsSettings()
        {
            string label = "Wizard Tools";
            string id = "editor-settings-wizard-tools";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorSettings.Instance.wizardTools.lightingModel = (GLightingModel)EditorGUILayout.EnumPopup("Lighting Model", GEditorSettings.Instance.wizardTools.lightingModel);
                GEditorSettings.Instance.wizardTools.texturingModel = (GTexturingModel)EditorGUILayout.EnumPopup("Texturing Model", GEditorSettings.Instance.wizardTools.texturingModel);
                GEditorSettings.Instance.wizardTools.splatsModel = (GSplatsModel)EditorGUILayout.EnumPopup("Splats Model", GEditorSettings.Instance.wizardTools.splatsModel);

                GEditorSettings.Instance.wizardTools.origin = GEditorCommon.InlineVector3Field("Origin", GEditorSettings.Instance.wizardTools.origin);
                GEditorSettings.Instance.wizardTools.tileSize = GEditorCommon.InlineVector3Field("Tile Size", GEditorSettings.Instance.wizardTools.tileSize);
                GEditorSettings.Instance.wizardTools.tileCountX = EditorGUILayout.IntField("Tile Count X", GEditorSettings.Instance.wizardTools.tileCountX);
                GEditorSettings.Instance.wizardTools.tileCountZ = EditorGUILayout.IntField("Tile Count Z", GEditorSettings.Instance.wizardTools.tileCountZ);
                GEditorSettings.Instance.wizardTools.groupId = EditorGUILayout.IntField("Group Id", GEditorSettings.Instance.wizardTools.groupId);
                GEditorSettings.Instance.wizardTools.setShaderGroupId = EditorGUILayout.IntField("Set Shader Group Id", GEditorSettings.Instance.wizardTools.setShaderGroupId);

                GEditorSettings.Instance.wizardTools.terrainNamePrefix = EditorGUILayout.TextField("Terrian Name Prefix", GEditorSettings.Instance.wizardTools.terrainNamePrefix);
                GEditorSettings.Instance.wizardTools.dataDirectory = EditorGUILayout.TextField("Data Directory", GEditorSettings.Instance.wizardTools.dataDirectory);
            });
        }

        public void DrawRenderPipelinesSettings()
        {
            string label = "Render Pipelines";
            string id = "editor-settings-render-pipelines";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.renderPipelines.universalRenderPipelinePackage = EditorGUILayout.ObjectField("Universal RP Package", instance.renderPipelines.universalRenderPipelinePackage, typeof(Object), false);
            });
        }

        public void DrawTopographicSettings()
        {
            string label = "Topographic";
            string id = "editor-settings-topographic";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.topographic.enable = EditorGUILayout.Toggle("Enable", instance.topographic.enable);
                instance.topographic.topographicMaterial = EditorGUILayout.ObjectField("Material", instance.topographic.topographicMaterial, typeof(Material), false) as Material;
            });
        }

        public void DrawLayersSettings()
        {
            string label = "Layers";
            string id = "editor-settings-layers";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.layers.raycastLayerIndex = EditorGUILayout.IntSlider("Raycast Layer Index", instance.layers.raycastLayerIndex, 8, 31);
                instance.layers.splineLayerIndex = EditorGUILayout.IntSlider("Spline Layer Index", instance.layers.splineLayerIndex, 8, 31);
            });
        }

        public void DrawDemoAssetSettings()
        {
            string label = "Demo Assets";
            string id = "editor-settings-demo-assets";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                SerializedProperty demoMaterialsProp = serializedObject.FindProperty(nameof(instance.demoAssets)).FindPropertyRelative(nameof(instance.demoAssets.demoMaterials));
                EditorGUILayout.PropertyField(demoMaterialsProp, true);
                serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
#endif
