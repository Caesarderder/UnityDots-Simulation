#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering;
using DateTime = System.DateTime;
using Unity.Collections;
using Unity.Jobs;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Pinwheel.Griffin.Physic;

namespace Pinwheel.Griffin
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class GCommon
    {
        public const string PINWHEEL_STUDIO = "Pinwheel Studio";
        public const string SUPPORT_EMAIL = "support@pinwheel.studio";
        public const string BUSINESS_EMAIL = "hello@pinwheel.studio";
        public const string YOUTUBE_CHANNEL = "https://www.youtube.com/channel/UCebwuk5CfIe5kolBI9nuBTg";
        public const string ONLINE_MANUAL = "https://docs.google.com/document/d/1Mw51zhOBgosXf3I13qzbtIqjLNlvl2yo3B4bJ8bd6zU/edit?usp=sharing";
        public const string FACEBOOK_PAGE = "https://www.facebook.com/polaris.terrain";
        public const string FORUM = "https://forum.unity.com/threads/pre-release-polaris-hybrid-procedural-low-poly-terrain-engine.541792/#post-3572618";
        public const string DISCORD = "https://discord.gg/j9p5PMWPhk";

        public const int SUB_DIV_MAP_RESOLUTION = 512;
        public const string SUB_DIV_MAP_SHADER = "Hidden/Griffin/SubDivisionMap";
        public const float SUB_DIV_EPSILON = 0.005f;
        public const float SUB_DIV_PIXEL_OFFSET = 2;
        public const float SUB_DIV_STEP = 0.1f;

        public const string CHUNK_ROOT_NAME_OBSOLETED = "_Geometry";
        public const string CHUNK_ROOT_NAME = "~Geometry";
        public const int MAX_LOD_COUNT = 4;
        public const int MAX_MESH_BASE_RESOLUTION = 10;
        public const int MAX_MESH_RESOLUTION = 13;

        public const string CHUNK_MESH_NAME_PREFIX = "~Chunk";
        public const string GRASS_MESH_NAME_PREFIX = "~GrassPatch";

        public const string BRUSH_MASK_RESOURCES_PATH = "PolarisBrushes";

        public const int PREVIEW_TEXTURE_SIZE = 512;
        public const int TEXTURE_SIZE_MIN = 1;
        public const int TEXTURE_SIZE_MAX = 8192;

        public const float MAX_TREE_DISTANCE = 500;
        public const float MAX_GRASS_DISTANCE = 500;
        public const float MAX_COLLIDER_BUDGET = 1000;

        public const string FIRST_HISTORY_ENTRY_NAME = "Begin";

        private static int mainThreadId;
        public static int MainThreadId
        {
            get
            {
                return mainThreadId;
            }
        }

        static GCommon()
        {
            Init();
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsMainThread
        {
            get
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
            }
        }

        public static GRenderPipelineType CurrentRenderPipeline
        {
            get
            {
                RenderPipelineAsset rpAsset = GraphicsSettings.renderPipelineAsset;
                if (rpAsset == null)
                {
                    return GRenderPipelineType.Builtin;
                }
                else if (rpAsset.GetType().Name.Equals("UniversalRenderPipelineAsset"))
                {
                    return GRenderPipelineType.Universal;
                }
                else
                {
                    return GRenderPipelineType.Unsupported;
                }
            }
        }

        private static Vector2[] fullRectUvPoints;
        public static Vector2[] FullRectUvPoints
        {
            get
            {
                if (fullRectUvPoints == null)
                {
                    fullRectUvPoints = new Vector2[]
                    {
                        Vector2.zero,
                        Vector2.up,
                        Vector2.one,
                        Vector2.right
                    };
                }
                return fullRectUvPoints;
            }
        }

        private static List<GTerrainResourceFlag> emptyResourceFlags;
        public static List<GTerrainResourceFlag> EmptyResourceFlags
        {
            get
            {
                if (emptyResourceFlags == null)
                {
                    emptyResourceFlags = new List<GTerrainResourceFlag>();
                }
                return emptyResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> heightMapResourceFlags;
        public static List<GTerrainResourceFlag> HeightMapResourceFlags
        {
            get
            {
                if (heightMapResourceFlags == null)
                {
                    heightMapResourceFlags = new List<GTerrainResourceFlag>();
                    heightMapResourceFlags.Add(GTerrainResourceFlag.HeightMap);
                }
                return heightMapResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> heightMapAndFoliageResourceFlags;
        public static List<GTerrainResourceFlag> HeightMapAndFoliageResourceFlags
        {
            get
            {
                if (heightMapAndFoliageResourceFlags == null)
                {
                    heightMapAndFoliageResourceFlags = new List<GTerrainResourceFlag>();
                    heightMapAndFoliageResourceFlags.Add(GTerrainResourceFlag.HeightMap);
                    heightMapAndFoliageResourceFlags.Add(GTerrainResourceFlag.TreeInstances);
                    heightMapAndFoliageResourceFlags.Add(GTerrainResourceFlag.GrassInstances);
                }
                return heightMapAndFoliageResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> albedoResourceFlags;
        public static List<GTerrainResourceFlag> AlbedoResourceFlags
        {
            get
            {
                if (albedoResourceFlags == null)
                {
                    albedoResourceFlags = new List<GTerrainResourceFlag>();
                    albedoResourceFlags.Add(GTerrainResourceFlag.AlbedoMap);
                }
                return albedoResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> metallicResourceFlags;
        public static List<GTerrainResourceFlag> MetallicResourceFlags
        {
            get
            {
                if (metallicResourceFlags == null)
                {
                    metallicResourceFlags = new List<GTerrainResourceFlag>();
                    metallicResourceFlags.Add(GTerrainResourceFlag.MetallicMap);
                }
                return metallicResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> splatResourceFlags;
        public static List<GTerrainResourceFlag> SplatResourceFlags
        {
            get
            {
                if (splatResourceFlags == null)
                {
                    splatResourceFlags = new List<GTerrainResourceFlag>();
                    splatResourceFlags.Add(GTerrainResourceFlag.SplatControlMaps);
                }
                return splatResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> maskMapResourceFlags;
        public static List<GTerrainResourceFlag> MaskMapResourceFlags
        {
            get
            {
                if (maskMapResourceFlags == null)
                {
                    maskMapResourceFlags = new List<GTerrainResourceFlag>();
                    maskMapResourceFlags.Add(GTerrainResourceFlag.MaskMap);
                }
                return maskMapResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> treeInstancesResourceFlags;
        public static List<GTerrainResourceFlag> TreeInstancesResourceFlags
        {
            get
            {
                if (treeInstancesResourceFlags == null)
                {
                    treeInstancesResourceFlags = new List<GTerrainResourceFlag>();
                    treeInstancesResourceFlags.Add(GTerrainResourceFlag.TreeInstances);
                }
                return treeInstancesResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> grassInstancesResourceFlags;
        public static List<GTerrainResourceFlag> GrassInstancesResourceFlags
        {
            get
            {
                if (grassInstancesResourceFlags == null)
                {
                    grassInstancesResourceFlags = new List<GTerrainResourceFlag>();
                    grassInstancesResourceFlags.Add(GTerrainResourceFlag.GrassInstances);
                }
                return grassInstancesResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> foliageInstancesResourceFlags;
        public static List<GTerrainResourceFlag> FoliageInstancesResourceFlags
        {
            get
            {
                if (foliageInstancesResourceFlags == null)
                {
                    foliageInstancesResourceFlags = new List<GTerrainResourceFlag>();
                    foliageInstancesResourceFlags.Add(GTerrainResourceFlag.TreeInstances);
                    foliageInstancesResourceFlags.Add(GTerrainResourceFlag.GrassInstances);
                }
                return foliageInstancesResourceFlags;
            }
        }

        private static List<GTerrainResourceFlag> allResourceFlags;
        public static List<GTerrainResourceFlag> AllResourceFlags
        {
            get
            {
                if (allResourceFlags == null)
                {
                    allResourceFlags = new List<GTerrainResourceFlag>();
                    foreach (GTerrainResourceFlag t in System.Enum.GetValues(typeof(GTerrainResourceFlag)))
                    {
                        allResourceFlags.Add(t);
                    }
                }
                return allResourceFlags;
            }
        }

        public static Rect UnitRect
        {
            get
            {
                return new Rect(0, 0, 1, 1);
            }
        }

        public static string GetUniqueID()
        {
            string s = GetTimeTick().ToString();
            return Reverse(s);
        }

        public static long GetTimeTick()
        {
            DateTime time = DateTime.Now;
            return time.Ticks;
        }

        public static string Reverse(string s)
        {
            char[] chars = s.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }

        public static void SetDirty(Object o)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(o);
#endif
        }

        public static void TryAddObjectToAsset(Object objectToAdd, Object asset)
        {
#if UNITY_EDITOR
            if (!IsMainThread)
                return;
            if (!Application.isPlaying)
            {
                if (!AssetDatabase.Contains(objectToAdd) && EditorUtility.IsPersistent(asset))
                {
                    AssetDatabase.AddObjectToAsset(objectToAdd, asset);
                }
            }
#endif
        }

        public static Texture2D CreateTexture(int resolution, Color fill, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            Texture2D t = new Texture2D(resolution, resolution, format, false, linear);
            Color[] colors = new Color[resolution * resolution];
            GUtilities.Fill(colors, fill);
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Texture2D CreateTexture(int width, int height, Color fill, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            Texture2D t = new Texture2D(width, height, format, false, linear);
            Color[] colors = new Color[width * height];
            GUtilities.Fill(colors, fill);
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static void ReadPixelsFullSize(RenderTexture rt, Texture2D des, int startX, int startY)
        {
            int desX = Mathf.Max(0, startX);
            int desY = Mathf.Max(0, startY);
            int srcX = desX - startX;
            int srcY = desY - startY;
            int width = Mathf.Min(rt.width - srcX, des.width - desX);
            int height = Mathf.Min(rt.height - srcY, des.height - desY);

            RenderTexture tmp = CopyToRT(rt, srcX, srcY, width, height, Color.red);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = tmp;
            des.ReadPixels(new Rect(0, 0, width, height), desX, desY);
            RenderTexture.active = active;
            tmp.Release();
            Object.DestroyImmediate(tmp);
        }

        public static Color[] GetPixels(Texture2D src, int startX, int startY, int width, int height, Color defaultColor)
        {
            int endX = startX + width - 1;
            int endY = startY + height - 1;
            Vector2 startUV = new Vector2(
                GUtilities.InverseLerpUnclamped(0, src.width - 1, startX),
                GUtilities.InverseLerpUnclamped(0, src.height - 1, startY));
            Vector2 endUV = new Vector2(
                GUtilities.InverseLerpUnclamped(0, src.width - 1, endX),
                GUtilities.InverseLerpUnclamped(0, src.height - 1, endY));
            Material mat = GInternalMaterials.CopyTextureMaterial;
            mat.SetTexture("_MainTex", src);
            mat.SetVector("_StartUV", startUV);
            mat.SetVector("_EndUV", endUV);
            mat.SetColor("_DefaultColor", defaultColor);
            mat.SetPass(0);
            RenderTexture rt = new RenderTexture(width, height, 32);
            RenderTexture.active = rt;
            Graphics.Blit(src, mat);

            Texture2D tex = new Texture2D(width, height);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            Color[] colors = tex.GetPixels();

            RenderTexture.active = null;
            rt.Release();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);

            return colors;
        }

        public static Color[,] GetPixels2D(
            Texture2D src,
            int startX, int startZ,
            int sampleWidth, int sampleLength,
            Color defaultColor)
        {
            Color[,] sample = new Color[sampleLength, sampleWidth];

            Color[] colors = GCommon.GetPixels(src, startX, startZ, sampleWidth, sampleLength, defaultColor);
            for (int z = 0; z < sampleLength; ++z)
            {
                for (int x = 0; x < sampleWidth; ++x)
                {
                    sample[z, x] = colors[GUtilities.To1DIndex(x, z, sampleWidth)];
                }
            }

            return sample;
        }

        public static float[,] GetPixels2D(
            Texture2D src,
            int startX, int startZ,
            int sampleWidth, int sampleLength,
            Color defaultColor,
            System.Func<Color, float> converter)
        {
            float[,] sample = new float[sampleLength, sampleWidth];

            Color[] colors = GCommon.GetPixels(src, startX, startZ, sampleWidth, sampleLength, defaultColor);
            for (int z = 0; z < sampleLength; ++z)
            {
                for (int x = 0; x < sampleWidth; ++x)
                {
                    sample[z, x] = converter(colors[GUtilities.To1DIndex(x, z, sampleWidth)]);
                }
            }

            return sample;
        }

        public static void GetPixels2DNonAlloc(
            Texture2D src,
            int startX, int startZ,
            float[,] sample,
            Color defaultColor,
            System.Func<Color, float> converter)
        {
            int sampleLength = sample.GetLength(0);
            int sampleWidth = sample.GetLength(1);
            Color[] colors = GCommon.GetPixels(src, startX, startZ, sampleWidth, sampleLength, defaultColor);
            for (int z = 0; z < sampleLength; ++z)
            {
                for (int x = 0; x < sampleWidth; ++x)
                {
                    sample[z, x] = converter(colors[GUtilities.To1DIndex(x, z, sampleWidth)]);
                }
            }
        }

        public static void GetPixels2DNonAlloc(
            Texture2D src,
            int startX, int startZ,
            Color[,] sample,
            Color defaultColor)
        {
            int sampleLength = sample.GetLength(0);
            int sampleWidth = sample.GetLength(1);
            Color[] colors = GCommon.GetPixels(src, startX, startZ, sampleWidth, sampleLength, defaultColor);
            for (int z = 0; z < sampleLength; ++z)
            {
                for (int x = 0; x < sampleWidth; ++x)
                {
                    sample[z, x] = colors[GUtilities.To1DIndex(x, z, sampleWidth)];
                }
            }
        }

        public static void SetPixels2D(Texture2D des, int startX, int startY, Color[,] sample)
        {
            SetPixels2D(des, startX, startY, sample, null);
        }

        public static void SetPixels2D(Texture2D des, int startX, int startY, Color[,] sample, Material mat)
        {
            int height = sample.GetLength(0);
            int width = sample.GetLength(1);

            Rect rect = new Rect();
            rect.position = new Vector2(
                startX < 0 ? -startX : 0,
                startY < 0 ? -startY : 0);
            rect.size = new Vector2(
                startX < 0 ? width + startX : width,
                startY < 0 ? height + startY : height);
            if (rect.size.x <= 0 || rect.size.y <= 0)
                return;

            int x = Mathf.Max(0, startX);
            int y = Mathf.Max(0, startY);
            if (x >= des.width || y >= des.height)
                return;

            Color[] colors = GUtilities.To1dArray(sample);
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(colors);
            tex.Apply();

            RenderTexture rt = new RenderTexture(width, height, 32);
            if (mat != null)
                Graphics.Blit(tex, rt, mat);
            else
                Graphics.Blit(tex, rt);
            des.ReadPixels(rect, x, y);

            RenderTexture.active = null;
            rt.Release();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
        }

        public static void SetPixels2D(Texture2D des, int startX, int startZ, float[,] sample, System.Func<float, Color> converter)
        {
            SetPixels2D(des, startX, startZ, sample, converter, null);
        }

        public static void SetPixels2D(Texture2D des, int startX, int startZ, float[,] sample, System.Func<float, Color> converter, Material mat)
        {
            int sampleLength = sample.GetLength(0);
            int sampleWidth = sample.GetLength(1);
            Color[,] colors = new Color[sampleLength, sampleWidth];
            float value;
            for (int z = 0; z < sampleLength; ++z)
            {
                for (int x = 0; x < sampleWidth; ++x)
                {
                    value = sample[z, x];
                    colors[z, x] = converter(value);
                }
            }
            SetPixels2D(des, startX, startZ, colors, mat);
        }

        public static void CopyToRT(Texture t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            Graphics.Blit(t, rt);
            RenderTexture.active = null;
        }

        public static void CopyFromRT(Texture2D t, RenderTexture rt)
        {
            RenderTexture.active = rt;
            t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            t.Apply();
            RenderTexture.active = null;
        }

        public static void CopyTexture(Texture2D src, Texture2D des)
        {
            RenderTexture rt = new RenderTexture(des.width, des.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            CopyToRT(src, rt);
            CopyFromRT(des, rt);
            rt.Release();
            GUtilities.DestroyObject(rt);
        }

        public static Texture2D CloneTexture(Texture2D t)
        {
            RenderTexture rt = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            CopyToRT(t, rt);
            Texture2D result = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false, true);
            result.filterMode = t.filterMode;
            result.wrapMode = t.wrapMode;
            CopyFromRT(result, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
            return result;
        }

        public static void FillTexture(Texture2D t, Color c)
        {
            Color[] colors = new Color[t.width * t.height];
            GUtilities.Fill(colors, c);
            t.SetPixels(colors);
            t.Apply();
        }

        public static void FillTexture(RenderTexture rt, Color c)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            CopyToRT(tex, rt);
            GUtilities.DestroyObject(tex);
        }

        public static Texture2D CloneAndResizeTexture(Texture2D t, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            CopyToRT(t, rt);
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.filterMode = t.filterMode;
            result.wrapMode = t.wrapMode;
            CopyFromRT(result, rt);
            rt.Release();
            Object.DestroyImmediate(rt);
            return result;
        }

        public static RenderTexture CopyToRT(Texture src, int startX, int startY, int width, int height, Color defaultColor)
        {
            int endX = startX + width - 1;
            int endY = startY + height - 1;
            Vector2 startUV = new Vector2(
                GUtilities.InverseLerpUnclamped(0, src.width - 1, startX),
                GUtilities.InverseLerpUnclamped(0, src.height - 1, startY));
            Vector2 endUV = new Vector2(
                GUtilities.InverseLerpUnclamped(0, src.width - 1, endX),
                GUtilities.InverseLerpUnclamped(0, src.height - 1, endY));
            Material mat = GInternalMaterials.CopyTextureMaterial;
            mat.SetTexture("_MainTex", src);
            mat.SetVector("_StartUV", startUV);
            mat.SetVector("_EndUV", endUV);
            mat.SetColor("_DefaultColor", defaultColor);
            mat.SetPass(0);
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture.active = rt;
            Graphics.Blit(src, mat);
            RenderTexture.active = null;

            return rt;
        }

        public static void DrawTexture(RenderTexture rt, Texture texture, Rect uvRect, Material mat, int pass = 0)
        {
            if (mat == null)
                mat = GInternalMaterials.UnlitTextureMaterial;
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetTexture("_MainTex", texture);
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(uvRect.min.x, uvRect.min.y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(uvRect.min.x, uvRect.max.y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(uvRect.max.x, uvRect.max.y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(uvRect.max.x, uvRect.min.y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawTriangle(RenderTexture rt, Vector2 v0, Vector2 v1, Vector2 v2, Color c)
        {
            Material mat = GInternalMaterials.SolidColorMaterial;
            mat.SetColor("_Color", c);
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);
            GL.Vertex3(v0.x, v0.y, 0);
            GL.Vertex3(v1.x, v1.y, 0);
            GL.Vertex3(v2.x, v2.y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static void DrawQuad(RenderTexture rt, Vector2[] quadCorners, Material mat, int pass)
        {
            RenderTexture.active = rt;
            GL.PushMatrix();
            mat.SetPass(pass);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0);
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0);
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0);
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0);
            GL.End();
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public static List<System.Type> GetAllLoadedTypes()
        {
            List<System.Type> loadedTypes = new List<System.Type>();
            List<string> typeName = new List<string>();
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (t.IsVisible && !t.IsGenericType)
                    {
                        typeName.Add(t.Name);
                        loadedTypes.Add(t);
                    }
                }
            }
            return loadedTypes;
        }

        public static IEnumerable<Rect> CompareTerrainTexture(int gridSize, Color[] oldValues, Color[] newValues)
        {
            if (oldValues.LongLength != newValues.LongLength)
            {
                return new Rect[1] { new Rect(0, 0, 1, 1) };
            }
            Rect[] rects = new Rect[gridSize * gridSize];
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    rects[GUtilities.To1DIndex(x, z, gridSize)] = GetUvRange(gridSize, x, z);
                }
            }

            HashSet<Rect> dirtyRects = new HashSet<Rect>();

            int index = 0;
            int resolution = Mathf.RoundToInt(Mathf.Sqrt(newValues.LongLength));
            for (int rectIndex = 0; rectIndex < rects.Length; ++rectIndex)
            {
                Rect r = rects[rectIndex];
                int startX = (int)Mathf.Lerp(0, resolution - 1, r.min.x);
                int startY = (int)Mathf.Lerp(0, resolution - 1, r.min.y);
                int endX = (int)Mathf.Lerp(0, resolution - 1, r.max.x);
                int endY = (int)Mathf.Lerp(0, resolution - 1, r.max.y);
                for (int x = startX; x <= endX; ++x)
                {
                    for (int y = startY; y <= endY; ++y)
                    {
                        index = GUtilities.To1DIndex(x, y, resolution);
                        if (oldValues[index].r == newValues[index].r &&
                            oldValues[index].g == newValues[index].g &&
                            oldValues[index].b == newValues[index].b &&
                            oldValues[index].a == newValues[index].a)
                            continue;
                        dirtyRects.Add(r);

                        Rect hRect = new Rect();
                        hRect.size = new Vector2(r.width * 1.2f, r.height);
                        hRect.center = r.center;
                        dirtyRects.Add(hRect);

                        Rect vRect = new Rect();
                        vRect.size = new Vector2(r.width, r.height * 1.2f);
                        vRect.center = r.center;
                        dirtyRects.Add(vRect);
                        break;
                    }
                    if (dirtyRects.Contains(r))
                        break;
                }
            }

            return dirtyRects;
        }

        public static IEnumerable<Rect> CompareTerrainTexture(int gridSize, Color32[] oldValues, Color32[] newValues)
        {
            if (oldValues.LongLength != newValues.LongLength)
            {
                return new Rect[1] { new Rect(0, 0, 1, 1) };
            }
            Rect[] rects = new Rect[gridSize * gridSize];
            for (int x = 0; x < gridSize; ++x)
            {
                for (int z = 0; z < gridSize; ++z)
                {
                    rects[GUtilities.To1DIndex(x, z, gridSize)] = GetUvRange(gridSize, x, z);
                }
            }

            HashSet<Rect> dirtyRects = new HashSet<Rect>();

            int index = 0;
            int resolution = Mathf.RoundToInt(Mathf.Sqrt(newValues.LongLength));
            for (int rectIndex = 0; rectIndex < rects.Length; ++rectIndex)
            {
                Rect r = rects[rectIndex];
                int startX = (int)Mathf.Lerp(0, resolution - 1, r.min.x);
                int startY = (int)Mathf.Lerp(0, resolution - 1, r.min.y);
                int endX = (int)Mathf.Lerp(0, resolution - 1, r.max.x);
                int endY = (int)Mathf.Lerp(0, resolution - 1, r.max.y);
                for (int x = startX; x <= endX; ++x)
                {
                    for (int y = startY; y <= endY; ++y)
                    {
                        index = GUtilities.To1DIndex(x, y, resolution);
                        if (oldValues[index].Equals(newValues[index]))
                            continue;
                        dirtyRects.Add(r);

                        Rect hRect = new Rect();
                        hRect.size = new Vector2(r.width * 1.2f, r.height);
                        hRect.center = r.center;
                        dirtyRects.Add(hRect);

                        Rect vRect = new Rect();
                        vRect.size = new Vector2(r.width, r.height * 1.2f);
                        vRect.center = r.center;
                        dirtyRects.Add(vRect);
                        break;
                    }
                    if (dirtyRects.Contains(r))
                        break;
                }
            }

            return dirtyRects;
        }

        public static Rect GetUvRange(int gridSize, int x, int z)
        {
            Vector2 position = new Vector2(x * 1.0f / gridSize, z * 1.0f / gridSize);
            Vector2 size = Vector2.one / gridSize;
            return new Rect(position, size);
        }

        public static Texture2D CreateTextureFromCurve(AnimationCurve curve, int width, int height)
        {
            Texture2D t = GCommon.CreateTexture(width, height, Color.black);
            t.wrapMode = TextureWrapMode.Clamp;
            Color[] colors = new Color[width * height];
            for (int x = 0; x < width; ++x)
            {
                float f = Mathf.InverseLerp(0, width - 1, x);
                float value = curve.Evaluate(f);
                Color c = new Color(value, value, value, value);
                for (int y = 0; y < height; ++y)
                {
                    colors[GUtilities.To1DIndex(x, y, width)] = c;
                }
            }
            t.filterMode = FilterMode.Bilinear;
            t.SetPixels(colors);
            t.Apply();
            return t;
        }

        public static Vector3[] GetBrushQuadCorners(Vector3 center, float radius, float rotation)
        {
            Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(0, rotation, 0));
            Vector3[] corners = new Vector3[]
            {
                center + matrix.MultiplyPoint(new Vector3(-1,0,-1)*radius),
                center + matrix.MultiplyPoint(new Vector3(-1,0,1)*radius),
                center + matrix.MultiplyPoint(new Vector3(1,0,1)*radius),
                center + matrix.MultiplyPoint(new Vector3(1,0,-1)*radius)
            };
            return corners;
        }

        public static GTerrainGeneratedData GetTerrainGeneratedDataAsset(GTerrainData terrainData, string namePrefix = "Generated")
        {
            GTerrainGeneratedData generatedData = null;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string path = AssetDatabase.GetAssetPath(terrainData);
                string directory = Path.GetDirectoryName(path);
                string filePath = Path.Combine(directory, string.Format("{0}_{1}.asset", namePrefix, terrainData.Id));
                generatedData = AssetDatabase.LoadAssetAtPath<GTerrainGeneratedData>(filePath);
                if (generatedData == null)
                {
                    generatedData = ScriptableObject.CreateInstance<GTerrainGeneratedData>();
                    AssetDatabase.CreateAsset(generatedData, filePath);
                }
            }
            else
            {
                generatedData = ScriptableObject.CreateInstance<GTerrainGeneratedData>();
            }
#else
            generatedData = ScriptableObject.CreateInstance<GTerrainGeneratedData>();
#endif
            return generatedData;
        }

        public static void RegisterBeginRender(Camera.CameraCallback callback)
        {
            Camera.onPreCull += callback;
        }

        public static void UnregisterBeginRender(Camera.CameraCallback callback)
        {
            Camera.onPreCull -= callback;
        }

        public static void RegisterEndRender(Camera.CameraCallback callback)
        {
            Camera.onPostRender += callback;
        }

        public static void UnregisterEndRender(Camera.CameraCallback callback)
        {
            Camera.onPostRender -= callback;
        }

        public static void RegisterBeginRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.beginCameraRendering += callback;
        }

        public static void UnregisterBeginRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.beginCameraRendering -= callback;
        }

        public static void RegisterEndRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.endCameraRendering += callback;
        }

        public static void UnregisterEndRenderSRP(System.Action<ScriptableRenderContext, Camera> callback)
        {
            RenderPipelineManager.endCameraRendering -= callback;
        }

        public static void ClearRT(RenderTexture rt)
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
        }

        public static void SetMaterialKeywordActive(Material mat, string keyword, bool active)
        {
            if (active)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }

        public static GStylizedTerrain CreateTerrain(GTerrainData data)
        {
            GameObject g = new GameObject("Styllized Terrain");
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            GStylizedTerrain terrain = g.AddComponent<GStylizedTerrain>();
            terrain.GroupId = 0;
            terrain.TerrainData = data;
            terrain.Refresh();
            CreateTreeCollider(terrain);

            return terrain;
        }

        public static GTreeCollider CreateTreeCollider(GStylizedTerrain terrain)
        {
            GameObject colliderGO = new GameObject("Tree Collider");
            GUtilities.ResetTransform(colliderGO.transform, terrain.transform);
            colliderGO.transform.localPosition = Vector3.zero;
            colliderGO.transform.localRotation = Quaternion.identity;
            colliderGO.transform.localScale = Vector3.one;

            GTreeCollider collider = colliderGO.AddComponent<GTreeCollider>();
            collider.Terrain = terrain;

            return collider;
        }

        public static void UpdateMaterials(int groupId)
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.GroupId != groupId && groupId >= 0)
                    continue;
                if (terrains.Current.TerrainData == null)
                    continue;
                terrains.Current.TerrainData.Shading.UpdateMaterials();
            }
        }

        public static Bounds GetLevelBounds()
        {
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;

            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                Bounds b = t.Bounds;

                min.x = Mathf.Min(min.x, b.min.x);
                min.y = Mathf.Min(min.y, b.min.y);
                min.z = Mathf.Min(min.z, b.min.z);

                max.x = Mathf.Max(max.x, b.max.x);
                max.y = Mathf.Max(max.y, b.max.y);
                max.z = Mathf.Max(max.z, b.max.z);
            }

            Vector3 center = (min + max) * 0.5f;
            Vector3 size = max - min;
            Bounds bounds = new Bounds(center, size);
            return bounds;
        }

        public static void ForEachTerrain(int groupId, System.Action<GStylizedTerrain> action)
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.GroupId != groupId && groupId >= 0)
                    continue;
                action.Invoke(terrains.Current);
            }
        }

        public static float Frac(float v)
        {
            return v - Mathf.Floor(v);
        }

        public static Vector2 Frac(Vector2 v)
        {
            return new Vector2(Frac(v.x), Frac(v.y));
        }

        public static Vector2 EncodeTerrainHeight(float h)
        {
            h = Mathf.Clamp(h, 0f, 0.99999f);
            Vector2 kEncodeMul = new Vector2(1.0f, 255.0f);
            float kEncodeBit = 1.0f / 255.0f;
            Vector2 enc = kEncodeMul * h;
            enc = Frac(enc);
            enc.x -= enc.y * kEncodeBit;
            return enc;
        }

        public static float DecodeTerrainHeight(Vector2 enc)
        {
            Vector2 kDecodeDot = new Vector2(1.0f, 1f / 255.0f);
            return Vector2.Dot(enc, kDecodeDot);
        }

#if UNITY_EDITOR
        public static string CreateAssetAtSameDirectory(Object newAsset, Object existingAsset)
        {
            string path = AssetDatabase.GetAssetPath(existingAsset);
            string directory = Path.GetDirectoryName(path);
            string fileName = string.Format("{0}.asset", newAsset.name);
            string fullPath = Path.Combine(directory, fileName);
            AssetDatabase.CreateAsset(newAsset, fullPath);
            return fullPath;
        }
#endif

        public static List<GOverlapTestResult> OverlapTest(int groupId, Vector3[] corners)
        {
            List<GOverlapTestResult> results = new List<GOverlapTestResult>();

            Vector2 p0 = new Vector2(corners[0].x, corners[0].z);
            Vector2 p1 = new Vector2(corners[1].x, corners[1].z);
            Vector2 p2 = new Vector2(corners[2].x, corners[2].z);
            Vector2 p3 = new Vector2(corners[3].x, corners[3].z);
            GQuad2D quad = new GQuad2D(p0, p1, p2, p3);

            GCommon.ForEachTerrain(groupId, t =>
            {
                if (t.TerrainData == null)
                    return;
                GOverlapTestResult r = new GOverlapTestResult();
                r.Terrain = t;
                results.Add(r);
            });

            NativeArray<Rect> terrainRects = new NativeArray<Rect>(results.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < terrainRects.Length; ++i)
            {
                terrainRects[i] = results[i].Terrain.Rect;
            }

            NativeArray<bool> terrainTestResults = new NativeArray<bool>(results.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            {
                GQuadOverlapTestJob job = new GQuadOverlapTestJob()
                {
                    rectsToTest = terrainRects,
                    result = terrainTestResults,
                    quad = quad
                };

                JobHandle jHandle = job.Schedule(results.Count, 5);
                jHandle.Complete();
            }

            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                r.IsOverlapped = terrainTestResults[i];
                results[i] = r;
            }

            terrainRects.Dispose();
            terrainTestResults.Dispose();

            List<JobHandle> chunkTestHandles = new List<JobHandle>();
            List<NativeArray<Rect>> chunkRectsHandles = new List<NativeArray<Rect>>();
            List<NativeArray<bool>> chunkTestResultsHandles = new List<NativeArray<bool>>();
            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                if (!r.IsOverlapped)
                {
                    chunkTestHandles.Add(default);
                    chunkRectsHandles.Add(default);
                    chunkTestResultsHandles.Add(default);
                    continue;
                }

                NativeArray<Rect> chunkRects = r.Terrain.GetChunkRectsNA();
                NativeArray<bool> chunkTestResults = new NativeArray<bool>(chunkRects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                GQuadOverlapTestJob job = new GQuadOverlapTestJob()
                {
                    rectsToTest = chunkRects,
                    result = chunkTestResults,
                    quad = quad
                };

                JobHandle jHandle = job.Schedule(chunkRects.Length, 5);
                chunkTestHandles.Add(jHandle);
                chunkRectsHandles.Add(chunkRects);
                chunkTestResultsHandles.Add(chunkTestResults);
            }

            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                if (!r.IsOverlapped)
                {
                    continue;
                }

                chunkTestHandles[i].Complete();
                r.IsChunkOverlapped = chunkTestResultsHandles[i].ToArray();
                results[i] = r;

                chunkRectsHandles[i].Dispose();
                chunkTestResultsHandles[i].Dispose();
            }

            return results;
        }
    }
}
#endif
