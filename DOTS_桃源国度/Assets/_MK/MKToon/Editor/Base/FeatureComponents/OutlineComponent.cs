//////////////////////////////////////////////////////
// MK Toon Editor Outline Component				    //
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

namespace MK.Toon.Editor
{
    internal sealed class OutlineComponent : ShaderGUI
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
		// Properties                                                                              //
		/////////////////////////////////////////////////////////////////////////////////////////////
        private MaterialProperty _outline;
        private MaterialProperty _outlineData;
        private MaterialProperty _outlineMap;
        private MaterialProperty _outlineSize;
        private MaterialProperty _outlineColor;
        private MaterialProperty _outlineNoise;

        #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
            private MaterialProperty _outlineFadeMin;
            private MaterialProperty _outlineFadeMax;
        #endif

        private MaterialProperty _outlineBehavior;
        internal bool active { get { return _outlineBehavior != null; } }

        internal void FindProperties(MaterialProperty[] props)
        {
            _outline = FindProperty(Properties.outline.uniform.name, props, false);
            _outlineData = FindProperty(Properties.outlineData.uniform.name, props, false);
            _outlineMap = FindProperty(Properties.outlineMap.uniform.name, props, false);
            _outlineSize = FindProperty(Properties.outlineSize.uniform.name, props, false);
            _outlineColor = FindProperty(Properties.outlineColor.uniform.name, props, false);
            _outlineNoise = FindProperty(Properties.outlineNoise.uniform.name, props, false);

            #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
                _outlineFadeMin = FindProperty(Properties.outlineFadeMin.uniform.name, props, false);
                _outlineFadeMax = FindProperty(Properties.outlineFadeMax.uniform.name, props, false);
            #endif

            _outlineBehavior = FindProperty(EditorProperties.outlineTab.uniform.name, props, false);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Draw                                                                                    //
		/////////////////////////////////////////////////////////////////////////////////////////////
        internal void DrawOutline(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //All outline properties needs to be available on the material
            //the outline tab is used for check
            if(_outlineBehavior != null)
            {
                if(EditorHelper.HandleBehavior(UI.outlineTab.text, "", _outlineBehavior, null, materialEditor, false))
                {
                    #if MK_URP
                    MK.Toon.Editor.InstallWizard.Configuration.ShowURPOutlineWarning();
                    MK.Toon.Editor.EditorHelper.VerticalSpace();
                    #endif

                    FindProperties(properties);
                    materialEditor.ShaderProperty(_outline, UI.outline);
                    if((Outline) _outline.floatValue != Outline.HullOrigin)
                    {
                        materialEditor.ShaderProperty(_outlineData, UI.outlineData);
                    }

                    materialEditor.ShaderProperty(_outlineColor, UI.outlineColor);
                    materialEditor.TexturePropertySingleLine(UI.outlineMap, _outlineMap, _outlineSize);

                    #if MK_TOON_OUTLINE_FADING_LINEAR  || MK_TOON_OUTLINE_FADING_EXPONENTIAL || MK_TOON_OUTLINE_FADING_INVERSE_EXPONENTIAL
                        materialEditor.ShaderProperty(_outlineFadeMin, UI.outlineFadeMin);
                        materialEditor.ShaderProperty(_outlineFadeMax, UI.outlineFadeMax);
                    #endif
                    materialEditor.ShaderProperty(_outlineNoise, UI.outlineNoise);
                }

                EditorHelper.DrawSplitter();
            }
        }

        internal void ManageKeywordsOutline(Material material)
        {
            if(_outlineBehavior != null)
            {
                material.SetShaderPassEnabled("Always", true);
                EditorHelper.SetKeyword(Properties.outline.GetValue(material) == Outline.HullClip, Keywords.outline[2], material);
                EditorHelper.SetKeyword(Properties.outline.GetValue(material) == Outline.HullOrigin, Keywords.outline[1], material);
            }
        }
        internal void ManageKeywordsOutlineData(Material material)
        {
            if(_outlineBehavior != null)
            {
                EditorHelper.SetKeyword(Properties.outlineData.GetValue(material) == OutlineData.Baked, Keywords.outlineData, material);
            }
        }
        internal void ManageKeywordsOutlineNoise(Material material)
        {
            if(_outlineBehavior != null)
            {
                EditorHelper.SetKeyword(Properties.outlineNoise.GetValue(material) != 0, Keywords.outlineNoise, material);
            }
        }
        internal void ManageKeywordsOutlineMap(Material material)
        {
            if(_outlineBehavior != null)
            {
                EditorHelper.SetKeyword(Properties.outlineMap.GetValue(material) != null, Keywords.outlineMap, material);
            }
        }
    }
}
#endif