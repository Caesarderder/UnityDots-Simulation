#if GRIFFIN && UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Griffin;
using Pinwheel.Griffin.URP;

namespace Pinwheel.Griffin.URP.GriffinExtension
{
    public static class GriffinUrpSupport
    {
        public static string GetExtensionName()
        {
            return "Universal Render Pipeline Support";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return "Adding support for URP.\n" +
                "Requires Unity 2019.3 or above.";
        }

        public static string GetVersion()
        {
            return "2020.1";
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                GCommon.SUPPORT_EMAIL,
                "[Polaris V2] LWRP Support",
                "YOUR_MESSAGE_HERE");
        }

        public static void OnGUI()
        {
            bool isUnity20193orNewer = false;
#if UNITY_2019_3_OR_NEWER 
            isUnity20193orNewer = true;
#endif

            GUI.enabled = isUnity20193orNewer;
            if (GUILayout.Button("Install"))
            {
                GGriffinUrpInstaller.Install();
            }

            GUI.enabled = isUnity20193orNewer && GCommon.CurrentRenderPipeline == GRenderPipelineType.Universal;
            string upgradeButtonLabel =
                GCommon.CurrentRenderPipeline == GRenderPipelineType.Universal ?
                "Upgrade Terrain Materials" :
                "Enable URP to upgrade terrain materials.";

            if (GUILayout.Button(upgradeButtonLabel))
            {
                GGriffinUrpInstaller.UpgradeTerrainMaterialInProject();
            }

            GUI.enabled = true;
        }
    }
}
#endif
