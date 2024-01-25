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
    internal struct GCreateBaseTreeJob : IJob
    {
        public NativeArray<GSubdivNode> nodes;
        [WriteOnly]
        public NativeArray<byte> creationState;
        [WriteOnly]
        public NativeArray<bool> vertexMarker;

        [WriteOnly]
        public NativeArray<int> metadata;

        public int baseResolution;
        public int resolution;
        public int lod;

        public void Execute()
        {
            ResetMetadata();
            ResetStates();
            ResetMarker();

            GSubdivNode nodes0 = new GSubdivNode()
            {
                v0 = Vector2.up,
                v1 = Vector2.one,
                v2 = Vector2.zero
            };
            nodes[0] = nodes0;
            creationState[0] = GGeometryJobUtilities.STATE_CREATED;
            GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref nodes0);

            GSubdivNode nodes1 = new GSubdivNode()
            {
                v0 = Vector2.right,
                v1 = Vector2.zero,
                v2 = Vector2.one
            };
            nodes[1] = nodes1;
            creationState[1] = GGeometryJobUtilities.STATE_CREATED;
            GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref nodes1);

            int startIndex = 0;
            int endIndex = 0;
            int leftNodeIndex = 0;
            int rightNodeIndex = 0;
            GSubdivNode currentNode;
            GSubdivNode leftNode = new GSubdivNode();
            GSubdivNode rightNode = new GSubdivNode();

            for (int res = 0; res < resolution; ++res)
            {
                startIndex = GGeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GGeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    currentNode = nodes[i];
                    currentNode.Split(ref leftNode, ref rightNode);
                    GGeometryJobUtilities.GetChildrenNodeIndex(ref i, ref leftNodeIndex, ref rightNodeIndex);

                    nodes[leftNodeIndex] = leftNode;
                    nodes[rightNodeIndex] = rightNode;
                }
            }

            baseResolution = Mathf.Max(0, baseResolution - lod);
            for (int res = 0; res <= baseResolution; ++res)
            {
                startIndex = GGeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GGeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    currentNode = nodes[i];
                    creationState[i] = GGeometryJobUtilities.STATE_CREATED;
                    GGeometryJobUtilities.MarkVertices(ref vertexMarker, ref currentNode);
                }
            }
        }

        private void ResetMetadata()
        {
            metadata[GGeometryJobUtilities.METADATA_LEAF_COUNT] = 0;
            metadata[GGeometryJobUtilities.METADATA_NEW_VERTEX_CREATED] = 0;
        }

        private void ResetStates()
        {
            for (int i = 0; i < creationState.Length; ++i)
            {
                creationState[i] = GGeometryJobUtilities.STATE_NOT_CREATED;
            }
        }

        private void ResetMarker()
        {
            for (int i = 0; i < vertexMarker.Length; ++i)
            {
                vertexMarker[i] = false;
            }
        }
    }
}
#endif
