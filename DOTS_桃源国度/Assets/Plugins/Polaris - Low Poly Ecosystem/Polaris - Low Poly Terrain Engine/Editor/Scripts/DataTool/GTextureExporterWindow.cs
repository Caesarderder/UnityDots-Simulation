#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GTextureExporterWindow : EditorWindow
    {
        public GTerrainData SrcData { get; set; }
        public bool ExportHeightMap { get; set; }
        public bool ExportVisibilityMap { get; set; }
        public bool ExportAlbedoMap { get; set; }
        public bool ExportMetallicMap { get; set; }
        public bool ExportGradientLookupMaps { get; set; }
        public bool ExportSplatControlMaps { get; set; }
        public string DataDirectory { get; set; }

        public bool BulkExport { get; set; }
        public int BulkExportGroupId { get; set; }

        private const string INSTRUCTION =
            "Export terrain textures to PNG files.";

        private const string PREF_PREFIX = "texture-exporter";
        private const string EXPORT_HEIGHTMAP_PREFIX = "export-heightmap";
        private const string EXPORT_VISIBILITY_PREFIX = "export-visibility";
        private const string EXPORT_ALBEDO_PREFIX = "export-albedo";
        private const string EXPORT_METALLIC_PREFIX = "export-metallic";
        private const string EXPORT_GRADIENT_PREFIX = "export-gradient";
        private const string EXPORT_SPLATS_PREFIX = "export-splats";
        private const string DATA_DIRECTORY_PREFIX = "directory";

        public static GTextureExporterWindow ShowWindow()
        {
            GTextureExporterWindow window = ScriptableObject.CreateInstance<GTextureExporterWindow>();
            window.titleContent = new GUIContent("Texture Exporter");
            window.minSize = new Vector2(400, 300);
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            ExportHeightMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_HEIGHTMAP_PREFIX), true);
            ExportVisibilityMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_VISIBILITY_PREFIX), true);
            ExportAlbedoMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_ALBEDO_PREFIX), true);
            ExportMetallicMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_METALLIC_PREFIX), true);
            ExportGradientLookupMaps = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GRADIENT_PREFIX), true);
            ExportSplatControlMaps = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_SPLATS_PREFIX), true);
            DataDirectory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREFIX), "Assets/Polaris Exported/");
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_HEIGHTMAP_PREFIX), ExportHeightMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_VISIBILITY_PREFIX), ExportVisibilityMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_ALBEDO_PREFIX), ExportAlbedoMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_METALLIC_PREFIX), ExportMetallicMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_GRADIENT_PREFIX), ExportGradientLookupMaps);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, EXPORT_SPLATS_PREFIX), ExportSplatControlMaps);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREFIX), DataDirectory);
        }

        private void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180;
            DrawInstructionGUI();
            DrawExportGUI();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "texture-exporter-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawExportGUI()
        {
            string label = "Export";
            string id = "texture-exporter-import";

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
                }
                ExportHeightMap = EditorGUILayout.Toggle("Export Height Map", ExportHeightMap);
                ExportVisibilityMap = EditorGUILayout.Toggle("Export Visibility Map", ExportVisibilityMap);
                ExportAlbedoMap = EditorGUILayout.Toggle("Export Albedo Map", ExportAlbedoMap);
                ExportMetallicMap = EditorGUILayout.Toggle("Export Metallic Map", ExportMetallicMap);
                ExportGradientLookupMaps = EditorGUILayout.Toggle("Export Gradient Maps", ExportGradientLookupMaps);
                ExportSplatControlMaps = EditorGUILayout.Toggle("Export Splat Control Maps", ExportSplatControlMaps);

                string dir = DataDirectory;
                GEditorCommon.BrowseFolder("Data Directory", ref dir);
                DataDirectory = dir;
                EditorGUILayout.LabelField("Asset with the same name will be overwriten!", GEditorCommon.WordWrapItalicLabel);

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

                    GTextureExporter exporter = new GTextureExporter();
                    exporter.SrcData = t.TerrainData;
                    exporter.ExportHeightMap = ExportHeightMap;
                    exporter.ExportVisibilityMap = ExportVisibilityMap;
                    exporter.ExportAlbedoMap = ExportAlbedoMap;
                    exporter.ExportMetallicMap = ExportMetallicMap;
                    exporter.ExportGradientLookupMaps = ExportGradientLookupMaps;
                    exporter.ExportSplatControlMaps = ExportSplatControlMaps;
                    exporter.DataDirectory = DataDirectory;

                    exporter.Export();
                });
        }

        private void DoExport()
        {
            GTextureExporter exporter = new GTextureExporter();
            exporter.SrcData = SrcData;
            exporter.ExportHeightMap = ExportHeightMap;
            exporter.ExportVisibilityMap = ExportVisibilityMap;
            exporter.ExportAlbedoMap = ExportAlbedoMap;
            exporter.ExportMetallicMap = ExportMetallicMap;
            exporter.ExportGradientLookupMaps = ExportGradientLookupMaps;
            exporter.ExportSplatControlMaps = ExportSplatControlMaps;
            exporter.DataDirectory = DataDirectory;

            exporter.Export();
        }
    }
}
#endif
