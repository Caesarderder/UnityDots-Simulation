#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Pinwheel.Griffin.Rendering
{
#if GRIFFIN_BURST
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct GCalculateGrassTransformJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<GGrassInstance> instances;
        [ReadOnly]
        public NativeArray<float> prototypePivotOffset;
        [ReadOnly]
        public NativeArray<Vector3> prototypeSize;

        [WriteOnly]
        public NativeArray<Matrix4x4> transforms;

        public Vector3 terrainSize;
        public Vector3 terrainPos;

        public void Execute(int i)
        {
            GGrassInstance grass = instances[i];
            
            float pivotOffset;
            Vector3 size;
            if (grass.prototypeIndex < 0 || grass.prototypeIndex >= prototypePivotOffset.Length)
            {
                pivotOffset = 0;
                size = Vector3.one;
            }
            else
            {
                pivotOffset = prototypePivotOffset[grass.prototypeIndex];
                size = prototypeSize[grass.prototypeIndex];
            }

            Vector3 worldPos = new Vector3(
                grass.position.x * terrainSize.x + terrainPos.x,
                grass.position.y * terrainSize.y + terrainPos.y + pivotOffset,
                grass.position.z * terrainSize.z + terrainPos.z);

            Vector3 worldScale = new Vector3(
                grass.scale.x * size.x,
                grass.scale.y * size.y,
                grass.scale.z * size.z);

            Matrix4x4 matrix = Matrix4x4.TRS(worldPos, grass.rotation, worldScale);

            transforms[i] = matrix;
        }
    }
}
#endif
