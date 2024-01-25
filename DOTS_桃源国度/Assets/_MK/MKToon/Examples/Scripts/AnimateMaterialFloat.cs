//////////////////////////////////////////////////////
// MK Toon Examples AnimateMaterialFloat        	//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2023 All rights reserved.            //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Toon.Examples
{
    public class AnimateMaterialFloat : AnimateMaterialProperty<float>
    {
        public float scale = 1;
        public float offset = 0;

        public enum Property
        {
            Smoothness,
            Metallic,
            DissolveAmount,
            NormalMapIntensity,
            OcclusionMapIntensity,
            Parallax,
            DetailMix,
            DetailAdditive,
            DetailMultiplicative,
            DetailNormalMapIntensity,
            DiffuseSmoothness,
            SpecularSmoothness,
            RimSmoothness,
            IridescenceSmoothness,
            LightTransmissionSmoothness,
            GoochRampIntensity,
            IridescenceSize,
            ColorGradingBrightness,
            ColorGradingSaturation,
            ColorGradingContrast,
            Anisotropy
        };
        public Property property;

        public override void SetValue(Material material, float value)
        {
            switch(property)
            {
                case Property.Smoothness:
                    MK.Toon.Properties.smoothness.SetValue(material, value);
                break;
                case Property.Metallic:
                    MK.Toon.Properties.metallic.SetValue(material, value);
                break;
                case Property.DissolveAmount:
                    MK.Toon.Properties.dissolveAmount.SetValue(material, value);
                break;
                case Property.NormalMapIntensity:
                    MK.Toon.Properties.normalMapIntensity.SetValue(material, value);
                break;
                case Property.OcclusionMapIntensity:
                    MK.Toon.Properties.occlusionMapIntensity.SetValue(material, value);
                break;
                case Property.Parallax:
                    MK.Toon.Properties.parallax.SetValue(material, value);
                break;
                case Property.DetailMix:
                case Property.DetailMultiplicative:
                case Property.DetailAdditive:
                    MK.Toon.Properties.detailMix.SetValue(material, value);
                break;
                case Property.DetailNormalMapIntensity:
                    MK.Toon.Properties.detailNormalMapIntensity.SetValue(material, value);
                break;
                case Property.DiffuseSmoothness:
                    MK.Toon.Properties.diffuseSmoothness.SetValue(material, value);
                break;
                case Property.SpecularSmoothness:
                    MK.Toon.Properties.specularSmoothness.SetValue(material, value);
                break;
                case Property.RimSmoothness:
                    MK.Toon.Properties.rimSmoothness.SetValue(material, value);
                break;
                case Property.IridescenceSmoothness:
                    MK.Toon.Properties.iridescenceSmoothness.SetValue(material, value);
                break;
                case Property.LightTransmissionSmoothness:
                    MK.Toon.Properties.lightTransmissionSmoothness.SetValue(material, value);
                break;
                case Property.GoochRampIntensity:
                    MK.Toon.Properties.goochRampIntensity.SetValue(material, value);
                break;
                case Property.IridescenceSize:
                    MK.Toon.Properties.iridescenceSize.SetValue(material, value);
                break;
                case Property.ColorGradingContrast:
                    MK.Toon.Properties.contrast.SetValue(material, value);
                break;
                case Property.ColorGradingSaturation:
                    MK.Toon.Properties.saturation.SetValue(material, value);
                break;
                case Property.ColorGradingBrightness:
                    MK.Toon.Properties.brightness.SetValue(material, value);
                break;
                case Property.Anisotropy:
                    MK.Toon.Properties.anisotropy.SetValue(material, value);
                break;
            }
        }

        public override float GenerateValue(Material material)
        {
            return scale * InterpValue() + offset;
        }
    }
}
