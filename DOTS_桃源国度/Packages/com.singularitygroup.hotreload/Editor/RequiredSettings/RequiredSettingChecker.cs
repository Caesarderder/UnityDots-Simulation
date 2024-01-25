using System.Reflection;
using UnityEditor;
using System;

namespace SingularityGroup.HotReload.Editor {
    // Before Unity 2021.3, value is 0 or 1. Only value of 1 is a problem.
    // From Unity 2021.3 onwards, the key is "kAutoRefreshMode".
    // kAutoRefreshMode options are:
    //   0: disabled
    //   1: enabled 
    //   2: enabled outside playmode
    // 
    // On newer Unity versions, Visual Studio is also checking the kAutoRefresh setting (but it should only check kAutoRefreshMode).
    // This is making hot reload unusable and so this setting needs to also get disabled.
    internal static class AutoRefreshSettingChecker {
        const string autoRefreshKey = "kAutoRefresh";
        #if UNITY_2021_3_OR_NEWER
        const string autoRefreshModeKey = "kAutoRefreshMode";
        #endif
        
        const int desiredValue = 0;

        public static void Apply() {
            if (HotReloadPrefs.AppliedAutoRefresh) {
                return;
            }
            
            var defaultPref = EditorPrefs.GetInt(autoRefreshKey);
            HotReloadPrefs.DefaultAutoRefresh = defaultPref;
            EditorPrefs.SetInt(autoRefreshKey, desiredValue);
            
            #if UNITY_2021_3_OR_NEWER
            var defaultModePref = EditorPrefs.GetInt(autoRefreshModeKey);
            HotReloadPrefs.DefaultAutoRefreshMode = defaultModePref;
            EditorPrefs.SetInt(autoRefreshModeKey, desiredValue);
            #endif

            HotReloadPrefs.AppliedAutoRefresh = true;
        }

        public static void Check() {
            if (!HotReloadPrefs.AppliedAutoRefresh) {
                return;
            }
            
            if (EditorPrefs.GetInt(autoRefreshKey) != desiredValue) {
                HotReloadPrefs.DefaultAutoRefresh = -1;
            }
            
            #if UNITY_2021_3_OR_NEWER
            if (EditorPrefs.GetInt(autoRefreshModeKey) != desiredValue) {
                HotReloadPrefs.DefaultAutoRefreshMode = -1;
            }
            #endif
        }

        public static void Reset() {
            if (!HotReloadPrefs.AppliedAutoRefresh) {
                return;
            }
            
            if (EditorPrefs.GetInt(autoRefreshKey) == desiredValue
                && HotReloadPrefs.DefaultAutoRefresh != -1
            ) {
                EditorPrefs.SetInt(autoRefreshKey, HotReloadPrefs.DefaultAutoRefresh);
            }
            HotReloadPrefs.DefaultAutoRefresh = -1;
            
            #if UNITY_2021_3_OR_NEWER
            if (EditorPrefs.GetInt(autoRefreshModeKey) == desiredValue 
                && HotReloadPrefs.DefaultAutoRefreshMode != -1
            ) {
                EditorPrefs.SetInt(autoRefreshModeKey, HotReloadPrefs.DefaultAutoRefreshMode);
            }
            HotReloadPrefs.DefaultAutoRefreshMode = -1;
            #endif

            HotReloadPrefs.AppliedAutoRefresh = false;
        }
    }
    
    internal static class ScriptCompilationSettingChecker {
        const string scriptCompilationKey = "ScriptCompilationDuringPlay";
        
        const int recompileAndContinuePlaying = 0;
        static int? recompileAfterFinishedPlaying = (int?)typeof(EditorWindow).Assembly.GetType("UnityEditor.ScriptChangesDuringPlayOptions")?
            .GetField("RecompileAfterFinishedPlaying", BindingFlags.Static | BindingFlags.Public)?
            .GetValue(null);

        public static void Apply() {
            if (HotReloadPrefs.AppliedScriptCompilation) {
                return;
            }
            
            var defaultPref = EditorPrefs.GetInt(scriptCompilationKey);
            HotReloadPrefs.DefaultScriptCompilation = defaultPref;
            EditorPrefs.SetInt(scriptCompilationKey, GetRecommendedAutoScriptCompilationKey());

            HotReloadPrefs.AppliedScriptCompilation = true;
        }
        
        public static void Check() {
            if (!HotReloadPrefs.AppliedScriptCompilation) {
                return;
            }
            if (EditorPrefs.GetInt(scriptCompilationKey) != GetRecommendedAutoScriptCompilationKey()) {
                HotReloadPrefs.DefaultScriptCompilation = -1;
            }
        }

        public static void Reset() {
            if (!HotReloadPrefs.AppliedScriptCompilation) {
                return;
            }
            if (EditorPrefs.GetInt(scriptCompilationKey) == GetRecommendedAutoScriptCompilationKey()
                && HotReloadPrefs.DefaultScriptCompilation != -1
            ) {
                EditorPrefs.SetInt(scriptCompilationKey, HotReloadPrefs.DefaultScriptCompilation);
            }
            HotReloadPrefs.DefaultScriptCompilation = -1;
            
            HotReloadPrefs.AppliedScriptCompilation = false;
        }
        
        static int GetRecommendedAutoScriptCompilationKey() {
            // In some projects due to an unknown reason both "RecompileAndContinuePlaying" and "StopPlayingAndRecompile" cause issues
            // We were unable to identify the cause and therefore we always try to default to "RecompileAfterFinishedPlaying"
            // The exact issue users are experiencing is that domain reload happens shortly after entering play mode causing nullrefs
            return recompileAfterFinishedPlaying ?? recompileAndContinuePlaying;
        }
    }
}