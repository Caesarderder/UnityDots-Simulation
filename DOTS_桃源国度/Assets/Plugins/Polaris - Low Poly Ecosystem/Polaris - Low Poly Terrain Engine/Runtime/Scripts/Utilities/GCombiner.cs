#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GCombiner
    {
        public static GCombineInfo Combine(List<GCombineInfo> combines)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color32> colors = new List<Color32>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < combines.Count; ++i)
            {
                GCombineInfo c = combines[i];
                int offset = vertices.Count;
                for (int j = 0; j < c.Triangles.Length; ++j)
                {
                    triangles.Add(offset + c.Triangles[j]);
                }

                for (int j = 0; j < c.Vertices.Length; ++j)
                {
                    vertices.Add(c.Transform.MultiplyPoint(c.Vertices[j]));
                }

                uvs.AddRange(c.UVs);
                colors.AddRange(c.Colors);
            }

            GCombineInfo result = new GCombineInfo();
            result.Vertices = vertices.ToArray();
            result.UVs = uvs.ToArray();
            result.Colors = colors.ToArray();
            result.Triangles = triangles.ToArray();
            return result;
        }

        public static GCombineInfo Combine(GCombineInfo meshTemplate, List<Matrix4x4> transforms)
        {
            int vertexCount = meshTemplate.Vertices.Length * transforms.Count;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            //Color32[] colors = new Color32[vertexCount];

            int trisIndexCount = meshTemplate.Triangles.Length * transforms.Count;
            int[] triangles = new int[trisIndexCount];

            int currentVertIndex = 0;
            int currentTrisIndex = 0;

            for (int i = 0; i < transforms.Count; ++i)
            {
                int offset = currentVertIndex;
                for (int j = 0; j < meshTemplate.Triangles.Length; ++j)
                {
                    triangles[currentTrisIndex] = offset + meshTemplate.Triangles[j];
                    currentTrisIndex += 1;
                }

                for (int j = 0; j < meshTemplate.Vertices.Length; ++j)
                {
                    vertices[currentVertIndex] = transforms[i].MultiplyPoint(meshTemplate.Vertices[j]);
                    uvs[currentVertIndex] = meshTemplate.UVs[j];
                    //colors[currentVertIndex] = meshTemplate.Colors[j];
                    currentVertIndex += 1;
                }
            }

            GCombineInfo result = new GCombineInfo();
            result.Vertices = vertices;
            result.UVs = uvs;
            //result.Colors = colors;
            result.Triangles = triangles;
            return result;
        }
    }
}
#endif
