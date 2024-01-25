#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GRawImporterWindow : EditorWindow
    {
        public GStylizedTerrain Terrain { get; set; }
        public GTerrainData DesData { get; set; }
        public GBitDepth BitDepth { get; set; }
        public bool UseRawResolution { get; set; }
        public string FilePath { get; set; }
        public string DataDirectory { get; set; }

        public bool BulkImport { get; set; }
        public int BulkImportGroupId { get; set; }

        private const string PREF_PREFIX = "raw-importer";
        private const string BIT_DEPTH_PREFIX = "bit-depth";
        private const string USE_RAW_RESOLUTION_PREFIX = "use-raw-resolution";
        private const string FILE_PATH_PREFIX = "file-path";
        private const string DIRECTORY_PREFIX = "directory";

        private const string HISTORY_PREFIX = "Import RAW";

        private const string INSTRUCTION =
            "Import Height Map from RAW file (raw, r16) generated from other applications like World Machine.\n" +
            "The RAW file must have squared size, Windows byte order.";

        public static GRawImporterWindow ShowWindow()
        {
            GRawImporterWindow window = ScriptableObject.CreateInstance<GRawImporterWindow>();
            window.titleContent = new GUIContent("Raw Importer");
            window.minSize = new Vector2(400, 200);
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            BitDepth = (GBitDepth)EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BIT_DEPTH_PREFIX), 0);
            UseRawResolution = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_RAW_RESOLUTION_PREFIX), true);
            FilePath = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, FILE_PATH_PREFIX), string.Empty);
            DataDirectory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREFIX), string.Empty);
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BIT_DEPTH_PREFIX), (int)BitDepth);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, USE_RAW_RESOLUTION_PREFIX), UseRawResolution);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, FILE_PATH_PREFIX), FilePath);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREFIX), DataDirectory);
        }

        private void OnGUI()
        {
            DrawInstructionGUI();
            DrawImportGUI();
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "raw-importer-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawImportGUI()
        {
            string label = "Import";
            string id = "raw-importer-import";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                if (BulkImport)
                {
                    GUI.enabled = false;
                    GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", BulkImportGroupId);
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Terrain", Terrain, typeof(GStylizedTerrain), true);
                    EditorGUILayout.ObjectField("Griffin Data", DesData, typeof(GTerrainData), false);
                    GUI.enabled = true;
                }
                BitDepth = (GBitDepth)EditorGUILayout.EnumPopup("Bit Depth", BitDepth);
                UseRawResolution = EditorGUILayout.Toggle("Use RAW Resolution", UseRawResolution);

                if (BulkImport)
                {
                    string path = DataDirectory;
                    GEditorCommon.BrowseFolder("Directory", ref path);
                    DataDirectory = path;
                    string convention = string.Format("{0}_{1}_{2}", BitDepth == GBitDepth.Bit8 ? "RAW8" : "RAW16", "<optional>", "< Polaris Terrain Data Id>");
                    EditorGUILayout.LabelField("File Name Convention", convention, GEditorCommon.WordWrapItalicLabel);
                }
                else
                {
                    string path = FilePath;
                    GEditorCommon.BrowseFile("Path", ref path, "raw", "raw", "r16", "r16");
                    FilePath = path;
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
            if (Terrain != null)
            {
                GBackup.TryCreateInitialBackup(HISTORY_PREFIX, Terrain, GCommon.HeightMapAndFoliageResourceFlags);
            }

            GRawImporter importer = new GRawImporter();
            importer.Terrain = Terrain;
            importer.DesData = DesData;
            importer.BitDepth = BitDepth;
            importer.UseRawResolution = UseRawResolution;
            importer.FilePath = FilePath;
            importer.Import();

            if (Terrain != null)
            {
                GBackup.TryCreateBackup(HISTORY_PREFIX, Terrain, GCommon.HeightMapAndFoliageResourceFlags);
            }
        }

        private void DoBulkImport()
        {
            string ext = BitDepth == GBitDepth.Bit8 ? "raw" : "r16";
            List<string> files = new List<string>(Directory.GetFiles(DataDirectory));
            files.RemoveAll(f => !f.EndsWith(ext));

            GCommon.ForEachTerrain(
                BulkImportGroupId,
                (t) =>
                {
                    if (t == null || t.TerrainData == null)
                        return;

                    string file = files.Find(
                        s =>
                        {
                            string f = Path.GetFileNameWithoutExtension(s);
                            return f.StartsWith(BitDepth == GBitDepth.Bit8 ? "RAW8" : "RAW16") && f.EndsWith(t.TerrainData.Id);
                        });
                    if (string.IsNullOrEmpty(file))
                        return;

                    GBackup.TryCreateInitialBackup(HISTORY_PREFIX, t, GCommon.HeightMapAndFoliageResourceFlags);

                    GRawImporter importer = new GRawImporter();
                    importer.Terrain = t;
                    importer.DesData = t.TerrainData;
                    importer.BitDepth = BitDepth;
                    importer.UseRawResolution = UseRawResolution;
                    importer.FilePath = file;
                    importer.Import();

                    GBackup.TryCreateBackup(HISTORY_PREFIX, t, GCommon.HeightMapAndFoliageResourceFlags);
                });
            GStylizedTerrain.MatchEdges(BulkImportGroupId);
        }
    }
}
#endif
