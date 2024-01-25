#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GHandleUtility
    {
        public static float DistanceMouseToSpline(Vector2 mousePosition, params Vector3[] splinePoint)
        {
            float d = float.MaxValue;
            for (int i = 0; i < splinePoint.Length - 1; ++i)
            {
                float d1 = HandleUtility.DistancePointToLineSegment(
                    mousePosition,
                    HandleUtility.WorldToGUIPoint(splinePoint[i]),
                    HandleUtility.WorldToGUIPoint(splinePoint[i + 1]));
                if (d1 < d)
                    d = d1;
            }
            return d;
        }

        public static float DistanceMouseToPoint(Vector2 mousePosition, Vector3 worldPoint)
        {
            float d = Vector2.Distance(
                mousePosition,
                HandleUtility.WorldToGUIPoint(worldPoint));
            return d;
        }

        public static float DistanceMouseToLine(Vector2 mousePosition, Vector3 lineStart, Vector3 lineEnd)
        {
            return HandleUtility.DistancePointToLineSegment(
                mousePosition,
                HandleUtility.WorldToGUIPoint(lineStart),
                HandleUtility.WorldToGUIPoint(lineEnd));
        }
    }
}
#endif
