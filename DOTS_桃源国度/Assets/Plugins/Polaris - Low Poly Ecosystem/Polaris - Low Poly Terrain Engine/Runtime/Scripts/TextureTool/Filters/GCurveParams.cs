#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [System.Serializable]
    public struct GCurveParams
    {
        [SerializeField]
        private AnimationCurve masterCurve;
        public AnimationCurve MasterCurve
        {
            get
            {
                if (masterCurve == null)
                {
                    masterCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }
                return masterCurve;
            }
            set
            {
                masterCurve = value;
            }
        }

        [SerializeField]
        private AnimationCurve redCurve;
        public AnimationCurve RedCurve
        {
            get
            {
                if (redCurve == null)
                {
                    redCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
                return redCurve;
            }
            set
            {
                redCurve = value;
            }
        }

        [SerializeField]
        private AnimationCurve greenCurve;
        public AnimationCurve GreenCurve
        {
            get
            {
                if (greenCurve == null)
                {
                    greenCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
                return greenCurve;
            }
            set
            {
                greenCurve = value;
            }
        }

        [SerializeField]
        private AnimationCurve blueCurve;
        public AnimationCurve BlueCurve
        {
            get
            {
                if (blueCurve == null)
                {
                    blueCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
                return blueCurve;
            }
            set
            {
                blueCurve = value;
            }
        }

        [SerializeField]
        private AnimationCurve alphaCurve;
        public AnimationCurve AlphaCurve
        {
            get
            {
                if (alphaCurve == null)
                {
                    alphaCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
                return alphaCurve;
            }
            set
            {
                alphaCurve = value;
            }
        }

        public static GCurveParams Create()
        {
            GCurveParams param = new GCurveParams();
            param.MasterCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            param.RedCurve = AnimationCurve.Linear(0, 0, 1, 1);
            param.GreenCurve = AnimationCurve.Linear(0, 0, 1, 1);
            param.BlueCurve = AnimationCurve.Linear(0, 0, 1, 1);
            param.AlphaCurve = AnimationCurve.Linear(0, 0, 1, 1);
            return param;
        }
    }
}
#endif
