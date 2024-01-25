#if GRIFFIN
using System.Collections.Generic;
using System.Reflection;
using Type = System.Type;

namespace Pinwheel.Griffin.ExtensionSystem
{
    public class GExtensionInfo
    {
        public const string GET_PUBLISHER_METHOD_NAME = "GetPublisherName";
        public const string GET_EXTENSION_NAME_METHOD_NAME = "GetExtensionName";
        public const string GET_DESCRIPTION_METHOD_NAME = "GetDescription";
        public const string GET_VERSION_METHOD_NAME = "GetVersion";
        public const string OPEN_USER_GUIDE_METHOD_NAME = "OpenUserGuide";
        public const string OPEN_SUPPORT_LINK_METHOD_NAME = "OpenSupportLink";
        public const string GUI_METHOD = "OnGUI";
        public const string BUTTON_METHOD_PREFIX = "Button_";

        private string name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    return "Unknown Extension";
                }
                else
                {
                    return name;
                }
            }
            private set
            {
                name = value;
            }
        }

        private string publisher;
        public string Publisher
        {
            get
            {
                if (string.IsNullOrEmpty(publisher))
                {
                    return "Unknown Publisher";
                }
                else
                {
                    return publisher;
                }
            }
            private set
            {
                publisher = value;
            }
        }

        private string version;
        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    return "0";
                }
                else
                {
                    return version;
                }
            }
            private set
            {
                version = value;
            }
        }

        private string description;
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                {
                    return "No description available.";
                }
                else
                {
                    return description;
                }
            }
            private set
            {
                description = value;
            }
        }

        private Type extensionClass;
        public Type ExtensionClass
        {
            get
            {
                return extensionClass;
            }
            set
            {
                extensionClass = value;
            }
        }

        private MethodInfo openUserGuideMethod;
        public MethodInfo OpenUserGuideMethod
        {
            get
            {
                return openUserGuideMethod;
            }
            set
            {
                openUserGuideMethod = value;
            }
        }

        private MethodInfo openSupportLinkMethod;
        public MethodInfo OpenSupportLinkMethod
        {
            get
            {
                return openSupportLinkMethod;
            }
            set
            {
                openSupportLinkMethod = value;
            }
        }

        private MethodInfo guiMethod;
        public MethodInfo GuiMethod
        {
            get
            {
                return guiMethod;
            }
            set
            {
                guiMethod = value;
            }
        }

        private List<MethodInfo> buttonMethods;
        public List<MethodInfo> ButtonMethods
        {
            get
            {
                if (buttonMethods == null)
                    buttonMethods = new List<MethodInfo>();
                return buttonMethods;
            }
            private set
            {
                buttonMethods = value;
            }
        }

        public static GExtensionInfo CreateFromType(Type t)
        {
            GExtensionInfo info = new GExtensionInfo();

            MethodInfo getPublisherMethod = t.GetMethod(
                GET_PUBLISHER_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            if (getPublisherMethod != null &&
                getPublisherMethod.ReturnType == typeof(string))
            {
                info.Publisher = (string)getPublisherMethod.Invoke(null, null);
            }

            MethodInfo getNameMethod = t.GetMethod(
                GET_EXTENSION_NAME_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            if (getNameMethod != null &&
                getNameMethod.ReturnType == typeof(string))
            {
                info.Name = (string)getNameMethod.Invoke(null, null);
            }

            MethodInfo getDescriptionMethod = t.GetMethod(
                GET_DESCRIPTION_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            if (getDescriptionMethod != null &&
                getDescriptionMethod.ReturnType == typeof(string))
            {
                info.Description = (string)getDescriptionMethod.Invoke(null, null);
            }

            MethodInfo getVersionMethod = t.GetMethod(
                GET_VERSION_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            if (getVersionMethod != null &&
                getVersionMethod.ReturnType == typeof(string))
            {
                info.Version = (string)getVersionMethod.Invoke(null, null);
            }

            MethodInfo openDocMethod = t.GetMethod(
                OPEN_USER_GUIDE_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            info.OpenUserGuideMethod = openDocMethod;

            MethodInfo supportMethod = t.GetMethod(
                OPEN_SUPPORT_LINK_METHOD_NAME,
                BindingFlags.Public | BindingFlags.Static);
            info.OpenSupportLinkMethod = supportMethod;

            MethodInfo guiMethod = t.GetMethod(
                GUI_METHOD,
                BindingFlags.Public | BindingFlags.Static);
            info.GuiMethod = guiMethod;

            List<MethodInfo> allMethods = new List<MethodInfo>(t.GetMethods(BindingFlags.Public | BindingFlags.Static));
            info.buttonMethods = allMethods.FindAll(m => m.Name.StartsWith(BUTTON_METHOD_PREFIX));

            return info;
        }
    }
}
#endif
