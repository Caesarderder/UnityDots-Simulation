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
    public class GTerrainTexturePainter : MonoBehaviour
    {
        public const float GEOMETRY_OPACITY_EXPONENT = 3;

        private static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GElevationPainter",
            "GHeightSamplingPainter",
            "GTerracePainter",
            "GRemapPainter",
            "GNoisePainter",
            "GSubDivPainter",
            "GVisibilityPainter",
            "GAlbedoPainter",
            "GMetallicPainter",
            "GSmoothnessPainter",
            "GSplatPainter",
            "GMaskPainter"
        });

        private static List<Type> customPainterTypes;
        private static List<Type> CustomPainterTypes
        {
            get
            {
                if (customPainterTypes == null)
                    customPainterTypes = new List<Type>();
                return customPainterTypes;
            }
            set
            {
                customPainterTypes = value;
            }
        }

        public static string TexturePainterInterfaceName
        {
            get
            {
                return typeof(IGTexturePainter).Name;
            }
        }

        static GTerrainTexturePainter()
        {
            RefreshCustomPainterTypes();
        }

        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(TexturePainterInterfaceName) != null &&
                !BUILTIN_PAINTER_NAME.Contains(t.Name));
        }

        public static List<Type> GetCustomPainterTypes()
        {
            return CustomPainterTypes;
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
        private GTexturePaintingMode mode;
        public GTexturePaintingMode Mode
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
        private bool enableTerrainMask = true;
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
        private bool forceUpdateGeometry;
        public bool ForceUpdateGeometry
        {
            get
            {
                return forceUpdateGeometry;
            }
            set
            {
                forceUpdateGeometry = value;
            }
        }

        public IGTexturePainter ActivePainter
        {
            get
            {
                if (Mode == GTexturePaintingMode.Elevation)
                {
                    return new GElevationPainter();
                }
                else if (Mode == GTexturePaintingMode.HeightSampling)
                {
                    return new GHeightSamplingPainter();
                }
                else if (Mode == GTexturePaintingMode.Terrace)
                {
                    return new GTerracePainter();
                }
                else if (Mode == GTexturePaintingMode.Remap)
                {
                    return new GRemapPainter();
                }
                else if (Mode == GTexturePaintingMode.Noise)
                {
                    return new GNoisePainter();
                }
                else if (Mode == GTexturePaintingMode.SubDivision)
                {
                    return new GSubDivPainter();
                }
                else if (Mode == GTexturePaintingMode.Visibility)
                {
                    return new GVisibilityPainter();
                }
                else if (Mode == GTexturePaintingMode.Albedo)
                {
                    return new GAlbedoPainter();
                }
                else if (Mode == GTexturePaintingMode.Metallic)
                {
                    return new GMetallicPainter();
                }
                else if (Mode == GTexturePaintingMode.Smoothness)
                {
                    return new GSmoothnessPainter();
                }
                else if (Mode == GTexturePaintingMode.Splat)
                {
                    return new GSplatPainter();
                }
                else if (Mode == GTexturePaintingMode.Mask)
                {
                    return new GMaskPainter();
                }
                else if (mode == GTexturePaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGTexturePainter;
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
        private float brushOpacity;
        public float BrushOpacity
        {
            get
            {
                return brushOpacity;
            }
            set
            {
                brushOpacity = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float brushOpacityJitter;
        public float BrushOpacityJitter
        {
            get
            {
                return brushOpacityJitter;
            }
            set
            {
                brushOpacityJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private float brushTargetStrength = 1;
        public float BrushTargetStrength
        {
            get
            {
                return brushTargetStrength;
            }
            set
            {
                brushTargetStrength = Mathf.Clamp01(value);
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
        private Color brushColor;
        public Color BrushColor
        {
            get
            {
                return brushColor;
            }
            set
            {
                brushColor = value;
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

        public int SelectedSplatIndex
        {
            get
            {
                if (SelectedSplatIndices.Count == 0)
                {
                    SelectedSplatIndices.Add(0);
                }
                return SelectedSplatIndices[0];
            }
            set
            {
                SelectedSplatIndices.Clear();
                SelectedSplatIndices.Add(value);
            }
        }

        [SerializeField]
        private List<int> selectedSplatIndices;
        public List<int> SelectedSplatIndices
        {
            get
            {
                if (selectedSplatIndices == null)
                {
                    selectedSplatIndices = new List<int>();
                }
                return selectedSplatIndices;
            }
            set
            {
                selectedSplatIndices = value;
            }
        }

        [SerializeField]
        private Vector3 samplePoint;
        public Vector3 SamplePoint
        {
            get
            {
                return samplePoint;
            }
            set
            {
                samplePoint = value;
            }
        }

        [SerializeField]
        private GConditionalPaintingConfigs conditionalPaintingConfigs;
        public GConditionalPaintingConfigs ConditionalPaintingConfigs
        {
            get
            {
                return conditionalPaintingConfigs;
            }
            set
            {
                conditionalPaintingConfigs = value;
            }
        }

        internal static Dictionary<string, RenderTexture> internal_RenderTextures;

        private void OnEnable()
        {
            ReloadBrushMasks();

            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.UpdateCurveTextures();
            }
        }

        private void OnDisable()
        {
            CleanUp();
        }

        private void Reset()
        {
            GroupId = 0;
            Mode = GTexturePaintingMode.Elevation;
            BrushRadius = 50;
            BrushRadiusJitter = 0;
            BrushOpacity = 0.5f;
            BrushOpacityJitter = 0;
            BrushTargetStrength = 1;
            BrushRotation = 0;
            BrushRotationJitter = 0;
            BrushColor = Color.white;

            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.CleanUp();
            }
            conditionalPaintingConfigs = new GConditionalPaintingConfigs();
            conditionalPaintingConfigs.UpdateCurveTextures();
        }

        public void ReloadBrushMasks()
        {
            BrushMasks = new List<Texture2D>(Resources.LoadAll<Texture2D>(GCommon.BRUSH_MASK_RESOURCES_PATH));
        }

        public void Paint(GTexturePainterArgs args)
        {
            IGTexturePainter p = ActivePainter;
            if (p == null)
                return;

            FillArgs(ref args);

            List<GStylizedTerrain> overlappedTerrain = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(GroupId, args.HitPoint, args.Radius, args.Rotation));
#if UNITY_EDITOR
            if (args.MouseEventType == GPainterMouseEventType.Down ||
                args.MouseEventType == GPainterMouseEventType.Drag)
            {
                Editor_CreateInitialHistoryEntry(args, overlappedTerrain);
            }
#endif
            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.BeginPainting(t, args);
            }

            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.EndPainting(t, args);
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

        private void Editor_CreateInitialHistoryEntry(GTexturePainterArgs args, List<GStylizedTerrain> overlappedTerrains)
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

        private void Editor_CreateHistory(GTexturePainterArgs args)
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

        internal static RenderTexture Internal_GetRenderTexture(GStylizedTerrain t, int resolution, int id = 0)
        {
            if (internal_RenderTextures == null)
            {
                internal_RenderTextures = new Dictionary<string, RenderTexture>();
            }

            string key = string.Format("{0}_{1}", t != null ? t.GetInstanceID() : 0, id);
            if (!internal_RenderTextures.ContainsKey(key) ||
                internal_RenderTextures[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                internal_RenderTextures[key] = rt;
            }
            else if (internal_RenderTextures[key].width != resolution ||
                internal_RenderTextures[key].height != resolution ||
                internal_RenderTextures[key].format != GGeometry.HeightMapRTFormat)
            {
                internal_RenderTextures[key].Release();
                Object.DestroyImmediate(internal_RenderTextures[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                internal_RenderTextures[key] = rt;
            }

            internal_RenderTextures[key].wrapMode = TextureWrapMode.Clamp;

            return internal_RenderTextures[key];
        }

        public static void Internal_ReleaseRenderTextures()
        {
            if (internal_RenderTextures != null)
            {
                foreach (string k in internal_RenderTextures.Keys)
                {
                    RenderTexture rt = internal_RenderTextures[k];
                    if (rt == null)
                        continue;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
            }
        }

        private Rand GetRandomGenerator()
        {
            return new Rand(System.DateTime.Now.Millisecond);
        }

        private void ProcessBrushDynamic(ref GTexturePainterArgs args)
        {
            Rand rand = GetRandomGenerator();
            args.Radius -= BrushRadius * BrushRadiusJitter * (float)rand.NextDouble();
            args.Rotation += Mathf.Sign((float)rand.NextDouble() - 0.5f) * BrushRotation * BrushRotationJitter * (float)rand.NextDouble();
            args.Opacity -= BrushOpacity * BrushOpacityJitter * (float)rand.NextDouble();

            Vector3 scatterDir = new Vector3((float)(rand.NextDouble() * 2 - 1), 0, (float)(rand.NextDouble() * 2 - 1)).normalized;
            float scatterLengthMultiplier = BrushScatter - (float)rand.NextDouble() * BrushScatterJitter;
            float scatterLength = args.Radius * scatterLengthMultiplier;

            args.HitPoint += scatterDir * scatterLength;
        }

        public void FillArgs(ref GTexturePainterArgs args, bool useBrushDynamic = true)
        {
            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Opacity = BrushOpacity * BrushTargetStrength;
            args.Color = BrushColor;
            if (SelectedSplatIndices.Count > 0)
            {
                args.SplatIndex = SelectedSplatIndices[Random.Range(0, SelectedSplatIndices.Count)];
            }
            else
            {
                args.SplatIndex = -1;
            }
            args.SamplePoint = SamplePoint;
            args.CustomArgs = CustomPainterArgs;
            args.ForceUpdateGeometry = ForceUpdateGeometry;
            args.EnableTerrainMask = EnableTerrainMask;
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.BrushMask = BrushMasks[SelectedBrushMaskIndex];
            }

            if (args.ActionType == GPainterActionType.Alternative &&
                args.MouseEventType == GPainterMouseEventType.Down)
            {
                SamplePoint = args.HitPoint;
                args.SamplePoint = args.HitPoint;
            }

            if (useBrushDynamic)
            {
                ProcessBrushDynamic(ref args);
            }

            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            args.ConditionalPaintingConfigs = this.ConditionalPaintingConfigs;
        }

        public void CleanUp()
        {
            Internal_ReleaseRenderTextures();
            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.CleanUp();
            }
        }
    }
}
#endif
