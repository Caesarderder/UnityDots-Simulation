#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public struct GSubdivNode
    {
        public Vector2 v0;
        public Vector2 v1;
        public Vector2 v2;

        public Vector2 v01
        {
            get
            {
                return new Vector2()
                {
                    x = (v0.x + v1.x) * 0.5f,
                    y = (v0.y + v1.y) * 0.5f
                };
            }
        }

        public Vector2 v12
        {
            get
            {
                return new Vector2()
                {
                    x = (v1.x + v2.x) * 0.5f,
                    y = (v1.y + v2.y) * 0.5f
                };
            }
        }

        public Vector2 v20
        {
            get
            {
                return new Vector2()
                {
                    x = (v2.x + v0.x) * 0.5f,
                    y = (v2.y + v0.y) * 0.5f
                };
            }
        }

        public Vector2 vc
        {
            get
            {
                return (v0 + v1 + v2) / 3f;
            }
        }

        public void Split(ref GSubdivNode leftNode, ref GSubdivNode rightNode)
        {
            Vector2 v12 = this.v12;

            leftNode.v0 = v12;
            leftNode.v1 = this.v2;
            leftNode.v2 = this.v0;

            rightNode.v0 = v12;
            rightNode.v1 = v0;
            rightNode.v2 = v1;
        }

        //public override string ToString()
        //{
        //    string s = string.Format("{0}; {1}; {2};", v0.ToString(), v1.ToString(), v2.ToString());
        //    return s;
        //}
    }
}
#endif
