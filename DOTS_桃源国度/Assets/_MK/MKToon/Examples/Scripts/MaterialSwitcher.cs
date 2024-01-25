//////////////////////////////////////////////////////
// MK Toon Examples MaterialSwitcher            	//
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
    public class MaterialSwitcher : MonoBehaviour
    {
        [SerializeField]
        private MK.Toon.Examples.Spectate _spectate = null;

        [SerializeField]
        private List<Material> _materials = new List<Material>();

        private int _currentMaterialIndex;
        [SerializeField]
        private UnityEngine.UI.Text _indexText = null;

        private void Awake()
        {
            Pick(0);
        }

        private List<GameObject> _currentChilds;
        private void GetAllChilds(GameObject go)
        {
            _currentChilds = new List<GameObject>();
            CheckDuplicatedSelection(go);
            GetObjectsRecursively(go);
        }

        private void CheckDuplicatedSelection(GameObject go)
        {
            foreach(GameObject g in _currentChilds)
            {
                if(g == go)
                    return;
            }

            _currentChilds.Add(go);
        }

        private void GetObjectsRecursively(GameObject go)
        {
            if(go == null)
                return;

            for(int i = 0; i < go.transform.childCount; i++)
            {
                if(go.transform == null)
                    continue;

                Transform ct = go.transform.GetChild(i);
                CheckDuplicatedSelection(ct.gameObject);
                GetObjectsRecursively(ct.gameObject);
            }
        }

        private void SetMaterials(GameObject gameObject)
        {
            GetAllChilds(gameObject);
            foreach(GameObject childGo in _currentChilds)
            {
                Renderer r = childGo.GetComponent<Renderer>();
                if(r != null)
                {
                    Material[] mats = new Material[r.materials.Length];
                    for(int i = 0; i < r.materials.Length; i++)
                    {
                        mats[i] = _materials[_currentMaterialIndex];
                        if(_currentMaterialIndex == 1 || _currentMaterialIndex == 3 || _currentMaterialIndex == 4 || _currentMaterialIndex == 5)
                        {
                            if(_spectate.currentObjectIndex == 2 || _spectate.currentObjectIndex == 3 || _currentMaterialIndex == 5)
                                mats[i].mainTextureScale = new Vector2(5, 5);
                            else
                                mats[i].mainTextureScale = new Vector2(1, 1);
                        }
                        else
                            mats[i].mainTextureScale = new Vector2(1, 1);
                    }
                    r.materials = mats;
                }
            }
        }

        public void UpdateMaterials()
        {
            foreach(GameObject go in _spectate.gameObjects)
                SetMaterials(go);
        }

        private void Pick(int index)
        {
            _currentMaterialIndex = index;
            if(_currentMaterialIndex >= _materials.Count)
                _currentMaterialIndex = 0;
            else if(_currentMaterialIndex < 0)
                _currentMaterialIndex = _materials.Count - 1;

            _indexText.text = (1 + _currentMaterialIndex).ToString() + " / " + _materials.Count.ToString();
            foreach(GameObject go in _spectate.gameObjects)
                UpdateMaterials();
        }

        public void PickNext()
        {
            Pick(_currentMaterialIndex + 1);
        }

        public void PickPrevious()
        {
            Pick(_currentMaterialIndex - 1);
        }
    }
}
