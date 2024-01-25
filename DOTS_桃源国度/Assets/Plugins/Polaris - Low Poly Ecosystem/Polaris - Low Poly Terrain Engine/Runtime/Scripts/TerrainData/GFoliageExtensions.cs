#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Pinwheel.Griffin
{
    public static class GFoliageExtensions
    {
        public static void AddGrassInstancesWithFilter(this GFoliage f, NativeArray<bool> filterNA, NativeArray<GPrototypeInstanceInfo> foliageInfoNA)
        {
            GGrassPatch[] patches = f.GrassPatches;
            NativeArray<Rect> patchRectsNA = new NativeArray<Rect>(patches.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int r = 0; r < patches.Length; ++r)
            {
                patchRectsNA[r] = patches[r].GetUvRange();
            }

            NativeArray<int> patchIndexNA = new NativeArray<int>(foliageInfoNA.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GPatchTestWithFilterJob job = new GPatchTestWithFilterJob()
            {
                patchRects = patchRectsNA,
                filter = filterNA,
                foliageInfo = foliageInfoNA,
                patchIndex = patchIndexNA
            };

            JobHandle jHandle = job.Schedule(foliageInfoNA.Length, 100);
            jHandle.Complete();

            int[] patchIndex = patchIndexNA.ToArray();
            GPrototypeInstanceInfo[] foliageInfo = foliageInfoNA.ToArray();

            patchRectsNA.Dispose();
            patchIndexNA.Dispose();

            bool[] dirty = new bool[patches.Length];
            for (int i = 0; i < foliageInfo.Length; ++i)
            {
                if (patchIndex[i] < 0)
                    continue;
                patches[patchIndex[i]].Instances.Add(foliageInfo[i].ToGrassInstance());
                dirty[patchIndex[i]] = true;
            }

            for (int i = 0; i < dirty.Length; ++i)
            {
                if (dirty[i] == true)
                {
                    patches[i].RecalculateBounds();
                    patches[i].Changed();
                }
            }
        }

#if GRIFFIN_BURST
        [BurstCompile(CompileSynchronously = false)]
#endif
        public struct GPatchTestWithFilterJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Rect> patchRects;
            [ReadOnly]
            public NativeArray<bool> filter;
            [ReadOnly]
            public NativeArray<GPrototypeInstanceInfo> foliageInfo;

            [WriteOnly]
            public NativeArray<int> patchIndex;

            public void Execute(int index)
            {
                patchIndex[index] = -1;
                if (filter[index] == false)
                {
                    return;
                }

                GPrototypeInstanceInfo info = foliageInfo[index];
                Vector2 pos = new Vector2(info.position.x, info.position.z);
                int length = patchRects.Length;
                for (int i = 0; i < length; ++i)
                {
                    if (patchRects[i].Contains(pos))
                    {
                        patchIndex[index] = i;
                        return;
                    }
                }
            }
        }
    }
}
#endif
