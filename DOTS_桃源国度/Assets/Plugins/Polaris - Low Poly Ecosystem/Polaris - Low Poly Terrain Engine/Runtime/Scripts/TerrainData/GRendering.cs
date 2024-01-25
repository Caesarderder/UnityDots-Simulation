#if GRIFFIN
using UnityEngine;
using UnityEngine.Serialization;

namespace Pinwheel.Griffin
{
    public class GRendering : ScriptableObject
    {
        [SerializeField]
        private GTerrainData terrainData;
        public GTerrainData TerrainData
        {
            get
            {
                return terrainData;
            }
            internal set
            {
                terrainData = value;
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

        [FormerlySerializedAs("drawFoliage")]
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
        private bool drawGrasses = true;
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
        private bool enableInstancing;
        public bool EnableInstancing
        {
            get
            {
                if (!SystemInfo.supportsInstancing)
                    enableInstancing = false;
                return enableInstancing;
            }
            set
            {
                if (SystemInfo.supportsInstancing)
                {
                    enableInstancing = value;
                }
                else
                {
                    enableInstancing = false;
                }
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
        private float grassFadeStart;
        public float GrassFadeStart
        {
            get
            {
                return grassFadeStart;
            }
            set
            {
                grassFadeStart = Mathf.Clamp01(value);
            }
        }

        public void Reset()
        {
            name = "Rendering";
            CastShadow = GRuntimeSettings.Instance.renderingDefault.terrainCastShadow;
            ReceiveShadow = GRuntimeSettings.Instance.renderingDefault.terrainReceiveShadow;
            DrawTrees = GRuntimeSettings.Instance.renderingDefault.drawTrees;
            DrawGrasses = GRuntimeSettings.Instance.renderingDefault.drawGrasses;
            EnableInstancing = GRuntimeSettings.Instance.renderingDefault.enableInstancing;
            BillboardStart = GRuntimeSettings.Instance.renderingDefault.billboardStart;
            TreeDistance = GRuntimeSettings.Instance.renderingDefault.treeDistance;
            GrassDistance = GRuntimeSettings.Instance.renderingDefault.grassDistance;
            GrassFadeStart = GRuntimeSettings.Instance.renderingDefault.grassFadeStart;
        }

        public void ResetFull()
        {
            Reset();
        }

        public void CopyTo(GRendering des)
        {
            des.CastShadow = CastShadow;
            des.ReceiveShadow = ReceiveShadow;
            des.DrawTrees = DrawTrees;
            des.EnableInstancing = EnableInstancing;
            des.BillboardStart = BillboardStart;
            des.TreeDistance = TreeDistance;
            des.GrassDistance = GrassDistance;
            des.GrassFadeStart = GrassFadeStart;
        }
    }
}
#endif
