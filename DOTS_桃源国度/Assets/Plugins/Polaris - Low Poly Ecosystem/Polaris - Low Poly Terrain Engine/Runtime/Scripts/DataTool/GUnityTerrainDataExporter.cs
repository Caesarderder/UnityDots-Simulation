#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainDataExporter
    {
        public GTerrainData SrcData { get; set; }
        public TerrainData DesData { get; set; }
        public bool CreateNewAsset { get; set; }
        public bool ExportGeometry { get; set; }
        public bool ExportTerrainSize { get; set; }
        public bool ExportSplats { get; set; }
        public bool OverwriteSplatLayers { get; set; }
        public bool ExportTrees { get; set; }
        public bool ExportGrasses { get; set; }
        public string DataDirectory { get; set; }

        public void Export()
        {
            try
            {
#if UNITY_EDITOR
                GCommonGUI.ProgressBar("Working", "Saving unsaved assets...", 1f);
                //SaveAssets();
                GCommonGUI.ProgressBar("Working", "Exporting", 1f);
#endif

                InitializeAssets();
                if (ExportGeometry)
                    DoExportGeometry();
                if (ExportSplats)
                    DoExportSplats();
                if (ExportTrees)
                    DoExportTrees();
                if (ExportGrasses)
                    DoExportGrasses();

#if UNITY_EDITOR
                GCommonGUI.ProgressBar("Working", "Saving unsaved assets...", 1f);
                //SaveAssets();
#endif
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

        private void SaveAssets()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        private void InitializeAssets()
        {
            if (CreateNewAsset)
            {
                DesData = new TerrainData();
                DesData.SetDetailResolution(512, 32);

#if UNITY_EDITOR
                GUtilities.EnsureDirectoryExists(DataDirectory);
                string fileName = string.Format("TerrainData_{0}.asset", SrcData.Id);
                string filePath = Path.Combine(DataDirectory, fileName);
                AssetDatabase.CreateAsset(DesData, filePath);
#endif
            }
            if (ExportSplats)
            {
#if !UNITY_2018_1 && !UNITY_2018_2
                TerrainLayer[] layers = DesData.terrainLayers;
                DesData.terrainLayers = null;

                GSplatPrototypeGroup group = SrcData.Shading.Splats;
                if (group != null)
                {
                    List<GSplatPrototype> prototypes = group.Prototypes;
                    List<TerrainLayer> newLayers = new List<TerrainLayer>();
                    List<TerrainLayer> layerToSave = new List<TerrainLayer>();
                    for (int i = 0; i < prototypes.Count; ++i)
                    {
                        if (i < layers.Length && layers[i] != null && OverwriteSplatLayers)
                        {
                            prototypes[i].CopyTo(layers[i]);
                            newLayers.Add(layers[i]);
                        }
                        else
                        {
                            TerrainLayer l = (TerrainLayer)prototypes[i];
                            layerToSave.Add(l);
                            newLayers.Add(l);
                        }
                    }
                    DesData.terrainLayers = newLayers.ToArray();

#if UNITY_EDITOR
                    if (layerToSave.Count > 0)
                    {
                        GUtilities.EnsureDirectoryExists(DataDirectory);
                        for (int i = 0; i < layerToSave.Count; ++i)
                        {
                            string fileName = string.Format("TerrainLayer_{0}_{1}.asset",
                                layerToSave[i].diffuseTexture != null ? layerToSave[i].diffuseTexture.name : i.ToString(),
                                System.DateTime.Now.Ticks);
                            string filePath = Path.Combine(DataDirectory, fileName);
                            AssetDatabase.CreateAsset(layerToSave[i], filePath);
                        }
                    }
#endif
                }
#else
                DesData.splatPrototypes = null;
                GSplatPrototypeGroup group = SrcData.Shading.Splats;
                if (group != null)
                {
                    SplatPrototype[] layers = new SplatPrototype[group.Prototypes.Count];
                    for (int i = 0; i < layers.Length; ++i)
                    {
                        layers[i] = (SplatPrototype)group.Prototypes[i];
                    }
                    DesData.splatPrototypes = layers;
                }
#endif
            }
        }

        private void DoExportGeometry()
        {
            DesData.heightmapResolution = SrcData.Geometry.HeightMapResolution + 1;
            int resolution = DesData.heightmapResolution;
            float[,] heightSample = new float[resolution, resolution];
            Vector2 uv = Vector2.zero;
            Vector2 enc = Vector2.zero;
            float h = 0;
            for (int z = 0; z < resolution; ++z)
            {
                for (int x = 0; x < resolution; ++x)
                {
                    uv.Set(
                        Mathf.InverseLerp(0, resolution - 1, x),
                        Mathf.InverseLerp(0, resolution - 1, z));
                    enc = (Vector4)SrcData.Geometry.HeightMap.GetPixelBilinear(uv.x, uv.y);
                    h = GCommon.DecodeTerrainHeight(enc);
                    heightSample[z, x] = h;
                }
            }

            DesData.SetHeights(0, 0, heightSample);

            if (ExportTerrainSize)
            {
                DesData.size = new Vector3(SrcData.Geometry.Width, SrcData.Geometry.Height, SrcData.Geometry.Length);
            }
        }

        private void DoExportSplats()
        {
            DesData.alphamapResolution = SrcData.Shading.SplatControlResolution;
            Texture2D[] alphaMaps = DesData.alphamapTextures;
            for (int i = 0; i < alphaMaps.Length; ++i)
            {
                Texture2D controlMap = SrcData.Shading.GetSplatControlOrDefault(i);
                GCommon.CopyTexture(controlMap, alphaMaps[i]);
            }
        }

        private void DoExportTrees()
        {
            DesData.treeInstances = new TreeInstance[0];
            DesData.treePrototypes = new TreePrototype[0];

            if (SrcData.Foliage.Trees == null)
                return;

            TreePrototype[] treePrototypes = new TreePrototype[SrcData.Foliage.Trees.Prototypes.Count];
            for (int i = 0; i < treePrototypes.Length; ++i)
            {
                treePrototypes[i] = (TreePrototype)SrcData.Foliage.Trees.Prototypes[i];
            }
            DesData.treePrototypes = treePrototypes;

            TreeInstance[] treeInstances = new TreeInstance[SrcData.Foliage.TreeInstances.Count];
            for (int i = 0; i < treeInstances.Length; ++i)
            {
                treeInstances[i] = (TreeInstance)SrcData.Foliage.TreeInstances[i];
            }
            DesData.treeInstances = treeInstances;
        }

        private void DoExportGrasses()
        {
            DesData.detailPrototypes = new DetailPrototype[0];

            if (SrcData.Foliage.Grasses == null)
                return;

            DetailPrototype[] detailPrototypes = new DetailPrototype[SrcData.Foliage.Grasses.Prototypes.Count];
            for (int i = 0; i < detailPrototypes.Length; ++i)
            {
                detailPrototypes[i] = (DetailPrototype)SrcData.Foliage.Grasses.Prototypes[i];
            }
            DesData.detailPrototypes = detailPrototypes;
            DesData.RefreshPrototypes();

            List<GGrassInstance> instances = SrcData.Foliage.GetGrassInstances();
            if (DesData.detailResolution <= 0)
            {
                DesData.SetDetailResolution(512, 32);
                Debug.Log("Detail Resolution is invalid, set to default value (512/32)");
            }

            int resolution = DesData.detailResolution;
            for (int layer = 0; layer < detailPrototypes.Length; ++layer)
            {
                int x = 0;
                int z = 0;
                int[,] density = new int[resolution, resolution];
                for (int i = 0; i < instances.Count; ++i)
                {
                    if (instances[i].PrototypeIndex != layer)
                        continue;
                    GGrassInstance g = instances[i];
                    x = Mathf.FloorToInt(Mathf.Lerp(0, resolution - 1, g.Position.x));
                    z = Mathf.FloorToInt(Mathf.Lerp(0, resolution - 1, g.Position.z));

                    density[z, x] += 1;
                }

                DesData.SetDetailLayer(0, 0, layer, density);
            }
        }
    }
}
#endif
