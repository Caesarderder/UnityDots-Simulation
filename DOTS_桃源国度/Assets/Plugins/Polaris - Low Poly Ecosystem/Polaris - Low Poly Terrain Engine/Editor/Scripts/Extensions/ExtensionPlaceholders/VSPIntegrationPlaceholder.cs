#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR && !GRIFFIN_VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class VSPIntegrationPlaceholder
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
            if (GUILayout.Button("Get Vegetation Studio Pro"))
            {
                Application.OpenURL(GAssetLink.VEGETATION_STUDIO_PRO);
            }
            if (GUILayout.Button("Get Integration Module"))
            {
                Application.OpenURL(GAssetLink.VSP_INTEGRATION);
            }
        }
    }
}
#endif
#endif
