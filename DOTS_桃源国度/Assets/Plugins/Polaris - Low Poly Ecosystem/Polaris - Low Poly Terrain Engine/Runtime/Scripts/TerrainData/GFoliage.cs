#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Unity.Collections;
using Pinwheel.Griffin.Rendering;
using Pinwheel.Griffin.Compression;

namespace Pinwheel.Griffin
{
    public class GFoliage : ScriptableObject, ISerializationCallbackReceiver
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
        private List<GTreeInstance> treeInstances;
        public List<GTreeInstance> TreeInstances
        {
            get
            {
                if (treeInstances == null)
                    treeInstances = new List<GTreeInstance>();
                return treeInstances;
            }
            set
            {
                treeInstances = value;
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
        private GGrassPrototypeGroup grasses;
        public GGrassPrototypeGroup Grasses
        {
            get
            {
                return grasses;
            }
            set
            {
                GGrassPrototypeGroup oldValue = grasses;
                GGrassPrototypeGroup newValue = value;
                grasses = newValue;
                if (oldValue != newValue)
                {
                    if (TerrainData != null)
                    {
                        TerrainData.InvokeGrassPrototypeGroupChanged();
                    }
                }
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
        private int patchGridSize;
        public int PatchGridSize
        {
            get
            {
                return patchGridSize;
            }
            set
            {
                int oldValue = patchGridSize;
                int newValue = Mathf.Clamp(value, 1, 20);

                patchGridSize = newValue;
                if (oldValue != newValue)
                {
                    if (grassPatches != null)
                    {
                        ResampleGrassPatches();
                    }
                    if (TerrainData != null)
                    {
                        TerrainData.InvokeGrassPatchGridSizeChange();
                    }
                }
            }
        }

        [SerializeField]
        private GGrassPatch[] grassPatches;
        public GGrassPatch[] GrassPatches
        {
            get
            {
                if (grassPatches == null)
                {
                    grassPatches = new GGrassPatch[PatchGridSize * PatchGridSize];
                    for (int x = 0; x < PatchGridSize; ++x)
                    {
                        for (int z = 0; z < patchGridSize; ++z)
                        {
                            GGrassPatch patch = new GGrassPatch(this, x, z);
                            grassPatches[GUtilities.To1DIndex(x, z, PatchGridSize)] = patch;
                        }
                    }
                }
                if (grassPatches.Length != PatchGridSize * PatchGridSize)
                {
                    ResampleGrassPatches();
                }
                return grassPatches;
            }
        }

        private List<Rect> treeDirtyRegions;
        private List<Rect> TreeDirtyRegions
        {
            get
            {
                if (treeDirtyRegions == null)
                {
                    treeDirtyRegions = new List<Rect>();
                }
                return treeDirtyRegions;
            }
            set
            {
                treeDirtyRegions = value;
            }
        }

        private List<Rect> grassDirtyRegions;
        private List<Rect> GrassDirtyRegions
        {
            get
            {
                if (grassDirtyRegions == null)
                {
                    grassDirtyRegions = new List<Rect>();
                }
                return grassDirtyRegions;
            }
            set
            {
                grassDirtyRegions = value;
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

        public int GrassInstanceCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < GrassPatches.Length; ++i)
                {
                    count += GrassPatches[i].Instances.Count;
                }
                return count;
            }
        }

        public float grassVersion;

        public const float GRASS_VERSION_COMPRESSED = 2020.1f;

        public void Reset()
        {
            name = "Foliage";
            TreeSnapMode = GRuntimeSettings.Instance.foliageDefault.treeSnapMode;
            TreeSnapLayerMask = GRuntimeSettings.Instance.foliageDefault.treeSnapLayerMask;
            GrassSnapMode = GRuntimeSettings.Instance.foliageDefault.grassSnapMode;
            GrassSnapLayerMask = GRuntimeSettings.Instance.foliageDefault.grassSnapLayerMask;
            PatchGridSize = GRuntimeSettings.Instance.foliageDefault.patchGridSize;
            EnableInteractiveGrass = GRuntimeSettings.Instance.foliageDefault.enableInteractiveGrass;
            VectorFieldMapResolution = GRuntimeSettings.Instance.foliageDefault.vectorFieldMapResolution;
            BendSensitive = GRuntimeSettings.Instance.foliageDefault.bendSensitive;
            RestoreSensitive = GRuntimeSettings.Instance.foliageDefault.restoreSensitive;
            ClearGrassInstances();
            ClearTreeInstances();

            grassVersion = GVersionInfo.Number;
        }

        public void ResetFull()
        {
            Reset();
        }

        public void Refresh()
        {
            //if (Trees != null)
            //{
            //    List<GTreePrototype> prototypes = Trees.Prototypes;
            //    //for (int i = 0; i < prototypes.Count; ++i)
            //    //{
            //    //    prototypes[i].Refresh();
            //    //}
            //    RemoveTreeInstances(t => t.PrototypeIndex < 0 || t.PrototypeIndex >= Trees.Prototypes.Count);
            //}
            //if (Grasses != null)
            //{
            //    for (int i = 0; i < GrassPatches.Length; ++i)
            //    {
            //        GrassPatches[i].RemoveInstances(g => g.PrototypeIndex < 0 || g.PrototypeIndex >= Grasses.Prototypes.Count);
            //    }
            //}
        }

        private void ResampleGrassPatches()
        {
            List<GGrassInstance> grassInstances = new List<GGrassInstance>();
            for (int i = 0; i < grassPatches.Length; ++i)
            {
                grassInstances.AddRange(grassPatches[i].Instances);
            }

            grassPatches = new GGrassPatch[PatchGridSize * PatchGridSize];
            for (int x = 0; x < PatchGridSize; ++x)
            {
                for (int z = 0; z < patchGridSize; ++z)
                {
                    int index = GUtilities.To1DIndex(x, z, PatchGridSize);
                    GGrassPatch patch = new GGrassPatch(this, x, z);
                    grassPatches[index] = patch;
                }
            }

            AddGrassInstances(grassInstances);
        }

        public void AddGrassInstances(List<GGrassInstance> instances)
        {
            Rect[] uvRects = new Rect[GrassPatches.Length];
            for (int r = 0; r < uvRects.Length; ++r)
            {
                uvRects[r] = GrassPatches[r].GetUvRange();
            }

            bool[] dirty = new bool[GrassPatches.Length];
            for (int i = 0; i < instances.Count; ++i)
            {
                GGrassInstance grass = instances[i];
                for (int r = 0; r < uvRects.Length; ++r)
                {
                    if (uvRects[r].Contains(new Vector2(grass.position.x, grass.position.z)))
                    {
                        grassPatches[r].Instances.Add(grass);
                        dirty[r] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < dirty.Length; ++i)
            {
                if (dirty[i] == true)
                {
                    GrassPatches[i].RecalculateBounds();
                    GrassPatches[i].Changed();
                }
            }
        }

        public List<GGrassInstance> GetGrassInstances()
        {
            List<GGrassInstance> instances = new List<GGrassInstance>();
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                instances.AddRange(GrassPatches[i].Instances);
            }
            return instances;
        }

        public void ClearGrassInstances()
        {
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                GrassPatches[i].ClearInstances();
            }
        }

        public void SetTreeRegionDirty(Rect uvRect)
        {
            TreeDirtyRegions.Add(uvRect);
        }

        public void SetTreeRegionDirty(IEnumerable<Rect> uvRects)
        {
            TreeDirtyRegions.AddRange(uvRects);
        }

        public Rect[] GetTreeDirtyRegions()
        {
            return TreeDirtyRegions.ToArray();
        }

        public void ClearTreeDirtyRegions()
        {
            TreeDirtyRegions.Clear();
        }

        public void SetGrassRegionDirty(Rect uvRect)
        {
            GrassDirtyRegions.Add(uvRect);
        }

        public void SetGrassRegionDirty(IEnumerable<Rect> uvRects)
        {
            GrassDirtyRegions.AddRange(uvRects);
        }

        public Rect[] GetGrassDirtyRegions()
        {
            return GrassDirtyRegions.ToArray();
        }

        public void ClearGrassDirtyRegions()
        {
            GrassDirtyRegions.Clear();
        }

        public void CopyTo(GFoliage des)
        {
            des.Trees = Trees;
            des.TreeSnapMode = TreeSnapMode;
            des.TreeSnapLayerMask = TreeSnapLayerMask;
            des.Grasses = Grasses;
            des.GrassSnapMode = GrassSnapMode;
            des.GrassSnapLayerMask = GrassSnapLayerMask;
            des.PatchGridSize = PatchGridSize;
        }

        public void OnBeforeSerialize()
        {
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                GrassPatches[i].Serialize();
            }
            GCompressor.CleanUp();
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                GrassPatches[i].Deserialize();
            }
            GCompressor.CleanUp();
        }

        public void Internal_UpgradeGrassSerializeVersion()
        {
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                GrassPatches[i].UpgradeSerializeVersion();
            }
            grassVersion = GVersionInfo.Number;
            Debug.Log("Successfully upgrade grass serialize to newer version!");
        }

