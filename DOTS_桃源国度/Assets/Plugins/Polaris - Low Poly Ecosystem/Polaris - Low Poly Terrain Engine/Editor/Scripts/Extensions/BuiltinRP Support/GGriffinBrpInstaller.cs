#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TerrainMaterialTemplate = Pinwheel.Griffin.GRuntimeSettings.TerrainRenderingSettings.TerrainMaterialTemplate;

namespace Pinwheel.Griffin.BuiltinRP
{
    public static class GGriffinBrpInstaller
    {
        public static void Install()
        {
            GGriffinBrpResources resources = GGriffinBrpResources.Instance;
            if (resources == null)
            {
                Debug.Log("Unable to load Griffin BuiltinRP Resources.");
            }

            List<TerrainMaterialTemplate> terrainMaterialTemplates = new List<TerrainMaterialTemplate>();
            #region PBR materials
            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4,
                material = resources.TerrainPbr4SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4Normals4,
                material = resources.TerrainPbr4Splats4NormalsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats8,
                material = resources.TerrainPbr8SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.GradientLookup,
                material = resources.TerrainPbrGradientLookupMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.VertexColor,
                material = resources.TerrainPbrVertexColorMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.PBR,
                texturingModel = GTexturingModel.ColorMap,
                material = resources.TerrainPbrColorMapMaterial
            });
            #endregion
            #region Lambert materials
            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4,
                material = resources.TerrainLambert4SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4Normals4,
                material = resources.TerrainLambert4Splats4NormalsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats8,
                material = resources.TerrainLambert8SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.GradientLookup,
                material = resources.TerrainLambertGradientLookupMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.VertexColor,
                material = resources.TerrainLambertVertexColorMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.Lambert,
                texturingModel = GTexturingModel.ColorMap,
                material = resources.TerrainLambertColorMapMaterial
            });
            #endregion
            #region Blinn-Phong materials
            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4,
                material = resources.TerrainBlinnPhong4SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats4Normals4,
                material = resources.TerrainBlinnPhong4Splats4NormalsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.Splat,
                splatsModel = GSplatsModel.Splats8,
                material = resources.TerrainBlinnPhong8SplatsMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.GradientLookup,
                material = resources.TerrainBlinnPhongGradientLookupMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.VertexColor,
                material = resources.TerrainBlinnPhongVertexColorMaterial
            });

            terrainMaterialTemplates.Add(new TerrainMaterialTemplate()
            {
                lightingModel = GLightingModel.BlinnPhong,
                texturingModel = GTexturingModel.ColorMap,
                material = resources.TerrainBlinnPhongColorMapMaterial
            });
            #endregion

            GRuntimeSettings.Instance.terrainRendering.builtinRpMaterials = terrainMaterialTemplates;
            GRuntimeSettings.Instance.foliageRendering.grassMaterial = resources.GrassMaterial;
            GRuntimeSettings.Instance.foliageRendering.grassBillboardMaterial = resources.GrassBillboardMaterial;
            GRuntimeSettings.Instance.foliageRendering.grassInteractiveMaterial = resources.GrassInteractiveMaterial;
            GRuntimeSettings.Instance.foliageRendering.treeBillboardMaterial = resources.TreeBillboardMaterial;

            EditorUtility.SetDirty(GRuntimeSettings.Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Completed", "Successfully installed Polaris Built-in Render Pipeline support.", "OK");
        }
    }
}
#endif
