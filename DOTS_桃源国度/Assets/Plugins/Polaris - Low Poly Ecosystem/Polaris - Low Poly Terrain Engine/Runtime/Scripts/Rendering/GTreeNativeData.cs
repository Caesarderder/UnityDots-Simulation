#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;

namespace Pinwheel.Griffin.Rendering
{
    internal class GTreeNativeData : IDisposable
    {
        internal NativeArray<GTreeInstance> instances;
        internal NativeArray<int> prototypeIndices;
        internal NativeArray<Matrix4x4> trs;
        internal NativeArray<byte> cullResults;

        public GTreeNativeData(List<GTreeInstance> trees)
        {
            instances = new NativeArray<GTreeInstance>(trees.ToArray(), Allocator.Persistent);
            prototypeIndices = new NativeArray<int>(trees.Count, Allocator.Persistent);
            GUtilities.Fill(prototypeIndices, -1);
            trs = new NativeArray<Matrix4x4>(trees.Count, Allocator.Persistent);
            cullResults = new NativeArray<byte>(trees.Count, Allocator.Persistent);
        }

        public void Dispose()
        {
            GNativeArrayUtilities.Dispose(instances);
            GNativeArrayUtilities.Dispose(prototypeIndices);
            GNativeArrayUtilities.Dispose(trs);
            GNativeArrayUtilities.Dispose(cullResults);
        }
    }
}
#endif
