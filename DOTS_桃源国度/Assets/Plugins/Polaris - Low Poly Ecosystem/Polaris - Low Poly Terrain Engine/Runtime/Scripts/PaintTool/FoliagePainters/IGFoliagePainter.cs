#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    public interface IGFoliagePainter
    {
        string HistoryPrefix { get; }
        string Instruction { get; }
        List<System.Type> SuitableFilterTypes { get; }
        void Paint(GStylizedTerrain terrain, GFoliagePainterArgs args);
        List<GTerrainResourceFlag> GetResourceFlagForHistory(GFoliagePainterArgs args);
    }
}
#endif
