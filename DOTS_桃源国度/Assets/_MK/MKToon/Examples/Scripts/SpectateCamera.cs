//////////////////////////////////////////////////////
// MK Toon Examples Spectate Camera	    	    	//
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
    public class SpectateCamera : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Text _lockedText = null;

        public Vector3 rotationOffset = Vector3.zero;

        private bool _wasLocked = false;
        private bool _locked = false;
        public bool locked
        {
            get { return _locked; }
            set { _locked = value; _lockedText.text = _locked ? "Free" : "Lock"; }
        }
        public void ChangeLockState()
        {
            locked = !locked;
            _wasLocked = locked;
        }
        public void UpdateLockState()
        {
            if(!_wasLocked)
                locked = _wasLocked;
        }

        public Transform center;
        [SerializeField]
        private float _distance = 1.5f;
        public float distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
        public float height = 25f;
        public float speed = 10f;

        private float _time = 270;
        public float time
        {
            get { return _time; }
            set { _time = value; }
        }
    
        private void Update()
        {    
            Quaternion rotation = Quaternion.Euler(height, _time + rotationOffset.y, 0);
    
            transform.position = center.position - rotation * (Vector3.forward * distance);
            transform.rotation = rotation;

            if(!_locked)
            {
                _time += Time.smoothDeltaTime * speed;
                if(_time > 360)
                    _time = _time - 360f;
            }
        }
    }
}
