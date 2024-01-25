#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    public struct GCombineInfo
    {
        private Vector3[] vertices;
        public Vector3[] Vertices
        {
            get
            {
                if (vertices == null)
                    vertices = new Vector3[0];
                return vertices;
            }
            set
            {
                vertices = value;
            }
        }

        private Vector2[] uvs;
        public Vector2[] UVs
        {
            get
            {
                if (uvs == null)
                    uvs = new Vector2[0];
                return uvs;
            }
            set
            {
                uvs = value;
            }
        }

        private Color32[] colors;
        public Color32[] Colors
        {
            get
            {
                if (colors == null)
                    colors = new Color32[0];
                return colors;
            }
            set
            {
                colors = value;
            }
        }

        private int[] triangles;
        public int[] Triangles
        {
            get
            {
                if (triangles == null)
                    triangles = new int[0];
                return triangles;
            }
            set
            {
                triangles = value;
            }
        }

        private Matrix4x4 transform;
        public Matrix4x4 Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
            }
        }

        public GCombineInfo(Mesh m)
        {
            if (m != null)
            {
                vertices = m.vertices;
                uvs = m.uv;
                colors = m.colors32;
                triangles = m.triangles;
                transform = new Matrix4x4();

                if (uvs.Length != vertices.Length)
                {
                    uvs = new Vector2[vertices.Length];
                    GUtilities.Fill(uvs, Vector2.zero);
                }
                if (colors.Length != vertices.Length)
                {
                    colors = new Color32[vertices.Length];
                    GUtilities.Fill(colors, Color.clear);
                }
            }
            else
            {
                vertices = null;
                uvs = null;
                colors = null;
                triangles = null;
                transform = new Matrix4x4();
            }
        }
    }
}
#endif
