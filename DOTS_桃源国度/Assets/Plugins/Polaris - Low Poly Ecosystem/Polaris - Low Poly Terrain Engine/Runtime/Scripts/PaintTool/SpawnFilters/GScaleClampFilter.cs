#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Clamp Scale")]
    public class GScaleClampFilter : GSpawnFilter
    {
        [SerializeField]
        private Vector3 minScale;
        public Vector3 MinScale
        {
            get
            {
                return minScale;
            }
            set
            {
                minScale = value;
            }
        }

        [SerializeField]
        private Vector3 maxScale;
        public Vector3 MaxScale
        {
            get
            {
                return maxScale;
            }
            set
            {
                maxScale = value;
            }
        }

        public override void Apply(ref GSpawnFilterArgs args)
        {
            Vector3 scale = new Vector3(
                Mathf.Clamp(args.Scale.x, MinScale.x, MaxScale.x),
                Mathf.Clamp(args.Scale.y, MinScale.y, MaxScale.y),
                Mathf.Clamp(args.Scale.z, MinScale.z, MaxScale.z));
            args.Scale = scale;
        }

        private void Reset()
        {
            MinScale = new Vector3(0.5f, 0.5f, 0.5f);
            MaxScale = new Vector3(2f, 2f, 2f);
        }
    }
}
#endif
