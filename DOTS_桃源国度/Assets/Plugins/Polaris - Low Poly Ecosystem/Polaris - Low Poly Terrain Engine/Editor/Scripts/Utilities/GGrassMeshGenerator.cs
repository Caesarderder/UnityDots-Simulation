#if GRIFFIN
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using Pinwheel.MeshToFile;
//using Den.Tools.Matrices;

//namespace Pinwheel.Griffin
//{
//    public class GGrassMeshGenerator
//    {
//        [MenuItem("Window/Griffin/Internal/GenerateGrassMeshes")]
//        public static void Generate()
//        {
//            Mesh quad = GenerateQuad();
//            Mesh cross = GenerateCross();
//            Mesh triCross = GenerateTriCross();
//            Mesh clump = GenerateClump(quad);
//            SaveToAsset(quad, cross, triCross, clump);
//        }

//        private static Mesh GenerateQuad()
//        {
//            Vector3[] vertices = new Vector3[]
//            {
//                new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0)
//            };
//            Vector2[] uvs = new Vector2[]
//            {
//                Vector2.zero, Vector2.up, Vector2.one, Vector2.right
//            };
//            int[] indices = new int[] { 0, 1, 2, 0, 2, 3 };
//            Mesh quad = new Mesh();
//            quad.vertices = vertices;
//            quad.uv = uvs;
//            quad.triangles = indices;
//            quad.name = "Quad";
//            quad.RecalculateBounds();
//            quad.RecalculateNormals();
//            quad.RecalculateTangents();
//            return quad;
//        }

//        private static Mesh GenerateCross()
//        {
//            Vector3[] vertices = new Vector3[]
//            {
//                new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0), new Vector3(0.5f, 0, 0),
//                new Vector3(0, 0, -0.5f), new Vector3(0, 1, -0.5f), new Vector3(0, 1, 0.5f), new Vector3(0, 0, 0.5f)
//            };
//            Vector2[] uvs = new Vector2[]
//            {
//                Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
//                Vector2.zero, Vector2.up, Vector2.one,Vector2.right
//            };

//            int[] indices = new int[]
//            {
//                0, 1, 2, 0, 2, 3,
//                4, 5, 6, 4, 6, 7
//            };
//            Mesh cross = new Mesh();
//            cross.vertices = vertices;
//            cross.uv = uvs;
//            cross.triangles = indices;
//            cross.name = "Cross";
//            cross.RecalculateBounds();
//            cross.RecalculateNormals();
//            cross.RecalculateTangents();
//            return cross;
//        }

//        private static Mesh GenerateTriCross()
//        {
//            Vector3[] quads = new Vector3[]
//            {
//                new Vector3(-0.5f, 0, 0), new Vector3(-0.5f, 1, 0), new Vector3(0.5f, 1, 0),new Vector3(0.5f, 0, 0)
//            };

//            List<Vector3> vertices = new List<Vector3>();
//            vertices.AddRange(quads);

//            Matrix4x4 rotate60 = Matrix4x4.Rotate(Quaternion.Euler(0, -60, 0));
//            for (int i = 0; i < quads.Length; ++i)
//            {
//                vertices.Add(rotate60.MultiplyPoint(quads[i]));
//            }

//            Matrix4x4 rotate120 = Matrix4x4.Rotate(Quaternion.Euler(0, -120, 0));
//            for (int i = 0; i < quads.Length; ++i)
//            {
//                vertices.Add(rotate120.MultiplyPoint(quads[i]));
//            }

//            Vector2[] uvs = new Vector2[]
//            {
//                Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
//                Vector2.zero, Vector2.up, Vector2.one, Vector2.right,
//                Vector2.zero, Vector2.up, Vector2.one, Vector2.right
//            };

//            int[] indices = new int[]
//            {
//                0, 1, 2, 0, 2, 3,
//                4, 5, 6, 4, 6, 7,
//                8, 9,10, 8,10,11
//            };
//            Mesh triCross = new Mesh();
//            triCross.vertices = vertices.ToArray();
//            triCross.uv = uvs;
//            triCross.triangles = indices;
//            triCross.name = "TriCross";
//            triCross.RecalculateBounds();
//            triCross.RecalculateNormals();
//            triCross.RecalculateTangents();
//            return triCross;
//        }

//        private static Mesh GenerateClump(Mesh baseMesh)
//        {
//            int count = 7;
//            Vector3[] verts = baseMesh.vertices;
//            Vector2[] texcoords = baseMesh.uv;
//            int[] tris = baseMesh.triangles;

//            List<Vector3> vertices = new List<Vector3>();
//            List<Vector2> uvs = new List<Vector2>();
//            List<int> triangles = new List<int>();

//            for (int i = 0; i < count; ++i)
//            {
//                Vector3 offset = new Vector3(Random.value, 0, Random.value);
//                Quaternion rotation = Quaternion.Euler(0, Random.value * 360, 0);
//                Vector3 scale = Vector3.one * Mathf.Lerp(0.7f, 1f, Random.value);
//                Matrix4x4 m = Matrix4x4.TRS(offset, rotation, scale);

//                int baseTris = vertices.Count;
//                for (int j = 0; j < verts.Length; ++j)
//                {
//                    vertices.Add(m.MultiplyPoint(verts[j]));
//                    uvs.Add(texcoords[j]);
//                }
//                for (int j = 0; j < tris.Length; ++j)
//                {
//                    triangles.Add(baseTris + tris[j]);
//                }
//            }

//            Mesh mesh = new Mesh();
//            mesh.vertices = vertices.ToArray();
//            mesh.uv = uvs.ToArray();
//            mesh.triangles = triangles.ToArray();
//            mesh.name = "Clump";
//            mesh.RecalculateBounds();
//            mesh.RecalculateNormals();
//            mesh.RecalculateTangents();
//            return mesh;
//        }

//        private static void SaveToAsset(params Mesh[] meshes)
//        {
//            MeshSaver.SaveFbxMultipleMesh(meshes, "Assets/", "GrassMeshes");
//            AssetDatabase.Refresh();
//        }
//    }
//}
#endif
