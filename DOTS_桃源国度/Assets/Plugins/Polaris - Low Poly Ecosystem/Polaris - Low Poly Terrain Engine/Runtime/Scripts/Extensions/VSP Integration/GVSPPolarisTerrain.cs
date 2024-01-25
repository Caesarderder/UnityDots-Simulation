#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using AwesomeTechnologies.Utility;
using AwesomeTechnologies.Utility.Quadtree;

namespace Pinwheel.Griffin.VegetationStudioPro
{
    public class GVSPPolarisTerrain : MeshTerrain
    {
        public new string TerrainType => "Polaris terrain";
        public GStylizedTerrain Terrain;

        private int _heightmapHeight;
        private int _heightmapWidth;
        private Vector3 _size;
        private Vector3 _scale;
        private Vector3 _heightmapScale;

        private Rect _terrainRect;
        public NativeArray<float> Heights;

        private JobHandle _splatMapHandle;
        private NativeArray<HeightMapSample> _heightMapSamples;
        private NativeArray<float> _currentSplatmapArray;
        private readonly List<NativeArray<float>> _nativeArrayFloatList = new List<NativeArray<float>>();

        public override bool NeedsSplatMapUpdate(Bounds updateBounds)
        {
            return updateBounds.Intersects(TerrainBounds);
        }

        public override void PrepareSplatmapGeneration(bool clearLockedTextures)
        {
            LoadHeightData();

            int width = Terrain.TerrainData.Shading.SplatControlResolution;
            int height = Terrain.TerrainData.Shading.SplatControlResolution;
            int layers = Terrain.TerrainData.Shading.Splats.Prototypes.Count;
            int heightMapLength = width * height;

            if (_heightMapSamples.IsCreated) _heightMapSamples.Dispose();

            _heightMapSamples = new NativeArray<HeightMapSample>(heightMapLength, Allocator.TempJob);
            SampleHeightMapJob sampleHeightMapJob = new SampleHeightMapJob
            {
                HeightMapSamples = _heightMapSamples,
                InputHeights = Heights,
                HeightMapScale = _heightmapScale,
                HeightmapHeight = _heightmapHeight,
                HeightmapWidth = _heightmapWidth,
                Scale = _scale,
                Size = _size,
                Width = width,
                Height = height
            };
            _splatMapHandle = sampleHeightMapJob.Schedule(heightMapLength, 32);


            if (_currentSplatmapArray.IsCreated) _currentSplatmapArray.Dispose();

            _currentSplatmapArray = new NativeArray<float>(width * height * layers, Allocator.TempJob);

            if (!clearLockedTextures)
            {
                float[,,] spltmapArray = Terrain.TerrainData.Shading.GetAlphamaps();
                _currentSplatmapArray.CopyFromFast(spltmapArray);
            }
        }

