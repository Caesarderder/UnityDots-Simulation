#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Align To Surface")]
    public class GAlignToSurfaceFilter : GSpawnFilter
    {
        public override void Apply(ref GSpawnFilterArgs args)
        {
            Quaternion currentRotationY = Quaternion.Euler(0, args.Rotation.eulerAngles.y, 0);
            Quaternion rotationXZ = Quaternion.FromToRotation(Vector3.up, args.SurfaceNormal);
            args.Rotation = rotationXZ * currentRotationY;
        }
    }
}
#endif
