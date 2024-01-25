//////////////////////////////////////////////////////
// MK Toon Unlit Editor Base						//
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

// ------------------------------------------------------------------------------------------
// Note:
// All Editors are based on 3 different Lit modes and should inherit from at least one
// Outline, Refraction and Particles: Are treated as a "component"
//
// Every feature added in the future that use an additional render pass should be treated as a component
// Components should only be drawn if at least one nessecary Property (all properties still required) is found
//
// UnlitBase = ShaderGUI + Particles + Outline + Refraction + virtual base functions
//
// Shader Template:
// UnlitBase => Simple => Physically Based
//
// Enabling/Disabling shader passes would make the whole thing much easier (avoid variants for outline and refraction), however it only works for builtin lightModes, not for custom passes
// ------------------------------------------------------------------------------------------

namespace MK.Toon.Editor
{
    /// <summary>
    /// Base class for unlit editors
    /// </summary>
    internal abstract class UnlitEditorBase : ShaderGUI
    {
        public UnlitEditorBase(RenderPipeline renderPipeline)
        {
            _shaderTemplate = ShaderTemplate.Unlit;
            _renderPipeline = renderPipeline;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Properties                                                                              //
		/////////////////////////////////////////////////////////////////////////////////////////////
        protected ShaderTemplate _shaderTemplate;
        protected RenderPipeline _renderPipeline; 
   
        /////////////////
        // Options     //
        /////////////////
        protected MaterialProperty _surface;
        protected MaterialProperty _zWrite;
        protected MaterialProperty _blend;
        protected MaterialProperty _zTest;
        protected MaterialProperty _blendSrc;
        protected MaterialProperty _blendDst;
        protected MaterialProperty _blendSrcAlpha;
        protected MaterialProperty _blendDstAlpha;
        protected MaterialProperty _alphaClipping;
        protected MaterialProperty _renderFace;

        /////////////////
        // Input       //
        /////////////////
        protected MaterialProperty _albedoColor;
        protected MaterialProperty _alphaCutoff;
        protected MaterialProperty _albedoMap;

        /////////////////
        // Stylize     //
        /////////////////
        protected MaterialProperty _contrast;
        protected MaterialProperty _saturation;
        protected MaterialProperty _brightness;
        protected MaterialProperty _colorGrading;
        protected MaterialProperty _vertexAnimation;
        protected MaterialProperty _vertexAnimationStutter;
        protected MaterialProperty _vertexAnimationFrequency;
        protected MaterialProperty _vertexAnimationIntensity;
        protected MaterialProperty _vertexAnimationMap;
        protected MaterialProperty _dissolve;
        protected MaterialProperty _dissolveMap;
        protected MaterialProperty _dissolveMapScale;
        protected MaterialProperty _dissolveAmount;
        protected MaterialProperty _dissolveBorderSize;
        protected MaterialProperty _dissolveBorderRamp;
        protected MaterialProperty _dissolveBorderColor;

        /////////////////
        // Advanced    //
        /////////////////
        protected MaterialProperty _renderPriority;
        protected MaterialProperty _alembicMotionVectors;

        //Stencil
        protected MaterialProperty _stencil;
        protected MaterialProperty _stencilRef;
        protected MaterialProperty _stencilReadMask;
        protected MaterialProperty _stencilWriteMask;
        protected MaterialProperty _stencilComp;
        protected MaterialProperty _stencilPass;
        protected MaterialProperty _stencilFail;
        protected MaterialProperty _stencilZFail;

        /////////////////
        // Editor Only //
        /////////////////
        protected MaterialProperty _initialized;
        protected MaterialProperty _optionsTab;
        protected MaterialProperty _inputTab;
        protected MaterialProperty _stylizeTab;
        protected MaterialProperty _advancedTab;
        protected FontStyle _defaultFontStyle;

        /////////////////
        // Outline     //
        /////////////////
        protected MK.Toon.Editor.OutlineComponent _outline = new MK.Toon.Editor.OutlineComponent();

        /////////////////
        // Refraction  //
        /////////////////
        protected MK.Toon.Editor.RefractionComponent _refraction = new MK.Toon.Editor.RefractionComponent();

        /////////////////
        // Particles   //
        /////////////////
        protected MK.Toon.Editor.ParticlesComponent _particles = new MK.Toon.Editor.ParticlesComponent();

        /// <summary>
        /// Find Properties to draw the editor
        /// </summary>
        /// <param name="props"></param>
        protected virtual void FindProperties(MaterialProperty[] props)
        {
            _surface = FindProperty(Properties.surface.uniform.name, props);
            _zWrite = FindProperty(Properties.zWrite.uniform.name, props);
            _blend = FindProperty(Properties.blend.uniform.name, props);
            _zTest = FindProperty(Properties.zTest.uniform.name, props);
            _blendSrc = FindProperty(Properties.blendSrc.uniform.name, props);
            _blendDst = FindProperty(Properties.blendDst.uniform.name, props);
            _blendSrcAlpha = FindProperty(Properties.blendSrcAlpha.uniform.name, props);
            _blendDstAlpha = FindProperty(Properties.blendDstAlpha.uniform.name, props);
            _alphaClipping = FindProperty(Properties.alphaClipping.uniform.name, props);
            _renderFace = FindProperty(Properties.renderFace.uniform.name, props);

            _albedoColor = FindProperty(Properties.albedoColor.uniform.name, props);
            _alphaCutoff = FindProperty(Properties.alphaCutoff.uniform.name, props);
            _albedoMap = FindProperty(Properties.albedoMap.uniform.name, props);

            _colorGrading = FindProperty(Properties.colorGrading.uniform.name, props);
            _contrast = FindProperty(Properties.contrast.uniform.name, props);
            _saturation = FindProperty(Properties.saturation.uniform.name, props);
            _brightness = FindProperty(Properties.brightness.uniform.name, props);

            _vertexAnimation = FindProperty(Properties.vertexAnimation.uniform.name, props);
            _vertexAnimationStutter = FindProperty(Properties.vertexAnimationStutter.uniform.name, props);
            _vertexAnimationFrequency = FindProperty(Properties.vertexAnimationFrequency.uniform.name, props);
            _vertexAnimationIntensity = FindProperty(Properties.vertexAnimationIntensity.uniform.name, props);
            _vertexAnimationMap = FindProperty(Properties.vertexAnimationMap.uniform.name, props);

            _dissolve = FindProperty(Properties.dissolve.uniform.name, props);
            _dissolveMap = FindProperty(Properties.dissolveMap.uniform.name, props);
            _dissolveMapScale = FindProperty(Properties.dissolveMapScale.uniform.name, props);
            _dissolveAmount = FindProperty(Properties.dissolveAmount.uniform.name, props);
            _dissolveBorderRamp = FindProperty(Properties.dissolveBorderRamp.uniform.name, props);
            _dissolveBorderSize = FindProperty(Properties.dissolveBorderSize.uniform.name, props);
            _dissolveBorderColor = FindProperty(Properties.dissolveBorderColor.uniform.name, props);
            
            _renderPriority = FindProperty(Properties.renderPriority.uniform.name, props);
            _alembicMotionVectors = FindProperty(Properties.alembicMotionVectors.uniform.name, props, false);

            _stencil = FindProperty(Properties.stencil.uniform.name, props);
            _stencilRef = FindProperty(Properties.stencilRef.uniform.name, props);
            _stencilReadMask = FindProperty(Properties.stencilReadMask.uniform.name, props);
            _stencilWriteMask = FindProperty(Properties.stencilWriteMask.uniform.name, props);
            _stencilComp = FindProperty(Properties.stencilComp.uniform.name, props);
            _stencilPass = FindProperty(Properties.stencilPass.uniform.name, props);
            _stencilFail = FindProperty(Properties.stencilFail.uniform.name, props);
            _stencilZFail = FindProperty(Properties.stencilZFail.uniform.name, props);

            _initialized = FindProperty(EditorProperties.initialized.uniform.name, props);
            _optionsTab = FindProperty(EditorProperties.optionsTab.uniform.name, props);
            _inputTab = FindProperty(EditorProperties.inputTab.uniform.name, props);
            _stylizeTab = FindProperty(EditorProperties.stylizeTab.uniform.name, props);
            _advancedTab = FindProperty(EditorProperties.advancedTab.uniform.name, props);

            _outline.FindProperties(props);
            _refraction.FindProperties(props);
            _particles.FindProperties(props);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Setup                                                                                   //
		/////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Find similar values from the changed shader
        /// </summary>
        /// <param name="propertiesSrc"></param>
        /// <param name="materialDst"></param>
        /// <param name="materialSrc"></param>
        protected virtual void ConvertSimilarValues(MaterialProperty[] propertiesSrc, Material materialSrc, Material materialDst)
        {
            MaterialProperty mainTex = FindProperty("_MainTex", propertiesSrc, false);
            MaterialProperty color = FindProperty("_Color", propertiesSrc, false);
            MaterialProperty baseTex = FindProperty("_BaseTex", propertiesSrc, false);
            MaterialProperty baseMap = FindProperty("_BaseMap", propertiesSrc, false);
            MaterialProperty baseColor = FindProperty("_BaseColor", propertiesSrc, false);
            MaterialProperty cutoff = FindProperty("_Cutoff", propertiesSrc, false);
            MaterialProperty cull = FindProperty("_Cull", propertiesSrc, false);
            MaterialProperty alphaClip = FindProperty("_AlphaClip", propertiesSrc, false);
            MaterialProperty surface = FindProperty("_SurfaceType", propertiesSrc, false);
            MaterialProperty mode = FindProperty("_Mode", propertiesSrc, false);
            MaterialProperty blend = FindProperty("_BlendMode", propertiesSrc, false);
            MaterialProperty addPrecomputedVelocity = FindProperty("_AddPrecomputedVelocity", propertiesSrc, false);

            if(mode != null)
                Properties.surface.SetValue(materialDst, mode.floatValue <= 1 ? Surface.Opaque : Surface.Transparent);
            if(surface != null)
                Properties.surface.SetValue(materialDst, surface.floatValue > 0 ? Surface.Transparent : Surface.Opaque);
            if(blend != null)
                Properties.blend.SetValue(materialDst, (Blend) ((int) blend.floatValue));

            if(materialSrc.shader.name.Contains("Universal Render Pipeline") || materialSrc.shader.name.Contains("Lightweight Render Pipeline"))
            {
                if(alphaClip != null)
                    Properties.alphaClipping.SetValue(materialDst, alphaClip.floatValue > 0 ? true : false);
                if(baseTex != null)
                    Properties.albedoMap.SetValue(materialDst, baseTex.textureValue);
                if(baseMap != null)
                    Properties.albedoMap.SetValue(materialDst, baseMap.textureValue);
                if(baseColor != null)
                    Properties.albedoColor.SetValue(materialDst, baseColor.colorValue);
            }
            else
            {
                if(mode != null)
                    Properties.alphaClipping.SetValue(materialDst, mode.floatValue == 1 ? true : false);
                if(mainTex != null)
                {
                    Properties.albedoMap.SetValue(materialDst, mainTex.textureValue);
                    Properties.mainTiling.SetValue(materialDst, materialSrc.mainTextureScale);
                    Properties.mainOffset.SetValue(materialDst, materialSrc.mainTextureOffset);
                }
                if(color != null)
                    Properties.albedoColor.SetValue(materialDst, color.colorValue);
            }

            if(cutoff != null)
                Properties.alphaCutoff.SetValue(materialDst, cutoff.floatValue);
            if(cull != null)
                Properties.renderFace.SetValue(materialDst, (RenderFace) cull.floatValue);

            if(_alembicMotionVectors != null)
            {
                if(addPrecomputedVelocity != null)
                {
                    Properties.alembicMotionVectors.SetValue(materialDst, addPrecomputedVelocity.floatValue > 0 ? true : false);
                }
            }
        }

        /// <summary>
        /// Unity Message AssignNewShaderToMaterial, override MaterialSetup instead
        /// </summary>
        /// <param name="materialDst"></param>
        /// <param name="oldShader"></param>
        /// <param name="newShader"></param>
        public override void AssignNewShaderToMaterial(Material materialDst, Shader oldShader, Shader newShader)
        {
            MaterialSetup(materialDst, oldShader, newShader);
        }

        protected void SetBoldFontStyle(bool b)
        {
            EditorStyles.label.fontStyle = b ? FontStyle.Bold : _defaultFontStyle;
        }

        /// <summary>
        /// Unity Message OnGUI, override DrawInspector instead
        /// </summary>
        /// <param name="materialEditor"></param>
        /// <param name="properties"></param>
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material material = materialEditor.target as Material;
            _defaultFontStyle = EditorStyles.label.fontStyle;
            FindProperties(properties);

            #if !UNITY_2021_2_OR_NEWER
            EditorGUI.BeginChangeCheck();
            #endif
            DrawInspector(materialEditor, properties, material);

            #if !UNITY_2021_2_OR_NEWER
            if(EditorGUI.EndChangeCheck())
            {
                foreach (Material mat in _stylizeTab.targets)
                    ValidateMaterial(mat);
            }
            #endif
        }

        /// <summary>
        /// Setup the material when shader is changed
        /// </summary>
        /// <param name="materialDst"></param>
        /// <param name="oldShader"></param>
        /// <param name="newShader"></param>
        protected virtual void MaterialSetup(Material materialDst, Shader oldShader, Shader newShader)
        {            
            Material materialSrc = new Material(materialDst);
            MaterialProperty[] propertiesSrc = MaterialEditor.GetMaterialProperties(new Material[] { materialSrc });

            base.AssignNewShaderToMaterial(materialDst, oldShader, newShader);

            MaterialProperty[] propertiesDst = MaterialEditor.GetMaterialProperties(new Material[] { materialDst });
            FindProperties(propertiesDst);
            _particles.FindProperties(propertiesDst);
            _outline.FindProperties(propertiesDst);
            _refraction.FindProperties(propertiesDst);

            ConvertSimilarValues(propertiesSrc, materialSrc, materialDst);

            if(_outline.active)
            {
                Properties.surface.SetValue(materialDst, Surface.Opaque, Properties.alphaClipping.GetValue(materialDst));
            }
            if(_refraction.active)
            {
                Properties.surface.SetValue(materialDst, Surface.Transparent, Properties.alphaClipping.GetValue(materialDst));
            }

            UpdateKeywords(materialDst);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Draw                                                                                    //
		/////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Unlit warning for legacy renderpipeline
        /// </summary>
        protected void LegacyUnlitWarning()
        {
            EditorGUILayout.HelpBox("To completely unlit, please disable shadow casting & recieving on the MeshRenderer", MessageType.Info);
        }

        /////////////////
        // Options     //
        /////////////////
        protected bool OptionsBehavior(MaterialEditor materialEditor)
        {
            return EditorHelper.HandleBehavior("Options", "", _optionsTab, null, materialEditor, false);
        }
        
        protected virtual void DrawSurfaceType(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_surface, UI.surface);
        }

        /*
        protected virtual void DrawZWrite(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(_zWrite, UI.zWrite, 1);
            EditorGUI.EndChangeCheck();
        }
        */

        protected virtual void DrawCustomBlend(MaterialEditor materialEditor)
        {
            if((Blend) _blend.floatValue == Blend.Custom)
            {
                materialEditor.ShaderProperty(_zWrite, UI.zWrite, 1);
                materialEditor.ShaderProperty(_zTest, UI.zTest, 1);
                materialEditor.ShaderProperty(_blendSrc, UI.blendSrc, 1);
                materialEditor.ShaderProperty(_blendDst, UI.blendDst, 1);
                if(_renderPipeline == RenderPipeline.Universal)
                {
                    materialEditor.ShaderProperty(_blendSrcAlpha, UI.blendSrcAlpha, 1);
                    materialEditor.ShaderProperty(_blendDstAlpha, UI.blendDstAlpha, 1);
                }
            }
        }

        protected virtual void DrawBlend(MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = _blend.hasMixedValue;
            Blend blend = (Blend) _blend.floatValue;

            EditorGUI.BeginChangeCheck();
            if((Surface) _surface.floatValue == Surface.Transparent)
                blend = (Blend) EditorGUILayout.EnumPopup(UI.blend, (Blend) blend);
            else
                blend = (Blend) EditorGUILayout.EnumPopup(UI.blend, (BlendOpaque) blend);
            if(EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Blend");
                _blend.floatValue = (int) blend;
            }
            EditorGUI.showMixedValue = false;
        }

        protected virtual void DrawRenderFace(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_renderFace, UI.renderFace);
        }

