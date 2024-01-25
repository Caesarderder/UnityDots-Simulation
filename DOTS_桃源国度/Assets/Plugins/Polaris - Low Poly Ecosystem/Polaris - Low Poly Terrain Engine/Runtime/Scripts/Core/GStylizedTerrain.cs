#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using Pinwheel.Griffin.TextureTool;
using UnityEngine.Rendering;
using Pinwheel.Griffin.Rendering;
using Unity.Collections;
using Unity.Jobs;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if __MICROSPLAT_POLARIS__
using JBooth.MicroSplat;
#endif
#if UNITY_EDITOR && GRIFFIN_EDITOR_COROUTINES
using Unity.EditorCoroutines.Editor;
#endif

namespace Pinwheel.Griffin
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GStylizedTerrain : MonoBehaviour
    {
        public const string RAYCAST_LAYER = "POLARIS RAYCAST";

        public delegate void HeightMapProcessCallback(Texture2D heightmap);
        public event HeightMapProcessCallback PreProcessHeightMap;
        public event HeightMapProcessCallback PostProcessHeightMap;

        private static HashSet<GStylizedTerrain> activeTerrainSet;
        private static HashSet<GStylizedTerrain> ActiveTerrainSet
        {
            get
            {
                if (activeTerrainSet == null)
                    activeTerrainSet = new HashSet<GStylizedTerrain>();
                return activeTerrainSet;
            }
        }

        public static IEnumerable<GStylizedTerrain> ActiveTerrains
        {
            get
            {
                return ActiveTerrainSet;
            }
        }

        [SerializeField]
        internal GTerrainData terrainData;
        public GTerrainData TerrainData
        {
            get
            {
                return terrainData;
            }
            set
            {
                GTerrainData oldData = terrainData;
                GTerrainData newData = value;
                terrainData = newData;

                if (oldData == null && newData != null)
                {
                    newData.Dirty += OnTerrainDataDirty;
                    newData.GrassPatchChanged += OnGrassPatchChanged;
                    newData.GrassPatchGridSizeChanged += OnGrassPatchGridSizeChanged;
                    newData.TreeChanged += OnTreeChanged;
                    newData.GrassPrototypeGroupChanged += OnGrassPrototypeGroupChanged;

                    if (newData.Geometry.StorageMode == GGeometry.GStorageMode.SaveToAsset)
                    {
                        newData.Geometry.SetRegionDirty(GCommon.UnitRect);
                        newData.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                        newData.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                        OnTerrainDataDirty(GTerrainData.DirtyFlags.All);
                    }
                    else
                    {
                        newData.Geometry.SetRegionDirty(GCommon.UnitRect);
                        OnTerrainDataDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                    }
                }
                else if (oldData != null && newData != null && oldData != newData)
                {
                    oldData.Dirty -= OnTerrainDataDirty;
                    oldData.GrassPatchChanged -= OnGrassPatchChanged;
                    oldData.GrassPatchGridSizeChanged -= OnGrassPatchGridSizeChanged;
                    oldData.TreeChanged -= OnTreeChanged;
                    oldData.GrassPrototypeGroupChanged -= OnGrassPrototypeGroupChanged;

                    newData.Dirty += OnTerrainDataDirty;
                    newData.GrassPatchChanged += OnGrassPatchChanged;
                    newData.GrassPatchGridSizeChanged += OnGrassPatchGridSizeChanged;
                    newData.TreeChanged += OnTreeChanged;
                    newData.GrassPrototypeGroupChanged += OnGrassPrototypeGroupChanged;

                    if (newData.Geometry.StorageMode == GGeometry.GStorageMode.SaveToAsset)
                    {
                        newData.Geometry.SetRegionDirty(GCommon.UnitRect);
                        newData.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                        newData.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                        OnTerrainDataDirty(GTerrainData.DirtyFlags.All);
                    }
                    else
                    {
                        newData.Geometry.SetRegionDirty(GCommon.UnitRect);
                        OnTerrainDataDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                    }
                }
                else if (oldData != null && newData == null)
                {
                    oldData.Dirty -= OnTerrainDataDirty;
                    oldData.GrassPatchChanged -= OnGrassPatchChanged;
                    oldData.GrassPatchGridSizeChanged -= OnGrassPatchGridSizeChanged;
                    oldData.TreeChanged -= OnTreeChanged;
                    oldData.GrassPrototypeGroupChanged -= OnGrassPrototypeGroupChanged;

                    Transform root = GetChunkRoot();
                    if (root != null)
                    {
                        GUtilities.DestroyGameobject(root.gameObject);
                    }
                }
            }
        }

        [SerializeField]
        private GStylizedTerrain topNeighbor;
        public GStylizedTerrain TopNeighbor
        {
            get
            {
                return topNeighbor;
            }
            set
            {
                topNeighbor = value;
                if (topNeighbor != null)
                {
                    topTerrainData = topNeighbor.TerrainData;
                }
            }
        }

        [SerializeField]
        private GTerrainData topTerrainData;
        public GTerrainData TopTerrainData
        {
            get
            {
                return topTerrainData;
            }
        }

        [SerializeField]
        private GStylizedTerrain bottomNeighbor;
        public GStylizedTerrain BottomNeighbor
        {
            get
            {
                return bottomNeighbor;
            }
            set
            {
                bottomNeighbor = value;
                if (bottomNeighbor != null)
                {
                    bottomTerrainData = bottomNeighbor.TerrainData;
                }
            }
        }

        [SerializeField]
        private GTerrainData bottomTerrainData;
        public GTerrainData BottomTerrainData
        {
            get
            {
                return bottomTerrainData;
            }
        }

        [SerializeField]
        private GStylizedTerrain leftNeighbor;
        public GStylizedTerrain LeftNeighbor
        {
            get
            {
                return leftNeighbor;
            }
            set
            {
                leftNeighbor = value;
                if (leftNeighbor != null)
                {
                    leftTerrainData = leftNeighbor.TerrainData;
                }
            }
        }

        [SerializeField]
        private GTerrainData leftTerrainData;
        public GTerrainData LeftTerrainData
        {
            get
            {
                return leftTerrainData;
            }
        }

        [SerializeField]
        private GStylizedTerrain rightNeighbor;
        public GStylizedTerrain RightNeighbor
        {
            get
            {
                return rightNeighbor;
            }
            set
            {
                rightNeighbor = value;
                if (rightNeighbor != null)
                {
                    rightTerrainData = rightNeighbor.TerrainData;
                }
            }
        }

        [SerializeField]
        private GTerrainData rightTerrainData;
        public GTerrainData RightTerrainData
        {
            get
            {
                return rightTerrainData;
            }
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
                groupId = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private bool autoConnect = true;
        public bool AutoConnect
        {
            get
            {
                return autoConnect;
            }
            set
            {
                autoConnect = value;
            }
        }

        [SerializeField]
        public float geometryVersion;
        public const float GEOMETRY_VERSION_CHUNK_POSITION_AT_CHUNK_CENTER = 245;

        public Bounds Bounds
        {
            get
            {
                Bounds b = new Bounds();
                Vector3 size = terrainData != null ?
                    new Vector3(terrainData.Geometry.width, terrainData.Geometry.height, terrainData.Geometry.length) :
                    Vector3.zero;
                b.size = size;
                b.center = transform.position + size * 0.5f;
                return b;
            }
        }

        public Rect Rect
        {
            get
            {
                Rect r = new Rect();
                r.size = TerrainData != null ?
                                    new Vector2(TerrainData.Geometry.Width, TerrainData.Geometry.Length) :
                                    Vector2.zero;
                r.center = new Vector2(transform.position.x + r.size.x * 0.5f, transform.position.z + r.size.y * 0.5f);
                return r;
            }
        }

        private RenderTexture heightMap;
        private RenderTexture normalMap;
        private RenderTexture normalMapInterpolated;
        private RenderTexture normalMapPerPixel;
        private RenderTexture grassVectorFieldMap;
        private RenderTexture grassVectorFieldMapTmp;

        private GTreeRenderer treeRenderer;
        public GGrassRenderer grassRenderer;

        [SerializeField]
        private Vector3 lastPosition;

        public Transform GetChunkRoot()
        {
            Transform root = transform.Find(GCommon.CHUNK_ROOT_NAME);
            return root;
        }

        public Transform GetOrCreateChunkRoot()
        {
            Transform root = GUtilities.GetChildrenWithName(transform, GCommon.CHUNK_ROOT_NAME);

#if UNITY_EDITOR
            root.hideFlags = GEditorSettings.Instance.general.showGeometryChunkInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy;
#if !UNITY_2022_2_OR_NEWER
            StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(root.gameObject);
            GameObjectUtility.SetStaticEditorFlags(root.gameObject, staticFlags | StaticEditorFlags.NavigationStatic);
#endif
#endif
            return root;
        }

        public static void ConnectAdjacentTiles()
        {
            IEnumerator<GStylizedTerrain> terrains = ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                t.ConnectNeighbor();
            }
        }

        public void ConnectNeighbor()
        {
            if (TerrainData == null)
                return;
            if (!AutoConnect)
                return;
            Vector2 sizeXZ = new Vector2(
                TerrainData.Geometry.Width,
                TerrainData.Geometry.Length);
            Vector2 posXZ = new Vector2(
                transform.position.x,
                transform.position.z);

            IEnumerator<GStylizedTerrain> terrains = ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    return;
                if (GroupId != t.GroupId)
                    continue;

                Vector2 neighborSizeXZ = new Vector2(
                    t.TerrainData.Geometry.Width,
                    t.TerrainData.Geometry.Length);
                Vector2 neighborPosXZ = new Vector2(
                    t.transform.position.x,
                    t.transform.position.z);
                Vector2 neighborCenter = neighborPosXZ + neighborSizeXZ * 0.5f;

                if (LeftNeighbor == null)
                {
                    Rect r = new Rect();
                    r.size = sizeXZ;
                    r.position = new Vector2(posXZ.x - sizeXZ.x, posXZ.y);
                    if (r.Contains(neighborCenter))
                    {
                        LeftNeighbor = t;
                        t.RightNeighbor = this;
                    }
                }
                if (TopNeighbor == null)
                {
                    Rect r = new Rect();
                    r.size = sizeXZ;
                    r.position = new Vector2(posXZ.x, posXZ.y + sizeXZ.y);
                    if (r.Contains(neighborCenter))
                    {
                        TopNeighbor = t;
                        t.BottomNeighbor = this;
                    }
                }
                if (RightNeighbor == null)
                {
                    Rect r = new Rect();
                    r.size = sizeXZ;
                    r.position = new Vector2(posXZ.x + sizeXZ.x, posXZ.y);
                    if (r.Contains(neighborCenter))
                    {
                        RightNeighbor = t;
                        t.LeftNeighbor = this;
                    }
                }
                if (BottomNeighbor == null)
                {
                    Rect r = new Rect();
                    r.size = sizeXZ;
                    r.position = new Vector2(posXZ.x, posXZ.y - sizeXZ.y);
                    if (r.Contains(neighborCenter))
                    {
                        BottomNeighbor = t;
                        t.TopNeighbor = this;
                    }
                }
            }
        }

        private void OnEnable()
        {
            ActiveTerrainSet.Add(this);
            //GStylizedTerrain.ConnectAdjacentTiles();
            ConnectNeighbor();
            GCommon.RegisterBeginRender(OnBeginCameraRendering);
            GCommon.RegisterBeginRenderSRP(OnBeginCameraRenderingSRP);
            GCommon.RegisterEndRender(OnEndCameraRendering);

#if __MICROSPLAT_POLARIS__
            PushControlTexturesToMicroSplat();
#endif

            if (TerrainData != null)
            {
                TerrainData.Dirty += OnTerrainDataDirty;
                TerrainData.GrassPatchChanged += OnGrassPatchChanged;
                TerrainData.GrassPatchGridSizeChanged += OnGrassPatchGridSizeChanged;
                TerrainData.TreeChanged += OnTreeChanged;
                TerrainData.GrassPrototypeGroupChanged += OnGrassPrototypeGroupChanged;
            }

            if (TerrainData != null)
            {
                TerrainData.Shading.UpdateMaterials();
            }

            if (TerrainData != null && TerrainData.Geometry.StorageMode == GGeometry.GStorageMode.GenerateOnEnable)
            {
                TerrainData.Geometry.SetRegionDirty(GCommon.UnitRect);
                OnTerrainDataDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                TerrainData.Geometry.ClearDirtyRegions();
            }

        }

        private void OnDisable()
        {
            ReleaseTemporaryTextures();
            ActiveTerrainSet.Remove(this);
            //GStylizedTerrain.ConnectAdjacentTiles();
            CleanUpChunks();

            GCommon.UnregisterBeginRender(OnBeginCameraRendering);
            GCommon.UnregisterBeginRenderSRP(OnBeginCameraRenderingSRP);
            GCommon.UnregisterEndRender(OnEndCameraRendering);

            if (TerrainData != null)
            {
                TerrainData.Dirty -= OnTerrainDataDirty;
                TerrainData.GrassPatchChanged -= OnGrassPatchChanged;
                TerrainData.GrassPatchGridSizeChanged -= OnGrassPatchGridSizeChanged;
                TerrainData.TreeChanged -= OnTreeChanged;
                TerrainData.GrassPrototypeGroupChanged -= OnGrassPrototypeGroupChanged;
                TerrainData.CleanUp();
            }

            if (grassRenderer != null)
            {
                grassRenderer.CleanUp();
            }
            if (treeRenderer != null)
            {
                treeRenderer.CleanUp();
            }
        }

        private void OnDestroy()
        {
            ReleaseTemporaryTextures();
            CleanUpChunks();
        }

        private void Reset()
        {
            geometryVersion = GVersionInfo.Number;
        }

        private void ReleaseTemporaryTextures()
        {
            if (heightMap != null)
            {
                heightMap.Release();
                GUtilities.DestroyObject(heightMap);
            }
            if (normalMap != null)
            {
                normalMap.Release();
                GUtilities.DestroyObject(normalMap);
            }
            if (normalMapInterpolated != null)
            {
                normalMapInterpolated.Release();
                GUtilities.DestroyObject(normalMapInterpolated);
            }
            if (normalMapPerPixel != null)
            {
                normalMapPerPixel.Release();
                GUtilities.DestroyObject(normalMapPerPixel);
            }
        }

        private void CleanUpChunks()
        {
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].CleanUpNonSerializedMeshes();
            }
        }

        private void OnGrassPatchChanged(int index)
        {
            if (grassRenderer != null)
            {
                grassRenderer.OnCellChanged(index);
            }
        }

        private void OnGrassPatchGridSizeChanged()
        {
            if (grassRenderer != null)
            {
                grassRenderer.OnPatchGridSizeChanged();
            }
        }

        private void OnTreeChanged()
        {
            if (treeRenderer != null)
            {
                treeRenderer.ResetTrees();
            }
        }

        private void OnGrassPrototypeGroupChanged()
        {
            if (grassRenderer != null)
            {
                grassRenderer.OnPrototypeGroupChanged();
            }
        }

        private void OnBeginCameraRendering(Camera cam)
        {
            if (transform.position != lastPosition &&
                (TerrainData.Rendering.DrawTrees || TerrainData.Rendering.DrawGrasses))
            {
                TerrainData.Foliage.TreeAllChanged();
                TerrainData.Foliage.GrassAllChanged();
                lastPosition = transform.position;
            }

            if (TerrainData != null &&
                transform.rotation == Quaternion.identity &&
                transform.localScale == Vector3.one &&
                GUtilities.IsSceneViewOrGameCamera(cam) &&
                !GUtilities.IsPrefabCamera(cam))
            {
                if (TerrainData.Rendering.DrawTrees)
                {
                    if (treeRenderer == null)
                    {
                        treeRenderer = new GTreeRenderer(this);
                    }
                    treeRenderer.Render(cam);
                }

                if (TerrainData.Rendering.DrawGrasses)
                {
                    if (grassRenderer == null)
                    {
                        grassRenderer = new GGrassRenderer(this);
                    }
                    grassRenderer.Render(cam);
                }
            }

#if UNITY_EDITOR
            if (GEditorSettings.Instance.general.debugMode)
            {
                DrawChunkUpdateDebug(cam);
            }

            if (GEditorSettings.Instance.topographic.enable)
            {
                DrawChunkTopographic(cam);
            }
#endif
        }

        private void OnBeginCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
        {
            OnBeginCameraRendering(cam);
        }

        private void OnEndCameraRendering(Camera cam)
        {

        }

        private void OnTerrainDataDirty(GTerrainData.DirtyFlags dirtyFlag)
        {
            if ((dirtyFlag & GTerrainData.DirtyFlags.Geometry) == GTerrainData.DirtyFlags.Geometry)
            {
                OnGeometryDirty();
            }
            else if ((dirtyFlag & GTerrainData.DirtyFlags.GeometryTimeSliced) == GTerrainData.DirtyFlags.GeometryTimeSliced)
            {
                if (TerrainData.Geometry.AllowTimeSlicedGeneration)
                {
                    OnGeometryTimeSlicedDirty();
                }
                else
                {
                    OnGeometryDirty();
                }
            }

            if ((dirtyFlag & GTerrainData.DirtyFlags.Shading) == GTerrainData.DirtyFlags.Shading)
            {
                OnShadingDirty();
            }

            if ((dirtyFlag & GTerrainData.DirtyFlags.Rendering) == GTerrainData.DirtyFlags.Rendering)
            {
                OnRenderingDirty();
            }

            if ((dirtyFlag & GTerrainData.DirtyFlags.Foliage) == GTerrainData.DirtyFlags.Foliage)
            {
                OnFoliageDirty();
            }
            GUtilities.MarkCurrentSceneDirty();
        }

        private void OnGeometryDirty()
        {
            geometryVersion = GVersionInfo.Number;

            if (PreProcessHeightMap != null)
                PreProcessHeightMap.Invoke(TerrainData.Geometry.HeightMap);

            ReleaseTemporaryTextures();
            GTerrainChunk[] chunks = InitChunks();
            List<GTerrainChunk> chunksToUpdate = ExtractDirtyChunks(chunks);

            int lodCount = TerrainData.Geometry.LODCount;
            for (int lod = 0; lod < lodCount; ++lod)
            {
                GenerateChunks(chunksToUpdate, lod);
            }

            if (PostProcessHeightMap != null)
            {
                PostProcessHeightMap.Invoke(TerrainData.Geometry.HeightMap);
            }
        }

        private void OnGeometryTimeSlicedDirty()
        {
            geometryVersion = GVersionInfo.Number;

            //if (PreProcessHeightMap != null)
            //    PreProcessHeightMap.Invoke(TerrainData.Geometry.HeightMap);

            GTerrainChunk[] chunks = InitChunks();
            List<GTerrainChunk> chunksToUpdate = ExtractDirtyChunks(chunks);
            SortChunkByDistance(
                Camera.main ? Camera.main.transform.position : Vector3.zero,
                chunksToUpdate);

            if (Application.isPlaying)
            {
                StartCoroutine(GenerateChunksTimeSliced(chunksToUpdate));
            }
#if UNITY_EDITOR
            else if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
#if GRIFFIN_EDITOR_COROUTINES
                EditorCoroutineUtility.StartCoroutine(GenerateChunksTimeSliced(chunksToUpdate), this);
#else
                int lodCount = TerrainData.Geometry.LODCount;
                for (int lod = 0; lod < lodCount; ++lod)
                {
                    GenerateChunks(chunksToUpdate, lod);
                }
#endif
            }
