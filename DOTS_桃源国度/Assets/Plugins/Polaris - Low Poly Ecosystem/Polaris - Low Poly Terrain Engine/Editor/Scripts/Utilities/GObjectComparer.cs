#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public class GObjectComparer<T> : IEqualityComparer<T> where T : Object
    {
        public bool Equals(T x, T y)
        {
            return x.GetInstanceID() == y.GetInstanceID();
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif
