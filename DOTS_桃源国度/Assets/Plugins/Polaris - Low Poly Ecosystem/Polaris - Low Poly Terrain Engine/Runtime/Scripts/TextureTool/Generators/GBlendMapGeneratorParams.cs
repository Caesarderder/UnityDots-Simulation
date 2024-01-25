#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GBlendMapGeneratorParams
    {
        [SerializeField]
        private List<GBlendLayer> layers;
        public List<GBlendLayer> Layers
        {
            get
            {
                if (layers == null)
                {
                    layers = new List<GBlendLayer>();
                }
                return layers;
            }
            set
            {
                layers = value;
            }
        }
    }
}
#endif
