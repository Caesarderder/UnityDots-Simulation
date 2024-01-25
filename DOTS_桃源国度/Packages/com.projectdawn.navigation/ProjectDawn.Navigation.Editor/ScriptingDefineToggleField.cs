using UnityEditor;
using UnityEngine;

namespace ProjectDawn.Navigation.Editor
{
    public static class ScriptingDefineToggleField
    {
        public static bool Draw(GUIContent label, string defineSymbol)
        {
            bool hasDefineSymbol = HasScriptingDefineSymbol(defineSymbol);

            EditorGUI.BeginChangeCheck();

            bool value = EditorGUILayout.Toggle(label, hasDefineSymbol);

            if (EditorGUI.EndChangeCheck())
            {
                if (!EditorUtility.DisplayDialog("Confirmation", $"This operation will modify scripting defines by adding/removing define symbol {defineSymbol}", "Yes", "No"))
                {
                    return value;
                }

                if (value)
                {
                    AddScriptingDefineSymbol(defineSymbol);
                }
                else
                {
                    RemoveScriptingDefineSymbol(defineSymbol);
                }
            }

            return value;
        }

        static bool HasScriptingDefineSymbol(string symbol)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            return defines.Contains(symbol);
        }

        static void AddScriptingDefineSymbol(string symbol)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!defines.Contains(symbol))
            {
                defines += ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
            }
        }

        static void RemoveScriptingDefineSymbol(string symbol)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (defines.Contains(symbol))
            {
                defines = defines.Replace(";" + symbol, "").Replace(symbol + ";", "").Replace(symbol, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
            }
        }
    }
}
