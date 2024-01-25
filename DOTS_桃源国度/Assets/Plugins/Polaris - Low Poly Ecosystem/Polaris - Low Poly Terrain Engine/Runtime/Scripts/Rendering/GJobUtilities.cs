#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;

namespace Pinwheel.Griffin.Rendering
{
    public static class GJobUtilities
    {
        public static void CompleteAll(JobHandle[] handles)
        {
            for (int i = 0; i < handles.Length; ++i)
            {
                handles[i].Complete();
            }
        }
    }
}
#endif
