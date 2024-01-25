#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR && !POSEIDON
using UnityEngine;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class PoseidonPlaceholder
    {
        public static string GetExtensionName()
        {
            return "Poseidon - Low Poly Water System";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return
                "Poseidon is an user friendly system which help you to create beautiful low poly water-scape. " +
                "Support for Builtin, Lightweight and Universal Render Pipeline.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                "customer@pinwheel.studio",
                "Griffin Extension - Poseidon",
                "YOUR_MESSAGE_HERE");
        }

        public static void Button_GetPoseidon()
        {
            string url = "http://bit.ly/31KXD8b";
            Application.OpenURL(url);
        }
    }
}
#endif
#endif
