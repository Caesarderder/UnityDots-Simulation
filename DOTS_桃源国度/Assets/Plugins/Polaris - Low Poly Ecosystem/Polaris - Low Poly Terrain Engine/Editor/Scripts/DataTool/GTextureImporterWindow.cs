#if GRIFFIN
using Pinwheel.Griffin.BackupTool;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.DataTool
{
    public class GTextureImporterWindow : EditorWindow
    {
        public GStylizedTerrain Terrain { get; set; }
        public GTerrainData DesData { get; set; }
        public Texture2D HeightMap { get; set; }
        public Texture2D MaskMap { get; set; }
        public Texture2D VisibilityMap { get; set; }
        public Texture2D AlbedoMap { get; set; }
        public Texture2D MetallicMap { get; set; }
        public Texture2D[] SplatControlMaps { get; set; }

        public bool BulkImport { get; set; }
        public int BulkImportGroupId { get; set; }
        public bool BulkImportHeightMap { get; set; }
        public bool BulkImportMaskMap { get; set; }
        public bool BulkImportVisibilityMap { get; set; }
        public bool BulkImportAlbedoMap { get; set; }
        public bool BulkImportMetallicMap { get; set; }
        public bool BulkImportControlMaps { get; set; }
        public string Directory { get; set; }

        private const string HISTORY_PREFIX = "Import Textures";
        private const string INSTRUCTION =
            "Import external textures into the Terrain Data.\n" +
            "These textures will be resampled to fit Terrain Data config.";

        private const string PREF_PREFIX = "texture-importer";
        private const string BULK_IMPORT_HEIGHT_MAP_PREF_KEY = "bulk-import-height-map";
        private const string BULK_IMPORT_MASK_MAP_PREF_KEY = "bulk-import-mask-map";
        private const string BULK_IMPORT_VISIBILITY_MAP_PREF_KEY = "bulk-import-visibility-map";
        private const string BULK_IMPORT_ALBEDO_MAP_PREF_KEY = "bulk-import-albedo-map";
        private const string BULK_IMPORT_METALLIC_MAP_PREF_KEY = "bulk-import-metallic-map";
        private const string BULK_IMPORT_CONTROL_MAP_PREF_KEY = "bulk-import-control-map";
        private const string DIRECTORY_PREF_KEY = "directory";

        public static GTextureImporterWindow ShowWindow(GStylizedTerrain terrain = null)
        {
            GTextureImporterWindow window = ScriptableObject.CreateInstance<GTextureImporterWindow>();
            window.Terrain = terrain;
            window.titleContent = new GUIContent("Texture Importer");
            window.minSize = new Vector2(400, 300);
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            BulkImportHeightMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_HEIGHT_MAP_PREF_KEY), true);
            BulkImportMaskMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_MASK_MAP_PREF_KEY), true);
            BulkImportVisibilityMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_VISIBILITY_MAP_PREF_KEY), true);
            BulkImportAlbedoMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_ALBEDO_MAP_PREF_KEY), true);
            BulkImportMetallicMap = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_METALLIC_MAP_PREF_KEY), true);
            BulkImportControlMaps = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_CONTROL_MAP_PREF_KEY), true);
            Directory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREF_KEY), string.Empty);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_HEIGHT_MAP_PREF_KEY), BulkImportHeightMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_MASK_MAP_PREF_KEY), BulkImportMaskMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_VISIBILITY_MAP_PREF_KEY), BulkImportVisibilityMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_ALBEDO_MAP_PREF_KEY), BulkImportAlbedoMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_METALLIC_MAP_PREF_KEY), BulkImportMetallicMap);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, BULK_IMPORT_CONTROL_MAP_PREF_KEY), BulkImportControlMaps);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DIRECTORY_PREF_KEY), Directory);
        }

        private void OnGUI()
        {
            DrawInstructionGUI();
            DrawImportGUI();
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "texture-importer-instruction";

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawImportGUI()
        {
            string label = "Import";
            string id = "texture-importer-import";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                if (BulkImport)
                {
                    GUI.enabled = false;
                    GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", BulkImportGroupId);
                    GUI.enabled = true;
                    BulkImportHeightMap = EditorGUILayout.Toggle("Height Map (R)", BulkImportHeightMap);
                    BulkImportMaskMap = EditorGUILayout.Toggle("Mask Map (R)", BulkImportMaskMap);
                    BulkImportVisibilityMap = EditorGUILayout.Toggle("Visibility Map (1-R)", BulkImportVisibilityMap);
                    BulkImportAlbedoMap = EditorGUILayout.Toggle("Albedo Map", BulkImportAlbedoMap);
                    BulkImportMetallicMap = EditorGUILayout.Toggle("Metallic Map", BulkImportMetallicMap);
                    BulkImportControlMaps = EditorGUILayout.Toggle("Control Maps", BulkImportControlMaps);
                    string dir = Directory;
                    GEditorCommon.BrowseFolder("Directory", ref dir);
                    Directory = dir;

                    string convention =
                        "HeightMap_<optional>_<Polaris Terrain Data Id>\n" +
                        "MaskMap_<optional>_<Polaris Terrain Data Id>\n" +
                        "VisibilityMap_<optional>_<Polaris Terrain Data Id>\n" +
                        "AlbedoMap_<optional>_<Polaris Terrain Data Id>\n" +
                        "MetallicMap_<optional>_<Polaris Terrain Data Id>\n" +
                        "ControlMap<index>_<optional>_<Polaris Terrain Data Id>";
                    EditorGUILayout.LabelField("File Name Convention", convention, GEditorCommon.WordWrapItalicLabel);

                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("Terrain", Terrain, typeof(GStylizedTerrain), true);
                    EditorGUILayout.ObjectField("Griffin Data", DesData, typeof(GTerrainData), false);
                    GUI.enabled = true;

                    HeightMap = EditorGUILayout.ObjectField("Height Map (R)", HeightMap, typeof(Texture2D), false) as Texture2D;
                    MaskMap = EditorGUILayout.ObjectField("Mask Map (R)", MaskMap, typeof(Texture2D), false) as Texture2D;
                    VisibilityMap = EditorGUILayout.ObjectField("Visibility Map (1-R)", VisibilityMap, typeof(Texture2D), false) as Texture2D;
                    AlbedoMap = EditorGUILayout.ObjectField("Albedo Map", AlbedoMap, typeof(Texture2D), false) as Texture2D;
                    MetallicMap = EditorGUILayout.ObjectField("Metallic Map", MetallicMap, typeof(Texture2D), false) as Texture2D;

                    if (SplatControlMaps == null || SplatControlMaps.Length != DesData.Shading.SplatControlMapCount)
                    {
                        SplatControlMaps = new Texture2D[DesData.Shading.SplatControlMapCount];
                    }
                    for (int i = 0; i < SplatControlMaps.Length; ++i)
                    {
                        SplatControlMaps[i] = EditorGUILayout.ObjectField("Splat Control " + i, SplatControlMaps[i], typeof(Texture2D), false) as Texture2D;
                    }
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

        private void DoBulkImport()
        {
            string[] guid = AssetDatabase.FindAssets("t:Texture2D", new string[] { Directory });
            List<Texture2D> textures = new List<Texture2D>();
            for (int i = 0; i < guid.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                textures.Add(tex);
            }

            GCommon.ForEachTerrain(
                BulkImportGroupId,
                (t) =>
                {
                    if (t == null || t.TerrainData == null)
                        return;

                    GBackup.TryCreateInitialBackup(HISTORY_PREFIX, t, GCommon.AllResourceFlags);

                    Texture2D hm = textures.Find(tex => tex.name.StartsWith("HeightMap") && tex.name.EndsWith(t.TerrainData.Id));
                    Texture2D mkm = textures.Find(tex => tex.name.StartsWith("MaskMap") && tex.name.EndsWith(t.TerrainData.Id));
                    Texture2D vm = textures.Find(tex => tex.name.StartsWith("VisibilityMap") && tex.name.EndsWith(t.TerrainData.Id));
                    Texture2D am = textures.Find(tex => tex.name.StartsWith("AlbedoMap") && tex.name.EndsWith(t.TerrainData.Id));
                    Texture2D mm = textures.Find(tex => tex.name.StartsWith("MetallicMap") && tex.name.EndsWith(t.TerrainData.Id));
                    Texture2D[] cm = new Texture2D[t.TerrainData.Shading.SplatControlMapCount];
                    for (int i = 0; i < cm.Length; ++i)
                    {
                        cm[i] = textures.Find(tex => tex.name.StartsWith("ControlMap" + i.ToString()) && tex.name.EndsWith(t.TerrainData.Id));
                    }

                    GTextureImporter importer = new GTextureImporter();
                    importer.Terrain = t;
                    importer.DesData = t.TerrainData;
                    importer.HeightMap = hm;
                    importer.MaskMap = mkm;
                    importer.VisibilityMap = vm;
                    importer.AlbedoMap = am;
                    importer.MetallicMap = mm;
                    importer.SplatControlMaps = cm;
                    importer.Import();

                    GBackup.TryCreateBackup(HISTORY_PREFIX, t, GCommon.AllResourceFlags);
                });

            GStylizedTerrain.MatchEdges(BulkImportGroupId);
        }

        private void DoImport()
        {
            if (Terrain != null)
            {
                GBackup.TryCreateInitialBackup(HISTORY_PREFIX, Terrain, GCommon.AllResourceFlags);
            }

            GTextureImporter importer = new GTextureImporter();
            importer.Terrain = Terrain;
            importer.DesData = DesData;
            importer.HeightMap = HeightMap;
            importer.MaskMap = MaskMap;
            importer.VisibilityMap = VisibilityMap;
            importer.AlbedoMap = AlbedoMap;
            importer.MetallicMap = MetallicMap;
            importer.SplatControlMaps = SplatControlMaps;
            importer.Import();

            if (Terrain != null)
            {
                GBackup.TryCreateBackup(HISTORY_PREFIX, Terrain, GCommon.AllResourceFlags);
            }
        }
    }
}
#endif
