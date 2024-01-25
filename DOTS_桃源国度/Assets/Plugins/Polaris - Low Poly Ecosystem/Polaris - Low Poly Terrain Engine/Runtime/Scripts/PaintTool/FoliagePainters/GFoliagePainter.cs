#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using Type = System.Type;
using Rand = System.Random;
#if UNITY_EDITOR
using Pinwheel.Griffin.BackupTool;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GFoliagePainter : MonoBehaviour
    {
        private static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GTreePainter",
            "GTreeScaler",
            "GGrassPainter",
            "GGrassScaler"
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

        public static string FoliagePainterInterfaceName
        {
            get
            {
                return typeof(IGFoliagePainter).Name;
            }
        }

        static GFoliagePainter()
        {
            RefreshCustomPainterTypes();
        }

        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(FoliagePainterInterfaceName) != null &&
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
        private GFoliagePaintingMode mode;
        public GFoliagePaintingMode Mode
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

        public IGFoliagePainter ActivePainter
        {
            get
            {
                if (Mode == GFoliagePaintingMode.PaintTree)
                {
                    return new GTreePainter();
                }
                else if (Mode == GFoliagePaintingMode.ScaleTree)
                {
                    return new GTreeScaler();
                }
                else if (Mode == GFoliagePaintingMode.PaintGrass)
                {
                    return new GGrassPainter();
                }
                else if (Mode == GFoliagePaintingMode.ScaleGrass)
                {
                    return new GGrassScaler();
                }
                else if (mode == GFoliagePaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGFoliagePainter;
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
        private List<int> selectedTreeIndices;
        public List<int> SelectedTreeIndices
        {
            get
            {
                if (selectedTreeIndices == null)
                {
                    selectedTreeIndices = new List<int>();
                }
                return selectedTreeIndices;
            }
            set
            {
                selectedTreeIndices = value;
            }
        }

        [SerializeField]
        private List<int> selectedGrassIndices;
        public List<int> SelectedGrassIndices
        {
            get
            {
                if (selectedGrassIndices == null)
                {
                    selectedGrassIndices = new List<int>();
                }
                return selectedGrassIndices;
            }
            set
            {
                selectedGrassIndices = value;
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
            Mode = GFoliagePaintingMode.PaintTree;
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

        public void Paint(GFoliagePainterArgs args)
        {
            IGFoliagePainter p = ActivePainter;
            if (p == null)
                return;

            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Density = BrushDensity;
            args.EraseRatio = EraseRatio;
            args.ScaleStrength = ScaleStrength;
            args.TreeIndices = SelectedTreeIndices;
            args.GrassIndices = SelectedGrassIndices;

            args.CustomArgs = CustomPainterArgs;
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.Mask = BrushMasks[SelectedBrushMaskIndex];
            }
            args.Filters = GetComponents<GSpawnFilter>();
            args.EnableTerrainMask = EnableTerrainMask;

            ProcessBrushDynamic(ref args);
            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            List<GStylizedTerrain> overlappedTerrain = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(GroupId, args.HitPoint, args.Radius, args.Rotation));
#if UNITY_EDITOR
            if ((args.MouseEventType == GPainterMouseEventType.Down ||
                args.MouseEventType == GPainterMouseEventType.Drag) &&
                args.ShouldCommitNow == false)
            {
                Editor_CreateInitialHistoryEntry(args, overlappedTerrain);
            }
#endif

            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.Paint(t, args);
            }

#if UNITY_EDITOR
            EditedTerrains.UnionWith(overlappedTerrain);
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                Editor_CreateHistory(args);
                currentInitialBackupName = null;
                InitialRecordedTerrains.Clear();
                EditedTerrains.Clear();
            }
#endif
        }

#if UNITY_EDITOR
        private HashSet<GStylizedTerrain> initialRecordedTerrains;
        private HashSet<GStylizedTerrain> InitialRecordedTerrains
        {
            get
            {
                if (initialRecordedTerrains == null)
                {
                    initialRecordedTerrains = new HashSet<GStylizedTerrain>();
                }
                return initialRecordedTerrains;
            }
        }

        private HashSet<GStylizedTerrain> editedTerrains;
        private HashSet<GStylizedTerrain> EditedTerrains
        {
            get
            {
                if (editedTerrains == null)
                {
                    editedTerrains = new HashSet<GStylizedTerrain>();
                }
                return editedTerrains;
            }
        }

        private string currentInitialBackupName;

        private void Editor_CreateInitialHistoryEntry(GFoliagePainterArgs args, List<GStylizedTerrain> overlappedTerrains)
        {
            if (!GEditorSettings.Instance.paintTools.enableHistory)
                return;
            if (overlappedTerrains.Count == 0)
                return;

            List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
            flags.AddRange(ActivePainter.GetResourceFlagForHistory(args));

            if (InitialRecordedTerrains.Count == 0)
            {
                currentInitialBackupName = GBackup.TryCreateInitialBackup(ActivePainter.HistoryPrefix, overlappedTerrains[0], flags, false);
                if (!string.IsNullOrEmpty(currentInitialBackupName))
                {
                    InitialRecordedTerrains.Add(overlappedTerrains[0]);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentInitialBackupName))
                {
                    for (int i = 0; i < overlappedTerrains.Count; ++i)
                    {
                        if (InitialRecordedTerrains.Contains(overlappedTerrains[i]))
                            continue;
                        GBackup.BackupTerrain(overlappedTerrains[i], currentInitialBackupName, flags);
                        InitialRecordedTerrains.Add(overlappedTerrains[i]);
                    }
                }
            }
        }

        private void Editor_CreateHistory(GFoliagePainterArgs args)
        {
            if (!GEditorSettings.Instance.paintTools.enableHistory)
                return;
            if (EditedTerrains.Count == 0)
                return;

            List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
            flags.AddRange(ActivePainter.GetResourceFlagForHistory(args));

            List<GStylizedTerrain> terrainList = new List<GStylizedTerrain>(EditedTerrains);
            string backupName = GBackup.TryCreateBackup(ActivePainter.HistoryPrefix, terrainList[0], flags, false);
            if (!string.IsNullOrEmpty(backupName))
            {
                for (int i = 1; i < terrainList.Count; ++i)
                {
                    GBackup.BackupTerrain(terrainList[i], backupName, flags);
                }
            }
        }
#endif

        private Rand GetRandomGenerator()
        {
            return new Rand(Time.frameCount);
        }

        private void ProcessBrushDynamic(ref GFoliagePainterArgs args)
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
