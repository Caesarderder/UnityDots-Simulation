#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Pinwheel.Griffin
{
    public struct GQuad2D
    {
        public float2 p0, p1, p2, p3;

        public GQuad2D(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public bool Contains(float2 point)
        {
            Vector2 d0, d1;
            float c0, c1, c2, c3;

            d0 = p1 - p0;
            d1 = point - p0;
            c0 = GMath.Float2Cross(d0, d1);

            d0 = p2 - p1;
            d1 = point - p1;
            c1 = GMath.Float2Cross(d0, d1);

            d0 = p3 - p2;
            d1 = point - p2;
            c2 = GMath.Float2Cross(d0, d1);

            d0 = p0 - p3;
            d1 = point - p3;
            c3 = GMath.Float2Cross(d0, d1);

            return c0 > 0 && c1 > 0 && c2 > 0 && c3 > 0;
        }
    }
}
#endif