        protected virtual void DrawAlphaClipping(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_alphaClipping, UI.alphaClipping);

            if(_alphaClipping.floatValue == 1)
            {
                materialEditor.ShaderProperty(_alphaCutoff, UI.alphaCutoff);
            }
        }

        /// <summary>
        /// Draw the content of the Options Behavior
        /// </summary>
        /// <param name="materialEditor"></param>
        protected virtual void DrawOptionsContent(MaterialEditor materialEditor)
        {
            DrawSurfaceType(materialEditor);
            DrawBlend(materialEditor);
            DrawCustomBlend(materialEditor);
            DrawRenderFace(materialEditor);
            DrawAlphaClipping(materialEditor);
        }

        private void DrawOptions(MaterialEditor materialEditor)
        {
            EditorHelper.DrawSplitter();
            if(OptionsBehavior(materialEditor))
            {
                DrawOptionsContent(materialEditor);
            }
        }

        /////////////////
        // Input       //
        /////////////////
        protected bool InputBehavior(MaterialEditor materialEditor)
        {
            return EditorHelper.HandleBehavior("Input", "", _inputTab, null, materialEditor, true);
        }

        protected void DrawMainHeader()
        {
            EditorGUILayout.LabelField("Main: ", UnityEditor.EditorStyles.boldLabel);
        }

