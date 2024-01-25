#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    public interface IGTexturePainterWithLivePreview
    {
        void Editor_DrawLivePreview(GStylizedTerrain terrain, GTexturePainterArgs args, Camera cam);
    }
}
#endif
