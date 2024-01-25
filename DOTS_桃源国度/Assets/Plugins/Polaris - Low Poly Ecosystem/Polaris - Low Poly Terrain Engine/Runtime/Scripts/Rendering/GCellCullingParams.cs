#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.Rendering
{
    public struct GCellCullingParams
    {
        public Vector3 boundCenter;
        public Vector3 boundSize;
        public int instanceCount;
    }
}
#endif
