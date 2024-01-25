#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Griffin.GroupTool;
using Pinwheel.Griffin.PaintTool;
using Pinwheel.Griffin.SplineTool;
using Pinwheel.Griffin.StampTool;

namespace Pinwheel.Griffin.Wizard
{
    public class GWizardWindow : EditorWindow
    {
        private const int TAB_CREATE = 0;
        private const int TAB_SET_SHADER = 1;
        private const int TAB_EXTENSION = 2;

        private int selectedTab;
        private readonly string[] tabLabels = new string[]
        {
            "Create Level",
            "Set Shader",
            "Extension"
        };

        private void OnEnable()
        {
            GExtensionTabDrawer.ReloadExtension();
            CheckForUrpFirstTimeImport();
        }

        private static GWizardWindow CreateWindow()
        {
            GWizardWindow window = GetWindow<GWizardWindow>();
            Texture2D icon = EditorGUIUtility.isProSkin ?
                GEditorSkin.Instance.GetTexture("IconWhite") :
                GEditorSkin.Instance.GetTexture("IconBlack");
            window.titleContent = new GUIContent(" " + GVersionInfo.ProductNameAndVersionShort, icon);
            window.minSize = new Vector2(600, 500);
            return window;
        }

        public static void ShowCreateLevelTab(MenuCommand menuCmd)
        {
            GWizardWindow window = CreateWindow();
            GCreateLevelTabDrawer.menuCmd = menuCmd;
            window.selectedTab = TAB_CREATE;
            window.Show();
        }

        public static void ShowSetShaderTab(GStylizedTerrain terrain)
        {
            GEditorSettings.Instance.wizardTools.setShaderTerrain = terrain;

            GWizardWindow window = CreateWindow();
            window.selectedTab = TAB_SET_SHADER;
            GSetShaderTabDrawer.bulkSetShader = false;
            window.Show();
        }

        public static void ShowSetShaderTab(int groupId)
        {
            GEditorSettings.Instance.wizardTools.setShaderGroupId = groupId;

            GWizardWindow window = CreateWindow();
            window.selectedTab = TAB_SET_SHADER;
            GSetShaderTabDrawer.bulkSetShader = true;
            window.Show();
        }

        public static void ShowExtensionTab()
        {
            GWizardWindow window = CreateWindow();
            window.selectedTab = TAB_EXTENSION;
            window.Show();
        }

        public void OnGUI()
        {
            DrawTabs();
            if (selectedTab == TAB_CREATE)
            {
                GCreateLevelTabDrawer.Draw();
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
            else if (selectedTab == TAB_SET_SHADER)
            {
                GSetShaderTabDrawer.Draw();
                EditorUtility.SetDirty(GEditorSettings.Instance);
            }
            else if (selectedTab == TAB_EXTENSION)
            {
                GExtensionTabDrawer.Draw();
            }
        }

        private void DrawTabs()
        {
            GEditorCommon.Space();
            selectedTab = GEditorCommon.Tabs(selectedTab, tabLabels);
            GEditorCommon.Space();
        }

        private void CheckForUrpFirstTimeImport()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (GCommon.CurrentRenderPipeline == GRenderPipelineType.Universal)
            {
                if (!GPackageInitializer.isUrpSupportInstalled)
                {
                    GUrpPackageImporter.Import();
                }
            }
        }
    }
}
#endif
