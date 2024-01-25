#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Pinwheel.Griffin.Physic;

namespace Pinwheel.Griffin.DataTool
{
    public class GUnityTerrainGroupConverter
    {
        public enum GAlbedoUsage
        {
            None, ColorMap, VertexColor
        }

        public GameObject Root { get; set; }
        public GTerrainData DataTemplate { get; set; }
        public bool ImportGeometry { get; set; }
        public bool ImportSplats { get; set; }
        public bool ImportSplatsAsAlbedo { get; set; }
        public GAlbedoUsage AlbedoUsage { get; set; }
        public bool ImportTrees { get; set; }
        public bool ImportGrasses { get; set; }
        public bool SkipFoliageSnap { get; set; }
        public string DataDirectory { get; set; }

        private string conversionName;
        private string ConversionName
        {
            get
            {
                if (string.IsNullOrEmpty(conversionName))
                {
                    conversionName = System.DateTime.Now.Ticks.ToString();
                }
                return conversionName;
            }
            set
            {
                conversionName = value;
            }
        }

        private List<Terrain> terrains;
        private List<Terrain> Terrains
        {
            get
            {
                if (terrains == null)
                {
                    terrains = new List<Terrain>();
                }
                return terrains;
            }
            set
            {
                terrains = value;
            }
        }

        private List<GStylizedTerrain> convertedTerrains;
        private List<GStylizedTerrain> ConvertedTerrains
        {
            get
            {
                if (convertedTerrains == null)
                {
                    convertedTerrains = new List<GStylizedTerrain>();
                }
                return convertedTerrains;
            }
            set
            {
                convertedTerrains = value;
            }
        }

        private List<GSplatPrototypeGroup> splatGroups;
        private List<GSplatPrototypeGroup> SplatGroups
        {
            get
            {
                if (splatGroups == null)
                {
                    splatGroups = new List<GSplatPrototypeGroup>();
                }
                return splatGroups;
            }
            set
            {
                splatGroups = value;
            }
        }

        private List<int> splatGroupIndices;
        private List<int> SplatGroupIndices
        {
            get
            {
                if (splatGroupIndices == null)
                {
                    splatGroupIndices = new List<int>();
                }
                return splatGroupIndices;
            }
            set
            {
                splatGroupIndices = value;
            }
        }

        private List<GTreePrototypeGroup> treeGroups;
        private List<GTreePrototypeGroup> TreeGroups
        {
            get
            {
                if (treeGroups == null)
                {
                    treeGroups = new List<GTreePrototypeGroup>();
                }
                return treeGroups;
            }
            set
            {
                treeGroups = value;
            }
        }

        private List<int> treeGroupIndices;
        private List<int> TreeGroupIndices
        {
            get
            {
                if (treeGroupIndices == null)
                {
                    treeGroupIndices = new List<int>();
                }
                return treeGroupIndices;
            }
            set
            {
                treeGroupIndices = value;
            }
        }

        private List<GGrassPrototypeGroup> grassGroups;
        private List<GGrassPrototypeGroup> GrassGroups
        {
            get
            {
                if (grassGroups == null)
                {
                    grassGroups = new List<GGrassPrototypeGroup>();
                }
                return grassGroups;
            }
            set
            {
                grassGroups = value;
            }
        }

        private List<int> grassGroupIndices;
        private List<int> GrassGroupIndices
        {
            get
            {
                if (grassGroupIndices == null)
                {
                    grassGroupIndices = new List<int>();
                }
                return grassGroupIndices;
            }
            set
            {
                grassGroupIndices = value;
            }
        }

