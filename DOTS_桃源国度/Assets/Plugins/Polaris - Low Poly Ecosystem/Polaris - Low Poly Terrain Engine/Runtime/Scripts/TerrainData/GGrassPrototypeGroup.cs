#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CreateAssetMenu(fileName = "Grass Prototype Group", menuName = "Polaris/Grass Prototype Group")]
    public class GGrassPrototypeGroup : ScriptableObject
    {
        [SerializeField]
        private List<GGrassPrototype> prototypes;
        public List<GGrassPrototype> Prototypes
        {
            get
            {
                if (prototypes == null)
                    prototypes = new List<GGrassPrototype>();
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

        public bool Equals(DetailPrototype[] detailPrototypes)
        {
            if (Prototypes.Count != detailPrototypes.Length)
                return false;
            for (int i = 0; i < Prototypes.Count; ++i)
            {
                if (!Prototypes[i].Equals(detailPrototypes[i]))
                    return false;
            }
            return true;
        }

        public static GGrassPrototypeGroup Create(DetailPrototype[] detailPrototypes)
        {
            GGrassPrototypeGroup group = CreateInstance<GGrassPrototypeGroup>();
            for (int i = 0; i < detailPrototypes.Length; ++i)
            {
                group.Prototypes.Add((GGrassPrototype)detailPrototypes[i]);
            }

            return group;
        }
    }
}
#endif
