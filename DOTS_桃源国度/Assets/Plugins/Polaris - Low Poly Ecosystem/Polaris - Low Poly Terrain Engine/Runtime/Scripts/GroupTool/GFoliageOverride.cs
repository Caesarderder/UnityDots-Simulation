#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [System.Serializable]
    public struct GFoliageOverride
    {
        [SerializeField]
        private bool overrideTrees;
        public bool OverrideTrees
        {
            get
            {
                return overrideTrees;
            }
            set
            {
                overrideTrees = value;
            }
        }

        [SerializeField]
        private GTreePrototypeGroup trees;
        public GTreePrototypeGroup Trees
        {
            get
            {
                return trees;
            }
            set
            {
                trees = value;
            }
        }

        [SerializeField]
        private bool overrideGrasses;
        public bool OverrideGrasses
        {
            get
            {
                return overrideGrasses;
            }
            set
            {
                overrideGrasses = value;
            }
        }

        [SerializeField]
        private GGrassPrototypeGroup grasses;
        public GGrassPrototypeGroup Grasses
        {
            get
            {
                return grasses;
            }
            set
            {
                grasses = value;
            }
        }

        [SerializeField]
        private bool overridePatchGridSize;
        public bool OverridePatchGridSize
        {
            get
            {
                return overridePatchGridSize;
            }
            set
            {
                overridePatchGridSize = value;
            }
        }

        [SerializeField]
        private int patchGridSize;
        public int PatchGridSize
        {
            get
            {
                return patchGridSize;
            }
            set
            {
                patchGridSize = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private bool overrideTreeSnapMode;
        public bool OverrideTreeSnapMode
        {
            get
            {
                return overrideTreeSnapMode;
            }
            set
            {
                overrideTreeSnapMode = value;
            }
        }

        [SerializeField]
        private GSnapMode treeSnapMode;
        public GSnapMode TreeSnapMode
        {
            get
            {
                return treeSnapMode;
            }
            set
            {
                treeSnapMode = value;
            }
        }

        [SerializeField]
        private bool overrideTreeSnapLayerMask;
        public bool OverrideTreeSnapLayerMask
        {
            get
            {
                return overrideTreeSnapLayerMask;
            }
            set
            {
                overrideTreeSnapLayerMask = value;
            }
        }

        [SerializeField]
        private LayerMask treeSnapLayerMask;
        public LayerMask TreeSnapLayerMask
        {
            get
            {
                return treeSnapLayerMask;
            }
            set
            {
                treeSnapLayerMask = value;
            }
        }

        [SerializeField]
        private bool overrideGrassSnapMode;
        public bool OverrideGrassSnapMode
        {
            get
            {
                return overrideGrassSnapMode;
            }
            set
            {
                overrideGrassSnapMode = value;
            }
        }

        [SerializeField]
        private GSnapMode grassSnapMode;
        public GSnapMode GrassSnapMode
        {
            get
            {
                return grassSnapMode;
            }
            set
            {
                grassSnapMode = value;
            }
        }

        [SerializeField]
        private bool overrideGrassSnapLayerMask;
        public bool OverrideGrassSnapLayerMask
        {
            get
            {
                return overrideGrassSnapLayerMask;
            }
            set
            {
                overrideGrassSnapLayerMask = value;
            }
        }

        [SerializeField]
        private LayerMask grassSnapLayerMask;
        public LayerMask GrassSnapLayerMask
        {
            get
            {
                return grassSnapLayerMask;
            }
            set
            {
                grassSnapLayerMask = value;
            }
        }

        [SerializeField]
        private bool overrideEnableInteractiveGrass;
        public bool OverrideEnableInteractiveGrass
        {
            get
            {
                return overrideEnableInteractiveGrass;
            }
            set
            {
                overrideEnableInteractiveGrass = value;
            }
        }

        [SerializeField]
        private bool enableInteractiveGrass;
        public bool EnableInteractiveGrass
        {
            get
            {
                return enableInteractiveGrass;
            }
            set
            {
                enableInteractiveGrass = value;
            }
        }

        [SerializeField]
        private bool overrideVectorFieldMapResolution;
        public bool OverrideVectorFieldMapResolution
        {
            get
            {
                return overrideVectorFieldMapResolution;
            }
            set
            {
                overrideVectorFieldMapResolution = value;
            }
        }

        [SerializeField]
        private int vectorFieldMapResolution;
        public int VectorFieldMapResolution
        {
            get
            {
                return vectorFieldMapResolution;
            }
            set
            {
                vectorFieldMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
            }
        }

        [SerializeField]
        private bool overrideBendSensitive;
        public bool OverrideBendSensitive
        {
            get
            {
                return overrideBendSensitive;
            }
            set
            {
                overrideBendSensitive = value;
            }
        }

        [SerializeField]
        private float bendSensitive;
        public float BendSensitive
        {
            get
            {
                return bendSensitive;
            }
            set
            {
                bendSensitive = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private bool overrideRestoreSensitive;
        public bool OverrideRestoreSensitive
        {
            get
            {
                return overrideRestoreSensitive;
            }
            set
            {
                overrideRestoreSensitive = value;
            }
        }

        [SerializeField]
        private float restoreSensitive;
        public float RestoreSensitive
        {
            get
            {
                return restoreSensitive;
            }
            set
            {
                restoreSensitive = Mathf.Clamp01(value);
            }
        }

        public void Reset()
        {
            OverrideTrees = false;
            OverrideTreeSnapMode = false;
            OverrideTreeSnapLayerMask = false;
            OverrideGrasses = false;
            OverrideGrassSnapMode = false;
            OverrideGrassSnapLayerMask = false;
            OverridePatchGridSize = false;
            OverrideEnableInteractiveGrass = false;
            OverrideVectorFieldMapResolution = false;
            OverrideBendSensitive = false;
            OverrideRestoreSensitive = false;

            TreeSnapMode = GRuntimeSettings.Instance.foliageDefault.treeSnapMode;
            TreeSnapLayerMask = GRuntimeSettings.Instance.foliageDefault.treeSnapLayerMask;
            GrassSnapMode = GRuntimeSettings.Instance.foliageDefault.grassSnapMode;
            GrassSnapLayerMask = GRuntimeSettings.Instance.foliageDefault.grassSnapLayerMask;
            PatchGridSize = GRuntimeSettings.Instance.foliageDefault.patchGridSize;
            EnableInteractiveGrass = GRuntimeSettings.Instance.foliageDefault.enableInteractiveGrass;
            VectorFieldMapResolution = GRuntimeSettings.Instance.foliageDefault.vectorFieldMapResolution;
            BendSensitive = GRuntimeSettings.Instance.foliageDefault.bendSensitive;
            RestoreSensitive = GRuntimeSettings.Instance.foliageDefault.restoreSensitive;
        }

        public void Override(GFoliage s)
        {
            if (OverrideTrees)
                s.Trees = Trees;
            if (OverrideTreeSnapMode)
                s.TreeSnapMode = TreeSnapMode;
            if (OverrideTreeSnapLayerMask)
                s.TreeSnapLayerMask = TreeSnapLayerMask;

            if (OverrideGrasses)
                s.Grasses = Grasses;
            if (OverrideGrassSnapMode)
                s.GrassSnapMode = GrassSnapMode;
            if (OverrideGrassSnapLayerMask)
                s.GrassSnapLayerMask = GrassSnapLayerMask;
            if (OverridePatchGridSize)
                s.PatchGridSize = PatchGridSize;
            if (OverrideEnableInteractiveGrass)
                s.EnableInteractiveGrass = EnableInteractiveGrass;
            if (OverrideVectorFieldMapResolution)
                s.VectorFieldMapResolution = VectorFieldMapResolution;
            if (OverrideBendSensitive)
                s.BendSensitive = BendSensitive;
            if (OverrideRestoreSensitive)
                s.RestoreSensitive = RestoreSensitive;

            s.Refresh();
        }
    }
}
#endif
