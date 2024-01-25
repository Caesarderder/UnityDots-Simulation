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
    public struct GBuildRaycastCommandJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Rect> dirtyRects;
        [ReadOnly]
        public NativeArray<Vector2> positions;

        [WriteOnly]
        public NativeArray<RaycastCommand> commands;

        public int mask;
        public int maxHit;
        public Vector3 terrainPosition;
        public Vector3 terrainSize;

        public void Execute(int index)
        {
            Vector2 pos = positions[index];

            Vector3 from = new Vector3(terrainPosition.x + pos.x * terrainSize.x, 10000, terrainPosition.z + pos.y * terrainSize.z);
#if UNITY_2022_2_OR_NEWER
            QueryParameters q = new QueryParameters(mask, false, QueryTriggerInteraction.Ignore, false);
            RaycastCommand cmd = new RaycastCommand(from, Vector3.down, q, float.MaxValue);
#else
            RaycastCommand cmd = new RaycastCommand(from, Vector3.down, float.MaxValue, mask, 1);
#endif

            commands[index] = cmd;
        }
    }
}
#endif
