#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR && !GRIFFIN_URP
using UnityEngine;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class UniversalRPSupportPlaceholder
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
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                GCommon.SUPPORT_EMAIL,
                "[Polaris] URP Support",
                "YOUR_MESSAGE_HERE");
        }

        public static void Button_Download()
        {
            string url = "https://assetstore.unity.com/packages/slug/157785";
            Application.OpenURL(url);
        }
    }
}
#endif
#endif
