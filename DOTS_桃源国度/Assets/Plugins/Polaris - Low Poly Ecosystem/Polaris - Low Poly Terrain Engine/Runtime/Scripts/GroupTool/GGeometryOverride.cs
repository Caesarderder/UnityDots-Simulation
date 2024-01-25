#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [System.Serializable]
    public struct GGeometryOverride
    {
        [SerializeField]
        private bool overrideWidth;
        public bool OverrideWidth
        {
            get
            {
                return overrideWidth;
            }
            set
            {
                overrideWidth = value;
            }
        }

        [SerializeField]
        private float width;
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
        private bool overrideHeight;
        public bool OverrideHeight
        {
            get
            {
                return overrideHeight;
            }
            set
            {
                overrideHeight = value;
            }
        }

        [SerializeField]
        private float height;
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
        private bool overrideLength;
        public bool OverrideLength
        {
            get
            {
                return overrideLength;
            }
            set
            {
                overrideLength = value;
            }
        }

        [SerializeField]
        private float length;
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

        [SerializeField]
        private bool overrideHeightMapResolution;
        public bool OverrideHeightMapResolution
        {
            get
            {
                return overrideHeightMapResolution;
            }
            set
            {
                overrideHeightMapResolution = value;
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
                heightMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
            }
        }

        [SerializeField]
        private bool overrideMeshBaseResolution;
        public bool OverrideMeshBaseResolution
        {
            get
            {
                return overrideMeshBaseResolution;
            }
            set
            {
                overrideMeshBaseResolution = value;
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
                meshBaseResolution = Mathf.Clamp(value, 0, 10);
            }
        }

        [SerializeField]
        private bool overrideMeshResolution;
        public bool OverrideMeshResolution
        {
            get
            {
                return overrideMeshResolution;
            }
            set
            {
                overrideMeshResolution = value;
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
                meshResolution = Mathf.Clamp(value, 0, 15);
            }
        }

        [SerializeField]
        private bool overrideChunkGridSize;
        public bool OverrideChunkGridSize
        {
            get
            {
                return overrideChunkGridSize;
            }
            set
            {
                overrideChunkGridSize = value;
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
        private bool overrideLodCount;
        public bool OverrideLodCount
        {
            get
            {
                return overrideLodCount;
            }
            set
            {
                overrideLodCount = value;
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
        private bool overrideDisplacementSeed;
        public bool OverrideDisplacementSeed
        {
            get
            {
                return overrideDisplacementSeed;
            }
            set
            {
                overrideDisplacementSeed = value;
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
        private bool overrideDisplacementStrength;
        public bool OverrideDisplacementStrength
        {
            get
            {
                return overrideDisplacementStrength;
            }
            set
            {
                overrideDisplacementStrength = value;
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
                displacementStrength = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        private bool overrideAlbedoToVertexColorMode;
        public bool OverrideAlbedoToVertexColorMode
        {
            get
            {
                return overrideAlbedoToVertexColorMode;
            }
            set
            {
                overrideAlbedoToVertexColorMode = value;
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
        private bool overrideStorageMode;
        public bool OverrideStorageMode
        {
            get
            {
                return overrideStorageMode;
            }
            set
            {
                overrideStorageMode = value;
            }
        }

        [SerializeField]
        private GGeometry.GStorageMode storageMode;
        public GGeometry.GStorageMode StorageMode
        {
            get
            {
                return storageMode;
            }
            set
            {
                storageMode = value;
            }
        }

        [SerializeField]
        private bool overrideAllowTimeSlicedGeneration;
        public bool OverrideAllowTimeSlicedGeneration
        {
            get
            {
                return overrideAllowTimeSlicedGeneration;
            }
            set
            {
                overrideAllowTimeSlicedGeneration = value;
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
        private bool overrideSmoothNormal;
        public bool OverrideSmoothNormal
        {
            get
            {
                return overrideSmoothNormal;
            }
            set
            {
                overrideSmoothNormal = value;
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
        private bool overrideUseSmoothNormalMask;
        public bool OverrideUseSmoothNormalMask
        {
            get
            {
                return overrideUseSmoothNormalMask;
            }
            set
            {
                overrideUseSmoothNormalMask = value;
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
        private bool overrideMergeUv;
        public bool OverrideMergeUv
        {
            get
            {
                return overrideMergeUv;
            }
            set
            {
                overrideMergeUv = value;
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

        public void Reset()
        {
            OverrideWidth = false;
            OverrideHeight = false;
            OverrideLength = false;
            OverrideHeightMapResolution = false;
            OverrideMeshBaseResolution = false;
            OverrideMeshResolution = false;
            OverrideChunkGridSize = false;
            overrideLodCount = false;
            OverrideDisplacementSeed = false;
            OverrideDisplacementStrength = false;
            OverrideAlbedoToVertexColorMode = false;
            OverrideStorageMode = false;
            OverrideAllowTimeSlicedGeneration = false;
            OverrideSmoothNormal = false;
            OverrideUseSmoothNormalMask = false;

            Width = GRuntimeSettings.Instance.geometryDefault.width;
            Height = GRuntimeSettings.Instance.geometryDefault.height;
            Length = GRuntimeSettings.Instance.geometryDefault.length;
            HeightMapResolution = GRuntimeSettings.Instance.geometryDefault.heightMapResolution;
            MeshBaseResolution = GRuntimeSettings.Instance.geometryDefault.meshBaseResolution;
            MeshResolution = GRuntimeSettings.Instance.geometryDefault.meshResolution;
            ChunkGridSize = GRuntimeSettings.Instance.geometryDefault.chunkGridSize;
            LODCount = GRuntimeSettings.Instance.geometryDefault.lodCount;
            DisplacementSeed = GRuntimeSettings.Instance.geometryDefault.displacementSeed;
            DisplacementStrength = GRuntimeSettings.Instance.geometryDefault.displacementStrength;
            AlbedoToVertexColorMode = GRuntimeSettings.Instance.geometryDefault.albedoToVertexColorMode;
            StorageMode = GRuntimeSettings.Instance.geometryDefault.storageMode;
            AllowTimeSlicedGeneration = GRuntimeSettings.Instance.geometryDefault.allowTimeSlicedGeneration;
            SmoothNormal = GRuntimeSettings.Instance.geometryDefault.smoothNormal;
            UseSmoothNormalMask = GRuntimeSettings.Instance.geometryDefault.useSmoothNormalMask;
        }

        public void Override(GGeometry g)
        {
            if (OverrideWidth)
                g.Width = Width;
            if (OverrideHeight)
                g.Height = Height;
            if (OverrideLength)
                g.Length = Length;
            if (OverrideHeightMapResolution)
                g.HeightMapResolution = HeightMapResolution;
            if (OverrideMeshBaseResolution)
                g.MeshBaseResolution = MeshBaseResolution;
            if (OverrideMeshResolution)
                g.MeshResolution = MeshResolution;
            if (OverrideChunkGridSize)
                g.ChunkGridSize = ChunkGridSize;
            if (OverrideLodCount)
                g.LODCount = LODCount;
            if (OverrideDisplacementSeed)
                g.DisplacementSeed = DisplacementSeed;
            if (OverrideDisplacementStrength)
                g.DisplacementStrength = DisplacementStrength;
            if (OverrideAlbedoToVertexColorMode)
                g.AlbedoToVertexColorMode = AlbedoToVertexColorMode;
            if (OverrideStorageMode)
                g.StorageMode = StorageMode;
            if (OverrideAllowTimeSlicedGeneration)
                g.AllowTimeSlicedGeneration = AllowTimeSlicedGeneration;
            if (OverrideSmoothNormal)
                g.SmoothNormal = SmoothNormal;
            if (OverrideUseSmoothNormalMask)
                g.UseSmoothNormalMask = UseSmoothNormalMask;
            if (OverrideMergeUv)
                g.MergeUv = MergeUv;
        }
    }
}
#endif
