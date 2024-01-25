#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Security.Cryptography;

namespace Pinwheel.Griffin
{
    public class GSelectionGridPreviews
    {
        private List<Color> colors;
        public List<Color> Colors
        {
            get
            {
                if (colors == null)
                {
                    colors = new List<Color>();
                }
                return colors;
            }
        }

        private List<Texture> textures;
        public List<Texture> Textures
        {
            get
            {
                if (textures == null)
                {
                    textures = new List<Texture>();
                }
                return textures;
            }
        }

        public GSelectionGridPreviews()
        {
            colors = new List<Color>();
            textures = new List<Texture>();
        }

        public void Add(GTreePrototype p)
        {
            if (p == null)
            {
                return;
            }
            if (p.Prefab == null)
            {
                return;
            }
            Texture t = AssetPreview.GetAssetPreview(p.Prefab);
            if (t != null)
            {
                Textures.Add(t);
                Colors.Add(Color.white);
            }
        }

        public void Add(GGrassPrototype p)
        {
            if (p == null)
            {
                return;
            }
            if (p.Shape == GGrassShape.DetailObject)
            {
                if (p.Prefab == null)
                {
                    return;
                }
                Texture t = AssetPreview.GetAssetPreview(p.Prefab);
                if (t != null)
                {
                    Textures.Add(t);
                    Colors.Add(Color.white);
                }
            }
            else
            {
                if (p.Texture == null)
                    return;
                Textures.Add(p.Texture);
                Colors.Add(p.Color);
            }
        }

        public struct ColorTexturePair : IEquatable<ColorTexturePair>
        {
            public Color color;
            public Texture texture;

            public ColorTexturePair(Color c, Texture t)
            {
                color = c;
                texture = t;
            }

            public bool Equals(ColorTexturePair other)
            {
                return color == other.color && texture == other.texture;
            }
        }
    }
}
#endif
