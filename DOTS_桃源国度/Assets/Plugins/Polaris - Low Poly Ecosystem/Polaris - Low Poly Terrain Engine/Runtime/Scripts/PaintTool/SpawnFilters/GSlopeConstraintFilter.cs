#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Slope Constraint")]
    public class GSlopeConstraintFilter : GSpawnFilter
    {
        [SerializeField]
        [Range(0f, 90f)]
        private float minSlopeAngle;
        public float MinSlopeAngle
        {
            get
            {
                return minSlopeAngle;
            }
            set
            {
                minSlopeAngle = Mathf.Clamp(value, 0f, 90f);
            }
        }

        [SerializeField]
        [Range(0f, 90f)]
        private float maxSlopeAngle;
        public float MaxSlopeAngle
        {
            get
            {
                return maxSlopeAngle;
            }
            set
            {
                maxSlopeAngle = Mathf.Clamp(value, 0f, 90f);
            }
        }

        public override void Apply(ref GSpawnFilterArgs args)
        {
            float angle = Vector3.Angle(args.SurfaceNormal, Vector3.up);
            args.ShouldExclude = angle < MinSlopeAngle || angle > MaxSlopeAngle;
        }

        private void Reset()
        {
            MinSlopeAngle = 0;
            MaxSlopeAngle = 90;
        }
    }
}
#endif
