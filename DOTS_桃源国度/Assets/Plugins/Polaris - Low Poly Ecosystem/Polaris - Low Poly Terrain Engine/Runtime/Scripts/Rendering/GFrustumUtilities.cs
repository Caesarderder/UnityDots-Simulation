#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.Rendering
{
    public static class GFrustumUtilities
    {
        private static Vector3[] corners = new Vector3[4];

        public static void Calculate(Camera cam, Plane[] planes, float zFar)
        {
            GeometryUtility.CalculateFrustumPlanes(cam, planes);
            cam.CalculateFrustumCorners(GCommon.UnitRect, zFar, Camera.MonoOrStereoscopicEye.Mono, corners);
            planes[5].Set3Points(
                cam.transform.TransformPoint(corners[0]),
                cam.transform.TransformPoint(corners[1]),
                cam.transform.TransformPoint(corners[2]));
        }
    }
}
#endif
