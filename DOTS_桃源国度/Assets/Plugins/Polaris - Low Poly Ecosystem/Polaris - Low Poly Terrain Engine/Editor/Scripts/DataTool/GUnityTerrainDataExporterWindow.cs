#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainDataExporterWindow : EditorWindow
    {
        public GTerrainData SrcData { get; set; }
        public TerrainData DesData { get; set; }
        public bool CreateNewAsset { get; set; }
        public bool ExportGeometry { get; set; }
        public bool ExportTerrainSize { get; set; }
        public bool ExportSplats { get; set; }
        public bool OverwriteSplatLayers { get; set; }
        public bool ExportTrees { get; set; }
        public bool ExportGrasses { get; set; }
        public string DataDirectory { get; set; }

        public bool BulkExport { get; set; }
        public int BulkExportGroupId { get; set; }

        private const string PREF_PREFIX = "unity-terrain-exporter";
        private const string CREATE_NEW_ASSET = "create-new-asset";
        private const string EXPORT_GEOMETRY_PREF_KEY = "export-geometry";
        private const string EXPORT_TERRAIN_SIZE_PREF_KEY = "export-terrain-size";
        private const string EXPORT_SPLATS_PREF_KEY = "export-splats";
        private const string OVERWRITE_SPLAT_LAYERS_PREF_KEY = "overwrite-splats";
        private const string EXPORT_TREES_PREF_KEY = "export-trees";
        private const string EXPORT_GRASS_PREF_KEY = "export-grasses";
        private const string DATA_DIRECTORY_PREF_KEY = "data-directory";

        private const string INSTRUCTION =
            "Export data to Unity Terain to use with Unity built-in or 3rd-parties terrain tools.";

        public static GUnityTerrainDataExporterWindow ShowWindow()
        {
            GUnityTerrainDataExporterWindow window = ScriptableObject.CreateInstance<GUnityTerrainDataExporterWindow>();
            window.titleContent = new GUIContent("Unity Terrain Data Exporter");
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            CreateNewAsset = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CREATE_NEW_ASSET), true);
            ExportGeometry = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GEOMETRY_PREF_KEY), true);
            ExportTerrainSize = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_TERRAIN_SIZE_PREF_KEY), true);
            ExportSplats = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_SPLATS_PREF_KEY), true);
            OverwriteSplatLayers = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, OVERWRITE_SPLAT_LAYERS_PREF_KEY), false);
            ExportTrees = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_TREES_PREF_KEY), true);
            ExportGrasses = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GRASS_PREF_KEY), false);
            DataDirectory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREF_KEY), "Assets/Polaris Exported/");
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, CREATE_NEW_ASSET), CreateNewAsset);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GEOMETRY_PREF_KEY), ExportGeometry);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_TERRAIN_SIZE_PREF_KEY), ExportTerrainSize);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_SPLATS_PREF_KEY), ExportSplats);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, OVERWRITE_SPLAT_LAYERS_PREF_KEY), OverwriteSplatLayers);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_TREES_PREF_KEY), ExportTrees);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GRASS_PREF_KEY), ExportGrasses);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREF_KEY), DataDirectory);
        }

        private void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            DrawInstructionGUI();
            DrawExportGUI();
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
            string id = "unity-terrain-exporter-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawExportGUI()
        {
            string label = "Export";
            string id = "unity-terrain-exporter-export";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                if (BulkExport)
                {
                    GUI.enabled = false;
                    GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", BulkExportGroupId);
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Griffin Data", SrcData, typeof(GTerrainData), false);
                    GUI.enabled = true;
                    CreateNewAsset = EditorGUILayout.Toggle("Create New Asset", CreateNewAsset);
                    if (!CreateNewAsset)
                    {
                        DesData = EditorGUILayout.ObjectField("Terrain Data", DesData, typeof(TerrainData), false) as TerrainData;
                    }

                    if (!CreateNewAsset && DesData == null)
                        return;
                }

                ExportGeometry = EditorGUILayout.Toggle("Export Geometry", ExportGeometry);
                if (ExportGeometry)
                {
                    EditorGUI.indentLevel += 1;
                    ExportTerrainSize = EditorGUILayout.Toggle("Export Terrain Size", ExportTerrainSize);
                    EditorGUI.indentLevel -= 1;
                }

                ExportSplats = EditorGUILayout.Toggle("Export Splats", ExportSplats);
                if (ExportSplats && !CreateNewAsset && !BulkExport)
                {
                    EditorGUI.indentLevel += 1;
#if !UNITY_2018_1 && !UNITY_2018_2
                    OverwriteSplatLayers = EditorGUILayout.Toggle("Overwrite Layers", OverwriteSplatLayers);
#else
                    OverwriteSplatLayers = true;
#endif
                    EditorGUI.indentLevel -= 1;
                }

                ExportTrees = EditorGUILayout.Toggle("Export Trees", ExportTrees);
                ExportGrasses = EditorGUILayout.Toggle("Export Grasses & Details", ExportGrasses);

                string dir = DataDirectory;
                GEditorCommon.BrowseFolder("Directory", ref dir);
                DataDirectory = dir;

                if (BulkExport)
                {
                    EditorGUILayout.LabelField("Asset with the same name will be overwriten!", GEditorCommon.WordWrapItalicLabel);
                }

                if (GUILayout.Button("Export"))
                {
                    Export();
                }
            });
        }

        private void Export()
        {
            if (BulkExport)
            {
                DoBulkExport();
            }
            else
            {
                DoExport();
            }
        }

        private void DoBulkExport()
        {
            GCommon.ForEachTerrain(
                BulkExportGroupId,
                (t) =>
                {
                    if (t == null || t.TerrainData == null)
                        return;

                    GUnityTerrainDataExporter exporter = new GUnityTerrainDataExporter();
                    exporter.SrcData = t.TerrainData;
                    exporter.DesData = null;
                    exporter.CreateNewAsset = true;
                    exporter.ExportGeometry = ExportGeometry;
                    exporter.ExportTerrainSize = ExportTerrainSize;
                    exporter.ExportSplats = ExportSplats;
                    exporter.OverwriteSplatLayers = false;
                    exporter.ExportTrees = ExportTrees;
                    exporter.ExportGrasses = ExportGrasses;
                    exporter.DataDirectory = DataDirectory;

                    exporter.Export();
                });
        }

        private void DoExport()
        {
            GUnityTerrainDataExporter exporter = new GUnityTerrainDataExporter();
            exporter.SrcData = SrcData;
            exporter.DesData = DesData;
            exporter.CreateNewAsset = CreateNewAsset;
            exporter.ExportGeometry = ExportGeometry;
            exporter.ExportTerrainSize = ExportTerrainSize;
            exporter.ExportSplats = ExportSplats;
            exporter.OverwriteSplatLayers = OverwriteSplatLayers;
            exporter.ExportTrees = ExportTrees;
            exporter.ExportGrasses = ExportGrasses;
            exporter.DataDirectory = DataDirectory;

            exporter.Export();
        }
    }
}
#endif
