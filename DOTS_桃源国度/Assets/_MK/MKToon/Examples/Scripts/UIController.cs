//////////////////////////////////////////////////////
// MK Toon Examples UI Controller	    	    	//
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
    public class UIController : MonoBehaviour
    {
        private bool _show = true;
        public GameObject[] _gameObjects;

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                _show = !_show;
                for(int i = 0; i < _gameObjects.Length; i++)
                {
                    _gameObjects[i].SetActive(_show);
                }
            }
        }

        public void ExitApplication()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBPLAYER
                Application.OpenURL("https://www.google.com");
            #else
                Application.Quit();
            #endif
        }
    }
}