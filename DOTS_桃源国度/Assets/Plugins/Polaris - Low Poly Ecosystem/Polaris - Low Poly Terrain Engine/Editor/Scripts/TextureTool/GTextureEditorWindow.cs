#if GRIFFIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pinwheel.Griffin.TextureTool
{
    public class GTextureEditorWindow : GTwoPaneWindowWindow
    {
        private readonly int[] textureResolutionValues = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private readonly string[] textureResolutionLabels = new string[] { "32", "64", "128", "256", "512", "1024", "2048*", "4096*" };

        private const int PREVIEW_RGB = -1;
        private const int PREVIEW_R = 0;
        private const int PREVIEW_G = 1;
        private const int PREVIEW_B = 2;
        private const int PREVIEW_A = 3;
        private int previewMode = -1;

        private const int FILTER_POINT = 0;
        private const int FILTER_BILINEAR = 1;
        private int filterMode = 1;

        private int previewPadding = 10;
        private bool lockLivePreviewTerrain = false;

        private MaterialPropertyBlock livePreviewMaterialProperties;

        private RenderTexture previewRt;
        private RenderTexture PreviewRt
        {
            get
            {
                int resolution = GTextureToolParams.Instance.General.Resolution;
                RenderTextureFormat format = GTextureToolParams.Instance.General.UseHighPrecisionTexture ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGB32;
                if (previewRt == null)
                {
                    previewRt = new RenderTexture(resolution, resolution, 32, format, RenderTextureReadWrite.Linear);
                }
                if (previewRt.width != resolution || previewRt.height != resolution || previewRt.format != format)
                {
                    previewRt.Release();
                    GUtilities.DestroyObject(previewRt);
                    previewRt = new RenderTexture(resolution, resolution, 32, format, RenderTextureReadWrite.Linear);
                }
                previewRt.wrapMode = TextureWrapMode.Clamp;
                previewRt.filterMode = (FilterMode)filterMode;

                return previewRt;
            }
        }

        private Material previewMat;
        private Material PreviewMat
        {
            get
            {
                if (previewMat == null)
                {
                    previewMat = new Material(GRuntimeSettings.Instance.internalShaders.unlitChannelMaskShader);
                }
                return previewMat;
            }
        }

        public static void ShowWindow()
        {
            GTextureEditorWindow window = GetWindow<GTextureEditorWindow>();
            window.titleContent = new GUIContent("Texture Creator");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        public void OnEnable()
        {
            GCommon.RegisterBeginRender(OnBeginCameraRender);
            GCommon.RegisterBeginRenderSRP(OnBeginCameraRenderSRP);

            GTerrainData.GlobalDirty += OnTerrainDataDirty;

            livePreviewMaterialProperties = new MaterialPropertyBlock();
        }

        public void OnDisable()
        {
            GCommon.UnregisterBeginRender(OnBeginCameraRender);
            GCommon.UnregisterBeginRenderSRP(OnBeginCameraRenderSRP);
            GTerrainData.GlobalDirty -= OnTerrainDataDirty;

            livePreviewMaterialProperties.Clear();
        }

        private void OnFocus()
        {
            RenderPreviewTexture();
        }

        private void OnDestroy()
        {
            if (previewRt != null)
            {
                previewRt.Release();
                GUtilities.DestroyObject(previewRt);
            }
            if (previewMat != null)
            {
                GUtilities.DestroyObject(previewMat);
            }
        }

        protected override void OnToolbarGUI(Rect r)
        {
            base.OnToolbarGUI(r);
            List<string> channelButtonLabels = new List<string>()
            {
                "RGB",
                "R",
                "G",
                "B",
                "A"
            };
            List<int> channelValue = new List<int>()
            {
                PREVIEW_RGB,
                PREVIEW_R,
                PREVIEW_G,
                PREVIEW_B,
                PREVIEW_A
            };

            List<Rect> channelButtonRects = EditorGUIUtility.GetFlowLayoutedRects(r, EditorStyles.toolbarButton, 0, 0, channelButtonLabels);

            for (int i = 0; i < channelButtonLabels.Count; ++i)
            {
                if (GUI.Button(channelButtonRects[i], channelButtonLabels[i], EditorStyles.toolbarButton))
                {
                    previewMode = channelValue[i];
                }
                if (previewMode == channelValue[i])
                {
                    Color highlightColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.1f) : new Color(1, 1, 1, 0.3f);
                    EditorGUI.DrawRect(channelButtonRects[i], highlightColor);
                }
            }

            List<string> filterButtonLabels = new List<string>()
            {
                "==============",
                "Point",
                "Bilinear"
            };
            List<int> filterValue = new List<int>()
            {
                0,
                FILTER_POINT,
                FILTER_BILINEAR
            };

            List<Rect> filterButtonRects = EditorGUIUtility.GetFlowLayoutedRects(r, EditorStyles.toolbarButton, 0, 0, filterButtonLabels);
            for (int i = 1; i < filterButtonLabels.Count; ++i)
            {
                if (GUI.Button(filterButtonRects[i], filterButtonLabels[i], EditorStyles.toolbarButton))
                {
                    filterMode = filterValue[i];
                }
                if (filterMode == filterValue[i])
                {
                    Color highlightColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.1f) : new Color(1, 1, 1, 0.3f);
                    EditorGUI.DrawRect(filterButtonRects[i], highlightColor);
                }
            }

            Rect saveButtonRect = new Rect(
                r.max.x - 100 - 2,
                r.min.y,
                100,
                r.height);
            if (GUI.Button(saveButtonRect, "Save", EditorStyles.toolbarButton))
            {
                SaveAsset();
            }
        }

        protected override void OnLeftPaneGUI(Rect r)
        {
            base.OnLeftPaneGUI(r);

            Rect previewRect = new Rect();
            int size = Mathf.FloorToInt(Mathf.Min(LeftPaneRect.width, LeftPaneRect.height) - 2 * previewPadding);
            previewRect.size = new Vector2(size, size);
            previewRect.center = LeftPaneRect.center;

            if (previewMode == PREVIEW_RGB)
            {
                EditorGUI.DrawPreviewTexture(previewRect, PreviewRt, null, ScaleMode.ScaleToFit);
            }
            else if (previewMode == PREVIEW_R)
            {
                PreviewMat.SetTexture("_MainTex", PreviewRt);
                PreviewMat.SetVector("_Mask", new Vector4(1, 0, 0, 0));
                EditorGUI.DrawPreviewTexture(previewRect, PreviewRt, PreviewMat, ScaleMode.ScaleToFit);
            }
            else if (previewMode == PREVIEW_G)
            {
                PreviewMat.SetTexture("_MainTex", PreviewRt);
                PreviewMat.SetVector("_Mask", new Vector4(0, 1, 0, 0));
                EditorGUI.DrawPreviewTexture(previewRect, PreviewRt, PreviewMat, ScaleMode.ScaleToFit);
            }
            else if (previewMode == PREVIEW_B)
            {
                PreviewMat.SetTexture("_MainTex", PreviewRt);
                PreviewMat.SetVector("_Mask", new Vector4(0, 0, 1, 0));
                EditorGUI.DrawPreviewTexture(previewRect, PreviewRt, PreviewMat, ScaleMode.ScaleToFit);
            }
            else if (previewMode == PREVIEW_A)
            {
                EditorGUI.DrawTextureAlpha(previewRect, PreviewRt, ScaleMode.ScaleToFit);
            }
        }

        protected override void OnRightPaneScrollViewGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnRightPaneScrollViewGUI();
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120;
            DrawGeneralParamsGUI();
            DrawSpecificParamsGUI();
            DrawFilters();
            DrawLivePreviewParamsGUI();
            EditorGUIUtility.labelWidth = labelWidth;

            if (EditorGUI.EndChangeCheck())
            {
                RenderPreviewTexture();
                MarkParamAssetDirty();
            }
        }

        private void MarkParamAssetDirty()
        {
            EditorUtility.SetDirty(GTextureToolParams.Instance);
        }

        private void DrawGeneralParamsGUI()
        {
            string id = "texture-editor-general";
            string label = "General Parameters";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                GGeneralParams param = GTextureToolParams.Instance.General;

                param.Mode = (GTextureGenerationMode)EditorGUILayout.EnumPopup("Mode", param.Mode);
                param.Resolution = EditorGUILayout.IntPopup("Resolution", param.Resolution, textureResolutionLabels, textureResolutionValues);
                param.Extension = (GImageFileExtension)EditorGUILayout.EnumPopup("Extension", param.Extension);
                param.UseHighPrecisionTexture = EditorGUILayout.Toggle("High Precision", param.UseHighPrecisionTexture);
                string dir = param.Directory;
                GEditorCommon.BrowseFolderMiniButton("Directory", ref dir);
                param.Directory = dir;

                GTextureToolParams.Instance.General = param;
            });
        }

        private void DrawSpecificParamsGUI()
        {
            string id = "texture-editor-specific";
            string label = "Specific Parameters";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                GTextureGenerationMode mode = GTextureToolParams.Instance.General.Mode;
                string methodName = "Draw" + mode.ToString() + "Params";
                MethodInfo guiMethod = typeof(GTextureEditorWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (guiMethod != null)
                {
                    guiMethod.Invoke(this, null);
                }
            });
        }

        private void DrawHeightMapParams()
        {
            GHeightMapGeneratorParams param = GTextureToolParams.Instance.HeightMap;

            param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            param.UseRealGeometry = EditorGUILayout.Toggle("Real Geometry", param.UseRealGeometry);

            lockLivePreviewTerrain = true;
            SetLivePreviewTerrain(param.Terrain);

            GTextureToolParams.Instance.HeightMap = param;
        }

        private void DrawHeightMapFromMeshParams()
        {
            GHeightMapFromMeshGeneratorParams param = GTextureToolParams.Instance.HeightMapFromMesh;

            bool isSrcMeshChanged = false;
            EditorGUI.BeginChangeCheck();
            param.SrcMesh = EditorGUILayout.ObjectField("Mesh", param.SrcMesh, typeof(Mesh), false) as Mesh;
            if (EditorGUI.EndChangeCheck())
            {
                isSrcMeshChanged = true;
            }

            GUI.enabled = param.SrcMesh != null;
            param.Offset = GEditorCommon.InlineVector3Field("Offset", param.Offset);
            param.Rotation = Quaternion.Euler(GEditorCommon.InlineVector3Field("Rotation", param.Rotation.eulerAngles));
            param.Scale = GEditorCommon.InlineVector3Field("Scale", param.Scale);
            param.ProjectionDepth = EditorGUILayout.FloatField("Depth", param.ProjectionDepth);
            EditorGUILayout.LabelField("Camera", "Top Down", GEditorCommon.WordWrapItalicLabel);

            if (GUILayout.Button("Fit Camera") || isSrcMeshChanged)
            {
                Bounds bounds = param.SrcMesh.bounds;
                param.Rotation = Quaternion.identity;
                param.Scale = Vector3.one * GHeightMapFromMeshGenerator.DEFAULT_CAMERA_ORTHO_SIZE * 2f / (Mathf.Max(bounds.size.x, bounds.size.z) + 0.0001f);
                Matrix4x4 scaleMatrix = Matrix4x4.Scale(param.Scale);
                param.Offset = scaleMatrix.MultiplyPoint(new Vector3(0, -bounds.size.y * 0.5f, 0));
                param.ProjectionDepth = scaleMatrix.MultiplyPoint(new Vector3(0, bounds.size.y, 0)).y;
                RenderPreviewTexture();
            }
            GUI.enabled = true;

            lockLivePreviewTerrain = false;

            GTextureToolParams.Instance.HeightMapFromMesh = param;
        }

        private void DrawNormalMapParams()
        {
            GNormalMapGeneratorParams param = GTextureToolParams.Instance.NormalMap;

            param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            param.Mode = (GNormalMapMode)EditorGUILayout.EnumPopup("Mode", param.Mode);
            param.Space = (GNormalMapSpace)EditorGUILayout.EnumPopup("Space", param.Space);

            lockLivePreviewTerrain = true;
            SetLivePreviewTerrain(param.Terrain);

            GTextureToolParams.Instance.NormalMap = param;
        }

        private void DrawSteepnessMapParams()
        {
            GSteepnessMapGeneratorParams param = GTextureToolParams.Instance.Steepness;

            param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            param.Mode = (GNormalMapMode)EditorGUILayout.EnumPopup("Mode", param.Mode);

            lockLivePreviewTerrain = true;
            SetLivePreviewTerrain(param.Terrain);

            GTextureToolParams.Instance.Steepness = param;
        }

        private void DrawNoiseMapParams()
        {
            GNoiseMapGeneratorParams param = GTextureToolParams.Instance.Noise;

            param.Type = (GNoiseType)EditorGUILayout.EnumPopup("Type", param.Type);
            param.Origin = GEditorCommon.InlineVector2Field("Origin", param.Origin);
            param.Frequency = EditorGUILayout.FloatField("Frequency", param.Frequency);
            param.Lacunarity = EditorGUILayout.FloatField("Lacunarity", param.Lacunarity);
            param.Persistence = EditorGUILayout.FloatField("Persistence", param.Persistence);
            param.Octaves = EditorGUILayout.IntField("Octaves", param.Octaves);
            param.Seed = EditorGUILayout.FloatField("Seed", param.Seed);

            lockLivePreviewTerrain = false;

            GTextureToolParams.Instance.Noise = param;
        }

        private void DrawColorMapParams()
        {
            GColorMapGeneratorParams param = GTextureToolParams.Instance.ColorMap;
            param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;

            lockLivePreviewTerrain = true;
            SetLivePreviewTerrain(param.Terrain);
            GTextureToolParams.Instance.ColorMap = param;
        }

        private void DrawBlendMapParams()
        {
            GBlendMapGeneratorParams param = GTextureToolParams.Instance.Blend;

            List<GBlendLayer> layers = param.Layers;
            for (int i = 0; i < layers.Count; ++i)
            {
                DrawBlendLayer(layers[i], i);
                GEditorCommon.Separator();
            }

            if (GUILayout.Button("Add Layer"))
            {
                layers.Add(GBlendLayer.Create());
            }

            GTextureToolParams.Instance.Blend = param;
        }

        private void DrawBlendLayer(GBlendLayer layer, int index)
        {
            string label = string.Format("{0} {1}", "Layer", index);
            string id = "texture-creator-blend-layer" + index;

            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            bool expanded = EditorPrefs.GetBool(prefKey, true);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect headerRect = EditorGUILayout.BeginHorizontal();
            expanded = EditorGUILayout.Foldout(expanded, label);
            EditorPrefs.SetBool(prefKey, expanded);
            GUILayout.FlexibleSpace();
            Rect deleteButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
            if (headerRect.Contains(Event.current.mousePosition))
            {
                if (GUI.Button(deleteButtonRect, "X", EditorStyles.label))
                {
                    ConfirmAndRemoveBlendLayerAt(index);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = indent;

            if (expanded)
            {
                //EditorGUI.indentLevel += 1;
                layer.DataSource = (GBlendDataSource)EditorGUILayout.EnumPopup("Source", layer.DataSource);
                if (layer.DataSource == GBlendDataSource.Texture)
                {
                    layer.Texture = GEditorCommon.InlineTexture2DField("Texture", layer.Texture, -1);
                }
                else if (layer.DataSource == GBlendDataSource.Number)
                {
                    layer.Number = EditorGUILayout.FloatField("Number", layer.Number);
                }
                else if (layer.DataSource == GBlendDataSource.Vector)
                {
                    layer.Vector = GEditorCommon.InlineVector4Field("Vector", layer.Vector);
                }

                if (index == 0)
                {
                    layer.BlendOps = GBlendOperation.Add;
                }
                GUI.enabled = index != 0;
                layer.BlendOps = (GBlendOperation)EditorGUILayout.EnumPopup("Operation", layer.BlendOps);
                GUI.enabled = true;
                if (layer.BlendOps == GBlendOperation.Lerp)
                {
                    layer.LerpFactor = EditorGUILayout.Slider("Factor", layer.LerpFactor, 0f, 1f);
                    layer.LerpMask = GEditorCommon.InlineTexture2DField("Mask", layer.LerpMask, -1);
                }

                layer.Saturate = EditorGUILayout.Toggle("Saturate", layer.Saturate);
                //EditorGUI.indentLevel -= 1;
            }
        }

        private void ConfirmAndRemoveBlendLayerAt(int index)
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove layer " + index.ToString() + "?",
                "OK", "Cancel"))
            {
                GTextureToolParams.Instance.Blend.Layers.RemoveAt(index);
                RenderPreviewTexture();
            }
        }

        private void DrawFoliageDistributionMapParams()
        {
            GFoliageDistributionMapGeneratorParams param = GTextureToolParams.Instance.TreeDistribution;

            param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            param.BrushMask = GEditorCommon.InlineTexture2DField("Brush", param.BrushMask, -1);
            param.Opacity = EditorGUILayout.Slider("Opacity", param.Opacity, 0f, 1f);
            param.Size = EditorGUILayout.Slider("Size", param.Size, 0f, 1f);
            param.RotationMin = EditorGUILayout.FloatField("Rotation Min", param.RotationMin);
            param.RotationMax = EditorGUILayout.FloatField("Rotation Max", param.RotationMax);
            EditorGUILayout.LabelField("Trees");
            if (param.Terrain == null ||
                param.Terrain.TerrainData == null ||
                param.Terrain.TerrainData.Foliage.Trees == null ||
                param.Terrain.TerrainData.Foliage.Trees.Prototypes.Count == 0)
            {
                EditorGUILayout.LabelField("No tree found!", GEditorCommon.WordWrapItalicLabel);
            }
            else
            {
                List<GTreePrototype> prototypes = param.Terrain.TerrainData.Foliage.Trees.Prototypes;
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = prototypes;
                args.selectedIndices = param.TreePrototypeIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
                args.itemPerRow = 4;
                args.drawPreviewFunction = GEditorCommon.DrawTreePreviewSingle;
                args.simpleMode = true;
                param.TreePrototypeIndices = GEditorCommon.MultiSelectionGrid(args);
            }

            EditorGUILayout.LabelField("Grasses");
            if (param.Terrain == null ||
                param.Terrain.TerrainData == null ||
                param.Terrain.TerrainData.Foliage.Grasses == null ||
                param.Terrain.TerrainData.Foliage.Grasses.Prototypes.Count == 0)
            {
                EditorGUILayout.LabelField("No grass found!", GEditorCommon.WordWrapItalicLabel);
            }
            else
            {
                List<GGrassPrototype> prototypes = param.Terrain.TerrainData.Foliage.Grasses.Prototypes;
                GSelectionGridArgs args = new GSelectionGridArgs();
                args.collection = prototypes;
                args.selectedIndices = param.GrassPrototypeIndices;
                args.itemSize = GEditorCommon.selectionGridTileSizeSmall;
                args.itemPerRow = 4;
                args.drawPreviewFunction = GEditorCommon.DrawGrassPreview;
                args.simpleMode = true;
                param.GrassPrototypeIndices = GEditorCommon.MultiSelectionGrid(args);
            }

            lockLivePreviewTerrain = true;
            SetLivePreviewTerrain(param.Terrain);

            GTextureToolParams.Instance.TreeDistribution = param;
        }

        private void DrawFilters()
        {
            string id = "texture-editor-filters";
            string label = "Filters";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                List<GTextureFilterLayer> layers = GTextureToolParams.Instance.Filters;

                for (int i = 0; i < layers.Count; ++i)
                {
                    DrawFilterLayer(layers[i], i);
                    GEditorCommon.Separator();
                }

                if (GUILayout.Button("Add Filter"))
                {
                    GenericMenu menu = new GenericMenu();
                    Array types = Enum.GetValues(typeof(GTextureFilterType));
                    foreach (GTextureFilterType t in types)
                    {
                        menu.AddItem(
                            new GUIContent(ObjectNames.NicifyVariableName(t.ToString())),
                            false,
                            () => { layers.Add(new GTextureFilterLayer(t)); RenderPreviewTexture(); });
                    }
                    menu.ShowAsContext();
                }
            });
        }

        private void DrawFilterLayer(GTextureFilterLayer layer, int index)
        {
            string label = string.Format("{0} {1}", ObjectNames.NicifyVariableName(layer.Type.ToString()), layer.Enabled ? "" : "[-]");
            string id = "texture-creator-filter" + index;

            string prefKey = GEditorCommon.GetProjectRelatedEditorPrefsKey("foldout", id);
            bool expanded = EditorPrefs.GetBool(prefKey, true);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect headerRect = EditorGUILayout.BeginHorizontal();
            expanded = EditorGUILayout.Foldout(expanded, label);
            EditorPrefs.SetBool(prefKey, expanded);
            GUILayout.FlexibleSpace();
            Rect deleteButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
            if (headerRect.Contains(Event.current.mousePosition))
            {
                if (GUI.Button(deleteButtonRect, "X", EditorStyles.label))
                {
                    ConfirmAndRemoveFilterAt(index);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (expanded)
            {
                EditorGUI.indentLevel += 1;
                layer.Enabled = EditorGUILayout.Toggle("Enable", layer.Enabled);
                GTextureFilterParams param = layer.Param;
                if (layer.Type == GTextureFilterType.Blur)
                {
                    GBlurParams blur = param.Blur;
                    blur.Radius = EditorGUILayout.DelayedIntField("Radius", blur.Radius);
                    EditorGUILayout.LabelField("This filter is expensive!", GEditorCommon.WordWrapItalicLabel);
                    param.Blur = blur;
                }
                else if (layer.Type == GTextureFilterType.Curve)
                {
                    GCurveParams curve = param.Curve;
                    curve.MasterCurve = EditorGUILayout.CurveField("Master", curve.MasterCurve, Color.white, GCommon.UnitRect);
                    curve.RedCurve = EditorGUILayout.CurveField("Red", curve.RedCurve, Color.red, GCommon.UnitRect);
                    curve.GreenCurve = EditorGUILayout.CurveField("Green", curve.GreenCurve, Color.green, GCommon.UnitRect);
                    curve.BlueCurve = EditorGUILayout.CurveField("Blue", curve.BlueCurve, Color.blue, GCommon.UnitRect);
                    curve.AlphaCurve = EditorGUILayout.CurveField("Alpha", curve.AlphaCurve, Color.gray, GCommon.UnitRect);
                    param.Curve = curve;
                }
                else if (layer.Type == GTextureFilterType.Invert)
                {
                    GInvertParams invert = param.Invert;
                    invert.InvertRed = EditorGUILayout.Toggle("Red", invert.InvertRed);
                    invert.InvertGreen = EditorGUILayout.Toggle("Green", invert.InvertGreen);
                    invert.InvertBlue = EditorGUILayout.Toggle("Blue", invert.InvertBlue);
                    invert.InvertAlpha = EditorGUILayout.Toggle("Alpha", invert.InvertAlpha);
                    param.Invert = invert;
                }
                else if (layer.Type == GTextureFilterType.Step)
                {
                    GStepParams step = param.Step;
                    step.Count = EditorGUILayout.IntSlider("Count", step.Count, 1, 256);
                    param.Step = step;
                }
                else if (layer.Type == GTextureFilterType.Warp)
                {
                    GWarpParams warp = param.Warp;
                    warp.MaskIsNormalMap = EditorGUILayout.Toggle("Is Normal Map", warp.MaskIsNormalMap);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Mask");
                    using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(-1))
                    {
                        warp.Mask = EditorGUILayout.ObjectField(warp.Mask, typeof(Texture2D), false) as Texture2D;
                    }
                    EditorGUILayout.EndHorizontal();
                    warp.Strength = EditorGUILayout.FloatField("Strength", warp.Strength);
                    param.Warp = warp;
                }


                /*
                else if (layer.Type == GTextureFilterType.HydraulicErosion)
                {
                    GHydraulicErosionParams erosion = param.HydraulicErosion;

                    erosion.Iteration = EditorGUILayout.IntField("Iteration", erosion.Iteration);

                    Vector3 dim = erosion.Dimension;
                    dim = GEditorCommon.InlineVector3Field("Dimension", dim);
                    dim.Set(Mathf.Max(1, dim.x), Mathf.Max(1, dim.y), Mathf.Max(1, dim.z));
                    erosion.Dimension = dim;

                    erosion.Rain = EditorGUILayout.Slider("Rain", erosion.Rain, 0f, 1f);
                    erosion.Transportation = EditorGUILayout.Slider("Transportation", erosion.Transportation, 0f, 1f);
                    erosion.AngleMin = EditorGUILayout.Slider("Min Angle", erosion.AngleMin, 1f, 45f);
                    erosion.Evaporation = EditorGUILayout.Slider("Evaporation", erosion.Evaporation, 0f, 1f);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Water Source");
                    using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(-1))
                    {
                        erosion.WaterSourceMap = EditorGUILayout.ObjectField(erosion.WaterSourceMap, typeof(Texture2D), false) as Texture2D;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Hardness");
                    using (EditorGUI.IndentLevelScope level = new EditorGUI.IndentLevelScope(-1))
                    {
                        erosion.HardnessMap = EditorGUILayout.ObjectField(erosion.HardnessMap, typeof(Texture2D), false) as Texture2D;
                    }
                    EditorGUILayout.EndHorizontal();

                    param.HydraulicErosion = erosion;
                }
                */

                layer.Param = param;
                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.indentLevel = indent;
        }

        private void ConfirmAndRemoveFilterAt(int index)
        {
            GTextureFilterLayer layer = GTextureToolParams.Instance.Filters[index];
            string layerName = ObjectNames.NicifyVariableName(layer.Type.ToString());
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove filter " + layerName + "?",
                "OK", "Cancel"))
            {
                GTextureToolParams.Instance.Filters.RemoveAt(index);
                RenderPreviewTexture();
            }
        }

        private void DrawLivePreviewParamsGUI()
        {
            string id = "texture-editor-live-preview";
            string label = "Live Preview";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                GLivePreviewParams param = GTextureToolParams.Instance.LivePreview;

                param.Enable = EditorGUILayout.Toggle("Enable", param.Enable);
                GUI.enabled = !lockLivePreviewTerrain;
                param.Terrain = EditorGUILayout.ObjectField("Terrain", param.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
                GUI.enabled = param.Mode == GLivePreviewMode.Mask;
                param.Color = EditorGUILayout.ColorField("Color", param.Color);
                GUI.enabled = true;
                param.Mode = (GLivePreviewMode)EditorGUILayout.EnumPopup("Mode", param.Mode);

                GTextureToolParams.Instance.LivePreview = param;
            });
        }

        private void SetLivePreviewTerrain(GStylizedTerrain terrain)
        {
            GLivePreviewParams param = GTextureToolParams.Instance.LivePreview;
            param.Terrain = terrain;
            GTextureToolParams.Instance.LivePreview = param;
        }

        private void RenderPreviewTexture()
        {
            PreviewRt.Release();

            IGTextureGenerator generator = GetActiveGenerator();
            if (generator != null)
            {
                generator.Generate(PreviewRt);
            }

            List<GTextureFilterLayer> layers = GTextureToolParams.Instance.Filters;
            for (int i = 0; i < layers.Count; ++i)
            {
                layers[i].Apply(PreviewRt);
            }

            SceneView.RepaintAll();
        }

        private IGTextureGenerator GetActiveGenerator()
        {
            GTextureGenerationMode mode = GTextureToolParams.Instance.General.Mode;
            string className = "G" + mode.ToString() + "Generator";
            Type type = GCommon.GetAllLoadedTypes().Find(t => t.Name.Equals(className));
            if (type != null && type.GetInterface(typeof(IGTextureGenerator).Name) != null)
            {
                return Activator.CreateInstance(type) as IGTextureGenerator;
            }

            return null;
        }

        private void SaveAsset()
        {
            RenderPreviewTexture();

            GGeneralParams generalParams = GTextureToolParams.Instance.General;
            GUtilities.EnsureDirectoryExists(generalParams.Directory);

            string ext =
                generalParams.Extension == GImageFileExtension.PNG ? "png" :
                generalParams.Extension == GImageFileExtension.JPG ? "jpg" :
                generalParams.Extension == GImageFileExtension.EXR ? "exr" :
                generalParams.Extension == GImageFileExtension.TGA ? "tga" : "file";
            string fileName = string.Format("{0}_{1}.{2}", GCommon.GetTimeTick(), generalParams.Mode.ToString(), ext);
            string filePath = Path.Combine(generalParams.Directory, fileName);

            TextureFormat format = generalParams.UseHighPrecisionTexture ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
            Texture2D tex = new Texture2D(generalParams.Resolution, generalParams.Resolution, format, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            GCommon.CopyFromRT(tex, PreviewRt);

            byte[] data =
                generalParams.Extension == GImageFileExtension.PNG ? tex.EncodeToPNG() :
                generalParams.Extension == GImageFileExtension.JPG ? tex.EncodeToPNG() :
                generalParams.Extension == GImageFileExtension.EXR ? tex.EncodeToPNG() :
                generalParams.Extension == GImageFileExtension.TGA ? tex.EncodeToPNG() : new byte[0];
            File.WriteAllBytes(filePath, data);

            GUtilities.DestroyObject(tex);
            AssetDatabase.Refresh();

            Object o = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            if (o != null)
            {
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        private void OnBeginCameraRender(Camera cam)
        {
            GLivePreviewParams livePreviewParam = GTextureToolParams.Instance.LivePreview;
            if (!livePreviewParam.Enable || livePreviewParam.Terrain == null)
                return;

            GGeneralParams generalParam = GTextureToolParams.Instance.General;

            Material mat = GInternalMaterials.UnlitTextureMaterial;
            livePreviewMaterialProperties.Clear();

            if (livePreviewParam.Mode == GLivePreviewMode.Mask)
            {
                mat = GInternalMaterials.MaskVisualizerMaterial;
                livePreviewMaterialProperties.SetTexture("_MainTex", PreviewRt);
                livePreviewMaterialProperties.SetColor("_Color", livePreviewParam.Color);
            }
            else if (livePreviewParam.Mode == GLivePreviewMode.ColorMap)
            {
                mat = GInternalMaterials.UnlitTextureMaterial;
                livePreviewMaterialProperties.SetTexture("_MainTex", PreviewRt);
            }
            else if (livePreviewParam.Mode == GLivePreviewMode.Geometry)
            {
                mat = GInternalMaterials.GeometryLivePreviewMaterial;
                GStylizedTerrain t = livePreviewParam.Terrain;
                Vector3 terrainSize = new Vector3(
                    t.TerrainData.Geometry.Width,
                    t.TerrainData.Geometry.Height,
                    t.TerrainData.Geometry.Length);
                livePreviewMaterialProperties.SetTexture("_OldHeightMap", t.TerrainData.Geometry.HeightMap);
                livePreviewMaterialProperties.SetTexture("_NewHeightMap", PreviewRt);
                livePreviewMaterialProperties.SetTexture("_MainTex", PreviewRt);
                livePreviewMaterialProperties.SetFloat("_Height", t.TerrainData.Geometry.Height);
                livePreviewMaterialProperties.SetVector("_BoundMin", t.transform.position);
                livePreviewMaterialProperties.SetVector("_BoundMax", t.transform.TransformPoint(terrainSize));
            }

            GTerrainChunk[] chunks = livePreviewParam.Terrain.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;
                Graphics.DrawMesh(
                    m,
                    chunks[i].transform.localToWorldMatrix,
                    mat,
                    chunks[i].gameObject.layer,
                    cam,
                    0,
                    livePreviewMaterialProperties,
                    false,
                    false);
            }
        }

        private void OnBeginCameraRenderSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
        {
            OnBeginCameraRender(cam);
        }

        private void OnTerrainDataDirty(GTerrainData data, GTerrainData.DirtyFlags flag)
        {
            GStylizedTerrain t = GTextureToolParams.Instance.LivePreview.Terrain;
            if (t == null || t.TerrainData != data)
                return;
            RenderPreviewTexture();
        }
    }
}
#endif
