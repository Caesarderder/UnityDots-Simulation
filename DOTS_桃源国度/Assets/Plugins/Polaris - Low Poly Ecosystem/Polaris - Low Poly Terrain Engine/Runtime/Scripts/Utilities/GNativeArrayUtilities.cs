#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.Griffin
{
    public static class GNativeArrayUtilities
    {
        public static void Dispose<T>(NativeArray<T> array) where T : struct
        {
            try
            {
                if (array.IsCreated)
                {
                    array.Dispose();
                }
            }
            catch (System.InvalidOperationException)
            {
            }
        }
    }
}
#endif
