#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [System.Serializable]
    public struct GRenderingOverride
    {
        [SerializeField]
        private bool overrideCastShadow;
        public bool OverrideCastShadow
        {
            get
            {
                return overrideCastShadow;
            }
            set
            {
                overrideCastShadow = value;
            }
        }

        [SerializeField]
        private bool castShadow;
        public bool CastShadow
        {
            get
            {
                return castShadow;
            }
            set
            {
                castShadow = value;
            }
        }

        [SerializeField]
        private bool overrideReceiveShadow;
        public bool OverrideReceiveShadow
        {
            get
            {
                return overrideReceiveShadow;
            }
            set
            {
                overrideReceiveShadow = value;
            }
        }

        [SerializeField]
        private bool receiveShadow;
        public bool ReceiveShadow
        {
            get
            {
                return receiveShadow;
            }
            set
            {
                receiveShadow = value;
            }
        }

        [SerializeField]
        private bool overrideDrawTrees;
        public bool OverrideDrawTrees
        {
            get
            {
                return overrideDrawTrees;
            }
            set
            {
                overrideDrawTrees = value;
            }
        }

        [SerializeField]
        private bool drawTrees;
        public bool DrawTrees
        {
            get
            {
                return drawTrees;
            }
            set
            {
                drawTrees = value;
            }
        }

        [SerializeField]
        private bool overrideEnableInstancing;
        public bool OverrideEnableInstancing
        {
            get
            {
                return overrideEnableInstancing;
            }
            set
            {
                overrideEnableInstancing = value;
            }
        }

        [SerializeField]
        private bool enableInstancing;
        public bool EnableInstancing
        {
            get
            {
                return enableInstancing;
            }
            set
            {
                enableInstancing = value;
            }
        }

        [SerializeField]
        private bool overrideBillboardStart;
        public bool OverrideBillboardStart
        {
            get
            {
                return overrideBillboardStart;
            }
            set
            {
                overrideBillboardStart = value;
            }
        }

        [SerializeField]
        private float billboardStart;
        public float BillboardStart
        {
            get
            {
                return billboardStart;
            }
            set
            {
                billboardStart = Mathf.Clamp(value, 0, GCommon.MAX_TREE_DISTANCE);
            }
        }

        [SerializeField]
        private bool overrideTreeDistance;
        public bool OverrideTreeDistance
        {
            get
            {
                return overrideTreeDistance;
            }
            set
            {
                overrideTreeDistance = value;
            }
        }

        [SerializeField]
        private float treeDistance;
        public float TreeDistance
        {
            get
            {
                return treeDistance;
            }
            set
            {
                treeDistance = Mathf.Clamp(value, 0, GCommon.MAX_TREE_DISTANCE);
            }
        }

        [SerializeField]
        private bool overrideDrawGrasses;
        public bool OverrideDrawGrasses
        {
            get
            {
                return overrideDrawGrasses;
            }
            set
            {
                overrideDrawGrasses = value;
            }
        }

        [SerializeField]
        private bool drawGrasses;
        public bool DrawGrasses
        {
            get
            {
                return drawGrasses;
            }
            set
            {
                drawGrasses = value;
            }
        }

        [SerializeField]
        private bool overrideGrassDistance;
        public bool OverrideGrassDistance
        {
            get
            {
                return overrideGrassDistance;
            }
            set
            {
                overrideGrassDistance = value;
            }
        }

        [SerializeField]
        private float grassDistance;
        public float GrassDistance
        {
            get
            {
                return grassDistance;
            }
            set
            {
                grassDistance = Mathf.Clamp(value, 0, GCommon.MAX_GRASS_DISTANCE);
            }
        }

        [SerializeField]
        private bool overrideGrassFadeStart;
        public bool OverrideGrassFadeStart
        {
            get
            {
                return overrideGrassFadeStart;
            }
            set
            {
                overrideGrassFadeStart = value;
            }
        }

        [SerializeField]
        private float grassFadeStart;
        public float GrassFadeStart
        {
            get
            {
                return grassFadeStart;
            }
            set
            {
                grassFadeStart = Mathf.Clamp(value, 0f, 1f);
            }
        }

        public void Reset()
        {
            OverrideCastShadow = false;
            OverrideReceiveShadow = false;
            OverrideDrawTrees = false;
            OverrideEnableInstancing = false;
            OverrideBillboardStart = false;
            OverrideTreeDistance = false;
            OverrideDrawGrasses = false;
            OverrideGrassDistance = false;
            OverrideGrassFadeStart = false;

            CastShadow = GRuntimeSettings.Instance.renderingDefault.terrainCastShadow;
            ReceiveShadow = GRuntimeSettings.Instance.renderingDefault.terrainReceiveShadow;
            DrawTrees = GRuntimeSettings.Instance.renderingDefault.drawTrees;
            EnableInstancing = GRuntimeSettings.Instance.renderingDefault.enableInstancing;
            BillboardStart = GRuntimeSettings.Instance.renderingDefault.billboardStart;
            TreeDistance = GRuntimeSettings.Instance.renderingDefault.treeDistance;
            DrawGrasses = GRuntimeSettings.Instance.renderingDefault.drawGrasses;
            GrassDistance = GRuntimeSettings.Instance.renderingDefault.grassDistance;
            GrassFadeStart = GRuntimeSettings.Instance.renderingDefault.grassFadeStart;
        }

        public void Override(GRendering r)
        {
            if (OverrideCastShadow)
                r.CastShadow = CastShadow;
            if (OverrideReceiveShadow)
                r.ReceiveShadow = ReceiveShadow;
            if (OverrideDrawTrees)
                r.DrawTrees = DrawTrees;
            if (OverrideEnableInstancing)
                r.EnableInstancing = EnableInstancing;
            if (OverrideBillboardStart)
                r.BillboardStart = BillboardStart;
            if (OverrideTreeDistance)
                r.TreeDistance = TreeDistance;
            if (OverrideDrawGrasses)
                r.DrawGrasses = DrawGrasses;
            if (OverrideGrassDistance)
                r.GrassDistance = GrassDistance;
            if (OverrideGrassFadeStart)
                r.GrassFadeStart = GrassFadeStart;
        }
    }
}
#endif
