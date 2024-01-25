#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    public struct GSpawnFilterArgs
    {
        public GStylizedTerrain Terrain { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Color Color { get; set; }
        public Vector3 SurfaceNormal { get; set; }
        public Vector2 SurfaceTexcoord { get; set; }
        public bool ShouldExclude { get; set; }

        public static GSpawnFilterArgs Create()
        {
            GSpawnFilterArgs args = new GSpawnFilterArgs();
            args.Position = Vector3.zero;
            args.Rotation = Quaternion.identity;
            args.Scale = Vector3.one;
            args.Color = Color.white;
            args.SurfaceNormal = Vector3.up;
            args.SurfaceTexcoord = Vector2.zero;
            args.ShouldExclude = false;
            return args;
        }
    }
}
#endif
