//////////////////////////////////////////////////////
// MK Toon Particles PBS Editor        			    //
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

namespace MK.Toon.Editor.URP
{
    internal sealed class ParticlesPBSEditor : MK.Toon.Editor.PhysicallyBasedEditorBase
    {
        public ParticlesPBSEditor() : base(RenderPipeline.Universal) {}
        
        /////////////////////////////////////////////////////////////////////////////////////////////
		// Properties                                                                              //
		/////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Draw                                                                                    //
		/////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////
        // Options     //
        /////////////////
        protected override void DrawEmissionFlags(MaterialEditor materialEditor)
        {

        }
        
        protected override void EmissionRealtimeSetup(Material material)
        {
            if(Properties.emissionColor.GetValue(material).maxColorComponent <= 0)
                material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        /////////////////
        // Advanced    //
        /////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Variants Setup                                                                          //
		/////////////////////////////////////////////////////////////////////////////////////////////
    }
}
#endif