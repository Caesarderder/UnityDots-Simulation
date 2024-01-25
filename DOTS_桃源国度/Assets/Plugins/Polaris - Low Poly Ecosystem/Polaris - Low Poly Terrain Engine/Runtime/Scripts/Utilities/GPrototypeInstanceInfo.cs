#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public struct GPrototypeInstanceInfo
    {
        public int prototypeIndex;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public GGrassInstance ToGrassInstance()
        {
            GGrassInstance g = new GGrassInstance();
            g.prototypeIndex = prototypeIndex;
            g.position = position;
            g.rotation = rotation;
            g.scale = scale;
            return g;
        }

        public GTreeInstance ToTreeInstance()
        {
            GTreeInstance t = new GTreeInstance();
            t.prototypeIndex = prototypeIndex;
            t.position = position;
            t.rotation = rotation;
            t.scale = scale;
            return t;
        }
    }
}
#endif
