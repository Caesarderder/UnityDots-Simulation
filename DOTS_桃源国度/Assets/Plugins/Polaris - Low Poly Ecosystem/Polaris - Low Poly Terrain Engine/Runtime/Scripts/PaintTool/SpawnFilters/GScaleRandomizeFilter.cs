#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Randomize Scale")]
    public class GScaleRandomizeFilter : GSpawnFilter
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
            Vector3 scale = Vector3.Lerp(MinScale, MaxScale, Random.value);
            args.Scale = scale;
        }

        private void Reset()
        {
            MinScale = new Vector3(0.7f, 0.8f, 0.7f);
            MaxScale = new Vector3(1.2f, 1.5f, 1.2f);
        }
    }
}
#endif
