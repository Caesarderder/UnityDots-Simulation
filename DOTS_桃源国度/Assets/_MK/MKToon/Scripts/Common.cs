//////////////////////////////////////////////////////
// MK Toon Common									//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

namespace MK.Toon
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Property Enums                                                                          //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public enum Workflow
    {
        Metallic = 0,
        Specular = 1,
        Roughness = 2
    };
    public enum Surface
    {
        Opaque = 0,
        Transparent = 1
    };
    //Refraction should be forced into a Transparent queue
    internal enum SurfaceRefraction
    {
        Transparent = 1
    };
    //Outline needs to be in opaque queue to avoid z issues on SRP
    internal enum SurfaceOutline
    {
        Opaque = 0
    };
    public enum RenderFace
    {
        DoubleSided = 0,
        Back = 1,
        Front = 2
    };
    public enum DetailBlend
    {
        Mix = 0,
        Add = 1,
        Multiply = 2
    };
    public enum Light
    {
        Builtin = 0,
        Cel = 1,
        Banded = 2,
        Ramp = 3
    };
    public enum ZWrite
    {
        Off = 0,
        On = 1
    };
    public enum Iridescence
    {
        Off = 0,
        On = 1
    };
    public enum Rim
    {
        Off = 0,
        Default = 1,
        Split = 2
    };
    public enum ColorGrading
    {
        Off = 0,
        Albedo = 1,
        FinalOutput = 2
    };
    public enum VertexAnimation
    {
        Off = 0,
        Sine = 1,
        Pulse = 2,
        Noise = 3
    }
    public enum Dissolve
    {
        Off = 0,
        Default = 1,
        BorderColor = 2,
        BorderRamp = 3
    };
    public enum LightTransmission
    {
        Off = 0,
        Translucent = 1,
        SubSurfaceScattering = 2
    };
    public enum Diffuse
    {
        Lambert = 0,
        OrenNayar = 1,
        Minnaert = 2,
    };
    public enum Specular
    {
        Off = 0,
        Isotropic = 1,
        Anisotropic = 2
    };
    internal enum SpecularSimple
    {
        Off = 0,
        Isotropic = 1
    };
    public enum Artistic
    {
        Off = 0,
        Drawn = 1,
        Hatching = 2,
        Sketch = 3
    };
    public enum ArtisticProjection
    {
        TangentSpace = 0,
        ScreenSpace = 1
    };
    public enum EnvironmentReflection
    {
        Off = 0,
        Ambient = 1,
        Advanced = 2
    };
    internal enum EnvironmentReflectionSimple
    {
        Off = 0,
        Ambient = 1
    };
    public enum Outline
    {
        //Off = 0, //Saved for later usage
        HullObject = 1,
        HullOrigin = 2,
        HullClip = 3
    };
    public enum OutlineData
    {
        Normal = 0,
        Baked = 1
    };
    public enum Stencil
    {
        //Off = 0, //Saved for later usage
        Builtin = 1,
        Custom = 2
    }

    //BlendFactor
    public enum BlendFactor
    {
        Zero = 0,
        One = 1,
        DstColor = 2,
        SrcColor = 3,
        OneMinusDstColor = 4,
        SrcAlpha = 5,
        OneMinusSrcColor = 6,
        DstAlpha = 7,
        OneMinusDstAlpha = 8,
        SrcAlphaSaturate = 9,
        OneMinusSrcAlpha = 10
    };
    
    public enum Blend
    {
        Alpha = 0,
        Premultiply = 1,
        Additive = 2,
        Multiply = 3,
        Custom = 4
    };
    internal enum BlendOpaque
    {
        Default = 0,
        Custom = 4
    };

    //BlendOperation
    public enum BlendOperation
    {
        Add = 0,
        Subtract = 1,
        ReverseSubtract = 2,
        Min = 3,
        Max = 4,
        LogicalClear = 5,
        LogicalSet = 6,
        LogicalCopy = 7,
        LogicalCopyInverted = 8,
        LogicalNoop = 9,
        LogicalInvert = 10,
        LogicalAnd = 11,
        LogicalNand = 12,
        LogicalOr = 13,
        LogicalNor = 14,
        LogicalXor = 15,
        LogicalEquivalence = 16,
        LogicalAndReverse = 17,
        LogicalAndInverted = 18,
        LogicalOrReverse = 19,
        LogicalOrInverted = 20,
        Multiply = 21,
        Screen = 22,
        Overlay = 23,
        Darken = 24,
        Lighten = 25,
        ColorDodge = 26,
        ColorBurn = 27,
        HardLight = 28,
        SoftLight = 29,
        Difference = 30,
        Exclusion = 31,
        HSLHue = 32,
        HSLSaturation = 33,
        HSLColor = 34,
        HSLLuminosity = 35
    };

    public enum ColorBlend
    {
        Multiply = 0,
        Additive = 1,
        Subtractive = 2,
        Overlay = 3,
        Color = 4,
        Difference = 5
    };

    //RenderQueue
    public enum RenderQueue
    {
        Background = 1000,
        Geometry = 2000,
        AlphaTest = 2450,
        GeometryLast = 2500,
        Transparent = 3000,
        Overlay = 4000
    };

    public enum ZTest
    {
        Always = 8,
        Equal = 3,
        GreaterEqual = 7,
        Greater = 5,
        LessEqual = 4,
        Less = 2,
        NotEqual = 6
    }

    //Stencil
    public enum StencilOperation
    {
        Keep = 0,
        Zero = 1,
        Replace = 2,
        IncrementSaturate = 3,
        DecrementSaturate = 4,
        Invert = 5,
        IncrementWrap = 6,
        DecrementWrap = 7,
        Always = 8
    };
    public enum StencilComparison
    {
        Disabled = 0,
        Never = 1,
        Less = 2,
        Equal = 3,
        LessEqual = 4,
        Greater = 5,
        NotEqual = 6,
        GreaterEqual = 7,
        Always = 8
    };
}
