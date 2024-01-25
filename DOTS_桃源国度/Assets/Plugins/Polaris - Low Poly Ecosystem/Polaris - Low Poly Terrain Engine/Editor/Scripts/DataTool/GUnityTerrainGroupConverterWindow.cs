#if GRIFFIN
using UnityEditor;
using UnityEngine;
using GAlbedoUsage = Pinwheel.Griffin.DataTool.GUnityTerrainGroupConverter.GAlbedoUsage;

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainGroupConverterWindow : EditorWindow
    {
        public GameObject Root { get; set; }
        public GTerrainData DataTemplate { get; set; }
        public bool ImportGeometry { get; set; }
        public bool ImportSplats { get; set; }
        public bool ImportSplatsAsAlbedo { get; set; }
        public GAlbedoUsage AlbedoUsage { get; set; }
        public bool ImportTrees { get; set; }
        public bool ImportGrasses { get; set; }
        public string DataDirectory { get; set; }

        private const string PREF_PREFIX = "unity-terrain-converter";
        private const string DATA_TEMPLATE_PREF_KEY = "data-template";
        private const string IMPORT_GEOMETRY_PREF_KEY = "import-geometry";
        private const string IMPORT_SPLATS_PREF_KEY = "import-splats";
        private const string IMPORT_SPLATS_AS_ALBEDO_PREF_KEY = "import-splats-as-albedo";
        private const string ALBEDO_USAGE_PREF_KEY = "albedo-usage";
        private const string IMPORT_TREES_PREF_KEY = "import-trees";
        private const string IMPORT_GRASS_PREF_KEY = "import-grasses";
        private const string DATA_DIRECTORY_PREF_KEY = "data-directory";

        private const string INSTRUCTION =
            "Convert all Unity Terrain under Root game object to Stylized Terrain. Generated data will be stored in Data Directory folder.";

        public static GUnityTerrainGroupConverterWindow ShowWindow()
        {
            GUnityTerrainGroupConverterWindow window = ScriptableObject.CreateInstance<GUnityTerrainGroupConverterWindow>();
            window.titleContent = new GUIContent("Terrain Group Converter");
            window.ShowUtility();
            return window;
        }

        private void OnEnable()
        {
            ImportGeometry = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GEOMETRY_PREF_KEY), true);
            ImportSplats = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_PREF_KEY), true);
            ImportSplatsAsAlbedo = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_AS_ALBEDO_PREF_KEY), false);
            AlbedoUsage = (GAlbedoUsage)EditorPrefs.GetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, ALBEDO_USAGE_PREF_KEY), 0);
            ImportTrees = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREES_PREF_KEY), true);
            ImportGrasses = EditorPrefs.GetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_PREF_KEY), false);
            DataDirectory = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREF_KEY), "Assets/Polaris Exported/");

            string dataTemplatePath = EditorPrefs.GetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_TEMPLATE_PREF_KEY), null);
            if (!string.IsNullOrEmpty(dataTemplatePath))
            {
                DataTemplate = AssetDatabase.LoadAssetAtPath<GTerrainData>(dataTemplatePath);
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GEOMETRY_PREF_KEY), ImportGeometry);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_PREF_KEY), ImportSplats);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_SPLATS_AS_ALBEDO_PREF_KEY), ImportSplatsAsAlbedo);
            EditorPrefs.SetInt(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, ALBEDO_USAGE_PREF_KEY), (int)AlbedoUsage);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_TREES_PREF_KEY), ImportTrees);
            EditorPrefs.SetBool(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, IMPORT_GRASS_PREF_KEY), ImportGrasses);
            EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_DIRECTORY_PREF_KEY), DataDirectory);

            if (DataTemplate != null && AssetDatabase.Contains(DataTemplate) && EditorUtility.IsPersistent(DataTemplate))
            {
                string path = AssetDatabase.GetAssetPath(DataTemplate);
                EditorPrefs.SetString(GEditorCommon.GetProjectRelatedEditorPrefsKey(PREF_PREFIX, DATA_TEMPLATE_PREF_KEY), path);
            }
        }

        private void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            DrawInstructionGUI();
            DrawConvertGUI();
            EditorGUIUtility.labelWidth = labelWidth;
            HandleRepaint();
        }

        private void HandleRepaint()
        {
            if (Event.current != null)
            {
                Repaint();
            }
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "terrain-converter-instruction";
            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField(INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawConvertGUI()
        {
            string label = "Convert";
            string id = "terrain-converter-convert";
            GEditorCommon.Foldout(label, false, id, () =>
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Root", Root, typeof(GameObject), true);
                GUI.enabled = true;

                DataTemplate = EditorGUILayout.ObjectField("Data Template", DataTemplate, typeof(GTerrainData), false) as GTerrainData;
                ImportGeometry = EditorGUILayout.Toggle("Import Geometry", ImportGeometry);
                ImportSplats = EditorGUILayout.Toggle("Import Splats", ImportSplats);
                if (ImportSplats)
                {
                    EditorGUI.indentLevel += 1;
                    ImportSplatsAsAlbedo = EditorGUILayout.Toggle("As Albedo", ImportSplatsAsAlbedo);
                    bool hasNoTemplateMaterial = DataTemplate == null || DataTemplate.Shading.CustomMaterial == null;
                    if (ImportSplatsAsAlbedo && hasNoTemplateMaterial)
                    {
                        AlbedoUsage = (GAlbedoUsage)EditorGUILayout.EnumPopup("Albedo Usage", AlbedoUsage);
                    }
                    EditorGUI.indentLevel -= 1;
                }

                ImportTrees = EditorGUILayout.Toggle("Import Trees", ImportTrees);
                ImportGrasses = EditorGUILayout.Toggle("Import Grasses & Details", ImportGrasses);
                string dir = DataDirectory;
                GEditorCommon.BrowseFolder("Data Directory", ref dir);
                DataDirectory = dir;

                if (GUILayout.Button("Convert"))
                {
                    Convert();
                }
            });
        }

        private void Convert()
        {
            GUnityTerrainGroupConverter converter = new GUnityTerrainGroupConverter();
            converter.Root = Root;
            converter.DataTemplate = DataTemplate;
            converter.ImportGeometry = ImportGeometry;
            converter.ImportSplats = ImportSplats;
            converter.ImportSplatsAsAlbedo = ImportSplatsAsAlbedo;
            converter.AlbedoUsage = AlbedoUsage;
            converter.ImportTrees = ImportTrees;
            converter.ImportGrasses = ImportGrasses;
            converter.DataDirectory = DataDirectory;

            converter.Convert();
        }
    }
}
#endif
