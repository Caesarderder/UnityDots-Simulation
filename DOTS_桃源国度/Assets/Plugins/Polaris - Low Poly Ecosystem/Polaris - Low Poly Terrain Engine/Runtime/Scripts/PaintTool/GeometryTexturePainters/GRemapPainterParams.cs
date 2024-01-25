#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    public struct GRemapPainterParams
    {
        [SerializeField]
        private AnimationCurve curve;
        public AnimationCurve Curve
        {
            get
            {
                if (curve == null)
                {
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }
                return curve;
            }
            set
            {
                curve = value;
            }
        }
    }
}
#endif
