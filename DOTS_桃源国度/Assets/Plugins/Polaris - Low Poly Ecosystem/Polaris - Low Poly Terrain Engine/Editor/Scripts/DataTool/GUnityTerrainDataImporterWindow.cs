#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainDataImporterWindow : EditorWindow
    {
        public Terrain SrcTerrain { get; set; }
        public TerrainData SrcData { get; set; }
        public GTerrainData DesData { get; set; }
        public GStylizedTerrain DesTerrain { get; set; }
        public bool ImportGeometry { get; set; }
        public bool UseUnityTerrainSize { get; set; }
        public bool ImportSplats { get; set; }
        public bool ImportSplatsAsAlbedo { get; set; }
        public bool ImportSplatControlMapsOnly { get; set; }
        public bool ImportSplatControlMapResolution { get; set; }
        public bool CreateNewSplatPrototypesGroup { get; set; }
        public bool ImportTrees { get; set; }
        public bool ImportTreeInstancesOnly { get; set; }
        public bool CreateNewTreePrototypesGroup { get; set; }
        public bool ImportGrasses { get; set; }
        public bool ImportGrassInstancesOnly { get; set; }
        public bool CreateNewGrassPrototypesGroup { get; set; }
        public float GrassDensity { get; set; }

        public bool BulkImport { get; set; }
        public int BulkImportGroupId { get; set; }
        public string Directory { get; set; }

        private const string HISTORY_PREFIX = "Import Unity Terrain";
        private const string PREF_PREFIX = "unity-terrain-importer";
        private const string IMPORT_GEOMETRY_PREF_KEY = "import-geometry";
        private const string USE_UNITY_TERRAIN_SIZE_PREF_KEY = "use-unity-terrain-size";
        private const string IMPORT_SPLATS_PREF_KEY = "import-splats";
        private const string IMPORT_SPLATS_AS_ALBEDO_PREF_KEY = "import-splats-as-albedo";
        private const string IMPORT_SPLATS_CONTROL_MAPS_ONLY_PREF_KEY = "import-splat-control-maps-only";
        private const string USE_UNITY_CONTROL_MAP_RESOLUTION_PREF_KEY = "import-splat-resolution";
        private const string NEW_SPLATS_GROUP_PREF_KEY = "new-splats-group";
        private const string IMPORT_TREES_PREF_KEY = "import-trees";
        private const string IMPORT_TREE_INSTANCES_ONLY_KEY = "import-trees-instances-only";
        private const string NEW_TREES_GROUP_PREF_KEY = "new-trees-group";
        private const string IMPORT_GRASS_PREF_KEY = "import-grasses";
        private const string IMPORT_GRASS_INSTANCES_ONLY_KEY = "import-grass-instances-only";
        private const string NEW_GRASSES_GROUP_PREF_KEY = "new-grasses-group";
        private const string GRASS_DENSITY = "grass-density";
        private const string DIRECTORY_PREF_KEY = "directory";

        private const string INSTRUCTION =
            "Import data from Unity Terrain Data.\n" +
            "Sometime you can see splat textures are not rendered correctly, this is caused mostly because there are more splat textures than the material can support!";

        public static GUnityTerrainDataImporterWindow ShowWindow()
        {
            GUnityTerrainDataImporterWindow window = ScriptableObject.CreateInstance<GUnityTerrainDataImporterWindow>();
            window.titleContent = new GUIContent("Unity Terrain Data Importer");
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            ImportGeometry = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GEOMETRY_PREF_KEY), true);
            UseUnityTerrainSize = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_UNITY_TERRAIN_SIZE_PREF_KEY), true);
            ImportSplats = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_PREF_KEY), true);
            ImportSplatsAsAlbedo = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_AS_ALBEDO_PREF_KEY), false);
            ImportSplatControlMapsOnly = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_CONTROL_MAPS_ONLY_PREF_KEY), false);
            ImportSplatControlMapResolution = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_UNITY_CONTROL_MAP_RESOLUTION_PREF_KEY), false);
            CreateNewSplatPrototypesGroup = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_SPLATS_GROUP_PREF_KEY), false);
            ImportTrees = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREES_PREF_KEY), true);
            ImportTreeInstancesOnly = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREE_INSTANCES_ONLY_KEY), true);
            CreateNewTreePrototypesGroup = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_TREES_GROUP_PREF_KEY), false);
            ImportGrasses = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_PREF_KEY), false);
            ImportGrassInstancesOnly = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_INSTANCES_ONLY_KEY), false);
            CreateNewGrassPrototypesGroup = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_GRASSES_GROUP_PREF_KEY), false);
            GrassDensity = EditorPrefs.GetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, GRASS_DENSITY), 0.5f);
            Directory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREF_KEY), string.Empty);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GEOMETRY_PREF_KEY), ImportGeometry);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_UNITY_TERRAIN_SIZE_PREF_KEY), UseUnityTerrainSize);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_PREF_KEY), ImportSplats);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_AS_ALBEDO_PREF_KEY), ImportSplatsAsAlbedo);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_CONTROL_MAPS_ONLY_PREF_KEY), ImportSplatControlMapsOnly);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_UNITY_CONTROL_MAP_RESOLUTION_PREF_KEY), ImportSplatControlMapResolution);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_SPLATS_GROUP_PREF_KEY), CreateNewSplatPrototypesGroup);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREES_PREF_KEY), ImportTrees);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREE_INSTANCES_ONLY_KEY), ImportTreeInstancesOnly);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_TREES_GROUP_PREF_KEY), CreateNewTreePrototypesGroup);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_PREF_KEY), ImportGrasses);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_INSTANCES_ONLY_KEY), ImportGrassInstancesOnly);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, NEW_GRASSES_GROUP_PREF_KEY), CreateNewGrassPrototypesGroup);
            EditorPrefs.SetFloat(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, GRASS_DENSITY), GrassDensity);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREF_KEY), Directory);
        }

        private void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            DrawInstructionGUI();
            DrawImportGUI();
            EditorGUIUtility.labelWidth = labelWidth;
            HandleRepaint();
        }

        private void HandleRepaint()
        {
            //if (Event.current != null)
            //{
            //    Repaint();
            //}
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "unity-terrain-importer-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawImportGUI()
        {
            string label = "Import";
            string id = "unity-terrain-importer-import";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                if (BulkImport)
                {

                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Terrain", DesTerrain, typeof(GStylizedTerrain), true);
                    EditorGUILayout.ObjectField("Griffin Data", DesData, typeof(GTerrainData), false);
                    GUI.enabled = true;

                    EditorGUI.BeginChangeCheck();
                    SrcTerrain = EditorGUILayout.ObjectField("Unity Terrain", SrcTerrain, typeof(Terrain), true) as Terrain;
                    if (SrcTerrain != null)
                    {
                        SrcData = SrcTerrain.terrainData;
                    }

                    GUI.enabled = SrcTerrain == null;
                    SrcData = EditorGUILayout.ObjectField("Unity Terrain Data", SrcData, typeof(TerrainData), true) as TerrainData;
                    GUI.enabled = true;

                    if (SrcData != null)
                    {
                        if (SrcTerrain != null && SrcTerrain.terrainData != SrcData)
                        {
                            SrcTerrain = null;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                    }

                    if (SrcData == null)
                        return;
                }

                ImportGeometry = EditorGUILayout.Toggle("Import Geometry", ImportGeometry);
                if (ImportGeometry)
                {
                    EditorGUI.indentLevel += 1;
                    UseUnityTerrainSize = EditorGUILayout.Toggle("Unity Terrain Size", UseUnityTerrainSize);
                    EditorGUI.indentLevel -= 1;
                }

                ImportSplats = EditorGUILayout.Toggle("Import Splats", ImportSplats);
                if (ImportSplats)
                {
                    EditorGUI.indentLevel += 1;
                    ImportSplatsAsAlbedo = EditorGUILayout.Toggle("As Albedo", ImportSplatsAsAlbedo);
                    ImportSplatControlMapResolution = EditorGUILayout.Toggle("Use Unity Resolution", ImportSplatControlMapResolution);
                    ImportSplatControlMapsOnly = EditorGUILayout.Toggle("Control Maps Only", ImportSplatControlMapsOnly);

                    if (!ImportSplatControlMapsOnly)
                    {
                        if (BulkImport)
                        {
                            CreateNewSplatPrototypesGroup = EditorGUILayout.Toggle("New Splats Group", CreateNewSplatPrototypesGroup);
                        }
                        else
                        {
                            if (DesData.Shading.Splats == null ||
                                DesData.Shading.Splats.IsSampleAsset)
                            {
                                CreateNewSplatPrototypesGroup = true;
                            }
                            GUI.enabled = DesData.Shading.Splats != null && !DesData.Shading.Splats.IsSampleAsset;
                            GUIContent newSplatsGroupGUI = new GUIContent("New Splats Group", "There is no splats group assigned or sample splats group is in used.");
                            CreateNewSplatPrototypesGroup = EditorGUILayout.Toggle(newSplatsGroupGUI, CreateNewSplatPrototypesGroup);
                            GUI.enabled = true;
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }

                ImportTrees = EditorGUILayout.Toggle("Import Trees", ImportTrees);
                if (ImportTrees)
                {
                    EditorGUI.indentLevel += 1;
                    ImportTreeInstancesOnly = EditorGUILayout.Toggle("Tree Instances Only", ImportTreeInstancesOnly);

                    if (!ImportTreeInstancesOnly)
                    {
                        if (BulkImport)
                        {
                            CreateNewTreePrototypesGroup = EditorGUILayout.Toggle("New Trees Group", CreateNewTreePrototypesGroup);
                        }
                        else
                        {
                            if (DesData.Foliage.Trees == null ||
                                DesData.Foliage.Trees.IsSampleAsset)
                            {
                                CreateNewTreePrototypesGroup = true;
                            }
                            GUI.enabled = DesData.Foliage.Trees != null && !DesData.Foliage.Trees.IsSampleAsset;
                            GUIContent newTreeGroupGUI = new GUIContent("New Trees Group", "There is no trees group assigned or sample trees group is in used.");
                            CreateNewTreePrototypesGroup = EditorGUILayout.Toggle(newTreeGroupGUI, CreateNewTreePrototypesGroup);
                            GUI.enabled = true;
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }

                ImportGrasses = EditorGUILayout.Toggle("Import Grasses & Details", ImportGrasses);
                if (ImportGrasses)
                {
                    EditorGUI.indentLevel += 1;
                    ImportGrassInstancesOnly = EditorGUILayout.Toggle("Grass Instances Only", ImportGrassInstancesOnly);

                    if (!ImportGrassInstancesOnly)
                    {
                        if (BulkImport)
                        {
                            CreateNewGrassPrototypesGroup = EditorGUILayout.Toggle("New Grasses Group", CreateNewGrassPrototypesGroup);
                        }
                        else
                        {
                            if (DesData.Foliage.Grasses == null ||
                                DesData.Foliage.Grasses.IsSampleAsset)
                            {
                                CreateNewGrassPrototypesGroup = true;
                            }
                            GUI.enabled = DesData.Foliage.Grasses != null && !DesData.Foliage.Grasses.IsSampleAsset;
                            GUIContent newGrassGroupGUI = new GUIContent("New Grasses Group", "There is no grasses group assigned or sample grasses group is in used.");
                            CreateNewGrassPrototypesGroup = EditorGUILayout.Toggle(newGrassGroupGUI, CreateNewGrassPrototypesGroup);
                            GUI.enabled = true;
                            EditorGUI.BeginChangeCheck();
                            GrassDensity = EditorGUILayout.Slider("Density", GrassDensity, 0f, 1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                            }
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }

                if (BulkImport)
                {
                    string dir = Directory;
                    GEditorCommon.BrowseFolder("Directory", ref dir);
                    Directory = dir;

                    EditorGUILayout.LabelField("File Name Convention", "TerrainData_<Polaris Terrain Data Id>", GEditorCommon.WordWrapItalicLabel);
                }

                if (GUILayout.Button("Import"))
                {
                    Import();
                }
            });
        }

        private void Import()
        {
            if (BulkImport)
            {
                DoBulkImport();
            }
            else
            {
                DoImport();
            }
        }

        private void DoImport()
        {
            if (DesTerrain != null)
            {
                GBackup.TryCreateInitialBackup(HISTORY_PREFIX, DesTerrain, GCommon.AllResourceFlags);
            }

            GUnityTerrainDataImporter importer = new GUnityTerrainDataImporter();
            importer.SrcTerrain = SrcTerrain;
            importer.SrcData = SrcData;
            importer.DesData = DesData;
            importer.DesTerrain = DesTerrain;
            importer.ImportGeometry = ImportGeometry;
            importer.UseUnityTerrainSize = UseUnityTerrainSize;
            importer.ImportSplats = ImportSplats;
            importer.ImportSplatsAsAlbedo = ImportSplatsAsAlbedo;
            importer.ImportSplatControlMapsOnly = ImportSplatControlMapsOnly;
            importer.ImportSplatControlMapResolution = ImportSplatControlMapResolution;
            importer.CreateNewSplatPrototypesGroup = CreateNewSplatPrototypesGroup;
            importer.ImportTrees = ImportTrees;
            importer.ImportTreeInstancesOnly = ImportTreeInstancesOnly;
            importer.CreateNewTreePrototypesGroup = CreateNewTreePrototypesGroup;
            importer.ImportGrasses = ImportGrasses;
            importer.ImportGrassInstancesOnly = ImportGrassInstancesOnly;
            importer.CreateNewGrassPrototypesGroup = CreateNewGrassPrototypesGroup;
            importer.GrassDensity = GrassDensity;
            importer.Import();

            if (DesTerrain != null)
            {
                GBackup.TryCreateBackup(HISTORY_PREFIX, DesTerrain, GCommon.AllResourceFlags);
            }
        }

        private void DoBulkImport()
        {
            string[] guid = AssetDatabase.FindAssets("t:TerrainData", new string[] { Directory });
            List<TerrainData> terrainDatas = new List<TerrainData>();
            for (int i = 0; i < guid.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
                terrainDatas.Add(data);
            }

            GCommon.ForEachTerrain(
                BulkImportGroupId,
                (t) =>
                {
                    if (t == null || t.TerrainData == null)
                        return;
                    TerrainData srcData = terrainDatas.Find(d => d.name.StartsWith("TerrainData") && d.name.EndsWith(t.TerrainData.Id));
                    if (srcData == null)
                        return;

                    GBackup.TryCreateInitialBackup(HISTORY_PREFIX, t, GCommon.AllResourceFlags);

                    GUnityTerrainDataImporter importer = new GUnityTerrainDataImporter();
                    importer.SrcTerrain = null;
                    importer.SrcData = srcData;
                    importer.DesData = t.TerrainData;
                    importer.DesTerrain = t;
                    importer.ImportGeometry = ImportGeometry;
                    importer.UseUnityTerrainSize = UseUnityTerrainSize;

                    importer.ImportSplats = ImportSplats;
                    importer.ImportSplatsAsAlbedo = ImportSplatsAsAlbedo;
                    importer.ImportSplatControlMapResolution = ImportSplatControlMapResolution;
                    importer.ImportSplatControlMapsOnly = ImportSplatControlMapsOnly;
                    bool createNewSplatGroup = CreateNewSplatPrototypesGroup;
                    if (t.TerrainData.Shading.Splats == null ||
                        t.TerrainData.Shading.Splats.IsSampleAsset)
                    {
                        createNewSplatGroup = true;
                    }
                    importer.CreateNewSplatPrototypesGroup = createNewSplatGroup;

                    importer.ImportTrees = ImportTrees;
                    importer.ImportTreeInstancesOnly = ImportTreeInstancesOnly;
                    bool createNewTreeGroup = CreateNewTreePrototypesGroup;
                    if (t.TerrainData.Foliage.Trees == null ||
                        t.TerrainData.Foliage.Trees.IsSampleAsset)
                    {
                        createNewTreeGroup = true;
                    }
                    importer.CreateNewTreePrototypesGroup = createNewTreeGroup;

                    importer.ImportGrasses = ImportGrasses;
                    importer.ImportGrassInstancesOnly = ImportGrassInstancesOnly;
                    bool createNewGrassGroup = CreateNewGrassPrototypesGroup;
                    if (t.TerrainData.Foliage.Grasses == null ||
                        t.TerrainData.Foliage.Grasses.IsSampleAsset)
                    {
                        createNewGrassGroup = true;
                    }
                    importer.CreateNewGrassPrototypesGroup = createNewGrassGroup;

                    GrassDensity = 1;
                    importer.GrassDensity = GrassDensity;
                    importer.Import();

                    GBackup.TryCreateBackup(HISTORY_PREFIX, t, GCommon.AllResourceFlags);
                });

            GStylizedTerrain.MatchEdges(BulkImportGroupId);
        }
    }
}
#endif