        protected virtual void DrawAlbedoMap(MaterialEditor materialEditor)
        {
            materialEditor.TexturePropertySingleLine(UI.albedoMap, _albedoMap, _albedoColor);
        }

        protected void DrawAlbedoScaleTransform(MaterialEditor materialEditor)
        {
            materialEditor.TextureScaleOffsetProperty(_albedoMap);
        }
            
        /// <summary>
        /// Draw the Input Content
        /// </summary>
        /// <param name="materialEditor"></param>
        protected virtual void DrawInputContent(MaterialEditor materialEditor)
        {
            DrawMainHeader();
            DrawAlbedoMap(materialEditor);
            DrawAlbedoScaleTransform(materialEditor);
        }

        private void DrawInput(MaterialEditor materialEditor)
        {
            if(InputBehavior(materialEditor))
            {
                DrawInputContent(materialEditor);
            }
        }

        /////////////////
        // Stylize     //
        /////////////////
        protected bool StylizeBehavior(MaterialEditor materialEditor)
        {
            return EditorHelper.HandleBehavior("Stylize", "", _stylizeTab, null, materialEditor, true);
        }

        protected void DrawColorGradingHeader()
        {
            EditorGUILayout.LabelField("Color Grading:", UnityEditor.EditorStyles.boldLabel);
        }

