#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Pinwheel.Griffin.Rendering;
using Pinwheel.Griffin.DataTool;
using Unity.Collections;
using System.Text;
using Pinwheel.Griffin.Wizard;
#if __MICROSPLAT_POLARIS__
using JBooth.MicroSplat;
#endif

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GStylizedTerrain))]
    public class GStylizedTerrainInspector : Editor
    {
        public delegate void GTerrainGUIInjectionHandler(GStylizedTerrain terrain, int order);
        public static event GTerrainGUIInjectionHandler GUIInject;

        private GStylizedTerrain terrain;
        private GTerrainData data;
        private bool isNeighboringFoldoutExpanded;

        private void OnEnable()
        {
            terrain = (GStylizedTerrain)target;

            SceneView.duringSceneGui += DuringSceneGUI;
            GLayerInitializer.SetupRaycastLayer();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            int injectGuiIndex = 0;
            InjectGUI(injectGuiIndex++);
            DrawBaseGUI();

            if (terrain.TerrainData == null)
                return;

            InjectGUI(injectGuiIndex++);
            DrawGeometryGUI();
            InjectGUI(injectGuiIndex++);
            DrawShadingGUI();
            InjectGUI(injectGuiIndex++);
            DrawRenderingGUI();
            InjectGUI(injectGuiIndex++);
            DrawFoliageGUI();
            InjectGUI(injectGuiIndex++);
            DrawMaskGUI();
            InjectGUI(injectGuiIndex++);
            DrawDataGUI();
            InjectGUI(injectGuiIndex++);
            DrawNeighboringGUI();
            InjectGUI(injectGuiIndex++);
            DrawStatisticsGUI();
            InjectGUI(injectGuiIndex++);
            DrawStreamingGUI();
            DrawProceduralTerrainGUI();
            InjectGUI(injectGuiIndex++);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(terrain);
                EditorUtility.SetDirty(terrain.TerrainData);
            }
        }

        private class GBaseGUI
        {
            public static readonly GUIContent TERRAIN_DATA = new GUIContent("Terrain Data", "The asset contains Polaris terrain data. Go to Assets> Create> Polaris> TerrainData to create one. This is NOT Unity Terrain Data.");
            public static readonly GUIContent GENERATED_GEOMETRY = new GUIContent("Generated Geometry", "The asset contains generated terrain meshes. It will be created automatically if you don't have one.");
        }

        private void DrawBaseGUI()
        {
            terrain.TerrainData = EditorGUILayout.ObjectField(GBaseGUI.TERRAIN_DATA, terrain.TerrainData, typeof(GTerrainData), false) as GTerrainData;
            data = terrain.TerrainData;
            if (data == null)
                return;

            if (data.Geometry.StorageMode == GGeometry.GStorageMode.SaveToAsset)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(GBaseGUI.GENERATED_GEOMETRY, data.GeometryData, typeof(GTerrainGeneratedData), false);
                GUI.enabled = true;
            }
            else
            {
                data.GeometryData = null;
            }
        }

        private class GGeometryGUI
        {
            public static readonly string LABEL = "Geometry";
            public static readonly string ID = "terrain-data-geometry";
            public static readonly string DEFERRED_UPDATE_KEY = "geometry-deferred-update";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");
            public static readonly GUIContent CONTEXT_UPDATE = new GUIContent("Update");
            public static readonly GUIContent CONTEXT_MATCH_EDGES = new GUIContent("Match Edges");
            public static readonly GUIContent CONTEXT_CLEAN_UP = new GUIContent("Clean Up");
            public static readonly GUIContent CONTEXT_TOGGLE_DEFERRED_UPDATE = new GUIContent("Toggle Deferred Update");
            public static readonly GUIContent CONTEXT_REMOVE_HEIGHT_MAP = new GUIContent("Advanced/Remove Height Map");

            public static readonly string CHUNK_POSITION_WARNING = "- Chunk position placement has been changed for better level streaming and baking. Go to CONTEXT>Update to re-generate the terrain.";
            public static readonly string BURST_WARNING = "- Install Burst Compiler (com.unity.burst) to speed up generation.";
            public static readonly string EDITOR_COROUTINES_WARNING = "- Install Editor Coroutines (com.unity.editorcoroutines) to enable time-sliced generation in editor.";

            public static readonly string HEADER_DIMENSION = "Dimension";
            public static readonly GUIContent WIDTH = new GUIContent("Width", "Size of the terrain on X-axis");
            public static readonly GUIContent HEIGHT = new GUIContent("Height", "Size of the terrain on Y-axis");
            public static readonly GUIContent LENGTH = new GUIContent("Length", "Size of the terrain on Z-axis");

            public static readonly string HEADER_HEIGHT_MAP = "Height Map";
            public static readonly GUIContent HEIGHT_MAP_RESOLUTION = new GUIContent("Height Map Resolution", "Size of the height map in pixel");

            public static readonly string HEADER_MESH_GENERATION = "Mesh Generation";
            public static readonly GUIContent MESH_BASE_RESOLUTION = new GUIContent("Mesh Base Resolution", "Define the triangle density at the least detailed part of the terrain, usually at smooth, flat, less bumpy areas");
            public static readonly GUIContent MESH_RESOLUTION = new GUIContent("Mesh Resolution", "Define the triangle density at the most detailed part of the terrain, usually at rough, bumpy areas");
            public static readonly GUIContent GRID_SIZE = new GUIContent("Grid Size", "Split the terrain into several smaller chunks in a square grid. The total chunk count is GridSize x GridSize");
            public static readonly GUIContent LOD_COUNT = new GUIContent("LOD Count", "Number of LOD for each chunk, should be kept at 1 during level editing for faster processing");
            public static readonly GUIContent DISPLACEMENT_SEED = new GUIContent("Displacement Seed", "Random seed for vertex XZ displacement");
            public static readonly GUIContent DISPLACEMENT_STRENGTH = new GUIContent("Displacement Strength", "Strength of the vertex XZ displacement. Choose an appropriated value to prevent triangles overlapping");
            public static readonly GUIContent ALBEDO_TO_VERTEX_COLOR = new GUIContent("Albedo To Vertex Color", "Choose how to write to vertex color channel from the Albedo map, only use for Vertex Color shading mode");
            public static readonly GUIContent SMOOTH_NORMAL = new GUIContent("Smooth Normal", "Calculate an interpolated normal vector for each vertex, instead of the sharp one");
            public static readonly GUIContent SMOOTH_NORMAL_USE_MASK = new GUIContent("Smooth Normal Use Mask (G)", "Use the G channel of the terrain Mask map to blend between sharp and smooth normal vector");
            public static readonly GUIContent MERGE_UV = new GUIContent("Merge UV", "Merge triangles UV to their midpoint to create sharp looking color");

            public static readonly string HEADER_UTILITIES = "Utilites";
            public static readonly GUIContent STORAGE = new GUIContent("Storage", "Mesh storage mode, write to asset files or clean up and re-generate on enable");
            public static readonly GUIContent TIME_SLICED = new GUIContent("Time Sliced", "If on, terrain generation will be splitted up to multiple frames");

            public static readonly GUIContent UPDATE_BTN = new GUIContent("Update", "Re-generate the terrain");

        }

        private void DrawGeometryGUI()
        {
            bool deferredUpdate = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(GGeometryGUI.DEFERRED_UPDATE_KEY), false);

            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_RESET),
                false,
                () => { ConfirmAndResetGeometry(); });
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_UPDATE),
                false,
                () => { data.Geometry.SetRegionDirty(GCommon.UnitRect); data.SetDirty(GTerrainData.DirtyFlags.Geometry); });
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_MATCH_EDGES),
                false,
                () => { terrain.MatchEdges(); });
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_CLEAN_UP),
                false,
                () => { data.Geometry.CleanUp(); });

            menu.AddSeparator(null);
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_TOGGLE_DEFERRED_UPDATE),
                deferredUpdate,
                () =>
                {
                    deferredUpdate = !deferredUpdate;
                    EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(GGeometryGUI.DEFERRED_UPDATE_KEY), deferredUpdate);
                });
            menu.AddSeparator(null);
            menu.AddItem(
                new GUIContent(GGeometryGUI.CONTEXT_REMOVE_HEIGHT_MAP),
                false,
                () => { ConfirmAndRemoveHeightMap(); });

            List<string> warnings = new List<string>();
            if (terrain != null &&
                terrain.geometryVersion < GStylizedTerrain.GEOMETRY_VERSION_CHUNK_POSITION_AT_CHUNK_CENTER)
            {
                warnings.Add(GGeometryGUI.CHUNK_POSITION_WARNING);
            }

