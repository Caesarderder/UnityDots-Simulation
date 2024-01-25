#if GRIFFIN
using UnityEngine;
using UnityEngine.Serialization;
#if __MICROSPLAT_POLARIS__
using JBooth.MicroSplat;
#endif
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Pinwheel.Griffin
{
    public class GShading : ScriptableObject
    {
        public const string ALBEDO_MAP_NAME = "Albedo Map";
        public const string METALLIC_MAP_NAME = "Metallic Map";
        public const string COLOR_BY_HEIGHT_MAP_NAME = "Color By Height Map";
        public const string COLOR_BY_NORMAL_MAP_NAME = "Color By Normal Map";
        public const string COLOR_BLEND_MAP_NAME = "Color Blend Map";
        public const string SPLAT_CONTROL_MAP_NAME = "Splat Control Map";

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
        private GShadingSystem shadingSystem;
        public GShadingSystem ShadingSystem
        {
            get
            {
#if !__MICROSPLAT_POLARIS__
                shadingSystem = GShadingSystem.Polaris;
#endif
                return shadingSystem;
            }
            set
            {
                shadingSystem = value;
            }
        }

        public Material MaterialToRender
        {
            get
            {
                return CustomMaterial;
            }
        }

        [SerializeField]
        private Material customMaterial;
        public Material CustomMaterial
        {
            get
            {
                return customMaterial;
            }
            set
            {
                customMaterial = value;
            }
        }

        [SerializeField]
        private int albedoMapResolution;
        public int AlbedoMapResolution
        {
            get
            {
                return albedoMapResolution;
            }
            set
            {
                int oldValue = albedoMapResolution;
                albedoMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
                if (oldValue != albedoMapResolution)
                {
                    ResizeAlbedoMap();
                }
            }
        }

        [SerializeField]
        private Texture2D albedoMap;
        public Texture2D AlbedoMap
        {
            get
            {
                if (albedoMap == null)
                {
                    albedoMap = GCommon.CreateTexture(AlbedoMapResolution, Color.clear);
                    albedoMap.filterMode = FilterMode.Bilinear;
                    albedoMap.wrapMode = TextureWrapMode.Clamp;
                    albedoMap.name = ALBEDO_MAP_NAME;
                }
                GCommon.TryAddObjectToAsset(albedoMap, TerrainData);
                return albedoMap;
            }
        }

        public Texture2D AlbedoMapOrDefault
        {
            get
            {
                if (albedoMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return albedoMap;
                }
            }
        }

        [SerializeField]
        private int metallicMapResolution;
        public int MetallicMapResolution
        {
            get
            {
                return metallicMapResolution;
            }
            set
            {
                int oldValue = metallicMapResolution;
                metallicMapResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
                if (oldValue != metallicMapResolution)
                {
                    ResizeMetallicMap();
                }
            }
        }

        [SerializeField]
        [FormerlySerializedAs("internal_MetallicMap")]
        private Texture2D metallicMap;
        public Texture2D MetallicMap
        {
            get
            {
                if (metallicMap == null)
                {
                    metallicMap = GCommon.CreateTexture(MetallicMapResolution, Color.clear);
                    metallicMap.filterMode = FilterMode.Bilinear;
                    metallicMap.wrapMode = TextureWrapMode.Clamp;
                    metallicMap.name = METALLIC_MAP_NAME; ;
                }
                GCommon.TryAddObjectToAsset(metallicMap, TerrainData);
                return metallicMap;
            }
        }

        public Texture2D MetallicMapOrDefault
        {
            get
            {
                if (metallicMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return metallicMap;
                }
            }
        }

        [SerializeField]
        private string albedoMapPropertyName;
        public string AlbedoMapPropertyName
        {
            get
            {
                return albedoMapPropertyName;
            }
            set
            {
                albedoMapPropertyName = value;
            }
        }

        [SerializeField]
        private string metallicMapPropertyName;
        public string MetallicMapPropertyName
        {
            get
            {
                return metallicMapPropertyName;
            }
            set
            {
                metallicMapPropertyName = value;
            }
        }

        [SerializeField]
        private Gradient colorByHeight;
        public Gradient ColorByHeight
        {
            get
            {
                if (colorByHeight == null)
                {
                    colorByHeight = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorByHeight);
                }
                return colorByHeight;
            }
            set
            {
                colorByHeight = value;
            }
        }

        [SerializeField]
        private Gradient colorByNormal;
        public Gradient ColorByNormal
        {
            get
            {
                if (colorByNormal == null)
                {
                    colorByNormal = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorByNormal);
                }
                return colorByNormal;
            }
            set
            {
                colorByNormal = value;
            }
        }

        [SerializeField]
        private AnimationCurve colorBlendCurve;
        public AnimationCurve ColorBlendCurve
        {
            get
            {
                if (colorBlendCurve == null)
                {
                    colorBlendCurve = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorBlendCurve);
                }
                return colorBlendCurve;
            }
            set
            {
                colorBlendCurve = value;
            }
        }

        [SerializeField]
        private string colorByHeightPropertyName;
        public string ColorByHeightPropertyName
        {
            get
            {
                return colorByHeightPropertyName;
            }
            set
            {
                colorByHeightPropertyName = value;
            }
        }

        [SerializeField]
        private string colorByNormalPropertyName;
        public string ColorByNormalPropertyName
        {
            get
            {
                return colorByNormalPropertyName;
            }
            set
            {
                colorByNormalPropertyName = value;
            }
        }

        [SerializeField]
        private string colorBlendPropertyName;
        public string ColorBlendPropertyName
        {
            get
            {
                return colorBlendPropertyName;
            }
            set
            {
                colorBlendPropertyName = value;
            }
        }

        [SerializeField]
        private string dimensionPropertyName;
        public string DimensionPropertyName
        {
            get
            {
                return dimensionPropertyName;
            }
            set
            {
                dimensionPropertyName = value;
            }
        }

        [SerializeField]
        private Texture2D colorByHeightMap;
        public Texture2D ColorByHeightMap
        {
            get
            {
                if (colorByHeightMap == null)
                {
                    UpdateLookupTextures();
                }
                return colorByHeightMap;
            }
        }

        public Texture2D ColorByHeightMapOrDefault
        {
            get
            {
                if (colorByHeightMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return colorByHeightMap;
                }
            }
        }

        [SerializeField]
        private Texture2D colorByNormalMap;
        public Texture2D ColorByNormalMap
        {
            get
            {
                if (colorByNormalMap == null)
                {
                    UpdateLookupTextures();
                }
                return colorByNormalMap;
            }
        }

        public Texture2D ColorByNormalMapOrDefault
        {
            get
            {
                if (colorByNormalMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return colorByNormalMap;
                }
            }
        }

        [SerializeField]
        private Texture2D colorBlendMap;
        public Texture2D ColorBlendMap
        {
            get
            {
                if (colorBlendMap == null)
                {
                    UpdateLookupTextures();
                }
                return colorBlendMap;
            }
        }

        public Texture2D ColorBlendMapOrDefault
        {
            get
            {
                if (colorBlendMap == null)
                {
                    return GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return colorBlendMap;
                }
            }
        }

        [SerializeField]
        private GSplatPrototypeGroup splats;
        public GSplatPrototypeGroup Splats
        {
            get
            {
                return splats;
            }
            set
            {
                splats = value;
            }
        }

        [SerializeField]
        private int splatControlResolution;
        public int SplatControlResolution
        {
            get
            {
                return splatControlResolution;
            }
            set
            {
                int oldValue = splatControlResolution;
                splatControlResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(value), GCommon.TEXTURE_SIZE_MIN, GCommon.TEXTURE_SIZE_MAX);
                if (oldValue != splatControlResolution)
                {
                    ResizeSplatControlMaps();
                }
            }
        }

        [SerializeField]
        private Texture2D[] splatControls;
        public Texture2D[] SplatControls
        {
            get
            {
                if (splatControls == null)
                {
                    splatControls = new Texture2D[0];
                }
                if (splatControls.Length == SplatControlMapCount)
                {
                    return splatControls;
                }
                else
                {
                    Texture2D[] controls = new Texture2D[SplatControlMapCount];
                    for (int i = 0; i < Mathf.Min(SplatControlMapCount, splatControls.Length); ++i)
                    {
                        controls[i] = splatControls[i];
                    }
                    if (splatControls.Length > SplatControlMapCount)
                    {
                        for (int i = SplatControlMapCount; i < splatControls.Length; ++i)
                        {
                            Object.DestroyImmediate(splatControls[i], true);
                        }
                    }

                    splatControls = controls;
                    return splatControls;
                }
            }
        }

        [SerializeField]
        private string splatControlMapPropertyName;
        public string SplatControlMapPropertyName
        {
            get
            {
                return splatControlMapPropertyName;
            }
            set
            {
                splatControlMapPropertyName = value;
            }
        }

        [SerializeField]
        private string splatMapPropertyName;
        public string SplatMapPropertyName
        {
            get
            {
                return splatMapPropertyName;
            }
            set
            {
                splatMapPropertyName = value;
            }
        }

        [SerializeField]
        private string splatNormalPropertyName;
        public string SplatNormalPropertyName
        {
            get
            {
                return splatNormalPropertyName;
            }
            set
            {
                splatNormalPropertyName = value;
            }
        }

        [SerializeField]
        private string splatMetallicPropertyName;
        public string SplatMetallicPropertyName
        {
            get
            {
                return splatMetallicPropertyName;
            }
            set
            {
                splatMetallicPropertyName = value;
            }
        }

        [SerializeField]
        private string splatSmoothnessPropertyName;
        public string SplatSmoothnessPropertyName
        {
            get
            {
                return splatSmoothnessPropertyName;
            }
            set
            {
                splatSmoothnessPropertyName = value;
            }
        }

        public int SplatControlMapCount
        {
            get
            {
                if (ShadingSystem == GShadingSystem.Polaris)
                {
                    if (Splats == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return (Splats.Prototypes.Count + 3) / 4;
                    }
                }
#if __MICROSPLAT_POLARIS__ 
                else
                {
                    if (MicroSplatTextureArrayConfig == null)
                        return 0;
                    int textureCount = MicroSplatTextureArrayConfig.sourceTextures.Count;
                    return (textureCount + 3) / 4;
                }
#else
                return 0;
#endif
            }
        }

