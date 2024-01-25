#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pinwheel.Griffin;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.TouchReact;
using AwesomeTechnologies.ColliderSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.PrefabSpawner;
using AwesomeTechnologies.TerrainSystem;
using UnityEditor;


namespace Pinwheel.Griffin.VegetationStudioPro
{
    public static class GVSPIntegration
    {
        public const int VEGETATION_SOURCE_ID = 20;

        public static void SetupMeshTerrainsAndVSPManager()
        {
            DisablePolarisFoliageRenderer();
            SetupMeshTerrainComponents();
            VegetationStudioManager manager = SetupVegetationStudioManager();
            VegetationSystemPro vs = manager.GetComponentInChildren<VegetationSystemPro>();
            vs.AddAllMeshTerrains();

            if (GVSPSettings.Instance.QuickSetup.CreateVegetationPackage)
            {
                VegetationPackagePro package = CreateVegetationPackage();
                vs.VegetationPackageProList.Clear();
                vs.AddVegetationPackage(package);
                vs.RefreshVegetationSystem();
            }
            if (GVSPSettings.Instance.QuickSetup.CreatePersistentStorage)
            {
                PersistentVegetationStorage pvs = manager.GetComponentInChildren<PersistentVegetationStorage>();
                PersistentVegetationStoragePackage package = CreatePersistentStorage();
                pvs.PersistentVegetationStoragePackage = package;
                pvs.InitializePersistentStorage();
            }
        }

        internal static void DisablePolarisFoliageRenderer()
        {
            GCommon.ForEachTerrain(-1, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                t.TerrainData.Rendering.DrawTrees = false;
                t.TerrainData.Rendering.DrawGrasses = false;
            });
        }

