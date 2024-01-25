#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    public struct GTerracePainterParams
    {
        [SerializeField]
        private int stepCount;
        public int StepCount
        {
            get
            {
                return stepCount;
            }
            set
            {
                stepCount = Mathf.Max(1, value);
            }
        }
    }
}
#endif
