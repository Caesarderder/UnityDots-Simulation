#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Rand = System.Random;

namespace Pinwheel.Griffin.TextureTool
{
    public class GFoliageDistributionMapGenerator : IGTextureGenerator
    {
        private Material mat;
        private Material Mat
        {
            get
            {
                if (mat == null)
                {
                    mat = new Material(GRuntimeSettings.Instance.internalShaders.distributionMapGeneratorShader);
                }
                return mat;
            }
        }

        public void Generate(RenderTexture targetRt)
        {
            GFoliageDistributionMapGeneratorParams param = GTextureToolParams.Instance.TreeDistribution;
            GCommon.FillTexture(targetRt, Color.clear);

            List<Vector2> pos = new List<Vector2>();
            if (param.Terrain != null &&
                param.Terrain.TerrainData != null &&
                param.Terrain.TerrainData.Foliage.Trees != null &&
                param.Terrain.TerrainData.Foliage.Trees.Prototypes.Count != 0)
            {
                GetTreePosition(param, pos);
                Draw(targetRt, param, pos);
            }
            if (param.Terrain != null &&
                param.Terrain.TerrainData != null &&
                param.Terrain.TerrainData.Foliage.Grasses != null &&
                param.Terrain.TerrainData.Foliage.Grasses.Prototypes.Count != 0)
            {
                GGrassPatch[] patches = param.Terrain.TerrainData.Foliage.GrassPatches;
                for (int i = 0; i < patches.Length; ++i)
                {
                    GetGrassPosition(param, patches[i], pos);
                    Draw(targetRt, param, pos);
                }
            }
        }

        private void GetTreePosition(GFoliageDistributionMapGeneratorParams param, List<Vector2> pos)
        {
            pos.Clear();
            HashSet<int> indices = new HashSet<int>(param.TreePrototypeIndices);
            List<GTreeInstance> instances = param.Terrain.TerrainData.Foliage.TreeInstances;

            for (int i = 0; i < instances.Count; ++i)
            {
                if (indices.Contains(instances[i].PrototypeIndex))
                {
                    pos.Add(new Vector2(instances[i].Position.x, instances[i].Position.z));
                }
            }
        }

        private void GetGrassPosition(GFoliageDistributionMapGeneratorParams param, GGrassPatch patch, List<Vector2> pos)
        {
            pos.Clear();
            HashSet<int> indices = new HashSet<int>(param.GrassPrototypeIndices);
            List<GGrassInstance> instances = patch.Instances;

            for (int i = 0; i < instances.Count; ++i)
            {
                if (indices.Contains(instances[i].PrototypeIndex))
                {
                    pos.Add(new Vector2(instances[i].Position.x, instances[i].Position.z));
                }
            }
        }

        private void Draw(RenderTexture targetRt, GFoliageDistributionMapGeneratorParams param, List<Vector2> pos)
        {
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            Mat.SetTexture("_MainTex", param.BrushMask);
            Mat.SetFloat("_Opacity", param.Opacity);
            Mat.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);

            Rand rand = new Rand(pos.Count);
            Rect r = new Rect();
            r.size = param.Size * Vector2.one;
            for (int i = 0; i < pos.Count; ++i)
            {
                r.center = pos[i];
                Matrix4x4 matrix = Matrix4x4.TRS(
                    r.center,
                    Quaternion.Euler(0, 0, Mathf.Lerp(param.RotationMin, param.RotationMax, (float)rand.NextDouble())),
                    Vector3.one);
                Vector3 bl = matrix.MultiplyPoint(new Vector3(-r.width, -r.height, 0) * 0.5f);
                Vector3 tl = matrix.MultiplyPoint(new Vector3(-r.width, r.height, 0) * 0.5f);
                Vector3 tr = matrix.MultiplyPoint(new Vector3(r.width, r.height, 0) * 0.5f);
                Vector3 br = matrix.MultiplyPoint(new Vector3(r.width, -r.height, 0) * 0.5f);

                GL.TexCoord(new Vector3(0, 0, 0));
                GL.Vertex3(bl.x, bl.y, 0);
                GL.TexCoord(new Vector3(0, 1, 0));
                GL.Vertex3(tl.x, tl.y, 0);
                GL.TexCoord(new Vector3(1, 1, 0));
                GL.Vertex3(tr.x, tr.y, 0);
                GL.TexCoord(new Vector3(1, 0, 0));
                GL.Vertex3(br.x, br.y, 0);
            }

            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }
    }
}
#endif
