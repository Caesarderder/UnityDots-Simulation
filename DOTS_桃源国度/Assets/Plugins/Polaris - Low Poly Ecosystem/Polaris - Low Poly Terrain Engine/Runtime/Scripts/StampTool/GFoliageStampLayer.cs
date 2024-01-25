#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [System.Serializable]
    public class GFoliageStampLayer : GConditionalStampLayer
    {
        [SerializeField]
        private Color visualizeColor;
        public Color VisualizeColor
        {
            get
            {
                return visualizeColor;
            }
            set
            {
                visualizeColor = value;
            }
        }

        [SerializeField]
        private bool stampTrees;
        public bool StampTrees
        {
            get
            {
                return stampTrees;
            }
            set
            {
                stampTrees = value;
            }
        }

        [SerializeField]
        private List<int> treeIndices;
        public List<int> TreeIndices
        {
            get
            {
                if (treeIndices == null)
                {
                    treeIndices = new List<int>();
                }
                return treeIndices;
            }
            set
            {
                treeIndices = value;
            }
        }

        [SerializeField]
        private bool stampGrass;
        public bool StampGrasses
        {
            get
            {
                return stampGrass;
            }
            set
            {
                stampGrass = value;
            }
        }

        [SerializeField]
        private List<int> grassIndices;
        public List<int> GrassIndices
        {
            get
            {
                if (grassIndices == null)
                {
                    grassIndices = new List<int>();
                }
                return grassIndices;
            }
            set
            {
                grassIndices = value;
            }
        }

        [SerializeField]
        private int treeInstanceCount;
        public int TreeInstanceCount
        {
            get
            {
                return treeInstanceCount;
            }
            set
            {
                treeInstanceCount = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private int grassInstanceCount;
        public int GrassInstanceCount
        {
            get
            {
                return grassInstanceCount;
            }
            set
            {
                grassInstanceCount = value;
            }
        }

        [SerializeField]
        private float minRotation;
        public float MinRotation
        {
            get
            {
                return minRotation;
            }
            set
            {
                minRotation = value;
            }
        }

        [SerializeField]
        private float maxRotation;
        public float MaxRotation
        {
            get
            {
                return maxRotation;
            }
            set
            {
                maxRotation = value;
            }
        }

        [SerializeField]
        private Vector3 minScale;
        public Vector3 MinScale
        {
            get
            {
                return minScale;
            }
            set
            {
                minScale = value;
            }
        }

        [SerializeField]
        private Vector3 maxScale;
        public Vector3 MaxScale
        {
            get
            {
                return maxScale;
            }
            set
            {
                maxScale = value;
            }
        }

        public GFoliageStampLayer() : base() { }

        public static GFoliageStampLayer Create()
        {
            GFoliageStampLayer layer = new GFoliageStampLayer();
            layer.StampTrees = true;
            layer.TreeIndices = null;
            layer.StampGrasses = false;
            layer.GrassIndices = null;
            layer.TreeInstanceCount = 1000;

#if UNITY_EDITOR
            layer.VisualizeColor = GEditorSettings.Instance.stampTools.visualizeColor;
            layer.MinRotation = GEditorSettings.Instance.stampTools.minRotation;
            layer.MaxRotation = GEditorSettings.Instance.stampTools.maxRotation;
            layer.MinScale = GEditorSettings.Instance.stampTools.minScale;
            layer.MaxScale = GEditorSettings.Instance.stampTools.maxScale;
#endif

            layer.UpdateCurveTextures();
            return layer;
        }
    }
}
#endif
