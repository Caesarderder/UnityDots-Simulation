#if GRIFFIN
using UnityEngine;
using System;

namespace Pinwheel.Griffin.SplineTool
{
    [System.Serializable]
    public class GSplineSegment : IDisposable
    {
        [SerializeField]
        private int startIndex;
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                startIndex = value;
            }
        }

        [SerializeField]
        private int endIndex;
        public int EndIndex
        {
            get
            {
                return endIndex;
            }
            set
            {
                endIndex = value;
            }
        }

        [SerializeField]
        private Vector3 startTangent;
        public Vector3 StartTangent
        {
            get
            {
                return startTangent;
            }
            set
            {
                startTangent = value;
            }
        }

        [SerializeField]
        private Vector3 endTangent;
        public Vector3 EndTangent
        {
            get
            {
                return endTangent;
            }
            set
            {
                endTangent = value;
            }
        }

        [SerializeField]
        private Mesh mesh;
        public Mesh Mesh
        {
            get
            {
                if (mesh == null)
                {
                    mesh = new Mesh();
                    mesh.name = "Spline Segment";
                    mesh.MarkDynamic();
                }
                return mesh;
            }
        }

        public GSweepTestData SweepTestData
        {
            get
            {
                return new GSweepTestData()
                {
                    startIndex = this.startIndex,
                    endIndex = this.endIndex,
                    startTangent = this.startTangent,
                    endTangent = this.endTangent
                };
            }
        }

        public void FlipDirection()
        {
            int tmp = startIndex;
            startIndex = endIndex;
            endIndex = tmp;
        }

        public void Dispose()
        {
            if (mesh != null)
            {
                GUtilities.DestroyObject(mesh);
            }
        }

        public struct GSweepTestData
        {
            public int startIndex;
            public int endIndex;
            public Vector3 startTangent;
            public Vector3 endTangent;
        }
    }
}
#endif
