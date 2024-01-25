//////////////////////////////////////////////////////
// MK Toon Examples FlyingIslesController        	//
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
    public class FlyingIslesController : MonoBehaviour
    {
        public SpectateCamera _spectateCamera = null;
        public GameObject[] _isles = null;
        public Transform[] _centers = null;
        private int _currentID = 0;

        private void Awake()
        {
            Pick(0);
        }

        public void PickNext()
        {
            _currentID++;
            if(_currentID >= _isles.Length)
                _currentID = 0;

            Pick(_currentID);
        }

        public void PickPrevious()
        {
            _currentID--;
            if(_currentID < 0)
                _currentID = _isles.Length - 1;
            
            Pick(_currentID);
        }

        private void Pick(int id)
        {
            foreach(GameObject g in _isles)
            {
                g.SetActive(false);
            }
            _isles[_currentID].SetActive(true);
            if(_spectateCamera)
                _spectateCamera.center = _centers[_currentID];
        }
    }
}