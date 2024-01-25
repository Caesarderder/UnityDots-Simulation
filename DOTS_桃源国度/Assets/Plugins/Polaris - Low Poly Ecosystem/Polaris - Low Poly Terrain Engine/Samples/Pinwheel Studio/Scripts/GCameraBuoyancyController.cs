#if POSEIDON
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Poseidon;

namespace Pinwheel.Griffin
{
    [ExecuteInEditMode]
    public class GCameraBuoyancyController : MonoBehaviour
    {
        public PWater water;
        public float speed;
        public Transform lookAt;

        private void Update()
        {
            if (water == null)
                return;
            Vector3 p = GetPosition(transform.position);
            Vector3 p0 = GetPosition(transform.position + Vector3.left);
            Vector3 p1 = GetPosition(transform.position + Vector3.forward);
            Vector3 p2 = GetPosition(transform.position + Vector3.right);
            Vector3 p3 = GetPosition(transform.position + Vector3.back);

            Vector3 center = (p + p0 + p1 + p2 + p3) / 5f;
            Vector3 n0 = Vector3.Cross(p0 - p, p1 - p);
            Vector3 n1 = Vector3.Cross(p1 - p, p2 - p);
            Vector3 n2 = Vector3.Cross(p2 - p, p3 - p);
            Vector3 n3 = Vector3.Cross(p3 - p, p0 - p);
            Vector3 up = (n0 + n1 + n2 + n3) / 4f;

            transform.position = center;
            transform.up = Vector3.Lerp(transform.up, up, 0.01667f * speed);

            if (lookAt != null)
            {
                transform.LookAt(lookAt.position, transform.up);
            }
        }

        private Vector3 GetPosition(Vector3 worldPoint)
        {
            Vector3 localPoint = water.transform.InverseTransformPoint(worldPoint);
            localPoint.y = 0;
            localPoint = PWaterExtension.GetLocalVertexPosition(water, localPoint, true);
            worldPoint = water.transform.TransformPoint(localPoint);
            return worldPoint;
        }
    }
}
#endif
