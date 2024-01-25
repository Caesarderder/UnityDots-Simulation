#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Pinwheel.Griffin
{
    public struct Line2D
    {
        public float2 startPoint;
        public float2 endPoint;

        public float2 Direction
        {
            get
            {
                return normalize(endPoint - startPoint);
            }
        }

        public float Length
        {
            get
            {
                return distance(startPoint, endPoint);
            }
        }

        public float SqrLength
        {
            get
            {
                return distancesq(startPoint, endPoint);
            }
        }

        public Line2D(float2 start, float2 end)
        {
            startPoint = start;
            endPoint = end;
        }

        public Line2D(float x1, float y1, float x2, float y2)
        {
            startPoint = new float2(x1, y1);
            endPoint = new float2(x2, y2);
        }

        public float GetX(float y)
        {
            float2 dir = endPoint - startPoint;
            float a = -dir.y;
            float b = dir.x;
            float c = -(a * startPoint.x + b * startPoint.y);
            float x = (-b * y - c) / a;
            return x;
        }

        public float GetY(float x)
        {
            float2 dir = endPoint - startPoint;
            float a = -dir.y;
            float b = dir.x;
            float c = -(a * startPoint.x + b * startPoint.y);
            float y = (-a * x - c) / b;
            return y;
        }

        public static bool Intersect(Line2D l1, Line2D l2, out float2 point)
        {
            bool result = false;
            float x1 = l1.startPoint.x;
            float x2 = l1.endPoint.x;
            float x3 = l2.startPoint.x;
            float x4 = l2.endPoint.x;
            float y1 = l1.startPoint.y;
            float y2 = l1.endPoint.y;
            float y3 = l2.startPoint.y;
            float y4 = l2.endPoint.y;

            float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
            {
                point = new float2(0, 0);
                result = false;
            }
            else
            {
                float xNumerator = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
                float yNumerator = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                point = new float2(xNumerator / denominator, yNumerator / denominator);
                float sqrLength1 = l1.SqrLength;
                float sqrLength2 = l2.SqrLength;
                if (distancesq(point, l1.startPoint) > sqrLength1 || distancesq(point, l1.endPoint) > sqrLength1)
                {
                    result = false;
                }
                else if (distancesq(point, l2.startPoint) > sqrLength2 || distancesq(point, l2.endPoint) > sqrLength2)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
#endif
