//////////////////////////////////////////////////////
// MK Toon Examples AnimateMaterialProperty Base	//
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
    public abstract class AnimateMaterialProperty<T> : MonoBehaviour
    {
        public float speed = 1;
        private List<Material> _instancedMaterials;
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _instancedMaterials = new List<Material>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public abstract void SetValue(Material material, T value);
        public abstract T GenerateValue(Material material);

        protected float InterpValue()
        {
            return Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
        }
        
        private void Update()
        {
            if(_meshRenderer)
            {
                foreach(Material m in _meshRenderer.materials)
                {
                    T value = GenerateValue(m);
                    SetValue(m, value);
                }
            }
        }
    }
}