#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.Griffin
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = false)]
#endif
    public struct GSplitBaseTreeForDynamicPolygonJob : IJob
    {
        [ReadOnly]
        public GTextureNativeDataDescriptor<Color32> subdivMap;

        public NativeArray<GSubdivNode> baseTree;
        [WriteOnly]
        public NativeArray<bool> vertexMarker;
        public NativeArray<byte> creationState;

        public int baseResolution;
        public int resolution;
        public int lod;
        public Rect uvRect;

        public void Execute()
        {
            int startIndex = 0;
            int endIndex = 0;
            int leftNodeIndex = 0;
            int rightNodeIndex = 0;

            GSubdivNode currentNode;
            GSubdivNode leftNode = new GSubdivNode();
            GSubdivNode rightNode = new GSubdivNode();

            Vector2 uv0 = Vector2.zero;
            Vector2 uv1 = Vector2.zero;
            Vector2 uv2 = Vector2.zero;
            Vector2 uvc = Vector2.zero;

            float r0 = 0;
            float r1 = 0;
            float r2 = 0;
            float rc = 0;
            float rMax = 0;
            int subDivLevel = 0;

            baseResolution = Mathf.Max(0, baseResolution - lod);
            resolution = Mathf.Max(0, resolution - lod);

            int maxLevel = baseResolution + Mathf.Min(Mathf.FloorToInt(1f / GGeometryJobUtilities.SUB_DIV_STEP), resolution - baseResolution);

            for (int res = baseResolution; res < maxLevel; ++res)
            {
                startIndex = GGeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GGeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    if (creationState[i] != GGeometryJobUtilities.STATE_CREATED)
                        continue;
                    currentNode = baseTree[i];
                    GGeometryJobUtilities.NormalizeToPoint(ref uv0, ref uvRect, ref currentNode.v0);
                    GGeometryJobUtilities.NormalizeToPoint(ref uv1, ref uvRect, ref currentNode.v1);
                    GGeometryJobUtilities.NormalizeToPoint(ref uv2, ref uvRect, ref currentNode.v2);
                    uvc = (uv0 + uv1 + uv2) / 3;

                    r0 = GGeometryJobUtilities.GetColorBilinear(subdivMap, ref uv0).r;
                    r1 = GGeometryJobUtilities.GetColorBilinear(subdivMap, ref uv1).r;
                    r2 = GGeometryJobUtilities.GetColorBilinear(subdivMap, ref uv2).r;
                    rc = GGeometryJobUtilities.GetColorBilinear(subdivMap, ref uvc).r;

                    rMax = Mathf.Max(Mathf.Max(r0, r1), Mathf.Max(r2, rc));

                    subDivLevel = baseResolution + (int)(rMax / GGeometryJobUtilities.SUB_DIV_STEP);
                    if (subDivLevel <= res)
                        continue;

                    GGeometryJobUtilities.GetChildrenNodeIndex(ref i, ref leftNodeIndex, ref rightNodeIndex);
                    currentNode.Split(ref leftNode, ref rightNode);

                    baseTree[leftNodeIndex] = leftNode;
                    creationState[leftNodeIndex] = GGeometryJobUtilities.STATE_CREATED;
                    GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref leftNode);

                    baseTree[rightNodeIndex] = rightNode;
                    creationState[rightNodeIndex] = GGeometryJobUtilities.STATE_CREATED;
                    GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref rightNode);
                }
            }
        }
    }
}
#endif
