//////////////////////////////////////////////////////
// MK Toon Editor Properties           			    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using MK.Toon;

namespace MK.Toon.Editor
{
    internal static class EditorProperties
    {
        /////////////////
        // Editor Only //
        /////////////////
        internal static readonly BoolProperty initialized   = new BoolProperty(Uniforms.initialized);
        internal static readonly BoolProperty optionsTab    = new BoolProperty(Uniforms.optionsTab);
        internal static readonly BoolProperty inputTab      = new BoolProperty(Uniforms.inputTab);
        internal static readonly BoolProperty stylizeTab    = new BoolProperty(Uniforms.stylizeTab);
        internal static readonly BoolProperty advancedTab   = new BoolProperty(Uniforms.advancedTab);
        internal static readonly BoolProperty particlesTab = new BoolProperty(Uniforms.particlesTab);
        internal static readonly BoolProperty outlineTab    = new BoolProperty(Uniforms.outlineTab);
        internal static readonly BoolProperty refractionTab = new BoolProperty(Uniforms.refractionTab);
        
    }
}
#endif