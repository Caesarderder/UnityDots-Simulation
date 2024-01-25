#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public class GMask : ScriptableObject
    {
        public const string MASK_MAP_NAME = "Mask Map";

        [SerializeField]
        private GTerrainData terrainData;
        public GTerrainData TerrainData
        {
            get
            {
                return terrainData;
            }
            internal set
            {
                terrainData = value;
            }
        }

        [SerializeField]
        private int maskMapResolution;
        public int MaskMapResolution
        {
            get
            {
                return maskMapResolution;
            }
            set
            {
                int oldValue = maskMapResolution;
                maskMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
                if (oldValue != maskMapResolution)
                {
                    ResampleMaskMap();
                }
            }
        }

        [SerializeField]
        private Texture2D maskMap;
        public Texture2D MaskMap
        {
            get
            {
                if (maskMap == null)
                {
                    maskMap = GCommon.CreateTexture(MaskMapResolution, Color.clear, TextureFormat.RGBA32);
                    maskMap.filterMode = FilterMode.Bilinear;
                    maskMap.wrapMode = TextureWrapMode.Clamp;
                    maskMap.name = MASK_MAP_NAME;
                }
                GCommon.TryAddObjectToAsset(maskMap, TerrainData);
                return maskMap;
            }
        }

        public Texture2D MaskMapOrDefault
        {
            get
            {
                if (maskMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return maskMap;
                }
            }
        }

        public void Reset()
        {
            name = "Mask";
            MaskMapResolution = GRuntimeSettings.Instance.maskDefault.maskMapResolution;
        }

        public void ResetFull()
        {
            Reset();
            if (maskMap!=null)
            {
                GUtilities.DestroyObject(maskMap);
            }
        }

        public void CopyTo(GMask des)
        {
            des.MaskMapResolution = MaskMapResolution;
        }

        private void ResampleMaskMap()
        {
            if (maskMap == null)
                return;
            Texture2D tmp = new Texture2D(MaskMapResolution, MaskMapResolution, TextureFormat.RGBA32, false);
            RenderTexture rt = new RenderTexture(MaskMapResolution, MaskMapResolution, 32, RenderTextureFormat.ARGB32);
            GCommon.CopyToRT(maskMap, rt);
            GCommon.CopyFromRT(tmp, rt);
            rt.Release();
            Object.DestroyImmediate(rt);

            tmp.name = maskMap.name;
            tmp.filterMode = maskMap.filterMode;
            tmp.wrapMode = maskMap.wrapMode;
            Object.DestroyImmediate(maskMap, true);
            maskMap = tmp;
            GCommon.TryAddObjectToAsset(maskMap, TerrainData);
        }

        public float GetMaskMapMemStats()
        {
            if (maskMap == null)
                return 0;
            return maskMap.width * maskMap.height * 4;
        }

        public void RemoveMaskMap()
        {
            if (maskMap != null)
            {
                GUtilities.DestroyObject(maskMap);
            }
        }
    }
}
#endif
