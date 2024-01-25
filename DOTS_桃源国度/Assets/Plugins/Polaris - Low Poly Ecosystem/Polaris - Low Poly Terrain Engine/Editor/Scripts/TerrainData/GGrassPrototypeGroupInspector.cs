#if GRIFFIN
using UnityEditor;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GGrassPrototypeGroup))]
    public class GGrassPrototypeGroupInspector : Editor
    {
        private GGrassPrototypeGroup instance;

        private void OnEnable()
        {
            instance = target as GGrassPrototypeGroup;
        }

        public override void OnInspectorGUI()
        {
            GGrassPrototypeGroupInspectorDrawer.Create(instance).DrawGUI();
            EditorUtility.SetDirty(instance);
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }
    }
}
#endif