        protected virtual void DrawColorGrading(MaterialEditor materialEditor)
        {
            //DrawColorGradingHeader();
            SetBoldFontStyle(true);
            materialEditor.ShaderProperty(_colorGrading, UI.colorGrading);
            SetBoldFontStyle(false);
            if(_colorGrading.floatValue != (int)(ColorGrading.Off))
            {
                materialEditor.ShaderProperty(_contrast, UI.contrast);
                materialEditor.ShaderProperty(_saturation, UI.saturation);
                materialEditor.ShaderProperty(_brightness, UI.brightness);
            }
        }

        protected void DrawDissolveHeader()
        {
            EditorGUILayout.LabelField("Dissolve:", UnityEditor.EditorStyles.boldLabel);
        }

        protected virtual void DrawDissolve(MaterialEditor materialEditor)
        {
            //DrawDissolveHeader();
            SetBoldFontStyle(true);
            materialEditor.ShaderProperty(_dissolve, UI.dissolve);
            SetBoldFontStyle(false);
            if(_dissolve.floatValue != (int)Dissolve.Off)
            {
                if(_dissolveMap.textureValue != null)
                    materialEditor.TexturePropertySingleLine(UI.dissolveMap, _dissolveMap, _dissolveMapScale);
                else
                    materialEditor.TexturePropertySingleLine(UI.dissolveMap, _dissolveMap);

                if(_dissolveMap.textureValue != null)
                {
                    materialEditor.ShaderProperty(_dissolveAmount, UI.dissolveAmount);
                    if(_dissolve.floatValue == (int)Dissolve.BorderRamp)
                    {
                        materialEditor.TexturePropertySingleLine(UI.dissolveBorderRamp, _dissolveBorderRamp, _dissolveBorderSize, _dissolveBorderColor);
                    }
                    else if(_dissolve.floatValue == (int)Dissolve.BorderColor)
                    {
                        materialEditor.ShaderProperty(_dissolveBorderColor, UI.dissolveBorderColor);
                        materialEditor.ShaderProperty(_dissolveBorderSize, UI.dissolveBorderSize);
                    }
                }
            }
        }

