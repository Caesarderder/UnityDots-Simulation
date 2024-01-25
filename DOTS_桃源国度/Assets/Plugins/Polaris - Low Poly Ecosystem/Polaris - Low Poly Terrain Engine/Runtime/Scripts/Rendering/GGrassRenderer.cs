#if GRIFFIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin.Rendering
{
    public class GGrassRenderer
    {
        public delegate void ConfiguringMaterialHandler(GStylizedTerrain terrain, int prototypeIndex, MaterialPropertyBlock propertyBlock);
        public static event ConfiguringMaterialHandler ConfiguringMaterial;

        private GStylizedTerrain terrain;

        private Camera camera;
        private Plane[] frustum;
        private int[] cellCullResults;
        private int[] cellCulledFrameCount;
        private float[] cellSqrDistanceToCam;
        private Bounds[] cellWorldBounds;
        private List<int> visibleCells;
        private List<int> cellToProcess;

        private const int CULLED = 0;
        private const int VISIBLE = 1;
        private const int FRAME_COUNT_TO_UNLOAD_CELL = 100;

        private Matrix4x4 normalizedToLocalMatrix;
        private Matrix4x4 localToWorldMatrix;
        private Matrix4x4 normalizedToWorldMatrix;

        private float grassDistance;
        private Vector3 terrainPosition;
        private Vector3 terrainSize;
        private float cullBias;

        private GGrassPatch[] cells;
        private GGrassPatchData[] cellsData;
        private GGrassPatchNativeData[] cellsNativeData;
        private List<GGrassPrototype> prototypes;
        private MaterialPropertyBlock[] propertyBlocks;
        private IGGrassMaterialConfigurator materialConfigurator;

        private Mesh[] baseMeshes;
        private Material[] materials;

        private const int BATCH_MAX_INSTANCE_COUNT = 1023;

        private bool willIgnoreCellLimit;

        public GGrassRenderer(GStylizedTerrain terrain)
        {
            this.terrain = terrain;
            RecalculateCellBounds();
        }

        private void RecalculateCellBounds()
        {
            GGrassPatch[] cells = terrain.TerrainData.Foliage.GrassPatches;
            for (int i = 0; i < cells.Length; ++i)
            {
                cells[i].RecalculateBounds();
            }
        }

        public void Render(Camera cam)
        {
            try
            {
                if (GRuntimeSettings.Instance.isEditingGeometry)
                    return;
                if (!CheckSystemInstancingAvailable())
                    return;
                InitFrame(cam);
                if (CullTerrain())
                    return;
                CullCells();
                CalculateAndCacheTransforms();
                BuildBatches();
                ConfigureMaterial();
                Submit();
            }
            catch (GSkipFrameException)
            { }

            CleanUpFrame();
        }

        private bool CheckSystemInstancingAvailable()
        {
            return SystemInfo.supportsInstancing;
        }

        private void InitFrame(Camera cam)
        {
            if (cullBias != GRuntimeSettings.Instance.renderingDefault.grassCullBias)
            {
                ClearAllCells();
            }

            cullBias = GRuntimeSettings.Instance.renderingDefault.grassCullBias;
            terrainPosition = terrain.transform.position;
            terrainSize = terrain.TerrainData.Geometry.Size;
            grassDistance = terrain.TerrainData.Rendering.GrassDistance;
            cells = terrain.TerrainData.Foliage.GrassPatches;
            willIgnoreCellLimit =
                cellsData == null ||
                GRuntimeSettings.Instance.isEditingFoliage;

            if (cellsData == null || cellsData.Length != cells.Length)
            {
                cellsData = new GGrassPatchData[cells.Length];
            }
            for (int i = 0; i < cellsData.Length; ++i)
            {
                if (cellsData[i] == null)
                {
                    cellsData[i] = new GGrassPatchData();
                }
            }

            if (cellsNativeData == null || cellsNativeData.Length != cells.Length)
            {
                cellsNativeData = new GGrassPatchNativeData[cells.Length];
            }

            if (visibleCells == null)
            {
                visibleCells = new List<int>();
            }
            if (cellToProcess == null)
            {
                cellToProcess = new List<int>();
            }

            if (terrain.TerrainData.Foliage.Grasses != null)
            {
                prototypes = terrain.TerrainData.Foliage.Grasses.Prototypes;
                if (propertyBlocks == null || propertyBlocks.Length != prototypes.Count)
                {
                    propertyBlocks = new MaterialPropertyBlock[prototypes.Count];
                }
            }
            else
            {
                prototypes = new List<GGrassPrototype>();
            }

            if (materialConfigurator == null)
            {
                materialConfigurator = System.Activator.CreateInstance<GSimpleGrassMaterialConfigurator>();
            }

            normalizedToLocalMatrix = Matrix4x4.Scale(terrainSize);
            localToWorldMatrix = terrain.transform.localToWorldMatrix;
            normalizedToWorldMatrix = localToWorldMatrix * normalizedToLocalMatrix;

            camera = cam;
            if (frustum == null)
            {
                frustum = new Plane[6];
            }
            GFrustumUtilities.Calculate(camera, frustum, grassDistance);

            if (cellCullResults == null || cellCullResults.Length != cells.Length)
            {
                cellCullResults = new int[cells.Length];
            }

            if (cellCulledFrameCount == null || cellCulledFrameCount.Length != cells.Length)
            {
                cellCulledFrameCount = new int[cells.Length];
            }

            if (cellSqrDistanceToCam == null || cellSqrDistanceToCam.Length != cells.Length)
            {
                cellSqrDistanceToCam = new float[cells.Length];
            }

            if (cellWorldBounds == null || cellWorldBounds.Length != cells.Length)
            {
                cellWorldBounds = new Bounds[cells.Length];
                for (int i = 0; i < cells.Length; ++i)
                {
                    cellWorldBounds[i] = new Bounds()
                    {
                        center = normalizedToWorldMatrix.MultiplyPoint(cells[i].bounds.center),
                        size = normalizedToWorldMatrix.MultiplyVector(cells[i].bounds.size) + Vector3.one * cullBias
                    };
                }
            }

            GUtilities.EnsureArrayLength(ref baseMeshes, prototypes.Count);
            GUtilities.EnsureArrayLength(ref materials, prototypes.Count);
            for (int i = 0; i < prototypes.Count; ++i)
            {
                baseMeshes[i] = prototypes[i].GetBaseMesh();
                materials[i] =
                    prototypes[i].Shape == GGrassShape.DetailObject ?
                    prototypes[i].DetailMaterial :
                    GGrassMaterialProvider.GetMaterial(terrain.TerrainData.Foliage.EnableInteractiveGrass, prototypes[i].IsBillboard);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the terrain is culled</returns>
        private bool CullTerrain()
        {
            bool prototypeCountTest = prototypes.Count > 0;
            if (!prototypeCountTest)
                return true;

            bool nonZeroDistanceTest = terrain.TerrainData.Rendering.GrassDistance > 0;
            if (!nonZeroDistanceTest)
                return true;

            bool frustumTest = GeometryUtility.TestPlanesAABB(frustum, terrain.Bounds);
            if (!frustumTest)
                return true;

            return false;
        }

        private void CullCells()
        {
            visibleCells.Clear();
            cellToProcess.Clear();

            Vector3 camWorldPos = camera.transform.position;
            for (int i = 0; i < cells.Length; ++i)
            {
                if (cells[i].InstanceCount == 0)
                {
                    cellCullResults[i] = CULLED;
                }
                else
                {
                    int cullResult = GeometryUtility.TestPlanesAABB(frustum, cellWorldBounds[i]) ? VISIBLE : CULLED;
                    cellCullResults[i] = cullResult;
                    if (cullResult == VISIBLE)
                    {
                        visibleCells.Add(i);
                    }
                }

                if (cellCullResults[i] == CULLED)
                {
                    cellCulledFrameCount[i] += 1;
                }
                else
                {
                    cellCulledFrameCount[i] = 0;
                }
                cellSqrDistanceToCam[i] = Vector3.SqrMagnitude(cellWorldBounds[i].center - camWorldPos);
            }

            for (int i = 0; i < visibleCells.Count; ++i)
            {
                int cellIndex = visibleCells[i];
                if (cellsData[cellIndex].instancedBatches == null)
                {
                    cellToProcess.Add(cellIndex);
                    if (cellToProcess.Count == GRuntimeSettings.Instance.renderingDefault.grassCellToProcessPerFrame &&
                        !willIgnoreCellLimit)
                    {
                        break;
                    }
                }
            }
        }

        private void CalculateAndCacheTransforms()
        {
            if (cellToProcess.Count == 0)
                return;
            bool willSkipFrame = false;

            try
            {
                NativeArray<float> prototypePivotOffset = new NativeArray<float>(prototypes.Count, Allocator.TempJob);
                NativeArray<Vector3> prototypeSize = new NativeArray<Vector3>(prototypes.Count, Allocator.TempJob);
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    prototypePivotOffset[i] = prototypes[i].pivotOffset;
                    prototypeSize[i] = prototypes[i].size;
                }

                JobHandle[] handles = new JobHandle[cellToProcess.Count];
                for (int i = 0; i < cellToProcess.Count; ++i)
                {
                    int cellIndex = cellToProcess[i];
                    GGrassPatch cell = cells[cellIndex];
                    GGrassPatchNativeData nativeData = new GGrassPatchNativeData(cell.Instances);
                    cellsNativeData[cellIndex] = nativeData;

                    GCalculateGrassTransformJob job = new GCalculateGrassTransformJob()
                    {
                        instances = nativeData.instances,
                        transforms = nativeData.trs,
                        prototypePivotOffset = prototypePivotOffset,
                        prototypeSize = prototypeSize,
                        terrainSize = terrainSize,
                        terrainPos = terrainPosition
                    };
                    //handles[i] = job.Schedule();
                    handles[i] = job.Schedule(nativeData.instances.Length, 100);
                }

                GJobUtilities.CompleteAll(handles);

                prototypePivotOffset.Dispose();
                prototypeSize.Dispose();
            }
            catch (System.InvalidOperationException)
            {
                willSkipFrame = true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            if (willSkipFrame)
            {
                throw new GSkipFrameException();
            }
        }

        private void BuildBatches()
        {
            JobHandle[] handles = new JobHandle[cellToProcess.Count];
            for (int i = 0; i < cellToProcess.Count; ++i)
            {
                int cellIndex = cellToProcess[i];
                GGrassPatchNativeData nativeData = cellsNativeData[cellIndex];

                //GGrassPatch cell = cellToProcess[i];
                GBuildInstancedBatchJob job = new GBuildInstancedBatchJob()
                {
                    instances = nativeData.instances,
                    batchMetadata = nativeData.metadata,
                    maxLength = BATCH_MAX_INSTANCE_COUNT
                };
                handles[i] = job.Schedule();
            }

            GJobUtilities.CompleteAll(handles);

            for (int i = 0; i < cellToProcess.Count; ++i)
            {
                int cellIndex = cellToProcess[i];
                CreateInstancedBatches(cellIndex);
            }
        }

        private void CreateInstancedBatches(int cellIndex)
        {
            GGrassPatchNativeData nativeData = cellsNativeData[cellIndex];

            NativeArray<Matrix4x4> trs = nativeData.trs;
            NativeArray<Vector3Int> metadata = nativeData.metadata;

            GInstancedBatch[] batches = new GInstancedBatch[metadata[0].z];
            for (int i = 0; i < batches.Length; ++i)
            {
                int prototypeIndex = metadata[i + 1].x;
                int startIndex = metadata[i + 1].y;
                int length = metadata[i + 1].z;
                int indexLimit = startIndex + length;

                GInstancedBatch batch = new GInstancedBatch(BATCH_MAX_INSTANCE_COUNT);
                batch.prototypeIndex = prototypeIndex;
                for (int j = startIndex; j < indexLimit; ++j)
                {
                    batch.AddTransform(trs[j]);
                }

                batches[i] = batch;
            }

            cellsData[cellIndex].instancedBatches = batches;
        }

        private void ConfigureMaterial()
        {
            for (int i = 0; i < prototypes.Count; ++i)
            {
                if (propertyBlocks[i] == null)
                    propertyBlocks[i] = new MaterialPropertyBlock();
                propertyBlocks[i].Clear();
                materialConfigurator.Configure(terrain, i, propertyBlocks[i]);
                if (ConfiguringMaterial != null)
                {
                    ConfiguringMaterial.Invoke(terrain, i, propertyBlocks[i]);
                }
            }
        }

        private void Submit()
        {
            for (int i = 0; i < visibleCells.Count; ++i)
            {
                int cellIndex = visibleCells[i];
                Submit(cellIndex);
            }
        }

        private void Submit(int cellIndex)
        {
            GGrassPatchData data = cellsData[cellIndex];
            if (data == null)
                return;
            GInstancedBatch[] batches = data.instancedBatches;
            if (batches == null)
                return;
            for (int i = 0; i < batches.Length; ++i)
            {
                GInstancedBatch b = batches[i];
                if (b.prototypeIndex >= prototypes.Count)
                    continue;

                GGrassPrototype proto = prototypes[b.prototypeIndex];
                MaterialPropertyBlock propertyBlock = propertyBlocks[b.prototypeIndex];

                Mesh baseMesh = baseMeshes[b.prototypeIndex];
                if (baseMesh == null)
                    continue;

                Material material = materials[b.prototypeIndex];
                if (material == null || !material.enableInstancing)
                    continue;

                Graphics.DrawMeshInstanced(
                    baseMesh,
                    0,
                    material,
                    b.transforms,
                    b.instanceCount,
                    propertyBlock,
                    proto.shadowCastingMode,
                    proto.receiveShadow,
                    proto.layer,
                    camera,
                    LightProbeUsage.BlendProbes);
            }
        }

        private void CleanUpFrame()
        {
            if (cellsNativeData != null)
            {
                for (int i = 0; i < cellsNativeData.Length; ++i)
                {
                    if (cellsNativeData[i] != null)
                    {
                        cellsNativeData[i].Dispose();
                        cellsNativeData[i] = null;
                    }
                }
            }

            UnloadInactiveCells();
        }

        private void UnloadInactiveCells()
        {
            float sqrRenderDistance = grassDistance * grassDistance;
            for (int i = 0; i < cells.Length; ++i)
            {
                if (cellCulledFrameCount[i] >= FRAME_COUNT_TO_UNLOAD_CELL &&
                    cellSqrDistanceToCam[i] >= sqrRenderDistance)
                {
                    ClearCellData(i);
                }
            }
        }

        internal void ClearCellData(int index)
        {
            if (cellsData != null)
            {
                cellsData[index] = null;
            }
        }

        private void CalculateCellWorldBounds(int index)
        {
            if (cellWorldBounds != null)
            {
                cellWorldBounds[index] = new Bounds()
                {
                    center = normalizedToWorldMatrix.MultiplyPoint(cells[index].bounds.center),
                    size = normalizedToWorldMatrix.MultiplyVector(cells[index].bounds.size)
                };
            }
        }

        internal void ClearAllCells()
        {
            cells = null;
            cellsData = null;
            cellWorldBounds = null;
            RecalculateCellBounds();
        }

        internal void CleanUp()
        {
            ClearAllCells();
        }

        internal void OnCellChanged(int index)
        {
            ClearCellData(index);
            CalculateCellWorldBounds(index);
        }

        internal void OnPatchGridSizeChanged()
        {
            ClearAllCells();
        }

        internal void OnPrototypeGroupChanged()
        {
            ClearAllCells();
        }
    }
}
#endif
