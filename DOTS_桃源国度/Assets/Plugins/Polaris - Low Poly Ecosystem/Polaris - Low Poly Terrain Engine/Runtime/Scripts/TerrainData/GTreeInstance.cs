#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    [System.Serializable]
    public struct GTreeInstance
    {
        [SerializeField]
        internal int prototypeIndex;
        public int PrototypeIndex
        {
            get
            {
                return prototypeIndex;
            }
            set
            {
                prototypeIndex = value;
            }
        }

        [SerializeField]
        internal Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        [SerializeField]
        internal Quaternion rotation;
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        [SerializeField]
        internal Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
            }
        }

        public static GTreeInstance Create(int prototypeIndex)
        {
            GTreeInstance tree = new GTreeInstance();
            tree.PrototypeIndex = prototypeIndex;
            tree.Position = Vector3.zero;
            tree.Rotation = Quaternion.identity;
            tree.Scale = Vector3.one;

            return tree;
        }

        public static explicit operator GTreeInstance(TreeInstance t)
        {
            GTreeInstance tree = Create(t.prototypeIndex);
            tree.Position = t.position;
            tree.Rotation = Quaternion.Euler(0, t.rotation * Mathf.Rad2Deg, 0);
            tree.Scale = new Vector3(t.widthScale, t.heightScale, t.widthScale);

            return tree;
        }

        public static explicit operator TreeInstance(GTreeInstance t)
        {
            TreeInstance tree = new TreeInstance();
            tree.prototypeIndex = t.PrototypeIndex;
            tree.position = t.Position;
            tree.widthScale = t.Scale.x;
            tree.heightScale = t.Scale.y;
            tree.color = Color.white;

            return tree;
        }

        internal static int GetStructSize()
        {
            return sizeof(int) + sizeof(float) * 3 + sizeof(float) * 4 + sizeof(float) * 3;
        }
    }
}
#endif