        protected void DrawVertexAnimationHeader()
        {
            EditorGUILayout.LabelField("Vertex Animation:", UnityEditor.EditorStyles.boldLabel);
        }

        protected virtual void DrawVertexAnimation(MaterialEditor materialEditor)
        {
            //DrawVertexAnimationHeader();
            SetBoldFontStyle(true);
            materialEditor.ShaderProperty(_vertexAnimation, UI.vertexAnimation);
            SetBoldFontStyle(false);

            if((VertexAnimation) _vertexAnimation.floatValue != VertexAnimation.Off)
            {
                materialEditor.TexturePropertySingleLine(UI.vertexAnimationMap, _vertexAnimationMap, _vertexAnimationIntensity);
                materialEditor.ShaderProperty(_vertexAnimationStutter, UI.vertexAnimationStutter);
                materialEditor.ShaderProperty(_vertexAnimationFrequency, UI.vertexAnimationFrequency);
            }
        }

        /// <summary>
        /// Draw Stylize Content
        /// </summary>
        /// <param name="materialEditor"></param>
        protected virtual void DrawStylizeContent(MaterialEditor materialEditor)
        {
            DrawColorGrading(materialEditor);

            EditorHelper.VerticalSpace();

            DrawDissolve(materialEditor);

            EditorHelper.VerticalSpace();
            DrawVertexAnimation(materialEditor);
        }

