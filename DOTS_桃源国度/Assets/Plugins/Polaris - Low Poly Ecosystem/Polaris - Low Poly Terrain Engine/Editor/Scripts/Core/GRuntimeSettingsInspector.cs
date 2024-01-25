#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GRuntimeSettings))]
    public class GRuntimeSettingsInspector : Editor
    {
        private GRuntimeSettings instance;
        private void OnEnable()
        {
            instance = target as GRuntimeSettings;
        }

        public override void OnInspectorGUI()
        {
            DrawGeometryDefaultSettings();
            DrawShadingDefaultSettings();
            DrawRenderingDefaultSettings();
            DrawFoliageDefaultSettings();
            DrawMaskDefaultSettings();
            DrawGeometryGenerationSettings();
            DrawTerrainRenderingSettings();
            DrawFoliageRenderingSettings();
            DrawInternalShaderSettings();
            DrawDefaultTexturesSettings();
            EditorUtility.SetDirty(instance);
        }

        private void DrawGeometryDefaultSettings()
        {
            string label = "Geometry Default";
            string id = "runtime-settings-geometry-default";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorCommon.Header("Dimension");
                instance.geometryDefault.width = EditorGUILayout.FloatField("Width", instance.geometryDefault.width);
                instance.geometryDefault.height = EditorGUILayout.FloatField("Height", instance.geometryDefault.height);
                instance.geometryDefault.length = EditorGUILayout.FloatField("Length", instance.geometryDefault.length);

                GEditorCommon.Header("Height Map");
                instance.geometryDefault.heightMapResolution = EditorGUILayout.IntField("Height Map Resolution", instance.geometryDefault.heightMapResolution);

                GEditorCommon.Header("Mesh Generation");
                instance.geometryDefault.meshBaseResolution = EditorGUILayout.IntField("Mesh Base Resolution", instance.geometryDefault.meshBaseResolution);
                instance.geometryDefault.meshResolution = EditorGUILayout.IntField("Mesh Resolution", instance.geometryDefault.meshResolution);
                instance.geometryDefault.chunkGridSize = EditorGUILayout.IntField("Grid Size", instance.geometryDefault.chunkGridSize);
                instance.geometryDefault.lodCount = EditorGUILayout.IntField("LOD Count", instance.geometryDefault.lodCount);
                instance.geometryDefault.displacementSeed = EditorGUILayout.IntField("Displacement Seed", instance.geometryDefault.displacementSeed);
                instance.geometryDefault.displacementStrength = EditorGUILayout.FloatField("Displacement Strength", instance.geometryDefault.displacementStrength);
                instance.geometryDefault.albedoToVertexColorMode = (GAlbedoToVertexColorMode)EditorGUILayout.EnumPopup("Albedo To Vertex Color", instance.geometryDefault.albedoToVertexColorMode);
                instance.geometryDefault.smoothNormal = EditorGUILayout.Toggle("Smooth Normal", instance.geometryDefault.smoothNormal);
                instance.geometryDefault.useSmoothNormalMask = EditorGUILayout.Toggle("Smooth Normal Use Mask (G)", instance.geometryDefault.useSmoothNormalMask);
                instance.geometryDefault.mergeUv = EditorGUILayout.Toggle("Merge UV", instance.geometryDefault.mergeUv);
                instance.geometryDefault.storageMode = (GGeometry.GStorageMode)EditorGUILayout.EnumPopup("Storage", instance.geometryDefault.storageMode);
                instance.geometryDefault.allowTimeSlicedGeneration = EditorGUILayout.Toggle("Time Sliced", instance.geometryDefault.allowTimeSlicedGeneration);
            });
        }

        private void DrawShadingDefaultSettings()
        {
            string label = "Shading Default";
            string id = "runtime-settings-shading-default";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorCommon.Header("Color Map & Gradient Lookup");
                instance.shadingDefault.albedoMapResolution = EditorGUILayout.IntField("Albedo Map Resolution", instance.shadingDefault.albedoMapResolution);
                instance.shadingDefault.metallicMapResolution = EditorGUILayout.IntField("Metallic Map Resolution", instance.shadingDefault.metallicMapResolution);

                instance.shadingDefault.colorByNormal = EditorGUILayout.GradientField("Color By Normal", instance.shadingDefault.colorByNormal);
                instance.shadingDefault.colorBlendCurve = EditorGUILayout.CurveField("Blend By Height", instance.shadingDefault.colorBlendCurve, Color.red, new Rect(0, 0, 1, 1));
                instance.shadingDefault.colorByHeight = EditorGUILayout.GradientField("Color By Height", instance.shadingDefault.colorByHeight);

                GEditorCommon.Header("Splats");
                instance.shadingDefault.splatControlResolution = EditorGUILayout.DelayedIntField("Control Map Resolution", instance.shadingDefault.splatControlResolution);

                GEditorCommon.Header("Properties Name");
                instance.shadingDefault.albedoMapPropertyName = EditorGUILayout.TextField("Albedo Map", instance.shadingDefault.albedoMapPropertyName);
                instance.shadingDefault.metallicMapPropertyName = EditorGUILayout.TextField("Metallic Map", instance.shadingDefault.metallicMapPropertyName);
                instance.shadingDefault.colorByHeightPropertyName = EditorGUILayout.TextField("Color By Height", instance.shadingDefault.colorByHeightPropertyName);
                instance.shadingDefault.colorByNormalPropertyName = EditorGUILayout.TextField("Color By Normal", instance.shadingDefault.colorByNormalPropertyName);
                instance.shadingDefault.colorBlendPropertyName = EditorGUILayout.TextField("Color Blend", instance.shadingDefault.colorBlendPropertyName);
                instance.shadingDefault.dimensionPropertyName = EditorGUILayout.TextField("Dimension", instance.shadingDefault.dimensionPropertyName);
                instance.shadingDefault.splatControlMapPropertyName = EditorGUILayout.TextField("Splat Control Map", instance.shadingDefault.splatControlMapPropertyName);
                instance.shadingDefault.splatMapPropertyName = EditorGUILayout.TextField("Splat Map", instance.shadingDefault.splatMapPropertyName);
                instance.shadingDefault.splatNormalPropertyName = EditorGUILayout.TextField("Splat Normal Map", instance.shadingDefault.splatNormalPropertyName);
                instance.shadingDefault.splatMetallicPropertyName = EditorGUILayout.TextField("Splat Metallic", instance.shadingDefault.splatMetallicPropertyName);
                instance.shadingDefault.splatSmoothnessPropertyName = EditorGUILayout.TextField("Splat Smoothness", instance.shadingDefault.splatSmoothnessPropertyName);
            });
        }

        private void DrawRenderingDefaultSettings()
        {
            string label = "Rendering Default";
            string id = "runtime-settings-rendering-defaults";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorCommon.Header("Terrain Shadow");
                instance.renderingDefault.terrainCastShadow = EditorGUILayout.Toggle("Cast Shadow", instance.renderingDefault.terrainCastShadow);
                instance.renderingDefault.terrainReceiveShadow = EditorGUILayout.Toggle("Receive Shadow", instance.renderingDefault.terrainReceiveShadow);

                GEditorCommon.Header("Tree Rendering");
                instance.renderingDefault.drawTrees = EditorGUILayout.Toggle("Draw", instance.renderingDefault.drawTrees);

                if (!SystemInfo.supportsInstancing)
                {
                    instance.renderingDefault.enableInstancing = false;
                }
                GUI.enabled = SystemInfo.supportsInstancing;
                instance.renderingDefault.enableInstancing = EditorGUILayout.Toggle("Enable Instancing", instance.renderingDefault.enableInstancing);
                GUI.enabled = true;
                instance.renderingDefault.billboardStart = EditorGUILayout.Slider("Billboard Start", instance.renderingDefault.billboardStart, 0f, GCommon.MAX_TREE_DISTANCE);
                instance.renderingDefault.treeDistance = EditorGUILayout.Slider("Tree Distance", instance.renderingDefault.treeDistance, 0f, GCommon.MAX_TREE_DISTANCE);
                instance.renderingDefault.treeCullBias = EditorGUILayout.Slider("Cull Bias", instance.renderingDefault.treeCullBias, 0f, 100f);

                GEditorCommon.Header("Grass & Detail Rendering");
                instance.renderingDefault.drawGrasses = EditorGUILayout.Toggle("Draw", instance.renderingDefault.drawGrasses);
                instance.renderingDefault.grassDistance = EditorGUILayout.Slider("Grass Distance", instance.renderingDefault.grassDistance, 0f, GCommon.MAX_GRASS_DISTANCE);
                instance.renderingDefault.grassFadeStart = EditorGUILayout.Slider("Fade Start", instance.renderingDefault.grassFadeStart, 0f, 1f);
                instance.renderingDefault.grassCellToProcessPerFrame = EditorGUILayout.IntField("Cell To Process Per Frame", instance.renderingDefault.grassCellToProcessPerFrame);
                instance.renderingDefault.grassCullBias = EditorGUILayout.Slider("Cull Bias", instance.renderingDefault.grassCullBias, 0f, 100f);
            });
        }

        private void DrawFoliageDefaultSettings()
        {
            string label = "Foliage Default";
            string id = "runtime-settings-foliage-defaults";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorCommon.Header("Trees");
                GRuntimeSettings.Instance.foliageDefault.treeSnapMode = (GSnapMode)EditorGUILayout.EnumPopup("Snap Mode", GRuntimeSettings.Instance.foliageDefault.treeSnapMode);
                GRuntimeSettings.Instance.foliageDefault.treeSnapLayerMask = GEditorCommon.LayerMaskField("Snap Layer", GRuntimeSettings.Instance.foliageDefault.treeSnapLayerMask);

                GEditorCommon.Header("Grasses & Details");
                GRuntimeSettings.Instance.foliageDefault.patchGridSize = EditorGUILayout.DelayedIntField("Patch Grid Size", GRuntimeSettings.Instance.foliageDefault.patchGridSize);
                GRuntimeSettings.Instance.foliageDefault.grassSnapMode = (GSnapMode)EditorGUILayout.EnumPopup("Snap Mode", GRuntimeSettings.Instance.foliageDefault.grassSnapMode);
                GRuntimeSettings.Instance.foliageDefault.grassSnapLayerMask = GEditorCommon.LayerMaskField("Snap Layer", GRuntimeSettings.Instance.foliageDefault.grassSnapLayerMask);
                GRuntimeSettings.Instance.foliageDefault.enableInteractiveGrass = EditorGUILayout.Toggle("Interactive Grass", GRuntimeSettings.Instance.foliageDefault.enableInteractiveGrass);
                GRuntimeSettings.Instance.foliageDefault.vectorFieldMapResolution = EditorGUILayout.DelayedIntField("Vector Field Map Resolution", GRuntimeSettings.Instance.foliageDefault.vectorFieldMapResolution);
                GRuntimeSettings.Instance.foliageDefault.bendSensitive = EditorGUILayout.Slider("Bend Sensitive", GRuntimeSettings.Instance.foliageDefault.bendSensitive, 0f, 1f);
                GRuntimeSettings.Instance.foliageDefault.restoreSensitive = EditorGUILayout.Slider("Restore Sensitive", GRuntimeSettings.Instance.foliageDefault.restoreSensitive, 0f, 1f);
            });
        }

        private void DrawMaskDefaultSettings()
        {
            string label = "Mask Default";
            string id = "runtime-settings-mask-defaults";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GRuntimeSettings.Instance.maskDefault.maskMapResolution = EditorGUILayout.IntField("Mask Map Resolution", GRuntimeSettings.Instance.maskDefault.maskMapResolution);
            });
        }

        private void DrawGeometryGenerationSettings()
        {
            string label = "Geometry Generation";
            string id = "runtime-settings-geometry-generation";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GRuntimeSettings.Instance.geometryGeneration.triangulateIteration = EditorGUILayout.IntField("Triangulate Iteration", GRuntimeSettings.Instance.geometryGeneration.triangulateIteration);
                GRuntimeSettings.Instance.geometryGeneration.lodTransition = EditorGUILayout.CurveField("LOD Transition", GRuntimeSettings.Instance.geometryGeneration.lodTransition, Color.red, GCommon.UnitRect);
            });
        }

        private void DrawTerrainRenderingSettings()
        {
            string label = "Terrain Rendering";
            string id = "runtime-settings-terrain-rendering";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                SerializedObject so = new SerializedObject(instance);
                SerializedProperty terrainRenderingSettings = so.FindProperty(nameof(instance.terrainRendering));
                SerializedProperty builtinMaterials = terrainRenderingSettings.FindPropertyRelative(nameof(instance.terrainRendering.builtinRpMaterials));
                EditorGUILayout.PropertyField(builtinMaterials, new GUIContent("Builtin RP Materials"), true);
                SerializedProperty universalMaterials = terrainRenderingSettings.FindPropertyRelative(nameof(instance.terrainRendering.universalRpMaterials));
                EditorGUILayout.PropertyField(universalMaterials, new GUIContent("Universal RP Materials"), true);
                so.ApplyModifiedProperties();
                universalMaterials.Dispose();
                builtinMaterials.Dispose();
                terrainRenderingSettings.Dispose();
                so.Dispose();
            });
        }

        private void DrawFoliageRenderingSettings()
        {
            string label = "Foliage Rendering";
            string id = "runtime-settings-foliage-rendering";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                GEditorCommon.Header("Grass Meshes");
                GRuntimeSettings.Instance.foliageRendering.grassQuad = EditorGUILayout.ObjectField("Quad", GRuntimeSettings.Instance.foliageRendering.grassQuad, typeof(Mesh), false) as Mesh;
                GRuntimeSettings.Instance.foliageRendering.grassCross = EditorGUILayout.ObjectField("Cross", GRuntimeSettings.Instance.foliageRendering.grassCross, typeof(Mesh), false) as Mesh;
                GRuntimeSettings.Instance.foliageRendering.grassTriCross = EditorGUILayout.ObjectField("TriCross", GRuntimeSettings.Instance.foliageRendering.grassTriCross, typeof(Mesh), false) as Mesh;
                GRuntimeSettings.Instance.foliageRendering.grassClump = EditorGUILayout.ObjectField("Clump", GRuntimeSettings.Instance.foliageRendering.grassClump, typeof(Mesh), false) as Mesh;

                GEditorCommon.Header("Builtin RP");
                GRuntimeSettings.Instance.foliageRendering.treeBillboardMaterial = EditorGUILayout.ObjectField("Tree Billboard", GRuntimeSettings.Instance.foliageRendering.treeBillboardMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.grassMaterial = EditorGUILayout.ObjectField("Grass", GRuntimeSettings.Instance.foliageRendering.grassMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.grassBillboardMaterial = EditorGUILayout.ObjectField("Grass Billboard", GRuntimeSettings.Instance.foliageRendering.grassBillboardMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.grassInteractiveMaterial = EditorGUILayout.ObjectField("Grass Interactive", GRuntimeSettings.Instance.foliageRendering.grassInteractiveMaterial, typeof(Material), false) as Material;

                GEditorCommon.Header("Universal RP");
                GRuntimeSettings.Instance.foliageRendering.urpTreeBillboardMaterial = EditorGUILayout.ObjectField("Tree Billboard", GRuntimeSettings.Instance.foliageRendering.urpTreeBillboardMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.urpGrassMaterial = EditorGUILayout.ObjectField("Grass", GRuntimeSettings.Instance.foliageRendering.urpGrassMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.urpGrassBillboardMaterial = EditorGUILayout.ObjectField("Grass Billboard", GRuntimeSettings.Instance.foliageRendering.urpGrassBillboardMaterial, typeof(Material), false) as Material;
                GRuntimeSettings.Instance.foliageRendering.urpGrassInteractiveMaterial = EditorGUILayout.ObjectField("Grass Interactive", GRuntimeSettings.Instance.foliageRendering.urpGrassInteractiveMaterial, typeof(Material), false) as Material;

                GEditorCommon.Header("Textures");
                GRuntimeSettings.Instance.foliageRendering.windNoiseTexture = GEditorCommon.InlineTexture2DField("Wind Noise Texture", GRuntimeSettings.Instance.foliageRendering.windNoiseTexture, -1);
            });
        }

        private void DrawInternalShaderSettings()
        {
            string label = "Internal Shaders";
            string id = "runtime-settings-internal-shaders";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                SerializedObject so = new SerializedObject(instance);
                SerializedProperty internalShaders = so.FindProperty(nameof(instance.internalShaders));
                EditorGUILayout.PropertyField(internalShaders, new GUIContent("Internal Shaders"), true);
                so.ApplyModifiedProperties();
                internalShaders.Dispose();
                so.Dispose();
            });
        }

        private void DrawDefaultTexturesSettings()
        {
            string label = "Default Textures";
            string id = "runtime-settings-default-texture";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                instance.defaultTextures.redTexture = EditorGUILayout.ObjectField("Red Texture", instance.defaultTextures.redTexture, typeof(Texture2D), false) as Texture2D;
                instance.defaultTextures.blackTexture = EditorGUILayout.ObjectField("Black Texture", instance.defaultTextures.blackTexture, typeof(Texture2D), false) as Texture2D;
            });
        }
    }
}
#endif
