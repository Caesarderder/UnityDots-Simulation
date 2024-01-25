#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR && !GRIFFIN_MESH_TO_FILE
using UnityEngine;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class MeshToFilePlaceholder
    {
        public static string GetExtensionName()
        {
            return "Mesh To File";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return
                "Mesh To File is a handy tool for exporting Unity meshes to 3D files. " +
                "This extension can be used for exporting Polaris terrain mesh.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                "customer@pinwheel.studio",
                "Griffin Extension - Mesh To File",
                "YOUR_MESSAGE_HERE");
        }

        public static void Button_GetMeshToFile()
        {
            string url = "http://bit.ly/2pW6kiI";
            Application.OpenURL(url);
        }
    }
}
#endif
#endif
