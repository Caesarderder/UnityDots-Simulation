#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.SplineTool
{
    [System.Serializable]
    [AddComponentMenu("")]
    [RequireComponent(typeof(GSplineCreator))]
    public abstract class GSplineModifier : MonoBehaviour
    {
        [SerializeField]
        private GSplineCreator splineCreator;
        public GSplineCreator SplineCreator
        {
            get
            {
                return splineCreator;
            }
            set
            {
                splineCreator = value;
            }
        }

        public abstract void Apply();
    }
}
#endif
