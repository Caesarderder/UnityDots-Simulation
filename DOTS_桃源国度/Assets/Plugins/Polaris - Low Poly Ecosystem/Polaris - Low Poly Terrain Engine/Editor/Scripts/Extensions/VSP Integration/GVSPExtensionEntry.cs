#if GRIFFIN && UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.VegetationStudioPro.GriffinExtension
{
    public static class GVSPExtensionEntry
    {
        public static string GetExtensionName()
        {
            return "Vegetation Studio Pro Integration";
        }
        
        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }
        
        public static string GetDescription()
        {
            return 
                "Providing quick setup specific for mesh-based terrain and many utility tasks.\n" +
                "Requires Polaris 2020.2.6 and up.";
        }
        
        public static string GetVersion()
        {
            return "1.1.0";
        }
        
        public static void OpenUserGuide()
        {
            Application.OpenURL("https://docs.google.com/document/d/1LQooyrEl2S5qP3w2cvX0RYy1CQvUs6mIBACJ8wNhSnE/edit#heading=h.qvs8ncv8wf1n");
        }
        
        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                GCommon.SUPPORT_EMAIL,
                "[Polaris] VSP Integration",
                "[YOUR_MESSAGE_HERE]");
        }
        
        public static void OnGUI()
        {
#if VEGETATION_STUDIO_PRO
            if (GUILayout.Button("Open Editor"))
            {
                GVSPIntegrationEditor.ShowWindow();
            }
#else
            if (GUILayout.Button("Get Vegetation Studio Pro"))
            {
                Application.OpenURL(GAssetLink.VEGETATION_STUDIO_PRO);
            }
#endif
        }
    }
}
#endif
