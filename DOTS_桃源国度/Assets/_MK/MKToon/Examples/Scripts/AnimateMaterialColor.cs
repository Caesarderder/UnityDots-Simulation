//////////////////////////////////////////////////////
// MK Toon Examples AnimateMaterialColor            //
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
    public class AnimateMaterialColor : AnimateMaterialProperty<Color>
    {
        public float intensity = 1f;
        public Color color0 = Color.white, color1 = Color.grey;
        
        public enum Property
        {
            EmissionColor,
            GoochBrightColor,
            GoochDarkColor,
            RimColor,
            RimBrightColor,
            RimDarkColor
        };
        public Property property;

        public override void SetValue(Material material, Color value)
        {
            switch(property)
            {
                case Property.EmissionColor:
                    MK.Toon.Properties.emissionColor.SetValue(material, value * intensity);
                break;
                case Property.GoochBrightColor:
                    MK.Toon.Properties.goochBrightColor.SetValue(material, value * intensity);
                break;
                case Property.GoochDarkColor:
                    MK.Toon.Properties.goochDarkColor.SetValue(material, value * intensity);
                break;
                case Property.RimColor:
                    MK.Toon.Properties.rimColor.SetValue(material, value * intensity);
                break;
                case Property.RimBrightColor:
                    MK.Toon.Properties.rimBrightColor.SetValue(material, value * intensity);
                break;
                case Property.RimDarkColor:
                    MK.Toon.Properties.rimDarkColor.SetValue(material, value * intensity);
                break;
            }
        }

        public override Color GenerateValue(Material material)
        {
            return Color.Lerp(color0, color1, InterpValue());
        }
    }
}