        private void DrawStylize(MaterialEditor materialEditor)
        {
            if(StylizeBehavior(materialEditor))
            {
                DrawStylizeContent(materialEditor);
            }
        }

        /////////////////
        // Advanced    //
        /////////////////
        protected bool AdvancedBehavior(MaterialEditor materialEditor)
        {
            return EditorHelper.HandleBehavior("Advanced", "", _advancedTab, null, materialEditor, true);
        }

        //Stencil Builtin
        private void SetBuiltinStencilSettings(MaterialEditor materialEditor, Material material)
        {
            EditorGUI.BeginChangeCheck();
            Properties.stencil.SetValue(material, Stencil.Builtin);
            if(EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Stencil Mode");
            }
        }

        protected void DrawStencilHeader()
        {
            EditorGUILayout.LabelField("Stencil:", UnityEditor.EditorStyles.boldLabel);
        }

        protected void DrawPipelineHeader()
        {
            EditorGUILayout.LabelField("Pipeline:", UnityEditor.EditorStyles.boldLabel);
        }

        protected void DrawRenderPriority(MaterialEditor materialEditor)
        {
            materialEditor.ShaderProperty(_renderPriority, UI.renderPriority);
        }

        protected void DrawAddPrecomputedVelocity(MaterialEditor materialEditor)
        {
            #if UNITY_2023_2_OR_NEWER
            if(_alembicMotionVectors != null)
                materialEditor.ShaderProperty(_alembicMotionVectors, UI.alembicMotionVectors);
            #endif
        }

        protected virtual void DrawPipeline(MaterialEditor materialEditor)
        {
            DrawPipelineHeader();

            materialEditor.EnableInstancingField();
            DrawRenderPriority(materialEditor);
            DrawAddPrecomputedVelocity(materialEditor);
        }

