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
    public struct GStitchSeamLODJob : IJob
    {
        public NativeArray<GSubdivNode> nodes;
        public NativeArray<byte> creationState;
        public NativeArray<int> metadata;

        public NativeArray<bool> vertexMarker;

        [ReadOnly]
        public NativeArray<bool> vertexMarker_LOD0;

        public int meshBaseResolution;
        public int meshResolution;
        public int lod;

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

            meshBaseResolution = Mathf.Max(0, meshBaseResolution - lod);

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
                    mark3 = CheckMarkLOD0(currentNode.v01);
                    mark4 = CheckMarkLOD0(currentNode.v12);
                    mark5 = CheckMarkLOD0(currentNode.v20);

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

        private bool CheckMarkLOD0(Vector2 p)
        {
            if (p.x > 0 && p.x < 1 && p.y > 0 && p.y < 1)
            {
                return false;
            }
            else
            {
                return GGeometryJobUtilities.GetVertexMark(vertexMarker_LOD0, p);
            }
        }
    }
}
#endif
