#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.Griffin
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = false)]
#endif
    public struct GStitchSeamJob : IJob
    {
        public NativeArray<GSubdivNode> nodes;
        public NativeArray<byte> creationState;
        public NativeArray<int> metadata;

        public NativeArray<bool> vertexMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarkerLeft;
        public bool hasLeftMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarkerTop;
        public bool hasTopMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarkerRight;
        public bool hasRightMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarkerBottom;
        public bool hasBottomMarker;

        public int meshBaseResolution;
        public int meshResolution;

        public void Execute()
        {
            int startIndex = 0;
            int endIndex = 0;
            int leftNodeIndex = 0;
            int rightNodeIndex = 0;
            bool mark0 = false;
            bool mark1 = false;
            bool mark2 = false;
            bool mark3 = false;
            bool mark4 = false;
            bool mark5 = false;
            Vector2 v12 = Vector2.zero;

            GSubdivNode currentNode;
            GSubdivNode leftNode = new GSubdivNode();
            GSubdivNode rightNode = new GSubdivNode();

            metadata[GGeometryJobUtilities.METADATA_NEW_VERTEX_CREATED] = 0;
            for (int res = meshBaseResolution; res < meshResolution; ++res)
            {
                startIndex = GGeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GGeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    GGeometryJobUtilities.GetChildrenNodeIndex(ref i, ref leftNodeIndex, ref rightNodeIndex);
                    if (creationState[leftNodeIndex] != GGeometryJobUtilities.STATE_NOT_CREATED ||
                        creationState[rightNodeIndex] != GGeometryJobUtilities.STATE_NOT_CREATED)
                        continue;
                    currentNode = nodes[i];
                    mark0 = GGeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v01);
                    mark1 = GGeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v12);
                    mark2 = GGeometryJobUtilities.GetVertexMark(vertexMarker, currentNode.v20);
                    mark3 = CheckMarkNeighbor(currentNode.v01);
                    mark4 = CheckMarkNeighbor(currentNode.v12);
                    mark5 = CheckMarkNeighbor(currentNode.v20);

                    if (mark0 || mark1 || mark2 ||
                        mark3 || mark4 || mark5)
                    {
                        currentNode.Split(ref leftNode, ref rightNode);
                        nodes[leftNodeIndex] = leftNode;
                        nodes[rightNodeIndex] = rightNode;
                        creationState[leftNodeIndex] = GGeometryJobUtilities.STATE_CREATED;
                        creationState[rightNodeIndex] = GGeometryJobUtilities.STATE_CREATED;
                        GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref leftNode);
                        GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref rightNode);

                        metadata[GGeometryJobUtilities.METADATA_NEW_VERTEX_CREATED] = 1;
                    }
                }
            }
        }

        private bool CheckMarkNeighbor(Vector2 p)
        {
            if (p.x > 0 && p.x < 1 &&
                p.y > 0 && p.y < 1)
            {
                return false;
            }

            if (p.x == 0)
            {
                if (hasLeftMarker)
                {
                    return GGeometryJobUtilities.GetVertexMark(vertexMarkerLeft, p, true, false);
                }
                else
                {
                    return false;
                }
            }

            if (p.x == 1)
            {
                if (hasRightMarker)
                {
                    return GGeometryJobUtilities.GetVertexMark(vertexMarkerRight, p, true, false);
                }
                else
                {
                    return false;
                }
            }

            if (p.y == 0)
            {
                if (hasBottomMarker)
                {
                    return GGeometryJobUtilities.GetVertexMark(vertexMarkerBottom, p, false, true);
                }
                else
                {
                    return false;
                }
            }

            if (p.y == 1)
            {
                if (hasTopMarker)
                {
                    return GGeometryJobUtilities.GetVertexMark(vertexMarkerTop, p, false, true);
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

    }
}
#endif
