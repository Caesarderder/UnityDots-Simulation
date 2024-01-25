#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public interface IGTextureGenerator
    {
        void Generate(RenderTexture targetRt);
    }
}
#endif
