#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.Rendering
{
    internal class GInstancedBatch
    {
        internal Matrix4x4[] transforms;
        internal int instanceCount;
        internal int prototypeIndex;

        public GInstancedBatch(int maxInstanceCount)
        {
            transforms = new Matrix4x4[maxInstanceCount];
            instanceCount = 0;
        }

        public void AddTransform(Matrix4x4 m)
        {
            transforms[instanceCount] = m;
            instanceCount += 1;
        }

        public void ClearTransforms()
        {
            instanceCount = 0;
        }
    }
}
#endif
