#if GRIFFIN
using UnityEditor;

namespace Pinwheel.Griffin.SplineTool
{
    public class GSplineSegmentInspectorDrawer
    {
        private GSplineSegment instance;

        public GSplineSegmentInspectorDrawer(GSplineSegment s)
        {
            instance = s;
        }

        public static GSplineSegmentInspectorDrawer Create(GSplineSegment s)
        {
            return new GSplineSegmentInspectorDrawer(s);
        }

        public void DrawGUI()
        {
            EditorGUIUtility.wideMode = true;
            instance.StartTangent = EditorGUILayout.Vector3Field("Start Tangent", instance.StartTangent);
            instance.EndTangent = EditorGUILayout.Vector3Field("End Tangent", instance.EndTangent);
            EditorGUIUtility.wideMode = false;
        }
    }
}
#endif
