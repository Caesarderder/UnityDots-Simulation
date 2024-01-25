#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.Griffin
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(LODGroup))]
    public class GTerrainChunk : MonoBehaviour
    {
        [SerializeField]
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            internal set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private MeshFilter meshFilterComponent;
        public MeshFilter MeshFilterComponent
        {
            get
            {
                if (meshFilterComponent == null)
                {
                    meshFilterComponent = GetComponent<MeshFilter>();
                }
                return meshFilterComponent;
            }
        }

        [SerializeField]
        private MeshRenderer meshRendererComponent;
        public MeshRenderer MeshRendererComponent
        {
            get
            {
                if (meshRendererComponent == null)
                {
                    meshRendererComponent = GetComponent<MeshRenderer>();
                }
                return meshRendererComponent;
            }
        }

        [SerializeField]
        private MeshCollider meshColliderComponent;
        public MeshCollider MeshColliderComponent
        {
            get
            {
                if (meshColliderComponent == null)
                {
                    meshColliderComponent = GetComponent<MeshCollider>();
                }
                return meshColliderComponent;
            }
        }

        [SerializeField]
        private LODGroup lodGroupComponent;
        public LODGroup LodGroupComponent
        {
            get
            {
                if (lodGroupComponent == null)
                {
                    lodGroupComponent = GetComponent<LODGroup>();
                }
                return lodGroupComponent;
            }
        }

        [SerializeField]
        private Vector2 index;
        public Vector2 Index
        {
            get
            {
                return index;
            }
            internal set
            {
                index = value;
            }
        }

        [SerializeField]
        private GTerrainChunkLOD[] chunkLowerLOD;
        private GTerrainChunkLOD[] ChunkLowerLOD
        {
            get
            {
                if (chunkLowerLOD == null || chunkLowerLOD.Length != Terrain.TerrainData.Geometry.LODCount - 1)
                {
                    chunkLowerLOD = new GTerrainChunkLOD[Terrain.TerrainData.Geometry.LODCount - 1];
                    GUtilities.ClearChildren(transform);
                }
                return chunkLowerLOD;
            }
        }

        [SerializeField]
        private GTerrainChunk[] neighborChunks;
        internal GTerrainChunk[] Internal_NeighborChunks
        {
            get
            {
                if (neighborChunks == null || neighborChunks.Length != 4)
                {
                    neighborChunks = new GTerrainChunk[4];
                }
                return neighborChunks;
            }
            set
            {
                neighborChunks = value;
            }
        }

        private System.DateTime lastUpdatedTime;
        public System.DateTime LastUpdatedTime
        {
            get
            {
                return lastUpdatedTime;
            }
            private set
            {
                lastUpdatedTime = value;
            }
        }

        public Matrix4x4 LocalToTerrainMatrix
        {
            get
            {
                return transform.localToWorldMatrix * Terrain.transform.worldToLocalMatrix;
            }
        }

        private NativeArray<GSubdivNode> subdivNodeNativeArray;
        private NativeArray<byte> subdivNodeCreationState;

        private NativeArray<Vector3> vertexNativeArray;
        private Vector3[] vertexArray;

        private NativeArray<Vector2> uvsNativeArray;
        private Vector2[] uvsArray;

        private NativeArray<int> trianglesNativeArray;
        private int[] trianglesArray;

        private NativeArray<Vector3> normalsNativeArray;
        private Vector3[] normalsArray;

        private NativeArray<Color32> vertexColorsNativeArray;
        private Color32[] vertexColorsArray;

        private NativeArray<bool> vertexMarkerNativeArray;
        private bool[] vertexMarker_Cache;

        private NativeArray<int> generationMetadata;

        private Mesh[] nonSerializedMeshes;
        private Mesh[] NonSerializedMeshes
        {
            get
            {
                if (nonSerializedMeshes == null)
                {
                    nonSerializedMeshes = new Mesh[GCommon.MAX_LOD_COUNT];
                }
                if (nonSerializedMeshes.Length != GCommon.MAX_LOD_COUNT)
                {
                    CleanUpNonSerializedMeshes();
                    nonSerializedMeshes = new Mesh[GCommon.MAX_LOD_COUNT];
                }
                return nonSerializedMeshes;
            }
        }

        public Rect GetUvRange()
        {
            if (Terrain == null || Terrain.TerrainData == null)
            {
                return GCommon.UnitRect;
            }
            else
            {
                int gridSize = Terrain.TerrainData.Geometry.ChunkGridSize;
                Vector2 position = index / gridSize;
                Vector2 size = Vector2.one / gridSize;
                return new Rect(position, size);
            }
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            CleanUpNonSerializedMeshes();
        }

        private void OnDestroy()
        {
            //CleanUpNativeArrays(); //Already cleaned up after generation, will cause error at runtime
        }

        private void RecalculateTangentIfNeeded(Mesh m)
        {
            if (m == null)
                return;
            if (Terrain.TerrainData.Shading.IsMaterialUseNormalMap())
            {
                m.RecalculateTangents();
            }
            else
            {
                m.tangents = null;
            }
        }

        internal void Internal_UpdateRenderer()
        {
            MeshRendererComponent.shadowCastingMode = Terrain.TerrainData.Rendering.CastShadow ?
                UnityEngine.Rendering.ShadowCastingMode.On :
                UnityEngine.Rendering.ShadowCastingMode.Off;
            MeshRendererComponent.receiveShadows = Terrain.TerrainData.Rendering.ReceiveShadow;

            for (int i = 1; i < Terrain.TerrainData.Geometry.LODCount; ++i)
            {
                GTerrainChunkLOD chunkLod = GetChunkLOD(i);
                chunkLod.MeshRendererComponent.shadowCastingMode = MeshRendererComponent.shadowCastingMode;
                chunkLod.MeshRendererComponent.receiveShadows = MeshRendererComponent.receiveShadows;
                chunkLod.MeshRendererComponent.sharedMaterials = MeshRendererComponent.sharedMaterials;
                chunkLod.MeshFilterComponent.sharedMesh = GetMesh(i);
            }
            //SetLastUpdatedTimeNow();
        }

        internal void Internal_UpdateMaterial()
        {
            MeshRendererComponent.sharedMaterials = new Material[] { Terrain.TerrainData.Shading.CustomMaterial };

            for (int i = 1; i < Terrain.TerrainData.Geometry.LODCount; ++i)
            {
                GTerrainChunkLOD chunkLod = GetChunkLOD(i);
                chunkLod.MeshRendererComponent.sharedMaterials = MeshRendererComponent.sharedMaterials;
            }
            //SetLastUpdatedTimeNow();
        }

        public void SetupLocalToTerrainMatrix(MaterialPropertyBlock props)
        {
            string localToTerrainPropName = "_LocalToTerrainMatrix";
            props.SetMatrix(localToTerrainPropName, LocalToTerrainMatrix);
        }

        private GTerrainChunkLOD GetChunkLOD(int level)
        {
            if (level <= 0 || level >= Terrain.TerrainData.Geometry.LODCount)
                throw new System.IndexOutOfRangeException();
            GTerrainChunkLOD c = ChunkLowerLOD[level - 1];
            if (c == null)
            {
                Transform t = GUtilities.GetChildrenWithName(transform, name + "_LOD" + level);
                c = t.gameObject.AddComponent<GTerrainChunkLOD>();
                ChunkLowerLOD[level - 1] = c;
            }
            return c;
        }

        public Mesh GetMesh(int lod)
        {
            int lodCount = Terrain.TerrainData.Geometry.LODCount;
            if (lod < 0 || lod >= lodCount)
            {
                throw new System.ArgumentOutOfRangeException("lod");
            }

            GGeometry.GStorageMode mode = Terrain.TerrainData.Geometry.StorageMode;
            if (mode == GGeometry.GStorageMode.SaveToAsset)
            {
                Vector3Int key = GetChunkMeshKey(Index, lod);
                Mesh m = Terrain.TerrainData.GeometryData.GetMesh(key);
                if (m == null)
                {
                    m = new Mesh();
                    m.name = GetChunkMeshName(Index, lod);
                    m.MarkDynamic();
                    Terrain.TerrainData.GeometryData.SetMesh(key, m);
                }
                return m;
            }
            else
            {
                string key = GetChunkMeshName(Index, lod);
                Mesh m = NonSerializedMeshes[lod];
                if (m == null)
                {
                    m = new Mesh();
                    m.name = string.Format("{0}_NonSerialized", key);
                    m.MarkDynamic();
                    NonSerializedMeshes[lod] = m;
                }
                m.hideFlags = HideFlags.DontSave;
                return m;
            }
        }

        public static string GetChunkMeshName(Vector2 index, int lod)
        {
            return string.Format("{0}_{1}_{2}_{3}", GCommon.CHUNK_MESH_NAME_PREFIX, (int)index.x, (int)index.y, lod);
        }

        public static Vector3Int GetChunkMeshKey(Vector2 index, int lod)
        {
            return new Vector3Int((int)index.x, (int)index.y, lod);
        }

        public void Refresh()
        {
            Mesh lod0 = GetMesh(0);
            MeshFilterComponent.sharedMesh = lod0;
            MeshColliderComponent.sharedMesh = lod0;

            for (int i = 1; i < Terrain.TerrainData.Geometry.LODCount; ++i)
            {
                Mesh lodi = GetMesh(i);
                GTerrainChunkLOD chunkLodi = GetChunkLOD(i);
                chunkLodi.MeshFilterComponent.sharedMesh = lodi;
            }
            //SetLastUpdatedTimeNow();
        }

        private void SetLastUpdatedTimeNow()
        {
            LastUpdatedTime = System.DateTime.Now;
        }

        public bool Raycast(Ray ray, out RaycastHit hit, float distance)
        {
            if (MeshColliderComponent == null || MeshColliderComponent.sharedMesh == null)
            {
                hit = new RaycastHit();
                return false;
            }
            return MeshColliderComponent.Raycast(ray, out hit, distance);
        }

        internal void InitSubdivTreeNativeContainers(int meshResolution)
        {
            int treeNodeCount = GetSubdivTreeNodeCount(meshResolution);
            subdivNodeNativeArray = new NativeArray<GSubdivNode>(treeNodeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            subdivNodeCreationState = new NativeArray<byte>(treeNodeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            generationMetadata = new NativeArray<int>(GGeometryJobUtilities.METADATA_LENGTH, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            int dimension = GGeometryJobUtilities.VERTEX_MARKER_DIMENSION;
            vertexMarkerNativeArray = new NativeArray<bool>(dimension * dimension, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        internal void InitMeshDataNativeContainers(GAlbedoToVertexColorMode albedoToVertexColor)
        {
            int vertexCount = generationMetadata[0] * 3;
            vertexNativeArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GUtilities.EnsureArrayLength(ref vertexArray, vertexCount);

            uvsNativeArray = new NativeArray<Vector2>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GUtilities.EnsureArrayLength(ref uvsArray, vertexCount);

            trianglesNativeArray = new NativeArray<int>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GUtilities.EnsureArrayLength(ref trianglesArray, vertexCount);

            normalsNativeArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            GUtilities.EnsureArrayLength(ref normalsArray, vertexCount);

            if (albedoToVertexColor == GAlbedoToVertexColorMode.None)
            {
                vertexColorsNativeArray = new NativeArray<Color32>(1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }
            else
            {
                vertexColorsNativeArray = new NativeArray<Color32>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                GUtilities.EnsureArrayLength(ref vertexColorsArray, vertexCount);
            }
        }

        internal int GetSubdivTreeNodeCount(int meshResolution)
        {
            int count = 0;
            for (int i = 0; i <= meshResolution; ++i)
            {
                count += GetSubdivTreeNodeCountForLevel(i);
            }
            return count;
        }

        internal int GetSubdivTreeNodeCountForLevel(int level)
        {
            return 2 * Mathf.FloorToInt(Mathf.Pow(2, level));
        }

        internal void CleanUpNativeArrays()
        {
            GNativeArrayUtilities.Dispose(subdivNodeNativeArray);
            GNativeArrayUtilities.Dispose(subdivNodeCreationState);
            GNativeArrayUtilities.Dispose(vertexNativeArray);
            GNativeArrayUtilities.Dispose(uvsNativeArray);
            GNativeArrayUtilities.Dispose(normalsNativeArray);
            GNativeArrayUtilities.Dispose(trianglesNativeArray);
            GNativeArrayUtilities.Dispose(vertexColorsNativeArray);
            GNativeArrayUtilities.Dispose(generationMetadata);
            GNativeArrayUtilities.Dispose(vertexMarkerNativeArray);
        }

        internal void CleanUpNonSerializedMeshes()
        {
            if (nonSerializedMeshes != null)
            {
                for (int i = 0; i < nonSerializedMeshes.Length; ++i)
                {
                    Mesh m = nonSerializedMeshes[i];
                    if (m != null)
                    {
                        GUtilities.DestroyObject(m);
                    }
                }
            }
        }

        internal GCreateBaseTreeJob GetCreateBaseSubdivTreeJob(
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            GCreateBaseTreeJob job = new GCreateBaseTreeJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod
            };

            return job;
        }

        internal GSplitBaseTreeForDynamicPolygonJob GetSplitBaseTreeForDynamicPolygonJob(
            int meshBaseResolution, int meshResolution, int lod,
            GTextureNativeDataDescriptor<Color32> subdivMap)
        {
            Rect uvRect = GetUvRange();
            GSplitBaseTreeForDynamicPolygonJob job = new GSplitBaseTreeForDynamicPolygonJob()
            {
                baseTree = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                subdivMap = subdivMap,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod,
                uvRect = uvRect
            };

            return job;
        }

        internal GStitchSeamJob GetStitchSeamJob(
            int meshBaseResolution,
            int meshResolution,
            bool hasLeftMarkers, NativeArray<bool> leftMarkers,
            bool hasTopMarkers, NativeArray<bool> topMarkers,
            bool hasRightMarkers, NativeArray<bool> rightMarkers,
            bool hasBottomMarkers, NativeArray<bool> bottomMarkers)
        {
            GStitchSeamJob job = new GStitchSeamJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,

                hasLeftMarker = hasLeftMarkers,
                vertexMarkerLeft = leftMarkers,

                hasTopMarker = hasTopMarkers,
                vertexMarkerTop = topMarkers,

                hasRightMarker = hasRightMarkers,
                vertexMarkerRight = rightMarkers,

                hasBottomMarker = hasBottomMarkers,
                vertexMarkerBottom = bottomMarkers
            };

            return job;
        }

        internal GStitchSeamLODJob GetStitchSeamLODJob(
            int meshBaseResolution,
            int meshResolution,
            int lod,
            NativeArray<bool> markerLOD0)
        {
            GStitchSeamLODJob job = new GStitchSeamLODJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,
                vertexMarker = vertexMarkerNativeArray,
                metadata = generationMetadata,
                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,
                lod = lod,
                vertexMarker_LOD0 = markerLOD0
            };

            return job;
        }

        internal void CacheVertexMarker()
        {
            if (vertexMarkerNativeArray.IsCreated)
            {
                GUtilities.EnsureArrayLength(ref vertexMarker_Cache, vertexMarkerNativeArray.Length);
                vertexMarkerNativeArray.CopyTo(vertexMarker_Cache);
            }
        }

        internal GCountLeafNodeJob GetCountLeafNodeJob(
            int meshBaseResolution,
            int meshResolution,
            int lod)
        {
            GCountLeafNodeJob job = new GCountLeafNodeJob()
            {
                creationState = subdivNodeCreationState,
                metadata = generationMetadata,
                baseResolution = meshBaseResolution,
                resolution = meshResolution,
                lod = lod
            };

            return job;
        }

        internal GCreateVertexJob GetCreateVertexJob(
            int meshBaseResolution,
            int meshResolution,
            int lod,
            int displacementSeed,
            float displacementStrength,
            bool smoothNormal,
            bool useSmoothNormalMask,
            bool mergeUv,
            GTextureNativeDataDescriptor<Color32> maskMap,
            GAlbedoToVertexColorMode albedoToVertexColor,
            GTextureNativeDataDescriptor<Color32> albedoMap,
            GTextureNativeDataDescriptor<Color32>[] hm)
        {
            InitMeshDataNativeContainers(albedoToVertexColor);

            Vector3 terrainSize = Terrain.TerrainData.Geometry.Size;
            Rect uvRect = GetUvRange();
            float texelSize = 1.0f / Terrain.TerrainData.Geometry.HeightMapResolution;
            GCreateVertexJob job = new GCreateVertexJob()
            {
                nodes = subdivNodeNativeArray,
                creationState = subdivNodeCreationState,

                hmC = hm[4],
                hmL = hm[3],
                hmT = hm[7],
                hmR = hm[5],
                hmB = hm[1],
                hmBL = hm[0],
                hmTL = hm[6],
                hmTR = hm[8],
                hmBR = hm[2],

                vertices = vertexNativeArray,
                uvs = uvsNativeArray,
                normals = normalsNativeArray,
                triangles = trianglesNativeArray,
                colors = vertexColorsNativeArray,
                metadata = generationMetadata,

                meshBaseResolution = meshBaseResolution,
                meshResolution = meshResolution,
                lod = lod,
                displacementSeed = displacementSeed,
                displacementStrength = displacementStrength,
                smoothNormal = smoothNormal,
                useSmoothNormalMask = useSmoothNormalMask,
                mergeUV = mergeUv,

                albedoToVertexColorMode = albedoToVertexColor,
                albedoMap = albedoMap,
                maskMap = maskMap,

                terrainSize = terrainSize,
                chunkUvRect = uvRect,
                chunkLocalPosition = transform.localPosition,
                texelSize = texelSize
            };

            return job;
        }

        internal void UpdateMesh(int lod, GAlbedoToVertexColorMode albedoToVertexColor)
        {
            Mesh m = GetMesh(lod);
            m.Clear();

            int leafCount = generationMetadata[GGeometryJobUtilities.METADATA_LEAF_COUNT];
            int removedLeafCount = generationMetadata[GGeometryJobUtilities.METADATA_LEAF_REMOVED];

            if (leafCount != removedLeafCount)
            {
                vertexNativeArray.CopyTo(vertexArray);
                uvsNativeArray.CopyTo(uvsArray);
                normalsNativeArray.CopyTo(normalsArray);
                trianglesNativeArray.CopyTo(trianglesArray);

                m.vertices = vertexArray;
                m.uv = uvsArray;
                m.triangles = trianglesArray;
                m.normals = normalsArray;

                if (albedoToVertexColor != GAlbedoToVertexColorMode.None)
                {
                    vertexColorsNativeArray.CopyTo(vertexColorsArray);
                    m.colors32 = vertexColorsArray;
                }

                m.RecalculateBounds();
                RecalculateTangentIfNeeded(m);
            }

            if (lod == 0)
            {
                MeshFilterComponent.sharedMesh = m;
            }
            else
            {
                GTerrainChunkLOD chunkLOD = GetChunkLOD(lod);
                chunkLOD.MeshFilterComponent.sharedMesh = m;
            }

            SetLastUpdatedTimeNow();
        }

        internal void UpdateCollisionMesh()
        {
            Mesh m = GetMesh(0);
            if (m != null)
            {
                MeshColliderComponent.sharedMesh = m;
            }
        }

        internal NativeArray<bool> GetVertexMarkerFromMeshUV(int lod)
        {
            int dimension = GGeometryJobUtilities.VERTEX_MARKER_DIMENSION;
            NativeArray<bool> markers = new NativeArray<bool>(dimension * dimension, Allocator.TempJob);

            Mesh m = GetMesh(lod);
            Vector2[] uvs = m.uv;
            int x = 0;
            int y = 0;
            Vector2 uv = Vector2.zero;
            for (int i = 0; i < uvs.Length; ++i)
            {
                uv = uvs[i];
                x = (int)(uv.x * (dimension - 1));
                y = (int)(uv.y * (dimension - 1));
                markers[GGeometryJobUtilities.To1DIndex(ref x, ref y, ref dimension)] = true;
            }

            //GUtilities.EnsureArrayLength(ref vertexMarker_Cache, markers.Length);
            //markers.CopyTo(vertexMarker_Cache);

            return markers;
        }

        internal NativeArray<bool> GetVertexMarker()
        {
            //if (vertexMarkerNativeArray.IsCreated)
            //{
            //    NativeArray<bool> markers = new NativeArray<bool>(vertexMarkerNativeArray, Allocator.TempJob);
            //    return markers;
            //}
            int dimension = GGeometryJobUtilities.VERTEX_MARKER_DIMENSION;
            if (vertexMarker_Cache != null && vertexMarker_Cache.Length == dimension * dimension)
            {
                NativeArray<bool> markers = new NativeArray<bool>(vertexMarker_Cache, Allocator.TempJob);
                return markers;
            }
            else
            {
                NativeArray<bool> markers = GetVertexMarkerFromMeshUV(0);
                return markers;
            }
        }

        internal int GetGenerationMetadata(int channel)
        {
            return generationMetadata[channel];
        }

        internal void SetupLODGroup(int lodCount)
        {
            float transitionStep = 1.0f / lodCount;
            LOD[] lods = new LOD[lodCount];
            lods[0] = new LOD(
                GRuntimeSettings.Instance.geometryGeneration.lodTransition.Evaluate(transitionStep),
                new Renderer[] { MeshRendererComponent });

            for (int level = 1; level < lodCount; ++level)
            {
                int i = level;
                lods[i] = new LOD(
                    GRuntimeSettings.Instance.geometryGeneration.lodTransition.Evaluate((i + 1) * transitionStep),
                    new Renderer[] { GetChunkLOD(i).MeshRendererComponent });

                GTerrainChunkLOD chunkLod = GetChunkLOD(i);
                chunkLod.MeshFilterComponent.sharedMesh = null;
                chunkLod.MeshRendererComponent.enabled = true;
            }

            LodGroupComponent.SetLODs(lods);
            Internal_UpdateRenderer();

            List<GameObject> childLod = new List<GameObject>();
            int maxIndex = transform.childCount - 1;
            for (int i = lodCount - 1; i <= maxIndex; ++i)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    childLod.Add(transform.GetChild(i).gameObject);
                }
            }
            for (int i = 0; i < childLod.Count; ++i)
            {
                GUtilities.DestroyGameobject(childLod[i]);
            }
        }
    }
}
#endif
