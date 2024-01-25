#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR
using UnityEngine;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class MegaWorldPlaceholder
    {
        public static string GetExtensionName()
        {
            return "Mega World Integration";
        }

        public static string GetPublisherName()
        {
            return "Vladislav Tsurikov";
        }

        public static string GetDescription()
        {
            return "Mega World is an ecosystem of tools for lightning fast creation of beautiful and realistic results and hyper-optimized object rendering.\n" +
                "This integration enable foliage rendering using MW's Quadro Renderer.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            string url = "https://assetstore.unity.com/publishers/45764";
            Application.OpenURL(url);
        }

        public static void Button_GetMegaWorld()
        {
            string url = "https://assetstore.unity.com/packages/tools/terrain/mega-world-163756";
            Application.OpenURL(url);
        }
    }
}
#endif
#endif
