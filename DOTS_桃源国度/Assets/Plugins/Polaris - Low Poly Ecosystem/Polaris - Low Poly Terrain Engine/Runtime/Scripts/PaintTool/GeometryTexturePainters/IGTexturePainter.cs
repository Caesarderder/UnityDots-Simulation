#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    public interface IGTexturePainter
    {
        string HistoryPrefix { get; }
        string Instruction { get; }
        void BeginPainting(GStylizedTerrain terrain, GTexturePainterArgs args);
        void EndPainting(GStylizedTerrain terrain, GTexturePainterArgs args);
        List<GTerrainResourceFlag> GetResourceFlagForHistory(GTexturePainterArgs args);
    }
}
#endif
