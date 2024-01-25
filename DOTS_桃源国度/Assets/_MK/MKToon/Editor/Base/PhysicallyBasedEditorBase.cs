//////////////////////////////////////////////////////
// MK Toon PBS Editor Base						    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Utils;
using UnityEditorInternal;
using EditorHelper = MK.Toon.Editor.EditorHelper;
using MK.Toon;

namespace MK.Toon.Editor
{
    /// <summary>
    /// Base class for pbs editors
    /// </summary>
    internal abstract class PhysicallyBasedEditorBase : SimpleEditorBase
    {
        public PhysicallyBasedEditorBase(RenderPipeline renderPipeline) : base(renderPipeline)
        {
            _shaderTemplate = ShaderTemplate.PhysicallyBased;
            _renderPipeline = renderPipeline;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Properties                                                                              //
		/////////////////////////////////////////////////////////////////////////////////////////////     

        /////////////////
        // Options     //
        /////////////////
        private MaterialProperty _workflow;

        /////////////////
        // Input       //
        /////////////////
        private MaterialProperty _metallic;
        private MaterialProperty _roughness;
        private MaterialProperty _roughnessMap;
        private MaterialProperty _metallicMap;
        private MaterialProperty _parallax;
        private MaterialProperty _heightMap;
        private MaterialProperty _lightTransmission;
        private MaterialProperty _lightTransmissionDistortion;
        private MaterialProperty _lightTransmissionColor;
        private MaterialProperty _thicknessMap;
        private MaterialProperty _occlusionMapIntensity;
        private MaterialProperty _occlusionMap;

        /////////////////
        // Detail      //
        /////////////////
        private MaterialProperty _detailBlend;
        private MaterialProperty _detailColor;
        private MaterialProperty _detailMix;
        private MaterialProperty _detailMap;
        private MaterialProperty _detailNormalMapIntensity;
        private MaterialProperty _detailNormalMap;

        /////////////////
        // Stylize     //
        /////////////////
        private MaterialProperty _lightTransmissionRamp;
        private MaterialProperty _lightTransmissionSmoothness;
        private MaterialProperty _lightTransmissionThresholdOffset;
        private MaterialProperty _goochBrightMap;
        private MaterialProperty _goochDarkMap;
        
        /////////////////
        // Advanced    //
        /////////////////
        private MaterialProperty _diffuse;
        private MaterialProperty _specularAnisotrophy;
        private MaterialProperty _lightTransmissionIntensity;
        private MaterialProperty _fresnelHighlights;

        protected override void FindProperties(MaterialProperty[] props)
        {
            base.FindProperties(props);

            _workflow = FindProperty(Properties.workflow.uniform.name, props);
            _metallic = FindProperty(Properties.metallic.uniform.name, props);
            _roughness = FindProperty(Properties.roughness.uniform.name, props);
            _metallicMap = FindProperty(Properties.metallicMap.uniform.name, props);
            _roughnessMap = FindProperty(Properties.roughnessMap.uniform.name, props);
            _parallax = FindProperty(Properties.parallax.uniform.name, props);
            _heightMap = FindProperty(Properties.heightMap.uniform.name, props);
            _lightTransmission = FindProperty(Properties.lightTransmission.uniform.name, props);
            _lightTransmissionDistortion = FindProperty(Properties.lightTransmissionDistortion.uniform.name, props);
            _lightTransmissionColor = FindProperty(Properties.lightTransmissionColor.uniform.name, props);
            _thicknessMap = FindProperty(Properties.thicknessMap.uniform.name, props);
            _occlusionMapIntensity = FindProperty(Properties.occlusionMapIntensity.uniform.name, props);
            _occlusionMap = FindProperty(Properties.occlusionMap.uniform.name, props);

            _detailBlend = FindProperty(Properties.detailBlend.uniform.name, props);
            _detailColor = FindProperty(Properties.detailColor.uniform.name, props);
            _detailMix = FindProperty(Properties.detailMix.uniform.name, props);
            _detailMap = FindProperty(Properties.detailMap.uniform.name, props);
            _detailNormalMapIntensity = FindProperty(Properties.detailNormalMapIntensity.uniform.name, props);
            _detailNormalMap = FindProperty(Properties.detailNormalMap.uniform.name, props);
            
            _lightTransmissionRamp = FindProperty(Properties.lightTransmissionRamp.uniform.name, props);
            _lightTransmissionSmoothness = FindProperty(Properties.lightTransmissionSmoothness.uniform.name, props);
            _lightTransmissionThresholdOffset = FindProperty(Properties.lightTransmissionThresholdOffset.uniform.name, props);
            _goochBrightMap = FindProperty(Properties.goochBrightMap.uniform.name, props);
            _goochDarkMap = FindProperty(Properties.goochDarkMap.uniform.name, props);

            _diffuse = FindProperty(Properties.diffuse.uniform.name, props);
            _specularAnisotrophy = FindProperty(Properties.anisotropy.uniform.name, props);
            _lightTransmissionIntensity = FindProperty(Properties.lightTransmissionIntensity.uniform.name, props);
            _fresnelHighlights = FindProperty(Properties.fresnelHighlights.uniform.name, props);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Draw                                                                                    //
		/////////////////////////////////////////////////////////////////////////////////////////////

        protected override void ConvertSimilarValues(MaterialProperty[] propertiesSrc, Material materialSrc, Material materialDst)
        {
            base.ConvertSimilarValues(propertiesSrc, materialSrc, materialDst);

            MaterialProperty workflowMode = FindProperty("_WorkflowMode", propertiesSrc, false);
            MaterialProperty specColor = FindProperty("_SpecColor", propertiesSrc, false);
            MaterialProperty metallic = FindProperty("_Metallic", propertiesSrc, false);
            MaterialProperty smoothness = FindProperty("_Smoothness", propertiesSrc, false);
            MaterialProperty glossiness = FindProperty("_Glossiness", propertiesSrc, false);
            MaterialProperty metallicGlossMap = FindProperty("_MetallicGlossMap", propertiesSrc, false);
            MaterialProperty glossMapScale = FindProperty("_GlossMapScale", propertiesSrc, false);
            MaterialProperty specGlossMap = FindProperty("_SpecGlossMap", propertiesSrc, false);
            MaterialProperty parallax = FindProperty("_Parallax", propertiesSrc, false);
            MaterialProperty parallaxMap = FindProperty("_ParallaxMap", propertiesSrc, false);
            MaterialProperty occlusionStrength = FindProperty("_OcclusionStrength", propertiesSrc, false);
            MaterialProperty occlusionMap = FindProperty("_OcclusionMap", propertiesSrc, false);
            MaterialProperty glossyReflections = FindProperty("_GlossyReflections", propertiesSrc, false);
            MaterialProperty glossinessSource = FindProperty("_GlossinessSource", propertiesSrc, false);
            MaterialProperty environmentReflections = FindProperty("_EnvironmentReflections", propertiesSrc, false);
            MaterialProperty specularHighlights = FindProperty("_SpecularHighlights", propertiesSrc, false);
            MaterialProperty detailAlbedoMap = FindProperty("_DetailAlbedoMap", propertiesSrc, false);
            MaterialProperty detailNormalMap = FindProperty("_DetailNormalMap", propertiesSrc, false);
            MaterialProperty detailNormalMapScale = FindProperty("_DetailNormalMapScale", propertiesSrc, false);
            MaterialProperty specular = FindProperty("_Specular", propertiesSrc, false);

            bool srcIsMKLit = materialSrc.shader.name.Contains("MK/Toon/") && !materialSrc.shader.name.Contains("Unlit");

            if(!materialSrc.shader.name.Contains("MK/Toon/"))
            {
                switch(materialSrc.shader.name)
                {
                    //Legacy
                    case "Standard":
                        Properties.workflow.SetValue(materialDst, Workflow.Metallic);
                        if(metallicGlossMap != null)
                            Properties.metallicMap.SetValue(materialDst, metallicGlossMap.textureValue);
                        if(metallic != null)
                            Properties.metallic.SetValue(materialDst, metallic.floatValue);
                        if(glossMapScale != null && glossiness != null)
                            Properties.smoothness.SetValue(materialDst, Properties.metallicMap.GetValue(materialDst) ? glossMapScale.floatValue : glossiness.floatValue);
                        Properties.fresnelHighlights.SetValue(materialDst, true);
                    break;
                    case "Standard (Specular setup)":
                        Properties.workflow.SetValue(materialDst, Workflow.Specular);
                        if(specGlossMap != null)
                            Properties.specularMap.SetValue(materialDst, specGlossMap.textureValue);
                        if(specColor != null)
                            Properties.specularColor.SetValue(materialDst, specColor.colorValue);
                        if(glossMapScale != null && glossiness != null)
                            Properties.smoothness.SetValue(materialDst, Properties.specularMap.GetValue(materialDst) ? glossMapScale.floatValue : glossiness.floatValue);
                        Properties.fresnelHighlights.SetValue(materialDst, true);
                    break;
                    case "Autodesk Interactive":
                        Properties.workflow.SetValue(materialDst, Workflow.Roughness);
                        if(metallicGlossMap != null)
                            Properties.metallicMap.SetValue(materialDst, metallicGlossMap.textureValue);
                        if(specGlossMap != null)
                            Properties.roughnessMap.SetValue(materialDst, specGlossMap.textureValue);
                        if(metallic != null)
                            Properties.metallic.SetValue(materialDst, metallic.floatValue);
                        if(glossiness != null)
                            Properties.roughness.SetValue(materialDst, glossiness.floatValue);
                        Properties.fresnelHighlights.SetValue(materialDst, true);
                    break;

                    //URP
                    case "Universal Render Pipeline/Baked Lit":
                    case "Universal Render Pipeline/Simple Lit":
                    case "Universal Render Pipeline/Lit":
                        if(workflowMode != null)
                            Properties.workflow.SetValue(materialDst, workflowMode.floatValue > 0 ? Workflow.Metallic : Workflow.Specular);
                        else
                            Properties.workflow.SetValue(materialDst, Workflow.Metallic);

                        switch(Properties.workflow.GetValue(materialDst))
                        {
                            case Workflow.Specular:
                                if(specGlossMap != null)
                                    Properties.specularMap.SetValue(materialDst, specGlossMap.textureValue);
                                if(specColor != null)
                                    Properties.specularColor.SetValue(materialDst, specColor.colorValue);
                                if(glossMapScale != null)
                                    Properties.smoothness.SetValue(materialDst, Properties.metallicMap.GetValue(materialDst) ? glossMapScale.floatValue : smoothness.floatValue);
                            break;
                            //Metallic
                            default:
                                if(metallicGlossMap != null)
                                    Properties.metallicMap.SetValue(materialDst, metallicGlossMap.textureValue);
                                if(metallic != null)
                                    Properties.metallic.SetValue(materialDst, metallic.floatValue);
                                if(glossMapScale != null)
                                    Properties.smoothness.SetValue(materialDst, Properties.metallicMap.GetValue(materialDst) ? glossMapScale.floatValue : smoothness.floatValue);
                            break;
                        }
                        Properties.fresnelHighlights.SetValue(materialDst, true);
                    break;
                    case "Universal Render Pipeline/Autodesk Interactive":
                    case "Universal Render Pipeline/Autodesk Interactive Masked":
                    case "Universal Render Pipeline/Autodesk Interactive Transparent":
                        Properties.workflow.SetValue(materialDst, Workflow.Roughness);
                        if(metallicGlossMap != null)
                            Properties.metallicMap.SetValue(materialDst, metallicGlossMap.textureValue);
                        if(specGlossMap != null)
                            Properties.roughnessMap.SetValue(materialDst, specGlossMap.textureValue);
                        if(metallic != null)
                            Properties.metallic.SetValue(materialDst, metallic.floatValue);
                        if(glossiness != null)
                            Properties.roughness.SetValue(materialDst, glossiness.floatValue);
                        Properties.fresnelHighlights.SetValue(materialDst, true);
                    break;

                    //Default Fallback Setup
                    default:
                        Properties.workflow.SetValue(materialDst, Workflow.Metallic);
                    break;
                }

                //Inverted Toggle
                if(specularHighlights != null)
                    Properties.specular.SetValue(materialDst, specularHighlights.floatValue > 0 ? Specular.Isotropic : Specular.Off);
                if(parallax != null)
                    Properties.parallax.SetValue(materialDst, parallax.floatValue);
                if(parallaxMap != null)
                    Properties.heightMap.SetValue(materialDst, parallaxMap.textureValue);
                if(occlusionStrength != null)
                    Properties.occlusionMapIntensity.SetValue(materialDst, occlusionStrength.floatValue);
                if(occlusionMap != null)
                    Properties.occlusionMap.SetValue(materialDst, occlusionMap.textureValue);
                if(detailAlbedoMap != null)
                    Properties.detailMap.SetValue(materialDst, detailAlbedoMap.textureValue);
                if(detailNormalMap != null)
                    Properties.detailNormalMap.SetValue(materialDst, detailNormalMap.textureValue);
                if(detailAlbedoMap != null || detailNormalMap != null)
                {
                    Properties.detailTiling.SetValue(materialDst, materialSrc.GetTextureScale(detailAlbedoMap.name));
                    Properties.detailOffset.SetValue(materialDst, materialSrc.GetTextureOffset(detailAlbedoMap.name));
                }
                if(detailNormalMapScale != null)
                    Properties.detailNormalMapIntensity.SetValue(materialDst, detailNormalMapScale.floatValue);
                if(glossyReflections != null)
                    Properties.environmentReflections.SetValue(materialDst, glossyReflections.floatValue > 0 ? EnvironmentReflection.Advanced : EnvironmentReflection.Ambient);
                if(environmentReflections != null)
                    Properties.environmentReflections.SetValue(materialDst, environmentReflections.floatValue > 0 ? EnvironmentReflection.Advanced : EnvironmentReflection.Ambient);
            }
        }
        /////////////////
        // Options     //
        /////////////////
        
        protected virtual void DrawWorkflow(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_workflow, UI.workflow);
        }

        protected override void DrawOptionsContent(MaterialEditor materialEditor)
        {
            DrawWorkflow(materialEditor);
            DrawSurfaceType(materialEditor);
            DrawBlend(materialEditor);
            DrawCustomBlend(materialEditor);
            DrawRenderFace(materialEditor);
            DrawAlphaClipping(materialEditor);
        }

        /////////////////
        // Input       //
        /////////////////

        protected virtual void DrawPBSProperties(MaterialEditor materialEditor)
        {
            if(_workflow.floatValue == (int)MK.Toon.Workflow.Specular)
            {
                materialEditor.TexturePropertySingleLine(UI.specularMap, _specularMap, _specularMap.textureValue == null ? _specularColor : null);
                materialEditor.ShaderProperty(_smoothness, UI.smoothness, 2);
            }
            else if(_workflow.floatValue == (int)MK.Toon.Workflow.Roughness)
            {
                materialEditor.TexturePropertySingleLine(UI.metallicMap, _metallicMap, _metallicMap.textureValue == null ? _metallic : null);
                materialEditor.TexturePropertySingleLine(UI.roughnessMap, _roughnessMap, _roughnessMap.textureValue == null ? _roughness : null);
            }
            else //Metallic
            {
                materialEditor.TexturePropertySingleLine(UI.metallicMap, _metallicMap, _metallicMap.textureValue == null ? _metallic : null);
                materialEditor.ShaderProperty(_smoothness, UI.smoothness, 2);
            }
        }

        protected virtual void DrawHeightMap(MaterialEditor materialEditor)
        {
            if (_heightMap.textureValue == null)
                materialEditor.TexturePropertySingleLine(UI.heightMap, _heightMap);
            else
                materialEditor.TexturePropertySingleLine(UI.heightMap, _heightMap, _parallax);
        }

        protected virtual void DrawLightTransmission(MaterialEditor materialEditor)
        {
            if (_lightTransmission.floatValue != (int) LightTransmission.Off)
                materialEditor.TexturePropertySingleLine(UI.thicknessMap, _thicknessMap, _lightTransmissionColor, _lightTransmissionDistortion);
        }

        protected virtual void DrawOcclusionMap(MaterialEditor materialEditor)
        {
            if(_occlusionMap.textureValue == null)
                materialEditor.TexturePropertySingleLine(UI.occlusionMap, _occlusionMap);
            else
            {
                materialEditor.TexturePropertySingleLine(UI.occlusionMap, _occlusionMap, _occlusionMapIntensity);
            }
        }

        protected void DrawDetailHeader()
        {
            EditorGUILayout.LabelField("Detail: ", UnityEditor.EditorStyles.boldLabel);
        }

        protected void DrawDetailScaleTransform(MaterialEditor materialEditor)
        {
            materialEditor.TextureScaleOffsetProperty(_detailMap);
        }

        protected virtual void DrawDetailMap(MaterialEditor materialEditor)
        {
            if(_detailMap.textureValue != null)
                materialEditor.TexturePropertySingleLine(UI.detailMap, _detailMap, _detailColor, _detailBlend);
            else
                materialEditor.TexturePropertySingleLine(UI.detailMap, _detailMap);
        }

        protected virtual void DrawDetailBlend(MaterialEditor materialEditor)
        {
            if(_detailMap.textureValue != null)
                materialEditor.ShaderProperty(_detailMix, UI.detailMix, 2);
        }

        protected virtual void DrawDetailNormalMap(MaterialEditor materialEditor)
        {
            if(_detailNormalMap.textureValue == null)
            {
                materialEditor.TexturePropertySingleLine(UI.detailNormalMap, _detailNormalMap);
            }
            else
            {
                materialEditor.TexturePropertySingleLine(UI.detailNormalMap, _detailNormalMap, _detailNormalMapIntensity);
            }
        }

        protected override void DrawInputContent(MaterialEditor materialEditor)
        {
            DrawMainHeader();
            DrawAlbedoMap(materialEditor);
            DrawPBSProperties(materialEditor);
            DrawNormalMap(materialEditor);
            DrawHeightMap(materialEditor);
            DrawLightTransmission(materialEditor);
            DrawOcclusionMap(materialEditor);
            DrawEmissionMap(materialEditor);
            DrawAlbedoScaleTransform(materialEditor);

            EditorHelper.Divider();
            DrawDetailHeader();
            DrawDetailMap(materialEditor);
            DrawDetailBlend(materialEditor);
            DrawDetailNormalMap(materialEditor);
            DrawDetailScaleTransform(materialEditor);
        }

        /////////////////
        // Stylize     //
        /////////////////
        protected override void DrawLightingThreshold(MaterialEditor materialEditor)
        {
            base.DrawLightingThreshold(materialEditor);
            if(_thresholdMap.textureValue != null && _lightTransmission.floatValue != (int) LightTransmission.Off)
                materialEditor.ShaderProperty(_lightTransmissionThresholdOffset, UI.lightTransmissionThresholdOffset);
        }

        protected override void DrawLightingSmoothness(MaterialEditor materialEditor)
        {
            base.DrawLightingSmoothness(materialEditor);
            if(_lightTransmission.floatValue != (int) LightTransmission.Off)
                materialEditor.ShaderProperty(_lightTransmissionSmoothness, UI.lightTransmissionSmoothness);
        }

        protected override void DrawLightingRampWarning()
        {
            //This override could be optimized some day, see base implementation
            bool displayWarning = false;
            bool d = _diffuseRamp.textureValue == null;
            bool s = (Specular) _specular.floatValue != Specular.Off && _specularRamp.textureValue == null;
            bool r = (Rim) _rim.floatValue != Rim.Off && _rimRamp.textureValue == null;
            bool lt = (LightTransmission) _lightTransmission.floatValue != LightTransmission.Off && _lightTransmissionRamp.textureValue == null;

            if((Light) _light.floatValue == Light.Ramp)
            {
                if(d || s || r || lt)
                    displayWarning = true;
            }

            if(displayWarning)
                DrawLightingRampWarningHeader();
        }

        protected override void DrawLightingRamp(MaterialEditor materialEditor)
        {
            base.DrawLightingRamp(materialEditor);
            if((LightTransmission) _lightTransmission.floatValue != LightTransmission.Off)
                materialEditor.TexturePropertySingleLine(UI.lightTransmissionRamp, _lightTransmissionRamp);
        }

        protected override void DrawGooch(MaterialEditor materialEditor)
        {
            DrawGoochHeader();
            if(_goochRamp.textureValue != null)
                materialEditor.TexturePropertySingleLine(UI.goochRamp, _goochRamp, _goochRampIntensity);
            else
                materialEditor.TexturePropertySingleLine(UI.goochRamp, _goochRamp);
            materialEditor.TexturePropertySingleLine(UI.goochBrightMap, _goochBrightMap, _goochBrightColor);
            materialEditor.TexturePropertySingleLine(UI.goochDarkMap, _goochDarkMap, _goochDarkColor);
        }

        /////////////////
        // Advanced    //
        /////////////////
        protected virtual void DrawDiffuseMode(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_diffuse, UI.diffuse);
        }

