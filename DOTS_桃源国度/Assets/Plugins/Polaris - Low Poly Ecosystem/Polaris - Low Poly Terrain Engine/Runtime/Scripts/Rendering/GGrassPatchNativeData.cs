#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;

namespace Pinwheel.Griffin.Rendering
{
    internal class GGrassPatchNativeData : IDisposable
    {
        internal NativeArray<GGrassInstance> instances;
        internal NativeArray<Matrix4x4> trs;
        internal NativeArray<Vector3Int> metadata;

        public GGrassPatchNativeData(List<GGrassInstance> grasses)
        {
            grasses.Sort((g0, g1) => { return g0.prototypeIndex.CompareTo(g1.prototypeIndex); });
            instances = new NativeArray<GGrassInstance>(grasses.ToArray(), Allocator.TempJob);
            trs = new NativeArray<Matrix4x4>(grasses.Count, Allocator.TempJob);
            metadata = new NativeArray<Vector3Int>(grasses.Count + 1, Allocator.TempJob);
        }

        public void Dispose()
        {
            GNativeArrayUtilities.Dispose(instances);
            GNativeArrayUtilities.Dispose(trs);
            GNativeArrayUtilities.Dispose(metadata);
        }
    }
}
#endif
