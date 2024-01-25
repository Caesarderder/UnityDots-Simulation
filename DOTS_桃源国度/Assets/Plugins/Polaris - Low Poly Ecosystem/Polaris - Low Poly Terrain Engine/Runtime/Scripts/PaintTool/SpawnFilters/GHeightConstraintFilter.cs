#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Height Constraint")]
    public class GHeightConstraintFilter : GSpawnFilter
    {
        [SerializeField]
        private Space space;
        public Space Space
        {
            get
            {
                return space;
            }
            set
            {
                space = value;
            }
        }

        [SerializeField]
        private float minHeight;
        public float MinHeight
        {
            get
            {
                return minHeight;
            }
            set
            {
                minHeight = value;
            }
        }

        [SerializeField]
        private float maxHeight;
        public float MaxHeight
        {
            get
            {
                return maxHeight;
            }
            set
            {
                maxHeight = value;
            }
        }

        public override void Apply(ref GSpawnFilterArgs args)
        {
            float h = 0;
            if (Space == Space.World)
            {
                h = args.Position.y;
            }
            else
            {
                h = args.Terrain.transform.InverseTransformPoint(args.Position).y;
            }

            args.ShouldExclude = h < MinHeight || h > MaxHeight;
        }

        private void Reset()
        {
            Space = Space.Self;
            MinHeight = 0;
            MaxHeight = 1000;
        }
    }
}
#endif
