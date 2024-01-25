#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GGrassUtilities
    {
        public static Vector3[] GetVertices(GGrassShape shape, Matrix4x4 matrix)
        {
            Vector3[] vertices = null;
            if (shape == GGrassShape.Quad)
            {
                vertices = new Vector3[]
                {
                    new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0),
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0)
                };
            }
            else if (shape == GGrassShape.Cross)
            {
                vertices = new Vector3[]
                {
                    new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0),
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0),
                    new Vector3(0, 0, -0.5f), new Vector3(0, 1, -0.5f), new Vector3(0, 1, 0.5f),
                    new Vector3(0, 0, -0.5f), new Vector3(0, 1, 0.5f), new Vector3(0, 0, 0.5f),
                };
            }
            else if (shape == GGrassShape.TriCross)
            {
                Vector3[] quads = new Vector3[]
                {
                    new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0),
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0)
                };

                List<Vector3> verts = new List<Vector3>();
                verts.AddRange(quads);

                Matrix4x4 rotate60 = Matrix4x4.Rotate(Quaternion.Euler(0, -60, 0));
                for (int i = 0; i < quads.Length; ++i)
                {
                    verts.Add(rotate60.MultiplyPoint(quads[i]));
                }

                Matrix4x4 rotate120 = Matrix4x4.Rotate(Quaternion.Euler(0, -120, 0));
                for (int i = 0; i < quads.Length; ++i)
                {
                    verts.Add(rotate120.MultiplyPoint(quads[i]));
                }
                vertices = verts.ToArray();
            }

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }
            return vertices;
        }

        public static Vector2[] GetUVs(GGrassShape shape)
        {
            Vector2[] uvs = null;
            if (shape == GGrassShape.Quad)
            {
                uvs = new Vector2[]
                {
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right
                };
            }
            else if (shape == GGrassShape.Cross)
            {
                uvs = new Vector2[]
                {
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right,
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right
                };
            }
            else if (shape == GGrassShape.TriCross)
            {
                uvs = new Vector2[]
                {
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right,
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right,
                    Vector2.zero, Vector2.up, Vector2.one,
                    Vector2.zero, Vector2.one, Vector2.right
                };
            }
            return uvs;
        }

        public static Color[] GetColors(GGrassShape shape)
        {
            Color[] colors = null;
            if (shape == GGrassShape.Quad)
            {
                colors = new Color[]
                {
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear
                };
            }
            else if (shape == GGrassShape.Cross)
            {
                colors = new Color[]
                {
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear,
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear
                };
            }
            else if (shape == GGrassShape.TriCross)
            {
                colors = new Color[]
                {
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear,
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear,
                    Color.clear, Color.white, Color.white,
                    Color.clear, Color.white, Color.clear
                };
            }
            return colors;
        }
    }
}
#endif
