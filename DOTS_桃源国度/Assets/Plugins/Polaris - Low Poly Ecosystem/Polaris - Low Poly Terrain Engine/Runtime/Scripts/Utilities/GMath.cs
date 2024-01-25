#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Pinwheel.Griffin
{
    public static class GMath
    {
        public static float Float2Cross(float2 lhs, float2 rhs)
        {
            return lhs.y * rhs.x - lhs.x * rhs.y;
        }
    }
}
#endif