#if __MICROSPLAT_POLARIS__
        [SerializeField]
        private TextureArrayConfig msTextureArrayConfig;
        public TextureArrayConfig MicroSplatTextureArrayConfig
        {
            get
            {
                return msTextureArrayConfig;
            }
            set
            {
                msTextureArrayConfig = value;
            }
        }
#endif

        public void Reset()
        {
            name = "Shading";
            ShadingSystem = GShadingSystem.Polaris;
            AlbedoMapResolution = GRuntimeSettings.Instance.shadingDefault.albedoMapResolution; ;
            MetallicMapResolution = GRuntimeSettings.Instance.shadingDefault.metallicMapResolution; ;
            AlbedoMapPropertyName = GRuntimeSettings.Instance.shadingDefault.albedoMapPropertyName;
            MetallicMapPropertyName = GRuntimeSettings.Instance.shadingDefault.metallicMapPropertyName;
            ColorByHeight = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorByHeight);
            ColorByNormal = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorByNormal);
            ColorBlendCurve = GUtilities.Clone(GRuntimeSettings.Instance.shadingDefault.colorBlendCurve);
            ColorByHeightPropertyName = GRuntimeSettings.Instance.shadingDefault.colorByHeightPropertyName;
            ColorByNormalPropertyName = GRuntimeSettings.Instance.shadingDefault.colorByNormalPropertyName;
            ColorBlendPropertyName = GRuntimeSettings.Instance.shadingDefault.colorBlendPropertyName;
            DimensionPropertyName = GRuntimeSettings.Instance.shadingDefault.dimensionPropertyName;
            SplatControlResolution = GRuntimeSettings.Instance.shadingDefault.splatControlResolution;
            SplatControlMapPropertyName = GRuntimeSettings.Instance.shadingDefault.splatControlMapPropertyName;
            SplatMapPropertyName = GRuntimeSettings.Instance.shadingDefault.splatMapPropertyName;
            SplatNormalPropertyName = GRuntimeSettings.Instance.shadingDefault.splatNormalPropertyName;
            SplatMetallicPropertyName = GRuntimeSettings.Instance.shadingDefault.splatMetallicPropertyName;
            SplatSmoothnessPropertyName = GRuntimeSettings.Instance.shadingDefault.splatSmoothnessPropertyName;
        }

        public void ResetFull()
        {
            Reset();
            if (albedoMap != null)
            {
                GUtilities.DestroyObject(albedoMap);
            }
            if (metallicMap != null)
            {
                GUtilities.DestroyObject(metallicMap);
            }
            for (int i = 0; i < SplatControlMapCount; ++i)
            {
                if (SplatControls[i] != null)
                {
                    GUtilities.DestroyObject(SplatControls[i]);
                }
            }
            if (colorByHeightMap != null)
            {
                GUtilities.DestroyObject(colorByHeightMap);
            }
            if (colorByNormalMap != null)
            {
                GUtilities.DestroyObject(colorByNormalMap);
            }
            if (colorBlendMap != null)
            {
                GUtilities.DestroyObject(colorBlendMap);
            }

            TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            UpdateMaterials();
        }

        private void ResizeAlbedoMap()
        {
            if (albedoMap == null)
                return;
            Texture2D tmp = GCommon.CreateTexture(AlbedoMapResolution, Color.clear);
            RenderTexture rt = new RenderTexture(AlbedoMapResolution, AlbedoMapResolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            GCommon.CopyToRT(albedoMap, rt);
            GCommon.CopyFromRT(tmp, rt);
            rt.Release();
            Object.DestroyImmediate(rt);

            tmp.name = albedoMap.name;
            tmp.filterMode = albedoMap.filterMode;
            tmp.wrapMode = albedoMap.wrapMode;
            Object.DestroyImmediate(albedoMap, true);
            albedoMap = tmp;
            GCommon.TryAddObjectToAsset(albedoMap, TerrainData);
            UpdateMaterials();
        }

        public void UpdateMaterials()
        {
            if (MaterialToRender != null)
            {
                if (MaterialToRender.HasProperty(AlbedoMapPropertyName))
                {
                    MaterialToRender.SetTexture(AlbedoMapPropertyName, AlbedoMapOrDefault);
                }
                if (MaterialToRender.HasProperty(MetallicMapPropertyName))
                {
                    MaterialToRender.SetTexture(MetallicMapPropertyName, MetallicMapOrDefault);
                }

                //if (MaterialToRender.HasProperty(ColorByHeightPropertyName)||
                //    MaterialToRender.HasProperty(ColorByNormalPropertyName)||
                //    MaterialToRender.HasProperty(ColorBlendPropertyName))
                //{
                //    UpdateLookupTextures();
                //}
                if (MaterialToRender.HasProperty(ColorByHeightPropertyName))
                {
                    MaterialToRender.SetTexture(ColorByHeightPropertyName, ColorByHeightMapOrDefault);
                }
                if (MaterialToRender.HasProperty(ColorByNormalPropertyName))
                {
                    MaterialToRender.SetTexture(ColorByNormalPropertyName, ColorByNormalMapOrDefault);
                }
                if (MaterialToRender.HasProperty(ColorBlendPropertyName))
                {
                    MaterialToRender.SetTexture(ColorBlendPropertyName, ColorBlendMapOrDefault);
                }

                if (MaterialToRender.HasProperty(DimensionPropertyName))
                {
                    Vector4 dim = new Vector4(
                        TerrainData.Geometry.Width,
                        TerrainData.Geometry.Height,
                        TerrainData.Geometry.Length,
                        0);
                    MaterialToRender.SetVector(DimensionPropertyName, dim);
                }

                if (MaterialToRender.HasProperty(SplatControlMapPropertyName))
                {
                    MaterialToRender.SetTexture(SplatControlMapPropertyName, GetSplatControlOrDefault(0));
                }

                for (int i = 0; i < SplatControlMapCount; ++i)
                {
                    if (MaterialToRender.HasProperty(SplatControlMapPropertyName + i))
                    {
                        MaterialToRender.SetTexture(SplatControlMapPropertyName + i, GetSplatControlOrDefault(i));
                    }
                }

                if (Splats != null)
                {
                    for (int i = 0; i < Splats.Prototypes.Count; ++i)
                    {
                        GSplatPrototype p = Splats.Prototypes[i];
                        if (MaterialToRender.HasProperty(SplatMapPropertyName + i))
                        {
                            MaterialToRender.SetTexture(SplatMapPropertyName + i, p.Texture);
                            Vector2 terrainSize = new Vector2(TerrainData.Geometry.Width, TerrainData.Geometry.Length);
                            Vector2 textureScale = new Vector2(
                                p.TileSize.x != 0 ? terrainSize.x / p.TileSize.x : 0,
                                p.TileSize.y != 0 ? terrainSize.y / p.TileSize.y : 0);
                            Vector2 textureOffset = new Vector2(
                                p.TileOffset.x != 0 ? terrainSize.x / p.TileOffset.x : 0,
                                p.TileOffset.y != 0 ? terrainSize.y / p.TileOffset.y : 0);
                            MaterialToRender.SetTextureScale(SplatMapPropertyName + i, textureScale);
                            MaterialToRender.SetTextureOffset(SplatMapPropertyName + i, textureOffset);
                        }
                        if (MaterialToRender.HasProperty(SplatNormalPropertyName + i))
                        {
                            MaterialToRender.SetTexture(SplatNormalPropertyName + i, p.NormalMap);
                        }
                        if (MaterialToRender.HasProperty(SplatMetallicPropertyName + i))
                        {
                            MaterialToRender.SetFloat(SplatMetallicPropertyName + i, p.Metallic);
                        }
                        if (MaterialToRender.HasProperty(SplatSmoothnessPropertyName + i))
                        {
                            MaterialToRender.SetFloat(SplatSmoothnessPropertyName + i, p.Smoothness);
                        }
                    }
                }
            }
        }

        private void ResizeMetallicMap()
        {
            if (metallicMap == null)
                return;
            Texture2D tmp = GCommon.CreateTexture(MetallicMapResolution, Color.black);
            RenderTexture rt = new RenderTexture(MetallicMapResolution, MetallicMapResolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            GCommon.CopyToRT(metallicMap, rt);
            GCommon.CopyFromRT(tmp, rt);
            rt.Release();
            Object.DestroyImmediate(rt);

            tmp.name = metallicMap.name;
            tmp.filterMode = metallicMap.filterMode;
            tmp.wrapMode = metallicMap.wrapMode;
            Object.DestroyImmediate(metallicMap, true);
            metallicMap = tmp;
            GCommon.TryAddObjectToAsset(metallicMap, TerrainData);
            UpdateMaterials();
        }

        public void UpdateLookupTextures()
        {
            if (colorByHeightMap != null)
                Object.DestroyImmediate(colorByHeightMap, true);
            if (colorByNormalMap != null)
                Object.DestroyImmediate(colorByNormalMap, true);
            if (colorBlendMap != null)
                Object.DestroyImmediate(colorBlendMap, true);

            int width = 256;
            int height = 8;
            colorByHeightMap = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            colorByHeightMap.filterMode = FilterMode.Bilinear;
            colorByHeightMap.wrapMode = TextureWrapMode.Clamp;
            colorByHeightMap.name = COLOR_BY_HEIGHT_MAP_NAME;

            colorByNormalMap = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            colorByNormalMap.filterMode = FilterMode.Bilinear;
            colorByNormalMap.wrapMode = TextureWrapMode.Clamp;
            colorByNormalMap.name = COLOR_BY_NORMAL_MAP_NAME;

            colorBlendMap = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            colorBlendMap.filterMode = FilterMode.Bilinear;
            colorBlendMap.wrapMode = TextureWrapMode.Clamp;
            colorBlendMap.name = COLOR_BLEND_MAP_NAME;

            Color[] cbhColors = new Color[width * height];
            Color[] cbnColors = new Color[width * height];
            Color[] cbColors = new Color[width * height];

            for (int x = 0; x < width; ++x)
            {
                float f = Mathf.InverseLerp(0, width - 1, x);
                Color cbh = ColorByHeight.Evaluate(f);
                Color cbn = ColorByNormal.Evaluate(f);
                Color cb = Color.white * ColorBlendCurve.Evaluate(f);
                for (int y = 0; y < height; ++y)
                {
                    int index = GUtilities.To1DIndex(x, y, width);
                    cbhColors[index] = cbh;
                    cbnColors[index] = cbn;
                    cbColors[index] = cb;
                }
            }

            colorByHeightMap.SetPixels(cbhColors);
            colorByHeightMap.Apply();
            GCommon.TryAddObjectToAsset(colorByHeightMap, TerrainData);

            colorByNormalMap.SetPixels(cbnColors);
            colorByNormalMap.Apply();
            GCommon.TryAddObjectToAsset(colorByNormalMap, TerrainData);

            colorBlendMap.SetPixels(cbColors);
            colorBlendMap.Apply();
            GCommon.TryAddObjectToAsset(colorBlendMap, TerrainData);
        }

        public Texture2D GetSplatControl(int index)
        {
            if (index < 0 || index >= SplatControlMapCount)
                throw new System.ArgumentException("Index must be >=0 and <=SplatControlMapCount");
            else
            {
                Texture2D t = SplatControls[index];
                if (t == null)
                {
                    Color fillColor = (index == 0 && SplatControlMapCount == 1) ? new Color(1, 0, 0, 0) : new Color(0, 0, 0, 0);
                    t = GCommon.CreateTexture(SplatControlResolution, fillColor);
                    t.filterMode = FilterMode.Bilinear;
                    t.wrapMode = TextureWrapMode.Clamp;
                    t.name = SPLAT_CONTROL_MAP_NAME + " " + index;
                    SplatControls[index] = t;
                }
                GCommon.TryAddObjectToAsset(t, TerrainData);
                return t;
            }
        }

        public Texture2D GetSplatControlOrDefault(int index)
        {
            if (index < 0 || index >= SplatControlMapCount)
                throw new System.ArgumentException("Index must be >=0 and <=SplatControlMapCount");
            else
            {
                Texture2D t = SplatControls[index];
                if (t == null)
                {
                    return index == 0 ? GRuntimeSettings.Instance.defaultTextures.redTexture : GRuntimeSettings.Instance.defaultTextures.blackTexture;
                }
                else
                {
                    return t;
                }
            }
        }

        private void ResizeSplatControlMaps()
        {
            for (int i = 0; i < SplatControlMapCount; ++i)
            {
                Texture2D t = SplatControls[i];
                if (t == null)
                    return;
                Texture2D tmp = GCommon.CreateTexture(SplatControlResolution, Color.clear);
                RenderTexture rt = new RenderTexture(SplatControlResolution, SplatControlResolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                GCommon.CopyToRT(t, rt);
                GCommon.CopyFromRT(tmp, rt);
                rt.Release();
                Object.DestroyImmediate(rt);

                tmp.name = t.name;
                tmp.filterMode = t.filterMode;
                tmp.wrapMode = t.wrapMode;
                Object.DestroyImmediate(t, true);
                SplatControls[i] = tmp;
                GCommon.TryAddObjectToAsset(tmp, TerrainData);
            }

            UpdateMaterials();
        }

        public void ConvertSplatsToAlbedo()
        {
            if (Splats == null)
                return;
            RenderTexture albedoRt = new RenderTexture(AlbedoMapResolution, AlbedoMapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Material mat = GInternalMaterials.SplatsToAlbedoMaterial;

            for (int i = 0; i < SplatControlMapCount; ++i)
            {
                Texture2D controlMap = GetSplatControlOrDefault(i);
                mat.SetTexture("_Control0", controlMap);
                for (int channel = 0; channel < 4; ++channel)
                {
                    int prototypeIndex = i * 4 + channel;
                    if (prototypeIndex < Splats.Prototypes.Count)
                    {
                        GSplatPrototype p = Splats.Prototypes[prototypeIndex];
                        mat.SetTexture("_Splat" + channel, p.Texture);
                        Vector2 terrainSize = new Vector2(TerrainData.Geometry.Width, TerrainData.Geometry.Length);
                        Vector2 textureScale = new Vector2(
                            p.TileSize.x != 0 ? terrainSize.x / p.TileSize.x : 0,
                            p.TileSize.y != 0 ? terrainSize.y / p.TileSize.y : 0);
                        Vector2 textureOffset = new Vector2(
                            p.TileOffset.x != 0 ? terrainSize.x / p.TileOffset.x : 0,
                            p.TileOffset.y != 0 ? terrainSize.y / p.TileOffset.y : 0);
                        mat.SetTextureScale("_Splat" + channel, textureScale);
                        mat.SetTextureOffset("_Splat" + channel, textureOffset);
                    }
                    else
                    {
                        mat.SetTexture("_Splat" + channel, null);
                        mat.SetTextureScale("_Splat" + channel, Vector2.zero);
                        mat.SetTextureOffset("_Splat" + channel, Vector2.zero);
                    }
                }

                GCommon.DrawQuad(albedoRt, GCommon.FullRectUvPoints, mat, 0);
            }

            GCommon.CopyFromRT(AlbedoMap, albedoRt);
            albedoRt.Release();
            GUtilities.DestroyObject(albedoRt);
        }

        public void CopyTo(GShading des)
        {
            des.AlbedoMapResolution = AlbedoMapResolution;
            des.MetallicMapResolution = MetallicMapResolution;
            des.AlbedoMapPropertyName = AlbedoMapPropertyName;
            des.MetallicMapPropertyName = MetallicMapPropertyName;
            des.ColorByHeight = GUtilities.Clone(ColorByHeight);
            des.ColorByNormal = GUtilities.Clone(ColorByNormal);
            des.ColorBlendCurve = GUtilities.Clone(ColorBlendCurve);
            des.ColorByHeightPropertyName = ColorByHeightPropertyName;
            des.ColorByNormalPropertyName = ColorByNormalPropertyName;
            des.ColorBlendPropertyName = ColorBlendPropertyName;
            des.DimensionPropertyName = DimensionPropertyName;
            des.Splats = Splats;
            des.SplatControlResolution = SplatControlResolution;
            des.SplatControlMapPropertyName = SplatControlMapPropertyName;
            des.SplatMapPropertyName = SplatMapPropertyName;
            des.SplatNormalPropertyName = SplatNormalPropertyName;
            des.SplatMetallicPropertyName = SplatMetallicPropertyName;
            des.SplatSmoothnessPropertyName = SplatSmoothnessPropertyName;
        }

        public bool IsMaterialUseNormalMap()
        {
            if (CustomMaterial == null)
                return false;
            return CustomMaterial.HasProperty(SplatNormalPropertyName + "0");
        }

        public float GetAlbedoMapMemStats()
        {
            if (albedoMap == null)
                return 0;
            return albedoMap.width * albedoMap.height * 4;
        }

        public float GetMetallicMapMemStats()
        {
            if (metallicMap == null)
                return 0;
            return metallicMap.width * metallicMap.height * 4;
        }

        public float GetControlMapMemStats()
        {
            if (splatControls == null)
                return 0;
            float memory = 0;
            for (int i = 0; i < splatControls.Length; ++i)
            {
                if (splatControls[i] != null)
                {
                    memory += splatControls[i].width * splatControls[i].height * 4;
                }
            }
            return memory;
        }

        public float GetLookupTexturesMemStats()
        {
            float memory = 0;
            if (colorByHeightMap != null)
            {
                memory += colorByHeightMap.width * colorByNormalMap.height * 4;
            }
            if (colorByNormalMap != null)
            {
                memory += colorByNormalMap.width * colorByNormalMap.height * 4;
            }
            if (colorBlendMap != null)
            {
                memory += colorBlendMap.width * colorBlendMap.height * 4;
            }
            return memory;
        }

        public void RemoveAlbedoMap()
        {
            if (albedoMap != null)
            {
                GUtilities.DestroyObject(albedoMap);
            }
        }

        public void RemoveMetallicMap()
        {
            if (metallicMap != null)
            {
                GUtilities.DestroyObject(metallicMap);
            }
        }

        public void RemoveSplatControlMaps()
        {
            if (splatControls != null)
            {
                for (int i = 0; i < splatControls.Length; ++i)
                {
                    if (splatControls[i] != null)
                    {
                        GUtilities.DestroyObject(splatControls[i]);
                    }
                }
            }
        }

        public void RemoveGradientLookupMaps()
        {
            if (colorByHeightMap != null)
            {
                GUtilities.DestroyObject(colorByHeightMap);
            }
            if (colorByNormalMap != null)
            {
                GUtilities.DestroyObject(colorByNormalMap);
            }
            if (colorBlendMap != null)
            {
                GUtilities.DestroyObject(colorBlendMap);
            }
        }

        public float[,,] GetAlphamaps()
        {
            int res = SplatControlResolution;
            int layerCount = Splats.Prototypes.Count;
            float[,,] alphamaps = new float[res, res, layerCount];
            int controlMapCount = SplatControlMapCount;
            Color color;
            int layer;

            for (int cIndex = 0; cIndex < controlMapCount; ++cIndex)
            {
                Texture2D tex = GetSplatControl(cIndex);
                for (int y = 0; y < res; ++y)
                {
                    for (int x = 0; x < res; ++x)
                    {
                        color = tex.GetPixel(x, y);

                        layer = cIndex * 4 + 0;
                        if (layer < layerCount)
                        {
                            alphamaps[x, y, layer] = color.r;
                        }

                        layer = cIndex * 4 + 1;
                        if (layer < layerCount)
                        {
                            alphamaps[x, y, layer] = color.g;
                        }

                        layer = cIndex * 4 + 2;
                        if (layer < layerCount)
                        {
                            alphamaps[x, y, layer] = color.b;
                        }

                        layer = cIndex * 4 + 3;
                        if (layer < layerCount)
                        {
                            alphamaps[x, y, layer] = color.a;
                        }
                    }
                }
            }

            return alphamaps;
        }

        public void SetAlphamaps(float[,,] alphamaps)
        {
            int res = SplatControlResolution;
            int layerCount = Splats.Prototypes.Count;
            int controlMapCount = SplatControlMapCount;
            Color color;
            int layer;

            for (int cIndex = 0; cIndex < controlMapCount; ++cIndex)
            {
                Texture2D tex = GetSplatControl(cIndex);
                for (int y = 0; y < res; ++y)
                {
                    for (int x = 0; x < res; ++x)
                    {
                        color = Color.clear;

                        layer = cIndex * 4 + 0;
                        if (layer < layerCount)
                        {
                            color.r = alphamaps[y, x, layer];
                        }

                        layer = cIndex * 4 + 1;
                        if (layer < layerCount)
                        {
                            color.g = alphamaps[y, x, layer];
                        }

                        layer = cIndex * 4 + 2;
                        if (layer < layerCount)
                        {
                            color.b = alphamaps[y, x, layer];
                        }

                        layer = cIndex * 4 + 3;
                        if (layer < layerCount)
                        {
                            color.a = alphamaps[y, x, layer];
                        }

                        tex.SetPixel(x, y, color);
                    }
                }
                tex.Apply();
            }
        }
    }
}
#endif