#endif

            //if (PostProcessHeightMap != null)
            //{
            //    PostProcessHeightMap.Invoke(TerrainData.Geometry.HeightMap);
            //}
        }

        private void GenerateChunks(List<GTerrainChunk> chunksToUpdate, int lod)
        {
            int meshBaseResolution = TerrainData.Geometry.MeshBaseResolution;
            int meshResolution = TerrainData.Geometry.MeshResolution;

            InitGeneration(chunksToUpdate, meshResolution);
            CreateBaseSubdivTree(chunksToUpdate, meshBaseResolution, meshResolution, lod);
            SplitBaseTreeForDynamicPolygon(chunksToUpdate, meshBaseResolution, meshResolution, lod);
            if (lod == 0)
            {
                if (meshBaseResolution != meshResolution)
                {
                    StitchSeam(chunksToUpdate, meshBaseResolution, meshResolution);
                }
            }
            else
            {
                StitchSeamLOD(chunksToUpdate, meshBaseResolution, meshResolution, lod);
            }
            CountLeafNode(chunksToUpdate, meshBaseResolution, meshResolution, lod);
            CreateVertex(chunksToUpdate, meshBaseResolution, meshResolution, lod);
            UpdateMesh(chunksToUpdate, lod);
            if (lod == 0)
            {
                BakeCollisionMesh(chunksToUpdate);
                UpdateCollisionMesh(chunksToUpdate);
            }
            CleanUpGeneration(chunksToUpdate);

            TerrainData.Geometry.ClearDirtyRegions();
        }

        private void InitGeneration(List<GTerrainChunk> chunksToUpdate, int meshResolution)
        {
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                chunksToUpdate[i].InitSubdivTreeNativeContainers(meshResolution);
            }
        }

        private void CreateBaseSubdivTree(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                GCreateBaseTreeJob j = chunksToUpdate[i].GetCreateBaseSubdivTreeJob(
                    meshBaseResolution,
                    meshResolution,
                    lod);
                jobHandles[i] = j.Schedule();
            }
            GJobUtilities.CompleteAll(jobHandles);
        }

        private void SplitBaseTreeForDynamicPolygon(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            TerrainData.Geometry.Internal_CreateNewSubDivisionMap();
            GTextureNativeDataDescriptor<Color32> subdivMap = new GTextureNativeDataDescriptor<Color32>(TerrainData.Geometry.Internal_SubDivisionMap);
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                GSplitBaseTreeForDynamicPolygonJob j = chunksToUpdate[i].GetSplitBaseTreeForDynamicPolygonJob(
                    meshBaseResolution,
                    meshResolution,
                    lod,
                    subdivMap);
                jobHandles[i] = j.Schedule();
            }
            GJobUtilities.CompleteAll(jobHandles);
        }

        private void StitchSeam(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            int stitchSeamIteration = 0;
            int stitchSeamMaxIteration = GRuntimeSettings.Instance.geometryGeneration.triangulateIteration;
            bool newVertexCreated = true;
            List<NativeArray<bool>> markers = new List<NativeArray<bool>>();

            while (newVertexCreated && stitchSeamIteration <= stitchSeamMaxIteration)
            {
                GStitchSeamJob[] stitchJobs = new GStitchSeamJob[chunksToUpdate.Count];
                for (int i = 0; i < chunksToUpdate.Count; ++i)
                {
                    GTerrainChunk c = chunksToUpdate[i];

                    GTerrainChunk leftChunk = GetLeftNeighborChunk(c);
                    bool hasLeftMarkers = leftChunk != null;
                    NativeArray<bool> leftMarkers = hasLeftMarkers ? leftChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    //if (!hasLeftMarkers)
                    //{
                    markers.Add(leftMarkers);
                    //}

                    GTerrainChunk topChunk = GetTopNeighborChunk(c);
                    bool hasTopMarkers = topChunk != null;
                    NativeArray<bool> topMarkers = hasTopMarkers ? topChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    //if (!hasTopMarkers)
                    //{
                    markers.Add(topMarkers);
                    //}

                    GTerrainChunk rightChunk = GetRightNeighborChunk(c);
                    bool hasRightMarkers = rightChunk != null;
                    NativeArray<bool> rightMarkers = hasRightMarkers ? rightChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    //if (!hasRightMarkers)
                    //{
                    markers.Add(rightMarkers);
                    //}

                    GTerrainChunk bottomChunk = GetBottomNeighborChunk(c);
                    bool hasBottomMarkers = bottomChunk != null;
                    NativeArray<bool> bottomMarkers = hasBottomMarkers ? bottomChunk.GetVertexMarker() : new NativeArray<bool>(1, Allocator.TempJob);
                    //if (!hasBottomMarkers)
                    //{
                    markers.Add(bottomMarkers);
                    //}

                    GStitchSeamJob j = c.GetStitchSeamJob(
                        meshBaseResolution,
                        meshResolution,
                        hasLeftMarkers, leftMarkers,
                        hasTopMarkers, topMarkers,
                        hasRightMarkers, rightMarkers,
                        hasBottomMarkers, bottomMarkers
                        );
                    stitchJobs[i] = j;
                }
                for (int i = 0; i < stitchJobs.Length; ++i)
                {
                    jobHandles[i] = stitchJobs[i].Schedule();
                }

                GJobUtilities.CompleteAll(jobHandles);

                stitchSeamIteration += 1;
                int tmp = 0;
                for (int i = 0; i < chunksToUpdate.Count; ++i)
                {
                    tmp += chunksToUpdate[i].GetGenerationMetadata(GGeometryJobUtilities.METADATA_NEW_VERTEX_CREATED);
                }
                newVertexCreated = tmp > 0;
            }

            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                chunksToUpdate[i].CacheVertexMarker();
            }

            for (int i = 0; i < markers.Count; ++i)
            {
                GNativeArrayUtilities.Dispose(markers[i]);
            }
        }

        private void StitchSeamLOD(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            List<NativeArray<bool>> markers = new List<NativeArray<bool>>();
            int stitchSeamIteration = 0;
            int stitchSeamMaxIteration = GRuntimeSettings.Instance.geometryGeneration.triangulateIteration;
            bool newVertexCreated = true;

            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                GTerrainChunk c = chunksToUpdate[i];
                NativeArray<bool> markerLOD0 = c.GetVertexMarker();
                markers.Add(markerLOD0);
            }

            while (newVertexCreated && stitchSeamIteration <= stitchSeamMaxIteration)
            {
                for (int i = 0; i < chunksToUpdate.Count; ++i)
                {
                    GTerrainChunk c = chunksToUpdate[i];
                    GStitchSeamLODJob j = c.GetStitchSeamLODJob(
                        meshBaseResolution,
                        meshResolution,
                        lod,
                        markers[i]);
                    jobHandles[i] = j.Schedule();
                }
                GJobUtilities.CompleteAll(jobHandles);

                stitchSeamIteration += 1;
                int tmp = 0;
                for (int i = 0; i < chunksToUpdate.Count; ++i)
                {
                    tmp += chunksToUpdate[i].GetGenerationMetadata(GGeometryJobUtilities.METADATA_NEW_VERTEX_CREATED);
                }
                newVertexCreated = tmp > 0;
            }

            for (int i = 0; i < markers.Count; ++i)
            {
                GNativeArrayUtilities.Dispose(markers[i]);
            }
        }

        private void CountLeafNode(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                GCountLeafNodeJob j = chunksToUpdate[i].GetCountLeafNodeJob(meshBaseResolution, meshResolution, lod);
                jobHandles[i] = j.Schedule();
            }
            GJobUtilities.CompleteAll(jobHandles);
        }

        private void CreateVertex(
            List<GTerrainChunk> chunksToUpdate,
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            JobHandle[] jobHandles = new JobHandle[chunksToUpdate.Count];
            int displacementSeed = TerrainData.Geometry.DisplacementSeed;
            float displacementStrength = TerrainData.Geometry.DisplacementStrength;
            bool smoothNormal = TerrainData.Geometry.SmoothNormal;
            bool useSmoothNormalMask = TerrainData.Geometry.UseSmoothNormalMask;
            bool mergeUv = TerrainData.Geometry.MergeUv;
            GTextureNativeDataDescriptor<Color32> maskMap = new GTextureNativeDataDescriptor<Color32>((smoothNormal && useSmoothNormalMask) ? TerrainData.Mask.MaskMapOrDefault : null);
            GTextureNativeDataDescriptor<Color32>[] heightMapGrid = GetHeightMapGrid();
            GAlbedoToVertexColorMode albedoToVertexColor = TerrainData.Geometry.AlbedoToVertexColorMode;
            GTextureNativeDataDescriptor<Color32> albedoMap = new GTextureNativeDataDescriptor<Color32>(albedoToVertexColor != GAlbedoToVertexColorMode.None ? TerrainData.Shading.AlbedoMapOrDefault : null);
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                GCreateVertexJob j = chunksToUpdate[i].GetCreateVertexJob(
                    meshBaseResolution,
                    meshResolution,
                    lod,
                    displacementSeed,
                    displacementStrength,
                    smoothNormal,
                    useSmoothNormalMask,
                    mergeUv,
                    maskMap,
                    albedoToVertexColor,
                    albedoMap,
                    heightMapGrid);

                jobHandles[i] = j.Schedule();
            }
            GJobUtilities.CompleteAll(jobHandles);

            for (int i = 0; i < heightMapGrid.Length; ++i)
            {
                heightMapGrid[i].Dispose();
            }
            if (!smoothNormal || !useSmoothNormalMask)
            {
                maskMap.Dispose();
            }
            if (albedoToVertexColor == GAlbedoToVertexColorMode.None)
            {
                albedoMap.Dispose();
            }
        }

        private void UpdateMesh(
            List<GTerrainChunk> chunksToUpdate,
            int lod)
        {
            GAlbedoToVertexColorMode albedoToVertexColor = TerrainData.Geometry.AlbedoToVertexColorMode;
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                chunksToUpdate[i].UpdateMesh(lod, albedoToVertexColor);
                chunksToUpdate[i].Refresh();
            }
        }

        private void BakeCollisionMesh(List<GTerrainChunk> chunksToUpdate)
        {
            NativeArray<int> meshLod0 = new NativeArray<int>(chunksToUpdate.Count, Allocator.TempJob);
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                Mesh m = chunksToUpdate[i].GetMesh(0);
                if (m != null)
                {
                    meshLod0[i] = m.GetInstanceID();
                }
            }

            GBakeCollisionMeshJob job = new GBakeCollisionMeshJob();
            job.instanceIds = meshLod0;

            JobHandle h = job.Schedule(meshLod0.Length, 1);
            h.Complete();

            meshLod0.Dispose();
        }

        private void UpdateCollisionMesh(
            List<GTerrainChunk> chunksToUpdate)
        {
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                chunksToUpdate[i].UpdateCollisionMesh();
            }
        }

        private void CleanUpGeneration(List<GTerrainChunk> chunksToUpdate)
        {
            for (int i = 0; i < chunksToUpdate.Count; ++i)
            {
                chunksToUpdate[i].CleanUpNativeArrays();
            }
        }

        private IEnumerator GenerateChunksTimeSliced(List<GTerrainChunk> chunksToUpdate)
        {
            yield return null;
            ForceLOD(0);

            int coreCount = 1; //SystemInfo.processorCount - 1;
            List<GTerrainChunk> chunks = new List<GTerrainChunk>();

            int step = (chunksToUpdate.Count + coreCount - 1) / coreCount;
            for (int repeat = 0; repeat < 2; ++repeat) //generate 2 times for seams to stitch up
            {
                for (int i = 0; i < step; ++i)
                {
                    chunks.Clear();
                    for (int j = 0; j < coreCount; ++j)
                    {
                        int index = i * coreCount + j;
                        if (index < chunksToUpdate.Count)
                        {
                            chunks.Add(chunksToUpdate[index]);
                        }
                    }
                    GenerateChunks(chunks, 0);

                    yield return null;
                }
            }

            int lodCount = TerrainData.Geometry.LODCount;
            if (lodCount == 1)
                yield break;

            for (int i = 0; i < step; ++i)
            {
                chunks.Clear();
                for (int j = 0; j < coreCount; ++j)
                {
                    int index = i * coreCount + j;
                    if (index < chunksToUpdate.Count)
                    {
                        chunks.Add(chunksToUpdate[index]);
                    }
                }

                for (int lod = 1; lod < lodCount; ++lod)
                {
                    GenerateChunks(chunks, lod);
                }
                yield return null;
            }
            BakeCollisionMesh(chunksToUpdate);
            UpdateCollisionMesh(chunksToUpdate);
            ForceLOD(-1);
        }

        private List<GTerrainChunk> ExtractDirtyChunks(GTerrainChunk[] chunks)
        {
            List<GTerrainChunk> dirtyChunks = new List<GTerrainChunk>();
            Rect[] dirtyRegion = TerrainData.Geometry.GetDirtyRegions();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Rect uvRect = chunks[i].GetUvRange();
                for (int j = 0; j < dirtyRegion.Length; ++j)
                {
                    if (uvRect.Overlaps(dirtyRegion[j]))
                    {
                        dirtyChunks.Add(chunks[i]);
                        break;
                    }
                }
            }

            return dirtyChunks;
        }

        private GTextureNativeDataDescriptor<Color32>[] GetHeightMapGrid()
        {
            GTextureNativeDataDescriptor<Color32>[] descriptors = new GTextureNativeDataDescriptor<Color32>[9];

            //center
            if (TerrainData != null)
            {
                descriptors[4] = new GTextureNativeDataDescriptor<Color32>(TerrainData.Geometry.HeightMap);
            }
            else
            {
                descriptors[4] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            //left - top - right - bottom
            //if (LeftNeighbor != null && LeftNeighbor.gameObject.activeInHierarchy && LeftNeighbor.TerrainData != null)
            if (LeftTerrainData != null)
            {
                descriptors[3] = new GTextureNativeDataDescriptor<Color32>(LeftTerrainData.Geometry.HeightMap);
            }
            else
            {
                descriptors[3] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            if (TopTerrainData != null)
            {
                descriptors[7] = new GTextureNativeDataDescriptor<Color32>(TopTerrainData.Geometry.HeightMap);
            }
            else
            {
                descriptors[7] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            if (RightTerrainData != null)
            {
                descriptors[5] = new GTextureNativeDataDescriptor<Color32>(RightTerrainData.Geometry.HeightMap);
            }
            else
            {
                descriptors[5] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            if (BottomTerrainData != null)
            {
                descriptors[1] = new GTextureNativeDataDescriptor<Color32>(BottomTerrainData.Geometry.HeightMap);
            }
            else
            {
                descriptors[1] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            //BL
            if (BottomNeighbor != null && BottomNeighbor.LeftNeighbor != null)
            {
                if (BottomNeighbor.LeftNeighbor.TerrainData != null && BottomNeighbor.LeftNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[0] = new GTextureNativeDataDescriptor<Color32>(BottomNeighbor.LeftNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[0] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else if (LeftNeighbor != null && LeftNeighbor.BottomNeighbor != null)
            {
                if (LeftNeighbor.BottomNeighbor.TerrainData != null && LeftNeighbor.BottomNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[0] = new GTextureNativeDataDescriptor<Color32>(LeftNeighbor.BottomNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[0] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else
            {
                descriptors[0] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            //TL
            if (TopNeighbor != null && TopNeighbor.LeftNeighbor != null)
            {
                if (TopNeighbor.LeftNeighbor.TerrainData != null && TopNeighbor.LeftNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[6] = new GTextureNativeDataDescriptor<Color32>(TopNeighbor.LeftNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[6] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else if (LeftNeighbor != null && LeftNeighbor.TopNeighbor != null)
            {
                if (LeftNeighbor.TopNeighbor.TerrainData != null && LeftNeighbor.TopNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[6] = new GTextureNativeDataDescriptor<Color32>(LeftNeighbor.TopNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[6] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else
            {
                descriptors[6] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            //TR
            if (TopNeighbor != null && TopNeighbor.RightNeighbor != null)
            {
                if (TopNeighbor.RightNeighbor.TerrainData != null && TopNeighbor.RightNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[8] = new GTextureNativeDataDescriptor<Color32>(TopNeighbor.RightNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[8] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else if (RightNeighbor != null && RightNeighbor.TopNeighbor != null)
            {
                if (RightNeighbor.TopNeighbor.TerrainData != null && RightNeighbor.TopNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[8] = new GTextureNativeDataDescriptor<Color32>(RightNeighbor.TopNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[8] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else
            {
                descriptors[8] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            //BR
            if (BottomNeighbor != null && BottomNeighbor.RightNeighbor != null)
            {
                if (BottomNeighbor.RightNeighbor.TerrainData != null && BottomNeighbor.RightNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[2] = new GTextureNativeDataDescriptor<Color32>(BottomNeighbor.RightNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[2] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else if (RightNeighbor != null && RightNeighbor.BottomNeighbor != null)
            {
                if (RightNeighbor.BottomNeighbor.TerrainData != null && RightNeighbor.BottomNeighbor.gameObject.activeInHierarchy)
                {
                    descriptors[2] = new GTextureNativeDataDescriptor<Color32>(RightNeighbor.BottomNeighbor.TerrainData.Geometry.HeightMap);
                }
                else
                {
                    descriptors[2] = new GTextureNativeDataDescriptor<Color32>(null);
                }
            }
            else
            {
                descriptors[2] = new GTextureNativeDataDescriptor<Color32>(null);
            }

            return descriptors;
        }

        private GTerrainChunk GetLeftNeighborChunk(GTerrainChunk c)
        {
            int maxIndex = TerrainData.Geometry.ChunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.x > 0)
            {
                return c.Internal_NeighborChunks[0];
            }

            if (LeftNeighbor == null)
                return null;
            if (!LeftNeighbor.gameObject.activeInHierarchy)
                return null;

            List<GTerrainChunk> leftTerrainChunks = new List<GTerrainChunk>(LeftNeighbor.GetChunks());
            index.x = maxIndex;
            GTerrainChunk neighborChunk = leftTerrainChunks.Find(c0 => c0.Index == index);
            return neighborChunk;
        }

        private GTerrainChunk GetTopNeighborChunk(GTerrainChunk c)
        {
            int maxIndex = TerrainData.Geometry.ChunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.y < maxIndex)
            {
                return c.Internal_NeighborChunks[1];
            }

            if (TopNeighbor == null)
                return null;
            if (!TopNeighbor.gameObject.activeInHierarchy)
                return null;

            List<GTerrainChunk> topTerrainChunks = new List<GTerrainChunk>(TopNeighbor.GetChunks());
            index.y = 0;
            GTerrainChunk neighborChunk = topTerrainChunks.Find(c0 => c0.Index == index);
            return neighborChunk;
        }

        private GTerrainChunk GetRightNeighborChunk(GTerrainChunk c)
        {
            int maxIndex = TerrainData.Geometry.ChunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.x < maxIndex)
            {
                return c.Internal_NeighborChunks[2];
            }

            if (RightNeighbor == null)
                return null;
            if (!RightNeighbor.gameObject.activeInHierarchy)
                return null;

            List<GTerrainChunk> rightTerrainChunks = new List<GTerrainChunk>(RightNeighbor.GetChunks());
            index.x = 0;
            GTerrainChunk neighborChunk = rightTerrainChunks.Find(c0 => c0.Index == index);
            return neighborChunk;
        }

        private GTerrainChunk GetBottomNeighborChunk(GTerrainChunk c)
        {
            int maxIndex = TerrainData.Geometry.ChunkGridSize - 1;
            Vector2 index = c.Index;
            if (index.y > 0)
            {
                return c.Internal_NeighborChunks[3];
            }

            if (BottomNeighbor == null)
                return null;
            if (!BottomNeighbor.gameObject.activeInHierarchy)
                return null;

            List<GTerrainChunk> bottomTerrainChunks = new List<GTerrainChunk>(BottomNeighbor.GetChunks());
            index.y = maxIndex;
            GTerrainChunk neighborChunk = bottomTerrainChunks.Find(c0 => c0.Index == index);
            return neighborChunk;
        }

        private void OnRenderingDirty()
        {
            InitChunks();
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].Internal_UpdateRenderer();
            }
        }

        private void OnShadingDirty()
        {
#if __MICROSPLAT_POLARIS__
            if (TerrainData.Shading.ShadingSystem == GShadingSystem.MicroSplat)
            {
                //PullMaterialAndSplatMapsFromMicroSplat();
                PushControlTexturesToMicroSplat();
            }
#endif

            if (TerrainData.Shading.ShadingSystem != GShadingSystem.Polaris)
                return;
            InitChunks();
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].Internal_UpdateMaterial();
            }
        }

#if __MICROSPLAT_POLARIS__
        public void PushControlTexturesToMicroSplat()
        {
            if (TerrainData == null)
                return;
            MicroSplatPolarisMesh pm = gameObject.GetComponent<MicroSplatPolarisMesh>();
            if (pm == null)
                return;
            Texture2D[] controls = new Texture2D[TerrainData.Shading.SplatControlMapCount];
            for (int i = 0; i < controls.Length; ++i)
            {
                controls[i] = TerrainData.Shading.GetSplatControl(i);
            }
            pm.controlTextures = controls;
            pm.Sync();
        }

        public void PullMaterialAndSplatMapsFromMicroSplat()
        {
            if (TerrainData == null)
                return;
            MicroSplatPolarisMesh pm = gameObject.GetComponent<MicroSplatPolarisMesh>();
            if (pm == null)
                return;
            TerrainData.Shading.CustomMaterial = pm.matInstance;

            TextureArrayConfig config = TerrainData.Shading.MicroSplatTextureArrayConfig;
            if (config == null)
                return;
            GSplatPrototypeGroup protoGroup = TerrainData.Shading.Splats;
            if (protoGroup == null)
                return;

            List<TextureArrayConfig.TextureEntry> entries = config.sourceTextures;
            List<GSplatPrototype> prototypes = new List<GSplatPrototype>();
            for (int i = 0; i < entries.Count; ++i)
            {
                GSplatPrototype p = new GSplatPrototype();
                p.Texture = entries[i].diffuse;
                prototypes.Add(p);
            }

            protoGroup.Prototypes = prototypes;
        }
#endif

        private void OnFoliageDirty()
        {
            TerrainData.Foliage.Refresh();
        }

        private GTerrainChunk[] InitChunks()
        {
            Transform root = GetOrCreateChunkRoot();
            GTerrainChunk[] chunks = root.GetComponentsInChildren<GTerrainChunk>();
            int gridSize = TerrainData.Geometry.ChunkGridSize;
            if (chunks.Length != gridSize * gridSize)
            {
                if (root != null)
                {
                    DestroyImmediate(root.gameObject);
                }
                for (int z = 0; z < gridSize; ++z)
                {
                    for (int x = 0; x < gridSize; ++x)
                    {
                        Transform chunkTransform = GUtilities.GetChildrenWithName(GetOrCreateChunkRoot(), string.Format("C({0},{1})", x, z));
                        GTerrainChunk chunk = chunkTransform.gameObject.AddComponent<GTerrainChunk>();
                        chunk.Index = new Vector2(x, z);
                        chunk.Terrain = this;
                        chunk.Internal_UpdateRenderer();
                        chunk.Internal_UpdateMaterial();
                    }
                }
            }

            Vector2 chunkPhysicalSize = new Vector2(TerrainData.Geometry.Width, TerrainData.Geometry.Length) / gridSize;
            chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                GTerrainChunk currentChunk = chunks[i];
                if (geometryVersion >= GEOMETRY_VERSION_CHUNK_POSITION_AT_CHUNK_CENTER)
                {
                    currentChunk.transform.localPosition = new Vector3((currentChunk.Index.x + 0.5f) * chunkPhysicalSize.x, 0, (currentChunk.Index.y + 0.5f) * chunkPhysicalSize.y);
                }

                currentChunk.SetupLODGroup(TerrainData.Geometry.LODCount);

                GUtilities.Fill(currentChunk.Internal_NeighborChunks, null);
                for (int j = 0; j < chunks.Length; ++j)
                {
                    GTerrainChunk otherChunk = chunks[j];
                    if (otherChunk.Index == currentChunk.Index + Vector2.left)
                    {
                        currentChunk.Internal_NeighborChunks[0] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.up)
                    {
                        currentChunk.Internal_NeighborChunks[1] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.right)
                    {
                        currentChunk.Internal_NeighborChunks[2] = otherChunk;
                    }
                    if (otherChunk.Index == currentChunk.Index + Vector2.down)
                    {
                        currentChunk.Internal_NeighborChunks[3] = otherChunk;
                    }
                }
#if UNITY_EDITOR && !UNITY_2022_2_OR_NEWER
                StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(currentChunk.gameObject);
                GameObjectUtility.SetStaticEditorFlags(currentChunk.gameObject, staticFlags | StaticEditorFlags.NavigationStatic);
#endif
            }

            GUtilities.MarkCurrentSceneDirty();
            return chunks;
        }

        public GTerrainChunk[] GetChunks()
        {
            Transform root = GetChunkRoot();
            if (root != null)
            {
                return root.GetComponentsInChildren<GTerrainChunk>();
            }
            else
            {
                return new GTerrainChunk[0];
            }
        }

        private List<GTerrainChunk> GetChunksSortedByDistance(Vector3 origin)
        {
            List<GTerrainChunk> chunks = new List<GTerrainChunk>(GetChunks());
            SortChunkByDistance(origin, chunks);
            return chunks;
        }

        private void SortChunkByDistance(Vector3 origin, List<GTerrainChunk> chunks)
        {
            chunks.Sort((c1, c2) =>
            {
                Vector3 center1 = c1.MeshColliderComponent.bounds.center;
                Vector3 center2 = c2.MeshColliderComponent.bounds.center;
                float d1 = Vector3.Distance(origin, center1);
                float d2 = Vector3.Distance(origin, center2);
                return d1.CompareTo(d2);
            });
        }

        public bool Raycast(Ray ray, out RaycastHit hit, float distance)
        {
            List<GTerrainChunk> chunks = GetChunksSortedByDistance(ray.origin);

            for (int i = 0; i < chunks.Count; ++i)
            {
                if (chunks[i].MeshColliderComponent.Raycast(ray, out hit, distance))
                {
                    return true;
                }
            }

            hit = new RaycastHit();
            return false;
        }

        public bool Raycast(Vector3 normalizePoint, out RaycastHit hit)
        {
            Ray r = new Ray();
            Vector3 origin = NormalizedToWorldPoint(normalizePoint);
            origin.y = 10000;
            r.origin = origin;
            r.direction = Vector3.down;

            return Raycast(r, out hit, float.MaxValue);
        }

        public static bool Raycast(Ray ray, out RaycastHit hit, float distance, int groupId)
        {
            List<RaycastHit> hitInfo = new List<RaycastHit>();
            IEnumerator<GStylizedTerrain> terrain = ActiveTerrains.GetEnumerator();
            while (terrain.MoveNext())
            {
                if (terrain.Current.GroupId != groupId && groupId >= 0)
                    continue;
                RaycastHit h;
                if (terrain.Current.Raycast(ray, out h, distance))
                {
                    hitInfo.Add(h);
                }
            }
            if (hitInfo.Count == 0)
            {
                hit = new RaycastHit();
                return false;
            }
            else
            {
                hitInfo.Sort((h0, h1) =>
                    Vector3.SqrMagnitude(h0.point - ray.origin)
                    .CompareTo(Vector3.SqrMagnitude(h1.point - ray.origin)));
                hit = hitInfo[0];
                return true;
            }
        }

        public Vector2 WorldPointToUV(Vector3 point)
        {
            if (TerrainData == null)
                return Vector2.zero;
            Vector3 localPoint = transform.InverseTransformPoint(point);
            Vector3 terrainSize = new Vector3(TerrainData.Geometry.Width, TerrainData.Geometry.Height, TerrainData.Geometry.Length);
            Vector2 uv = new Vector2(
                GUtilities.InverseLerpUnclamped(0, terrainSize.x, localPoint.x),
                GUtilities.InverseLerpUnclamped(0, terrainSize.z, localPoint.z));
            return uv;
        }

        public Vector3 WorldPointToNormalized(Vector3 point)
        {
            if (TerrainData == null)
                return Vector2.zero;
            Vector3 localPoint = transform.InverseTransformPoint(point);
            Vector3 terrainSize = new Vector3(TerrainData.Geometry.Width, TerrainData.Geometry.Height, TerrainData.Geometry.Length);
            Vector3 normalized = new Vector3(
                GUtilities.InverseLerpUnclamped(0, terrainSize.x, localPoint.x),
                GUtilities.InverseLerpUnclamped(0, terrainSize.y, localPoint.y),
                GUtilities.InverseLerpUnclamped(0, terrainSize.z, localPoint.z));
            return normalized;
        }

        public Vector3 NormalizedToWorldPoint(Vector3 normalizedPoint)
        {
            if (TerrainData == null)
            {
                return normalizedPoint;
            }
            else
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    transform.position,
                    transform.rotation,
                    new Vector3(
                        transform.lossyScale.x * TerrainData.Geometry.Width,
                        transform.lossyScale.y * TerrainData.Geometry.Height,
                        transform.lossyScale.z * TerrainData.Geometry.Length));
                return matrix.MultiplyPoint(normalizedPoint);
            }
        }

        public Matrix4x4 GetWorldToNormalizedMatrix()
        {
            Vector3 terrainSize = TerrainData.Geometry.Size;
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            Matrix4x4 localToNormalized = Matrix4x4.Scale(new Vector3(1 / terrainSize.x, 1 / terrainSize.y, 1 / terrainSize.z));
            Matrix4x4 worldToNormalized = localToNormalized * worldToLocal;
            return worldToNormalized;
        }

        public void ForceLOD(int level)
        {
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].LodGroupComponent.ForceLOD(level);
            }
        }

        public Vector4 GetHeightMapSample(Vector2 uv)
        {
            if (TerrainData == null)
                return Vector4.zero;
            Vector4 sample = TerrainData.Geometry.GetDecodedHeightMapSample(uv);
            return sample;
        }

        private static Vector4 GetHeightMapSample(GStylizedTerrain t, Vector2 uv)
        {
            if (t == null || !t.isActiveAndEnabled)
                return Vector4.zero;
            else
                return t.GetHeightMapSample(uv);
        }

        public Vector4 GetInterpolatedHeightMapSample(Vector2 uv)
        {
            int count = 1;
            Vector4 sample = Vector4.zero;
            if (uv.x == 0 && uv.y == 0) //bottom left
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(LeftNeighbor, new Vector2(1, 0));
                sample += GetHeightMapSample(BottomNeighbor, new Vector2(0, 1));
                sample +=
                    LeftNeighbor != null ? GetHeightMapSample(LeftNeighbor.BottomNeighbor, new Vector2(1, 1)) :
                    BottomNeighbor != null ? GetHeightMapSample(BottomNeighbor.LeftNeighbor, new Vector2(1, 1)) : Vector4.zero;

                count += LeftNeighbor != null ? 1 : 0;
                count += BottomNeighbor != null ? 1 : 0;
                count += LeftNeighbor != null && LeftNeighbor.BottomNeighbor != null ? 1 :
                    BottomNeighbor != null && BottomNeighbor.LeftNeighbor != null ? 1 : 0;
            }
            else if (uv.x == 0 && uv.y == 1) //top left
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(LeftNeighbor, new Vector2(1, 1));
                sample += GetHeightMapSample(TopNeighbor, new Vector2(0, 0));
                sample +=
                    LeftNeighbor != null ? GetHeightMapSample(LeftNeighbor.TopNeighbor, new Vector2(1, 0)) :
                    TopNeighbor != null ? GetHeightMapSample(TopNeighbor.LeftNeighbor, new Vector2(1, 0)) : Vector4.zero;

                count += LeftNeighbor != null ? 1 : 0;
                count += TopNeighbor != null ? 1 : 0;
                count += LeftNeighbor != null && LeftNeighbor.TopNeighbor != null ? 1 :
                    TopNeighbor != null && TopNeighbor.LeftNeighbor != null ? 1 : 0;
            }
            else if (uv.x == 1 && uv.y == 1) //top right
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(RightNeighbor, new Vector2(0, 1));
                sample += GetHeightMapSample(TopNeighbor, new Vector2(1, 0));
                sample +=
                    RightNeighbor != null ? GetHeightMapSample(RightNeighbor.TopNeighbor, new Vector2(0, 0)) :
                    TopNeighbor != null ? GetHeightMapSample(TopNeighbor.RightNeighbor, new Vector2(0, 0)) : Vector4.zero;

                count += RightNeighbor != null ? 1 : 0;
                count += TopNeighbor != null ? 1 : 0;
                count += RightNeighbor != null && RightNeighbor.TopNeighbor != null ? 1 :
                    TopNeighbor != null && TopNeighbor.RightNeighbor != null ? 1 : 0;
            }
            else if (uv.x == 1 && uv.y == 0) //bottom right
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(RightNeighbor, new Vector2(0, 0));
                sample += GetHeightMapSample(BottomNeighbor, new Vector2(1, 1));
                sample +=
                    RightNeighbor != null ? GetHeightMapSample(RightNeighbor.BottomNeighbor, new Vector2(0, 1)) :
                    BottomNeighbor != null ? GetHeightMapSample(BottomNeighbor.RightNeighbor, new Vector2(0, 1)) : Vector4.zero;

                count += RightNeighbor != null ? 1 : 0;
                count += BottomNeighbor != null ? 1 : 0;
                count += RightNeighbor != null && RightNeighbor.BottomNeighbor != null ? 1 :
                    BottomNeighbor != null && BottomNeighbor.RightNeighbor != null ? 1 : 0;
            }
            else if (uv.x == 0 && uv.y != 0 && uv.y != 1) //left edge
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(LeftNeighbor, new Vector2(1, uv.y));

                count += LeftNeighbor != null ? 1 : 0;
            }
            else if (uv.x == 1 && uv.y != 0 && uv.y != 1) //right edge
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(RightNeighbor, new Vector2(0, uv.y));

                count += RightNeighbor != null ? 1 : 0;
            }
            else if (uv.x != 0 && uv.x != 1 && uv.y == 0) //bottom edge
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(BottomNeighbor, new Vector2(uv.x, 1));

                count += BottomNeighbor != null ? 1 : 0;
            }
            else if (uv.x != 0 && uv.x != 1 && uv.y == 1) //top edge
            {
                sample += GetHeightMapSample(uv);
                sample += GetHeightMapSample(TopNeighbor, new Vector2(uv.x, 0));

                count += TopNeighbor != null ? 1 : 0;
            }
            else
            {
                sample = GetHeightMapSample(uv);
            }
            return sample * 1.0f / count;
        }

        public static void MatchEdges(int groupId)
        {
            GCommon.ForEachTerrain(groupId, (t) =>
            {
                t.MatchEdges();
            });
        }

        public void MatchEdges()
        {
            if (TerrainData == null)
                return;
            if (LeftNeighbor != null)
            {
                Rect leftRect = new Rect();
                leftRect.size = new Vector2(0.01f, 1);
                leftRect.center = new Vector2(0, 0.5f);
                TerrainData.Geometry.SetRegionDirty(leftRect);
            }

            if (TopNeighbor != null)
            {
                Rect topRect = new Rect();
                topRect.size = new Vector2(1, 0.01f);
                topRect.center = new Vector2(0.5f, 1);
                TerrainData.Geometry.SetRegionDirty(topRect);
            }

            if (RightNeighbor != null)
            {
                Rect rightRect = new Rect();
                rightRect.size = new Vector2(0.01f, 1f);
                rightRect.center = new Vector2(1, 0.5f);
                TerrainData.Geometry.SetRegionDirty(rightRect);
            }

            if (BottomNeighbor != null)
            {
                Rect bottomRect = new Rect();
                bottomRect.size = new Vector2(1, 0.01f);
                bottomRect.center = new Vector2(0.5f, 0);
                TerrainData.Geometry.SetRegionDirty(bottomRect);
            }

            TerrainData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
        }

        public void UpdateTreesPosition()
        {
            if (TerrainData == null)
                return;

            int oldLayer = gameObject.layer;
            int raycastLayer = LayerMask.NameToLayer(RAYCAST_LAYER);
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].gameObject.layer = raycastLayer;
            }

            LayerMask mask = LayerMask.GetMask(RAYCAST_LAYER);
            if (TerrainData.Foliage.TreeSnapMode == GSnapMode.World)
            {
                mask |= TerrainData.Foliage.TreeSnapLayerMask;
            }

            NativeArray<Vector2> treePosisitonsNA = TerrainData.Foliage.GetTreesPositionArray(Allocator.TempJob);
            NativeArray<Rect> dirtyRectsNA = new NativeArray<Rect>(TerrainData.Foliage.GetTreeDirtyRegions(), Allocator.TempJob);
            NativeArray<RaycastCommand> commandsNA = new NativeArray<RaycastCommand>(treePosisitonsNA.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<RaycastHit> hitsNA = new NativeArray<RaycastHit>(treePosisitonsNA.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            Vector3 terrainSize = TerrainData.Geometry.Size;
            int maxHit = 1;

            GBuildRaycastCommandJob job = new GBuildRaycastCommandJob()
            {
                dirtyRects = dirtyRectsNA,
                positions = treePosisitonsNA,
                commands = commandsNA,
                terrainPosition = transform.position,
                terrainSize = terrainSize,
                maxHit = maxHit,
                mask = mask.value
            };
            {
                JobHandle jHandle = job.Schedule(treePosisitonsNA.Length, 100);
                jHandle.Complete();
            }

            treePosisitonsNA.Dispose();
            dirtyRectsNA.Dispose();

            {
                JobHandle jHandle = RaycastCommand.ScheduleBatch(commandsNA, hitsNA, 100);
                jHandle.Complete();
            }

            List<GTreeInstance> trees = TerrainData.Foliage.TreeInstances;
            int treeCount = trees.Count;

            for (int i = 0; i < treeCount; ++i)
            {
                RaycastHit h = hitsNA[i];
                if (h.collider == null)
                    continue;
                GTreeInstance t = trees[i];
                t.position.y = Mathf.InverseLerp(0, terrainSize.y, transform.InverseTransformPoint(h.point).y);
                trees[i] = t;
            }

            commandsNA.Dispose();
            hitsNA.Dispose();

            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].gameObject.layer = oldLayer;
            }

            TerrainData.Foliage.TreeAllChanged();
        }

        public void UpdateGrassPatches()
        {
            if (TerrainData == null)
                return;

            int oldLayer = gameObject.layer;
            int raycastLayer = LayerMask.NameToLayer(RAYCAST_LAYER);
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].gameObject.layer = raycastLayer;
            }

            LayerMask mask = LayerMask.GetMask(RAYCAST_LAYER);
            if (TerrainData.Foliage.GrassSnapMode == GSnapMode.World)
            {
                mask |= TerrainData.Foliage.GrassSnapLayerMask;
            }

            NativeArray<Rect> dirtyRectsNA = new NativeArray<Rect>(TerrainData.Foliage.GetGrassDirtyRegions(), Allocator.TempJob);
            List<GGrassPrototype> prototypes = null;
            if (TerrainData.Foliage.Grasses != null)
            {
                prototypes = TerrainData.Foliage.Grasses.Prototypes;
            }

            GGrassPatch[] patches = TerrainData.Foliage.GrassPatches;
            for (int pIndex = 0; pIndex < patches.Length; ++pIndex)
            {
                GGrassPatch p = patches[pIndex];
                if (!GUtilities.TestOverlap(p.GetUvRange(), dirtyRectsNA))
                {
                    continue;
                }

                NativeArray<Vector2> positionsNA = p.GetGrassPositionArray();
                NativeArray<RaycastCommand> commandsNA = new NativeArray<RaycastCommand>(p.InstanceCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                NativeArray<RaycastHit> hitsNA = new NativeArray<RaycastHit>(p.InstanceCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                Vector3 terrainSize = TerrainData.Geometry.Size;
                int maxHit = 1;

                GBuildRaycastCommandJob job = new GBuildRaycastCommandJob()
                {
                    dirtyRects = dirtyRectsNA,
                    positions = positionsNA,
                    commands = commandsNA,
                    terrainPosition = transform.position,
                    terrainSize = terrainSize,
                    maxHit = maxHit,
                    mask = mask.value
                };
                {
                    JobHandle jHandle = job.Schedule(p.InstanceCount, 100);
                    jHandle.Complete();
                }

                positionsNA.Dispose();

                {
                    JobHandle jHandle = RaycastCommand.ScheduleBatch(commandsNA, hitsNA, 100);
                    jHandle.Complete();
                }

                List<GGrassInstance> grasses = p.Instances;
                int count = grasses.Count;

                for (int i = 0; i < count; ++i)
                {
                    RaycastHit h = hitsNA[i];
                    if (h.collider == null)
                        continue;
                    GGrassInstance g = grasses[i];
                    g.position.y = Mathf.InverseLerp(0, terrainSize.y, transform.InverseTransformPoint(h.point).y);

                    if (prototypes != null && g.prototypeIndex >= 0 && g.prototypeIndex < prototypes.Count)
                    {
                        GGrassPrototype proto = prototypes[g.PrototypeIndex];
                        if (proto.AlignToSurface)
                        {
                            Quaternion currentRotationY = Quaternion.Euler(0, g.rotation.eulerAngles.y, 0);
                            Quaternion rotationXZ = Quaternion.FromToRotation(Vector3.up, h.normal);
                            g.rotation = rotationXZ * currentRotationY;
                        }
                        else
                        {
                            Quaternion currentRotationY = Quaternion.Euler(0, g.rotation.eulerAngles.y, 0);
                            g.rotation = currentRotationY;
                        }
                    }
                    grasses[i] = g;
                }

                commandsNA.Dispose();
                hitsNA.Dispose();

                p.RecalculateBounds();
                p.Changed();
            }

            dirtyRectsNA.Dispose();

            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].gameObject.layer = oldLayer;
            }
        }

        public RenderTexture GetHeightMap(int resolution)
        {
            if (heightMap == null)
            {
                heightMap = new RenderTexture(resolution, resolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
                RenderHeightMap(heightMap);
            }
            if (heightMap.width != resolution ||
                heightMap.height != resolution ||
                heightMap.format != GGeometry.HeightMapRTFormat)
            {
                heightMap.Release();
                GUtilities.DestroyObject(heightMap);
                heightMap = new RenderTexture(resolution, resolution, 0, GGeometry.HeightMapRTFormat, RenderTextureReadWrite.Linear);
                RenderHeightMap(heightMap);
            }
            if (!heightMap.IsCreated())
            {
                RenderHeightMap(heightMap);
            }

            return heightMap;
        }

        private void RenderHeightMap(RenderTexture rt)
        {
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                RenderHeightMap(rt, chunks[i]);
            }
        }

        private void RenderHeightMap(RenderTexture rt, GTerrainChunk chunk)
        {
            Mesh m = chunk.MeshFilterComponent.sharedMesh;
            if (m == null)
                return;
            Vector2[] uvs = m.uv;
            Material mat = GInternalMaterials.GeometricalHeightMapMaterial;
            mat.SetPass(0);
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);

            for (int i = 0; i < uvs.Length; ++i)
            {
                GL.Vertex3(uvs[i].x, uvs[i].y, GetInterpolatedHeightMapSample(uvs[i]).x);
            }

            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public RenderTexture GetSharpNormalMap(int resolution)
        {
            if (normalMap == null)
            {
                normalMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderSharpNormalMap(normalMap);
            }
            if (normalMap.width != resolution ||
                normalMap.height != resolution)
            {
                normalMap.Release();
                GUtilities.DestroyObject(normalMap);
                normalMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderSharpNormalMap(normalMap);
            }
            if (!normalMap.IsCreated())
            {
                RenderSharpNormalMap(normalMap);
            }

            return normalMap;
        }

        private void RenderSharpNormalMap(RenderTexture rt)
        {
            GNormalMapGeneratorParams param = new GNormalMapGeneratorParams();
            param.Terrain = this;
            param.Space = GNormalMapSpace.Local;
            param.Mode = GNormalMapMode.Sharp;
            GNormalMapGenerator gen = new GNormalMapGenerator();
            gen.RenderSharpNormalMap(param, rt);
        }

        public RenderTexture GetInterpolatedNormalMap(int resolution)
        {
            if (normalMapInterpolated == null)
            {
                normalMapInterpolated = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderInterpolatedNormalMap(normalMapInterpolated);
            }
            if (normalMapInterpolated.width != resolution ||
                normalMapInterpolated.height != resolution)
            {
                normalMapInterpolated.Release();
                GUtilities.DestroyObject(normalMapInterpolated);
                normalMapInterpolated = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderInterpolatedNormalMap(normalMapInterpolated);
            }
            if (!normalMapInterpolated.IsCreated())
            {
                RenderInterpolatedNormalMap(normalMapInterpolated);
            }

            return normalMapInterpolated;
        }

        private void RenderInterpolatedNormalMap(RenderTexture rt)
        {
            GNormalMapGeneratorParams param = new GNormalMapGeneratorParams();
            param.Terrain = this;
            param.Space = GNormalMapSpace.Local;
            param.Mode = GNormalMapMode.Interpolated;
            GNormalMapGenerator gen = new GNormalMapGenerator();
            gen.RenderInterpolatedNormalMap(param, rt);
        }

        public RenderTexture GetPerPixelNormalMap(int resolution)
        {
            if (normalMapPerPixel == null)
            {
                normalMapPerPixel = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderPerPixelNormalMap(normalMapPerPixel);
            }
            if (normalMapPerPixel.width != resolution ||
                normalMapPerPixel.height != resolution)
            {
                normalMapPerPixel.Release();
                GUtilities.DestroyObject(normalMapPerPixel);
                normalMapPerPixel = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                RenderPerPixelNormalMap(normalMapPerPixel);
            }
            if (!normalMapPerPixel.IsCreated())
            {
                RenderPerPixelNormalMap(normalMapPerPixel);
            }

            return normalMapPerPixel;
        }

        private void RenderPerPixelNormalMap(RenderTexture rt)
        {
            GNormalMapGeneratorParams param = new GNormalMapGeneratorParams();
            param.Terrain = this;
            param.Space = GNormalMapSpace.Local;
            param.Mode = GNormalMapMode.PerPixel;
            GNormalMapGenerator gen = new GNormalMapGenerator();
            gen.RenderPerPixelNormalMap(param, rt);
        }

        public void Refresh()
        {
            InitChunks();
            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].Refresh();
            }
        }

#if UNITY_EDITOR
        private void DrawChunkUpdateDebug(Camera cam)
        {
            Material mat = GInternalMaterials.MaskVisualizerMaterial;
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                GTerrainChunk c = chunks[i];
                Mesh m = c.MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;

                System.DateTime now = System.DateTime.Now;
                System.TimeSpan timeSpan = now - c.LastUpdatedTime;
                float duration = 0.3f;
                float span = (float)timeSpan.TotalSeconds;
                if (span > duration)
                    continue;

                float alpha = 1 - Mathf.InverseLerp(0, duration, span);
                Color color = new Color(1, 1, 1, alpha * 0.5f);
                block.SetColor("_Color", color);
                Graphics.DrawMesh(
                    m,
                    c.transform.localToWorldMatrix,
                    mat,
                    LayerMask.NameToLayer("Default"),
                    cam,
                    0,
                    block);
            }

        }

        private void DrawChunkTopographic(Camera cam)
        {
            if (TerrainData == null)
                return;

            Material mat = GEditorSettings.Instance.topographic.topographicMaterial;
            if (mat == null)
                return;
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            GTerrainChunk[] chunks = GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                GTerrainChunk c = chunks[i];
                Mesh m = c.MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;
                Graphics.DrawMesh(
                    m,
                    c.transform.localToWorldMatrix,
                    mat,
                    LayerMask.NameToLayer("Default"),
                    cam,
                    0,
                    block);
            }

        }
#endif

        public RenderTexture GetGrassVectorFieldRenderTexture()
        {
            if (TerrainData == null)
                return null;

            int resolution = TerrainData.Foliage.VectorFieldMapResolution;
            if (grassVectorFieldMap == null)
            {
                grassVectorFieldMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                GCommon.FillTexture(grassVectorFieldMap, Color.gray);
            }
            if (grassVectorFieldMap.width != resolution ||
                grassVectorFieldMap.height != resolution)
            {
                grassVectorFieldMap.Release();
                GUtilities.DestroyObject(grassVectorFieldMap);
                grassVectorFieldMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                GCommon.FillTexture(grassVectorFieldMap, Color.gray);
            }
            if (!grassVectorFieldMap.IsCreated())
            {
                GCommon.FillTexture(grassVectorFieldMap, Color.gray);
            }

            if (grassVectorFieldMapTmp == null)
            {
                grassVectorFieldMapTmp = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }
            if (grassVectorFieldMapTmp.width != resolution ||
                grassVectorFieldMapTmp.height != resolution)
            {
                grassVectorFieldMapTmp.Release();
                GUtilities.DestroyObject(grassVectorFieldMapTmp);
                grassVectorFieldMapTmp = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }

            //TerrainData.Shading.MaterialToRender.SetTexture("_MainTex", grassVectorFieldMap);

            return grassVectorFieldMap;
        }

        private void LateUpdate()
        {
            if (TerrainData != null && TerrainData.Foliage.EnableInteractiveGrass)
            {
                FadeGrassVectorField();
            }
            if (TerrainData != null && !TerrainData.Foliage.EnableInteractiveGrass)
            {
                if (grassVectorFieldMap != null)
                {
                    grassVectorFieldMap.Release();
                    GUtilities.DestroyObject(grassVectorFieldMap);
                }
                if (grassVectorFieldMapTmp != null)
                {
                    grassVectorFieldMapTmp.Release();
                    GUtilities.DestroyObject(grassVectorFieldMapTmp);
                }
            }
        }

        private void FadeGrassVectorField()
        {
            RenderTexture rt = GetGrassVectorFieldRenderTexture();
            RenderTexture bg = grassVectorFieldMapTmp;
            GCommon.CopyToRT(rt, bg);

            Material mat = GInternalMaterials.InteractiveGrassVectorFieldMaterial;
            mat.SetTexture("_Background", bg);
            mat.SetFloat("_Opacity", TerrainData.Foliage.RestoreSensitive);
            int pass = 1;
            GCommon.DrawQuad(rt, GCommon.FullRectUvPoints, mat, pass);
        }

        public NativeArray<Rect> GetChunkRectsNA()
        {
            if (TerrainData == null)
                return new NativeArray<Rect>(0, Allocator.TempJob);

            int gridSize = TerrainData.Geometry.ChunkGridSize;
            Vector3 terrainSize = TerrainData.Geometry.Size;
            Vector2 rectSize = new Vector2(terrainSize.x / gridSize, terrainSize.z / gridSize);

            NativeArray<Rect> rects = new NativeArray<Rect>(gridSize * gridSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int z = 0; z < gridSize; ++z)
            {
                for (int x = 0; x < gridSize; ++x)
                {
                    Rect r = new Rect();
                    r.x = transform.position.x + x * rectSize.x;
                    r.y = transform.position.z + z * rectSize.y;
                    r.size = rectSize;
                    rects[GUtilities.To1DIndex(x, z, gridSize)] = r;
                }
            }

            return rects;
        }

        public Rect[] GetChunkRects()
        {
            if (TerrainData == null)
                return new Rect[0];

            int gridSize = TerrainData.Geometry.ChunkGridSize;
            Vector3 terrainSize = TerrainData.Geometry.Size;
            Vector2 rectSize = new Vector2(terrainSize.x / gridSize, terrainSize.z / gridSize);

            Rect[] rects = new Rect[gridSize * gridSize];
            for (int z = 0; z < gridSize; ++z)
            {
                for (int x = 0; x < gridSize; ++x)
                {
                    Rect r = new Rect();
                    r.x = transform.position.x + x * rectSize.x;
                    r.y = transform.position.z + z * rectSize.y;
                    r.size = rectSize;
                    rects[GUtilities.To1DIndex(x, z, gridSize)] = r;
                }
            }

            return rects;
        }

        public Matrix4x4 GetUnitRectToWorldMatrix()
        {
            Vector3 worldSize = Vector3.zero;
            if (TerrainData != null)
            {
                worldSize = TerrainData.Geometry.Size;
            }

            Quaternion rotation = Quaternion.identity;
            Vector3 position = transform.position;
            Matrix4x4 rectToWorld = Matrix4x4.TRS(position, rotation, worldSize);
            return rectToWorld;
        }

        public void ResetNeighboring()
        {
            leftNeighbor = null;
            leftTerrainData = null;

            topNeighbor = null;
            topTerrainData = null;

            rightNeighbor = null;
            rightTerrainData = null;

            bottomNeighbor = null;
            bottomTerrainData = null;
        }
    }
}

#endif
