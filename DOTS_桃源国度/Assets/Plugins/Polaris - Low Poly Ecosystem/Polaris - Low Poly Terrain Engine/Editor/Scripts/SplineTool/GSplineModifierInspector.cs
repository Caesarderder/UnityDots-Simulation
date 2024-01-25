#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    [CustomEditor(typeof(GSplineModifier), true)]
    public class GSplineModifierInspector : Editor
    {
        private GSplineModifier instance;

        public void OnEnable()
        {
            instance = (GSplineModifier)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (instance.SplineCreator == null)
                return;
            EditorGUILayout.Space();
            if (GUILayout.Button("Apply"))
            {
                instance.Apply();
            }
        }
    }
}
#endif
