//////////////////////////////////////////////////////
// MK Toon Examples Animate Material Int	     	//
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
    public class AnimateMaterialInt : AnimateMaterialProperty<int>
    {
        public float scale = 1;
        public float offset = 0;

        public enum Property
        {
            LightBands
        };
        public Property property;

        public override void SetValue(Material material, int value)
        {
            switch(property)
            {
                case Property.LightBands:
                    MK.Toon.Properties.lightBands.SetValue(material, value);
                break;
            }
        }

        public override int GenerateValue(Material material)
        {
            return Mathf.FloorToInt(scale * InterpValue() + offset);
        }
    }
}
