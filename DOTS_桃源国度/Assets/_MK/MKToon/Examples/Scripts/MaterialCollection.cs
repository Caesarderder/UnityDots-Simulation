using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.Toon.Examples
{
    public class MaterialCollection : MonoBehaviour
    {
        private MeshRenderer _meshRenderer;
        public List<MK.Toon.Examples.MaterialSet> materialSets = new List<MK.Toon.Examples.MaterialSet>();
        private int _currentIndex = 0;

        private void Start()
        {
            MaterialCollectionController.instance.changeMaterialSets += ChangeMaterialSet;
            _meshRenderer = GetComponent<MeshRenderer>();
            _currentIndex = 0;
        }

        private void ChangeMaterialSet()
        {   
            _currentIndex++;
            if(_currentIndex >= materialSets.Count)
            {
                _currentIndex = 0;
            }
            ApplyMaterialSet(_currentIndex);
        }

        private void ApplyMaterialSet(int index)
        {
            _meshRenderer.sharedMaterials = materialSets[index].materials.ToArray();
        }

        private void OnDestroy()
        {
            MaterialCollectionController.instance.changeMaterialSets -= ChangeMaterialSet;
        }
    }
}