        protected virtual void DrawStencil(MaterialEditor materialEditor, Material material)
        {
            DrawStencilHeader();

            materialEditor.ShaderProperty(_stencil, UI.stencil);
            if(_stencil.floatValue == (int)Stencil.Custom)
            {
                materialEditor.ShaderProperty(_stencilRef, UI.stencilRef);
                materialEditor.ShaderProperty(_stencilReadMask, UI.stencilReadMask);
                materialEditor.ShaderProperty(_stencilWriteMask, UI.stencilWriteMask);
                materialEditor.ShaderProperty(_stencilComp, UI.stencilComp);
                materialEditor.ShaderProperty(_stencilPass, UI.stencilPass);
                materialEditor.ShaderProperty(_stencilFail, UI.stencilFail);
                materialEditor.ShaderProperty(_stencilZFail, UI.stencilZFail);
            }
            else// if(_stencil.floatValue == (int)Stencil.Builtin)
            {
                SetBuiltinStencilSettings(materialEditor, material);
            }
        }

        /// <summary>
        /// Draw Advanced Content
        /// </summary>
        /// <param name="materialEditor"></param>
        protected virtual void DrawAdvancedContent(MaterialEditor materialEditor, Material material)
        {            
            DrawPipeline(materialEditor);
            EditorHelper.Divider();
            DrawStencil(materialEditor, material);
        }

        private void DrawAdvanced(MaterialEditor materialEditor, Material material)
        {
            if(AdvancedBehavior(materialEditor))
            {
                DrawAdvancedContent(materialEditor, material);
            }
            EditorHelper.DrawSplitter();
        }

