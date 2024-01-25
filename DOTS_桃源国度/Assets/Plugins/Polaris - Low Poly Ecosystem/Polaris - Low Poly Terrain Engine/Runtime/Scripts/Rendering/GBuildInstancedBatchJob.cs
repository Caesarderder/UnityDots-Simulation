#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Pinwheel.Griffin.Rendering
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct GBuildInstancedBatchJob : IJob
    {
        [ReadOnly]
        public NativeArray<GGrassInstance> instances;

        [WriteOnly]
        public NativeArray<Vector3Int> batchMetadata; //x: prototypeIndex, y: startIndex, z: length

        public int maxLength;

        public void Execute()
        {
            int startIndex = 0;
            int prototypeIndex = int.MinValue;
            Vector3Int metadata = new Vector3Int(instances[0].prototypeIndex, startIndex, 0);
            int batchCount = 0;

            int length = instances.Length;
            for (int i = 0; i < length; ++i)
            {
                prototypeIndex = instances[i].prototypeIndex;
                if (prototypeIndex != metadata.x)
                {
                    metadata.z = i - metadata.y;
                    batchCount += 1;
                    batchMetadata[batchCount] = metadata;

                    metadata.x = prototypeIndex;
                    metadata.y = i;
                    metadata.z = 0;
                }
                else if (i - metadata.y + 1 > maxLength)
                {
                    metadata.z = maxLength;
                    batchCount += 1;
                    batchMetadata[batchCount] = metadata;

                    metadata.y = i;
                    metadata.z = 0;
                }
                else if (i == length - 1)
                {
                    metadata.z = i - metadata.y + 1;
                    batchCount += 1;
                    batchMetadata[batchCount] = metadata;
                }
            }

            metadata.Set(-1, -1, batchCount);
            batchMetadata[0] = metadata;
        }
    }
}
#endif
