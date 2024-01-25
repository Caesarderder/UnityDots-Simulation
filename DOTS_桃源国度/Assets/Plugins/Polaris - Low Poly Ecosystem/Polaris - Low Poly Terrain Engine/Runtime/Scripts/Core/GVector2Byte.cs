#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public struct GVector2Byte
    {
        public static readonly GVector2Byte bottomLeft = new GVector2Byte(0, 0);
        public static readonly GVector2Byte topLeft = new GVector2Byte(0, 255);
        public static readonly GVector2Byte topRight = new GVector2Byte(255, 255);
        public static readonly GVector2Byte bottomRight = new GVector2Byte(255, 0);

        public byte x;
        public byte y;

        public float xFloat01
        {
            get
            {
                return (float)System.Math.Round(Mathf.InverseLerp(0, 255, x), 3);
            }
        }

        public float yFloat01
        {
            get
            {
                return (float)System.Math.Round(Mathf.InverseLerp(0, 255, y), 3);
            }
        }

        public GVector2Byte(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            string s = string.Format("({0}, {1})", xFloat01.ToString("0.00"), yFloat01.ToString("0.00"));
            return s;
        }
    }
}
#endif
