#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor;

namespace Pinwheel.Griffin.Wizard
{
    public static class GUrpPackageImporter
    {
        public static void Import()
        {
            string packagePath = GEditorSettings.Instance.renderPipelines.GetUrpPackagePath();
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.Log("URP Support package not found. Please re-install Polaris.");
                return;
            }

            AssetDatabase.ImportPackage(packagePath, false);
        }
    }
}
#endif
