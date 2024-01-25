#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.Wizard
{
    public static class GWizardEditorCommon
    {
        private class GMaterialSettingsGUI
        {
            public static readonly GUIContent RP_LABEL = new GUIContent("Render Pipeline", "The render pipeline currently in used.");
            public static readonly GUIContent LIGHT_MODEL_BUILTIN = new GUIContent(
                "Lighting Model",
                "Lighting model to use.\n" +
                "- PBR: Best visual quality with metallic & smoothness setup.\n" +
                "- Lambert: Simple shading with no specularity.\n" +
                "- Blinn-Phong: Simple shading with specularity.");
            public static readonly GUIContent LIGHT_MODEL_URP = new GUIContent(
                "Lighting Model",
                "Lighting model to use.\n" +
                "Universal Render Pipeline only use PBR model which yield high visual quality yet still performant.");
            public static readonly GUIContent TEXTURING_MODEL = new GUIContent(
                "Texturing Model",
                "Terrain texturing/coloring method to use.\n" +
                "- Gradient Lookup: use Gradients and Curves to shade the vertex based on it height and normal vector.\n" +
                "- Color Map: Use a single Albedo map for the whole terrain. Fast but only suitable for small terrain.\n" +
                "- Splats: Blend between multiple textures stacked on top of each others. Similar to Unity terrain.\n" +
                "- Vertex Color: Use the color of each vertex to shade the terrain.");
            public static readonly GUIContent SPLAT_MODEL = new GUIContent(
                "Splats Model",
                "Number of texture layers and whether to use normal maps or not.");
        }

        public static void DrawMaterialSettingsGUI()
        {

            EditorGUILayout.LabelField(GMaterialSettingsGUI.RP_LABEL, new GUIContent(GCommon.CurrentRenderPipeline.ToString()));

            GUI.enabled = GCommon.CurrentRenderPipeline == GRenderPipelineType.Builtin;
            GEditorSettings.Instance.wizardTools.lightingModel = (GLightingModel)EditorGUILayout.EnumPopup(
                GCommon.CurrentRenderPipeline == GRenderPipelineType.Builtin ?
                GMaterialSettingsGUI.LIGHT_MODEL_BUILTIN :
                GMaterialSettingsGUI.LIGHT_MODEL_URP,
                GEditorSettings.Instance.wizardTools.lightingModel);
            if (GCommon.CurrentRenderPipeline == GRenderPipelineType.Universal)
            {
                GEditorSettings.Instance.wizardTools.lightingModel = GLightingModel.PBR;
            }
            GUI.enabled = true;

            GEditorSettings.Instance.wizardTools.texturingModel = (GTexturingModel)EditorGUILayout.EnumPopup(GMaterialSettingsGUI.TEXTURING_MODEL, GEditorSettings.Instance.wizardTools.texturingModel);
            if (GEditorSettings.Instance.wizardTools.texturingModel == GTexturingModel.Splat)
            {
                GEditorSettings.Instance.wizardTools.splatsModel = (GSplatsModel)EditorGUILayout.EnumPopup(GMaterialSettingsGUI.SPLAT_MODEL, GEditorSettings.Instance.wizardTools.splatsModel);
            }
        }
    }
}
#endif
