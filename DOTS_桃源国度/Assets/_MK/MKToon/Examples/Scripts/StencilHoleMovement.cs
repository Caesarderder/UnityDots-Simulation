//////////////////////////////////////////////////////
// MK Toon Examples Stencil Hole Movement 	    	//
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
    public class StencilHoleMovement : MonoBehaviour
    {
        [SerializeField]
        private Transform _endPos = null;
        [SerializeField]
        private Transform _startPos = null;
        private bool _moveForward = true;
        private readonly float _moveSpeed = 0.5f;

        private void Update()
        {
            transform.position += ((_moveForward) ? (_endPos.position - transform.position).normalized * _moveSpeed * Time.smoothDeltaTime : (_startPos.position - transform.position).normalized * _moveSpeed * Time.smoothDeltaTime) * 0.5f;

            if(_moveForward)
            {
                if(Vector3.Magnitude(_endPos.position - transform.position) < 0.1f)
                    _moveForward = !_moveForward;
            }
            if(!_moveForward)
            {
                if(Vector3.Magnitude(_startPos.position - transform.position) < 0.1f)
                    _moveForward = !_moveForward;
            }
        }
    }
}
