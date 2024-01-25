#if GRIFFIN
#if __MICROSPLAT_POLARIS__
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using JBooth.MicroSplat;

namespace Pinwheel.Griffin.MicroSplat
{
    public static class GMicroSplatSetup
    {
        private static TextureArrayConfig currentTAConfig;

        public static void Setup(HashSet<GStylizedTerrain> terrains)
        {
            GSplatPrototypeGroup splats = null;
            IEnumerator<GStylizedTerrain> iTerrains = terrains.GetEnumerator();
            while (iTerrains.MoveNext())
            {
                GStylizedTerrain t = iTerrains.Current;
                if (t == null)
                    continue;
                Setup(t);
                if (t.TerrainData != null && t.TerrainData.Shading.Splats != null)
                {
                    if (splats == null)
                    {
                        splats = t.TerrainData.Shading.Splats;
                    }
                }
            }

            if (GMicroSplatIntegrationSettings.Instance.InitTextureEntries)
            {
                if (splats != null && currentTAConfig != null)
                {
                    currentTAConfig.sourceTextures.Clear();
                    currentTAConfig.sourceTextures2.Clear();
                    currentTAConfig.sourceTextures3.Clear();
                    foreach (GSplatPrototype p in splats.Prototypes)
                    {
                        TextureArrayConfig.TextureEntry entry = new TextureArrayConfig.TextureEntry();
                        entry.diffuse = p.Texture;
                        entry.normal = p.NormalMap;
                        currentTAConfig.sourceTextures.Add(entry);
                        currentTAConfig.sourceTextures2.Add(entry);
                        currentTAConfig.sourceTextures3.Add(entry);
                    }
                    TextureArrayConfigEditor.CompileConfig(currentTAConfig);
                }
            }

            iTerrains.Reset();
            while (iTerrains.MoveNext())
            {
                GStylizedTerrain t = iTerrains.Current;
                if (t == null)
                    continue;
                t.PushControlTexturesToMicroSplat();
            }
        }

        public static void Setup(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;

            t.TerrainData.Shading.ShadingSystem = GShadingSystem.MicroSplat;
            GMicroSplatIntegrationSettings settings = GMicroSplatIntegrationSettings.Instance;
            MicroSplatPolarisMesh pm = t.gameObject.GetComponent<MicroSplatPolarisMesh>();
            if (pm != null)
            {
                GUtilities.DestroyObject(pm);
            }

            MeshRenderer[] renderers = t.GetOrCreateChunkRoot().GetComponentsInChildren<MeshRenderer>();
            List<MeshRenderer> rendererList = new List<MeshRenderer>();
            rendererList.AddRange(renderers);

            MicroSplatPolarisMeshEditor.PolarisData data = new MicroSplatPolarisMeshEditor.PolarisData();
            data.basePath = settings.DataDirectory;
            data.name = settings.ShaderNamePrefix;
            data.additionalKeywords = new string[0];
            data.rootObject = t.gameObject;
            data.renderers = rendererList;
            MicroSplatPolarisMeshEditor.Setup(data);

            pm = t.gameObject.GetComponent<MicroSplatPolarisMesh>();
            t.TerrainData.Shading.CustomMaterial = pm.templateMaterial;

            string materialPath = AssetDatabase.GetAssetPath(pm.templateMaterial);
            string directory = Path.GetDirectoryName(materialPath);
            string configPath = string.Format("{0}/MicroSplatConfig.asset", directory);
            TextureArrayConfig config = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(configPath);
            t.TerrainData.Shading.MicroSplatTextureArrayConfig = config;

            currentTAConfig = config;
        }
    }
}
#endif
#endif
