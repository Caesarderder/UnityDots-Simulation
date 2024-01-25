#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [System.Serializable]
    public class GTextureStampLayer : GConditionalStampLayer
    {
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
        private float metallic;
        public float Metallic
        {
            get
            {
                return metallic;
            }
            set
            {
                metallic = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float smoothness;
        public float Smoothness
        {
            get
            {
                return smoothness;
            }
            set
            {
                smoothness = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private int splatIndex;
        public int SplatIndex
        {
            get
            {
                return splatIndex;
            }
            set
            {
                splatIndex = value;
            }
        }

        public GTextureStampLayer() : base() { }

        public static GTextureStampLayer Create()
        {
            GTextureStampLayer layer = new GTextureStampLayer();
            layer.Color = Color.white;
            layer.Metallic = 0;
            layer.Smoothness = 0;
            layer.SplatIndex = 0;
            layer.UpdateCurveTextures();

            return layer;
        }
    }
}
#endif
