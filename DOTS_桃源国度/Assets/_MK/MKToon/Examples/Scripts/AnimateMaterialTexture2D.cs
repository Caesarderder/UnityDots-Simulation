//////////////////////////////////////////////////////
// MK Toon Examples AnimateMaterialTexture2D        //
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
    public class AnimateMaterialTexture2D : AnimateMaterialProperty<Texture2D>
    {
        public List<Texture2D> _textures = new List<Texture2D>();

        public enum Property
        {
            DiffuseRamp,
            SpecularRamp,
            GoochBrightMap,
            GoochDarkMap
        };
        public Property property;

        public override void SetValue(Material material, Texture2D value)
        {
            switch(property)
            {
                case Property.DiffuseRamp:
                    MK.Toon.Properties.diffuseRamp.SetValue(material, value);
                break;
                case Property.SpecularRamp:
                    MK.Toon.Properties.specularRamp.SetValue(material, value);
                break;
                case Property.GoochBrightMap:
                    MK.Toon.Properties.goochBrightMap.SetValue(material, value);
                break;
                case Property.GoochDarkMap:
                    MK.Toon.Properties.goochDarkMap.SetValue(material, value);
                break;
            }
        }

        public override Texture2D GenerateValue(Material material)
        {
            return _textures[Mathf.FloorToInt((InterpValue() * (_textures.Count - 1) + 0.5f))];
        }
    }
}
