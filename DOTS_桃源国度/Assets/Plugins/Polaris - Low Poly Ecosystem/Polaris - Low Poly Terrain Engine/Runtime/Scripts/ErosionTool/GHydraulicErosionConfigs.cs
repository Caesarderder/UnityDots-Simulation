#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.ErosionTool
{
    [System.Serializable]
    public class GHydraulicErosionConfigs
    {
        [SerializeField]
        private float waterSourceAmount;
        public float WaterSourceAmount
        {
            get
            {
                return waterSourceAmount;
            }
            set
            {
                waterSourceAmount = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve waterSourceOverTime;
        public AnimationCurve WaterSourceOverTime
        {
            get
            {
                if (waterSourceOverTime == null)
                {
                    waterSourceOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return waterSourceOverTime;
            }
            set
            {
                waterSourceOverTime = value;
            }
        }

        [SerializeField]
        private float waterSourceMultiplier;
        public float WaterSourceMultiplier
        {
            get
            {
                return waterSourceMultiplier;
            }
            set
            {
                waterSourceMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float rainRate;
        public float RainRate
        {
            get
            {
                return rainRate;
            }
            set
            {
                rainRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve rainOverTime;
        public AnimationCurve RainOverTime
        {
            get
            {
                if (rainOverTime == null)
                {
                    rainOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return rainOverTime;
            }
            set
            {
                rainOverTime = value;
            }
        }

        [SerializeField]
        private float rainMultiplier;
        public float RainMultiplier
        {
            get
            {
                return rainMultiplier;
            }
            set
            {
                rainMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float flowRate;
        public float FlowRate
        {
            get
            {
                return flowRate;
            }
            set
            {
                flowRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve flowOverTime;
        public AnimationCurve FlowOverTime
        {
            get
            {
                if (flowOverTime == null)
                {
                    flowOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return flowOverTime;
            }
            set
            {
                flowOverTime = value;
            }
        }

        [SerializeField]
        private float flowMultiplier;
        public float FlowMultiplier
        {
            get
            {
                return flowMultiplier;
            }
            set
            {
                flowMultiplier = Mathf.Max(0, value);
            }
        }

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
        private float depositionRate;
        public float DepositionRate
        {
            get
            {
                return depositionRate;
            }
            set
            {
                depositionRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve depositionOverTime;
        public AnimationCurve DepositionOverTime
        {
            get
            {
                if (depositionOverTime == null)
                {
                    depositionOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return depositionOverTime;
            }
            set
            {
                depositionOverTime = value;
            }
        }

        [SerializeField]
        private float depositionMultiplier;
        public float DepositionMultiplier
        {
            get
            {
                return depositionMultiplier;
            }
            set
            {
                depositionMultiplier = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float evaporationRate;
        public float EvaporationRate
        {
            get
            {
                return evaporationRate;
            }
            set
            {
                evaporationRate = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private AnimationCurve evaporationOverTime;
        public AnimationCurve EvaporationOverTime
        {
            get
            {
                if (evaporationOverTime == null)
                {
                    evaporationOverTime = AnimationCurve.Linear(0, 1, 1, 1);
                }
                return evaporationOverTime;
            }
            set
            {
                evaporationOverTime = value;
            }
        }

        [SerializeField]
        private float evaporationMultiplier;
        public float EvaporationMultiplier
        {
            get
            {
                return evaporationMultiplier;
            }
            set
            {
                evaporationMultiplier = Mathf.Max(0, value);
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
                iterationCount = Mathf.Max(0, value);
            }
        }

        public GHydraulicErosionConfigs()
        {
            waterSourceAmount = 0;
            waterSourceOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            waterSourceMultiplier = 1;

            rainRate = 0.075f;
            rainOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            rainMultiplier = 1;

            flowRate = 1;
            flowOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            flowMultiplier = 1;

            erosionRate = 0.3f;
            erosionOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            erosionMultiplier = 1;

            depositionRate = 0.075f;
            depositionOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            depositionMultiplier = 1;

            evaporationRate = 0.03f;
            evaporationOverTime = AnimationCurve.Linear(0, 1, 1, 1);
            evaporationMultiplier = 1;

            iterationCount = 500;
        }
    }
}
#endif
