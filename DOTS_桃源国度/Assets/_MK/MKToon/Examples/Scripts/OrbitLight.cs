//////////////////////////////////////////////////////
// MK Toon Examples OrbitLight              	    //
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
    public class OrbitLight : MonoBehaviour
    {
        public Transform lookTo;
        public Transform center;
        [SerializeField]
        private float _distance = 1.5f;
        public float _distanceVariation = 1f;
        public float distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
        public float height = 25f;
        public float heightVariation = 1f;
        public float speed = 10f;

        private float _time = -180;
    
        private void Update()
        {    
            Quaternion rotation = Quaternion.Euler(0, _time, 0);
    
            transform.position = center.position - rotation * (Vector3.forward * (distance + Mathf.Sin(_time * 0.125f) * _distanceVariation)) + Vector3.up * height + Vector3.up * Mathf.Sin(_time * 0.125f) * heightVariation;
            transform.LookAt(lookTo);
            _time += Time.smoothDeltaTime * speed;
        }
    }
}