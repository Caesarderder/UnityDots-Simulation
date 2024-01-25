//////////////////////////////////////////////////////
// MK Toon Property									//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

using UnityEngine;

namespace MK.Toon
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Property Base                                                                           //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public abstract class Property<T>
    {
        protected string[] _keywords;

        protected Uniform _uniform;
        public Uniform uniform
        {
            get{ return _uniform; }
        }

        public Property(Uniform uniform, params string[] keywords)
        {
            _keywords = keywords;
            _uniform = uniform;
        }

        public abstract T GetValue(UnityEngine.Material material);
        public abstract void SetValue(UnityEngine.Material material, T value);

        protected void SetKeyword(UnityEngine.Material material, bool b, int keywordIndex)
        {
            if(b && _keywords != null && _keywords.Length > keywordIndex && _keywords.Length > 0)
            {
                CleanKeywords(material);
                material.EnableKeyword(_keywords[keywordIndex]);
            }
            else
            {
                CleanKeywords(material);
            }
        }

        private void CleanKeywords(Material material)
        {
            for(int i = 0; i < _keywords.Length; i++)
                material.DisableKeyword(_keywords[i]);
        }
    }

    public abstract class Property<T, U> : Property<T>
    {
        public Property(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}
        public abstract void SetValue(UnityEngine.Material material, T valueM, U valueS);
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    // Custom Properties                                                                       //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public class BoolProperty : Property<bool>
    {
        public BoolProperty(Uniform uniform, string keyword) : base(uniform, keyword) {}
        public BoolProperty(Uniform uniform) : base(uniform) {}

        public override bool GetValue(Material material)
        {
            return material.GetInt(_uniform.id) > 0 ? true : false;
        }
        public override void SetValue(Material material, bool value)
        {
            material.SetInt(_uniform.id, value ? 1 : 0);
            SetKeyword(material, value, 0);
        }
    }
    public class IntProperty : Property<int>
    {
        private int _keywordDisabled = 0;
        public IntProperty(Uniform uniform, string keyword, int keywordDisabled = 0) : base(uniform, keyword) {}
        public IntProperty(Uniform uniform) : base(uniform) {}

        public override int GetValue(Material material)
        {
            return material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, int value)
        {
            material.SetInt(_uniform.id, value);
            SetKeyword(material, value != _keywordDisabled, value);
        }
    }
    public class StepProperty : Property<int>
    {
        private int _keywordDisabled = 0;
        private int _minValue, _maxValue;

        public StepProperty(Uniform uniform, int minValue, int maxValue, string keyword, int keywordDisabled = 0) : base(uniform, keyword)
        {
            _keywordDisabled = keywordDisabled; 
            _minValue = minValue;
            _maxValue = maxValue;
        }
        public StepProperty(Uniform uniform, int minValue, int maxValue) : base(uniform)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public override int GetValue(Material material)
        {
            return material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, int value)
        {
            value = Mathf.Clamp(value, _minValue, _maxValue);
            material.SetInt(_uniform.id, value);
            SetKeyword(material, value != _keywordDisabled, value);
        }
    }
    public class FloatProperty : Property<float>
    {
        private float _keywordDisabled = 0;
        public FloatProperty(Uniform uniform, string keyword, float keywordDisabled = 0) : base(uniform, keyword) { _keywordDisabled = keywordDisabled; }
        public FloatProperty(Uniform uniform) : base(uniform) {}

        public override float GetValue(Material material)
        {
            return material.GetFloat(_uniform.id);
        }
        public override void SetValue(Material material, float value)
        {
            material.SetFloat(_uniform.id, value);
            SetKeyword(material, value != _keywordDisabled, (int) value);
        }
    }
    public class RangeProperty : Property<float>
    {
        private float _keywordDisabled = 0;
        private float _minValue, _maxValue;

        public RangeProperty(Uniform uniform, string keyword, float minValue, float maxValue, float keywordDisabled = 0) : base(uniform, keyword)
        {
            _keywordDisabled = keywordDisabled; 
            _minValue = minValue;
            _maxValue = maxValue;
        }
        public RangeProperty(Uniform uniform, string keyword, float minValue, float keywordDisabled = 0) : base(uniform, keyword)
        {
            _keywordDisabled = keywordDisabled; 
            _minValue = minValue;
            _maxValue = Mathf.Infinity;
        }
        public RangeProperty(Uniform uniform, float minValue, float maxValue) : base(uniform)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }
        public RangeProperty(Uniform uniform, float minValue) : base(uniform)
        {
            _minValue = minValue;
            _maxValue = Mathf.Infinity;
        }

        public override float GetValue(Material material)
        {
            return material.GetFloat(_uniform.id);
        }
        public override void SetValue(Material material, float value)
        {
            value = Mathf.Clamp(value, _minValue, _maxValue);
            material.SetFloat(_uniform.id, value);
            SetKeyword(material, value != _keywordDisabled, (int) value);
        }
    }
    public class Vector2Property : Property<Vector2>
    {
        public Vector2Property(Uniform uniform) : base(uniform) {}

        public override Vector2 GetValue(Material material)
        {
            return material.GetVector(_uniform.id);
        }
        public override void SetValue(Material material, Vector2 value)
        {
            material.SetVector(_uniform.id, value);
        }
    }
    public class Vector3Property : Property<Vector3>
    {
        public Vector3Property(Uniform uniform) : base(uniform) {}

        public override Vector3 GetValue(Material material)
        {
            return material.GetVector(_uniform.id);
        }
        public override void SetValue(Material material, Vector3 value)
        {
            material.SetVector(_uniform.id, value);
        }
    }
    public class Vector4Property : Property<Vector4>
    {
        public Vector4Property(Uniform uniform) : base(uniform) {}

        public override Vector4 GetValue(Material material)
        {
            return material.GetVector(_uniform.id);
        }
        public override void SetValue(Material material, Vector4 value)
        {
            material.SetVector(_uniform.id, value);
        }
    }
    public class ColorProperty : Property<UnityEngine.Color>
    {
        public ColorProperty(Uniform uniform, string keyword) : base(uniform, keyword) {}
        public ColorProperty(Uniform uniform) : base(uniform) {}

        public override UnityEngine.Color GetValue(Material material)
        {
            return material.GetColor(_uniform.id);
        }
        public override void SetValue(Material material, UnityEngine.Color color)
        {
            material.SetColor(_uniform.id, color);
            SetKeyword(material, color.maxColorComponent > 0, 0);
        }
    }
    public class TextureProperty : Property<UnityEngine.Texture>
    {
        public TextureProperty(Uniform uniform, string keyword) : base(uniform, keyword) {}
        public TextureProperty(Uniform uniform) : base(uniform) {}

        public override UnityEngine.Texture GetValue(Material material)
        {
            return material.GetTexture(_uniform.id);
        }
        public override void SetValue(Material material, UnityEngine.Texture texture)
        {
            material.SetTexture(_uniform.id, texture);
            SetKeyword(material, texture != null, 0);
        }
    }
    public class EnumProperty<T> : Property<T> where T : System.Enum
    {
        public EnumProperty(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}

        public override T GetValue(Material material)
        {
            return (T) (object) material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, T value)
        {
            material.SetInt(_uniform.id, (int) (object) value);
            SetKeyword(material,(int) (object) value != 0, (int) (object) value);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////
    // Explicit Properties                                                                     //
    /////////////////////////////////////////////////////////////////////////////////////////////
    public class AlphaClippingProperty : Property<bool>
    {
        public AlphaClippingProperty(Uniform uniform, string keyword) : base(uniform, keyword) {}

        public override bool GetValue(Material material)
        {
            return material.GetInt(_uniform.id) > 0 ? true : false;
        }
        public override void SetValue(Material material, bool value)
        {
            material.SetInt(_uniform.id, value ? 1 : 0);
            SetKeyword(material, value, 0);
            Properties.surface.SetValue(material, Properties.surface.GetValue(material), value);
            Properties.renderPriority.SetValue(material, Properties.renderPriority.GetValue(material), value);
        }
    }
    
    public class TilingProperty : Property<Vector2>
    {
        public TilingProperty(Uniform uniform) : base(uniform) {}

        public override Vector2 GetValue(Material material)
        {
            return material.GetTextureScale(uniform.id);
        }
        public override void SetValue(Material material, Vector2 value)
        {
            material.SetTextureScale(uniform.id, value);
        }
    }
    public class OffsetProperty : Property<Vector2>
    {
        public OffsetProperty(Uniform uniform) : base(uniform) {}

        public override Vector2 GetValue(Material material)
        {
            return material.GetTextureOffset(uniform.id);
        }
        public override void SetValue(Material material, Vector2 value)
        {
            material.SetTextureOffset(uniform.id, value);
        }
    }
    public class SpecularProperty : Property<Specular>
    {
        public SpecularProperty(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}

        public override Specular GetValue(Material material)
        {
            return (Specular) material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, Specular specular)
        {
            if(material.shader.name.Contains(Properties.shaderVariantSimpleName))
                specular = (int) specular >= 1 ? Specular.Isotropic : Specular.Off;
            material.SetInt(_uniform.id, (int) specular);
            SetKeyword(material,(int) specular != 0, (int) specular);
        }
    }
    public class EnvironmentReflectionProperty : Property<EnvironmentReflection>
    {
        public EnvironmentReflectionProperty(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}

        public override EnvironmentReflection GetValue(Material material)
        {
            return (EnvironmentReflection) material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, EnvironmentReflection environmentReflection)
        {
            if(material.shader.name.Contains(Properties.shaderVariantSimpleName))
                environmentReflection = (int) environmentReflection >= 1 ? EnvironmentReflection.Ambient : EnvironmentReflection.Off;
            material.SetInt(_uniform.id, (int) environmentReflection);
            SetKeyword(material,(int) environmentReflection != 0, (int) environmentReflection);
        }
    }
    public class StencilModeProperty : Property<Stencil>
    {
        public StencilModeProperty(Uniform uniform) : base(uniform) {}

        public override Stencil GetValue(Material material)
        {
            return (Stencil)material.GetInt(_uniform.id);
        }
        public override void SetValue(Material material, Stencil stencil)
        {
            if(stencil == Stencil.Builtin)
            {
                Properties.stencilRef.SetValue(material, 0);
                Properties.stencilReadMask.SetValue(material, 255);
                Properties.stencilWriteMask.SetValue(material, 255);
                Properties.stencilComp.SetValue(material, StencilComparison.Always);
                Properties.stencilPass.SetValue(material, StencilOperation.Keep);
                Properties.stencilFail.SetValue(material, StencilOperation.Keep);
                Properties.stencilZFail.SetValue(material, StencilOperation.Keep);
            }
            material.SetInt(_uniform.id, (int)stencil);
        }
    }
    public class RenderPriorityProperty : Property<int, bool>
    {
        public RenderPriorityProperty(Uniform uniform) : base(uniform) {}

        public override int GetValue(Material material)
        {
            return material.GetInt(uniform.id);
        }

        public override void SetValue(Material material, int priority) => SetValue(material, priority, false);
        public override void SetValue(Material material, int priority, bool alphaClipping)
        {
            Surface surface = Properties.surface.GetValue(material);
            switch(surface)
            {
                default:
                if(alphaClipping)
                {
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                }
                else
                {
                    material.renderQueue = (int)RenderQueue.Geometry;
                }
                break;
                case Surface.Transparent:
                material.renderQueue = (int)RenderQueue.Transparent;
                break;
            }
            material.SetInt(uniform.id, priority);
            material.renderQueue -= Properties.renderPriority.GetValue(material);
        }
    }
    public class SurfaceProperty : Property<Surface, bool>
    {
        public SurfaceProperty(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}

        public override Surface GetValue(Material material)
        {
            return (Surface) material.GetInt(uniform.id);
        }

        public override void SetValue(Material material, Surface surface) => SetValue(material, surface, false);
        public override void SetValue(Material material, Surface surface, bool alphaClipping)
        {
            if(material.shader.name.Contains(Properties.shaderComponentOutlineName))
                surface = Surface.Opaque;
            if(material.shader.name.Contains(Properties.shaderComponentRefractionName))
                surface = Surface.Transparent;

            bool customBlendingEnabled = Properties.blend.GetValue(material) == Blend.Custom ? true : false;

            switch(surface)
            {
                default:
                if(!customBlendingEnabled)
                {
                    Properties.zWrite.SetValue(material, ZWrite.On);
                    Properties.zTest.SetValue(material, ZTest.LessEqual);
                }
                if(alphaClipping)
                {
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetOverrideTag("IgnoreProjector", "true");
                }
                else
                {
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetOverrideTag("IgnoreProjector", "false");
                }
                material.SetShaderPassEnabled("ShadowCaster", true);
                break;
                case Surface.Transparent:
                if(!customBlendingEnabled)
                {
                    Properties.zWrite.SetValue(material, ZWrite.Off);
                    Properties.zTest.SetValue(material, ZTest.LessEqual);
                }
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("IgnoreProjector", "true");
                material.SetShaderPassEnabled("ShadowCaster", false);
                break;
            }
            Properties.blend.SetValue(material, Properties.blend.GetValue(material));
            Properties.renderPriority.SetValue(material, Properties.renderPriority.GetValue(material), Properties.alphaClipping.GetValue(material));
            material.SetInt(uniform.id, (int)surface);
            SetKeyword(material,(int) surface != 0, (int) surface);
            
            if(material.shader.name.Contains("URP"))
                material.SetShaderPassEnabled("DepthOnly", Properties.zWrite.GetValue(material) == ZWrite.On ? true : false);
        }
    }

    public class BlendProperty : Property<Blend>
    {
        public BlendProperty(Uniform uniform, params string[] keywords) : base(uniform, keywords) {}

        public override Blend GetValue(Material material)
        {
            return (Blend)material.GetInt(uniform.id);
        }

        /// <summary>
        /// Material will be changed to opaque
        /// </summary>
        /// <param uniform="material"></param>
        /// <param uniform="blend"></param>
        public override void SetValue(Material material, Blend blend)
        {
            if(Properties.blend.GetValue(material) != Blend.Custom)
            {
                Properties.zTest.SetValue(material, ZTest.LessEqual);
                if(Properties.surface.GetValue(material) == Surface.Opaque)
                {
                    Properties.blendSrc.SetValue(material, BlendFactor.One);
                    Properties.blendDst.SetValue(material, BlendFactor.Zero);
                    Properties.blendSrcAlpha.SetValue(material, BlendFactor.One);
                    Properties.blendDstAlpha.SetValue(material, BlendFactor.Zero);
                }
                else
                {
                    switch(blend)
                    {
                        case Blend.Alpha: //Alpha
                        Properties.blendSrc.SetValue(material, BlendFactor.SrcAlpha);
                        Properties.blendDst.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        Properties.blendSrcAlpha.SetValue(material, BlendFactor.One);
                        Properties.blendDstAlpha.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        break;
                        case Blend.Premultiply:
                        Properties.blendSrc.SetValue(material, BlendFactor.One);
                        Properties.blendDst.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        Properties.blendSrcAlpha.SetValue(material, BlendFactor.One);
                        Properties.blendDstAlpha.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        break;
                        case Blend.Additive:
                        Properties.blendSrc.SetValue(material, BlendFactor.SrcAlpha);
                        Properties.blendDst.SetValue(material, BlendFactor.One);
                        Properties.blendSrcAlpha.SetValue(material, BlendFactor.One);
                        Properties.blendDstAlpha.SetValue(material, BlendFactor.One);
                        break;
                        case Blend.Multiply:
                        Properties.blendSrc.SetValue(material, BlendFactor.DstColor);
                        Properties.blendDst.SetValue(material, BlendFactor.Zero);
                        Properties.blendSrcAlpha.SetValue(material, BlendFactor.Zero);
                        Properties.blendDstAlpha.SetValue(material, BlendFactor.One);
                        break;
                        default:
                        Properties.blendSrc.SetValue(material, BlendFactor.SrcAlpha);
                        Properties.blendDst.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        Properties.blendSrcAlpha.SetValue(material, BlendFactor.One);
                        Properties.blendDstAlpha.SetValue(material, BlendFactor.OneMinusSrcAlpha);
                        break;
                    }
                }
                material.SetInt(Properties.blend.uniform.id, (int) blend);
                SetKeyword(material,(int) blend != 0, (int) blend);
            }
            else
            {
                material.SetInt(Properties.blend.uniform.id, (int) blend);
                SetKeyword(material,(int) blend != 0, (int) blend);
            }
        }
    }
}
