#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Pinwheel.Griffin
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = false)]
#endif
    public struct GCreateVertexJob : IJob
    {
        [ReadOnly]
        public NativeArray<GSubdivNode> nodes;
        [ReadOnly]
        public NativeArray<byte> creationState;

        public GTextureNativeDataDescriptor<Color32> hmC;
        public GTextureNativeDataDescriptor<Color32> hmL;
        public GTextureNativeDataDescriptor<Color32> hmT;
        public GTextureNativeDataDescriptor<Color32> hmR;
        public GTextureNativeDataDescriptor<Color32> hmB;
        public GTextureNativeDataDescriptor<Color32> hmBL;
        public GTextureNativeDataDescriptor<Color32> hmTL;
        public GTextureNativeDataDescriptor<Color32> hmTR;
        public GTextureNativeDataDescriptor<Color32> hmBR;

        public GTextureNativeDataDescriptor<Color32> maskMap;
        public GTextureNativeDataDescriptor<Color32> albedoMap;
        public GAlbedoToVertexColorMode albedoToVertexColorMode;

        [WriteOnly]
        public NativeArray<Vector3> vertices;
        [WriteOnly]
        public NativeArray<Vector2> uvs;
        [WriteOnly]
        public NativeArray<int> triangles;
        [WriteOnly]
        public NativeArray<Vector3> normals;
        [WriteOnly]
        public NativeArray<Color32> colors;
        [WriteOnly]
        public NativeArray<int> metadata;

        public int meshBaseResolution;
        public int meshResolution;
        public int lod;
        public int displacementSeed;
        public float displacementStrength;
        public bool smoothNormal;
        public bool useSmoothNormalMask;
        public bool mergeUV;

        public Vector3 terrainSize;
        public Rect chunkUvRect;
        public Vector3 chunkLocalPosition;
        public float texelSize;

        public void Execute()
        {
            GSubdivNode n;
            Vector3 v0 = Vector3.zero;
            Vector3 v1 = Vector3.zero;
            Vector3 v2 = Vector3.zero;

            Vector2 uv0 = Vector2.zero;
            Vector2 uv1 = Vector2.zero;
            Vector2 uv2 = Vector2.zero;
            Vector2 uvc = Vector2.zero;

            Vector3 normal = Vector3.zero;
            Color32 color = new Color32();

            int i0 = 0;
            int i1 = 0;
            int i2 = 0;

            Color hmData0 = Color.black;
            Color hmData1 = Color.black;
            Color hmData2 = Color.black;
            float heightSample = 0;

            meshBaseResolution = Mathf.Max(0, meshBaseResolution - lod);

            int length = nodes.Length;
            int leafIndex = 0;
            int startIndex = GGeometryJobUtilities.GetStartIndex(ref meshBaseResolution);
            int removedLeafCount = 0;

            for (int i = startIndex; i < length; ++i)
            {
                if (creationState[i] != GGeometryJobUtilities.STATE_LEAF)
                    continue;
                n = nodes[i];
                ProcessTriangle(
                    ref n, ref leafIndex,
                    ref uv0, ref uv1, ref uv2, ref uvc,
                    ref v0, ref v1, ref v2,
                    ref i0, ref i1, ref i2,
                    ref normal, ref color,
                    ref hmData0, ref hmData1, ref hmData2,
                    ref heightSample, ref removedLeafCount);
                leafIndex += 1;
            }

            metadata[GGeometryJobUtilities.METADATA_LEAF_REMOVED] = removedLeafCount;
        }

        private void ProcessTriangle(
            ref GSubdivNode n, ref int leafIndex,
            ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uvc,
            ref Vector3 v0, ref Vector3 v1, ref Vector3 v2,
            ref int i0, ref int i1, ref int i2,
            ref Vector3 normal, ref Color32 color,
            ref Color hmData0, ref Color hmData1, ref Color hmData2,
            ref float heightSample, ref int removedLeafCount)
        {
            GGeometryJobUtilities.NormalizeToPoint(ref uv0, ref chunkUvRect, ref n.v0);
            GGeometryJobUtilities.NormalizeToPoint(ref uv1, ref chunkUvRect, ref n.v1);
            GGeometryJobUtilities.NormalizeToPoint(ref uv2, ref chunkUvRect, ref n.v2);

            if (displacementStrength > 0)
            {
                DisplaceUV(ref uv0);
                DisplaceUV(ref uv1);
                DisplaceUV(ref uv2);
            }

            GetHeightMapData(ref hmData0, ref uv0);
            GetHeightMapData(ref hmData1, ref uv1);
            GetHeightMapData(ref hmData2, ref uv2);

            GetHeightSample(ref heightSample, ref hmData0);
            v0.Set(
                uv0.x * terrainSize.x - chunkLocalPosition.x,
                heightSample * terrainSize.y,
                uv0.y * terrainSize.z - chunkLocalPosition.z);

            GetHeightSample(ref heightSample, ref hmData1);
            v1.Set(
                uv1.x * terrainSize.x - chunkLocalPosition.x,
                heightSample * terrainSize.y,
                uv1.y * terrainSize.z - chunkLocalPosition.z);

            GetHeightSample(ref heightSample, ref hmData2);
            v2.Set(
                uv2.x * terrainSize.x - chunkLocalPosition.x,
                heightSample * terrainSize.y,
                uv2.y * terrainSize.z - chunkLocalPosition.z);

            i0 = leafIndex * 3 + 0;
            i1 = leafIndex * 3 + 1;
            i2 = leafIndex * 3 + 2;

            vertices[i0] = v0;
            vertices[i1] = v1;
            vertices[i2] = v2;

            if (mergeUV)
            {
                Vector2 mergedUV = (uv0 + uv1 + uv2) / 3f;
                uvs[i0] = mergedUV;
                uvs[i1] = mergedUV;
                uvs[i2] = mergedUV;
            }
            else
            {
                uvs[i0] = uv0;
                uvs[i1] = uv1;
                uvs[i2] = uv2;
            }

            if (!smoothNormal)
            {
                normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                normals[i0] = normal;
                normals[i1] = normal;
                normals[i2] = normal;
            }
            else if (smoothNormal && !useSmoothNormalMask)
            {
                normals[i0] = GetSmoothNormal(ref uv0, ref v0).normalized;
                normals[i1] = GetSmoothNormal(ref uv1, ref v1).normalized;
                normals[i2] = GetSmoothNormal(ref uv2, ref v2).normalized;
            }
            else if (smoothNormal && useSmoothNormalMask)
            {
                normal = Vector3.Cross(v1 - v0, v2 - v0);
                Vector3 smoothNormal;
                float mask;

                smoothNormal = GetSmoothNormal(ref uv0, ref v0);
                mask = GetSmoothNormalMask(ref uv0);
                normals[i0] = Vector3.Lerp(normal, smoothNormal, mask).normalized;

                smoothNormal = GetSmoothNormal(ref uv1, ref v1);
                mask = GetSmoothNormalMask(ref uv1);
                normals[i1] = Vector3.Lerp(normal, smoothNormal, mask).normalized;

                smoothNormal = GetSmoothNormal(ref uv2, ref v2);
                mask = GetSmoothNormalMask(ref uv2);
                normals[i2] = Vector3.Lerp(normal, smoothNormal, mask).normalized;
            }

            if (hmData0.a >= 0.5 || hmData1.a >= 0.5 || hmData2.a >= 0.5)
            {
                triangles[i0] = i0;
                triangles[i1] = i0;
                triangles[i2] = i0;
                removedLeafCount += 1;
            }
            else
            {
                triangles[i0] = i0;
                triangles[i1] = i1;
                triangles[i2] = i2;
            }

            if (albedoToVertexColorMode == GAlbedoToVertexColorMode.Sharp)
            {
                uvc = (uv0 + uv1 + uv2) / 3f;
                color = GGeometryJobUtilities.GetColorBilinear(albedoMap, ref uvc);
                colors[i0] = color;
                colors[i1] = color;
                colors[i2] = color;
            }
            else if (albedoToVertexColorMode == GAlbedoToVertexColorMode.Smooth)
            {
                colors[i0] = GGeometryJobUtilities.GetColorBilinear(albedoMap, ref uv0);
                colors[i1] = GGeometryJobUtilities.GetColorBilinear(albedoMap, ref uv1);
                colors[i2] = GGeometryJobUtilities.GetColorBilinear(albedoMap, ref uv2);
            }
        }

        private void DisplaceUV(ref Vector2 uv)
        {
            if (uv.x == 0 || uv.y == 0 || uv.x == 1 || uv.y == 1)
                return;

            Random rnd = Random.CreateFromIndex((uint)(displacementSeed ^ (uint)(uv.x * 1000) ^ (uint)(uv.y * 1000)));            
            float noise0 = rnd.NextFloat() - 0.5f;
            float noise1 = rnd.NextFloat() - 0.5f;

            Vector2 v = new Vector2(noise0 * displacementStrength / terrainSize.x, noise1 * displacementStrength / terrainSize.z);
            uv.Set(
                Mathf.Clamp01(uv.x + v.x),
                Mathf.Clamp01(uv.y + v.y));
        }

        private void GetHeightMapData(ref Color data, ref Vector2 uv)
        {
            Color sample = Vector4.zero;
            float sampleCount = 0f;

            sample += GGeometryJobUtilities.GetColorBilinear(hmC, ref uv);
            sampleCount += 1;

            if (uv.x == 0 && uv.y == 0) //bottom left corner
            {
                if (hmB.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmB, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
                if (hmL.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmL, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
                if (hmBL.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmBL, Flip(ref uv, true, true));
                    sampleCount += 1;
                }
            }
            else if (uv.x == 0 && uv.y == 1) //top left corner
            {
                if (hmT.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmT, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
                if (hmL.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmL, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
                if (hmTL.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmTL, Flip(ref uv, true, true));
                    sampleCount += 1;
                }
            }
            else if (uv.x == 1 && uv.y == 1) //top right corner
            {
                if (hmT.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmT, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
                if (hmR.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmR, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
                if (hmTR.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmTR, Flip(ref uv, true, true));
                    sampleCount += 1;
                }
            }
            else if (uv.x == 1 && uv.y == 0) //bottom right corner
            {
                if (hmB.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmB, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
                if (hmR.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmR, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
                if (hmBR.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmBR, Flip(ref uv, true, true));
                    sampleCount += 1;
                }
            }
            else if (uv.x == 0) //left edge
            {
                if (hmL.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmL, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
            }
            else if (uv.y == 1) //top edge
            {
                if (hmT.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmT, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
            }
            else if (uv.x == 1) //right edge
            {
                if (hmR.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmR, Flip(ref uv, true, false));
                    sampleCount += 1;
                }
            }
            else if (uv.y == 0) //bottom edge
            {
                if (hmB.IsValid)
                {
                    sample += GGeometryJobUtilities.GetColorBilinear(hmB, Flip(ref uv, false, true));
                    sampleCount += 1;
                }
            }

            data = sample / sampleCount;
        }

        private float DecodeFloatRG(ref Vector2 enc)
        {
            Vector2 kDecodeDot = new Vector2(1.0f, 1f / 255.0f);
            return Vector2.Dot(enc, kDecodeDot);
        }

        private void GetHeightSample(ref float sample, ref Color data)
        {
            Vector2 enc = new Vector2(data.r, data.g);
            sample = DecodeFloatRG(ref enc);
        }

        private Vector2 Flip(ref Vector2 uv, bool flipX, bool flipY)
        {
            Vector2 v = new Vector2(
                flipX ? 1 - uv.x : uv.x,
                flipY ? 1 - uv.y : uv.y);
            return v;
        }

        private Vector3 GetSmoothNormal(ref Vector2 uv, ref Vector3 v)
        {
            Color hmData = Color.black;
            Vector2 sampleUV = Vector2.zero;

            //bl
            sampleUV.Set(uv.x - texelSize, uv.y - texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h0 = 0;
            GetHeightSample(ref h0, ref hmData);
            Vector3 v0 = new Vector3(sampleUV.x * terrainSize.x, h0 * terrainSize.y, sampleUV.y * terrainSize.z);

            //l
            sampleUV.Set(uv.x - texelSize, uv.y);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h1 = 0;
            GetHeightSample(ref h1, ref hmData);
            Vector3 v1 = new Vector3(sampleUV.x * terrainSize.x, h1 * terrainSize.y, sampleUV.y * terrainSize.z);

            //tl
            sampleUV.Set(uv.x - texelSize, uv.y + texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h2 = 0;
            GetHeightSample(ref h2, ref hmData);
            Vector3 v2 = new Vector3(sampleUV.x * terrainSize.x, h2 * terrainSize.y, sampleUV.y * terrainSize.z);

            //t
            sampleUV.Set(uv.x, uv.y + texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h3 = 0;
            GetHeightSample(ref h3, ref hmData);
            Vector3 v3 = new Vector3(sampleUV.x * terrainSize.x, h3 * terrainSize.y, sampleUV.y * terrainSize.z);

            //tr
            sampleUV.Set(uv.x + texelSize, uv.y + texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h4 = 0;
            GetHeightSample(ref h4, ref hmData);
            Vector3 v4 = new Vector3(sampleUV.x * terrainSize.x, h4 * terrainSize.y, sampleUV.y * terrainSize.z);

            //r
            sampleUV.Set(uv.x + texelSize, uv.y);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h5 = 0;
            GetHeightSample(ref h5, ref hmData);
            Vector3 v5 = new Vector3(sampleUV.x * terrainSize.x, h5 * terrainSize.y, sampleUV.y * terrainSize.z);

            //br
            sampleUV.Set(uv.x + texelSize, uv.y - texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h6 = 0;
            GetHeightSample(ref h6, ref hmData);
            Vector3 v6 = new Vector3(sampleUV.x * terrainSize.x, h6 * terrainSize.y, sampleUV.y * terrainSize.z);

            //b
            sampleUV.Set(uv.x, uv.y - texelSize);
            GetHeightMapData(ref hmData, ref sampleUV);
            float h7 = 0;
            GetHeightSample(ref h7, ref hmData);
            Vector3 v7 = new Vector3(sampleUV.x * terrainSize.x, h7 * terrainSize.y, sampleUV.y * terrainSize.z);

            Vector3 n0 = Vector3.Cross(v0 - v, v1 - v);
            Vector3 n1 = Vector3.Cross(v1 - v, v2 - v);
            Vector3 n2 = Vector3.Cross(v2 - v, v3 - v);
            Vector3 n3 = Vector3.Cross(v3 - v, v4 - v);
            Vector3 n4 = Vector3.Cross(v4 - v, v5 - v);
            Vector3 n5 = Vector3.Cross(v5 - v, v6 - v);
            Vector3 n6 = Vector3.Cross(v6 - v, v7 - v);
            Vector3 n7 = Vector3.Cross(v7 - v, v0 - v);

            Vector3 n = (n0 + n1 + n2 + n3 + n4 + n5 + n6 + n7) / 8.0f;

            return n;
        }

        private float GetSmoothNormalMask(ref Vector2 uv)
        {
            return GGeometryJobUtilities.GetColorPoint(maskMap, uv).g;
        }
    }
}
#endif
