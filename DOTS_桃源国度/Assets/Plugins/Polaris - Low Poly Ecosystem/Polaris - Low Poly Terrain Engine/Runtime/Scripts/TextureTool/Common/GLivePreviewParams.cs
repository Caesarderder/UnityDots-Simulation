#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GLivePreviewParams
    {
        [SerializeField]
        private bool enable;
        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                enable = value;
            }
        }

        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private Color color;
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        [SerializeField]
        private GLivePreviewMode mode;
        public GLivePreviewMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }
    }
}
#endif
