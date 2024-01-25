#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Griffin;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Reflection;
using TerrainMaterialTemplate = Pinwheel.Griffin.GRuntimeSettings.TerrainRenderingSettings.TerrainMaterialTemplate;

namespace Pinwheel.Griffin.URP
{
    public static class GGriffinUrpInstaller
    {
        [DidReloadScripts]
        private static void HandleAutomaticInstallAndUpgrade()
        {
            string key = GEditorCommon.GetProjectRelatedEditorPrefsKey("polaris-urp-package-imported-0");
            bool isFirstImport = !EditorPrefs.HasKey(key);

            if (isFirstImport)
            {
                EditorApplication.update += () =>
                {
                    Install();
                    UpgradeTerrainMaterialInProject();
                    Debug.Log("POLARIS: Universal Render Pipeline shaders installed!");
                    EditorApplication.update = null;
                    EditorPrefs.SetBool(key, true);
                };
            }
        }

        public static void Install()
        {
            GGriffinUrpResources resources = GGriffinUrpResources.Instance;
            if (resources == null)
            {
                return;
            }
            List<TerrainMaterialTemplate> terrainMaterialTemplates = new List<TerrainMaterialTemplate>();
            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4,
                material = resources.Terrain4SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4Normals4,
                material = resources.Terrain4Splats4NormalsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats8,
                material = resources.Terrain8SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.GradientLookup,
                material = resources.TerrainGradientLookupMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.VertexColor,
                material = resources.TerrainVertexColorMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.ColorMap,
                material = resources.TerrainColorMapMaterial
            });

            GRuntimeSettings.Instance.terrainRendering.universalRpMaterials = terrainMaterialTemplates;
            GRuntimeSettings.Instance.foliageRendering.urpGrassMaterial = resources.GrassMaterial;
            GRuntimeSettings.Instance.foliageRendering.urpGrassBillboardMaterial = resources.GrassBillboardMaterial;
            GRuntimeSettings.Instance.foliageRendering.urpGrassInteractiveMaterial = resources.GrassInteractiveMaterial;
            GRuntimeSettings.Instance.foliageRendering.urpTreeBillboardMaterial = resources.TreeBillboardMaterial;

            EditorUtility.SetDirty(GRuntimeSettings.Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void UpgradeTerrainMaterialInProject()
        {
            if (GCommon.CurrentRenderPipeline != GRenderPipelineType.Universal)
            {
                return;
            }
            string[] guid = AssetDatabase.FindAssets("t:GTerrainData");
            for (int i = 0; i < guid.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid[i]);
                GTerrainData data = AssetDatabase.LoadAssetAtPath<GTerrainData>(path);
                Material mat = data.Shading.CustomMaterial;
                if (mat != null)
                {
                    if (UpgradeMaterial(mat))
                    {
                        data.Shading.UpdateMaterials();
                    }
                }
            }

            Material[] demoMaterials = GEditorSettings.Instance.demoAssets.demoMaterials;
            if (demoMaterials != null)
            {
                for (int i = 0; i < demoMaterials.Length; ++i)
                {
                    Material mat = demoMaterials[i];
                    if (mat != null)
                    {
                        if (mat.shader == null)
                            continue;
                        if (mat.shader.name.Equals("Standard") || mat.shader.name.Equals("Standard (Specular setup)"))
                        {
                            Debug.Log($"Upgrade {mat.name} to URP Lit");
                            mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                        }
                        else if (mat.shader.name.Equals("Polaris/BuiltinRP/Demo/WaterBasic"))
                        {
                            Debug.Log($"Upgrade {mat.name} to URP Lit");
                            mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                        }
                    }
                }
            }
        }

        public static bool UpgradeMaterial(Material mat)
        {
            if (GCommon.CurrentRenderPipeline != GRenderPipelineType.Universal)
                return false;

            Shader currentShader = mat.shader;
            TerrainMaterialTemplate template;
            bool found = GRuntimeSettings.Instance.terrainRendering.FindMaterialTemplate(
                currentShader,
                GRenderPipelineType.Builtin,
                out template);
            if (!found)
                return false;

            Material urpMat = GRuntimeSettings.Instance.terrainRendering.GetClonedMaterial(
                GRenderPipelineType.Universal,
                GLightingModel.PBR,
                template.texturingModel,
                template.splatsModel);
            if (urpMat != null)
            {
                mat.shader = urpMat.shader;
                GUtilities.DestroyObject(urpMat);
                Debug.Log(string.Format("POLARIS: Upgrade material {0} to URP succeeded.", mat.name));
                return true;
            }
            return false;
        }
    }
}
#endif
