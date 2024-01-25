#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [GDisplayName("Randomize Rotation")]
    public class GRotationRandomizeFilter : GSpawnFilter
    {
        [SerializeField]
        private float minAngle;
        public float MinAngle
        {
            get
            {
                return minAngle;
            }
            set
            {
                minAngle = value;
            }
        }

        [SerializeField]
        private float maxAngle;
        public float MaxAngle
        {
            get
            {
                return maxAngle;
            }
            set
            {
                maxAngle = value;
            }
        }

        public override void Apply(ref GSpawnFilterArgs args)
        {
            args.Rotation = Quaternion.Euler(0, Random.Range(MinAngle, MaxAngle), 0);
        }

        private void Reset()
        {
            MinAngle = 0;
            MaxAngle = 360;
        }
    }
}
#endif
