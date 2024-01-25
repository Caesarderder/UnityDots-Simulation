#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.StampTool
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class GTextureStamper : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool editor_ShowLivePreview = true;
        public bool Editor_ShowLivePreview
        {
            get
            {
                return editor_ShowLivePreview;
            }
            set
            {
                editor_ShowLivePreview = value;
            }
        }

        [SerializeField]
        private bool editor_ShowBounds = true;
        public bool Editor_ShowBounds
        {
            get
            {
                return editor_ShowBounds;
            }
            set
            {
                editor_ShowBounds = value;
            }
        }
#endif

        [SerializeField]
        private bool enableTerrainMask;
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        private int groupId;
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                transform.position = value;
            }
        }

        [SerializeField]
        private Quaternion rotation = Quaternion.identity;
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                transform.rotation = value;
            }
        }

        [SerializeField]
        private Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                transform.localScale = value;
            }
        }

        [SerializeField]
        private Texture2D mask;
        public Texture2D Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
            }
        }

        [SerializeField]
        private AnimationCurve falloff;
        public AnimationCurve Falloff
        {
            get
            {
                return falloff;
            }
            set
            {
                falloff = value;
            }
        }

        [SerializeField]
        private GTextureStampChannel channel;
        public GTextureStampChannel Channel
        {
            get
            {
                return channel;
            }
            set
            {
                channel = value;
            }
        }

        [SerializeField]
        private List<GTextureStampLayer> layers;
        public List<GTextureStampLayer> Layers
        {
            get
            {
                if (layers == null)
                {
                    layers = new List<GTextureStampLayer>();
                }
                return layers;
            }
            set
            {
                layers = value;
            }
        }

        public Rect Rect
        {
            get
            {
                Vector3[] quad = new Vector3[4];
                GetQuad(quad);
                Rect r = GUtilities.GetRectContainsPoints(quad);
                return r;
            }
        }

        private Texture2D falloffTexture;

        private Vector3[] worldPoints = new Vector3[4];
        private Vector2[] uvPoints = new Vector2[4];

        private Dictionary<string, RenderTexture> tempRt;
        private Dictionary<string, RenderTexture> TempRt
        {
            get
            {
                if (tempRt == null)
                {
                    tempRt = new Dictionary<string, RenderTexture>();
                }
                return tempRt;
            }
        }

        private static Material albedoPainterMaterial;
        private static Material AlbedoPainterMaterial
        {
            get
            {
                if (albedoPainterMaterial == null)
                {
                    albedoPainterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.albedoPainterShader);
                }
                return albedoPainterMaterial;
            }
        }

        private static Material metallicPainterMaterial;
        private static Material MetallicPainterMaterial
        {
            get
            {
                if (metallicPainterMaterial == null)
                {
                    metallicPainterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.metallicPainterShader);
                }
                return metallicPainterMaterial;
            }
        }

        private static Material smoothnessPainterMaterial;
        private static Material SmoothnessPainterMaterial
        {
            get
            {
                if (smoothnessPainterMaterial == null)
                {
                    smoothnessPainterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.smoothnessPainterShader);
                }
                return smoothnessPainterMaterial;
            }
        }

        private static Material splatPainterMaterial;
        private static Material SplatPainterMaterial
        {
            get
            {
                if (splatPainterMaterial == null)
                {
                    splatPainterMaterial = new Material(GRuntimeSettings.Instance.internalShaders.splatPainterShader);
                }
                return splatPainterMaterial;
            }
        }


        private void Reset()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one * 100;
            mask = null;
            falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
            channel = GTextureStampChannel.AlbedoMetallicSmoothness;
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            foreach (RenderTexture rt in TempRt.Values)
            {
                if (rt != null)
                {
                    rt.Release();
                    GUtilities.DestroyObject(rt);
                }
            }
        }

        public void Apply()
        {
            if (falloffTexture != null)
                Object.DestroyImmediate(falloffTexture);
            Internal_UpdateFalloffTexture();
            Internal_UpdateLayerTransitionTextures();
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (groupId < 0 ||
                    (groupId >= 0 && groupId == t.GroupId))
                {
                    Apply(t);
                }
            }
        }

        private void Apply(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            if (Layers.Count == 0)
                return;
            if (Channel == GTextureStampChannel.AlbedoMetallicSmoothness)
            {
                ApplyAlbedoMetallicSmoothness(t);
            }
            else if (Channel == GTextureStampChannel.Splat)
            {
                ApplySplat(t);
            }

            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
        }

        private void ApplySplat(GStylizedTerrain t)
        {
            int splatControlResolution = t.TerrainData.Shading.SplatControlResolution;
            int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
            RenderTexture[] rtControls = new RenderTexture[controlMapCount];
            for (int i = 0; i < controlMapCount; ++i)
            {
                rtControls[i] = new RenderTexture(splatControlResolution, splatControlResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            }

            bool succeed = Internal_ApplySplat(t, rtControls);
            if (!succeed)
            {
                for (int i = 0; i < controlMapCount; ++i)
                {
                    rtControls[i].Release();
                    Object.DestroyImmediate(rtControls[i]);
                }
                return;
            }

            for (int i = 0; i < controlMapCount; ++i)
            {
                Texture2D splatControl = t.TerrainData.Shading.GetSplatControl(i);
                RenderTexture.active = rtControls[i];
                splatControl.ReadPixels(new Rect(0, 0, splatControlResolution, splatControlResolution), 0, 0);
                splatControl.Apply();
                RenderTexture.active = null;

                rtControls[i].Release();
                Object.DestroyImmediate(rtControls[i]);
            }
        }

        private RenderTexture[] RenderBrushes(GStylizedTerrain t, Vector2[] uvPoints, int brushResolution)
        {
            RenderTexture[] brushes = new RenderTexture[Layers.Count];
            for (int i = 0; i < Layers.Count; ++i)
            {
                brushes[i] = GetRenderTexture("brush" + i.ToString(), brushResolution);
                GStampLayerMaskRenderer.Render(
                    brushes[i],
                    Layers[i],
                    t,
                    Matrix4x4.TRS(Position, Rotation, Scale),
                    Mask,
                    falloffTexture,
                    uvPoints,
                    EnableTerrainMask);
            }

            return brushes;
        }

        public bool Internal_ApplySplat(GStylizedTerrain t, RenderTexture[] rtControls)
        {
            GetQuad(worldPoints);
            GetUvPoints(t, worldPoints, uvPoints);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvPoints);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return false;

            int brushResolution = t.TerrainData.Shading.SplatControlResolution;
            RenderTexture[] brushes = RenderBrushes(t, uvPoints, brushResolution);

            RenderTexture[] bg = new RenderTexture[rtControls.Length];
            for (int i = 0; i < bg.Length; ++i)
            {
                bg[i] = GetRenderTexture("bg" + i.ToString(), brushResolution);
                Texture2D splatControl = t.TerrainData.Shading.GetSplatControlOrDefault(i);
                GCommon.CopyToRT(splatControl, bg[i]);
                GCommon.CopyToRT(splatControl, rtControls[i]);
            }

            Material paintMaterial = SplatPainterMaterial;
            paintMaterial.SetFloat("_Opacity", 1);
            for (int i = 0; i < brushes.Length; ++i)
            {
                if (Layers[i].Ignore)
                    continue;
                paintMaterial.SetTexture("_Mask", brushes[i]);
                int controlMapCount = rtControls.Length;
                for (int controlIndex = 0; controlIndex < controlMapCount; ++controlIndex)
                {
                    GCommon.CopyToRT(bg[controlIndex], rtControls[controlIndex]);
                    paintMaterial.SetTexture("_MainTex", bg[controlIndex]);
                    if (Layers[i].SplatIndex / 4 == controlIndex)
                    {
                        paintMaterial.SetInt("_ChannelIndex", Layers[i].SplatIndex % 4);
                    }
                    else
                    {
                        paintMaterial.SetInt("_ChannelIndex", -1);
                    }
                    int pass = 0;
                    GCommon.DrawQuad(rtControls[controlIndex], GCommon.FullRectUvPoints, paintMaterial, pass);
                    GCommon.CopyToRT(rtControls[controlIndex], bg[controlIndex]);
                }
            }

            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            return true;
        }

        private void ApplyAlbedoMetallicSmoothness(GStylizedTerrain t)
        {
            int albedoMapResolution = t.TerrainData.Shading.AlbedoMapResolution;
            RenderTexture albedoRt = new RenderTexture(albedoMapResolution, albedoMapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            int metallicMapResolution = t.TerrainData.Shading.MetallicMapResolution;
            RenderTexture metallicRt = new RenderTexture(metallicMapResolution, metallicMapResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            bool succeed = Internal_ApplyAlbedoMetallicSmoothness(t, albedoRt, metallicRt);
            if (!succeed)
                return;

            RenderTexture.active = albedoRt;
            t.TerrainData.Shading.AlbedoMap.ReadPixels(new Rect(0, 0, albedoMapResolution, albedoMapResolution), 0, 0);
            t.TerrainData.Shading.AlbedoMap.Apply();
            RenderTexture.active = null;

            RenderTexture.active = metallicRt;
            t.TerrainData.Shading.MetallicMap.ReadPixels(new Rect(0, 0, metallicMapResolution, metallicMapResolution), 0, 0);
            t.TerrainData.Shading.MetallicMap.Apply();
            RenderTexture.active = null;

            albedoRt.Release();
            GUtilities.DestroyObject(albedoRt);

            metallicRt.Release();
            GUtilities.DestroyObject(metallicRt);
        }

        public bool Internal_ApplyAlbedoMetallicSmoothness(GStylizedTerrain t, RenderTexture albedoRt, RenderTexture metallicRt)
        {
            GetQuad(worldPoints);
            GetUvPoints(t, worldPoints, uvPoints);
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvPoints);
            if (!dirtyRect.Overlaps(new Rect(0, 0, 1, 1)))
                return false;

            int brushResolution = Mathf.Max(t.TerrainData.Shading.AlbedoMapResolution, t.TerrainData.Shading.MetallicMapResolution);
            RenderTexture[] brushes = RenderBrushes(t, uvPoints, brushResolution);

            RenderTexture bg0 = GetRenderTexture("bg0", brushResolution);
            Texture2D albedoMap = t.TerrainData.Shading.AlbedoMapOrDefault;
            GCommon.CopyToRT(albedoMap, bg0);
            GCommon.CopyToRT(albedoMap, albedoRt);

            Material mat = AlbedoPainterMaterial;
            mat.SetFloat("_Opacity", 1);
            for (int i = 0; i < brushes.Length; ++i)
            {
                if (Layers[i].Ignore)
                    continue;
                mat.SetTexture("_MainTex", bg0);
                mat.SetTexture("_Mask", brushes[i]);
                mat.SetColor("_Color", Layers[i].Color);
                int pass = 0;
                GCommon.DrawQuad(albedoRt, GCommon.FullRectUvPoints, mat, pass);
                GCommon.CopyToRT(albedoRt, bg0);
            }

            RenderTexture bg1 = GetRenderTexture("bg1", brushResolution);
            Texture2D metallicMap = t.TerrainData.Shading.MetallicMapOrDefault;
            GCommon.CopyToRT(metallicMap, bg1);
            GCommon.CopyToRT(metallicMap, metallicRt);

            mat = MetallicPainterMaterial;
            mat.SetFloat("_Opacity", 1);
            for (int i = 0; i < brushes.Length; ++i)
            {
                if (Layers[i].Ignore)
                    continue;
                mat.SetTexture("_MainTex", bg1);
                mat.SetTexture("_Mask", brushes[i]);
                mat.SetColor("_Color", Color.white * Layers[i].Metallic);
                int pass = 3; //fragSet
                GCommon.DrawQuad(metallicRt, GCommon.FullRectUvPoints, mat, pass);
                GCommon.CopyToRT(metallicRt, bg1);
            }

            mat = SmoothnessPainterMaterial;
            mat.SetFloat("_Opacity", 1);
            for (int i = 0; i < brushes.Length; ++i)
            {
                if (Layers[i].Ignore)
                    continue;
                mat.SetTexture("_MainTex", bg1);
                mat.SetTexture("_Mask", brushes[i]);
                mat.SetColor("_Color", Color.white * Layers[i].Smoothness);
                int pass = 3; //fragSet
                GCommon.DrawQuad(metallicRt, GCommon.FullRectUvPoints, mat, pass);
                GCommon.CopyToRT(metallicRt, bg1);
            }

            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
            return true;
        }

        public void Internal_UpdateFalloffTexture()
        {
            if (falloffTexture != null)
                GUtilities.DestroyObject(falloffTexture);
            falloffTexture = GCommon.CreateTextureFromCurve(Falloff, 256, 1);
        }

        public void Internal_UpdateLayerTransitionTextures()
        {
            for (int i = 0; i < Layers.Count; ++i)
            {
                Layers[i].UpdateCurveTextures();
            }
        }

        public Vector3[] GetQuad()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            Vector3[] quad = new Vector3[4]
            {
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f)),
                matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f)),
                matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f))
            };

            return quad;
        }

        public void GetQuad(Vector3[] quad)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            quad[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            quad[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            quad[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            quad[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
        }

        private void GetUvPoints(GStylizedTerrain t, Vector3[] worldPoint, Vector2[] uvPoint)
        {
            for (int i = 0; i < uvPoints.Length; ++i)
            {
                uvPoints[i] = t.WorldPointToUV(worldPoints[i]);
            }
        }

        public void GetBox(Vector3[] box)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Position, Rotation, Scale);
            box[0] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, -0.5f));
            box[1] = matrix.MultiplyPoint(new Vector3(-0.5f, 0, 0.5f));
            box[2] = matrix.MultiplyPoint(new Vector3(0.5f, 0, 0.5f));
            box[3] = matrix.MultiplyPoint(new Vector3(0.5f, 0, -0.5f));
            box[4] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, -0.5f));
            box[5] = matrix.MultiplyPoint(new Vector3(-0.5f, 1, 0.5f));
            box[6] = matrix.MultiplyPoint(new Vector3(0.5f, 1, 0.5f));
            box[7] = matrix.MultiplyPoint(new Vector3(0.5f, 1, -0.5f));
        }

        private RenderTexture GetRenderTexture(string key, int resolution)
        {
            if (!TempRt.ContainsKey(key) ||
                TempRt[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                TempRt[key] = rt;
            }
            else if (TempRt[key].width != resolution || TempRt[key].height != resolution)
            {
                TempRt[key].Release();
                Object.DestroyImmediate(TempRt[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                TempRt[key] = rt;
            }

            return TempRt[key];
        }
    }
}
#endif
