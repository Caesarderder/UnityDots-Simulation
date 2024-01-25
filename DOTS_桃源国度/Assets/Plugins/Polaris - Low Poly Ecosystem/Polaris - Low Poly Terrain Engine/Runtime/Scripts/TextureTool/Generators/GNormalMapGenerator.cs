#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public class GNormalMapGenerator : IGTextureGenerator
    {
        public void Generate(RenderTexture targetRt)
        {
            GNormalMapGeneratorParams param = GTextureToolParams.Instance.NormalMap;
            Color defaultColor = param.Space == GNormalMapSpace.Local ? new Color(0.5f, 1, 0.5f, 1) : new Color(0.5f, 0.5f, 1, 1);
            if (param.Terrain == null || param.Terrain.TerrainData == null)
            {
                GCommon.FillTexture(targetRt, defaultColor);
            }
            else
            {
                if (param.Mode == GNormalMapMode.Sharp)
                {
                    RenderSharpNormalMap(param, targetRt);
                }
                else if (param.Mode == GNormalMapMode.Interpolated)
                {
                    RenderInterpolatedNormalMap(param, targetRt);
                }
                else if (param.Mode == GNormalMapMode.PerPixel)
                {
                    RenderPerPixelNormalMap(param, targetRt);
                }
                else
                {
                    GCommon.FillTexture(targetRt, defaultColor);
                }
            }
        }

        public void RenderSharpNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            Material mat = GInternalMaterials.TerrainNormalMapRendererMaterial;
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);
            mat.SetPass(0);
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);

            Vector2 terrainSize = param.Terrain.Rect.size;
            GTerrainChunk[] chunks = param.Terrain.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Vector3 chunkLocalPosition = chunks[i].transform.localPosition;
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;

                Vector3[] vertices = m.vertices;
                Vector3[] normals = m.normals;

                for (int j = 0; j < vertices.Length; ++j)
                {
                    //vx = uvx*sizex-posx;
                    //vx+posx = uvx*sizex
                    // uvx = (vx+posx)/sizex;
                    GL.TexCoord(normals[j]);
                    GL.Vertex3(
                        (vertices[j].x + chunkLocalPosition.x) / terrainSize.x,
                        (vertices[j].z + chunkLocalPosition.z) / terrainSize.y,
                        0);
                }
            }
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public void RenderInterpolatedNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            List<Vector2> uvs = new List<Vector2>();
            Dictionary<Vector2, Vector3> normals = new Dictionary<Vector2, Vector3>();
            Dictionary<Vector2, int> normalsCount = new Dictionary<Vector2, int>();

            Vector3 terrainSize = param.Terrain.Rect.size;
            GTerrainChunk[] chunks = param.Terrain.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Vector3 chunkLocalPosition = chunks[i].transform.localPosition;
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;

                //Vector2[] meshUv = m.uv;
                Vector3[] meshVertices = m.vertices;
                Vector3[] meshNormal = m.normals;

                for (int j = 0; j < meshVertices.Length; ++j)
                {
                    //vx = uvx*sizex-posx;
                    //vx+posx = uvx*sizex
                    // uvx = (vx+posx)/sizex;
                    Vector2 meshUv = new Vector2(
                        (meshVertices[j].x + chunkLocalPosition.x) / terrainSize.x,
                        (meshVertices[j].z + chunkLocalPosition.z) / terrainSize.y);
                    uvs.Add(meshUv);
                    if (normals.ContainsKey(meshUv))
                        normals[meshUv] += meshNormal[j];
                    else
                        normals[meshUv] = meshNormal[j];

                    if (normalsCount.ContainsKey(meshUv))
                        normalsCount[meshUv] += 1;
                    else
                        normalsCount[meshUv] = 1;
                }
            }

            List<Vector3> smoothNormals = new List<Vector3>();
            for (int i = 0; i < uvs.Count; ++i)
            {
                smoothNormals.Add(normals[uvs[i]] / normalsCount[uvs[i]]);
            }

            Material mat = GInternalMaterials.TerrainNormalMapRendererMaterial;
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);
            mat.SetPass(0);
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);

            for (int i = 0; i < uvs.Count; ++i)
            {
                GL.TexCoord(smoothNormals[i]);
                GL.Vertex3(uvs[i].x, uvs[i].y, 0);
            }

            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public void RenderPerPixelNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            Material mat = GInternalMaterials.TerrainPerPixelNormalMapRendererMaterial;
            mat.SetTexture("_HeightMap", param.Terrain.TerrainData.Geometry.HeightMap);
            mat.SetFloat("_Width", param.Terrain.TerrainData.Geometry.Width);
            mat.SetFloat("_Height", param.Terrain.TerrainData.Geometry.Height);
            mat.SetFloat("_Length", param.Terrain.TerrainData.Geometry.Length);
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);

            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, mat, 0);
        }
    }
}
#endif
