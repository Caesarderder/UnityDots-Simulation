#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    public struct GMaskPainterParams
    {
        [SerializeField]
        private GTextureChannel channel;
        public GTextureChannel Channel
        {
            get
            {
                return channel;
            }
            set
            {
                channel = value;
            }
        }

        [SerializeField]
        private bool visualize;
        public bool Visualize
        {
            get
            {
                return visualize;
            }
            set
            {
                visualize = value;
            }
        }
    }
}
#endif
