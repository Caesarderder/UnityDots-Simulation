#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    [System.Serializable]
    public class GSplineAnchor
    {
        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        [SerializeField]
        private Quaternion rotation;
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        [SerializeField]
        private Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public GSweepTestData SweepTestData
        {
            get
            {
                return new GSweepTestData()
                {
                    position = this.position,
                    scale = this.scale
                };
            }
        }

        public GSplineAnchor(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }

        public struct GSweepTestData
        {
            public Vector3 position;
            public Vector3 scale;
        }
    }
}
#endif
