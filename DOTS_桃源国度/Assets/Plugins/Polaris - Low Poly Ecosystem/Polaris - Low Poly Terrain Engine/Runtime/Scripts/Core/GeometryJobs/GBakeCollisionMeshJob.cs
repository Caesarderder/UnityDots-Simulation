using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

namespace Pinwheel.Griffin
{
    public struct GBakeCollisionMeshJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> instanceIds;

        public void Execute(int index)
        {
            Physics.BakeMesh(instanceIds[index], false);
        }
    }
}
