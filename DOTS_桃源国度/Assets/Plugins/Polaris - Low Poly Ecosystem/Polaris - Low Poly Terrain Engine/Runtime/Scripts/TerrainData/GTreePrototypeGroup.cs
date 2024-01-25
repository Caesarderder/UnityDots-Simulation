#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CreateAssetMenu(fileName = "Tree Prototype Group", menuName = "Polaris/Tree Prototype Group")]
    public class GTreePrototypeGroup : ScriptableObject
    {
        [SerializeField]
        private List<GTreePrototype> prototypes;
        public List<GTreePrototype> Prototypes
        {
            get
            {
                if (prototypes == null)
                    prototypes = new List<GTreePrototype>();
                return prototypes;
            }
            set
            {
                prototypes = value;
            }
        }

        [SerializeField]
        private bool isSampleAsset;
        public bool IsSampleAsset => isSampleAsset;

        public bool Equals(TreePrototype[] treePrototypes)
        {
            if (Prototypes.Count != treePrototypes.Length)
                return false;
            for (int i = 0; i < Prototypes.Count; ++i)
            {
                if (!Prototypes[i].Equals(treePrototypes[i]))
                    return false;
            }
            return true;
        }

        public static GTreePrototypeGroup Create(TreePrototype[] treePrototypes)
        {
            GTreePrototypeGroup group = CreateInstance<GTreePrototypeGroup>();
            for (int i = 0; i < treePrototypes.Length; ++i)
            {
                group.Prototypes.Add((GTreePrototype)treePrototypes[i]);
            }

            return group;
        }
    }
}
#endif
