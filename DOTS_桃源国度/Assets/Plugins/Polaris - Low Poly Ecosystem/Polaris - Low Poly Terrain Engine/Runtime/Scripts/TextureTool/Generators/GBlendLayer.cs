#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public class GBlendLayer
    {
        [SerializeField]
        private GBlendDataSource dataSource;
        public GBlendDataSource DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
            }
        }

        private Texture2D texture;
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        [SerializeField]
        private float number;
        public float Number
        {
            get
            {
                return number;
            }
            set
            {
                number = value;
            }
        }

        [SerializeField]
        private Vector4 vector;
        public Vector4 Vector
        {
            get
            {
                return vector;
            }
            set
            {
                vector = value;
            }
        }

        [SerializeField]
        private GBlendOperation blendOps;
        public GBlendOperation BlendOps
        {
            get
            {
                return blendOps;
            }
            set
            {
                blendOps = value;
            }
        }

        [SerializeField]
        private float lerpFactor;
        public float LerpFactor
        {
            get
            {
                return lerpFactor;
            }
            set
            {
                lerpFactor = Mathf.Clamp01(value);
            }
        }

        private Texture2D lerpMask;
        public Texture2D LerpMask
        {
            get
            {
                return lerpMask;
            }
            set
            {
                lerpMask = value;
            }
        }

        [SerializeField]
        private bool saturate;
        public bool Saturate
        {
            get
            {
                return saturate;
            }
            set
            {
                saturate = value;
            }
        }

        public static GBlendLayer Create()
        {
            GBlendLayer layer = new GBlendLayer();
            layer.dataSource = GBlendDataSource.Texture;
            layer.texture = null;
            layer.number = 1;
            layer.vector = Vector4.one;
            layer.blendOps = GBlendOperation.Add;
            layer.lerpFactor = 0.5f;
            layer.saturate = true;
            return layer;
        }
    }
}
#endif
