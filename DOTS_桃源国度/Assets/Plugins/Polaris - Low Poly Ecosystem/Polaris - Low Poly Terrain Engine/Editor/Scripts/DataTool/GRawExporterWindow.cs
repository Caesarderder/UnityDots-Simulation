#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GRawExporterWindow : EditorWindow
    {
        public GTerrainData SrcData { get; set; }
        public GBitDepth BitDepth { get; set; }
        public string DataDirectory { get; set; }

        public bool BulkExport { get; set; }
        public int BulkExportGroupId { get; set; }

        private const string PREF_PREFIX = "raw-exporter";
        private const string BIT_DEPTH_PREFIX = "bit-depth";
        private const string DATA_DIRECTORY_PREFIX = "directory";

        private const string INSTRUCTION =
            "Export height map (R channel) to RAW file (raw, r16).";
        private const string INSTRUCTION_BULK =
            "Export height maps (R channel) to RAW files (raw, r16).";

        public static GRawExporterWindow ShowWindow()
        {
            GRawExporterWindow window = ScriptableObject.CreateInstance<GRawExporterWindow>();
            window.titleContent = new GUIContent("Raw Exporter");
            window.minSize = new Vector2(400, 300);
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            BitDepth = (GBitDepth)EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BIT_DEPTH_PREFIX), 0);
            DataDirectory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREFIX), "Assets/Polaris Exported/");
            if (string.IsNullOrEmpty(DataDirectory))
            {
                DataDirectory = "Assets/";
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BIT_DEPTH_PREFIX), (int)BitDepth);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREFIX), DataDirectory);
        }

        private void OnGUI()
        {
            DrawInstructionGUI();
            DrawExportGUI();
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "raw-exporter-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(BulkExport ? INSTRUCTION_BULK : INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawExportGUI()
        {
            string label = "Export";
            string id = "raw-exporter-import";

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
                BitDepth = (GBitDepth)EditorGUILayout.EnumPopup("Bit Depth", BitDepth);
                string path = DataDirectory;
                GEditorCommon.BrowseFolder("Directory", ref path);
                DataDirectory = path;
                EditorGUILayout.LabelField("Files with the same name will be overwriten!", GEditorCommon.WordWrapItalicLabel);

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

        private void DoExport()
        {
            GRawExporter exporter = new GRawExporter();
            exporter.SrcData = SrcData;
            exporter.BitDepth = BitDepth;
            exporter.DataDirectory = DataDirectory;
            exporter.Export();
        }

        private void DoBulkExport()
        {
            GCommon.ForEachTerrain(
                BulkExportGroupId,
                (t) =>
                {
                    if (t.TerrainData == null)
                        return;
                    GRawExporter exporter = new GRawExporter();
                    exporter.SrcData = t.TerrainData;
                    exporter.BitDepth = BitDepth;
                    exporter.DataDirectory = DataDirectory;
                    exporter.Export();
                });
        }
    }
}
#endif
