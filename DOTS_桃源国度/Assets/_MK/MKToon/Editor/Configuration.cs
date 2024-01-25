//////////////////////////////////////////////////////
// MK Install Wizard Configuration            		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

#if MK_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

using Configuration = MK.Toon.Editor.InstallWizard.Configuration;
namespace MK.Toon.Editor.InstallWizard
{
    //[CreateAssetMenu(fileName = "Configuration", menuName = "MK/Install Wizard/Create Configuration Asset")]
    public sealed class Configuration : ScriptableObject
    {
        #pragma warning disable CS0414
        internal static bool isReady 
        { 
            get
            { 
                if(_instance == null)
                    TryGetInstance();
                return _instance != null; 
            } 
        }

        [SerializeField]
        private RenderPipeline _renderPipeline = RenderPipeline.Built_in;

        [SerializeField]
        internal bool showInstallerOnReload = true;
        [SerializeField]
        internal bool disablePerObjectOutlinesWarning = false;

        [SerializeField][Space]
        private Texture2D _titleImage = null;

        [SerializeField][Space]
        private Object _readMe = null;

        [SerializeField][Space]
        private Object _basePackageBuiltin = null;
        [SerializeField]
        private Object _basePackageURP = null;

        [SerializeField][Space]
        private Object _examplesPackageInc = null;
        [SerializeField]
        private Object _examplesPackageBuiltin = null;
        [SerializeField]
        private Object _examplesPackageURP = null;
        [SerializeField]
        private Object _examplesPackageURP_2023_2_Or_Newer = null;
        [SerializeField]
        private Object _examplesPackageBuiltin_2023_2_Or_Newer = null;

        [SerializeField][Space]
        private ExampleContainer[] _examples = null;

        [Space][Header("Global Shader Features")]
        [SerializeField]
        private UnityEngine.Object _globalShaderFeaturesFile = null;
        public static void ConfigureGlobalShaderFeatures()
        {
            if(isReady)
            {
                List<string> content = new List<string>();
                string projectPath = Application.dataPath;
                projectPath = projectPath.Substring(0, projectPath.Length - 6);
                string filePath = projectPath + AssetDatabase.GetAssetPath(_instance._globalShaderFeaturesFile);

                //Read
                using(MemoryStream dataStream = new MemoryStream(File.ReadAllBytes(filePath)))
                {
                    using (StreamReader dataReader = new StreamReader(dataStream, System.Text.Encoding.UTF8))
                    {
                        string line;
                        while((line = dataReader.ReadLine()) != null)
                        {
                            content.Add(line);
                        }
                    }
                }

                const string detectFeature = "//%%";
                bool updated = false;
                //Modify
                for(int i = 0; i < content.Count; i++)
                {
                    for(int f = 0; f < _instance._globalShaderFeatures.Count; f++)
                    {
                        for(int id = 0; id < _instance._globalShaderFeatures[f].identifiers.Count; id++)
                        {
                            if(content[i].Contains(detectFeature + _instance._globalShaderFeatures[f].identifiers[id]))
                            {
                                if(ParseFeature(content, i, _instance._globalShaderFeatures[f].mode == id))
                                    updated = true;
                            }
                        }
                    }
                }

                if(!updated)
                    return;

                //Write
                using(MemoryStream dataStream = new MemoryStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(dataStream, System.Text.Encoding.UTF8))
                    {
                        while(content.Count > 0)
                        {
                            streamWriter.WriteLine(content[0]);
                            content.RemoveAt(0);
                        }
                    }
                    File.WriteAllBytes(filePath, dataStream.ToArray());
                }
                AssetDatabase.Refresh();

                Configuration.SetCompileDirectives();
            }
        }

        private static bool ParseFeature(List<string> content, int startIndex, bool enable)
        {
            int featureEnd = startIndex + 5;
            bool updated = false;
            if(enable)
            {
                for(int i = startIndex; i <= featureEnd; i++)
                {
                    if(content[i] == null)
                        break;

                    string s = content[i];
                    content[i] = content[i].Replace("/*!!", "//!!");
                    content[i] = content[i].Replace("$$*/", "//$$");
                    updated = s != content[i];
                }
            }
            else
            {
                for(int i = startIndex; i <= featureEnd; i++)
                {
                    if(content[i] == null)
                        break;

                    string s = content[i];
                    content[i] = content[i].Replace("//!!", "/*!!");
                    content[i] = content[i].Replace("//$$", "$$*/");
                    updated = s != content[i];
                }
            }

            return updated;
        }
        
