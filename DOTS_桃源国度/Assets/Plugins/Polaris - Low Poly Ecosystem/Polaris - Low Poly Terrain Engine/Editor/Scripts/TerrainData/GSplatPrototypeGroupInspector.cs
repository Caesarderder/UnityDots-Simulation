#if GRIFFIN
using UnityEditor;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GSplatPrototypeGroup))]
    public class GSplatPrototypeGroupInspector : Editor
    {
        private GSplatPrototypeGroup instance;

        public void OnEnable()
        {
            instance = (GSplatPrototypeGroup)target;
        }

        public override void OnInspectorGUI()
        {
            GSplatPrototypeGroupInspectorDrawer.Create(instance).DrawGUI();
            EditorUtility.SetDirty(instance);
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }
    }
}
#endif
