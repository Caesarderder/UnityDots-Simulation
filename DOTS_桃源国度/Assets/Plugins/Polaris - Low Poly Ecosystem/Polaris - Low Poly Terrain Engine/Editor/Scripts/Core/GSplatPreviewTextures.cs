#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public class GSplatPreviewTextures
    {
        public int Index { get; set; }
        public List<Texture> Textures { get; set; }
        public List<Texture2DArray> TextureArray { get; set; }

        public GSplatPreviewTextures(int index)
        {
            Index = index;
            Textures = new List<Texture>();
            TextureArray = new List<Texture2DArray>();
        }

        public int GetTextureCount()
        {
            int count = Textures.Count;
            for (int i = 0; i < TextureArray.Count; ++i)
            {
                if (TextureArray[i]!=null)
                {
                    count += TextureArray[i].depth;
                }
            }
            return count;
        }
    }
}
#endif
