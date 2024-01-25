#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public struct GOverlapTestResult
    {
        public GStylizedTerrain Terrain;
        public bool IsOverlapped;
        public bool[] IsChunkOverlapped;
    }
}
#endif
