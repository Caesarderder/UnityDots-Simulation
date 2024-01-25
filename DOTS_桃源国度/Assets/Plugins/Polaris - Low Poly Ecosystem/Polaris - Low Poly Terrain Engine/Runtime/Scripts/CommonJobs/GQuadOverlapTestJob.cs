#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using static Unity.Mathematics.math;

namespace Pinwheel.Griffin
{
    public struct GQuadOverlapTestJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Rect> rectsToTest;
        public GQuad2D quad;

        [WriteOnly]
        public NativeArray<bool> result;

        public void Execute(int index)
        {
            Rect r = rectsToTest[index];
            result[index] = GJobCommon.IsOverlap(r, quad);
        }
    }
}
#endif
