#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [ExecuteInEditMode]
    public class GWindZone : MonoBehaviour
    {
        private static HashSet<GWindZone> activeWindZoneSet;
        private static HashSet<GWindZone> ActiveWindZoneSet
        {
            get
            {
                if (activeWindZoneSet == null)
                    activeWindZoneSet = new HashSet<GWindZone>();
                return activeWindZoneSet;
            }
        }

        public static IEnumerable<GWindZone> ActiveWindZones
        {
            get
            {
                return ActiveWindZoneSet;
            }
        }

        [SerializeField]
        private float directionX;
        public float DirectionX
        {
            get
            {
                return directionX;
            }
            set
            {
                directionX = value;
            }
        }

        [SerializeField]
        private float directionZ;
        public float DirectionZ
        {
            get
            {
                return directionZ;
            }
            set
            {
                directionZ = value;
            }
        }

        [SerializeField]
        private float speed;
        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        [SerializeField]
        private float spread;
        public float Spread
        {
            get
            {
                return spread;
            }
            set
            {
                spread = value;
            }
        }

        private void Reset()
        {
            directionX = 1;
            directionZ = 1;
            speed = 4;
            spread = 8;
        }

        private void OnEnable()
        {
            ActiveWindZoneSet.Add(this);
        }

        private void OnDisable()
        {
            ActiveWindZoneSet.Remove(this);
        }

        public Vector4 GetWindParams()
        {
            Vector2 dir = new Vector2(DirectionX, DirectionZ).normalized;
            Vector4 param = new Vector4(dir.x, dir.y, speed, spread);
            return param;
        }

        public void SyncTransform()
        {
            transform.rotation = Quaternion.identity;
            transform.forward = new Vector3(DirectionX, 0, DirectionZ).normalized;
        }

        public void SyncDirection()
        {
            Matrix4x4 matrix = Matrix4x4.Rotate(transform.rotation);
            Vector3 dir = matrix.MultiplyVector(Vector3.forward);
            DirectionX = dir.x;
            DirectionZ = dir.z;
        }
    }
}
#endif
