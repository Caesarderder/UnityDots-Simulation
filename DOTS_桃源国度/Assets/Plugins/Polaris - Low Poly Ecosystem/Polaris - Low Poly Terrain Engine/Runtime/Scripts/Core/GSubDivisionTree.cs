#if GRIFFIN
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GSubDivisionTree
    {
        public class Node
        {
            public int Level { get; private set; }

            private Vector2 v0;
            public Vector2 V0
            {
                get
                {
                    return v0;
                }
                set
                {
                    v0 = new Vector2(
                        (float)System.Math.Round(value.x, 3),
                        (float)System.Math.Round(value.y, 3));
                }
            }

            private Vector2 v1;
            public Vector2 V1
            {
                get
                {
                    return v1;
                }
                set
                {
                    v1 = new Vector2(
                        (float)System.Math.Round(value.x, 3),
                        (float)System.Math.Round(value.y, 3));
                }
            }

            private Vector2 v2;
            public Vector2 V2
            {
                get
                {
                    return v2;
                }
                set
                {
                    v2 = new Vector2(
                        (float)System.Math.Round(value.x, 3),
                        (float)System.Math.Round(value.y, 3));
                }
            }

            public Vector2 VC
            {
                get
                {
                    Vector2 v = (V0 + V1 + V2) / 3;
                    return new Vector2(
                        (float)System.Math.Round(v.x, 3),
                        (float)System.Math.Round(v.y, 3));
                }
            }

            public Vector2 V01
            {
                get
                {
                    Vector2 v = (V0 + V1) * 0.5f;
                    return new Vector2(
                        (float)System.Math.Round(v.x, 3),
                        (float)System.Math.Round(v.y, 3));
                }
            }

            public Vector2 V12
            {
                get
                {
                    Vector2 v = (V1 + V2) * 0.5f;
                    return new Vector2(
                        (float)System.Math.Round(v.x, 3),
                        (float)System.Math.Round(v.y, 3));
                }
            }

            public Vector2 V20
            {
                get
                {
                    Vector2 v = (V2 + V0) * 0.5f;
                    return new Vector2(
                        (float)System.Math.Round(v.x, 3),
                        (float)System.Math.Round(v.y, 3));
                }
            }

            private Node leftNode;
            public Node LeftNode
            {
                get
                {
                    return leftNode;
                }
                set
                {
                    leftNode = value;
                    if (leftNode != null)
                        leftNode.Level = Level + 1;
                }
            }

            private Node rightNode;
            public Node RightNode
            {
                get
                {
                    return rightNode;
                }
                set
                {
                    rightNode = value;
                    if (rightNode != null)
                        rightNode.Level = Level + 1;
                }
            }

            public Node()
            {
                this.Level = -1;
                this.V0 = Vector2.zero;
                this.V1 = Vector2.zero;
                this.V2 = Vector2.zero;
            }

            public Node(Vector2 v0, Vector2 v1, Vector2 v2)
            {
                this.Level = -1;
                this.V0 = v0;
                this.V1 = v1;
                this.V2 = v2;
            }

            public void Split()
            {
                Vector2 v12 = (V1 + V2) * 0.5f;
                Node n0 = new Node(v12, V0, V1);
                Node n1 = new Node(v12, V2, V0);
                LeftNode = n0;
                RightNode = n1;
            }

            public Node Clone()
            {
                Node n = new Node(V0, V1, V2);
                n.Level = Level;
                if (LeftNode != null)
                    n.LeftNode = LeftNode.Clone();
                if (RightNode != null)
                    n.RightNode = RightNode.Clone();
                return n;
            }
        }

        public Node Root;
        private Stack<Node> st = new Stack<Node>(1000);

        public GSubDivisionTree()
        {
            Root = new Node();
            Node left = new Node(
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(0, 0));
            Node right = new Node(
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(1, 1));
            Root.LeftNode = left;
            Root.RightNode = right;
        }

        public static GSubDivisionTree Rect(Rect r)
        {
            GSubDivisionTree tree = new GSubDivisionTree();
            tree.Root = new Node();

            Vector2 bottomLeft = new Vector2(r.min.x, r.min.y);
            Vector2 topLeft = new Vector2(r.min.x, r.max.y);
            Vector2 topRight = new Vector2(r.max.x, r.max.y);
            Vector2 bottomRight = new Vector2(r.max.x, r.min.y);

            Node left = new Node(
                topLeft,
                topRight,
                bottomLeft);
            Node right = new Node(
                bottomRight,
                bottomLeft,
                topRight);
            tree.Root.LeftNode = left;
            tree.Root.RightNode = right;

            return tree;
        }

        public void ForEachLeaf(Action<Node> action)
        {
            st.Clear();
            st.Push(Root.RightNode);
            st.Push(Root.LeftNode);

            Node n;
            while (st.Count > 0)
            {
                n = st.Pop();
                if (n.LeftNode == null && n.RightNode == null)
                {
                    action.Invoke(n);
                }
                else
                {
                    st.Push(n.RightNode);
                    st.Push(n.LeftNode);
                }
            }
        }

        public GSubDivisionTree Clone(int maxLevel)
        {
            GSubDivisionTree tree = new GSubDivisionTree();
            tree.Root = Root.Clone();

            st.Clear();
            st.Push(tree.Root.RightNode);
            st.Push(tree.Root.LeftNode);
            Node n;
            while (st.Count > 0)
            {
                n = st.Pop();
                if (n.Level == maxLevel)
                {
                    n.LeftNode = null;
                    n.RightNode = null;
                }
                else
                {
                    if (n.LeftNode != null)
                        st.Push(n.LeftNode);
                    if (n.RightNode != null)
                        st.Push(n.RightNode);
                }
            }
            return tree;
        }

        public int GetMaxLevel()
        {
            int max = -1;
            ForEachLeaf(n =>
            {
                if (n.Level > max)
                    max = n.Level;
            });
            return max;
        }
    }
}
#endif
