#if MK_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MK.Toon.URP
{
    public class MKToonPerObjectOutlines : ScriptableRendererFeature
    {
        private const string _componentName = "MKToonPerObjectOutlines";
        private const string _shaderPassName = "MKToonOutline";

        private UnityEngine.Experimental.Rendering.Universal.RenderObjects _renderObjectsFeature;

        public LayerMask _layerMask = -1;

        public override void Create()
        {
            _renderObjectsFeature = ScriptableObject.CreateInstance<UnityEngine.Experimental.Rendering.Universal.RenderObjects>();
            _renderObjectsFeature.settings.passTag = _componentName;
            name = _componentName;
            _renderObjectsFeature.settings.filterSettings.LayerMask = _layerMask;
            _renderObjectsFeature.settings.filterSettings.PassNames = new string[1] { _shaderPassName };

            _renderObjectsFeature.Create();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _renderObjectsFeature.settings.filterSettings.LayerMask = _layerMask;
            _renderObjectsFeature.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
#endif