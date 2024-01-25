#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Pinwheel.Griffin
{
    public class GJobCommon
    {
        public static Color GetColorBilinear(GTextureNativeDataDescriptor<Color32> tex, Vector2 uv)
        {
            return GetColorBilinear(tex, ref uv);
        }

        public static Color GetColorBilinear(GTextureNativeDataDescriptor<Color32> tex, ref Vector2 uv)
        {
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, tex.width - 1, uv.x),
                Mathf.Lerp(0, tex.height - 1, uv.y));
            //apply a bilinear filter
            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int xCeil = Mathf.CeilToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            int yCeil = Mathf.CeilToInt(pixelCoord.y);

            Color f00 = tex.data[To1DIndex(ref xFloor, ref yFloor, ref tex.width)];
            Color f01 = tex.data[To1DIndex(ref xFloor, ref yCeil, ref tex.width)];
            Color f10 = tex.data[To1DIndex(ref xCeil, ref yFloor, ref tex.width)];
            Color f11 = tex.data[To1DIndex(ref xCeil, ref yCeil, ref tex.width)];

            Vector2 unitCoord = new Vector2(
                pixelCoord.x - xFloor,
                pixelCoord.y - yFloor);

            Color color =
                f00 * (1 - unitCoord.x) * (1 - unitCoord.y) +
                f01 * (1 - unitCoord.x) * unitCoord.y +
                f10 * unitCoord.x * (1 - unitCoord.y) +
                f11 * unitCoord.x * unitCoord.y;

            return color;
        }

        public static Color GetColorPoint(GTextureNativeDataDescriptor<Color32> tex, Vector2 uv)
        {
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, tex.width - 1, uv.x),
                Mathf.Lerp(0, tex.height - 1, uv.y));

            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            return tex.data[To1DIndex(ref xFloor, ref yFloor, ref tex.width)];
        }

        public static int To1DIndex(ref int x, ref int y, ref int width)
        {
            return y * width + x;
        }

        public static bool IsOverlap(Rect rect, GQuad2D quad)
        {
            if (rect.Contains(quad.p0) || rect.Contains(quad.p1) || rect.Contains(quad.p2) || rect.Contains(quad.p3))
            {
                return true;
            }

            float2 r0 = new float2(rect.min.x, rect.min.y);
            float2 r1 = new float2(rect.min.x, rect.max.y);
            float2 r2 = new float2(rect.max.x, rect.max.y);
            float2 r3 = new float2(rect.max.x, rect.min.y);

            if (quad.Contains(r0) || quad.Contains(r1) || quad.Contains(r2) || quad.Contains(r3))
            {
                return true;
            }

            float2 q0 = quad.p0;
            float2 q1 = quad.p1;
            float2 q2 = quad.p2;
            float2 q3 = quad.p3;

            if (IsIntersect(r0, r1, q0, q1, q2, q3))
                return true;
            if (IsIntersect(r1, r2, q0, q1, q2, q3))
                return true;
            if (IsIntersect(r2, r3, q0, q1, q2, q3))
                return true;
            if (IsIntersect(r3, r0, q0, q1, q2, q3))
                return true;


            return false;
        }

        private static bool IsIntersect(float2 r0, float2 r1, float2 q0, float2 q1, float2 q2, float2 q3)
        {
            Line2D l1 = new Line2D(r0, r1);
            Line2D l2;
            float2 point;

            l2 = new Line2D(q0, q1);
            if (Line2D.Intersect(l1, l2, out point))
                return true;

            l2 = new Line2D(q1, q2);
            if (Line2D.Intersect(l1, l2, out point))
                return true;

            l2 = new Line2D(q2, q3);
            if (Line2D.Intersect(l1, l2, out point))
                return true;

            l2 = new Line2D(q3, q0);
            if (Line2D.Intersect(l1, l2, out point))
                return true;

            return false;
        }
    }
}
#endif