        public void GrassAllChanged()
        {
            for (int i = 0; i < GrassPatches.Length; ++i)
            {
                GrassPatches[i].Changed();
            }
        }

        public void TreeAllChanged()
        {
            if (TerrainData != null)
            {
                TerrainData.InvokeTreeChanged();
            }
        }

        public void AddTreeInstances(IEnumerable<GTreeInstance> newInstances)
        {
            TreeInstances.AddRange(newInstances);
            TreeAllChanged();
        }

        public void RemoveTreeInstances(System.Predicate<GTreeInstance> condition)
        {
            int removedCount = TreeInstances.RemoveAll(condition);
            if (removedCount > 0)
            {
                TreeAllChanged();
            }
        }

        public void ClearTreeInstances()
        {
            TreeInstances.Clear();
            TreeAllChanged();
        }

        public int GetTreeMemStats()
        {
            return TreeInstances.Count * GTreeInstance.GetStructSize();
        }

        public int GetGrassMemStats()
        {
            int memory = 0;
            if (grassPatches != null)
            {
                for (int i = 0; i < grassPatches.Length; ++i)
                {
                    if (grassPatches[i] != null)
                    {
                        memory += grassPatches[i].GetMemStats();
                    }
                }
            }
            return memory;
        }

        public NativeArray<Vector2> GetTreesPositionArray(Allocator allocator = Allocator.TempJob)
        {
            List<GTreeInstance> trees = TreeInstances;
            int treeCount = trees.Count;
            NativeArray<Vector2> positions = new NativeArray<Vector2>(treeCount, allocator, NativeArrayOptions.UninitializedMemory);
            Vector2 pos = Vector2.zero;

            for (int i = 0; i < treeCount; ++i)
            {
                GTreeInstance t = trees[i];
                pos.Set(t.position.x, t.position.z);
                positions[i] = pos;
            }

            return positions;
        }
    }
}
#endif
