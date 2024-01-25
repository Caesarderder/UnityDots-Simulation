//////////////////////////////////////////////////////
// MK Toon Examples UpdateRotationSlider            //
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
    public class UpdateRotationSlider : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Slider _slider = null;
        [SerializeField]
        private SpectateCamera _spectateCamera = null;

        private void Update()
        {
            if(_spectateCamera)
                _slider.value = _spectateCamera.time;
        }
    }
}
