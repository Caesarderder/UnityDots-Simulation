#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.TouchReact;
using AwesomeTechnologies.ColliderSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.PrefabSpawner;

namespace Pinwheel.Griffin.VegetationStudioPro
{
    public class GVSPIntegrationEditor : EditorWindow
    {
        [MenuItem("Window/Polaris/Tools/Vegetation Studio Pro Integration")]
        public static void ShowWindow()
        {
            GVSPIntegrationEditor window = GVSPIntegrationEditor.GetWindow<GVSPIntegrationEditor>();
            window.titleContent = new GUIContent("VSP Integration");
            window.Show();
        }

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 250;
            DrawQuickSetupGUI();
            DrawImportPolarisFoliageGUI();
            DrawMaskPaintingGUI();
            DrawSplineMaskingGUI();
        }

        public void DrawQuickSetupGUI()
        {
            string label = "Quick Setup";
            string id = "quick-setup-vsp-editor";
            GEditorCommon.Foldout(label, true, id, () =>
            {
                const string INSTRUCTION = "Quickly setup Mesh Terrain components for each terrains and create Vegetation Studio Manager instance.";
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);

                GVSPSettings.GQuickSetupSettings settings = GVSPSettings.Instance.QuickSetup;
                GUIContent createVegetationContent = new GUIContent(
                    "Create Vegetation Package",
                    "Create a new Vegetation Package from all foliage prototypes in the scene. All prototype will be merged into one package.");
                settings.CreateVegetationPackage = EditorGUILayout.Toggle(createVegetationContent, settings.CreateVegetationPackage);

                GUIContent createStorageContent = new GUIContent(
                    "Create Persistent Storage",
                    "Create a new Persistent Storage asset, useful if you want to import spawned foliage from Polaris to Vegetation Studio Pro.");
                settings.CreatePersistentStorage = EditorGUILayout.Toggle(createStorageContent, settings.CreatePersistentStorage);

                GVSPSettings.Instance.QuickSetup = settings;

                if (GUILayout.Button("Setup"))
                {
                    GVSPIntegration.SetupMeshTerrainsAndVSPManager();
                }
            });
        }

        public void DrawImportPolarisFoliageGUI()
        {
            string label = "Import Polaris Foliage to Persistent Vegetation Storage";
            string id = "import-polaris-foliage-vsp-editor";
            GEditorCommon.Foldout(label, false, id, () =>
            {
                const string INSTRUCTION = "Import foliage instances from Polaris to Vegetation Studio Pro as persistent data.\n" +
                "Instances that share the same texture or prefab will be merged into one vegetation type.\n" +
                "Requires Vegetation Package to be setup beforehand.";
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
                GVSPSettings.GImportSettings settings = GVSPSettings.Instance.Import;
                settings.VSManager = EditorGUILayout.ObjectField("Vegetation Studio Manager", settings.VSManager, typeof(VegetationStudioManager), true) as VegetationStudioManager;
                settings.Terrain = EditorGUILayout.ObjectField("Terrain", settings.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                settings.SetProceduralDensityToZero = EditorGUILayout.Toggle("Set Procedural Density To Zero", settings.SetProceduralDensityToZero);
                GVSPSettings.Instance.Import = settings;
                if (GUILayout.Button("Import"))
                {
                    GVSPIntegration.ImportFoliageAsPersistantData(settings.Terrain, settings.VSManager);
                }
            });
        }

        public void DrawMaskPaintingGUI()
        {
            string label = "Mask Painting";
            string id = "mask-painting-vsp-editor";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField("Paint on terrain mask texture and use that mask with VSP texture rules.", GEditorCommon.WordWrapItalicLabel);
                EditorGUILayout.LabelField("1. Set mask resolution using the terrain Inspector.", GEditorCommon.WordWrapItalicLabel);
                EditorGUILayout.LabelField("2. Create Texture Mask Group and configure texture mask rules using VSP Inspector.", GEditorCommon.WordWrapItalicLabel);
                EditorGUILayout.LabelField("3. Use the Geometry-Texture Painter, Mask mode to paint on specific mask channel.", GEditorCommon.WordWrapItalicLabel);
                EditorGUILayout.LabelField("4. Click Refresh Vegetation System button in the Inspector, under Mask Painting foldout.", GEditorCommon.WordWrapItalicLabel);
            });
        }

        public void DrawSplineMaskingGUI()
        {
            string label = "Spline Masking";
            string id = "spline-masking-vsp-editor";
            GEditorCommon.Foldout(label, false, id, () =>
            {
                const string INSTRUCTION = "Spline Masking adds Line Mask component along a spline to remove foliage from roads, rivers, etc.\n" +
                "Select a Spline in the scene and add a Spline Masking modifier.";
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }
    }
}
#endif