        protected virtual void DrawLightTransmissionMode(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_lightTransmission, UI.lightTransmission);
        }

        protected virtual void DrawAnisotrophy(MaterialEditor materialEditor)
        {
            if(_specular.floatValue == (int)Specular.Anisotropic)
            {
                materialEditor.ShaderProperty(_specularAnisotrophy, UI.anisotropy, 1);
            }
        }

        protected virtual void DrawLightTransmissionIntensity(MaterialEditor materialEditor)
        {
            if(_lightTransmission.floatValue != (int) LightTransmission.Off)
            {
                materialEditor.ShaderProperty(_lightTransmissionIntensity, UI.lightTransmissionIntensity, 1);
            }
        }

        protected virtual void DrawFresnelHighlights(MaterialEditor materialEditor)
        {
            if(_environmentReflections.floatValue != (int)EnvironmentReflection.Off)
            {
                materialEditor.ShaderProperty(_fresnelHighlights, UI.fresnelHighlights);
            }
        }

        protected override void DrawAdvancedLighting(MaterialEditor materialEditor)
        {
            DrawLightingHeader();
            DrawReceiveShadows(materialEditor);
            DrawWrappedLighting(materialEditor);
            DrawDiffuseMode(materialEditor);
            DrawSpecularMode(materialEditor);
            DrawAnisotrophy(materialEditor);
            DrawLightTransmissionMode(materialEditor);
            DrawLightTransmissionIntensity(materialEditor);
            DrawEnvironmentReflections(materialEditor);
            DrawFresnelHighlights(materialEditor);
        }

