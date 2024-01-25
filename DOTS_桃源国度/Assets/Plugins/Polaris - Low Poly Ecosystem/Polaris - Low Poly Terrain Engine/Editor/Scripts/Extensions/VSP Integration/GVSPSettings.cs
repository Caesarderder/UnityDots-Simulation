#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.TouchReact;
using AwesomeTechnologies.ColliderSystem;
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.PrefabSpawner;

namespace Pinwheel.Griffin.VegetationStudioPro
{
    //[CreateAssetMenu(menuName = "Polaris/GVSP Settings")]
    public class GVSPSettings : ScriptableObject
    {
        [System.Serializable]
        public struct GQuickSetupSettings
        {
            [SerializeField]
            private bool createVegetationPackage;
            public bool CreateVegetationPackage
            {
                get
                {
                    return createVegetationPackage;
                }
                set
                {
                    createVegetationPackage = value;
                }
            }

            [SerializeField]
            private bool createPersistentStorage;
            public bool CreatePersistentStorage
            {
                get
                {
                    return createPersistentStorage;
                }
                set
                {
                    createPersistentStorage = value;
                }
            }
        }

        [System.Serializable]
        public struct GImportSettings
        {
            public VegetationStudioManager VSManager { get; set; }
            public GStylizedTerrain Terrain { get; set; }

            [SerializeField]
            private bool setProceduralDensityToZero;
            public bool SetProceduralDensityToZero
            {
                get
                {
                    return setProceduralDensityToZero;
                }
                set
                {
                    setProceduralDensityToZero = value;
                }
            }
        }

        private static GVSPSettings instance;
        public static GVSPSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GVSPSettings>("GVSPSettings");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GVSPSettings>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private GQuickSetupSettings quickSetup;
        public GQuickSetupSettings QuickSetup
        {
            get
            {
                return quickSetup;
            }
            set
            {
                quickSetup = value;
            }
        }

        [SerializeField]
        private GImportSettings import;
        public GImportSettings Import
        {
            get
            {
                return import;
            }
            set
            {
                import = value;
            }
        }
    }
}
#endif
