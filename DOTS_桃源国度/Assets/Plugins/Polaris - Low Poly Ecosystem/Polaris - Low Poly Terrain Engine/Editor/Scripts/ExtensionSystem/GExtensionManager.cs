#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Griffin.ExtensionSystem
{
    [InitializeOnLoad]
    public static class GExtensionManager
    {
        private static List<GExtensionInfo> extensions;
        public static List<GExtensionInfo> Extensions
        {
            get
            {
                if (extensions == null)
                {
                    extensions = new List<GExtensionInfo>();
                }
                return new List<GExtensionInfo>(extensions);
            }
            private set
            {
                extensions = value;
            }
        }

        static GExtensionManager()
        {
            GPackageInitializer.Completed += OnPackageInitializeCompleted;
        }

        private static void OnPackageInitializeCompleted()
        {
            ReloadExtensions();
        }

        public static void ReloadExtensions()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();

            extensions = new List<GExtensionInfo>();
            for (int i = 0; i < loadedTypes.Count; ++i)
            {
                Type t = loadedTypes[i];
                if (string.IsNullOrEmpty(t.Namespace))
                    continue;
                if (!t.Namespace.EndsWith(".GriffinExtension"))
                    continue;

                GExtensionInfo info = GExtensionInfo.CreateFromType(t);
                if (!t.Namespace.StartsWith("Pinwheel.") &&
                    info.Publisher.Equals("Pinwheel Studio"))
                {
                    string error = string.Format(
                        "Griffin Extension: Error on initiating '{0}/{1}'. " +
                        "Publisher name 'Pinwheel Studio' is reserved. Please choose another name.",
                        t.Name,
                        info.Name);
                    Debug.Log(error);
                }
                else
                {
                    extensions.Add(info);
                }
            }

            extensions.RemoveAll(e =>
                string.IsNullOrEmpty(e.Publisher) ||
                string.IsNullOrEmpty(e.Name) ||
                e.OpenSupportLinkMethod == null);

            extensions.Sort((e0, e1) => e0.Name.CompareTo(e1.Name));
        }
    }
}
#endif
