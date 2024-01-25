#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CreateAssetMenu(fileName = "Splat Prototype Group", menuName = "Polaris/Splat Prototype Group")]
    public class GSplatPrototypeGroup : ScriptableObject
    {
        [SerializeField]
        private List<GSplatPrototype> prototypes;
        public List<GSplatPrototype> Prototypes
        {
            get
            {
                if (prototypes == null)
                {
                    prototypes = new List<GSplatPrototype>();
                }
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

        public bool Equals(SplatPrototype[] layers)
        {
            if (Prototypes.Count != layers.Length)
                return false;
            for (int i = 0; i < layers.Length; ++i)
            {
                if (!Prototypes[i].Equals(layers[i]))
                    return false;
            }
            return true;
        }

        public static GSplatPrototypeGroup Create(SplatPrototype[] layers)
        {
            GSplatPrototypeGroup group = CreateInstance<GSplatPrototypeGroup>();
            for (int i = 0; i < layers.Length; ++i)
            {
                group.Prototypes.Add((GSplatPrototype)layers[i]);
            }

            return group;
        }

#if !UNITY_2018_1 && !UNITY_2018_2
        public bool Equals(TerrainLayer[] layers)
        {
            if (Prototypes.Count != layers.Length)
                return false;
            for (int i = 0; i < layers.Length; ++i)
            {
                if (!Prototypes[i].Equals(layers[i]))
                    return false;
            }
            return true;
        }

        public static GSplatPrototypeGroup Create(TerrainLayer[] layers)
        {
            GSplatPrototypeGroup group = CreateInstance<GSplatPrototypeGroup>();
            for (int i = 0; i < layers.Length; ++i)
            {
                group.Prototypes.Add((GSplatPrototype)layers[i]);
            }

            return group;
        }
#endif
    }
}
#endif
