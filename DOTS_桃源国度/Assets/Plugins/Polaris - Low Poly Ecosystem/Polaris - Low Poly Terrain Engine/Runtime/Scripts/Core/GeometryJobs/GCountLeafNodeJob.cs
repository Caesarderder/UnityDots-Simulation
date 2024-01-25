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
    public struct GCountLeafNodeJob : IJob
    {
        public NativeArray<byte> creationState;
        [WriteOnly]
        public NativeArray<int> metadata;

        public int baseResolution;
        public int resolution;
        public int lod;
         
        public void Execute()
        {
            int startIndex = 0;
            int endIndex = 0;
            int leftNodeIndex = 0;
            int rightNodeIndex = 0;
            int count = 0;
            int length = creationState.Length;

            baseResolution = Mathf.Max(0, baseResolution - lod);

            for (int res = baseResolution; res <= resolution; ++res)
            {
                startIndex = GGeometryJobUtilities.GetStartIndex(ref res);
                endIndex = startIndex + GGeometryJobUtilities.GetElementCountForSubdivLevel(ref res) - 1;
                for (int i = startIndex; i <= endIndex; ++i)
                {
                    if (creationState[i] != GGeometryJobUtilities.STATE_CREATED)
                        continue;
                    GGeometryJobUtilities.GetChildrenNodeIndex(ref i, ref leftNodeIndex, ref rightNodeIndex);
                    if (leftNodeIndex >= length || rightNodeIndex >= length)
                    {
                        creationState[i] = GGeometryJobUtilities.STATE_LEAF;
                        count += 1;
                        continue;
                    }
                    if (creationState[leftNodeIndex] == GGeometryJobUtilities.STATE_NOT_CREATED ||
                        creationState[rightNodeIndex] == GGeometryJobUtilities.STATE_NOT_CREATED)
                    {
                        creationState[i] = GGeometryJobUtilities.STATE_LEAF;
                        count += 1;
                        continue;
                    }
                }
            }
            metadata[0] = count;
        }
    }
}
#endif
