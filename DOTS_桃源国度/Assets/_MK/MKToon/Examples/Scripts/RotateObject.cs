//////////////////////////////////////////////////////
// MK Toon Examples RotateObject                	//
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
    public class RotateObject : MonoBehaviour
    {
        [SerializeField]
        private float _rotateSpeedY = 1;
        [SerializeField]
        private float _bounceSpeed = 1;
        [SerializeField]
        private float _bounceLimit = 0.05f;

        public bool limitYRotation = false;
        public float secondsForChange = 3;

        private float directionYRotation;

        private Vector3 _startPos = Vector3.zero;
        private IEnumerator coroutine;

        private void Start()
        {
            _startPos = transform.position;
            directionYRotation = 1;
            coroutine = ChangeYDirection();
            if(limitYRotation)
                StartCoroutine(coroutine);
            secondsForChange *=2;
        }

        IEnumerator ChangeYDirection()
        {
            yield return new WaitForSeconds(secondsForChange);
            directionYRotation *= -1;
            StopCoroutine(coroutine);
            coroutine = ChangeYDirection();
            StartCoroutine(coroutine);
        }

        private void OnDestroy()
        {
            if(limitYRotation)
                StopCoroutine(coroutine);
        }

        private void Update()
        {
            Vector3 pos = new Vector3(transform.position.x, _startPos.y, transform.position.z);
            pos.y += Mathf.Sin(Time.time * _bounceSpeed) * _bounceLimit;
            transform.position = pos;

            if(limitYRotation)
            {
                transform.Rotate(new Vector3(0, _rotateSpeedY * directionYRotation, 0) * Time.smoothDeltaTime, Space.World);
            }
            else
            {
                transform.Rotate(new Vector3(0, _rotateSpeedY, 0) * Time.smoothDeltaTime, Space.World);
            }
        }
    }
}
