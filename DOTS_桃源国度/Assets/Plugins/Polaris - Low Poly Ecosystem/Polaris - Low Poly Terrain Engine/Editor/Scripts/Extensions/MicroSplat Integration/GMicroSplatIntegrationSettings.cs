#if GRIFFIN
#if __MICROSPLAT_POLARIS__
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.MicroSplat
{
    //[CreateAssetMenu(menuName = "Polaris V2/MicroSplat Integration Settings")]
    public class GMicroSplatIntegrationSettings : ScriptableObject
    {
        private static GMicroSplatIntegrationSettings instance;
        public static GMicroSplatIntegrationSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GMicroSplatIntegrationSettings>("MicroSplatIntegrationSettings");
                }
                return instance;
            }
        }

        [SerializeField]
        private string dataDirectory;
        public string DataDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(dataDirectory))
                {
                    dataDirectory = "Assets/";
                }
                return dataDirectory;
            }
            set
            {
                dataDirectory = value;
            }
        }

        [SerializeField]
        private string shaderNamePrefix;
        public string ShaderNamePrefix
        {
            get
            {
                if (string.IsNullOrEmpty(shaderNamePrefix))
                {
                    shaderNamePrefix = "MicroSplat";
                }
                return shaderNamePrefix;
            }
            set
            {
                shaderNamePrefix = value;
            }
        }

        [SerializeField]
        private bool initTextureEntries;
        public bool InitTextureEntries
        {
            get
            {
                return initTextureEntries;
            }
            set
            {
                initTextureEntries = value;
            }
        }
    }
}
#endif
#endif
