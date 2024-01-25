#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [GDisplayName("Foliage Spawner")]
    public class GFoliageSpawner : GSplineModifier
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
        private bool spawnTrees;
        public bool SpawnTrees
        {
            get
            {
                return spawnTrees;
            }
            set
            {
                spawnTrees = value;
            }
        }

        [SerializeField]
        private bool spawnGrasses;
        public bool SpawnGrasses
        {
            get
            {
                return spawnGrasses;
            }
            set
            {
                spawnGrasses = value;
            }
        }

        [SerializeField]
        private List<int> treePrototypeIndices;
        public List<int> TreePrototypeIndices
        {
            get
            {
                if (treePrototypeIndices == null)
                {
                    treePrototypeIndices = new List<int>();
                }
                return treePrototypeIndices;
            }
            set
            {
                treePrototypeIndices = value;
            }
        }

        [SerializeField]
        private List<int> grassPrototypeIndices;
        public List<int> GrassPrototypeIndices
        {
            get
            {
                if (grassPrototypeIndices == null)
                {
                    grassPrototypeIndices = new List<int>();
                }
                return grassPrototypeIndices;
            }
            set
            {
                grassPrototypeIndices = value;
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
        private float treeDensity;
        public float TreeDensity
        {
            get
            {
                return treeDensity;
            }
            set
            {
                treeDensity = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private float grassDensity;
        public float GrassDensity
        {
            get
            {
                return grassDensity;
            }
            set
            {
                grassDensity = Mathf.Max(0, value);
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
            RenderTexture rt = new RenderTexture(MaskResolution, MaskResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Internal_Apply(t, rt);
            Texture2D mask = GCommon.CreateTexture(MaskResolution, Color.clear);
            GCommon.CopyFromRT(mask, rt);
            mask.wrapMode = TextureWrapMode.Clamp;

            t.TerrainData.Foliage.SetTreeRegionDirty(SplineCreator.SweepDirtyRect(t));
            t.TerrainData.Foliage.SetGrassRegionDirty(SplineCreator.SweepDirtyRect(t));
            if (SpawnTrees &&
                t.TerrainData.Foliage.Trees != null &&
                TreePrototypeIndices.Count > 0)
            {
                SpawnTreesOnTerrain(t, mask);
                t.UpdateTreesPosition();
            }
            if (SpawnGrasses &&
                t.TerrainData.Foliage.Grasses != null &&
                GrassPrototypeIndices.Count > 0)
            {
                SpawnGrassesOnTerrain(t, mask);
                t.UpdateGrassPatches();
            }

            t.TerrainData.Foliage.ClearTreeDirtyRegions();
            t.TerrainData.Foliage.ClearGrassDirtyRegions();
            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);

            rt.Release();
            GUtilities.DestroyObject(rt);
            GUtilities.DestroyObject(mask);
        }

        private void SpawnTreesOnTerrain(GStylizedTerrain t, Texture2D mask)
        {
            int sampleCount = Mathf.FloorToInt(TreeDensity * t.TerrainData.Geometry.Width * t.TerrainData.Geometry.Length);
            NativeArray<bool> cullResult = new NativeArray<bool>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<GPrototypeInstanceInfo> foliageInfo = new NativeArray<GPrototypeInstanceInfo>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> selectedPrototypeIndices = new NativeArray<int>(TreePrototypeIndices.ToArray(), Allocator.TempJob);
            GTextureNativeDataDescriptor<Color32> maskHandle = new GTextureNativeDataDescriptor<Color32>(mask);

            GSampleInstanceJob job = new GSampleInstanceJob()
            {
                cullResult = cullResult,
                instanceInfo = foliageInfo,
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

            List<GTreeInstance> instances = new List<GTreeInstance>();
            for (int i = 0; i < sampleCount; ++i)
            {
                if (cullResult[i] == false)
                    continue;
                GTreeInstance tree = foliageInfo[i].ToTreeInstance();
                instances.Add(tree);
            }
            t.TerrainData.Foliage.AddTreeInstances(instances);

            cullResult.Dispose();
            foliageInfo.Dispose();
            selectedPrototypeIndices.Dispose();
        }

        private void SpawnGrassesOnTerrain(GStylizedTerrain t, Texture2D mask)
        {
            int sampleCount = Mathf.FloorToInt(GrassDensity * t.TerrainData.Geometry.Width * t.TerrainData.Geometry.Length);
            NativeArray<bool> cullResultNA = new NativeArray<bool>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<GPrototypeInstanceInfo> foliageInfoNA = new NativeArray<GPrototypeInstanceInfo>(sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> selectedPrototypeIndicesNA = new NativeArray<int>(GrassPrototypeIndices.ToArray(), Allocator.TempJob);
            GTextureNativeDataDescriptor<Color32> maskHandleNA = new GTextureNativeDataDescriptor<Color32>(mask);

            GSampleInstanceJob job = new GSampleInstanceJob()
            {
                cullResult = cullResultNA,
                instanceInfo = foliageInfoNA,
                mask = maskHandleNA,
                selectedPrototypeIndices = selectedPrototypeIndicesNA,

                minRotation = minRotation,
                maxRotation = maxRotation,

                minScale = minScale,
                maxScale = maxScale,

                seed = Random.Range(int.MinValue, int.MaxValue)
            };

            JobHandle jHandle = job.Schedule(sampleCount, 100);
            jHandle.Complete();

            t.TerrainData.Foliage.AddGrassInstancesWithFilter(cullResultNA, foliageInfoNA);
            cullResultNA.Dispose();
            foliageInfoNA.Dispose();
            selectedPrototypeIndicesNA.Dispose();
        }

        private Material PrepareMaterial(GStylizedTerrain t, RenderTexture rt)
        {
            GCommon.ClearRT(rt);
            Material mat = MaskMaterial;
            mat.SetTexture(FALL_OFF, falloffTexture);
            mat.SetTexture(FALL_OFF_NOISE, FalloffNoise != null ? FalloffNoise : Texture2D.blackTexture);
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
            SplineCreator.DrawOnTexture(rt, t.Bounds, maskMaterial);
        }

        public void Internal_Apply(GStylizedTerrain t, RenderTexture rt, ScriptableRenderContext context)
        {
            Material mat = PrepareMaterial(t, rt);
            SplineCreator.DrawOnTexture(rt, t.Bounds, maskMaterial, context);
        }

        public void Reset()
        {
            SplineCreator = GetComponent<GSplineCreator>();
            Falloff = AnimationCurve.EaseInOut(0, 0, 1, 1);
            SpawnTrees = true;
            SpawnGrasses = true;
            MaskResolution = 1024;
            TreePrototypeIndices = null;
            GrassPrototypeIndices = null;
            MinRotation = 0;
            MaxRotation = 360;
            MinScale = new Vector3(0.7f, 0.8f, 0.7f);
            MaxScale = new Vector3(1.2f, 1.5f, 1.2f);
        }

        public void Internal_UpdateFalloffTexture()
        {
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }
    }
}
#endif