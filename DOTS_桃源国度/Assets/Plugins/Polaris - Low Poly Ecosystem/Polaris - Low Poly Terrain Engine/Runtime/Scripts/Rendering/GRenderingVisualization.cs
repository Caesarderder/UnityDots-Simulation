#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.Griffin.Rendering
{
    public class GRenderingVisualization
    {
        public Vector3 boxMin;
        public Vector3 boxMax;
        public Color terrainBoundsColor;

        public Matrix4x4[] trs;
        public byte[] cullResult;
    }
}
#endif