        /// <summary>
        /// Draw the complete editor based on the tabs
        /// </summary>
        /// <param name="materialEditor"></param>
        /// <param name="properties"></param>
        protected virtual void DrawInspector(MaterialEditor materialEditor, MaterialProperty[] properties, Material material)
        {
            //get properties
            FindProperties(properties);

            //EditorGUI.BeginChangeCheck();
            DrawOptions(materialEditor);
            DrawInput(materialEditor);
            DrawStylize(materialEditor);
            DrawAdvanced(materialEditor, material);
            _particles.DrawParticles(materialEditor, properties, _surface, _shaderTemplate);
            _refraction.DrawRefraction(materialEditor, properties);
            _outline.DrawOutline(materialEditor, properties);

            //EditorGUI.EndChangeCheck();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Variants Setup                                                                          //
		/////////////////////////////////////////////////////////////////////////////////////////////
        
        private void ManageKeywordsBlend(Material material)
        {
            //Colorsource
            Blend bm = Properties.blend.GetValue(material);
            EditorHelper.SetKeyword(Properties.blend.GetValue(material) == Blend.Premultiply, Keywords.blend[1], material);
            EditorHelper.SetKeyword(Properties.blend.GetValue(material) == Blend.Additive, Keywords.blend[2], material);
            EditorHelper.SetKeyword(Properties.blend.GetValue(material) == Blend.Multiply, Keywords.blend[3], material);
            EditorHelper.SetKeyword(Properties.blend.GetValue(material) == Blend.Custom, Keywords.blend[4], material);
            //No Keyword == Alpha

            Properties.blend.SetValue(material, bm);
        }

        private void ManageKeywordsAlbedoMap(Material material)
        {
            //Colorsource
            EditorHelper.SetKeyword(Properties.albedoMap.GetValue(material), Keywords.albedoMap, material);
            //No Keyword == Vertex Colors
        }

        private void ManageKeywordsSurface(Material material)
        {
            //Surface Type
            Properties.surface.SetValue(material, Properties.surface.GetValue(material), Properties.alphaClipping.GetValue(material));
            //No Keyword == Opaque
        }

        private void ManageKeywordsAlphaClipping(Material material)
        {
            //Alpha Clipping
            Properties.alphaClipping.SetValue(material, Properties.alphaClipping.GetValue(material));
            //No Keyword == No Alpha Clipping
        }

        private void ManageKeywordsColorGrading(Material material)
        {
            //ColorGrading
            EditorHelper.SetKeyword(Properties.colorGrading.GetValue(material) == ColorGrading.Albedo, Keywords.colorGrading[1], material);
            EditorHelper.SetKeyword(Properties.colorGrading.GetValue(material) == ColorGrading.FinalOutput, Keywords.colorGrading[2], material);
            //No keyword = No ColorGrading
        }

        private void ManageKeywordsDissolve(Material material)
        {
            //Dissolve
            EditorHelper.SetKeyword(Properties.dissolve.GetValue(material) == Dissolve.Default, Keywords.dissolve[1], material);
            EditorHelper.SetKeyword(Properties.dissolve.GetValue(material) == Dissolve.BorderColor, Keywords.dissolve[2], material);
            EditorHelper.SetKeyword(Properties.dissolve.GetValue(material) == Dissolve.BorderRamp, Keywords.dissolve[3], material);
            //No Keyword = Dissolve Off
        }

        private void ManageKeywordsVertexAnimation(Material material)
        {
            //Vertex animation
            EditorHelper.SetKeyword(Properties.vertexAnimation.GetValue(material) == VertexAnimation.Sine, Keywords.vertexAnimation[1], material);
            EditorHelper.SetKeyword(Properties.vertexAnimation.GetValue(material) == VertexAnimation.Pulse, Keywords.vertexAnimation[2], material);
            EditorHelper.SetKeyword(Properties.vertexAnimation.GetValue(material) == VertexAnimation.Noise, Keywords.vertexAnimation[3], material);
            //No Keyword = Vertex Animation Off
        }

        private void ManageKeywordsVertexAnimationStutter(Material material)
        {
            EditorHelper.SetKeyword(Properties.vertexAnimationStutter.GetValue(material), Keywords.vertexAnimationStutter, material);
            //No Keyword = Vertex Animation Stutter Off
        }

        private void ManageKeywordsVertexAnimationMap(Material material)
        {
            EditorHelper.SetKeyword(Properties.vertexAnimationMap.GetValue(material) != null, Keywords.vertexAnimationMap, material);
            //No Keyword = Vertex Animation Map Off
        }

        private void ManageKeywordsAlembicMotionVecotrs(Material material)
        {
            #if UNITY_2023_2_OR_NEWER
            if(_alembicMotionVectors != null)
            {
                EditorHelper.SetKeyword(Properties.alembicMotionVectors.GetValue(material), Keywords.alembicMotionVectors, material);
            }
            material.SetShaderPassEnabled("MotionVectors", true);
            #endif
        }

        private void UpdateRenderPriority(Material material)
        {
            Properties.renderPriority.SetValue(material, Properties.renderPriority.GetValue(material), Properties.alphaClipping.GetValue(material));
        }

        private void UpdateSystemProperties(Material material)
        {
            Properties.UpdateSystemProperties(material);
        }

        protected virtual void UpdateKeywords(Material material)
        {
            ManageKeywordsBlend(material);
            ManageKeywordsAlbedoMap(material);
            ManageKeywordsDissolve(material);
            ManageKeywordsAlphaClipping(material);
            ManageKeywordsSurface(material);
            UpdateRenderPriority(material);
            UpdateSystemProperties(material);
            ManageKeywordsColorGrading(material);
            ManageKeywordsVertexAnimation(material);
            ManageKeywordsVertexAnimationMap(material);
            ManageKeywordsVertexAnimationStutter(material);
            ManageKeywordsAlembicMotionVecotrs(material);
            _particles.UpdateKeywords(material);
            _outline.ManageKeywordsOutline(material);
            _outline.ManageKeywordsOutlineNoise(material);
            _outline.ManageKeywordsOutlineMap(material);
            _outline.ManageKeywordsOutlineData(material);
            _refraction.ManageKeywordsRefractionMap(material);
            _refraction.ManageKeywordsIndexOfRefraction(material);
        }

        #if UNITY_2021_2_OR_NEWER
        public override void ValidateMaterial(Material material)
        {
            UpdateKeywords(material);
        }
        #else
        public void ValidateMaterial(Material material)
        {
            UpdateKeywords(material);
        }
        #endif
    }
}
#endif