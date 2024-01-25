//////////////////////////////////////////////////////
// MK Toon Examples MoveObject                      //
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
    public class MoveObject : MonoBehaviour
    {
        private Vector3 _startPosition = Vector3.zero;

        [SerializeField]
        private Transform _targetPosition = null;

        [SerializeField]
        private float _speed = 1;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            Vector3 dir = _targetPosition.position - transform.position;
            dir.Normalize();

            transform.position += Time.smoothDeltaTime * dir * _speed;

            if(Vector3.Distance(transform.position, _targetPosition.position) < 1.0)
                transform.position = _startPosition;
        }
    }
}
