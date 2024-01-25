using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using System;

namespace ProjectDawn.Navigation
{
    [InitializeOnLoad]
    public static class GizmosField
    {
        static Field s_Instance;

        static GizmosField()
        {
            s_Instance = new();
            AssemblyReloadEvents.beforeAssemblyReload -= Release;
            AssemblyReloadEvents.beforeAssemblyReload += Release;
        }

        static void Release()
        {
            s_Instance.Dispose();
        }

        public static void DrawDepth(ReadOnlySpan<float> heightField, ReadOnlySpan<int> obstacleField, int width, int height, float4x4 transform)
        {
            s_Instance.DrawDepth(heightField.ToArray(), obstacleField.ToArray(), width, height, transform);
        }

        public static void DrawSolid(ReadOnlySpan<float> heightField, ReadOnlySpan<int> obstacleField, int width, int height, float4x4 transform, Color color)
        {
            s_Instance.DrawSolid(heightField.ToArray(), obstacleField.ToArray(), width, height, transform, color);
        }

        public static void DrawSolid(ReadOnlySpan<float> heightField, ReadOnlySpan<int> obstacleField, ReadOnlySpan<Color> colorField, int width, int height, float4x4 transform)
        {
            s_Instance.DrawSolid(heightField.ToArray(), obstacleField.ToArray(), colorField.ToArray(), width, height, transform);
        }

        public static void DrawWire(ReadOnlySpan<float> heightField, ReadOnlySpan<int> obstacleField, int width, int height, float4x4 transform, Color color)
        {
            s_Instance.DrawWire(heightField.ToArray(), obstacleField.ToArray(), width, height, transform, color);
        }

        unsafe class Field : IDisposable
        {
            const int Capacity = 2 << 14;

            GraphicsBuffer m_HeightField;
            GraphicsBuffer m_ObstacleField;
            GraphicsBuffer m_ColorField;
            Material m_Material;

            Material GetOrCreateMaterial()
            {
                if (m_Material != null)
                    return m_Material;
                m_Material = new Material(Shader.Find("Hidden/GizmosField"));
                return m_Material;
            }

            GraphicsBuffer GetOrCrateHeightField()
            {
                if (m_HeightField != null)
                    return m_HeightField;

                m_HeightField = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Capacity, sizeof(float));

                return m_HeightField;
            }

            GraphicsBuffer GetOrCrateObstacleField()
            {
                if (m_ObstacleField != null)
                    return m_ObstacleField;

                m_ObstacleField = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Capacity, sizeof(int));

                return m_ObstacleField;
            }

            GraphicsBuffer GetOrCrateColorField()
            {
                if (m_ColorField != null)
                    return m_ColorField;

                m_ColorField = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Capacity, sizeof(Color));

                return m_ColorField;
            }

            public void DrawDepth(float[] heightField, int[] obstacleField, int width, int height, float4x4 transform)
            {
                int count = width * height;

                if (count > Capacity)
                    throw new InvalidOperationException($"GizmosField can no be drawn with {count} as capacity is {Capacity}.");

                var heightFieldBuffer = GetOrCrateHeightField();
                heightFieldBuffer.SetData(heightField, 0, 0, count);

                var obstacleFieldBuffer = GetOrCrateObstacleField();
                obstacleFieldBuffer.SetData(obstacleField, 0, 0, count);

                var material = GetOrCreateMaterial();
                material.SetBuffer("_HeightField", GetOrCrateHeightField());
                material.SetBuffer("_ObstacleField", GetOrCrateObstacleField());
                material.SetInteger("_Width", width);
                material.SetInteger("_Height", height);
                material.SetMatrix("_Transform", transform);
                material.SetPass(0);

                Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, count);
            }

            public void DrawSolid(float[] heightField, int[] obstacleField, int width, int height, float4x4 transform, Color color)
            {
                int count = width * height;

                if (count > Capacity)
                    throw new InvalidOperationException($"GizmosField can no be drawn with {count} as capacity is {Capacity}.");

                var heightFieldBuffer = GetOrCrateHeightField();
                heightFieldBuffer.SetData(heightField, 0, 0, count);

                var obstacleFieldBuffer = GetOrCrateObstacleField();
                obstacleFieldBuffer.SetData(obstacleField, 0, 0, count);

                var material = GetOrCreateMaterial();
                material.SetBuffer("_HeightField", GetOrCrateHeightField());
                material.SetBuffer("_ObstacleField", GetOrCrateObstacleField());
                material.SetInteger("_Width", width);
                material.SetInteger("_Height", height);
                material.SetMatrix("_Transform", transform);
                material.SetColor("_Color", color);
                material.SetPass(1);

                Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, count);
            }

            public void DrawSolid(float[] heightField, int[] obstacleField, Color[] colorField, int width, int height, float4x4 transform)
            {
                int count = width * height;

                if (count > Capacity)
                    throw new InvalidOperationException($"GizmosField can no be drawn with {count} as capacity is {Capacity}.");

                var heightFieldBuffer = GetOrCrateHeightField();
                heightFieldBuffer.SetData(heightField, 0, 0, count);

                var obstacleFieldBuffer = GetOrCrateObstacleField();
                obstacleFieldBuffer.SetData(obstacleField, 0, 0, count);

                var colorFieldBuffer = GetOrCrateColorField();
                colorFieldBuffer.SetData(colorField, 0, 0, count);

                var material = GetOrCreateMaterial();
                material.SetBuffer("_HeightField", GetOrCrateHeightField());
                material.SetBuffer("_ObstacleField", GetOrCrateObstacleField());
                material.SetBuffer("_ColorField", GetOrCrateColorField());
                material.SetInteger("_Width", width);
                material.SetInteger("_Height", height);
                material.SetMatrix("_Transform", transform);
                material.SetPass(2);

                Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, count);
            }

            public void DrawWire(float[] heightField, int[] obstacleField, int width, int height, float4x4 transform, Color color)
            {
                int count = width * height;

                if (count > Capacity)
                    throw new InvalidOperationException($"GizmosField can no be drawn with {count} as capacity is {Capacity}.");

                var heightFieldBuffer = GetOrCrateHeightField();
                heightFieldBuffer.SetData(heightField, 0, 0, count);

                var obstacleFieldBuffer = GetOrCrateObstacleField();
                obstacleFieldBuffer.SetData(obstacleField, 0, 0, count);

                var material = GetOrCreateMaterial();
                material.SetBuffer("_HeightField", GetOrCrateHeightField());
                material.SetBuffer("_ObstacleField", GetOrCrateObstacleField());
                material.SetInteger("_Width", width);
                material.SetInteger("_Height", height);
                material.SetMatrix("_Transform", transform);
                material.SetColor("_Color", color);
                material.SetPass(3);

                Graphics.DrawProceduralNow(MeshTopology.Lines, 8, count);
            }

            public void Dispose()
            {
                if (m_Material)
                    GameObject.DestroyImmediate(m_Material);
                m_HeightField?.Release();
                m_ObstacleField?.Release();
                m_ColorField?.Release();
            }
        }
    }
}
