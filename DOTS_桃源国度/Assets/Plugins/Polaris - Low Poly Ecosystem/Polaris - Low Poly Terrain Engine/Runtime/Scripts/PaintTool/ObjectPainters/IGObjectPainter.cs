#if GRIFFIN
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    public interface IGObjectPainter
    {
        string Instruction { get; }
        List<System.Type> SuitableFilterTypes { get; }
        void Paint(GStylizedTerrain terrain, GObjectPainterArgs args);
    }
}
#endif
