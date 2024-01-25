#if GRIFFIN
using UnityEditor;

namespace Pinwheel.Griffin
{
    public class GGenericMenuItem
    {
        public string Name { get; set; }
        public bool IsOn { get; set; }
        public GenericMenu.MenuFunction Action { get; set; }

        public GGenericMenuItem(string name, bool isOn, GenericMenu.MenuFunction action)
        {
            Name = name;
            IsOn = isOn;
            Action = action;
        }
    }
}
#endif
