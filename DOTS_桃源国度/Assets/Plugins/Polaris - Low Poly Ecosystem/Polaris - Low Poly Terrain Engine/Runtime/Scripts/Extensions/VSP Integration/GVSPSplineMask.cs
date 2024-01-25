#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Griffin.SplineTool;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies;

namespace Pinwheel.Griffin.VegetationStudioPro
{
    [GDisplayName("VSP Spline Mask")]
    public class GVSPSplineMask : GSplineModifier
    {
        [SerializeField]
        private int nodeDensity;
        public int NodeDensity
        {
            get
            {
                return nodeDensity;
            }
            set
            {
                nodeDensity = Mathf.Max(1, value);
            }
        }

        public override void Apply()
        {
            VegetationMaskLine mask = gameObject.GetComponent<VegetationMaskLine>();
            if (mask == null)
            {
                mask = gameObject.AddComponent<VegetationMaskLine>();
                mask.ShowHandles = false;
                mask.GroundLayerMask = LayerMask.GetMask("Default");
            }

            mask.MaskName = "Spline Mask";

            List<Vector3> nodes = new List<Vector3>();
            List<float> widths = new List<float>();
            List<bool> actives = new List<bool>();

            float step = 1f / (NodeDensity + 1);
            float t = 0;
            float baseWidth = mask.LineWidth;
            int segmentCount = SplineCreator.Spline.Segments.Count;
            for (int i = 0; i < segmentCount; ++i)
            {
                nodes.Add(SplineCreator.Spline.EvaluatePosition(i, 0));
                widths.Add(baseWidth * MaxComponent(SplineCreator.Spline.EvaluateScale(i, 0)));
                actives.Add(true);

                for (int j = 1; j <= NodeDensity; ++j)
                {
                    t = j * step;
                    nodes.Add(SplineCreator.Spline.EvaluatePosition(i, t));
                    widths.Add(baseWidth * MaxComponent(SplineCreator.Spline.EvaluateScale(i, t)));
                    actives.Add(true);
                }

                nodes.Add(SplineCreator.Spline.EvaluatePosition(i, 1));
                widths.Add(baseWidth * MaxComponent(SplineCreator.Spline.EvaluateScale(i, 1)));
                actives.Add(false);

                if (i < segmentCount - 1)
                {
                    nodes.Add(SplineCreator.Spline.EvaluatePosition(i + 1, 0));
                    widths.Add(0);
                    actives.Add(false);
                }
            }

            for (int i = 0; i < nodes.Count; ++i)
            {
                nodes[i] = transform.TransformPoint(nodes[i]);
            }

            mask.ClearNodes();
            mask.AddNodesToEnd(nodes.ToArray(), widths.ToArray(), actives.ToArray());
            mask.UpdateVegetationMask();
        }

        private float MaxComponent(Vector3 v)
        {
            return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
        }
    }
}
#endif
