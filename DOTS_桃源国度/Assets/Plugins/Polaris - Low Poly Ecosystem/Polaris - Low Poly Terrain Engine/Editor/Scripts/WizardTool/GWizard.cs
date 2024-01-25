#if GRIFFIN
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Pinwheel.Griffin;
using Pinwheel.Griffin.Physic;
using Pinwheel.Griffin.GroupTool;
using Pinwheel.Griffin.HelpTool;
using Pinwheel.Griffin.PaintTool;
using Pinwheel.Griffin.StampTool;
using Pinwheel.Griffin.SplineTool;
using Pinwheel.Griffin.ErosionTool;
using System;
using Object = UnityEngine.Object;

namespace Pinwheel.Griffin.Wizard
{
    public static class GWizard
    {
        public static List<GStylizedTerrain> CreateTerrains(GameObject root)
        {
            GEditorSettings.WizardToolsSettings settings = GEditorSettings.Instance.wizardTools;
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUtilities.EnsureDirectoryExists(settings.dataDirectory);
            }
#endif
            try
            {
                float totalTile = settings.tileCountX * settings.tileCountZ;
                float tileCount = 0;
                for (int z = 0; z < settings.tileCountZ; ++z)
                {
                    for (int x = 0; x < settings.tileCountX; ++x)
                    {
                        tileCount += 1;
#if UNITY_EDITOR
                        GCommonGUI.CancelableProgressBar(
                            "Creating Terrains",
                            string.Format("Creating tile ({0},{1})", x, z),
                            tileCount / totalTile);
#endif
                        GameObject g = new GameObject();
                        g.transform.parent = root.transform;
                        g.transform.position = new Vector3(x * settings.tileSize.x, 0, z * settings.tileSize.z) + settings.origin;
                        g.transform.rotation = Quaternion.identity;
                        g.transform.localScale = Vector3.one;
                        g.name = string.Format("{0}_({1},{2})", settings.terrainNamePrefix, x, z);

                        GTerrainData data = ScriptableObject.CreateInstance<GTerrainData>();

                        if (Application.isPlaying) //Reset() only called in edit mode
                        {
                            data.Reset();
                            data.Geometry.Reset();
                            data.Shading.Reset();
                            data.Rendering.Reset();
                            data.Foliage.Reset();
                            data.Mask.Reset();
                        }
                        data.name = string.Format("TerrainData_{0}", data.Id);
                        data.Geometry.Width = settings.tileSize.x;
                        data.Geometry.Height = settings.tileSize.y;
                        data.Geometry.Length = settings.tileSize.z;
                        if (settings.texturingModel == GTexturingModel.VertexColor)
                        {
                            data.Geometry.AlbedoToVertexColorMode = GAlbedoToVertexColorMode.Sharp;
                        }

#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            string dataAssetPath = Path.Combine(settings.dataDirectory, data.name + ".asset");
                            AssetDatabase.CreateAsset(data, dataAssetPath);
                        }
#endif

                        Material material = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(
                            GCommon.CurrentRenderPipeline,
                            settings.lightingModel,
                            settings.texturingModel,
                            settings.splatsModel);
                        if (material != null)
                        {
                            material.name = string.Format("TerrainMaterial_{0}", data.Id);
#if UNITY_EDITOR
                            if (!Application.isPlaying)
                            {
                                string materialAssetPath = Path.Combine(settings.dataDirectory, material.name + ".mat");
                                AssetDatabase.CreateAsset(material, materialAssetPath);
                            }
#endif
                        }
                        data.Shading.CustomMaterial = material;

                        if (material.HasProperty(data.Shading.ColorByHeightPropertyName) ||
                            material.HasProperty(data.Shading.ColorByNormalPropertyName) ||
                            material.HasProperty(data.Shading.ColorBlendPropertyName))
                        {
                            data.Shading.UpdateLookupTextures();
                        }
                        data.Shading.UpdateMaterials();

                        GStylizedTerrain terrain = g.AddComponent<GStylizedTerrain>();
                        terrain.GroupId = settings.groupId;
                        terrain.TerrainData = data;

#if UNITY_EDITOR
                        Undo.RegisterCreatedObjectUndo(g, "Creating Low Poly Terrain");
#endif

                        GameObject colliderGO = new GameObject("Tree Collider");
                        colliderGO.transform.parent = g.transform;
                        colliderGO.transform.localPosition = Vector3.zero;
                        colliderGO.transform.localRotation = Quaternion.identity;
                        colliderGO.transform.localScale = Vector3.one;

                        GTreeCollider collider = colliderGO.AddComponent<GTreeCollider>();
                        collider.Terrain = terrain;
                    }
                }
            }
            catch (GProgressCancelledException)
            {

            }

