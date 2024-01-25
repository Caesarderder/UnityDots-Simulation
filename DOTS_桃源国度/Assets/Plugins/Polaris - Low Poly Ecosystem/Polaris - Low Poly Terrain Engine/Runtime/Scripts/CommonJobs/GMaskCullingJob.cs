#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Pinwheel.Griffin
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = false)]
#endif
    public struct GMaskCullingJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector2> positions;
        [ReadOnly]
        public GTextureNativeDataDescriptor<Color32> mask;

        [WriteOnly]
        public NativeArray<bool> result;


        public void Execute(int index)
        {
            Vector2 pos = positions[index];
            Color c = GJobCommon.GetColorBilinear(mask, pos);
            float value = c.r;

            Random rand = Random.CreateFromIndex((uint)index);
            if (rand.NextFloat() < value)
            {
                result[index] = true;
            }
            else
            {
                result[index] = false;
            }
        }
    }
    public struct GMaskCullingDataHolder
    {
        public NativeArray<Vector2> positionsNA;
        public NativeArray<bool> resultNA;
    }
}
#endif