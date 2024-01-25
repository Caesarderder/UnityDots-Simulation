#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin
{
    [System.Serializable]
    public class GSplatPrototype
    {
        [SerializeField]
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
        private Texture2D normalMap;
        public Texture2D NormalMap
        {
            get
            {
                return normalMap;
            }
            set
            {
                normalMap = value;
            }
        }

        [SerializeField]
        private Vector2 tileSize = Vector2.one;
        public Vector2 TileSize
        {
            get
            {
                return tileSize;
            }
            set
            {
                tileSize = value;
            }
        }

        [SerializeField]
        private Vector2 tileOffset = Vector2.zero;
        public Vector2 TileOffset
        {
            get
            {
                return tileOffset;
            }
            set
            {
                tileOffset = value;
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

        public bool Equals(SplatPrototype layer)
        {
            return
                texture == layer.texture &&
                normalMap == layer.normalMap &&
                tileSize == layer.tileSize &&
                tileOffset == layer.tileOffset &&
                metallic == layer.metallic &&
                smoothness == layer.smoothness;
        }

        public void CopyTo(SplatPrototype layer)
        {
            layer.texture = Texture;
            layer.normalMap = NormalMap;
            layer.tileSize = TileSize;
            layer.tileOffset = TileOffset;
            layer.metallic = Metallic;
            layer.smoothness = Smoothness;
        }

        public static explicit operator GSplatPrototype(SplatPrototype layer)
        {
            GSplatPrototype proto = new GSplatPrototype();
            proto.Texture = layer.texture;
            proto.NormalMap = layer.normalMap;
            proto.TileSize = layer.tileSize;
            proto.TileOffset = layer.tileOffset;
            proto.Metallic = layer.metallic;
            proto.Smoothness = layer.smoothness;
            return proto;
        }

        public static explicit operator SplatPrototype(GSplatPrototype proto)
        {
            SplatPrototype layer = new SplatPrototype();
            layer.texture = proto.Texture;
            layer.normalMap = proto.NormalMap;
            layer.tileSize = proto.TileSize;
            layer.tileOffset = proto.TileOffset;
            layer.metallic = proto.Metallic;
            layer.smoothness = proto.Smoothness;
            return layer;
        }

#if !UNITY_2018_1 && !UNITY_2018_2
        public bool Equals(TerrainLayer layer)
        {
            return
                texture == layer.diffuseTexture &&
                normalMap == layer.normalMapTexture &&
                tileSize == layer.tileSize &&
                tileOffset == layer.tileOffset &&
                metallic == layer.metallic &&
                smoothness == layer.smoothness;
        }

        public void CopyTo(TerrainLayer layer)
        {
            layer.diffuseTexture = Texture;
            layer.normalMapTexture = NormalMap;
            layer.tileSize = TileSize;
            layer.tileOffset = TileOffset;
            layer.metallic = Metallic;
            layer.smoothness = Smoothness;
        }

        public static explicit operator GSplatPrototype(TerrainLayer layer)
        {
            GSplatPrototype proto = new GSplatPrototype();
            proto.Texture = layer.diffuseTexture;
            proto.NormalMap = layer.normalMapTexture;
            proto.TileSize = layer.tileSize;
            proto.TileOffset = layer.tileOffset;
            proto.Metallic = layer.metallic;
            proto.Smoothness = layer.smoothness;
            return proto;
        }

        public static explicit operator TerrainLayer(GSplatPrototype proto)
        {
            TerrainLayer layer = new TerrainLayer();
            layer.diffuseTexture = proto.Texture;
            layer.normalMapTexture = proto.NormalMap;
            layer.tileSize = proto.TileSize;
            layer.tileOffset = proto.TileOffset;
            layer.metallic = proto.Metallic;
            layer.smoothness = proto.Smoothness;
            return layer;
        }
#endif

#if POLARIS
        public static explicit operator GSplatPrototype(Pinwheel.Polaris.LPTSplatInfo layer)
        {
            GSplatPrototype proto = new GSplatPrototype();
            proto.Texture = layer.Texture;
            proto.NormalMap = layer.NormalMap;
            proto.TileSize = Vector2.one;
            proto.TileOffset = Vector2.zero;
            return proto;
        }
#endif
    }
}
#endif
