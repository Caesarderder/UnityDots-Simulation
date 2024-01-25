//////////////////////////////////////////////////////
// MK Toon Examples Spectate    	    	    	//
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
    public class Spectate : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Light _mainLight = null;
        [SerializeField]
        private UnityEngine.UI.Text _indexText = null;
        [SerializeField]
        private List<GameObject> _gameObjects = new List<GameObject>();
        public List<GameObject> gameObjects { get{ return _gameObjects; } }
        [SerializeField]
        private Transform _spectatePosition = null;
        private Vector3 _currentObjectSrcPosition = Vector3.zero;
        private int _currentObjectIndex = 0;
        public int currentObjectIndex { get{ return _currentObjectIndex; } }

        protected virtual void PPSetup(string name){}

        private void Awake()
        {
            Pick(0);
        }

        private void Pick(int index)
        {
            _currentObjectIndex = index;
            if(_currentObjectIndex >= _gameObjects.Count)
                _currentObjectIndex = 0;
            else if(_currentObjectIndex < 0)
                _currentObjectIndex = _gameObjects.Count - 1;
            if(_gameObjects[_currentObjectIndex])
            {
                _currentObjectSrcPosition = _gameObjects[_currentObjectIndex].transform.position;
                _gameObjects[_currentObjectIndex].transform.position = _spectatePosition.position;
            }

            _indexText.text = (1 + _currentObjectIndex).ToString() + " / " + _gameObjects.Count.ToString();

            if(_gameObjects[_currentObjectIndex].name.Contains("Particles"))
                _mainLight.intensity = 0.0f;
            else
                _mainLight.intensity = 1f;

            PPSetup(_gameObjects[_currentObjectIndex].name);
        }

        private void Reset(int index)
        {
            if(_gameObjects[index])
            {
                _gameObjects[_currentObjectIndex].transform.position = _currentObjectSrcPosition;
            }
        }

        public void PickNext()
        {
            Reset(_currentObjectIndex);
            Pick(_currentObjectIndex + 1);
        }

        public void PickPrevious()
        {
            Reset(_currentObjectIndex);
            Pick(_currentObjectIndex - 1);
        }
    }
}