        public List<GStylizedTerrain> Convert()
        {
            try
            {
                Validate();
                Initialize();
                if (DataTemplate == null)
                {
                    CreateSharedData();
                }
                ImportDataAndCreateTerrain();
                FinishingUp();
            }
            catch (GProgressCancelledException)
            {
                Debug.Log("Converting process canceled!");
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

            return ConvertedTerrains;
        }

        private void Validate()
        {
            if (Root == null)
                throw new System.ArgumentNullException("Root");

            Terrain[] terrains = Root.GetComponentsInChildren<Terrain>();
            if (terrains.Length == 0)
            {
                throw new System.Exception("No Terrain found under Root");
            }

            bool hasData = false;
            for (int i = 0; i < terrains.Length; ++i)
            {
                if (terrains[i].terrainData != null)
                {
                    hasData = true;
                }
            }

            if (!hasData)
            {
                throw new System.Exception("No Terrain with Terrain Data found under Root");
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (string.IsNullOrEmpty(DataDirectory) || !DataDirectory.StartsWith("Assets/"))
                {
                    throw new System.ArgumentException("Data Directory must be related to Assets folder");
                }
            }
#endif
        }

        private void SaveAssets()
        {
#if UNITY_EDITOR
            GCommonGUI.ProgressBar("Saving", "Saving unsaved assets...", 1f);
            AssetDatabase.SaveAssets();
            GCommonGUI.ClearProgressBar();
#endif
        }

        private void Initialize()
        {
            Terrains = new List<Terrain>();
            Terrain[] t = Root.GetComponentsInChildren<Terrain>();
            for (int i = 0; i < t.Length; ++i)
            {
                if (t[i].terrainData != null)
                    Terrains.Add(t[i]);
            }

            if (!Application.isPlaying)
            {
                GUtilities.EnsureDirectoryExists(DataDirectory);
            }
        }

        private void CreateSharedData()
        {
            if (ImportSplats)
                CreateSharedSplats();

            if (ImportTrees)
                CreateSharedTrees();

            if (ImportGrasses)
                CreateSharedGrasses();

            //SaveAssets();
        }

        private void CreateSharedSplats()
        {
            SplatGroups.Clear();
            SplatGroupIndices.Clear();

            for (int i = 0; i < Terrains.Count; ++i)
            {
                Terrain t = Terrains[i];
#if UNITY_2018_3_OR_NEWER
                TerrainLayer[] layers = t.terrainData.terrainLayers;
#else
                SplatPrototype[] layers = t.terrainData.splatPrototypes;
#endif

                int splatGroupIndex = -1;
                for (int j = 0; j < SplatGroups.Count; ++j)
                {
                    if (SplatGroups[j].Equals(layers))
                    {
                        splatGroupIndex = j;
                        break;
                    }
                }

                if (splatGroupIndex >= 0)
                {
                    SplatGroupIndices.Add(splatGroupIndex);
                }
                else
                {
                    GSplatPrototypeGroup group = GSplatPrototypeGroup.Create(layers);
                    SplatGroups.Add(group);
                    SplatGroupIndices.Add(SplatGroups.Count - 1);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUtilities.EnsureDirectoryExists(DataDirectory);
                for (int i = 0; i < SplatGroups.Count; ++i)
                {
                    GSplatPrototypeGroup group = SplatGroups[i];
                    group.name = string.Format("{0}{1}_{2}", "SharedSplats", i.ToString(), ConversionName);
                    string assetPath = string.Format("{0}.asset", Path.Combine(DataDirectory, group.name));
                    AssetDatabase.CreateAsset(group, assetPath);
                }
            }
#endif
        }

        private void CreateSharedTrees()
        {
            TreeGroups.Clear();
            TreeGroupIndices.Clear();

            for (int i = 0; i < Terrains.Count; ++i)
            {
                Terrain t = Terrains[i];
                TreePrototype[] treePrototypes = t.terrainData.treePrototypes;

                int treeGroupIndex = -1;
                for (int j = 0; j < TreeGroups.Count; ++j)
                {
                    if (TreeGroups[j].Equals(treePrototypes))
                    {
                        treeGroupIndex = j;
                        break;
                    }
                }

                if (treeGroupIndex >= 0)
                {
                    TreeGroupIndices.Add(treeGroupIndex);
                }
                else
                {
                    GTreePrototypeGroup group = GTreePrototypeGroup.Create(treePrototypes);
                    TreeGroups.Add(group);
                    TreeGroupIndices.Add(TreeGroups.Count - 1);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUtilities.EnsureDirectoryExists(DataDirectory);
                for (int i = 0; i < TreeGroups.Count; ++i)
                {
                    GTreePrototypeGroup group = TreeGroups[i];
                    group.name = string.Format("{0}{1}_{2}", "SharedTrees", i.ToString(), ConversionName);
                    string assetPath = string.Format("{0}.asset", Path.Combine(DataDirectory, group.name));
                    AssetDatabase.CreateAsset(group, assetPath);
                }
            }
#endif
        }

        private void CreateSharedGrasses()
        {
            GrassGroups.Clear();
            GrassGroupIndices.Clear();

            for (int i = 0; i < Terrains.Count; ++i)
            {
                Terrain t = Terrains[i];
                DetailPrototype[] detailPrototypes = t.terrainData.detailPrototypes;

                int grassGroupIndex = -1;
                for (int j = 0; j < GrassGroups.Count; ++j)
                {
                    if (GrassGroups[j].Equals(detailPrototypes))
                    {
                        grassGroupIndex = j;
                        break;
                    }
                }

                if (grassGroupIndex >= 0)
                {
                    GrassGroupIndices.Add(grassGroupIndex);
                }
                else
                {
                    GGrassPrototypeGroup group = GGrassPrototypeGroup.Create(detailPrototypes);
                    GrassGroups.Add(group);
                    GrassGroupIndices.Add(GrassGroups.Count - 1);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUtilities.EnsureDirectoryExists(DataDirectory);
                for (int i = 0; i < GrassGroups.Count; ++i)
                {
                    GGrassPrototypeGroup group = GrassGroups[i];
                    group.name = string.Format("{0}{1}_{2}", "SharedGrasses", i.ToString(), ConversionName);
                    string assetPath = string.Format("{0}.asset", Path.Combine(DataDirectory, group.name));
                    AssetDatabase.CreateAsset(group, assetPath);
                }
            }
#endif
        }

        private void ImportDataAndCreateTerrain()
        {
            GameObject terrainRoot = new GameObject(string.Format("{0}-{1}", Root.name, ConversionName));
            terrainRoot.transform.parent = Root.transform.parent;
            terrainRoot.transform.position = Root.transform.position;

            for (int i = 0; i < Terrains.Count; ++i)
            {
#if UNITY_EDITOR
                GCommonGUI.CancelableProgressBar("Converting", "Converting " + Terrains[i].name, 1f);
#endif

                GTerrainData data = ScriptableObject.CreateInstance<GTerrainData>();
                if (DataTemplate != null)
                {
                    DataTemplate.Geometry.CopyTo(data.Geometry);
                    DataTemplate.Shading.CopyTo(data.Shading);
                    DataTemplate.Rendering.CopyTo(data.Rendering);
                    DataTemplate.Foliage.CopyTo(data.Foliage);
                    DataTemplate.Mask.CopyTo(data.Mask);
                }
                else
                {
                    if (Application.isPlaying) //Reset() only called in edit mode
                    {
                        data.Reset();
                        data.Geometry.Reset();
                        data.Shading.Reset();
                        data.Rendering.Reset();
                        data.Foliage.Reset();
                        data.Mask.Reset();
                    }
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    string assetName = "TerrainData_" + data.Id;
                    AssetDatabase.CreateAsset(data, string.Format("{0}.asset", Path.Combine(DataDirectory, assetName)));
                }
#endif

                Material templateMaterial = null;
                if (DataTemplate != null)
                {
                    templateMaterial = DataTemplate.Shading.CustomMaterial;
                }

                Material material = null;
                if (templateMaterial != null)
                {
                    material = new Material(templateMaterial.shader);
                }
                else
                {
                    if (ImportSplats)
                    {
                        if (ImportSplatsAsAlbedo && AlbedoUsage == GAlbedoUsage.ColorMap)
                        {
                            material = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(GCommon.CurrentRenderPipeline, GLightingModel.PBR, GTexturingModel.ColorMap);
                        }
                        else if (ImportSplatsAsAlbedo && AlbedoUsage == GAlbedoUsage.VertexColor)
                        {
                            material = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(GCommon.CurrentRenderPipeline, GLightingModel.PBR, GTexturingModel.VertexColor);
                            data.Geometry.AlbedoToVertexColorMode = GAlbedoToVertexColorMode.Sharp;
                        }
                        else
                        {
                            GSplatPrototypeGroup splats = DataTemplate ? DataTemplate.Shading.Splats : SplatGroups[SplatGroupIndices[i]];
                            GSplatsModel splatModel = (splats == null || splats.Prototypes.Count <= 4) ?
                            GSplatsModel.Splats4Normals4 :
                            GSplatsModel.Splats8;
                            material = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(GCommon.CurrentRenderPipeline, GLightingModel.PBR, GTexturingModel.Splat, splatModel);
                        }
                    }
                    else
                    {
                        material = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(GCommon.CurrentRenderPipeline, GLightingModel.PBR, GTexturingModel.Splat, GSplatsModel.Splats4);
                    }
                }
                data.Shading.CustomMaterial = material;

#if UNITY_EDITOR
                if (!Application.isPlaying && material != null)
                {
                    string matName = "TerrainMaterial_" + data.Id;
                    material.name = matName;
                    AssetDatabase.CreateAsset(material, string.Format("{0}.mat", Path.Combine(DataDirectory, matName)));
                }
#endif

                if (ImportSplats)
                {
                    data.Shading.Splats = DataTemplate ? DataTemplate.Shading.Splats : SplatGroups[SplatGroupIndices[i]];
                    data.Shading.UpdateMaterials();
                }
                if (ImportTrees)
                {
                    data.Foliage.Trees = DataTemplate ? DataTemplate.Foliage.Trees : TreeGroups[TreeGroupIndices[i]];
                }
                if (ImportGrasses)
                {
                    data.Foliage.Grasses = DataTemplate ? DataTemplate.Foliage.Grasses : GrassGroups[GrassGroupIndices[i]];
                }

                GUnityTerrainDataImporter importer = new GUnityTerrainDataImporter();
                importer.SrcData = Terrains[i].terrainData;
                importer.SrcTerrain = Terrains[i];
                importer.DesData = data;
                importer.DesTerrain = null;
                importer.ImportGeometry = ImportGeometry;
                importer.UseUnityTerrainSize = true;
                importer.ImportSplats = ImportSplats;
                importer.ImportSplatsAsAlbedo = ImportSplatsAsAlbedo;
                importer.ImportSplatControlMapsOnly = DataTemplate != null && DataTemplate.Shading.Splats != null;
                importer.ImportSplatControlMapResolution = DataTemplate == null;
                importer.CreateNewSplatPrototypesGroup = false;
                importer.ImportTrees = ImportTrees;
                importer.ImportTreeInstancesOnly = DataTemplate != null && DataTemplate.Foliage.Trees != null;
                importer.CreateNewTreePrototypesGroup = false;
                importer.ImportGrasses = ImportGrasses;
                importer.ImportGrassInstancesOnly = DataTemplate != null && DataTemplate.Foliage.Grasses != null;
                importer.CreateNewGrassPrototypesGroup = false;
                importer.GrassDensity = 1;
                importer.SkipFoliageSnap = SkipFoliageSnap;
                importer.Import();

                GStylizedTerrain t = CreateTerrain();
                t.transform.parent = terrainRoot.transform;
                t.transform.position = Terrains[i].transform.position;
                t.name = Terrains[i].name;

#if UNITY_2018_3_OR_NEWER
                t.GroupId = Terrains[i].groupingID;
#endif

                t.TerrainData = data;

                if (ImportTrees || ImportGrasses)
                {
                    data.Foliage.SetTreeRegionDirty(GCommon.UnitRect);
                    data.Foliage.SetGrassRegionDirty(GCommon.UnitRect);
                    t.UpdateTreesPosition();
                    t.UpdateGrassPatches();
                    data.Foliage.ClearTreeDirtyRegions();
                    data.Foliage.ClearGrassDirtyRegions();
                }

                ConvertedTerrains.Add(t);
#if UNITY_EDITOR
                //SaveAssets();
                GCommonGUI.ClearProgressBar();
#endif
            }
        }

        private GStylizedTerrain CreateTerrain()
        {
            GameObject g = new GameObject("Stylized Terrain");
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            GStylizedTerrain terrain = g.AddComponent<GStylizedTerrain>();
            terrain.GroupId = 0;

            GameObject colliderGO = new GameObject("Tree Collider");
            colliderGO.transform.parent = terrain.transform;
            colliderGO.transform.localPosition = Vector3.zero;
            colliderGO.transform.localRotation = Quaternion.identity;
            colliderGO.transform.localScale = Vector3.one;

            GTreeCollider collider = colliderGO.AddComponent<GTreeCollider>();
            collider.Terrain = terrain;

            return terrain;
        }

        private void FinishingUp()
        {
#if UNITY_EDITOR
            GCommonGUI.ProgressBar("Finishing Up", "Matching geometry...", 1f);
#endif

            for (int i = 0; i < ConvertedTerrains.Count; ++i)
            {
                ConvertedTerrains[i].ConnectNeighbor();
                ConvertedTerrains[i].MatchEdges();
            }

            Root.gameObject.SetActive(false);
#if UNITY_EDITOR
            GCommonGUI.ClearProgressBar();
#endif
        }
    }
}
#endif
