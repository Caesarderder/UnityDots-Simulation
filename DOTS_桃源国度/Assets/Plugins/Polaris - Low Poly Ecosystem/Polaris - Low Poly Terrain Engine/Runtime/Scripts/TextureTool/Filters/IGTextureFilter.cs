#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    public interface IGTextureFilter
    {
        void Apply(RenderTexture targetRt, GTextureFilterParams param);
    }
}
#endif
