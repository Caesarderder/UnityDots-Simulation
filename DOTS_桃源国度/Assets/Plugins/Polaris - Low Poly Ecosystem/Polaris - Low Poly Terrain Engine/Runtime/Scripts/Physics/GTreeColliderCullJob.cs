#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.Griffin.Physic
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = false)]
#endif
    public struct GTreeColliderCullJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<GTreeInstance> instances; //position in local space
        [WriteOnly]
        public NativeArray<bool> cullResults;

        public float maxDistance;
        public Vector3 targetPos;

        public void Execute(int index)
        {
            GTreeInstance tree = instances[index];
            Vector3 v = targetPos - tree.position;
            float sqrDistance = v.x * v.x + v.y * v.y + v.z * v.z;
            float sqrMaxDistance = maxDistance * maxDistance;
            cullResults[index] = sqrDistance <= sqrMaxDistance ? true : false;
        }
    }
}
#endif
