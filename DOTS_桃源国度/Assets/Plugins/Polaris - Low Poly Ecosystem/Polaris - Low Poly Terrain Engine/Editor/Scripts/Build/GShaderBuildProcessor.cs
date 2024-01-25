#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using TerrainRenderingSettings = Pinwheel.Griffin.GRuntimeSettings.TerrainRenderingSettings;
using TerrainMaterialTemplate = Pinwheel.Griffin.GRuntimeSettings.TerrainRenderingSettings.TerrainMaterialTemplate;
using FoliageRenderingSettings = Pinwheel.Griffin.GRuntimeSettings.FoliageRenderingSettings;
using UnityEditor.Rendering;

namespace Pinwheel.Griffin.Build
{
    public class GShaderBuildProcessor : IOrderedCallback, IPreprocessShaders
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            string shaderName = shader.name;
            bool isPolarisShader = shaderName.StartsWith("Polaris");
            GRenderPipelineType shaderPipeline =
                shaderName.StartsWith("Polaris/BuiltinRP/") ? GRenderPipelineType.Builtin :
                shaderName.StartsWith("Polaris/URP/") ? GRenderPipelineType.Universal :
                GRenderPipelineType.Unsupported;
            GRenderPipelineType currentPipeline = GCommon.CurrentRenderPipeline;

            if (isPolarisShader)
            {
                if (shaderPipeline != currentPipeline)
                {
                    data.Clear();
                }
            }
        }
    }
}
#endif
