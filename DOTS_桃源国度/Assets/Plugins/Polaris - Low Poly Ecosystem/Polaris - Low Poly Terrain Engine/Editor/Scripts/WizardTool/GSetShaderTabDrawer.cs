#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


namespace Pinwheel.Griffin.Wizard
{
    public static class GSetShaderTabDrawer
    {
        private class GSetShaderTabGUI
        {
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the terrain group to change the material");
            public static readonly GUIContent TERRAIN = new GUIContent("Terrain", "The terrain to change its material");

            public static readonly GUIContent SET_BTN = new GUIContent("Set");
        }

        internal static bool bulkSetShader = true;

        internal static void Draw()
        {
            GEditorSettings.WizardToolsSettings settings = GEditorSettings.Instance.wizardTools;
            if (bulkSetShader)
            {
                settings.setShaderGroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GSetShaderTabGUI.GROUP_ID, settings.setShaderGroupId);
            }
            else
            {
                settings.setShaderTerrain = EditorGUILayout.ObjectField(GSetShaderTabGUI.TERRAIN, settings.setShaderTerrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            }
            GWizardEditorCommon.DrawMaterialSettingsGUI();

            if (GUILayout.Button(GSetShaderTabGUI.SET_BTN))
            {
                if (bulkSetShader)
                {
                    GWizard.SetShader(settings.setShaderGroupId);
                }
                else
                {
                    GWizard.SetShader(settings.setShaderTerrain);
                }
            }
        }
    }
}
#endif
