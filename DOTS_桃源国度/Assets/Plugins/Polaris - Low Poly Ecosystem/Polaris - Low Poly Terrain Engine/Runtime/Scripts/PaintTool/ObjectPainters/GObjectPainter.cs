#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Rand = System.Random;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GObjectPainter : MonoBehaviour
    {
        private static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GObjectSpawner",
            "GObjectScaler"
        });

        private static List<Type> customPainterTypes;
        public static List<Type> CustomPainterTypes
        {
            get
            {
                if (customPainterTypes == null)
                    customPainterTypes = new List<Type>();
                return customPainterTypes;
            }
            private set
            {
                customPainterTypes = value;
            }
        }

        public static string ObjectPainterInterfaceName
        {
            get
            {
                return typeof(IGObjectPainter).Name;
            }
        }

        static GObjectPainter()
        {
            RefreshCustomPainterTypes();
        }

        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(ObjectPainterInterfaceName) != null &&
                !BUILTIN_PAINTER_NAME.Contains(t.Name));
        }

        [SerializeField]
        private int groupId;
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private bool enableTerrainMask;
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        private GObjectPaintingMode mode;
        public GObjectPaintingMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        [SerializeField]
        private int customPainterIndex;
        public int CustomPainterIndex
        {
            get
            {
                return customPainterIndex;
            }
            set
            {
                customPainterIndex = value;
            }
        }

        [SerializeField]
        private string customPainterArgs;
        public string CustomPainterArgs
        {
            get
            {
                return customPainterArgs;
            }
            set
            {
                customPainterArgs = value;
            }
        }

        public IGObjectPainter ActivePainter
        {
            get
            {
                if (Mode == GObjectPaintingMode.Spawn)
                {
                    return new GObjectSpawner();
                }
                else if (Mode == GObjectPaintingMode.Scale)
                {
                    return new GObjectScaler();
                }
                else if (mode == GObjectPaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGObjectPainter;
                }
                return null;
            }
        }

        [SerializeField]
        private float brushRadius;
        public float BrushRadius
        {
            get
            {
                return brushRadius;
            }
            set
            {
                brushRadius = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        private float brushRadiusJitter;
        public float BrushRadiusJitter
        {
            get
            {
                return brushRadiusJitter;
            }
            set
            {
                brushRadiusJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float brushRotation;
        public float BrushRotation
        {
            get
            {
                return brushRotation;
            }
            set
            {
                brushRotation = value;
            }
        }

        [SerializeField]
        private float brushRotationJitter;
        public float BrushRotationJitter
        {
            get
            {
                return brushRotationJitter;
            }
            set
            {
                brushRotationJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private int brushDensity;
        public int BrushDensity
        {
            get
            {
                return brushDensity;
            }
            set
            {
                brushDensity = Mathf.Clamp(value, 1, 100);
            }
        }

        [SerializeField]
        private float brushDensityJitter;
        public float BrushDensityJitter
        {
            get
            {
                return brushDensityJitter;
            }
            set
            {
                brushDensityJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float brushScatter;
        public float BrushScatter
        {
            get
            {
                return brushScatter;
            }
            set
            {
                brushScatter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float brushScatterJitter;
        public float BrushScatterJitter
        {
            get
            {
                return brushScatterJitter;
            }
            set
            {
                brushScatterJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private List<Texture2D> brushMasks;
        public List<Texture2D> BrushMasks
        {
            get
            {
                if (brushMasks == null)
                    brushMasks = new List<Texture2D>();
                return brushMasks;
            }
            set
            {
                brushMasks = value;
            }
        }

        [SerializeField]
        private int selectedBrushMaskIndex;
        public int SelectedBrushMaskIndex
        {
            get
            {
                return selectedBrushMaskIndex;
            }
            set
            {
                if (BrushMasks.Count > 0)
                    selectedBrushMaskIndex = Mathf.Clamp(value, 0, BrushMasks.Count);
                else
                    selectedBrushMaskIndex = -1;
            }
        }

        [SerializeField]
        private List<GameObject> prototypes;
        public List<GameObject> Prototypes
        {
            get
            {
                if (prototypes == null)
                {
                    prototypes = new List<GameObject>();
                }
                return prototypes;
            }
            set
            {
                prototypes = value;
            }
        }

        [SerializeField]
        private List<int> selectedPrototypeIndices;
        public List<int> SelectedPrototypeIndices
        {
            get
            {
                if (selectedPrototypeIndices == null)
                {
                    selectedPrototypeIndices = new List<int>();
                }
                return selectedPrototypeIndices;
            }
            set
            {
                selectedPrototypeIndices = value;
            }
        }

        [SerializeField]
        private float eraseRatio;
        public float EraseRatio
        {
            get
            {
                return eraseRatio;
            }
            set
            {
                eraseRatio = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float scaleStrength;
        public float ScaleStrength
        {
            get
            {
                return scaleStrength;
            }
            set
            {
                scaleStrength = Mathf.Max(0, value);
            }
        }

        private void OnEnable()
        {
            ReloadBrushMasks();
        }

        private void Reset()
        {
            GroupId = 0;
            Mode = GObjectPaintingMode.Spawn;
            BrushRadius = 50;
            BrushRadiusJitter = 0;
            BrushDensity = 1;
            BrushDensityJitter = 0;
            BrushRotation = 0;
            BrushRotationJitter = 0;
            EraseRatio = 1;
            ScaleStrength = 1;
        }

        public void ReloadBrushMasks()
        {
            BrushMasks = new List<Texture2D>(Resources.LoadAll<Texture2D>(GCommon.BRUSH_MASK_RESOURCES_PATH));
        }

        public void Paint(GObjectPainterArgs args)
        {
            IGObjectPainter p = ActivePainter;
            if (p == null)
                return;

            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Density = BrushDensity;
            args.EraseRatio = EraseRatio;
            args.ScaleStrength = ScaleStrength;
            args.CustomArgs = CustomPainterArgs;
            args.Filters = GetComponents<GSpawnFilter>();
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.Mask = BrushMasks[SelectedBrushMaskIndex];
            }
            if (SelectedPrototypeIndices.Count == 0)
            {
                return;
            }
            args.Prototypes = Prototypes;
            args.PrototypeIndices = SelectedPrototypeIndices;
            args.EnableTerrainMask = EnableTerrainMask;

            ProcessBrushDynamic(ref args);
            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(groupId, args.HitPoint, args.Radius, args.Rotation));
            foreach(GStylizedTerrain t in terrains)
            {
                p.Paint(t, args);
            }
        }

        private Rand GetRandomGenerator()
        {
            return new Rand(Time.frameCount);
        }

        private void ProcessBrushDynamic(ref GObjectPainterArgs args)
        {
            Rand rand = GetRandomGenerator();
            args.Radius -= BrushRadius * BrushRadiusJitter * (float)rand.NextDouble();
            args.Rotation += Mathf.Sign((float)rand.NextDouble() - 0.5f) * BrushRotation * BrushRotationJitter * (float)rand.NextDouble();
            args.Density -= Mathf.RoundToInt(BrushDensity * BrushDensityJitter * (float)rand.NextDouble());

            Vector3 scatterDir = new Vector3((float)(rand.NextDouble() * 2 - 1), 0, (float)(rand.NextDouble() * 2 - 1)).normalized;
            float scatterLengthMultiplier = BrushScatter - (float)rand.NextDouble() * BrushScatterJitter;
            float scatterLength = args.Radius * scatterLengthMultiplier;

            args.HitPoint += scatterDir * scatterLength;
        }
    }
}
#endif
