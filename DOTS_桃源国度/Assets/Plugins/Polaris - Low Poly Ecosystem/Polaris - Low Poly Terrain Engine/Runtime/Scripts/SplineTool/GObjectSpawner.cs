#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [GDisplayName("Object Spawner")]
    public class GObjectSpawner : GSplineModifier
    {
        [SerializeField]
        private AnimationCurve falloff;
        public AnimationCurve Falloff
        {
            get
            {
                if (falloff == null)
                {
                    falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }
                return falloff;
            }
            set
            {
                falloff = value;
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
        private List<int> prototypeIndices;
        public List<int> PrototypeIndices
        {
            get
            {
                if (prototypeIndices == null)
                {
                    prototypeIndices = new List<int>();
                }
                return prototypeIndices;
            }
            set
            {
                prototypeIndices = value;
            }
        }

        [SerializeField]
        private Texture2D falloffNoise;
        public Texture2D FalloffNoise
        {
            get
            {
                return falloffNoise;
            }
            set
            {
                falloffNoise = value;
            }
        }

        [SerializeField]
        private Vector2 falloffNoiseSize;
        public Vector2 FalloffNoiseSize
        {
            get
            {
                return falloffNoiseSize;
            }
            set
            {
                falloffNoiseSize = value;
            }
        }

        [SerializeField]
        private int maskResolution;
        public int MaskResolution
        {
            get
            {
                return maskResolution;
            }
            set
            {
                maskResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
            }
        }

        [SerializeField]
        private float density;
        public float Density
        {
            get
            {
                return density;
            }
            set
            {
                density = Mathf.Max(0, value);
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
        private LayerMask worldRaycastMask;
        public LayerMask WorldRaycastMask
        {
            get
            {
                return worldRaycastMask;
            }
            set
            {
                worldRaycastMask = value;
            }
        }

        private Texture2D falloffTexture;

        private Material maskMaterial;
        private Material MaskMaterial
        {
            get
            {
                if (maskMaterial == null)
                {
                    maskMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splineMaskShader);
                }
                return maskMaterial;
            }
        }

        private static readonly int FALL_OFF = Shader.PropertyToID("_Falloff");
        private static readonly int FALL_OFF_NOISE = Shader.PropertyToID("_FalloffNoise");
        private static readonly int TERRAIN_MASK = Shader.PropertyToID("_TerrainMask");

        public override void Apply()
        {
            if (SplineCreator == null)
                return;
            if (falloffTexture != null)
                Object.DestroyImmediate(falloffTexture);
            Internal_UpdateFalloffTexture();
            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(SplineCreator.SweepTest());
            foreach (GStylizedTerrain t in terrains)
            {
                Apply(t);
            }
        }

        private void Apply(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            if (PrototypeIndices.Count == 0)
                return;
            if (Prototypes.Count == 0)
                return;
            RenderTexture rt = new RenderTexture(MaskResolution, MaskResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Internal_Apply(t, rt);
            Texture2D mask = GCommon.CreateTexture(MaskResolution, Color.clear);
            GCommon.CopyFromRT(mask, rt);
            mask.wrapMode = TextureWrapMode.Clamp;

            SpawnObjectsOnTerrain(t, mask);

            rt.Release();
            GUtilities.DestroyObject(rt);
            GUtilities.DestroyObject(mask);
        }

        private void SpawnObjectsOnTerrain(GStylizedTerrain t, Texture2D mask)
        {
            int sampleCount = Mathf.FloorToInt(Density * t.TerrainData.Geometry.Width * t.TerrainData.Geometry.Length);
            NativeArray<bool> cullResult = new NativeArray<bool>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<GPrototypeInstanceInfo> instanceInfo = new NativeArray<GPrototypeInstanceInfo>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> selectedPrototypeIndices = new NativeArray<int>(PrototypeIndices.ToArray(), Allocator.TempJob);
            GTextureNativeDataDescriptor<Color32> maskHandle = new GTextureNativeDataDescriptor<Color32>(mask);

            {
                GSampleInstanceJob job = new GSampleInstanceJob()
                {
                    cullResult = cullResult,
                    instanceInfo = instanceInfo,
                    mask = maskHandle,
                    selectedPrototypeIndices = selectedPrototypeIndices,

                    minRotation = minRotation,
                    maxRotation = maxRotation,

                    minScale = minScale,
                    maxScale = maxScale,

                    seed = Random.Range(int.MinValue, int.MaxValue)
                };

                JobHandle jHandle = job.Schedule(sampleCount, 100);
                jHandle.Complete();
            }

            Vector3 terrainSize = t.TerrainData.Geometry.Size;
            NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(instanceInfo.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(instanceInfo.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < raycastCommands.Length; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                Vector3 from = new Vector3(t.transform.position.x + info.position.x * terrainSize.x, 10000, t.transform.position.z + info.position.z * terrainSize.z);
#if UNITY_2022_2_OR_NEWER
                QueryParameters q = new QueryParameters(WorldRaycastMask, false, QueryTriggerInteraction.Ignore, false);
                RaycastCommand cmd = new RaycastCommand(from, Vector3.down, q, float.MaxValue);
#else
                RaycastCommand cmd = new RaycastCommand(from, Vector3.down, float.MaxValue, WorldRaycastMask, 1);
#endif
                raycastCommands[i] = cmd;
            }
            {
                JobHandle jHandle = RaycastCommand.ScheduleBatch(raycastCommands, hits, 10);
                jHandle.Complete();
            }

            for (int i = 0; i < instanceInfo.Length; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                RaycastHit h = hits[i];
                info.position.Set(info.position.x * terrainSize.x, h.collider != null ? h.point.y : 0, info.position.z * terrainSize.z);
                instanceInfo[i] = info;
            }

            int count = instanceInfo.Length;
            for (int i = 0; i < count; ++i)
            {
                GPrototypeInstanceInfo info = instanceInfo[i];
                if (cullResult[i] == false)
                    continue;

                GameObject prototype = Prototypes[info.prototypeIndex];
                GameObject go = GSpawner.Spawn(t, prototype, Vector3.zero);
                go.transform.position = t.transform.position + info.position;
                go.transform.rotation = info.rotation;
                go.transform.localScale = info.scale;

                if (AlignToSurface)
                {
                    RaycastHit hit = hits[i];
                    if (hit.collider != null)
                    {
                        Quaternion q = Quaternion.FromToRotation(go.transform.up, hit.normal);
                        go.transform.rotation *= q;
                    }
                }
            }

            raycastCommands.Dispose();
            hits.Dispose();

            cullResult.Dispose();
            instanceInfo.Dispose();
            selectedPrototypeIndices.Dispose();
        }

        private Material PrepareMaterial(GStylizedTerrain t, RenderTexture rt)
        {
            GCommon.ClearRT(rt);
            Material mat = MaskMaterial;
            mat.SetTexture(FALL_OFF, falloffTexture);
            mat.SetTexture(FALL_OFF_NOISE, FalloffNoise);
            mat.SetTextureScale(FALL_OFF_NOISE, new Vector2(
                FalloffNoiseSize.x != 0 ? 1f / FalloffNoiseSize.x : 0,
                FalloffNoiseSize.y != 0 ? 1f / FalloffNoiseSize.y : 0));
            mat.SetTextureOffset(FALL_OFF_NOISE, Vector2.zero);
            if (SplineCreator.EnableTerrainMask)
            {
                mat.SetTexture(TERRAIN_MASK, t.TerrainData.Mask.MaskMapOrDefault);
            }
            else
            {
                mat.SetTexture(TERRAIN_MASK, Texture2D.blackTexture);
            }
            return mat;
        }

        public void Internal_Apply(GStylizedTerrain t, RenderTexture rt)
        {
            Material mat = PrepareMaterial(t, rt);
            SplineCreator.DrawOnTexture(rt, t.Bounds, mat);
        }

        public void Internal_Apply(GStylizedTerrain t, RenderTexture rt, ScriptableRenderContext context)
        {
            Material mat = PrepareMaterial(t, rt);
            SplineCreator.DrawOnTexture(rt, t.Bounds, mat, context);
        }

        public void Reset()
        {
            SplineCreator = GetComponent<GSplineCreator>();
            Falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Prototypes = null;
            PrototypeIndices = null;
            Density = 1;
            MaskResolution = 1024;
            MinRotation = 0;
            MaxRotation = 360;
            MinScale = new Vector3(0.7f, 0.8f, 0.7f);
            MaxScale = new Vector3(1.2f, 1.5f, 1.2f);
            WorldRaycastMask = 1;
        }

        public void Internal_UpdateFalloffTexture()
        {
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }
    }
}
#endif
