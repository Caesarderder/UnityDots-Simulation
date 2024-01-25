#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.BillboardTool
{
    public static class GBillboardCreator
    {
        private static MaterialPropertyBlock materialProperties;
        private static MaterialPropertyBlock MaterialProperties
        {
            get
            {
                if (materialProperties == null)
                    materialProperties = new MaterialPropertyBlock();
                return materialProperties;
            }
        }

        public static void PrepareRenderTexture(ref RenderTexture rt, GBillboardCreatorArgs args)
        {
            int width = args.Column * args.CellSize;
            int height = args.Row * args.CellSize;
            if (args.Mode == GBillboardRenderMode.Flipbook)
            {
                width = args.CellSize;
                height = args.CellSize;
            }

            int depth = 16;
            RenderTextureFormat format = args.Mode == GBillboardRenderMode.Normal ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32;
            if (rt == null ||
                rt.width != width ||
                rt.height != height ||
                rt.depth != depth ||
                rt.format != format)
            {
                if (rt != null)
                {
                    rt.Release();
                }
                rt = new RenderTexture(width, height, depth, format, RenderTextureReadWrite.Linear);
            }
        }

        public static void RenderPreview(RenderTexture rt, GBillboardCreatorArgs args)
        {
            if (args.Mode == GBillboardRenderMode.Atlas)
                RenderPreviewAtlas(rt, args);
            else if (args.Mode == GBillboardRenderMode.Normal)
                RenderPreviewNormal(rt, args);
            else if (args.Mode == GBillboardRenderMode.Flipbook)
                RenderPreviewFlipbook(rt, args);
        }

        private static void RenderPreviewAtlas(RenderTexture rt, GBillboardCreatorArgs args)
        {
            Clear(rt, Color.clear);

            if (args.AtlasMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Atlas;

            Vector2 viewPortSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 viewPortPosition = new Vector2(0, 0);
            RenderTexture tempRt = new RenderTexture(Mathf.RoundToInt(viewPortSize.x * rt.width), Mathf.RoundToInt(viewPortSize.y * rt.height), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = tempRt;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    Clear(tempRt, Color.clear);
                    g.transform.rotation = Quaternion.Euler(0, GUtilities.To1DIndex(x, y, args.Column) * angleStep - 90, 0);
                    cam.Render();

                    viewPortPosition = new Vector2(x * viewPortSize.x, y * viewPortSize.y);
                    GCommon.DrawTexture(rt, tempRt, new Rect(viewPortPosition, viewPortSize), GInternalMaterials.UnlitTransparentMaterial);
                }
            }

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
            tempRt.Release();
            GUtilities.DestroyObject(tempRt);
        }

        private static void RenderPreviewNormal(RenderTexture rt, GBillboardCreatorArgs args)
        {
            Clear(rt, new Color(0.5f, 0.5f, 1f, 1f));

            if (args.NormalMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Normal;

            Vector2 viewPortSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 viewPortPosition = new Vector2(0, 0);
            RenderTexture tempRt = new RenderTexture(Mathf.RoundToInt(viewPortSize.x * rt.width), Mathf.RoundToInt(viewPortSize.y * rt.height), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = tempRt;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    Clear(tempRt, new Color(0.5f, 0.5f, 1f, 1f));
                    g.transform.rotation = Quaternion.Euler(0, GUtilities.To1DIndex(x, y, args.Column) * angleStep, 0);
                    cam.Render();

                    viewPortPosition = new Vector2(x * viewPortSize.x, y * viewPortSize.y);
                    GCommon.DrawTexture(rt, tempRt, new Rect(viewPortPosition, viewPortSize), GInternalMaterials.UnlitTransparentMaterial);
                }
            }

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
            tempRt.Release();
            GUtilities.DestroyObject(tempRt);
        }

        private static void RenderPreviewFlipbook(RenderTexture rt, GBillboardCreatorArgs args)
        {
            Clear(rt, Color.clear);

            if (args.AtlasMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Flipbook;

            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = rt;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            g.transform.rotation = Quaternion.Euler(0, args.CellIndex * angleStep, 0);
            cam.rect = new Rect(0, 0, 1, 1);
            cam.Render();

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
        }

        private static Camera CreatePreviewCamera(GBillboardCreatorArgs args)
        {
            GameObject previewCam = new GameObject("~BillboardEditorCam");
            //previewCam.hideFlags = HideFlags.HideAndDontSave;
            previewCam.transform.position = -Vector3.one * 10000;
            previewCam.transform.rotation = Quaternion.identity;
            previewCam.transform.localScale = Vector3.one;

            Camera cam = previewCam.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = args.CameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = args.Mode == GBillboardRenderMode.Normal ? new Color(0.5f, 0.5f, 1f, 1f) : Color.clear;
            cam.depth = -1000;
            cam.aspect = 1;
            cam.farClipPlane = 2 * Mathf.Abs(args.CameraOffset.z);
            cam.enabled = false;
            return cam;
        }

        private static GameObject CreatePreviewGameObject(Transform cameraTransform, GBillboardCreatorArgs args)
        {
            if (args.Target == null)
            {
                return new GameObject("~EmptyBillboardCreatorTarget");
            }
            GameObject g = GameObject.Instantiate(args.Target) as GameObject;
            g.name = "~BillboardCreatorTarget";
            //g.hideFlags = HideFlags.HideAndDontSave;
            g.transform.position = cameraTransform.transform.TransformPoint(args.CameraOffset);
            g.transform.rotation = cameraTransform.rotation;
            g.transform.localScale = Vector3.one;

            Material baseMaterial = args.Mode == GBillboardRenderMode.Normal ? args.NormalMaterial : args.AtlasMaterial;
            MeshRenderer[] renderers = g.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                Material[] sharedMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < sharedMaterials.Length; ++j)
                {
                    //Material mat = Object.Instantiate<Material>(baseMaterial);
                    Material mat = new Material(baseMaterial.shader);
                    mat.SetColor(args.DesColorProps, Color.white);
                    mat.SetTexture(args.DesTextureProps, null);
                    //mat.CopyPropertiesFromMaterial(sharedMaterials[j]);
                    try
                    {
                        if (sharedMaterials[j].HasProperty(args.SrcColorProps))
                        {
                            Color color = sharedMaterials[j].GetColor(args.SrcColorProps);
                            color.a = 1;
                            if (mat.HasProperty(args.DesColorProps))
                            {
                                mat.SetColor(args.DesColorProps, color);
                            }
                        }
                        else
                        {
                            if (mat.HasProperty(args.DesColorProps))
                            {
                                mat.SetColor(args.DesColorProps, Color.white);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        if (sharedMaterials[j].HasProperty(args.SrcTextureProps))
                        {
                            Texture tex = sharedMaterials[j].GetTexture(args.SrcTextureProps);
                            if (mat.HasProperty(args.DesTextureProps))
                            {
                                mat.SetTexture(args.DesTextureProps, tex);
                            }
                        }
                    }
                    catch { }

                    sharedMaterials[j] = mat;
                }
                renderers[i].sharedMaterials = sharedMaterials;
            }

            return g;
        }

        private static void Clear(RenderTexture rt, Color backgroundColor)
        {
            RenderTexture.active = rt;
            //            bool isGammaSpace = true;
            //#if UNITY_EDITOR
            //            if (UnityEditor.PlayerSettings.colorSpace != ColorSpace.Gamma)
            //            {
            //                isGammaSpace = false;
            //            }
            //#endif

            //            GL.Clear(true, true, isGammaSpace ? backgroundColor : backgroundColor.linear);
            GL.Clear(true, true, backgroundColor);
            RenderTexture.active = null;
        }

        public static ushort[] Triangulate(Vector2[] vertices)
        {
            List<ushort> tris = new List<ushort>();
            for (ushort x = 0; x < vertices.Length; ++x)
            {
                for (ushort y = 0; y < vertices.Length; ++y)
                {
                    for (ushort z = 0; z < vertices.Length; ++z)
                    {
                        Vector2 v0 = vertices[x];
                        Vector2 v1 = vertices[y];
                        Vector2 v2 = vertices[z];

                        Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
                        if (cross.z < 0 && !IsTriangleAdded(tris, new ushort[] { x, y, z }))
                        {
                            tris.Add(x);
                            tris.Add(y);
                            tris.Add(z);
                        }
                    }
                }
            }

            List<ushort> result = new List<ushort>();
            int trisCount = tris.Count / 3;
            for (ushort i = 0; i < trisCount; ++i)
            {
                ushort t0 = tris[i * 3 + 0];
                ushort t1 = tris[i * 3 + 1];
                ushort t2 = tris[i * 3 + 2];

                bool isValidTriangle = true;
                for (ushort vIndex = 0; vIndex < vertices.Length; ++vIndex)
                {
                    if (vIndex == t0 || vIndex == t1 || vIndex == t2)
                        continue;
                    if (GUtilities.IsPointInCircumcircle(vertices[t0], vertices[t1], vertices[t2], vertices[vIndex]))
                    {
                        isValidTriangle = false;
                        break;
                    }
                }

                if (isValidTriangle)
                {
                    result.Add(t0);
                    result.Add(t1);
                    result.Add(t2);
                }
            }

            return result.ToArray();
        }

        private static bool IsTriangleAdded(List<ushort> tris, ushort[] newTris)
        {
            int trisCount = tris.Count / 3;
            for (ushort i = 0; i < trisCount; ++i)
            {
                ushort i0 = tris[i * 3 + 0];
                ushort i1 = tris[i * 3 + 1];
                ushort i2 = tris[i * 3 + 2];
                if (GUtilities.AreSetEqual(new ushort[] { i0, i1, i2 }, newTris))
                {
                    return true;
                }
            }

            return false;
        }

        public static GBillboardCreatorArgs FitCameraToTarget(GBillboardCreatorArgs args)
        {
            if (args.Target == null)
                return args;
            Renderer[] renderers = args.Target.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds b = new Bounds();
                b.SetMinMax(
                    renderers[0].bounds.min,
                    renderers[0].bounds.max);
                for (int i = 1; i < renderers.Length; ++i)
                {
                    Bounds bi = renderers[i].bounds;
                    b.Encapsulate(bi.min);
                    b.Encapsulate(bi.max);
                }
                b.Encapsulate(args.Target.transform.position);

                Vector3 center = args.Target.transform.position;
                float dWidth = 2 * Mathf.Max(
                    Vector3.Distance(center, new Vector3(b.min.x, center.y, b.min.z)),
                    Vector3.Distance(center, new Vector3(b.max.x, center.y, b.max.z)));
                float dHeight = b.size.y;
                float bottom = b.min.y - center.y;
                args.CameraSize = Mathf.Max(dWidth, dHeight) * 0.5f;
                args.CameraOffset = -(b.center - center) + Vector3.forward * dWidth * 2;

                args.Height = dHeight - bottom;
                args.Bottom = bottom;
                args.Width = dHeight;
            }
            return args;
        }

        public static BillboardAsset CreateBillboardAsset(GBillboardCreatorArgs args)
        {
            BillboardAsset billboard = new BillboardAsset();
            billboard.SetVertices(args.Vertices);
            billboard.SetIndices(Triangulate(args.Vertices));
            billboard.width = args.Width;
            billboard.height = args.Height;
            billboard.bottom = args.Bottom;

            Vector4[] texcoords = new Vector4[args.Row * args.Column];
            Vector2 imageSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 imageTopLeft = new Vector2(0, 0);

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    imageTopLeft = new Vector2(x * imageSize.x, y * imageSize.y);
                    texcoords[GUtilities.To1DIndex(x, y, args.Column)] = new Vector4(imageTopLeft.x, imageTopLeft.y, imageSize.x, imageSize.y);
                }
            }
            billboard.SetImageTexCoords(texcoords);
            billboard.name = args.Target.name + "_Billboard";
            return billboard;
        }

        public static Texture2D RenderAtlas(GBillboardCreatorArgs args)
        {
            args.Mode = GBillboardRenderMode.Atlas;
            RenderTexture rt = null;
            PrepareRenderTexture(ref rt, args);
            RenderPreviewAtlas(rt, args);
            Texture2D atlas = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true, true);
            GCommon.CopyFromRT(atlas, rt);
            rt.Release();
            GUtilities.DestroyObject(rt);
            atlas.name = args.Target.name + "_Atlas";
            return atlas;
        }

        public static Texture2D RenderNormal(GBillboardCreatorArgs args)
        {
            args.Mode = GBillboardRenderMode.Normal;
            RenderTexture rt = null;
            PrepareRenderTexture(ref rt, args);
            RenderPreviewNormal(rt, args);
            Texture2D atlas = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true, true);
            GCommon.CopyFromRT(atlas, rt);
            rt.Release();
            GUtilities.DestroyObject(rt);
            atlas.name = args.Target.name + "_Normal";
            return atlas;
        }
    }
}
#endif
