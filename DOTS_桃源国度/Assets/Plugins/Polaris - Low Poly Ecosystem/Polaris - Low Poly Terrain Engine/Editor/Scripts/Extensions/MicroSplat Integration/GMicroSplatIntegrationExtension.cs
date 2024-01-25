#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#if __MICROSPLAT__
using JBooth.MicroSplat;
#endif

namespace Pinwheel.Griffin.MicroSplat.GriffinExtension
{
    public static class GMicroSplatIntegrationExtension
    {
        public static string GetExtensionName()
        {
            return "MicroSplat Integration";
        }

        public static string GetPublisherName()
        {
            return "Jason Booth";
        }

        public static string GetDescription()
        {
            return "Provide support and make it easier to use MicroSplat shaders on Polaris terrain.";
        }

        public static string GetVersion()
        {
            return "1.0.0";
        }

        public static void OpenUserGuide()
        {
            Application.OpenURL("https://docs.google.com/document/d/1LQooyrEl2S5qP3w2cvX0RYy1CQvUs6mIBACJ8wNhSnE/edit#heading=h.1mgw1o27bmpg");
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                GCommon.SUPPORT_EMAIL,
                "[Polaris V2] MicroSplat Integration",
                "YOUR_MESSAGE_HERE");
        }

        public static void OnGUI()
        {
            if (GUILayout.Button("Get MicroSplat Core Module"))
            {
                Application.OpenURL(GAssetLink.MICRO_SPLAT);
            }
            if (GUILayout.Button("Get Polaris Integration Module"))
            {
                Application.OpenURL(GAssetLink.MICRO_SPLAT_INTEGRATION);
            }
        }

#if __MICROSPLAT_POLARIS__
        [DidReloadScripts]
        public static void OnScriptReload()
        {
            GStylizedTerrainInspector.GUIInject += InjectTerrainGUI;
        }

        private static void InjectTerrainGUI(GStylizedTerrain terrain, int order)
        {
            if (order != 3)
                return;
            string label = "MicroSplat Integration";
            string id = "terrain-gui-microsplat-integration";
            GEditorCommon.Foldout(label, false, id, () =>
            {
                if (GUILayout.Button("Open Editor"))
                {
                    GMicroSplatSetupWindow.ShowWindow();
                }
            });
        }
#endif
    }
}
#endif
#endif