#if !GRIFFIN_BURST
            warnings.Add(GGeometryGUI.BURST_WARNING);
#endif
#if !GRIFFIN_EDITOR_COROUTINES
            if (data.Geometry.AllowTimeSlicedGeneration == true)
            {
                warnings.Add(GGeometryGUI.EDITOR_COROUTINES_WARNING);
            }
#endif

            string headerWarning = GUtilities.ListElementsToString(warnings, "\n");

            GEditorCommon.Foldout(GGeometryGUI.LABEL, false, GGeometryGUI.ID, () =>
            {
                GGeometry settings = data.Geometry;
                EditorGUI.BeginChangeCheck();

                GEditorCommon.Header(GGeometryGUI.HEADER_DIMENSION);
                settings.Width = EditorGUILayout.DelayedFloatField(GGeometryGUI.WIDTH, settings.Width);
                settings.Height = EditorGUILayout.DelayedFloatField(GGeometryGUI.HEIGHT, settings.Height);
                settings.Length = EditorGUILayout.DelayedFloatField(GGeometryGUI.LENGTH, settings.Length);

                GEditorCommon.Header(GGeometryGUI.HEADER_HEIGHT_MAP);
                settings.HeightMapResolution = EditorGUILayout.DelayedIntField(GGeometryGUI.HEIGHT_MAP_RESOLUTION, settings.HeightMapResolution);

                GEditorCommon.Header(GGeometryGUI.HEADER_MESH_GENERATION);
                settings.MeshBaseResolution = EditorGUILayout.DelayedIntField(GGeometryGUI.MESH_BASE_RESOLUTION, settings.MeshBaseResolution);
                settings.MeshResolution = EditorGUILayout.DelayedIntField(GGeometryGUI.MESH_RESOLUTION, settings.MeshResolution);
                settings.ChunkGridSize = EditorGUILayout.DelayedIntField(GGeometryGUI.GRID_SIZE, settings.ChunkGridSize);
                settings.LODCount = EditorGUILayout.DelayedIntField(GGeometryGUI.LOD_COUNT, settings.LODCount);
                settings.DisplacementSeed = EditorGUILayout.DelayedIntField(GGeometryGUI.DISPLACEMENT_SEED, settings.DisplacementSeed);
                settings.DisplacementStrength = EditorGUILayout.DelayedFloatField(GGeometryGUI.DISPLACEMENT_STRENGTH, settings.DisplacementStrength);
                settings.AlbedoToVertexColorMode = (GAlbedoToVertexColorMode)EditorGUILayout.EnumPopup(GGeometryGUI.ALBEDO_TO_VERTEX_COLOR, settings.AlbedoToVertexColorMode);
                settings.SmoothNormal = EditorGUILayout.Toggle(GGeometryGUI.SMOOTH_NORMAL, settings.SmoothNormal);
                if (settings.SmoothNormal)
                {
                    settings.UseSmoothNormalMask = EditorGUILayout.Toggle(GGeometryGUI.SMOOTH_NORMAL_USE_MASK, settings.UseSmoothNormalMask);
                }
                settings.MergeUv = EditorGUILayout.Toggle(GGeometryGUI.MERGE_UV, settings.MergeUv);

                GEditorCommon.Header(GGeometryGUI.HEADER_UTILITIES);
                settings.StorageMode = (GGeometry.GStorageMode)EditorGUILayout.EnumPopup(GGeometryGUI.STORAGE, settings.StorageMode);
                settings.AllowTimeSlicedGeneration = EditorGUILayout.Toggle(GGeometryGUI.TIME_SLICED, settings.AllowTimeSlicedGeneration);

                if (EditorGUI.EndChangeCheck() && !deferredUpdate)
                {
                    data.Geometry.SetRegionDirty(new Rect(0, 0, 1, 1));
                    data.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                }

                if (deferredUpdate)
                {
                    GEditorCommon.Separator();
                    if (GUILayout.Button(GGeometryGUI.UPDATE_BTN))
                    {
                        data.Geometry.SetRegionDirty(new Rect(0, 0, 1, 1));
                        data.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                    }
                }
            },
            menu,
            headerWarning.ToString());
        }

        private class GGeometryDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string RESET_GEOMETRY_TEXT = "Reset geometry data on this terrain? This action cannot be undone!";
            public static readonly string REMOVE_HEIGHT_MAP_TEXT = "Remove the Height Map of this terrain? This action cannot be undone!";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";
        }

        private void ConfirmAndResetGeometry()
        {
            if (EditorUtility.DisplayDialog(
                GGeometryDialogGUI.CONFIRM,
                GGeometryDialogGUI.RESET_GEOMETRY_TEXT,
                GGeometryDialogGUI.OK, GGeometryDialogGUI.CANCEL))
            {
                data.Geometry.ResetFull();
            }
        }

        private void ConfirmAndRemoveHeightMap()
        {
            if (EditorUtility.DisplayDialog(
                GGeometryDialogGUI.CONFIRM,
                GGeometryDialogGUI.REMOVE_HEIGHT_MAP_TEXT,
                GGeometryDialogGUI.OK, GGeometryDialogGUI.CANCEL))
            {
                data.Geometry.RemoveHeightMap();
            }
        }

        private class GShadingGUI
        {
            public static readonly string LABEL = "Shading";
            public static readonly string ID = "terrain-data-shading";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");
            public static readonly GUIContent CONTEXT_REFRESH = new GUIContent("Refresh");
            public static readonly GUIContent CONTEXT_SET_SHADER = new GUIContent("Set Shader");

            public static readonly string CONTEXT_ADVANCED_SEPARATOR = "Advanced/";
            public static readonly GUIContent CONTEXT_SPLAT_TO_ALBEDO = new GUIContent("Advanced/Convert Splats To Albedo");
            public static readonly GUIContent CONTEXT_REMOVE_ALBEDO = new GUIContent("Advanced/Remove Albedo Map");
            public static readonly GUIContent CONTEXT_REMOVE_METALLIC = new GUIContent("Advanced/Remove Metallic Map");
            public static readonly GUIContent CONTEXT_REMOVE_CONTROLS = new GUIContent("Advanced/Remove Splat Control Maps");
            public static readonly GUIContent CONTEXT_REMOVE_LOOKUP = new GUIContent("Advanced/Remove Gradient Lookup Maps");

            public static readonly string HEADER_SHADING_SYSTEM = "System";
            public static readonly GUIContent SHADING_SYSTEM = new GUIContent("Shading System", "Whether to use Polaris built-in shaders or MicroSplat shaders");

            public static readonly string HEADER_MATERIAL_SHADER = "Material & Shader";
            public static readonly GUIContent MATERIAL = new GUIContent("Material", "The material to render the terrain");
            public static readonly GUIContent SHADER = new GUIContent("Shader", "The terrain shader in used");

            public static readonly GUIContent TEXTURE_ARRAY_CONFIG = new GUIContent("Texture Array Config", "The texture array config asset generated from MicroSplat");

            public static readonly string HEADER_COLOR_MAP_GRADIENT_LOOKUP = "Color Map & Gradient Lookup";
            public static readonly GUIContent ALBEDO_MAP_RESOLUTION = new GUIContent("Albedo Map Resolution", "Size of the Albedo Map in pixel");
            public static readonly GUIContent METALLIC_MAP_RESOLUTION = new GUIContent("Metallic Map Resolution", "Size of the Metallic Map in pixel");
            public static readonly GUIContent COLOR_BY_NORMAL = new GUIContent("Color By Normal", "Color strip to shade the terrain based on its normal vector, from perpendicular to parallel to the up vector");
            public static readonly GUIContent BLEND_BY_HEIGHT = new GUIContent("Blend By Height", "A curve to blend between CBH and CBN, where X-axis represent vertex height, Y-axis represent the color blend factor");
            public static readonly GUIContent COLOR_BY_HEIGHT = new GUIContent("Color By Height", "Color strip to shade the terrain based on its height, from 0 to max height");

            public static readonly string HEADER_SPLATS = "Splats";
            public static readonly GUIContent PROTOTYPES = new GUIContent("Prototypes", "The Splat Prototypes Group asset contains splat layers for this terrain. Go to Assets> Create> Polaris> Splat Prototype Group to create one");
            public static readonly GUIContent CONTROL_MAP_RESOLUTION = new GUIContent("Control Map Resolution", "Size of the Splat Control Maps in pixel");

            public static readonly string HEADER_ADVANCED = "Advanced";
            public static readonly string PROPERTIES_NAME_FOLDOUT_KEY = "foldout-shading-properties-name";
            public static readonly GUIContent PROPERTIES_NAME = new GUIContent("Properties Name", "Name of the material properties to bind terrain textures to");
            public static readonly GUIContent PROPS_ALBEDO_MAP = new GUIContent("Albedo Map");
            public static readonly GUIContent PROPS_METALLIC_MAP = new GUIContent("Metallic Map");
            public static readonly GUIContent PROPS_COLOR_BY_HEIGHT = new GUIContent("Color By Height");
            public static readonly GUIContent PROPS_COLOR_BY_NORMAL = new GUIContent("Color By Normal");
            public static readonly GUIContent PROPS_COLOR_BLEND = new GUIContent("Color Blend");
            public static readonly GUIContent PROPS_DIMENSION = new GUIContent("Dimension");
            public static readonly GUIContent PROPS_SPLAT_CONTROL_MAP = new GUIContent("Splat Control Map");
            public static readonly GUIContent PROPS_SPLAT_MAP = new GUIContent("Splat Map");
            public static readonly GUIContent PROPS_SPLAT_NORMAL_MAP = new GUIContent("Splat Normal Map");
            public static readonly GUIContent PROPS_SPLAT_METALLIC = new GUIContent("Splat Metallic");
            public static readonly GUIContent PROPS_SPLAT_SMOOTHNESS = new GUIContent("Splat Smoothness");

        }

        private void DrawShadingGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GShadingGUI.CONTEXT_RESET,
                false,
                () => { data.Shading.ResetFull(); });
            menu.AddItem(
                GShadingGUI.CONTEXT_REFRESH,
                false,
                () => { data.Shading.UpdateMaterials(); });
            menu.AddItem(
                GShadingGUI.CONTEXT_SET_SHADER,
                false,
                () => { GWizardWindow.ShowSetShaderTab(terrain); });

            menu.AddSeparator(null);

            menu.AddItem(
                new GUIContent(GShadingGUI.CONTEXT_SPLAT_TO_ALBEDO),
                false,
                () => { data.Shading.ConvertSplatsToAlbedo(); });
            menu.AddSeparator(GShadingGUI.CONTEXT_ADVANCED_SEPARATOR);
            menu.AddItem(
                new GUIContent(GShadingGUI.CONTEXT_REMOVE_ALBEDO),
                false,
                () => { ConfirmAndRemoveAlbedoMap(); });
            menu.AddItem(
                new GUIContent(GShadingGUI.CONTEXT_REMOVE_METALLIC),
                false,
                () => { ConfirmAndRemoveMetallicMap(); });
            menu.AddItem(
                new GUIContent(GShadingGUI.CONTEXT_REMOVE_CONTROLS),
                false,
                () => { ConfirmAndRemoveControlMaps(); });
            menu.AddItem(
                new GUIContent(GShadingGUI.CONTEXT_REMOVE_LOOKUP),
                false,
                () => { ConfirmAndRemoveGradientLookupMaps(); });

            GEditorCommon.Foldout(GShadingGUI.LABEL, false, GShadingGUI.ID, () =>
            {
                GShading settings = data.Shading;
                EditorGUI.BeginChangeCheck();
#if __MICROSPLAT_POLARIS__
                GEditorCommon.Header(GShadingGUI.HEADER_SHADING_SYSTEM);
                settings.ShadingSystem = (GShadingSystem)EditorGUILayout.EnumPopup(GShadingGUI.SHADING_SYSTEM, settings.ShadingSystem);
#endif

                GEditorCommon.Header(GShadingGUI.HEADER_MATERIAL_SHADER);
                settings.CustomMaterial = EditorGUILayout.ObjectField(GShadingGUI.MATERIAL, settings.CustomMaterial, typeof(Material), false) as Material;
                if (settings.CustomMaterial != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField(GShadingGUI.SHADER, new GUIContent(settings.CustomMaterial.shader.name));
                    GUI.enabled = true;
                }

#if __MICROSPLAT_POLARIS__
                if (settings.ShadingSystem == GShadingSystem.MicroSplat)
                {
                    settings.MicroSplatTextureArrayConfig = EditorGUILayout.ObjectField(GShadingGUI.TEXTURE_ARRAY_CONFIG, settings.MicroSplatTextureArrayConfig, typeof(TextureArrayConfig), false) as TextureArrayConfig;
                }
#endif

                if (settings.ShadingSystem == GShadingSystem.Polaris)
                {
                    GEditorCommon.Header(GShadingGUI.HEADER_COLOR_MAP_GRADIENT_LOOKUP);
                    settings.AlbedoMapResolution = EditorGUILayout.DelayedIntField(GShadingGUI.ALBEDO_MAP_RESOLUTION, settings.AlbedoMapResolution);
                    settings.MetallicMapResolution = EditorGUILayout.DelayedIntField(GShadingGUI.METALLIC_MAP_RESOLUTION, settings.MetallicMapResolution);
                    if (EditorGUI.EndChangeCheck())
                    {
                        data.SetDirty(GTerrainData.DirtyFlags.Shading);
                    }

                    EditorGUI.BeginChangeCheck();
                    settings.ColorByNormal = EditorGUILayout.GradientField(GShadingGUI.COLOR_BY_NORMAL, settings.ColorByNormal);
                    settings.ColorBlendCurve = EditorGUILayout.CurveField(GShadingGUI.BLEND_BY_HEIGHT, settings.ColorBlendCurve, Color.red, GCommon.UnitRect);
                    settings.ColorByHeight = EditorGUILayout.GradientField(GShadingGUI.COLOR_BY_HEIGHT, settings.ColorByHeight);
                    if (EditorGUI.EndChangeCheck())
                    {
                        settings.UpdateLookupTextures();
                        data.SetDirty(GTerrainData.DirtyFlags.Shading);
                    }
                }

                EditorGUI.BeginChangeCheck();
                GEditorCommon.Header(GShadingGUI.HEADER_SPLATS);
                if (settings.ShadingSystem == GShadingSystem.Polaris)
                {
                    settings.Splats = EditorGUILayout.ObjectField(GShadingGUI.PROTOTYPES, settings.Splats, typeof(GSplatPrototypeGroup), false) as GSplatPrototypeGroup;
                }
                settings.SplatControlResolution = EditorGUILayout.DelayedIntField(GShadingGUI.CONTROL_MAP_RESOLUTION, settings.SplatControlResolution);

                if (settings.ShadingSystem == GShadingSystem.Polaris)
                {
                    GEditorCommon.Header(GShadingGUI.HEADER_ADVANCED);
                    DrawAdvancedShading();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    data.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
#if !GRIFFIN_ASE
                GEditorCommon.DrawAffLinks(
                    "Edit terrain shader with Amplify Shader Editor",
                    "https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570");
#endif
#if !__MICROSPLAT_POLARIS__
                GEditorCommon.DrawAffLinks(
                    "Better terrain shader with MicroSplat",
                    "https://assetstore.unity.com/packages/tools/terrain/microsplat-96478",
                    "https://assetstore.unity.com/packages/tools/terrain/microsplat-polaris-integration-166851",
                    "https://assetstore.unity.com/packages/tools/terrain/microsplat-ultimate-bundle-180948");
#endif
            }, menu);
        }

        private void DrawAdvancedShading()
        {
            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey(GShadingGUI.PROPERTIES_NAME_FOLDOUT_KEY);
            bool expanded = SessionState.GetBool(prefKey, false);
            expanded = EditorGUILayout.Foldout(expanded, GShadingGUI.PROPERTIES_NAME);
            SessionState.SetBool(prefKey, expanded);
            if (expanded)
            {
                EditorGUI.indentLevel += 1;

                GShading settings = data.Shading;
                settings.AlbedoMapPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_ALBEDO_MAP, settings.AlbedoMapPropertyName);
                settings.MetallicMapPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_METALLIC_MAP, settings.MetallicMapPropertyName);
                settings.ColorByHeightPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_COLOR_BY_HEIGHT, settings.ColorByHeightPropertyName);
                settings.ColorByNormalPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_COLOR_BY_NORMAL, settings.ColorByNormalPropertyName);
                settings.ColorBlendPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_COLOR_BLEND, settings.ColorBlendPropertyName);
                settings.DimensionPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_DIMENSION, settings.DimensionPropertyName);
                settings.SplatControlMapPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_SPLAT_CONTROL_MAP, settings.SplatControlMapPropertyName);
                settings.SplatMapPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_SPLAT_MAP, settings.SplatMapPropertyName);
                settings.SplatNormalPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_SPLAT_NORMAL_MAP, settings.SplatNormalPropertyName);
                settings.SplatMetallicPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_SPLAT_METALLIC, settings.SplatMetallicPropertyName);
                settings.SplatSmoothnessPropertyName = EditorGUILayout.DelayedTextField(GShadingGUI.PROPS_SPLAT_SMOOTHNESS, settings.SplatSmoothnessPropertyName);

                EditorGUI.indentLevel -= 1;
            }
        }

        private class GShadingDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";

            public static readonly string REMOVE_ALBEDO_TEXT = "Remove the Albedo Map of this terrain? This action cannot be undone!";
            public static readonly string REMOVE_METALLIC_TEXT = "Remove the Metallic Map of this terrain? This action cannot be undone!";
            public static readonly string REMOVE_SPLAT_TEXT = "Remove the Splat Control Maps of this terrain? This action cannot be undone!";
            public static readonly string REMOVE_LOOKUP_TEXT = "Remove the Gradient Lookup Maps of this terrain? This action cannot be undone!";
        }

        private void ConfirmAndRemoveAlbedoMap()
        {
            if (EditorUtility.DisplayDialog(
                GShadingDialogGUI.CONFIRM,
                GShadingDialogGUI.REMOVE_ALBEDO_TEXT,
                GShadingDialogGUI.OK, GShadingDialogGUI.CANCEL))
            {
                data.Shading.RemoveAlbedoMap();
            }
        }

        private void ConfirmAndRemoveMetallicMap()
        {
            if (EditorUtility.DisplayDialog(
                 GShadingDialogGUI.CONFIRM,
                 GShadingDialogGUI.REMOVE_METALLIC_TEXT,
                 GShadingDialogGUI.OK, GShadingDialogGUI.CANCEL))
            {
                data.Shading.RemoveMetallicMap();
            }
        }

        private void ConfirmAndRemoveControlMaps()
        {
            if (EditorUtility.DisplayDialog(
                 GShadingDialogGUI.CONFIRM,
                 GShadingDialogGUI.REMOVE_SPLAT_TEXT,
                 GShadingDialogGUI.OK, GShadingDialogGUI.CANCEL))
            {
                data.Shading.RemoveSplatControlMaps();
            }
        }

        private void ConfirmAndRemoveGradientLookupMaps()
        {
            if (EditorUtility.DisplayDialog(
                 GShadingDialogGUI.CONFIRM,
                 GShadingDialogGUI.REMOVE_LOOKUP_TEXT,
                 GShadingDialogGUI.OK, GShadingDialogGUI.CANCEL))
            {
                data.Shading.RemoveGradientLookupMaps();
            }
        }

        private class GRenderingGUI
        {
            public static readonly string LABEL = "Rendering";
            public static readonly string ID = "terrain-data-rendering";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");

            public static readonly string HEADER_TERRAIN_SHADOW = "Terrain Shadow";
            public static readonly GUIContent CAST_SHADOW = new GUIContent("Cast Shadow", "Should the terrain cast shadow?");
            public static readonly GUIContent RECEIVE_SHADOW = new GUIContent("Receive Shadow", "Should the terrain receive shadow?");

            public static readonly string HEADER_TREE_RENDERING = "Tree Rendering";
            public static readonly GUIContent DRAW_TREES = new GUIContent("Draw", "Toggle tree drawing");
            public static readonly GUIContent ENABLE_INSTANCING = new GUIContent("Enable Instancing", "Toggle instancing for tree rendering");
            public static readonly GUIContent BILLBOARD_START = new GUIContent("Billboard Start", "Minimum distance from the camera where trees begin to be rendered as billboards");
            public static readonly GUIContent TREE_DISTANCE = new GUIContent("Tree Distance", "Maximum distance where trees are visible");
            public static readonly GUIContent TREE_CULL_BIAS = new GUIContent("Cull Bias", "Bias the tree culling to prevent popping shadow");

            public static readonly string HEADER_GRASS_RENDERING = "Grass & Detail Rendering";
            public static readonly GUIContent DRAW_GRASS = new GUIContent("Draw", "Toggle grass & detail rendering");
            public static readonly GUIContent GRASS_DISTANCE = new GUIContent("Grass Distance", "Maximum distance where grasses are visible");
            public static readonly GUIContent FADE_START = new GUIContent("Fade Start", "Relative distance where grass begin to fade out in size");
            public static readonly GUIContent GRASS_CULL_BIAS = new GUIContent("Cull Bias", "Bias the grass culling to prevent popping grass");

            public static readonly string HEADER_TOPOGRAPHIC = "Topographic";
            public static readonly GUIContent ENABLE_TOPOGRAPHIC = new GUIContent("Enable", "Draw a topographic view overlay in the scene view for better sense of elevation");
        }

        private void DrawRenderingGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GRenderingGUI.CONTEXT_RESET,
                false,
                () => { data.Rendering.ResetFull(); });
            GEditorCommon.Foldout(GRenderingGUI.LABEL, false, GRenderingGUI.ID, () =>
            {
                GRendering settings = data.Rendering;
                EditorGUI.BeginChangeCheck();
                GEditorCommon.Header(GRenderingGUI.HEADER_TERRAIN_SHADOW);
                settings.CastShadow = EditorGUILayout.Toggle(GRenderingGUI.CAST_SHADOW, settings.CastShadow);
                settings.ReceiveShadow = EditorGUILayout.Toggle(GRenderingGUI.RECEIVE_SHADOW, settings.ReceiveShadow);

                GEditorCommon.Header(GRenderingGUI.HEADER_TREE_RENDERING);
                settings.DrawTrees = EditorGUILayout.Toggle(GRenderingGUI.DRAW_TREES, settings.DrawTrees);
                GUI.enabled = SystemInfo.supportsInstancing;
                settings.EnableInstancing = EditorGUILayout.Toggle(GRenderingGUI.ENABLE_INSTANCING, settings.EnableInstancing);
                GUI.enabled = true;
                settings.BillboardStart = EditorGUILayout.Slider(GRenderingGUI.BILLBOARD_START, settings.BillboardStart, 0f, GCommon.MAX_TREE_DISTANCE);
                settings.TreeDistance = EditorGUILayout.Slider(GRenderingGUI.TREE_DISTANCE, settings.TreeDistance, 0f, GCommon.MAX_TREE_DISTANCE);
                GRuntimeSettings.Instance.renderingDefault.treeCullBias = EditorGUILayout.Slider(GRenderingGUI.TREE_CULL_BIAS, GRuntimeSettings.Instance.renderingDefault.treeCullBias, 0f, 100f);

                GEditorCommon.Header(GRenderingGUI.HEADER_GRASS_RENDERING);
                settings.DrawGrasses = EditorGUILayout.Toggle(GRenderingGUI.DRAW_GRASS, settings.DrawGrasses);
                settings.GrassDistance = EditorGUILayout.Slider(GRenderingGUI.GRASS_DISTANCE, settings.GrassDistance, 0f, GCommon.MAX_GRASS_DISTANCE);
                settings.GrassFadeStart = EditorGUILayout.Slider(GRenderingGUI.FADE_START, settings.GrassFadeStart, 0f, 1f);
                GRuntimeSettings.Instance.renderingDefault.grassCullBias = EditorGUILayout.Slider(GRenderingGUI.GRASS_CULL_BIAS, GRuntimeSettings.Instance.renderingDefault.grassCullBias, 0f, 100f);

                if (EditorGUI.EndChangeCheck())
                {
                    data.SetDirty(GTerrainData.DirtyFlags.Rendering);
                    EditorUtility.SetDirty(GRuntimeSettings.Instance);
                    if (settings.EnableInstancing)
                    {
                    }
                }

                GEditorCommon.Header(GRenderingGUI.HEADER_TOPOGRAPHIC);
                GEditorSettings.Instance.topographic.enable = EditorGUILayout.Toggle(GRenderingGUI.ENABLE_TOPOGRAPHIC, GEditorSettings.Instance.topographic.enable);

#if !GRIFFIN_VEGETATION_STUDIO_PRO
                GEditorCommon.DrawAffLinks(
                    "Better vegetation spawning & rendering with Vegetation Studio Pro",
                    "https://assetstore.unity.com/packages/tools/terrain/vegetation-studio-pro-131835");
#endif
            }, menu);
        }

        private class GFoliageGUI
        {
            public static readonly string LABEL = "Foliage";
            public static readonly string ID = "terrain-data-foliage";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");
            public static readonly GUIContent CONTEXT_REFRESH = new GUIContent("Refresh");
            public static readonly GUIContent CONTEXT_CLEAR_TREES = new GUIContent("Clear All Trees");
            public static readonly GUIContent CONTEXT_CLEAR_GRASSES = new GUIContent("Clear All Grasses");
            public static readonly GUIContent CONTEXT_UPDATE_TREES = new GUIContent("Update Trees");
            public static readonly GUIContent CONTEXT_UPDATE_GRASSES = new GUIContent("Update Grasses");
            public static readonly GUIContent CONTEXT_UPDATE_GRASS_VERSION = new GUIContent("Update Grass Serialize Version");
            public static readonly string GRASS_VERSION_WARNING = "New grass serialize version is available, use context menu to upgrade (Recommended).";

            public static readonly string HEADER_TREES = "Trees";
            public static readonly GUIContent TREE_PROTOTYPES = new GUIContent("Prototypes", "The Tree Prototype Group Asset contains tree types to render on this terrain. Go to Assets> Create> Polaris> Tree Prototype Group to create one");
            public static readonly GUIContent TREE_SNAP_MODE = new GUIContent("Snap Mode", "Whether to snap trees to the terrain or world objects");
            public static readonly GUIContent TREE_SNAP_LAYERS = new GUIContent("Snap Layers", "Game object layers to snap trees on");
            public static readonly GUIContent TREE_INSTANCE_COUNT = new GUIContent("Instance Count", "Total tree instance of this terrain");

            public static readonly string HEADER_GRASSES = "Grasses & Details";
            public static readonly GUIContent GRASS_PROTOTYPES = new GUIContent("Prototypes", "The Grass Prototype Group Asset contains grass types to render on this terrain. Go to Assets> Create> Polaris> Grass Prototype Group to create one");
            public static readonly GUIContent PATCH_GRID_SIZE = new GUIContent("Patch Grid Size", "Divide grasses into several patches for culling and rendering");
            public static readonly GUIContent GRASS_SNAP_MODE = new GUIContent("Snap Mode", "Whether to snap grasses to the terrain or world objects");
            public static readonly GUIContent GRASS_SNAP_LAYERS = new GUIContent("Snap Layers", "Game object layers to snap grasses on");
            public static readonly GUIContent INTERACTIVE_GRASS = new GUIContent("Interactive Grass", "Toggle grass bending when the player passes by. You also need to add a GInteractiveGrassAgent component to your character");
            public static readonly GUIContent VECTOR_FIELD_MAP_RESOLUTION = new GUIContent("Vector Field Map Resolution", "Size of the vector field map in pixel");
            public static readonly GUIContent BEND_SENSITIVE = new GUIContent("Bend Sensitive", "How fast the grass bend down when the player passes by");
            public static readonly GUIContent RESTORE_SENSITIVE = new GUIContent("Restore Sensive", "How fast the grass get back to its initial shape when the player goes away");

            public static readonly GUIContent GRASS_INSTANCE_COUNT = new GUIContent("Instance Count", "Total grass instance of this terrain");
        }

        private void DrawFoliageGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GFoliageGUI.CONTEXT_RESET,
                false,
                () => { ConfirmAndResetFoliage(); });
            menu.AddItem(
                GFoliageGUI.CONTEXT_REFRESH,
                false,
                () => { data.Foliage.Refresh(); });
            menu.AddItem(
                GFoliageGUI.CONTEXT_CLEAR_TREES,
                false,
                () => { ConfirmAndClearAllTrees(); });
            menu.AddItem(
                GFoliageGUI.CONTEXT_CLEAR_GRASSES,
                false,
                () => { ConfirmAndClearAllGrasses(); });

            menu.AddSeparator(null);
            menu.AddItem(
                GFoliageGUI.CONTEXT_UPDATE_TREES,
                false,
                () =>
                {
                    if (terrain.TerrainData != null)
                    {
                        terrain.TerrainData.Foliage.SetTreeRegionDirty(new Rect(0, 0, 1, 1));
                        terrain.UpdateTreesPosition();
                        terrain.TerrainData.Foliage.ClearTreeDirtyRegions();
                        terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                    }
                });
            menu.AddItem(
                GFoliageGUI.CONTEXT_UPDATE_GRASSES,
                false,
                () =>
                {
                    if (terrain.TerrainData != null)
                    {
                        terrain.TerrainData.Foliage.SetGrassRegionDirty(new Rect(0, 0, 1, 1));
                        terrain.UpdateGrassPatches();
                        terrain.TerrainData.Foliage.ClearGrassDirtyRegions();
                        terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                    }
                });

            GFoliage settings = data.Foliage;
            bool showUpgradeMessage = settings.grassVersion < GFoliage.GRASS_VERSION_COMPRESSED;
            if (showUpgradeMessage)
            {
                menu.AddSeparator(null);
                menu.AddItem(
                    GFoliageGUI.CONTEXT_UPDATE_GRASS_VERSION,
                    false,
                    () =>
                    {
                        settings.Internal_UpgradeGrassSerializeVersion();
                    });
            }

            string headerWarning = null;
            if (showUpgradeMessage)
            {
                headerWarning = GFoliageGUI.GRASS_VERSION_WARNING;
            }

            GEditorCommon.Foldout(GFoliageGUI.LABEL, false, GFoliageGUI.ID, () =>
            {
                EditorGUI.BeginChangeCheck();
                GEditorCommon.Header(GFoliageGUI.HEADER_TREES);
                settings.Trees = EditorGUILayout.ObjectField(GFoliageGUI.TREE_PROTOTYPES, settings.Trees, typeof(GTreePrototypeGroup), false) as GTreePrototypeGroup;
                settings.TreeSnapMode = (GSnapMode)EditorGUILayout.EnumPopup(GFoliageGUI.TREE_SNAP_MODE, settings.TreeSnapMode);
                if (settings.TreeSnapMode == GSnapMode.World)
                {
                    settings.TreeSnapLayerMask = GEditorCommon.LayerMaskField(GFoliageGUI.TREE_SNAP_LAYERS, settings.TreeSnapLayerMask);
                }

                GUI.enabled = false;
                EditorGUILayout.LabelField(GFoliageGUI.TREE_INSTANCE_COUNT, new GUIContent(settings.TreeInstances.Count.ToString()));
                GUI.enabled = true;
                if (EditorGUI.EndChangeCheck())
                {
                    data.SetDirty(GTerrainData.DirtyFlags.Foliage);
                }

                EditorGUI.BeginChangeCheck();
                GEditorCommon.Header(GFoliageGUI.HEADER_GRASSES);
                settings.Grasses = EditorGUILayout.ObjectField(GFoliageGUI.GRASS_PROTOTYPES, settings.Grasses, typeof(GGrassPrototypeGroup), false) as GGrassPrototypeGroup;
                settings.PatchGridSize = EditorGUILayout.DelayedIntField(GFoliageGUI.PATCH_GRID_SIZE, settings.PatchGridSize);

                settings.GrassSnapMode = (GSnapMode)EditorGUILayout.EnumPopup(GFoliageGUI.GRASS_SNAP_MODE, settings.GrassSnapMode);
                if (settings.GrassSnapMode == GSnapMode.World)
                {
                    settings.GrassSnapLayerMask = GEditorCommon.LayerMaskField(GFoliageGUI.GRASS_SNAP_LAYERS, settings.GrassSnapLayerMask);
                }
                settings.EnableInteractiveGrass = EditorGUILayout.Toggle(GFoliageGUI.INTERACTIVE_GRASS, settings.EnableInteractiveGrass);
                if (settings.EnableInteractiveGrass)
                {
                    settings.VectorFieldMapResolution = EditorGUILayout.DelayedIntField(GFoliageGUI.VECTOR_FIELD_MAP_RESOLUTION, settings.VectorFieldMapResolution);
                    settings.BendSensitive = EditorGUILayout.Slider(GFoliageGUI.BEND_SENSITIVE, settings.BendSensitive, 0f, 1f);
                    settings.RestoreSensitive = EditorGUILayout.Slider(GFoliageGUI.RESTORE_SENSITIVE, settings.RestoreSensitive, 0f, 1f);
                }

                GUI.enabled = false;
                EditorGUILayout.LabelField(GFoliageGUI.GRASS_INSTANCE_COUNT, new GUIContent(settings.GrassInstanceCount.ToString()));

                GUI.enabled = true;
                if (EditorGUI.EndChangeCheck())
                {
                    data.SetDirty(GTerrainData.DirtyFlags.Foliage);
                    if (settings.EnableInteractiveGrass)
                    {
                    }
                }

                GEditorCommon.DrawAffLinks(
                    "Find the best vegetation assets for your project",
                    "https://assetstore.unity.com/packages/3d/vegetation/trees/polygon-nature-low-poly-3d-art-by-synty-120152",
                    "https://assetstore.unity.com/lists/stylized-vegetation-120082",
                    "https://assetstore.unity.com/lists/stylized-rock-props-120083");
            },
            menu,
            headerWarning);
        }

        private class GFoliageDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";

            public static readonly string RESET_FOLIAGE_TEXT = "Reset foliage data on this terrain? This action cannot be undone!";
            public static readonly string CLEAR_TREES_TEXT = "Clear all trees on this terrain? This action cannot be undone!";
            public static readonly string CLEAR_GRASSES_TEXT = "Clear all grasses on this terrain? This action cannot be undone!";
        }

        private void ConfirmAndResetFoliage()
        {
            if (EditorUtility.DisplayDialog(
                GFoliageDialogGUI.CONFIRM,
                GFoliageDialogGUI.RESET_FOLIAGE_TEXT,
                GFoliageDialogGUI.OK, GFoliageDialogGUI.CANCEL))
            {
                data.Foliage.ResetFull();
            }
        }

        private void ConfirmAndClearAllTrees()
        {
            if (EditorUtility.DisplayDialog(
                GFoliageDialogGUI.CONFIRM,
                GFoliageDialogGUI.CLEAR_TREES_TEXT,
                GFoliageDialogGUI.OK, GFoliageDialogGUI.CANCEL))
            {
                data.Foliage.ClearTreeInstances();
            }
        }

        private void ConfirmAndClearAllGrasses()
        {
            if (EditorUtility.DisplayDialog(
                GFoliageDialogGUI.CONFIRM,
                GFoliageDialogGUI.CLEAR_GRASSES_TEXT,
                GFoliageDialogGUI.OK, GFoliageDialogGUI.CANCEL))
            {
                data.Foliage.ClearGrassInstances();
            }
        }

        private class GMaskGUI
        {
            public static readonly string LABEL = "Mask";
            public static readonly string ID = "terrain-data-mask";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");
            public static readonly GUIContent CONTEXT_REMOVE_MASK_MAP = new GUIContent("Advanced/Remove Mask Map");
            public static readonly GUIContent MASK_RESOLUTION = new GUIContent("Resolution", "Size of the Mask Map in pixel");

            public static readonly string HEADER_MASK_USAGE = "Mask Usage";
            public static readonly string R = "R";
            public static readonly string R_TEXT = "Lock regions from editing.";
            public static readonly string G = "G";
            public static readonly string G_TEXT = "Sharp/Smooth normals blend factor.";
            public static readonly string B = "B";
            public static readonly string B_TEXT = "Water source for Hydraulic Erosion";
            public static readonly string A = "A";
            public static readonly string A_TEXT = "Custom";
        }

        private void DrawMaskGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GMaskGUI.CONTEXT_RESET,
                false,
                () => { ConfirmAndResetMask(); });
            menu.AddSeparator(null);
            menu.AddItem(
                GMaskGUI.CONTEXT_REMOVE_MASK_MAP,
                false,
                () => { ConfirmAndRemoveMaskMap(); });

            GEditorCommon.Foldout(GMaskGUI.LABEL, false, GMaskGUI.ID, () =>
            {
                GMask mask = data.Mask;
                mask.MaskMapResolution = EditorGUILayout.DelayedIntField(GMaskGUI.MASK_RESOLUTION, mask.MaskMapResolution);

                GEditorCommon.Header(GMaskGUI.HEADER_MASK_USAGE);
                EditorGUILayout.LabelField(GMaskGUI.R, GMaskGUI.R_TEXT);
                EditorGUILayout.LabelField(GMaskGUI.G, GMaskGUI.G_TEXT);
                EditorGUILayout.LabelField(GMaskGUI.B, GMaskGUI.B_TEXT);
                EditorGUILayout.LabelField(GMaskGUI.A, GMaskGUI.A_TEXT);
            },
            menu);
        }

        private class GMaskDialogGUI
        {
            public static readonly string CONFIRM = "Confirm";
            public static readonly string OK = "OK";
            public static readonly string CANCEL = "Cancel";
            public static readonly string RESET_TEXT = "Reset Mask settings on this terrain? This action cannot be undone!";
            public static readonly string REMOVE_MASK_TEXT = "Remove the Mask Map of this terrain? This action cannot be undone!";
        }

        private void ConfirmAndResetMask()
        {
            if (EditorUtility.DisplayDialog(
                GMaskDialogGUI.CONFIRM,
                GMaskDialogGUI.RESET_TEXT,
                GMaskDialogGUI.OK, GMaskDialogGUI.CANCEL))
            {
                data.Mask.ResetFull();
            }

        }

        private void ConfirmAndRemoveMaskMap()
        {
            if (EditorUtility.DisplayDialog(
                GMaskDialogGUI.CONFIRM,
                GMaskDialogGUI.REMOVE_MASK_TEXT,
                GMaskDialogGUI.OK, GMaskDialogGUI.CANCEL))
            {
                data.Mask.RemoveMaskMap();
            }
        }

        private class GDataGUI
        {
            public static readonly string LABEL = "Data";
            public static readonly string ID = "terrain-data-data";
            public static readonly GUIContent IMPORT = new GUIContent("Import");
            public static readonly GUIContent EXPORT = new GUIContent("Export");

            public static readonly GUIContent UNITY_TERRAIN_DATA = new GUIContent("Unity Terrain Data");
            public static readonly GUIContent RAW = new GUIContent("Raw");
            public static readonly GUIContent TEXTURES = new GUIContent("Textures");
        }

        private void DrawDataGUI()
        {
            GEditorCommon.Foldout(GDataGUI.LABEL, false, GDataGUI.ID, () =>
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(GDataGUI.IMPORT, EditorStyles.miniButtonLeft))
                {
                    ShowImportContext();
                }
                if (GUILayout.Button(GDataGUI.EXPORT, EditorStyles.miniButtonRight))
                {
                    ShowExportContext();
                }
                EditorGUILayout.EndHorizontal();
            });
        }

        private void ShowImportContext()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GDataGUI.UNITY_TERRAIN_DATA,
                false,
                () =>
                {
                    ShowUnityTerrainDataImporter();
                });
            menu.AddItem(
                GDataGUI.RAW,
                false,
                () =>
                {
                    ShowRawImporter();
                });
            menu.AddItem(
                GDataGUI.TEXTURES,
                false,
                () =>
                {
                    ShowTextureImporter();
                });

            menu.ShowAsContext();
        }

        private void ShowUnityTerrainDataImporter()
        {
            GUnityTerrainDataImporterWindow window = GUnityTerrainDataImporterWindow.ShowWindow();
            window.DesData = data;

            GameObject g = Selection.activeGameObject;
            if (g != null)
            {
                GStylizedTerrain t = g.GetComponent<GStylizedTerrain>();
                if (t != null && t.TerrainData == data)
                {
                    window.DesTerrain = t;
                }
            }
        }

        private void ShowRawImporter()
        {
            GRawImporterWindow window = GRawImporterWindow.ShowWindow();
            window.DesData = data;
            GameObject g = Selection.activeGameObject;
            if (g != null)
            {
                GStylizedTerrain t = g.GetComponent<GStylizedTerrain>();
                if (t != null && t.TerrainData == data)
                {
                    window.Terrain = t;
                }
            }
        }

        private void ShowTextureImporter()
        {
            GTextureImporterWindow window = GTextureImporterWindow.ShowWindow();
            window.DesData = data;
            GameObject g = Selection.activeGameObject;
            if (g != null)
            {
                GStylizedTerrain t = g.GetComponent<GStylizedTerrain>();
                if (t != null && t.TerrainData == data)
                {
                    window.Terrain = t;
                }
            }
        }

        private void ShowExportContext()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GDataGUI.UNITY_TERRAIN_DATA,
                false,
                () =>
                {
                    ShowUnityTerrainDataExporter();
                });
            menu.AddItem(
                GDataGUI.RAW,
                false,
                () =>
                {
                    ShowRawExporter();
                });
            menu.AddItem(
                GDataGUI.TEXTURES,
                false,
                () =>
                {
                    ShowTexturesExporter();
                });

            menu.ShowAsContext();
        }

        private void ShowUnityTerrainDataExporter()
        {
            GUnityTerrainDataExporterWindow window = GUnityTerrainDataExporterWindow.ShowWindow();
            window.SrcData = data;
        }

        private void ShowRawExporter()
        {
            GRawExporterWindow window = GRawExporterWindow.ShowWindow();
            window.SrcData = data;
        }

        private void ShowTexturesExporter()
        {
            GTextureExporterWindow window = GTextureExporterWindow.ShowWindow();
            window.SrcData = data;
        }

        private class GNeighboringGUI
        {
            public static readonly string LABEL = "Neighboring";
            public static readonly string ID = "terrain-neighboring";

            public static readonly GUIContent CONTEXT_RESET = new GUIContent("Reset");
            public static readonly GUIContent CONTEXT_CONNECT = new GUIContent("Connect");

            public static readonly GUIContent AUTO_CONNECT = new GUIContent("Auto Connect", "Auto connect this terrain with nearby tiles");
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the group which this terrain is belong too. This Id is used for Auto Connect and other terrain tools");
            public static readonly GUIContent TOP_NEIGHBOR = new GUIContent("Top Neighbor", "The neighbor terrain on +Z direction");
            public static readonly GUIContent BOTTOM_NEIGHBOR = new GUIContent("Bottom Neighbor", "The neighbor terrain on -Z direction");
            public static readonly GUIContent LEFT_NEIGHBOR = new GUIContent("Left Neighbor", "The neighbor terrain on -X direction");
            public static readonly GUIContent RIGHT_NEIGHBOR = new GUIContent("Right Neighbor", "The neighbor terrain on +X direction");
        }

        private void DrawNeighboringGUI()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                GNeighboringGUI.CONTEXT_RESET,
                false,
                () => { ResetNeighboring(); });
            menu.AddItem(
                GNeighboringGUI.CONTEXT_CONNECT,
                false,
                () => { GStylizedTerrain.ConnectAdjacentTiles(); });


            isNeighboringFoldoutExpanded = GEditorCommon.Foldout(GNeighboringGUI.LABEL, false, GNeighboringGUI.ID, () =>
             {
                 EditorGUI.BeginChangeCheck();
                 terrain.AutoConnect = EditorGUILayout.Toggle(GNeighboringGUI.AUTO_CONNECT, terrain.AutoConnect);
                 terrain.GroupId = EditorGUILayout.DelayedIntField(GNeighboringGUI.GROUP_ID, terrain.GroupId);
                 terrain.TopNeighbor = EditorGUILayout.ObjectField(GNeighboringGUI.TOP_NEIGHBOR, terrain.TopNeighbor, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                 GUI.enabled = false;
                 EditorGUILayout.ObjectField(" ", terrain.TopTerrainData, typeof(GTerrainData), false);
                 GUI.enabled = true;

                 terrain.BottomNeighbor = EditorGUILayout.ObjectField(GNeighboringGUI.BOTTOM_NEIGHBOR, terrain.BottomNeighbor, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                 GUI.enabled = false;
                 EditorGUILayout.ObjectField(" ", terrain.BottomTerrainData, typeof(GTerrainData), false);
                 GUI.enabled = true;

                 terrain.LeftNeighbor = EditorGUILayout.ObjectField(GNeighboringGUI.LEFT_NEIGHBOR, terrain.LeftNeighbor, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                 GUI.enabled = false;
                 EditorGUILayout.ObjectField(" ", terrain.LeftTerrainData, typeof(GTerrainData), false);
                 GUI.enabled = true;

                 terrain.RightNeighbor = EditorGUILayout.ObjectField(GNeighboringGUI.RIGHT_NEIGHBOR, terrain.RightNeighbor, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                 GUI.enabled = false;
                 EditorGUILayout.ObjectField(" ", terrain.RightTerrainData, typeof(GTerrainData), false);
                 GUI.enabled = true;

                 if (EditorGUI.EndChangeCheck())
                 {
                     if (terrain.TopNeighbor != null || terrain.BottomNeighbor != null || terrain.LeftNeighbor != null || terrain.RightNeighbor != null)
                     {
                     }
                 }
             }, menu);
        }

        private class GStatsGUI
        {
            public static readonly string LABEL = "Statistics";
            public static readonly string ID = "terrain-statistics";

            public static readonly string HEADER_TEXTURES_MEMORY = "Textures Memory";
            public static readonly string HEIGHT_MAP = "Height Map";
            public static readonly string ALBEDO_MAP = "Albedo Map";
            public static readonly string METALLIC_MAP = "Metallic Map";
            public static readonly string SPLAT_CONTROL_MAP = "Splat Control Maps";
            public static readonly string GRADIENT_LOOKUP_MAP = "Gradient Lookup Maps";
            public static readonly string MASK_MAP = "Mask Map";

            public static readonly string HEADER_PERSISTENT_FOLIAGE_MEMORY = "Persistent Foliage Memory";
            public static readonly string TREE = "Tree";
            public static readonly string GRASS = "Grass";

            public static readonly string HEADER_TOTAL_MEMORY = "Total Memory";
            public static readonly string TOTAL = "Total";

            public static readonly string HEADER_NOTE = "Note";
            public static readonly string NOTE = "Terrain Data file on disk may take up more space. Set project serialization mode to Binary yields smaller file, but cannot be diff/merged using version control softwares.";

            public static readonly string MEM_FORMAT = "0 KB";
        }

        private void DrawStatisticsGUI()
        {
            GEditorCommon.Foldout(GStatsGUI.LABEL, false, GStatsGUI.ID, () =>
            {
                float kiloToByte = 1024;

                GEditorCommon.Header(GStatsGUI.HEADER_TEXTURES_MEMORY);
                float heightMapStats = data.Geometry.GetHeightMapMemoryStats();
                float albedoMapStats = data.Shading.GetAlbedoMapMemStats();
                float metallicMapStats = data.Shading.GetMetallicMapMemStats();
                float splatControlMapsStats = data.Shading.GetControlMapMemStats();
                float lookupMapsStats = data.Shading.GetLookupTexturesMemStats();
                float maskMapStats = data.Mask.GetMaskMapMemStats();
                EditorGUILayout.LabelField(GStatsGUI.HEIGHT_MAP, (heightMapStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.ALBEDO_MAP, (albedoMapStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.METALLIC_MAP, (metallicMapStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.SPLAT_CONTROL_MAP, (splatControlMapsStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.GRADIENT_LOOKUP_MAP, (lookupMapsStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.MASK_MAP, (maskMapStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));

                GEditorCommon.Header(GStatsGUI.HEADER_PERSISTENT_FOLIAGE_MEMORY);
                float treeStats = data.Foliage.GetTreeMemStats();
                float grassStats = data.Foliage.GetGrassMemStats();
                EditorGUILayout.LabelField(GStatsGUI.TREE, (treeStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));
                EditorGUILayout.LabelField(GStatsGUI.GRASS, (grassStats / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));

                GEditorCommon.Header(GStatsGUI.HEADER_TOTAL_MEMORY);
                float total = heightMapStats + albedoMapStats + metallicMapStats + splatControlMapsStats + lookupMapsStats + maskMapStats + treeStats + grassStats;
                EditorGUILayout.LabelField(GStatsGUI.TOTAL, (total / kiloToByte).ToString(GStatsGUI.MEM_FORMAT));

                GEditorCommon.Header(GStatsGUI.HEADER_NOTE);
                EditorGUILayout.LabelField(GStatsGUI.NOTE, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private class GStreamingGUI
        {
            public static readonly string LABEL = "Streaming";
            public static readonly string ID = "terrain-streaming";
        }

        private void DrawStreamingGUI()
        {
            if (!GPackageInitializer.isWorldStreamer2Installed)
            {
                GEditorCommon.Foldout(GStreamingGUI.LABEL, false, GStreamingGUI.ID, () =>
                {
                    GEditorCommon.DrawAffLinks(
                        "Enable level streaming with World Streamer 2",
                        "https://assetstore.unity.com/packages/tools/terrain/world-streamer-2-176482");
                });
            }
        }

        private class GProcTerrainGUI
        {
            public static readonly string LABEL = "Procedural Terrain";
            public static readonly string ID = "procedural-terrain";
        }

        private void DrawProceduralTerrainGUI()
        {
            if (!GPackageInitializer.isVistaInstalled)
            {
                GEditorCommon.Foldout(GProcTerrainGUI.LABEL, true, GProcTerrainGUI.ID, () =>
                {
                    EditorGUILayout.LabelField("Vista is an advanced toolset for procedural terrain creation that works perfectly with Polaris.");                    
                    GEditorCommon.DrawAffLinks(
                        "Generate beautiful terrain with Vista",
                        "https://assetstore.unity.com/packages/tools/terrain/vista-advanced-terrain-graph-editor-210496");
                });
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private class GSceneViewGUI
        {
            public static readonly GUIContent MOUSE_MESSAGE = new GUIContent(
                "Click on a rectangle to pin.\n" +
                "Close the Neighboring foldout to disable terrain pinning mode.");
        }

        private void DuringSceneGUI(SceneView sv)
        {
            if (terrain.TerrainData == null)
                return;
            if (!terrain.AutoConnect)
                return;
            if (!isNeighboringFoldoutExpanded)
                return;

            Vector3 terrainSizeXZ = new Vector3(
                terrain.TerrainData.Geometry.Width,
                0,
                terrain.TerrainData.Geometry.Length);

            if (terrain.LeftNeighbor == null)
            {
                Vector3 pos = terrain.transform.position + Vector3.left * terrainSizeXZ.x + terrainSizeXZ * 0.5f;
                if (Handles.Button(pos, Quaternion.Euler(90, 0, 0), terrainSizeXZ.z * 0.5f, terrainSizeXZ.z * 0.5f, Handles.RectangleHandleCap) && Event.current.button == 0)
                {
                    GStylizedTerrain t = CreateNeighborTerrain();
                    t.transform.parent = terrain.transform.parent;
                    t.transform.position = terrain.transform.position + Vector3.left * terrainSizeXZ.x;
                    t.name = string.Format("{0}-{1}", t.name, t.transform.position.ToString());
                    Selection.activeGameObject = t.gameObject;
                    GStylizedTerrain.ConnectAdjacentTiles();
                }
            }
            if (terrain.TopNeighbor == null)
            {
                Vector3 pos = terrain.transform.position + Vector3.forward * terrainSizeXZ.z + terrainSizeXZ * 0.5f;
                if (Handles.Button(pos, Quaternion.Euler(90, 0, 0), terrainSizeXZ.z * 0.5f, terrainSizeXZ.z * 0.5f, Handles.RectangleHandleCap) && Event.current.button == 0)
                {
                    GStylizedTerrain t = CreateNeighborTerrain();
                    t.transform.parent = terrain.transform.parent;
                    t.transform.position = terrain.transform.position + Vector3.forward * terrainSizeXZ.z;
                    t.name = string.Format("{0}-{1}", t.name, t.transform.position.ToString());
                    Selection.activeGameObject = t.gameObject;
                    GStylizedTerrain.ConnectAdjacentTiles();
                }
            }
            if (terrain.RightNeighbor == null)
            {
                Vector3 pos = terrain.transform.position + Vector3.right * terrainSizeXZ.z + terrainSizeXZ * 0.5f;
                if (Handles.Button(pos, Quaternion.Euler(90, 0, 0), terrainSizeXZ.z * 0.5f, terrainSizeXZ.z * 0.5f, Handles.RectangleHandleCap) && Event.current.button == 0)
                {
                    GStylizedTerrain t = CreateNeighborTerrain();
                    t.transform.parent = terrain.transform.parent;
                    t.transform.position = terrain.transform.position + Vector3.right * terrainSizeXZ.x;
                    t.name = string.Format("{0}-{1}", t.name, t.transform.position.ToString());
                    Selection.activeGameObject = t.gameObject;
                    GStylizedTerrain.ConnectAdjacentTiles();
                }
            }
            if (terrain.BottomNeighbor == null)
            {
                Vector3 pos = terrain.transform.position + Vector3.back * terrainSizeXZ.z + terrainSizeXZ * 0.5f;
                if (Handles.Button(pos, Quaternion.Euler(90, 0, 0), terrainSizeXZ.z * 0.5f, terrainSizeXZ.z * 0.5f, Handles.RectangleHandleCap) && Event.current.button == 0)
                {
                    GStylizedTerrain t = CreateNeighborTerrain();
                    t.transform.parent = terrain.transform.parent;
                    t.transform.position = terrain.transform.position + Vector3.back * terrainSizeXZ.z;
                    t.name = string.Format("{0}-{1}", t.name, t.transform.position.ToString());
                    Selection.activeGameObject = t.gameObject;
                    GStylizedTerrain.ConnectAdjacentTiles();
                }
            }

            GEditorCommon.SceneViewMouseMessage(GSceneViewGUI.MOUSE_MESSAGE);
        }

        private GStylizedTerrain CreateNeighborTerrain()
        {
            GStylizedTerrain t = GWizard.CreateTerrainFromSource(terrain.TerrainData);
            GEditorCommon.ExpandFoldout(GNeighboringGUI.ID);

            return t;
        }

        private void ResetNeighboring()
        {
            terrain.AutoConnect = true;
            terrain.GroupId = 0;
            terrain.ResetNeighboring();
        }

        private void InjectGUI(int order)
        {
            if (GUIInject != null)
            {
                GUIInject.Invoke(terrain, order);
            }
        }
    }
}
#endif