        public override void GenerateSplatMapBiome(Bounds updateBounds, BiomeType biomeType, List<PolygonBiomeMask> polygonBiomeMaskList, List<TerrainTextureSettings> terrainTextureSettingsList, float heightCurveSampleHeight, float worldSpaceSeaLevel, bool clearLockedTextures)
        {
            int width = Terrain.TerrainData.Shading.SplatControlResolution;
            int height = Terrain.TerrainData.Shading.SplatControlResolution;
            int layers = Terrain.TerrainData.Shading.Splats.Prototypes.Count;

            int blendMaskLength = width * height;
            NativeArray<float> blendMask = new NativeArray<float>(blendMaskLength, Allocator.TempJob);
            NativeArray<float> splatmapArray = new NativeArray<float>(width * height * layers, Allocator.TempJob);

            if (biomeType == BiomeType.Default)
            {
                UnityTerrain.GenerateDefaultBiomeBlendMaskJob generateDefaultBiomeBlendMaskJob =
                    new UnityTerrain.GenerateDefaultBiomeBlendMaskJob { BlendMask = blendMask };
                _splatMapHandle = generateDefaultBiomeBlendMaskJob.Schedule(blendMaskLength, 32, _splatMapHandle);
            }
            else
            {
                for (int i = 0; i <= polygonBiomeMaskList.Count - 1; i++)
                {
                    UnityTerrain.GenerateBlendMaskJob generateBlendMaskJob = new UnityTerrain.GenerateBlendMaskJob
                    {
                        Width = width,
                        Height = height,
                        TerrainSize = _size,
                        TerrainPosition = Terrain.transform.position,
                        BlendMask = blendMask,
                        PolygonArray = polygonBiomeMaskList[i].PolygonArray,
                        SegmentArray = polygonBiomeMaskList[i].SegmentArray,
                        CurveArray = polygonBiomeMaskList[i].TextureCurveArray,
                        UseNoise = polygonBiomeMaskList[i].UseNoise,
                        NoiseScale = polygonBiomeMaskList[i].NoiseScale,
                        BlendDistance = polygonBiomeMaskList[i].BlendDistance,
                        PolygonRect = RectExtension.CreateRectFromBounds(polygonBiomeMaskList[i].MaskBounds),
                        Include = true
                    };

                    _splatMapHandle = generateBlendMaskJob.Schedule(blendMaskLength, 32, _splatMapHandle);
                }
            }

            for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
            {
                if (i >= layers) continue;
                if (terrainTextureSettingsList[i].Enabled)
                {
                    UnityTerrain.ProcessSplatMapJob processSplatMap = new UnityTerrain.ProcessSplatMapJob
                    {
                        Height = height,
                        Width = width,
                        Layers = layers,
                        SplatMapArray = splatmapArray,
                        BlendMask = blendMask,
                        HeightMap = _heightMapSamples,
                        Heights = Heights,
                        TextureIndex = i,
                        TextureUseNoise = terrainTextureSettingsList[i].UseNoise,
                        TextureNoiseScale = terrainTextureSettingsList[i].NoiseScale,
                        TextureWeight = terrainTextureSettingsList[i].TextureWeight,
                        TextureNoiseOffset = terrainTextureSettingsList[i].NoiseOffset,
                        InverseTextureNoise = terrainTextureSettingsList[i].InverseNoise,
                        HeightCurve = terrainTextureSettingsList[i].HeightCurveArray,
                        SteepnessCurve = terrainTextureSettingsList[i].SteepnessCurveArray,
                        TerrainHeight = heightCurveSampleHeight,
                        TerrainYPosition = Terrain.transform.position.y,
                        WorldspaceSeaLevel = worldSpaceSeaLevel,
                        HeightMapScale = _heightmapScale,
                        HeightmapHeight = _heightmapHeight,
                        HeightmapWidth = _heightmapWidth,
                        ConcaveEnable = terrainTextureSettingsList[i].ConcaveEnable,
                        ConvexEnable = terrainTextureSettingsList[i].ConvexEnable,
                        ConcaveAverage = terrainTextureSettingsList[i].ConcaveAverage,
                        ConcaveMinHeightDifference = terrainTextureSettingsList[i].ConcaveMinHeightDifference,
                        ConcaveDistance = terrainTextureSettingsList[i].ConcaveDistance,
                        ConcaveMode = (int)terrainTextureSettingsList[i].ConcaveMode,
                        TerrainSize = _size,
                        TerrainPosition = Terrain.transform.position
                    };
                    _splatMapHandle = processSplatMap.Schedule(width * height * layers, 32, _splatMapHandle);


                }
                else
                {
                    if (!clearLockedTextures && terrainTextureSettingsList[i].LockTexture)
                    {
                        UnityTerrain.CopyLockedDataJob copyLockedDataJobJob = new UnityTerrain.CopyLockedDataJob
                        {
                            Height = height,
                            Width = width,
                            Layers = layers,
                            SplatMapArray = splatmapArray,
                            CurrentSplatMapArray = _currentSplatmapArray,
                            TextureIndex = i,
                        };
                        _splatMapHandle = copyLockedDataJobJob.Schedule(width * height * layers, 32, _splatMapHandle);
                    }
                }
            }


            int firstEnabledIndex = 0;
            for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
            {
                if (terrainTextureSettingsList[i].Enabled)
                {
                    firstEnabledIndex = i;
                    break;
                }
            }

            if (!clearLockedTextures)
            {
                NativeArray<int> lockedTextureArray = new NativeArray<int>(terrainTextureSettingsList.Count, Allocator.TempJob);
                NativeArray<int> automaticGenerationArray = new NativeArray<int>(terrainTextureSettingsList.Count, Allocator.TempJob);
                for (int i = 0; i <= terrainTextureSettingsList.Count - 1; i++)
                {

                    if (terrainTextureSettingsList[i].Enabled)
                    {
                        automaticGenerationArray[i] = 1;
                    }
                    else if (terrainTextureSettingsList[i].LockTexture)
                    {
                        lockedTextureArray[i] = 1;
                    }


                }
                UnityTerrain.NormalizeSplatMapKeepLockedDataJob normalizeSplatMapJob = new UnityTerrain.NormalizeSplatMapKeepLockedDataJob
                {
                    SplatMapArray = splatmapArray,
                    FirstEnabledIndex = firstEnabledIndex,
                    AutomaticGenerationArray = automaticGenerationArray,
                    LockedTextureArray = lockedTextureArray
                };
                _splatMapHandle = normalizeSplatMapJob.ScheduleBatch(width * height * layers, layers, _splatMapHandle);
            }
            else
            {
                UnityTerrain.NormalizeSplatMapJob normalizeSplatMapJob = new UnityTerrain.NormalizeSplatMapJob
                {
                    SplatMapArray = splatmapArray,
                    FirstEnabledIndex = firstEnabledIndex
                };
                _splatMapHandle = normalizeSplatMapJob.ScheduleBatch(width * height * layers, layers, _splatMapHandle);
            }

            //blend biome splatmap against current splatmap
            UnityTerrain.BlendSplatMapJob blendSplatMapJob = new UnityTerrain.BlendSplatMapJob
            {
                CurrentSplatMapArray = _currentSplatmapArray,
                SplatMapArray = splatmapArray,
                BlendMask = blendMask,
                Height = height,
                Width = width,
                Layers = layers
            };
            _splatMapHandle = blendSplatMapJob.Schedule(width * height * layers, 32, _splatMapHandle);

            _nativeArrayFloatList.Add(splatmapArray);
            _nativeArrayFloatList.Add(blendMask);
        }
         
