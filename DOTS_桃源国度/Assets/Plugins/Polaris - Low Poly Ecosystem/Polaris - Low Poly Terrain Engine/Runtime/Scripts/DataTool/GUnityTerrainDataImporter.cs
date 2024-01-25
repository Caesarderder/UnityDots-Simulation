#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using GC = System.GC;
using Rand = System.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainDataImporter
    {
        public Terrain SrcTerrain { get; set; }
        public TerrainData SrcData { get; set; }
        public GTerrainData DesData { get; set; }
        public GStylizedTerrain DesTerrain { get; set; }
        public bool ImportGeometry { get; set; }
        public bool UseUnityTerrainSize { get; set; }
        public bool ImportSplats { get; set; }
        public bool ImportSplatsAsAlbedo { get; set; }
        public bool ImportSplatControlMapsOnly { get; set; }
        public bool ImportSplatControlMapResolution { get; set; }
        public bool CreateNewSplatPrototypesGroup { get; set; }
        public bool ImportTrees { get; set; }
        public bool ImportTreeInstancesOnly { get; set; }
        public bool CreateNewTreePrototypesGroup { get; set; }
        public bool ImportGrasses { get; set; }
        public bool ImportGrassInstancesOnly { get; set; }
        public bool CreateNewGrassPrototypesGroup { get; set; }
        public float GrassDensity { get; set; }
        public bool SkipFoliageSnap { get; set; }

        public void Import()
        {
            try
            {
                if (ImportSplats)
                {
#if UNITY_EDITOR
                    GCommonGUI.CancelableProgressBar("Working...", "Importing Splats...", 1f);
#endif
                    DoImportSplats();
                }
                if (ImportTrees)
                {
#if UNITY_EDITOR
                    GCommonGUI.CancelableProgressBar("Working...", "Importing Trees...", 1f);
#endif
                    DoImportTrees();
                }
                if (ImportGrasses)
                {
#if UNITY_EDITOR
                    GCommonGUI.CancelableProgressBar("Working...", "Importing Grasses & Details...", 1f);
#endif
                    DoImportGrasses();
                }
                if (ImportGeometry)
                {
#if UNITY_EDITOR
                    GCommonGUI.CancelableProgressBar("Working...", "Importing Geometry...", 1f);
#endif
                    DoImportGeometry();
                }

            }
            catch (GProgressCancelledException)
            {
                Debug.Log("Importing process canceled!");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
        }

        private void DoImportGeometry()
        {
            if (UseUnityTerrainSize)
            {
                DesData.Geometry.Width = SrcData.size.x;
                DesData.Geometry.Height = SrcData.size.y;
                DesData.Geometry.Length = SrcData.size.z;
            }
            int hmResolution = SrcData.heightmapResolution - 1;
            DesData.Geometry.HeightMapResolution = hmResolution;

            float[,] heightSample = SrcData.GetHeights(0, 0, DesData.Geometry.HeightMapResolution, DesData.Geometry.HeightMapResolution);
            Color[] heightMapColor = new Color[DesData.Geometry.HeightMapResolution * DesData.Geometry.HeightMapResolution];
            float h = 0;
            Vector2 enc;

            int length = heightSample.GetLength(0);
            int width = heightSample.GetLength(1);
            for (int z = 0; z < length; ++z)
            {
                for (int x = 0; x < width; ++x)
                {
                    h = Mathf.Clamp(heightSample[z, x], 0f, 0.99999f);
                    enc = GCommon.EncodeTerrainHeight(h);
                    heightMapColor[GUtilities.To1DIndex(x, z, width)] = new Color(enc.x, enc.y, 0, 0);
                }
            }

            DesData.Geometry.HeightMap.SetPixels(heightMapColor);
            DesData.Geometry.HeightMap.Apply();
            DesData.Geometry.SetRegionDirty(GCommon.UnitRect);
            DesData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);

            if (!SkipFoliageSnap)
            {
                if (DesTerrain != null)
                {
                    DesData.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                    DesData.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                    DesTerrain.UpdateTreesPosition();
                    DesTerrain.UpdateGrassPatches();
                    DesData.Foliage.ClearTreeDirtyRegions();
                    DesData.Foliage.ClearGrassDirtyRegions();
                }
            }

            //GC.Collect();
        }

        private void DoImportSplats()
        {
            if (!ImportSplatControlMapsOnly)
            {
                GSplatPrototypeGroup splatGroup = DesData.Shading.Splats;
                if (splatGroup == null ||
                    splatGroup.IsSampleAsset)
                {
                    CreateNewSplatPrototypesGroup = true;
                }

                if (CreateNewSplatPrototypesGroup)
                {
                    splatGroup = ScriptableObject.CreateInstance<GSplatPrototypeGroup>();

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = AssetDatabase.GetAssetPath(DesData);
                        string directory = Path.GetDirectoryName(path);
                        string filePath = Path.Combine(directory, string.Format("Splats_{0}_{1}.asset", DesData.Id, System.DateTime.Now.Ticks));
                        AssetDatabase.CreateAsset(splatGroup, filePath);
                    }
#endif
                    DesData.Shading.Splats = splatGroup;
                }

                TerrainLayer[] layers = SrcData.terrainLayers;
                GSplatPrototype[] prototypes = new GSplatPrototype[layers.Length];
                for (int i = 0; i < layers.Length; ++i)
                {
                    if (layers[i] != null)
                    {
                        GSplatPrototype p = (GSplatPrototype)layers[i];
                        prototypes[i] = p;
                    }
                    else
                    {
                        prototypes[i] = new GSplatPrototype();
                    }
                }
                splatGroup.Prototypes = new List<GSplatPrototype>(prototypes);
                GCommon.SetDirty(splatGroup);
            }

            if (ImportSplatControlMapResolution)
            {
                DesData.Shading.SplatControlResolution = SrcData.alphamapResolution;
            }

            Texture2D[] alphaMaps = SrcData.alphamapTextures;
            int alphaMapCount = Mathf.Min(alphaMaps.Length, DesData.Shading.SplatControlMapCount);
            for (int i = 0; i < alphaMapCount; ++i)
            {
                try
                {
                    Texture2D controlMap = DesData.Shading.GetSplatControl(i);
                    //controlMap.SetPixels(alphaMaps[i].GetPixels());
                    //controlMap.Apply();
                    GCommon.CopyTexture(alphaMaps[i], controlMap);
                    controlMap.filterMode = alphaMaps[i].filterMode;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("Skip import splat alpha map {0}, error: {1}", alphaMaps[i].name, e.ToString()));
                }
            }

            if (ImportSplatsAsAlbedo)
            {
                DesData.Shading.ConvertSplatsToAlbedo();
            }

            DesData.SetDirty(GTerrainData.DirtyFlags.Shading);
            //GC.Collect();
        }

        private void DoImportTrees()
        {
            if (!ImportTreeInstancesOnly)
            {
                GTreePrototypeGroup treeGroup = DesData.Foliage.Trees;
                if (treeGroup == null ||
                    treeGroup.IsSampleAsset)
                {
                    CreateNewTreePrototypesGroup = true;
                }

                if (CreateNewTreePrototypesGroup)
                {
                    treeGroup = ScriptableObject.CreateInstance<GTreePrototypeGroup>();

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = AssetDatabase.GetAssetPath(DesData);
                        string directory = Path.GetDirectoryName(path);
                        string filePath = Path.Combine(directory, string.Format("Trees_{0}_{1}.asset", DesData.Id, System.DateTime.Now.Ticks));
                        AssetDatabase.CreateAsset(treeGroup, filePath);
                    }
#endif
                    DesData.Foliage.Trees = treeGroup;
                }

                treeGroup.Prototypes.Clear();
                TreePrototype[] treePrototypes = SrcData.treePrototypes;
                for (int i = 0; i < treePrototypes.Length; ++i)
                {
                    GTreePrototype proto = (GTreePrototype)treePrototypes[i];
                    treeGroup.Prototypes.Add(proto);
                }
                GCommon.SetDirty(treeGroup);
            }

            DesData.Foliage.TreeInstances.Clear();
            TreeInstance[] treeInstances = SrcData.treeInstances;
            for (int i = 0; i < treeInstances.Length; ++i)
            {
                GTreeInstance t = (GTreeInstance)treeInstances[i];
                DesData.Foliage.TreeInstances.Add(t);
            }

            if (DesTerrain != null)
            {
                DesData.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                DesTerrain.UpdateTreesPosition();
                DesData.Foliage.ClearTreeDirtyRegions();
            }

            DesData.SetDirty(GTerrainData.DirtyFlags.Foliage);
            //GC.Collect();
        }

        private void DoImportGrasses()
        {
            if (!ImportGrassInstancesOnly)
            {
                GGrassPrototypeGroup grassesGroup = DesData.Foliage.Grasses;
                if (grassesGroup == null ||
                    grassesGroup.IsSampleAsset)
                {
                    CreateNewGrassPrototypesGroup = true;
                }

                if (CreateNewGrassPrototypesGroup)
                {
                    grassesGroup = ScriptableObject.CreateInstance<GGrassPrototypeGroup>();

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = AssetDatabase.GetAssetPath(DesData);
                        string directory = Path.GetDirectoryName(path);
                        string filePath = Path.Combine(directory, string.Format("Grasses_{0}_{1}.asset", DesData.Id, System.DateTime.Now.Ticks));
                        AssetDatabase.CreateAsset(grassesGroup, filePath);
                    }
#endif
                    DesData.Foliage.Grasses = grassesGroup;
                }

                grassesGroup.Prototypes.Clear();

                DetailPrototype[] detailPrototypes = SrcData.detailPrototypes;
                for (int i = 0; i < detailPrototypes.Length; ++i)
                {
                    GGrassPrototype proto = (GGrassPrototype)detailPrototypes[i];
                    grassesGroup.Prototypes.Add(proto);
                }
                GCommon.SetDirty(grassesGroup);
            }

            List<GGrassInstance> grasses = new List<GGrassInstance>();
            int detailResolution = SrcData.detailResolution;
            int detailLayerCount = SrcData.detailPrototypes.Length;
            for (int layer = 0; layer < detailLayerCount; ++layer)
            {
                int[,] density = SrcData.GetDetailLayer(0, 0, detailResolution, detailResolution, layer);
                DoImportDetailLayer(layer, density, grasses);
            }

            DesData.Foliage.ClearGrassInstances();
            DesData.Foliage.AddGrassInstances(grasses);
            if (DesTerrain != null)
            {
                DesData.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                DesTerrain.UpdateGrassPatches();
                DesData.Foliage.ClearGrassDirtyRegions();
            }

            DesData.SetDirty(GTerrainData.DirtyFlags.Foliage);
            //GC.Collect();
        }

        private void DoImportDetailLayer(int layerIndex, int[,] density, List<GGrassInstance> grasses)
        {
            GGrassPrototype prototype = DesData.Foliage.Grasses.Prototypes[layerIndex];

            Rand rand = new Rand(layerIndex + SrcData.detailResolution);
            int resolution = SrcData.detailResolution;
            int d = 0;
            float maxOffset = 1.0f / resolution;
            Vector2 uv = Vector2.zero;
            Vector3 pos = Vector3.zero;

            for (int z = 0; z < resolution; ++z)
            {
                for (int x = 0; x < resolution; ++x)
                {
                    d = density[z, x];
                    for (int i = 0; i < d; ++i)
                    {
                        if (rand.NextDouble() > GrassDensity)
                            continue;
                        uv.Set(
                            Mathf.InverseLerp(0, resolution - 1, x),
                            Mathf.InverseLerp(0, resolution - 1, z));
                        pos.Set(
                            (float)(uv.x + rand.NextDouble() * maxOffset),
                            0,
                            (float)(uv.y + rand.NextDouble() * maxOffset));


                        GGrassInstance grass = GGrassInstance.Create(layerIndex);
                        grass.Position = pos;
                        grass.Rotation = Quaternion.Euler(0, (float)rand.NextDouble() * 360f, 0);
                        grass.Scale = Mathf.Lerp(0.5f, 1f, (float)rand.NextDouble()) * Vector3.one;

                        grasses.Add(grass);
                    }
                }
            }

            //GC.Collect();
        }
    }
}
#endif
