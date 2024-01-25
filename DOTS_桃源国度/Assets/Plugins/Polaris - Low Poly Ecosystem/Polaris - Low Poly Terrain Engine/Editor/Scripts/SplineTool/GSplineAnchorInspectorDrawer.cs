#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    public class GSplineAnchorInspectorDrawer
    {
        private GSplineAnchor instance;

        public GSplineAnchorInspectorDrawer(GSplineAnchor anchor)
        {
            instance = anchor;
        }

        public static GSplineAnchorInspectorDrawer Create(GSplineAnchor anchor)
        {
            return new GSplineAnchorInspectorDrawer(anchor);
        }

        public void DrawGUI()
        {
            EditorGUIUtility.wideMode = true;
            instance.Position = EditorGUILayout.Vector3Field("Position", instance.Position);
            instance.Rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", instance.Rotation.eulerAngles));
            instance.Scale = EditorGUILayout.Vector3Field("Scale", instance.Scale);
            EditorGUIUtility.wideMode = false;
        }
    }
}
#endif
