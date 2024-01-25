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
    public struct GTransformTreesToLocalSpaceJob : IJobParallelFor
    {
        public NativeArray<GTreeInstance> instances;
        public Vector3 terrainSize;

        public void Execute(int index)
        {
            GTreeInstance tree = instances[index];
            Vector3 pos = new Vector3(
                tree.position.x * terrainSize.x,
                tree.position.y * terrainSize.y,
                tree.position.z * terrainSize.z);
            tree.position = pos;
            instances[index] = tree;
        }
    }
}
#endif
