#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace Pinwheel.Griffin.Rendering
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct GCullCellJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<GCellCullingParams> cullParams;
        [ReadOnly]
        public NativeArray<Plane> frustum;

        public Matrix4x4 normalizeToWorldMatrix;
        public int visibleValue;
        public int culledValue;

        [WriteOnly]
        public NativeArray<int> cullResults;

        public void Execute(int index)
        {
            GCellCullingParams param = cullParams[index];
            if (param.instanceCount == 0)
            {
                cullResults[index] = culledValue;
                return;
            }
            BoundingSphere b = new BoundingSphere();
            b.position = normalizeToWorldMatrix.MultiplyPoint(param.boundCenter);
            b.radius = normalizeToWorldMatrix.MultiplyVector(param.boundSize).x;
            if (!DoFrustumTest(frustum, b))
            {
                cullResults[index] = culledValue;
            }
            else
            {
                cullResults[index] = visibleValue;
            }
        }

        private bool DoFrustumTest(NativeArray<Plane> frustum, BoundingSphere bounds)
        {
            float d = 0;
            for (int i = 0; i < 6; ++i)
            {
                d = frustum[i].GetDistanceToPoint(bounds.position);
                if (d < -bounds.radius)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif
