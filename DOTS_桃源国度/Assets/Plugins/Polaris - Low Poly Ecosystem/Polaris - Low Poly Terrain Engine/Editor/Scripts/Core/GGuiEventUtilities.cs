#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    /// <summary>
    /// Utility class to handle GUI event
    /// </summary>
    public static class GGuiEventUtilities
    {
        public static bool IsShift
        {
            get
            {
                return Event.current != null && Event.current.shift;
            }
        }

        public static bool IsCtrl
        {
            get
            {
                return Event.current != null && Event.current.control;
            }
        }

        public static bool IsLeftMouse
        {
            get
            {
                return Event.current != null && Event.current.isMouse && Event.current.button == 0;
            }
        }

        public static bool IsLeftMouseDown
        {
            get
            {
                return Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            }
        }

        public static bool IsLeftMouseUp
        {
            get
            {
                return Event.current != null && Event.current.type == EventType.MouseUp && Event.current.button == 0;
            }
        }

        public static bool IsPlus
        {
            get
            {
                return IsButtonPressed(KeyCode.KeypadPlus) ||
                IsButtonPressed(KeyCode.Plus) ||
                IsButtonPressed(KeyCode.Equals);
            }
        }

        public static bool IsMinus
        {
            get
            {
                return IsButtonPressed(KeyCode.KeypadMinus) ||
                IsButtonPressed(KeyCode.Minus);
            }
        }

        public static bool IsButtonPressed(KeyCode key)
        {
            return Event.current != null && Event.current.keyCode == key;
        }
    }
}
#endif