        protected override void DrawAdvancedContent(MaterialEditor materialEditor, Material material)
        {
            DrawAdvancedLighting(materialEditor);

            EditorHelper.Divider();
            DrawPipeline(materialEditor);

            EditorHelper.Divider();
            DrawStencil(materialEditor, material);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Variants Setup                                                                          //
		/////////////////////////////////////////////////////////////////////////////////////////////

        private void ManageKeywordsWorkflow(Material material)
        {
            //Workflow
            EditorHelper.SetKeyword(Properties.workflow.GetValue(material) == MK.Toon.Workflow.Specular, Keywords.workflow[0], material);
            EditorHelper.SetKeyword(Properties.workflow.GetValue(material) == MK.Toon.Workflow.Roughness, Keywords.workflow[1], material);
            //No Keyword set == Unlit
        }

        private void ManageKeywordsPBSMaps(Material material)
        {
            //Workflow
            EditorHelper.SetKeyword(Properties.metallicMap.GetValue(material) && (Properties.workflow.GetValue(material) == Workflow.Metallic || Properties.workflow.GetValue(material) == Workflow.Roughness) || Properties.specularMap.GetValue(material) && Properties.workflow.GetValue(material) == Workflow.Specular, Keywords.pbsMap0, material);
            EditorHelper.SetKeyword(Properties.roughnessMap.GetValue(material), Keywords.pbsMap1, material);
        }

        private void ManageKeywordsHeightMap(Material material)
        {
            //Height map
            EditorHelper.SetKeyword(Properties.heightMap.GetValue(material), Keywords.heightMap, material);
        }

        private void ManageKeywordsParallax(Material material)
        {
            //Parallax
            EditorHelper.SetKeyword(Properties.heightMap.GetValue(material) && Properties.parallax.GetValue(material) != 0.0f, Keywords.parallax, material);
        }

        private void ManageKeywordsLightTransmission(Material material)
        {
            //LightTransmission
            EditorHelper.SetKeyword(Properties.lightTransmission.GetValue(material) == LightTransmission.Translucent, Keywords.lightTransmission[1], material);
            EditorHelper.SetKeyword(Properties.lightTransmission.GetValue(material) == LightTransmission.SubSurfaceScattering, Keywords.lightTransmission[2], material);
        }

        private void ManageKeywordsOcclusionMap(Material material)
        {
            //Occlusion
            EditorHelper.SetKeyword(Properties.occlusionMap.GetValue(material), Keywords.occlusionMap, material);
        }

        private void ManageKeywordsDetailMap(Material material)
        {
            //detail albedo
            EditorHelper.SetKeyword(Properties.detailMap.GetValue(material), Keywords.detailMap, material);
        }
        private void ManageKeywordsDetailBlend(Material material)
        {
            //detail albedo blend
            EditorHelper.SetKeyword(Properties.detailBlend.GetValue(material) == MK.Toon.DetailBlend.Mix, Keywords.detailBlend[1], material);
            EditorHelper.SetKeyword(Properties.detailBlend.GetValue(material) == MK.Toon.DetailBlend.Add, Keywords.detailBlend[2], material);
            //no detail map set = No blending / No Detail required
        }

        private void ManageKeywordsDetailNormalMap(Material material)
        {
            //detail bump
            EditorHelper.SetKeyword(Properties.detailNormalMap.GetValue(material), Keywords.detailNormalMap, material);
        }

        private void ManageKeywordsDiffuse(Material material)
        {
            //diffuse light
            EditorHelper.SetKeyword(Properties.diffuse.GetValue(material) == Diffuse.OrenNayar, Keywords.diffuse[1], material);
            EditorHelper.SetKeyword(Properties.diffuse.GetValue(material) == Diffuse.Minnaert, Keywords.diffuse[2], material);
            //No Keyword = Lambert
        }

        private void ManageKeywordsFresnelHighlights(Material material)
        {
            //env reflections
            EditorHelper.SetKeyword(Properties.fresnelHighlights.GetValue(material), Keywords.fresnelHighlights, material);
        }

        private void ManageKeywordsGoochRamp(Material material)
        {
            //Gooch Ramp
            EditorHelper.SetKeyword(Properties.goochRamp.GetValue(material), Keywords.goochRamp, material);
        }

        private void ManageKeywordsGoochBrightMap(Material material)
        {
            //Gooch Bright Map
            EditorHelper.SetKeyword(Properties.goochBrightMap.GetValue(material), Keywords.goochBrightMap, material);
        }

        private void ManageKeywordsGoochDarkMap(Material material)
        {
            //Gooch Dark Map
            EditorHelper.SetKeyword(Properties.goochDarkMap.GetValue(material), Keywords.goochDarkMap, material);
        }

        private void ManageKeywordsThicknessMap(Material material)
        {
            //Thickness
            EditorHelper.SetKeyword(Properties.thicknessMap.GetValue(material), Keywords.thicknessMap, material);
        }

        protected override void UpdateKeywords(Material material)
        {
            base.UpdateKeywords(material);
            ManageKeywordsWorkflow(material);
            ManageKeywordsHeightMap(material);
            ManageKeywordsParallax(material);
            ManageKeywordsLightTransmission(material);
            ManageKeywordsOcclusionMap(material);
            ManageKeywordsDetailMap(material);
            ManageKeywordsDetailBlend(material);
            ManageKeywordsDetailNormalMap(material);
            ManageKeywordsDiffuse(material);
            ManageKeywordsFresnelHighlights(material);
            ManageKeywordsGoochBrightMap(material);
            ManageKeywordsGoochDarkMap(material);
            ManageKeywordsThicknessMap(material);
            ManageKeywordsPBSMaps(material);
        }
    }
}
#endif