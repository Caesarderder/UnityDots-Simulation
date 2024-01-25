#if GRIFFIN
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin
{
    [System.Serializable]
    public class GGrassPrototype
    {
        [SerializeField]
        private Texture2D texture;
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        [SerializeField]
        private GameObject prefab;
        public GameObject Prefab
        {
            get
            {
                return prefab;
            }
            set
            {
                prefab = value;
                RefreshDetailObjectSettings();
            }
        }

        [SerializeField]
        internal Vector3 size;
        public Vector3 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        [SerializeField]
        internal int layer;
        public int Layer
        {
            get
            {
                return layer;
            }
            set
            {
                layer = value;
            }
        }

        [SerializeField]
        private GGrassShape shape;
        public GGrassShape Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;
            }
        }

        [SerializeField]
        private Mesh customMesh;
        public Mesh CustomMesh
        {
            get
            {
                return customMesh;
            }
            set
            {
                customMesh = value;
            }
        }

        [SerializeField]
        private Mesh detailMesh;
        public Mesh DetailMesh
        {
            get
            {
                return detailMesh;
            }
            private set
            {
                detailMesh = value;
            }
        }

        [SerializeField]
        private Material detailMaterial;
        public Material DetailMaterial
        {
            get
            {
                return detailMaterial;
            }
            private set
            {
                detailMaterial = value;
            }
        }

        [SerializeField]
        internal ShadowCastingMode shadowCastingMode;
        public ShadowCastingMode ShadowCastingMode
        {
            get
            {
                return shadowCastingMode;
            }
            set
            {
                shadowCastingMode = value;
            }
        }

        [SerializeField]
        internal bool receiveShadow;
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
        private bool alignToSurface;
        public bool AlignToSurface
        {
            get
            {
                return alignToSurface;
            }
            set
            {
                alignToSurface = value;
            }
        }

        [SerializeField]
        internal float pivotOffset;
        public float PivotOffset
        {
            get
            {
                return pivotOffset;
            }
            set
            {
                pivotOffset = Mathf.Clamp(value, -1, 1);
            }
        }

        [SerializeField]
        private float bendFactor = 1;
        public float BendFactor
        {
            get
            {
                return bendFactor;
            }
            set
            {
                bendFactor = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private Color color = Color.white;
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        [SerializeField]
        private bool isBillboard;
        public bool IsBillboard
        {
            get
            {
                return isBillboard;
            }
            set
            {
                isBillboard = value;
            }
        }

        public static GGrassPrototype Create(Texture2D tex)
        {
            GGrassPrototype prototype = new GGrassPrototype();
            prototype.Shape = GGrassShape.Quad;
            prototype.Texture = tex;
            prototype.ShadowCastingMode = ShadowCastingMode.On;
            prototype.ReceiveShadow = true;
            prototype.Size = Vector3.one;
            prototype.Layer = LayerMask.NameToLayer("Default");
            prototype.AlignToSurface = false;
            prototype.PivotOffset = 0;
            prototype.BendFactor = 1;
            prototype.Color = Color.white;
            return prototype;
        }

        public static GGrassPrototype Create(GameObject prefab)
        {
            GGrassPrototype prototype = new GGrassPrototype();
            prototype.Shape = GGrassShape.DetailObject;
            prototype.Prefab = prefab;
            prototype.Size = Vector3.one;
            prototype.Layer = LayerMask.NameToLayer("Default");
            prototype.AlignToSurface = false;
            prototype.PivotOffset = 0;
            prototype.BendFactor = 1;
            prototype.Color = Color.white;
            return prototype;
        }

        public Mesh GetBaseMesh()
        {
            if (Shape == GGrassShape.DetailObject)
            {
                return DetailMesh;
            }
            if (Shape == GGrassShape.CustomMesh)
            {
                return CustomMesh != null ? CustomMesh : GRuntimeSettings.Instance.foliageRendering.GetGrassMesh(GGrassShape.Quad);
            }
            else
            {
                return GRuntimeSettings.Instance.foliageRendering.GetGrassMesh(Shape);
            }
        }

        public void RefreshDetailObjectSettings()
        {
            if (Prefab == null)
                return;
            MeshFilter mf = Prefab.GetComponentInChildren<MeshFilter>();
            MeshRenderer mr = Prefab.GetComponentInChildren<MeshRenderer>();
            if (mf != null)
            {
                DetailMesh = mf.sharedMesh;
            }
            if (mr != null)
            {
                DetailMaterial = mr.sharedMaterial;
            }
        }

        public static explicit operator GGrassPrototype(DetailPrototype p)
        {
            GGrassPrototype proto = new GGrassPrototype();
            proto.Color = p.healthyColor;
            proto.Shape = p.usePrototypeMesh ? GGrassShape.DetailObject : GGrassShape.Quad;
            proto.Texture = p.prototypeTexture;
            proto.Prefab = p.prototype;
            proto.Size = new Vector3(p.maxWidth, p.maxHeight, p.maxWidth);
            proto.Layer = LayerMask.NameToLayer("Default");
            proto.AlignToSurface = false;
            proto.BendFactor = 1;
            proto.IsBillboard = p.renderMode == DetailRenderMode.GrassBillboard;
            return proto;
        }

        public static explicit operator DetailPrototype(GGrassPrototype p)
        {
            DetailPrototype proto = new DetailPrototype();
            proto.usePrototypeMesh = p.Shape == GGrassShape.DetailObject;
            proto.prototypeTexture = p.Texture;
            proto.prototype = p.Prefab;
            proto.minWidth = p.size.x;
            proto.maxWidth = p.size.x * 2;
            proto.minHeight = p.size.y;
            proto.maxHeight = p.size.y * 2;
            proto.healthyColor = p.color;
            if (p.IsBillboard && p.Shape != GGrassShape.DetailObject)
                proto.renderMode = DetailRenderMode.GrassBillboard;

            return proto;
        }

        public bool Equals(DetailPrototype detailPrototype)
        {
            bool modeEqual =
                (Shape == GGrassShape.Quad && !detailPrototype.usePrototypeMesh) ||
                (Shape == GGrassShape.DetailObject && detailPrototype.usePrototypeMesh);
            return
                modeEqual &&
                Texture == detailPrototype.prototypeTexture &&
                Prefab == detailPrototype.prototype &&
                Size == new Vector3(detailPrototype.maxWidth, detailPrototype.maxHeight, detailPrototype.maxWidth);
        }
    }
}
#endif