        public override void CompleteSplatmapGeneration()
        {
            _splatMapHandle.Complete();

            int width = Terrain.TerrainData.Shading.SplatControlResolution;
            int height = Terrain.TerrainData.Shading.SplatControlResolution;
            int layers = Terrain.TerrainData.Shading.Splats.Prototypes.Count;

            //float[] splatmap1DArray = new float[width * height * layers];
            //NativeToManagedCopyMemory(splatmap1DArray, _currentSplatmapArray);            
            //_terrain.terrainData.SetAlphamaps(0, 0, To3DArray(splatmap1DArray, width, height, layers));

            float[,,] splatmapArray = new float[width, height, layers];
            _currentSplatmapArray.CopyToFast(splatmapArray);
            Terrain.TerrainData.Shading.SetAlphamaps(splatmapArray);

            if (_heightMapSamples.IsCreated) _heightMapSamples.Dispose();
            if (_currentSplatmapArray.IsCreated) _currentSplatmapArray.Dispose();

            for (int i = 0; i <= _nativeArrayFloatList.Count - 1; i++)
            {
                if (_nativeArrayFloatList[i].IsCreated) _nativeArrayFloatList[i].Dispose();
            }


            //splatmapArray.CopyTo(splatmap1DArray);

            _nativeArrayFloatList.Clear();
        }

        void LoadHeightData()
        {
            var terrainData = Terrain.TerrainData;
            Vector2 _heightmapTexelSize = terrainData.Geometry.HeightMap.texelSize;
            _heightmapScale = new Vector3(_heightmapTexelSize.x * terrainData.Geometry.Width, terrainData.Geometry.Height, _heightmapTexelSize.y * terrainData.Geometry.Length);
            _heightmapHeight = terrainData.Geometry.HeightMapResolution;
            _heightmapWidth = terrainData.Geometry.HeightMapResolution;

            _size = terrainData.Geometry.Size;
            _scale.x = _size.x / (_heightmapWidth - 1);
            _scale.y = _size.y;
            _scale.z = _size.z / (_heightmapHeight - 1);

            Vector2 terrainCenter = new Vector2(Terrain.transform.position.x, Terrain.transform.position.z);
            Vector2 terrainSize = new Vector2(_size.x, _size.z);
            _terrainRect = new Rect(terrainCenter, terrainSize);

            float[,] hs = Terrain.TerrainData.Geometry.GetHeights();

            if (Heights.IsCreated) Heights.Dispose();

            Heights = new NativeArray<float>(_heightmapWidth * _heightmapHeight, Allocator.Persistent);
            Heights.CopyFromFast(hs);
        }
    }
}
#endif
