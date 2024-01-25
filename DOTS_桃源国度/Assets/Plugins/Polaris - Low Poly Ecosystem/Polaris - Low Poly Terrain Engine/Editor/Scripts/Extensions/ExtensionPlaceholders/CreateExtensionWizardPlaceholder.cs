#if GRIFFIN
#if GRIFFIN && UNITY_EDITOR && !GRIFFIN_CSHARP_WIZARD
using UnityEngine;

namespace Pinwheel.Griffin.GriffinExtension
{
    public static class CreateExtensionWizardPlaceholder
    {
        public static string GetExtensionName()
        {
            return "Create Extension Wizard";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return
                "CSharp Wizard is a free and powerful tool for programmer to generate skeleton code in common scenarios. " +
                "It can also be used to generate Griffin Extension script file with required functions defined.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            GEditorCommon.OpenEmailEditor(
                "customer@pinwheel.studio",
                "Griffin Extension - CSharp Wizard",
                "YOUR_MESSAGE_HERE");
        }

        public static void Button_GetCSharpWizardForFree()
        {
            string url = "http://bit.ly/2JfsGTk";
            Application.OpenURL(url);
        }
    }
}
#endif
#endif