            GStylizedTerrain.ConnectAdjacentTiles();

#if UNITY_EDITOR
            GCommonGUI.ClearProgressBar();
#endif

            return terrains;
        }

        public static GStylizedTerrain CreateTerrainFromSource(GTerrainData srcData)
        {
            GEditorSettings.WizardToolsSettings settings = GEditorSettings.Instance.wizardTools;
            GameObject g = new GameObject("Low Poly Terrain");

            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            GTerrainData terrainData = GTerrainData.CreateInstance<GTerrainData>();
            terrainData.Reset();

#if UNITY_EDITOR
            string assetName = "TerrainData_" + terrainData.Id;
            GUtilities.EnsureDirectoryExists(settings.dataDirectory);
            AssetDatabase.CreateAsset(terrainData, Path.Combine(settings.dataDirectory, assetName + ".asset"));
#endif
            srcData.CopyTo(terrainData);

            Material mat = null;
            if (srcData != null && srcData.Shading.CustomMaterial != null)
            {
                mat = UnityEngine.Object.Instantiate(srcData.Shading.CustomMaterial);
            }
            if (mat != null)
            {
                string matName = "TerrainMaterial_" + terrainData.Id;
                mat.name = matName;
#if UNITY_EDITOR
                GUtilities.EnsureDirectoryExists(settings.dataDirectory);
                AssetDatabase.CreateAsset(mat, Path.Combine(settings.dataDirectory, matName + ".mat"));
#endif
                terrainData.Shading.CustomMaterial = mat;
            }

            GStylizedTerrain terrain = g.AddComponent<GStylizedTerrain>();
            terrain.GroupId = 0;
            terrain.TerrainData = terrainData;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(g, "Creating Low Poly Terrain");
#endif

            GameObject colliderGO = new GameObject("Tree Collider");
            colliderGO.transform.parent = terrain.transform;
            colliderGO.transform.localPosition = Vector3.zero;
            colliderGO.transform.localRotation = Quaternion.identity;
            colliderGO.transform.localScale = Vector3.one;

            GTreeCollider collider = colliderGO.AddComponent<GTreeCollider>();
            collider.Terrain = terrain;

#if UNITY_EDITOR
            Selection.activeGameObject = g;
#endif
            return terrain;
        }

        public static GameObject GetTerrainToolsRoot()
        {
            GTerrainTools tools = Object.FindObjectOfType<GTerrainTools>();
            if (tools != null)
            {
                return tools.gameObject;
            }
            else
            {
                return null;
            }
        }

        public static GameObject CreateTerrainToolsRoot()
        {
            GameObject root = new GameObject("Polaris Tools");
            root.AddComponent<GTerrainTools>();
            root.hideFlags = HideFlags.HideInInspector;

            GameObject helpGO = new GameObject("Help");
            helpGO.transform.parent = root.transform;
            helpGO.transform.position = Vector3.zero;
            helpGO.transform.rotation = Quaternion.identity;
            helpGO.transform.localScale = Vector3.one;
            GHelpComponent h = helpGO.AddComponent<GHelpComponent>();

            GameObject assetExplorerGO = new GameObject("Asset Explorer");
            assetExplorerGO.transform.parent = root.transform;
            assetExplorerGO.transform.position = Vector3.zero;
            assetExplorerGO.transform.rotation = Quaternion.identity;
            assetExplorerGO.transform.localScale = Vector3.one;
            GAssetExplorer a = assetExplorerGO.AddComponent<GAssetExplorer>();

            return root;
        }

        public static GTerrainGroup CreateGroupTool()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Group Tool");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GTerrainGroup group = g.AddComponent<GTerrainGroup>();
            return group;
        }

        public static GTerrainTexturePainter CreateGeometryTexturePainter()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Geometry - Texture Painter");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GTerrainTexturePainter painter = g.AddComponent<GTerrainTexturePainter>();
            return painter;
        }

        public static GFoliagePainter CreateFoliagePainter()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Foliage Painter");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GFoliagePainter painter = g.AddComponent<GFoliagePainter>();
            g.AddComponent<GRotationRandomizeFilter>();
            g.AddComponent<GScaleRandomizeFilter>();
            return painter;
        }

        public static GObjectPainter CreateObjectPainter()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Object Painter");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GObjectPainter painter = g.AddComponent<GObjectPainter>();
            g.AddComponent<GRotationRandomizeFilter>();
            g.AddComponent<GScaleRandomizeFilter>();
            return painter;
        }

        public static GGeometryStamper CreateGeometryStamper()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Geometry Stamper");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GGeometryStamper stamper = g.AddComponent<GGeometryStamper>();
            return stamper;
        }

        public static GTextureStamper CreateTextureStamper()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Texture Stamper");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GTextureStamper stamper = g.AddComponent<GTextureStamper>();
            return stamper;
        }

        public static GErosionSimulator CreateErosionSimulator()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Erosion Simulator");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one*100;

            GErosionSimulator simulator = g.AddComponent<GErosionSimulator>();
            return simulator;
        }

        public static GFoliageStamper CreateFoliageStamper()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Foliage Stamper");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GFoliageStamper stamper = g.AddComponent<GFoliageStamper>();
            return stamper;
        }

        public static GObjectStamper CreateObjectStamper()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Object Stamper");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GObjectStamper stamper = g.AddComponent<GObjectStamper>();
            return stamper;
        }

        public static GSplineCreator CreateSplineTool()
        {
            GameObject root = GetTerrainToolsRoot();
            if (root == null)
            {
                root = CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Spline");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            g.transform.hideFlags = HideFlags.HideInInspector;

            GSplineCreator spline = g.AddComponent<GSplineCreator>();
            return spline;
        }

        public static GWindZone CreateWindZone()
        {
            GameObject g = new GameObject("Wind Zone");
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            GWindZone wind = g.AddComponent<GWindZone>();
            return wind;
        }

        public static void SetShader(GStylizedTerrain terrain)
        {
            GEditorSettings.WizardToolsSettings settings = GEditorSettings.Instance.wizardTools;
            SetShader(terrain, settings.lightingModel, settings.texturingModel, settings.splatsModel);
        }

        public static void SetShader(GStylizedTerrain terrain, GLightingModel lighting, GTexturingModel texturing, GSplatsModel splats = GSplatsModel.Splats4)
        {
            if (terrain.TerrainData == null)
            {
                throw new NullReferenceException("The selected terrain doesn't have terrain data.");
            }
            if (terrain.TerrainData.Shading.CustomMaterial == null)
            {
                throw new NullReferenceException("The selected terrain doesn't have material. Make sure you've assigned a material for it.");
            }

            Material mat = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(
                GCommon.CurrentRenderPipeline,
                lighting,
                texturing,
                splats);
            if (mat == null)
            {
                throw new Exception("Fail to get template material. Try re-install render pipeline extension package.");
            }
            terrain.TerrainData.Shading.CustomMaterial.shader = mat.shader;
            terrain.TerrainData.Shading.UpdateMaterials();
            GUtilities.DestroyObject(mat);
        }

        public static void SetShader(int groupId)
        {
            GEditorSettings.WizardToolsSettings settings = GEditorSettings.Instance.wizardTools;
            GCommon.ForEachTerrain(groupId, (t) =>
            {
                SetShader(t, settings.lightingModel, settings.texturingModel, settings.splatsModel);
            });
        }
    }
}
#endif
#endif
