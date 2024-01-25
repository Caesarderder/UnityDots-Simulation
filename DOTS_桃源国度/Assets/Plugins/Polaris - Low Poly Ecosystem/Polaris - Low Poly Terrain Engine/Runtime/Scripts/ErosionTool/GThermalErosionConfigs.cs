#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.ErosionTool
{
    [System.Serializable]
    public class GThermalErosionConfigs
    {
        [SerializeField]
        private float erosionRate;
        public float ErosionRate
        {
            get
            {
                return erosionRate;
            }
            set
            {
                erosionRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve erosionOverTime;
        public AnimationCurve ErosionOverTime
        {
            get
            {
                if (erosionOverTime == null)
                {
                    erosionOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return erosionOverTime;
            }
            set
            {
                erosionOverTime = value;
            }
        }

        [SerializeField]
        private float erosionMultiplier;
        public float ErosionMultiplier
        {
            get
            {
                return erosionMultiplier;
            }
            set
            {
                erosionMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float restingAngle;
        public float RestingAngle
        {
            get
            {
                return restingAngle;
            }
            set
            {
                restingAngle = Mathf.Clamp(value, 0f, 90f);
            }
        }

        [SerializeField]
        private AnimationCurve restingAngleOverTime;
        public AnimationCurve RestingAngleOverTime
        {
            get
            {
                if (restingAngleOverTime == null)
                {
                    restingAngleOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return restingAngleOverTime;
            }
            set
            {
                restingAngleOverTime = value;
            }
        }

        [SerializeField]
        private float restingAngleMultiplier;
        public float RestingAngleMultiplier
        {
            get
            {
                return restingAngleMultiplier;
            }
            set
            {
                restingAngleMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int iterationCount;
        public int IterationCount
        {
            get
            {
                return iterationCount;
            }
            set
            {
                iterationCount = value;
            }
        }

        public GThermalErosionConfigs()
        {
            erosionRate = 0.1f;
            erosionOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            erosionMultiplier = 1f;

            restingAngle = 30f;
            restingAngleOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            restingAngleMultiplier = 1f;

            iterationCount = 10;
        }
    }
}
#endif