        internal static void SetupMeshTerrainComponents()
        {
            GCommon.ForEachTerrain(-1, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                SetupMeshTerrainComponent(t);
            });
        }

        internal static void SetupMeshTerrainComponent(GStylizedTerrain t)
        {
            MeshTerrain oldMeshTerrainComponent = t.GetComponent<MeshTerrain>();
            if (oldMeshTerrainComponent)
            {
                GUtilities.DestroyObject(oldMeshTerrainComponent);
            }

            GVSPPolarisTerrain polarisTerrain = t.GetComponent<GVSPPolarisTerrain>();
            if (polarisTerrain == null)
            {
                polarisTerrain = t.gameObject.AddComponent<GVSPPolarisTerrain>();
            }
            polarisTerrain.Terrain = t;
            polarisTerrain.AutoAddToVegegetationSystem = true;
            polarisTerrain.Filterlods = true;
            polarisTerrain.MeshTerrainMeshSourceList.Clear();

            GTerrainChunk[] chunks = t.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                polarisTerrain.AddMeshRenderer(chunks[i].gameObject, TerrainSourceID.TerrainSourceID1);
            }
            polarisTerrain.MeshTerrainData = CreateMeshTerrainDataAsset(t);
            polarisTerrain.GenerateMeshTerrain();
            polarisTerrain.NeedGeneration = false;
            VegetationStudioManager.ClearCache();
        }

        internal static MeshTerrainData CreateMeshTerrainDataAsset(GStylizedTerrain t)
        {
            MeshTerrainData meshTerrainData = ScriptableObject.CreateInstance<MeshTerrainData>();
            meshTerrainData.name = "VSP_MeshTerrainData_" + t.TerrainData.Id;
            string fullPath = GCommon.CreateAssetAtSameDirectory(meshTerrainData, t.TerrainData);
            meshTerrainData = AssetDatabase.LoadAssetAtPath<MeshTerrainData>(fullPath);
            return meshTerrainData;
        }

        internal static VegetationStudioManager SetupVegetationStudioManager()
        {
            VegetationStudioManager vegetationStudioManager = Object.FindObjectOfType<VegetationStudioManager>();
            if (vegetationStudioManager)
            {
                return vegetationStudioManager;
            }
            else
            {
                GameObject go = new GameObject { name = "VegetationStudioPro" };
                vegetationStudioManager = go.AddComponent<VegetationStudioManager>();

                GameObject vegetationSystem = new GameObject { name = "VegetationSystemPro" };
                vegetationSystem.transform.SetParent(go.transform);
                VegetationSystemPro vegetationSystemPro = vegetationSystem.AddComponent<VegetationSystemPro>();
                vegetationSystem.AddComponent<TerrainSystemPro>();
                vegetationSystemPro.AddAllMeshTerrains();

#if TOUCH_REACT
                GameObject touchReactSystem = new GameObject { name = "TouchReactSystem" };
                touchReactSystem.transform.SetParent(go.transform);
                touchReactSystem.AddComponent<TouchReactSystem>();
#endif                
                vegetationSystem.AddComponent<ColliderSystemPro>();
                vegetationSystem.AddComponent<PersistentVegetationStorage>();
                RuntimePrefabSpawner runtimePrefabSpawner = vegetationSystem.AddComponent<RuntimePrefabSpawner>();
                runtimePrefabSpawner.enabled = false;
            }
            return vegetationStudioManager;
        }

        internal static VegetationPackagePro CreateVegetationPackage()
        {
            VegetationPackagePro v = ScriptableObject.CreateInstance<VegetationPackagePro>();
            v.PackageName = "VSP_Vegetation_" + GCommon.GetUniqueID();
            v.name = v.PackageName;
            AssetDatabase.CreateAsset(v, "Assets/" + v.name + ".asset");
            List<GTreePrototypeGroup> trees = GetActiveTreePrototypesGroups();
            for (int i = 0; i < trees.Count; ++i)
            {

                GTreePrototypeGroup t = trees[i];
                for (int j = 0; j < t.Prototypes.Count; ++j)
                {
                    GTreePrototype proto = t.Prototypes[j];
                    if (proto.Prefab == null)
                        continue;
                    v.AddVegetationItem(proto.Prefab, VegetationType.Tree);
                }
                EditorUtility.SetDirty(v);
            }

            List<GGrassPrototypeGroup> grasses = GetActiveGrassPrototypesGroups();
            for (int i = 0; i < grasses.Count; ++i)
            {

                GGrassPrototypeGroup g = grasses[i];
                for (int j = 0; j < g.Prototypes.Count; ++j)
                {
                    GGrassPrototype proto = g.Prototypes[j];
                    if (proto.Shape == GGrassShape.DetailObject)
                    {
                        if (proto.Prefab == null)
                            continue;
                        string path = AssetDatabase.GetAssetPath(proto.Prefab);
                        string id = string.IsNullOrEmpty(path) ?
                            System.Guid.NewGuid().ToString() :
                            AssetDatabase.AssetPathToGUID(path); ;
                        v.AddVegetationItem(proto.Prefab, VegetationType.Objects, true, id);
                    }
                    else
                    {
                        if (proto.Texture == null)
                            continue;
                        string path = AssetDatabase.GetAssetPath(proto.Texture);
                        string id = string.IsNullOrEmpty(path) ?
                            System.Guid.NewGuid().ToString() :
                            AssetDatabase.AssetPathToGUID(path);
                        v.AddVegetationItem(proto.Texture, VegetationType.Grass, true, id);
                        VegetationItemInfoPro item = v.GetVegetationInfo(id);

                        SerializedControllerProperty dryColorProps = item.ShaderControllerSettings.ControlerPropertyList.Find(prop => prop.PropertyName.Equals("TintColor1"));
                        if (dryColorProps != null)
                        {
                            dryColorProps.ColorValue = proto.Color;
                        }

                        SerializedControllerProperty healthyColorProps = item.ShaderControllerSettings.ControlerPropertyList.Find(prop => prop.PropertyName.Equals("TintColor2"));
                        if (healthyColorProps != null)
                        {
                            healthyColorProps.ColorValue = proto.Color;
                        }
                    }
                }
                EditorUtility.SetDirty(v);
            }

            return v;
        }

        internal static List<GTreePrototypeGroup> GetActiveTreePrototypesGroups()
        {
            List<GTreePrototypeGroup> trees = new List<GTreePrototypeGroup>();
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    continue;
                if (t.TerrainData.Foliage.Trees == null)
                    continue;
                if (t.TerrainData.Foliage.Trees.Prototypes.Count == 0)
                    continue;
                trees.AddIfNotContains(t.TerrainData.Foliage.Trees);
            }
            return trees;
        }

        internal static List<GGrassPrototypeGroup> GetActiveGrassPrototypesGroups()
        {
            List<GGrassPrototypeGroup> trees = new List<GGrassPrototypeGroup>();
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData == null)
                    continue;
                if (t.TerrainData.Foliage.Grasses == null)
                    continue;
                if (t.TerrainData.Foliage.Grasses.Prototypes.Count == 0)
                    continue;
                trees.AddIfNotContains(t.TerrainData.Foliage.Grasses);
            }
            return trees;
        }

        internal static PersistentVegetationStoragePackage CreatePersistentStorage()
        {
            PersistentVegetationStoragePackage package = ScriptableObject.CreateInstance<PersistentVegetationStoragePackage>();
            package.name = "VSP_PersistentStorage_" + GCommon.GetUniqueID();
            AssetDatabase.CreateAsset(package, "Assets/" + package.name + ".asset");
            return package;
        }

        public static void ImportFoliageAsPersistantData(GStylizedTerrain terrain, VegetationStudioManager vsm)
        {
            GTerrainData terrainData = terrain.TerrainData;
            if (terrainData == null)
            {
                Debug.Log("Nothing to import.");
                return;
            }
            if (terrainData.Foliage.TreeInstances.Count == 0 && terrainData.Foliage.GrassInstanceCount == 0)
            {
                Debug.Log("Nothing to import.");
                return;
            }

            VegetationSystemPro vs = vsm.GetComponentInChildren<VegetationSystemPro>();
            if (vs == null ||
                vs.VegetationPackageProList.Count == 0)
            {
                Debug.Log("No Vegetation System found.");
                return;
            }

            PersistentVegetationStorage pvs = vsm.GetComponentInChildren<PersistentVegetationStorage>();
            if (pvs == null ||
                pvs.PersistentVegetationStoragePackage == null)
            {
                Debug.Log("No Persistent Vegetation Storage found");
                return;
            }

            ImportTrees(terrain, vs.VegetationPackageProList, pvs.PersistentVegetationStoragePackage);
            ImportGrasses(terrain, vs.VegetationPackageProList, pvs.PersistentVegetationStoragePackage);

            if (GVSPSettings.Instance.Import.SetProceduralDensityToZero)
            {
                for (int i = 0; i < vs.VegetationPackageProList.Count; ++i)
                {
                    SetProceduralDensityToZero(vs.VegetationPackageProList[i]);
                }
            }
        }

        private static void ImportTrees(GStylizedTerrain terrain, List<VegetationPackagePro> vegetationPackages, PersistentVegetationStoragePackage persistantStorage)
        {
            EditorUtility.DisplayProgressBar(
                "Importing",
                "Importing Tree ... 0%",
                0);

            try
            {
                List<GTreePrototype> prototypes = terrain.TerrainData.Foliage.Trees.Prototypes;

                string[] ids = new string[prototypes.Count];
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    GTreePrototype proto = prototypes[i];
                    if (proto.Prefab == null)
                    {
                        ids[i] = string.Empty;
                        continue;
                    }
                    string path = AssetDatabase.GetAssetPath(proto.Prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string guid = AssetDatabase.AssetPathToGUID(path);
                        ids[i] = VegetationStudioManager.GetVegetationItemID(guid);
                    }
                }

                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = terrain.TerrainData.Geometry.Size;
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;

                float pivotOffset = 0;
                Quaternion baseRotation = Quaternion.identity;
                Vector3 baseScale = Vector3.zero;

                List<GTreeInstance> instances = terrain.TerrainData.Foliage.TreeInstances;
                for (int i = 0; i < instances.Count; ++i)
                {
                    if (i % 100 == 0)
                    {
                        int percent = (int)(i * 100f / instances.Count);
                        EditorUtility.DisplayProgressBar(
                            "Importing",
                            "Importing Tree ... " + percent.ToString() + "%",
                            i * 1.0f / instances.Count);
                    }

                    GTreeInstance tree = instances[i];
                    if (tree.PrototypeIndex < 0 || tree.PrototypeIndex >= prototypes.Count)
                        continue;

                    GTreePrototype proto = prototypes[tree.PrototypeIndex];
                    pivotOffset = proto.PivotOffset;
                    baseRotation = proto.BaseRotation;
                    baseScale = proto.BaseScale;

                    position.Set(
                        tree.Position.x * terrainSize.x + terrainPos.x,
                        tree.Position.y * terrainSize.y + terrainPos.y + pivotOffset,
                        tree.Position.z * terrainSize.z + terrainPos.z);
                    rotation = tree.Rotation * baseRotation;
                    scale.Set(
                        tree.Scale.x * baseScale.x,
                        tree.Scale.y * baseScale.y,
                        tree.Scale.z * baseScale.z);

                    VegetationStudioManager.AddVegetationItemInstance(
                        ids[tree.PrototypeIndex],
                        position,
                        scale,
                        rotation,
                        true,
                        VEGETATION_SOURCE_ID,
                        100,
                        false);
                }

                VegetationStudioManager.ClearCache();
                VegetationStudioManager.RefreshVegetationSystem();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            EditorUtility.ClearProgressBar();
        }

        private static void ImportGrasses(GStylizedTerrain terrain, List<VegetationPackagePro> vegetationPackages, PersistentVegetationStoragePackage persistantStorage)
        {
            EditorUtility.DisplayProgressBar(
                "Importing",
                "Importing Grass ... 0%",
                0);

            try
            {
                List<GGrassPrototype> prototypes = terrain.TerrainData.Foliage.Grasses.Prototypes;

                string[] ids = new string[prototypes.Count];
                for (int i = 0; i < prototypes.Count; ++i)
                {
                    GGrassPrototype proto = prototypes[i];
                    string path = null;
                    if (proto.Shape == GGrassShape.DetailObject)
                    {
                        if (proto.Prefab == null)
                        {
                            ids[i] = string.Empty;
                            continue;
                        }
                        path = AssetDatabase.GetAssetPath(proto.Prefab);
                    }
                    else
                    {
                        if (proto.Texture == null)
                        {
                            ids[i] = string.Empty;
                            continue;
                        }
                        path = AssetDatabase.GetAssetPath(proto.Texture);
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        string guid = AssetDatabase.AssetPathToGUID(path);
                        ids[i] = VegetationStudioManager.GetVegetationItemID(guid);
                    }
                }

                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = terrain.TerrainData.Geometry.Size;
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;

                float pivotOffset = 0;
                Vector3 baseScale = Vector3.zero;

                List<GGrassInstance> instances = terrain.TerrainData.Foliage.GetGrassInstances();
                for (int i = 0; i < instances.Count; ++i)
                {
                    if (i % 100 == 0)
                    {
                        int percent = (int)(i * 100f / instances.Count);
                        EditorUtility.DisplayProgressBar(
                            "Importing",
                            "Importing Grass ... " + percent.ToString() + "%",
                            i * 1.0f / instances.Count);
                    }

                    GGrassInstance grass = instances[i];
                    if (grass.PrototypeIndex < 0 || grass.PrototypeIndex >= prototypes.Count)
                        continue;

                    GGrassPrototype proto = prototypes[grass.PrototypeIndex];
                    pivotOffset = proto.PivotOffset;
                    baseScale = proto.Size;

                    position.Set(
                        grass.Position.x * terrainSize.x + terrainPos.x,
                        grass.Position.y * terrainSize.y + terrainPos.y + pivotOffset,
                        grass.Position.z * terrainSize.z + terrainPos.z);
                    rotation = grass.Rotation;
                    scale.Set(
                        grass.Scale.x * baseScale.x,
                        grass.Scale.y * baseScale.y,
                        grass.Scale.z * baseScale.z);

                    VegetationStudioManager.AddVegetationItemInstance(
                        ids[grass.PrototypeIndex],
                        position,
                        scale,
                        rotation,
                        true,
                        VEGETATION_SOURCE_ID,
                        100,
                        false);
                }

                VegetationStudioManager.ClearCache();
                VegetationStudioManager.RefreshVegetationSystem();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            EditorUtility.ClearProgressBar();
        }

        private static void SetProceduralDensityToZero(VegetationPackagePro package)
        {
            List<VegetationItemInfoPro> items = package.VegetationInfoList;
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].Density = 0;
            }
        }
    }
}
#endif
