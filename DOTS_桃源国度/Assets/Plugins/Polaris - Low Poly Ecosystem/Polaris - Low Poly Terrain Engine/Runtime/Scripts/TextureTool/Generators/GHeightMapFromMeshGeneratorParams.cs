#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GHeightMapFromMeshGeneratorParams
    {
        private Mesh srcMesh;
        public Mesh SrcMesh
        {
            get
            {
                return srcMesh;
            }
            set
            {
                srcMesh = value;
            }
        }

        [SerializeField]
        private Vector3 offset;
        public Vector3 Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
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

        [SerializeField]
        private float projectionDepth;
        public float ProjectionDepth
        {
            get
            {
                return projectionDepth;
            }
            set
            {
                projectionDepth = Mathf.Max(0, value);
            }
        }
    }
}
#endif