        private readonly static List<GlobalShaderFeatureBase> _globalShaderFeaturesTemplate = new List<GlobalShaderFeatureBase>()
        {
            //keep features in sync with the shader files
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP_OFF", "MK_COMBINE_VERTEX_COLOR_WITH_ALBEDO_MAP" }, new List<string>() {}, "Multiply vertex color with albedo", "Multiplies vertex color into the albedo Color."),
            new GlobalShaderFeatureOutlineFading(MGlobalShaderFeatureOutlineFadingMode.Off, new List<string>() { "MK_OUTLINE_FADING_OFF", "MK_OUTLINE_FADING_LINEAR", "MK_OUTLINE_FADING_EXPONENTIAL", "MK_OUTLINE_FADING_INVERSE_EXPONENTIAL" }, new List<string>() { "MK_NULL", "MK_TOON_OUTLINE_FADING_LINEAR", "MK_TOON_OUTLINE_FADING_EXPONENTIAL", "MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL" }, "Outline Distance Fading", "Enable outline distance based fading."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_POINT_FILTERING_OFF", "MK_POINT_FILTERING" }, new List<string>() {}, "Force Point Filtering", "Forces point filtering on textures."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_DISSOLVE_PROJECTION_SCREEN_SPACE_OFF", "MK_DISSOLVE_PROJECTION_SCREEN_SPACE" }, new List<string>() {}, "Dissolve Projection Screen Space", "Forces dissolve projection into screen space."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.On, new List<string>() { "MK_LOCAL_ANTIALIASING_OFF", "MK_LOCAL_ANTIALIASING" }, new List<string>() {}, "Enable Local Antialiasing", "Enables local antialiasing except for mobile devices."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_STYLIZE_SYSTEM_SHADOWS_OFF", "MK_STYLIZE_SYSTEM_SHADOWS" }, new List<string>() {}, "Stylize System Shadows", "Stylizes the system shadows like the lighting. Be careful with the thresholds!"),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_LEGACY_NOISE_OFF", "MK_LEGACY_NOISE" }, new List<string>() {}, "Legacy Noise", "Enables the legacy noise calculation for animation and outline noise."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_REGULAR_SCREEN_SPACE_OFF", "MK_REGULAR_SCREEN_SPACE" }, new List<string>() {}, "Regular Screen Space UV", "Enables the regular screen space uv for screen spaced effects instead MK Toons customized screen space uv."),
            //Conditional features
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_LEGACY_SCREEN_SCALING_OFF", "MK_LEGACY_SCREEN_SCALING" }, new List<string>() {}, "Legacy Screen Scaling", "Enables legacy screen spaced scaling for artistic textures and outlines in clip space."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_MULTI_PASS_STEREO_SCALING_OFF", "MK_MULTI_PASS_STEREO_SCALING" }, new List<string>() {}, "Multi Pass Scaling", "Enables correct scaling for artistic textures and outlines in clip space. \n\n This should only be enabled if your XR Render Mode is set to \"Multi Pass\". \n\n This is required because there is no workaround to detect the \"Multi Pass\" rendering mode, when using XR."),
            new GlobalShaderFeature(GlobalShaderFeatureMode.Off, new List<string>() { "MK_LINEAR_lIGHT_DISTANCE_ATTENUATION_OFF", "MK_LINEAR_lIGHT_DISTANCE_ATTENUATION" }, new List<string>() {}, "Linear Light Attenuation", "Changes the light attenuation for non-directional lights more linear (Approximately like builtin renderpipeline).")
        };

        [UnityEngine.SerializeReference]
        private List<GlobalShaderFeatureBase> _globalShaderFeatures = new List<GlobalShaderFeatureBase>();
        [UnityEngine.SerializeReference][HideInInspector]
        private List<GlobalShaderFeatureBase> _globalShaderFeaturesTemp = new List<GlobalShaderFeatureBase>();

        public static void BeginRegisterChangesOnGlobalShaderFeatures()
        {
            if(isReady)
            {
                _instance._globalShaderFeaturesTemp = new List<GlobalShaderFeatureBase>();
                for(int i = 0; i < _instance._globalShaderFeatures.Count; i++)
                {
                    List<string> identifiers = new List<string>();
                    for(int id = 0; id < _instance._globalShaderFeatures[i].identifiers.Count; id++)
                    {
                        identifiers.Add(_instance._globalShaderFeatures[i].identifiers[id]);
                    }
                    List<string> compileDirectives = new List<string>();
                    for(int id = 0; id < _instance._globalShaderFeatures[i].compileDirectives.Count; id++)
                    {
                        compileDirectives.Add(_instance._globalShaderFeatures[i].compileDirectives[id]);
                    }
                    _instance._globalShaderFeaturesTemp.Add(new GlobalShaderFeature(_instance._globalShaderFeatures[i].modeEnum, identifiers, compileDirectives, _instance._globalShaderFeatures[i].name, _instance._globalShaderFeatures[i].tooltip));
                }
                SaveInstance();
            }
        }

        public static bool CheckGlobalShaderFeaturesChanges()
        {
            if(isReady)
            {
                bool hasChanged = false;
                for(int i = 0; i < _instance._globalShaderFeatures.Count; i++)
                {
                    if(_instance._globalShaderFeaturesTemp[i].mode != _instance._globalShaderFeatures[i].mode)
                        hasChanged = true;
                }
                return hasChanged;
            }
            else
            {
                return false;
            }
        }

        //List has to be updated once a feature is added
        //[MenuItem("MK/Install Wizard/Update Global Shader Features List")]
        private static void UpdateGlobalShaderFeatures()
        {
            if(isReady)
            {
                _instance._globalShaderFeatures = new List<GlobalShaderFeatureBase>(_globalShaderFeaturesTemplate);
                SaveInstance();
                BeginRegisterChangesOnGlobalShaderFeatures();
                Debug.Log("Global Shader Features List Updated");
            }
        }

        public static void DrawGlobalShaderFeaturesInspector()
        {
            if(isReady)
            {   
                //linear atten is only for URP
                for(int i = 0; i < _instance._globalShaderFeatures.Count - 2; i++)
                {
                    _instance._globalShaderFeatures[i].DrawInspector();
                }
                if((int) (System.Object) _instance._globalShaderFeatures[_instance._globalShaderFeatures.Count - 3].modeEnum < 1)
                    _instance._globalShaderFeatures[_instance._globalShaderFeatures.Count - 2].DrawInspector();
                if(_instance._renderPipeline == RenderPipeline.Universal)
                    _instance._globalShaderFeatures[_instance._globalShaderFeatures.Count - 1].DrawInspector();
            }
        }

        private static void LogAssetNotFoundError()
        {
            Debug.LogError("Could not find Install Wizard Configuration Asset, please try to import the package again, if this issue persists.");
        }

        private static Configuration _instance = null;
        
        internal static Configuration TryGetInstance()
        {
            if(_instance == null)
            {
                string[] _guids = AssetDatabase.FindAssets("t:" + typeof(Configuration).Namespace + ".Configuration", null);
                if(_guids.Length > 0)
                {
                    _instance = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guids[0]), typeof(Configuration)) as Configuration;
                    if(_instance != null)
                        return _instance;
                    else
                    {
                        LogAssetNotFoundError();
                        return null;
                    }
                }
                else
                {
                    LogAssetNotFoundError();
                    return null;
                }
            }
            else
                return _instance;
        }

        internal static string TryGetPath()
        {
            if(isReady)
            {
                return AssetDatabase.GetAssetPath(_instance);
            }
            else
            {
                return string.Empty;
            }
        }

        internal static Texture2D TryGetTitleImage()
        {
            if(isReady)
            {
                return _instance._titleImage;
            }
            else
            {
                return null;
            }
        }

        internal static ExampleContainer[] TryGetExamples()
        {
            if(isReady)
            {
                return _instance._examples;
            }
            else
            {
                return null;
            }
        }

        internal static bool TryGetDisablePerOutlinesWarning()
        {
            if(isReady)
            {
                return _instance.disablePerObjectOutlinesWarning;
            }
            else
            {
                return false;
            }
        }
        internal static void TrySetDisablePerOutlinesWarning(bool v)
        {
            if(isReady)
            {
                _instance.disablePerObjectOutlinesWarning = v;
                SaveInstance();
            }
        }

        internal static bool TryGetShowInstallerOnReload()
        {
            if(isReady)
            {
                return _instance.showInstallerOnReload;
            }
            else
            {
                return false;
            }
        }
        internal static void TrySetShowInstallerOnReload(bool v)
        {
            if(isReady)
            {
                _instance.showInstallerOnReload = v;
                SaveInstance();
            }
        }
        internal static RenderPipeline TryGetRenderPipeline()
        {
            if(isReady)
            {
                return _instance._renderPipeline;
            }
            else
            {
                return RenderPipeline.Built_in;
            }
        }
        internal static void TrySetRenderPipeline(RenderPipeline v)
        {
            if(isReady)
            {
                _instance._renderPipeline = v;

                SaveInstance();
            }
        }

        internal static void SaveInstance()
        {
            if(isReady)
            {
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        internal static void ImportShaders(RenderPipeline renderPipeline)
        {
            if(isReady)
            {
                switch(renderPipeline)
                {
                    case RenderPipeline.Built_in:
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageBuiltin), false);
                    break;
                    //case RenderPipeline.Lightweight:
                    //    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageLWRP), false);
                    //break;
                    case RenderPipeline.Universal:
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._basePackageURP), false);
                    break;
                    default:
                    //All cases should be handled
                    break;
                }
                TrySetShowInstallerOnReload(false);
            }
        }

        internal static void ImportExamples(RenderPipeline renderPipeline)
        {
            if(isReady)
            {
                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageInc), false);
                switch(renderPipeline)
                {
                    case RenderPipeline.Built_in:
                        #if UNITY_2023_2_OR_NEWER
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageBuiltin_2023_2_Or_Newer), false);
                        #else
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageBuiltin), false);
                        #endif
                    break;
                    case RenderPipeline.Universal:
                        #if UNITY_2023_2_OR_NEWER
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP_2023_2_Or_Newer), false);
                        #else
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP), false);
                        #endif
                    break;
                    default:
                    //All cases should be handled
                    break;
                }
            }
        }

        internal static void OpenReadMe()
        {
            AssetDatabase.OpenAsset(_instance._readMe);
        }

        #if UNITY_2021_2_OR_NEWER
        public static void SetCompileDirectives()
        {
            if(isReady)
            {
                IEnumerable<BuildTargetGroup> buildTargetsGroups = System.Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().Where(b => b != BuildTargetGroup.Unknown).Where(b => !CheckIfObsolete(b));

                foreach (BuildTargetGroup buildTargetGroup in buildTargetsGroups)
                {
                    //string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
                    string[] defines;
                    
                    UnityEditor.Build.NamedBuildTarget namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                    PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out defines);

                    List<string> definesList = defines.Where(b => !string.IsNullOrEmpty(b)).ToList();

                    for(int i = 0; i < _instance._globalShaderFeatures.Count; i++)
                    {
                        GlobalShaderFeatureBase gsfb = _instance._globalShaderFeatures[i];
                        for(int cd = 0; cd < gsfb.compileDirectives.Count; cd++)
                        {
                            if(gsfb.compileDirectives.Count > 0)
                            {
                                if (gsfb.compileDirectives[gsfb.mode] == null)
                                    continue;

                                if (definesList.Contains(gsfb.compileDirectives[cd]))
                                    definesList.Remove(gsfb.compileDirectives[cd]);
                                
                                if(gsfb.mode > 0 && gsfb.mode == cd)
                                {
                                    definesList.Add(gsfb.compileDirectives[gsfb.mode]);
                                }
                            }
                        }
                    }
                    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesList.ToArray<string>());
                }
            }
        }

        private static bool CheckIfObsolete(BuildTargetGroup buildTargetGroup)
        {
            System.Object[] attribute = typeof(BuildTargetGroup).GetField(buildTargetGroup.ToString()).GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
            return attribute != null && attribute.Length > 0;
        }

        #else
        public static void SetCompileDirectives()
        {
            if(isReady)
            {
                IEnumerable<BuildTargetGroup> buildTargetsGroups = System.Enum.GetValues(typeof(BuildTargetGroup)).Cast<BuildTargetGroup>().Where(b => b != BuildTargetGroup.Unknown).Where(b => !CheckIfObsolete(b));

                foreach (BuildTargetGroup buildTargetGroup in buildTargetsGroups)
                {
                    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();

                    List<string> definesList = defines.Split(';', ' ').Where(b => !string.IsNullOrEmpty(b)).ToList();

                    for(int i = 0; i < _instance._globalShaderFeatures.Count; i++)
                    {
                        GlobalShaderFeatureBase gsfb = _instance._globalShaderFeatures[i];
                        for(int cd = 0; cd < gsfb.compileDirectives.Count; cd++)
                        {
                            if(gsfb.compileDirectives.Count > 0)
                            {
                                if (gsfb.compileDirectives[gsfb.mode] == null)
                                    continue;

                                if (definesList.Contains(gsfb.compileDirectives[cd]))
                                    definesList.Remove(gsfb.compileDirectives[cd]);
                                
                                if(gsfb.mode > 0 && gsfb.mode == cd)
                                {
                                    definesList.Add(gsfb.compileDirectives[gsfb.mode]);
                                }
                            }
                        }
                    }
                    if(definesList.Count > 0)
                        defines = definesList.Aggregate((a, b) => a + ";" + b);
                    else
                        defines = string.Empty;

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
                }
            }
        }

        private static bool CheckIfObsolete(BuildTargetGroup buildTargetGroup)
        {
            System.Object[] attribute = typeof(BuildTargetGroup).GetField(buildTargetGroup.ToString()).GetCustomAttributes(typeof(System.ObsoleteAttribute), false);
            return attribute != null && attribute.Length > 0;
        }
        #endif

        #if MK_URP
        public static void ShowURPOutlineWarning()
        {   
            if(Configuration.TryGetDisablePerOutlinesWarning() || GraphicsSettings.currentRenderPipeline == null)
                return;

            bool MKToonPerObjectOutlinesRendererFeatureFound = false;

            try
            {
                RenderPipelineAsset renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                ScriptableRenderer defaultRenderer = (renderPipelineAsset as UniversalRenderPipelineAsset).scriptableRenderer;

                var scriptableRendererFeaturesProperty = typeof(ScriptableRenderer).GetProperty("rendererFeatures", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                System.Collections.Generic.List<ScriptableRendererFeature> defaultScriptableRendererFeatures = scriptableRendererFeaturesProperty.GetValue(defaultRenderer) as System.Collections.Generic.List<ScriptableRendererFeature>;
                
                foreach(ScriptableRendererFeature srf in defaultScriptableRendererFeatures)
                {
                    if(srf.GetType() == typeof(MK.Toon.URP.MKToonPerObjectOutlines))
                    {
                        MKToonPerObjectOutlinesRendererFeatureFound = true;
                        break;
                    }
                }
            }
            catch {}

            if(!MKToonPerObjectOutlinesRendererFeatureFound)
            {
                System.Text.StringBuilder warning = new System.Text.StringBuilder("Could not find the MK Toon Per Object Outlines Renderer Feature on your default renderer asset.");
                warning.Append(System.Environment.NewLine);
                warning.Append("The MK Toon Per Object Outline Renderer Feature is required in order to make the outlines rendered in a proper way");
                warning.Append(System.Environment.NewLine);
                warning.Append(System.Environment.NewLine);
                warning.Append("It's recommend to attach the MK Toon Per Object Outlines Renderer Feature to every of your renderer assets that should render the per object outlines of MK Toon. Otherwise outlines can't be rendered.");
                EditorGUILayout.HelpBox(warning.ToString(), MessageType.Warning);

                if(GUILayout.Button("Find all Renderer Assets in Project Window"))
                {
                    FindAllURPRendererAssetsInProjectWindow();
                }
                if(GUILayout.Button("Select active default Renderer Asset"))
                {
                    SelectDefaultRendererAsset();
                }
            }
        }

        [MenuItem("Window/MK/Toon/Select Default URP Renderer Asset")]
        public static void SelectDefaultRendererAsset()
        {
            try
            {
                RenderPipelineAsset renderPipelineAsset = GraphicsSettings.currentRenderPipeline;
                UniversalRenderPipelineAsset universalRenderPipelineAsset = renderPipelineAsset as UniversalRenderPipelineAsset;
                var defaultScriptableRendererIndexField = typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                int defaultRendererIndex = (int) defaultScriptableRendererIndexField.GetValue(universalRenderPipelineAsset);
                var scriptableRendererDataList = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                ScriptableRendererData[] scriptableRenderers = scriptableRendererDataList.GetValue(universalRenderPipelineAsset) as ScriptableRendererData[];
                Selection.objects = new UnityEngine.Object[] { scriptableRenderers[defaultRendererIndex] };
                EditorGUIUtility.PingObject(scriptableRenderers[defaultRendererIndex]);
            }
            catch 
            {
                Debug.LogWarning("Could not find your default URP Renderer Asset");
            }
        }

        [MenuItem("Window/MK/Toon/Find All URP Renderer Assets in Project Window")]
        public static void FindAllURPRendererAssetsInProjectWindow()
        {
            try
            {
                System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
                EditorWindow window = EditorWindow.GetWindow(projectBrowserType);
                System.Reflection.MethodInfo searchMethod = projectBrowserType.GetMethod("SetSearch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new System.Type[] { typeof(string) }, null);
                searchMethod.Invoke(window, new object[] { "t:UnityEngine.Rendering.Universal.ScriptableRendererData" });
            }
            catch
            {
                Debug.LogWarning("Unable to find your URP Renderer Assets");
            }
        }
        #endif

        #pragma warning restore CS0414
    }
}
#endif