#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Type = System.Type;

namespace Pinwheel.Griffin
{
    public class GGeometry : ScriptableObject
    {
        [System.Serializable]
        public enum GStorageMode
        {
            SaveToAsset, GenerateOnEnable
        }

        public const string HEIGHT_MAP_NAME = "Height Map";

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
        internal float width;
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        internal float height;
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        internal float length;
        public float Length
        {
            get
            {
                return length;
            }
            set
            {
                length = Mathf.Max(1, value);
            }
        }

        public Vector3 Size
        {
            get
            {
                return new Vector3(Width, Height, Length);
            }
        }

        [SerializeField]
        private int heightMapResolution;
        public int HeightMapResolution
        {
            get
            {
                return heightMapResolution;
            }
            set
            {
                int oldValue = heightMapResolution;
                heightMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
                if (oldValue != heightMapResolution)
                {
                    ResampleHeightMap();
                }
            }
        }

        [SerializeField]
        private Texture2D heightMap;
        public Texture2D HeightMap
        {
            get
            {
                if (heightMap == null)
                {
                    heightMap = GCommon.CreateTexture(HeightMapResolution, Color.clear, HeightMapFormat);
                    heightMap.filterMode = FilterMode.Bilinear;
                    heightMap.wrapMode = TextureWrapMode.Clamp;
                    heightMap.name = HEIGHT_MAP_NAME;
                    heightmapVersion = GVersionInfo.Number;
                }
                GCommon.TryAddObjectToAsset(heightMap, TerrainData);
                if (heightMap.format != HeightMapFormat)
                {
                    ReFormatHeightMap();
                }
                return heightMap;
            }
        }

        [SerializeField]
        private float heightmapVersion;
        private const float HEIGHT_MAP_VERSION_ENCODE_RG = 246;

        public static TextureFormat HeightMapFormat
        {
            get
            {
                return TextureFormat.RGBA32;
            }
        }

        public static RenderTextureFormat HeightMapRTFormat
        {
            get
            {
                return RenderTextureFormat.ARGB32;
            }
        }

        internal Texture2D subDivisionMap;
        public Texture2D Internal_SubDivisionMap
        {
            get
            {
                if (subDivisionMap == null)
                {
                    Internal_CreateNewSubDivisionMap();
                }
                return subDivisionMap;
            }
        }

        [SerializeField]
        private int meshBaseResolution;
        public int MeshBaseResolution
        {
            get
            {
                return meshBaseResolution;
            }
            set
            {
                meshBaseResolution = Mathf.Min(meshResolution, Mathf.Clamp(value, 0, GCommon.MAX_MESH_BASE_RESOLUTION));
            }
        }

        [SerializeField]
        private int meshResolution;
        public int MeshResolution
        {
            get
            {
                return meshResolution;
            }
            set
            {
                meshResolution = Mathf.Clamp(value, 0, GCommon.MAX_MESH_RESOLUTION);
            }
        }

