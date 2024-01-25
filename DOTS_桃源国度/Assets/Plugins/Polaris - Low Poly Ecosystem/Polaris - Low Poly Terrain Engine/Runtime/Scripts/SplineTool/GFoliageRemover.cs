#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.SplineTool
{
    [GDisplayName("Foliage Remover")]
    public class GFoliageRemover : GSplineModifier
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
        private bool removeTrees;
        public bool RemoveTrees
        {
            get
            {
                return removeTrees;
            }
            set
            {
                removeTrees = value;
            }
        }

        [SerializeField]
        private bool removeGrasses;
        public bool RemoveGrasses
        {
            get
            {
                return removeGrasses;
            }
            set
            {
                removeGrasses = value;
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

            if (RemoveTrees)
            {
                RemoveTreeOnTerrain(t, mask);
            }
            if (RemoveGrasses)
            {
                RemoveGrassOnTerrain(t, mask);
            }

            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);

            rt.Release();
            GUtilities.DestroyObject(rt);
            GUtilities.DestroyObject(mask);
        }

        private void RemoveTreeOnTerrain(GStylizedTerrain t, Texture2D mask)
        {
            NativeArray<Vector2> positionsNA = t.TerrainData.Foliage.GetTreesPositionArray();
            NativeArray<bool> resultNA = new NativeArray<bool>(positionsNA.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GTextureNativeDataDescriptor<Color32> maskNA = new GTextureNativeDataDescriptor<Color32>(mask);

            GMaskCullingJob job = new GMaskCullingJob()
            {
                positions = positionsNA,
                result = resultNA,
                mask = maskNA
            };

            JobHandle jHandle = job.Schedule(positionsNA.Length, 100);
            jHandle.Complete();

            HashSet<int> selectedPrototypes = new HashSet<int>(TreePrototypeIndices);
            int index = 0;
            t.TerrainData.Foliage.RemoveTreeInstances(tree =>
            {
                int i = index;
                index += 1;
                if (!selectedPrototypes.Contains(tree.prototypeIndex))
                    return false;
                return resultNA[i];
            });

            positionsNA.Dispose();
            resultNA.Dispose();
        }

        private void RemoveGrassOnTerrain(GStylizedTerrain t, Texture2D mask)
        {
            GGrassPatch[] patches = t.TerrainData.Foliage.GrassPatches;
            GMaskCullingDataHolder[] data = new GMaskCullingDataHolder[patches.Length];
            JobHandle[] jHandles = new JobHandle[patches.Length];
            GTextureNativeDataDescriptor<Color32> maskNA = new GTextureNativeDataDescriptor<Color32>(mask);

            for (int i = 0; i < patches.Length; ++i)
            {
                GMaskCullingDataHolder d = new GMaskCullingDataHolder();
                d.positionsNA = patches[i].GetGrassPositionArray();
                d.resultNA = new NativeArray<bool>(d.positionsNA.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                data[i] = d;

                GMaskCullingJob job = new GMaskCullingJob()
                {
                    positions = d.positionsNA,
                    result = d.resultNA,
                    mask = maskNA
                };

                JobHandle h = job.Schedule(d.positionsNA.Length, 100);
                jHandles[i] = h;
            }

            HashSet<int> selectedPrototypes = new HashSet<int>(GrassPrototypeIndices);
            for (int i = 0; i < patches.Length; ++i)
            {
                JobHandle h = jHandles[i];
                h.Complete();

                GMaskCullingDataHolder d = data[i];
                int index = 0;
                patches[i].RemoveInstances(g =>
                {
                    int indexClone = index;
                    index += 1;

                    if (!selectedPrototypes.Contains(g.prototypeIndex))
                        return false;
                    return d.resultNA[indexClone];
                });

                d.positionsNA.Dispose();
                d.resultNA.Dispose();
            }
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
            RemoveTrees = true;
            RemoveGrasses = true;
            TreePrototypeIndices = null;
            GrassPrototypeIndices = null;
            MaskResolution = 1024;
        }

        public void Internal_UpdateFalloffTexture()
        {
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }
    }
}
#endif