        [SerializeField]
        private int chunkGridSize;
        public int ChunkGridSize
        {
            get
            {
                return chunkGridSize;
            }
            set
            {
                chunkGridSize = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private int lodCount;
        public int LODCount
        {
            get
            {
                return lodCount;
            }
            set
            {
                lodCount = Mathf.Clamp(value, 1, GCommon.MAX_LOD_COUNT);
            }
        }

        [SerializeField]
        private int displacementSeed;
        public int DisplacementSeed
        {
            get
            {
                return displacementSeed;
            }
            set
            {
                displacementSeed = value;
            }
        }

        [SerializeField]
        private float displacementStrength;
        public float DisplacementStrength
        {
            get
            {
                return displacementStrength;
            }
            set
            {
                displacementStrength = Mathf.Max(0, value);
            }
        }

        [SerializeField]
        private GAlbedoToVertexColorMode albedoToVertexColorMode;
        public GAlbedoToVertexColorMode AlbedoToVertexColorMode
        {
            get
            {
                return albedoToVertexColorMode;
            }
            set
            {
                albedoToVertexColorMode = value;
            }
        }

        [SerializeField]
        private GStorageMode storageMode;
        public GStorageMode StorageMode
        {
            get
            {
                return storageMode;
            }
            set
            {
                storageMode = value;
                if (storageMode == GStorageMode.GenerateOnEnable)
                {
                    TerrainData.GeometryData = null;
                }
            }
        }

        [SerializeField]
        private bool allowTimeSlicedGeneration;
        public bool AllowTimeSlicedGeneration
        {
            get
            {
                return allowTimeSlicedGeneration;
            }
            set
            {
                allowTimeSlicedGeneration = value;
            }
        }

        [SerializeField]
        private bool smoothNormal;
        public bool SmoothNormal
        {
            get
            {
                return smoothNormal;
            }
            set
            {
                smoothNormal = value;
            }
        }

        [SerializeField]
        private bool useSmoothNormalMask;
        public bool UseSmoothNormalMask
        {
            get
            {
                return useSmoothNormalMask;
            }
            set
            {
                useSmoothNormalMask = value;
            }
        }

        [SerializeField]
        private bool mergeUv;
        public bool MergeUv
        {
            get
            {
                return mergeUv;
            }
            set
            {
                mergeUv = value;
            }
        }

        private List<Rect> dirtyRegion;
        private List<Rect> DirtyRegion
        {
            get
            {
                if (dirtyRegion == null)
                {
                    dirtyRegion = new List<Rect>();
                }
                return dirtyRegion;
            }
            set
            {
                dirtyRegion = value;
            }
        }

        public void Reset()
        {
            name = "Geometry";
            Width = GRuntimeSettings.Instance.geometryDefault.width;
            Height = GRuntimeSettings.Instance.geometryDefault.height;
            Length = GRuntimeSettings.Instance.geometryDefault.length;
            HeightMapResolution = GRuntimeSettings.Instance.geometryDefault.heightMapResolution;
            MeshResolution = GRuntimeSettings.Instance.geometryDefault.meshResolution;
            MeshBaseResolution = GRuntimeSettings.Instance.geometryDefault.meshBaseResolution;
            ChunkGridSize = GRuntimeSettings.Instance.geometryDefault.chunkGridSize;
            LODCount = GRuntimeSettings.Instance.geometryDefault.lodCount;
            DisplacementSeed = GRuntimeSettings.Instance.geometryDefault.displacementSeed;
            DisplacementStrength = GRuntimeSettings.Instance.geometryDefault.displacementStrength;
            AlbedoToVertexColorMode = GRuntimeSettings.Instance.geometryDefault.albedoToVertexColorMode;
            StorageMode = GRuntimeSettings.Instance.geometryDefault.storageMode;
            AllowTimeSlicedGeneration = GRuntimeSettings.Instance.geometryDefault.allowTimeSlicedGeneration;
            SmoothNormal = GRuntimeSettings.Instance.geometryDefault.smoothNormal;
            UseSmoothNormalMask = GRuntimeSettings.Instance.geometryDefault.useSmoothNormalMask;
            MergeUv = GRuntimeSettings.Instance.geometryDefault.mergeUv;
        }

        public void ResetFull()
        {
            Reset();
            GCommon.FillTexture(HeightMap, Color.clear);
            SetRegionDirty(GCommon.UnitRect);
            TerrainData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
        }

        private void ResampleHeightMap()
        {
            if (heightMap == null)
                return;
            Texture2D tmp = GCommon.CreateTexture(HeightMapResolution, Color.clear, HeightMapFormat);
            RenderTexture rt = new RenderTexture(HeightMapResolution, HeightMapResolution, 32, HeightMapRTFormat);
            
            GCommon.CopyTexture(heightMap, tmp);
            tmp.name = heightMap.name;
            tmp.filterMode = heightMap.filterMode;
            tmp.wrapMode = heightMap.wrapMode;
            Object.DestroyImmediate(heightMap, true);
            heightMap = tmp;
            GCommon.TryAddObjectToAsset(heightMap, TerrainData);

            Internal_CreateNewSubDivisionMap();
            SetRegionDirty(GCommon.UnitRect);
        }

        private void ReFormatHeightMap()
        {
            if (heightMap == null)
                return;
            if (heightmapVersion < HEIGHT_MAP_VERSION_ENCODE_RG)
            {
                Texture2D tmp = GCommon.CreateTexture(HeightMapResolution, Color.clear, HeightMapFormat);
                RenderTexture rt = new RenderTexture(HeightMapResolution, HeightMapResolution, 32, HeightMapRTFormat);
                Material mat = GInternalMaterials.HeightmapConverterEncodeRGMaterial;
                mat.SetTexture("_MainTex", heightMap);
                GCommon.DrawQuad(rt, GCommon.FullRectUvPoints, mat, 0);
                GCommon.CopyFromRT(tmp, rt);
                rt.Release();
                Object.DestroyImmediate(rt);

                tmp.name = heightMap.name;
                tmp.filterMode = heightMap.filterMode;
                tmp.wrapMode = heightMap.wrapMode;
                Object.DestroyImmediate(heightMap, true);
                heightMap = tmp;
                GCommon.TryAddObjectToAsset(heightMap, TerrainData);

                heightmapVersion = HEIGHT_MAP_VERSION_ENCODE_RG;
                Debug.Log("Polaris auto upgrade: Converted Height Map from RGBAFloat to RGBA32.");
            }
        }

        internal void Internal_CreateNewSubDivisionMap()
        {
            if (subDivisionMap != null)
            {
                if (subDivisionMap.width != GCommon.SUB_DIV_MAP_RESOLUTION ||
                    subDivisionMap.height != GCommon.SUB_DIV_MAP_RESOLUTION)
                    Object.DestroyImmediate(subDivisionMap);
            }

            if (subDivisionMap == null)
            {
                subDivisionMap = new Texture2D(GCommon.SUB_DIV_MAP_RESOLUTION, GCommon.SUB_DIV_MAP_RESOLUTION, TextureFormat.RGBA32, false);
            }

            int resolution = GCommon.SUB_DIV_MAP_RESOLUTION;
            RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
            Material mat = GInternalMaterials.SubDivisionMapMaterial;
            Graphics.Blit(HeightMap, rt, mat);
            GCommon.CopyFromRT(subDivisionMap, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
        }

        internal void Internal_CreateNewSubDivisionMap(Texture altHeightMap)
        {
            if (subDivisionMap != null)
            {
                if (subDivisionMap.width != GCommon.SUB_DIV_MAP_RESOLUTION ||
                    subDivisionMap.height != GCommon.SUB_DIV_MAP_RESOLUTION)
                    Object.DestroyImmediate(subDivisionMap);
            }

            if (subDivisionMap == null)
            {
                subDivisionMap = new Texture2D(GCommon.SUB_DIV_MAP_RESOLUTION, GCommon.SUB_DIV_MAP_RESOLUTION, TextureFormat.ARGB32, false);
            }

            int resolution = GCommon.SUB_DIV_MAP_RESOLUTION;
            RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
            Material mat = GInternalMaterials.SubDivisionMapMaterial;
            Graphics.Blit(altHeightMap, rt, mat);
            GCommon.CopyFromRT(subDivisionMap, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
        }

        public void CleanUp()
        {
            int count = 0;
            List<Vector3Int> keys = TerrainData.GeometryData.GetKeys();
            for (int i = 0; i < keys.Count; ++i)
            {
                bool delete = false;
                try
                {
                    int indexX = keys[i].x;
                    int indexY = keys[i].y;
                    int lod = keys[i].z;
                    if (indexX >= ChunkGridSize || indexY >= ChunkGridSize)
                    {
                        delete = true;
                    }
                    else if (lod >= LODCount)
                    {
                        delete = true;
                    }
                    else
                    {
                        delete = false;
                    }
                }
                catch
                {
                    delete = false;
                }

                if (delete)
                {
                    count += 1;
                    TerrainData.GeometryData.DeleteMesh(keys[i]);
                }
            }

            if (count > 0)
            {
                Debug.Log(string.Format("Deleted {0} object{1} from generated data!", count, count > 1 ? "s" : ""));
            }
        }

        public void SetRegionDirty(Rect uvRect)
        {
            DirtyRegion.Add(uvRect);
        }

        public void SetRegionDirty(IEnumerable<Rect> uvRects)
        {
            DirtyRegion.AddRange(uvRects);
        }

        public Rect[] GetDirtyRegions()
        {
            return DirtyRegion.ToArray();
        }

        public void ClearDirtyRegions()
        {
            DirtyRegion.Clear();
        }

        public void CopyTo(GGeometry des)
        {
            des.Width = Width;
            des.Height = Height;
            des.Length = Length;
            des.HeightMapResolution = HeightMapResolution;
            des.MeshResolution = MeshResolution;
            des.MeshBaseResolution = MeshBaseResolution;
            des.ChunkGridSize = ChunkGridSize;
            des.LODCount = LODCount;
            des.DisplacementSeed = DisplacementSeed;
            des.DisplacementStrength = DisplacementStrength;
            des.AlbedoToVertexColorMode = AlbedoToVertexColorMode;
            des.StorageMode = StorageMode;
            des.AllowTimeSlicedGeneration = AllowTimeSlicedGeneration;
        }

        public Vector4 GetDecodedHeightMapSample(Vector2 uv)
        {
            Vector4 c = HeightMap.GetPixelBilinear(uv.x, uv.y);
            Vector2 encodedHeight = new Vector2(c.x, c.y);
            float decodedHeight = GCommon.DecodeTerrainHeight(encodedHeight);
            c.x = decodedHeight;
            c.y = decodedHeight;
            return c;
        }

        public float GetHeightMapMemoryStats()
        {
            if (heightMap == null)
                return 0;
            return heightMap.width * heightMap.height * 4;
        }

        public void RemoveHeightMap()
        {
            if (heightMap != null)
            {
                GUtilities.DestroyObject(heightMap);
            }
        }

        public float[,] GetHeights()
        {
            int res = HeightMapResolution;
            float[,] samples = new float[res, res];
            Vector4 color;

            for (int z = 0; z < res; ++z)
            {
                for (int x = 0; x< res; ++x)
                {
                    color = HeightMap.GetPixel(x, z);
                    float h = GCommon.DecodeTerrainHeight(color);
                    samples[z, x] = h; 
                }
            }
            return samples;
        }
    }
}
#endif
