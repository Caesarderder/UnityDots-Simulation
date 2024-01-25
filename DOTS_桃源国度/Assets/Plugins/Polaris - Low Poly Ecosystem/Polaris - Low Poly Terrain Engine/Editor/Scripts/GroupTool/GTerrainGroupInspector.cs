#if GRIFFIN
using Pinwheel.Griffin.DataTool;
using Pinwheel.Griffin.Wizard;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    [CustomEditor(typeof(GTerrainGroup))]
    public class GTerrainGroupInspector : Editor
    {
        private const int TOGGLE_WIDTH = 12;

        private GTerrainGroup instance;

        private void OnEnable()
        {
            instance = (GTerrainGroup)target;
            //Tools.hidden = true;
        }

        private void OnDisable()
        {
            //Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            instance.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption("Group Id", instance.GroupId);
            instance.DeferredUpdate = EditorGUILayout.Toggle("Deferred Update", instance.DeferredUpdate);
            if (instance.DeferredUpdate)
            {
                Rect r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Update"))
                {
                    EditorUtility.DisplayProgressBar("Overriding", "Updating terrain...", 1);
                    instance.OverrideGeometry();
                    instance.OverrideShading();
                    instance.OverrideRendering();
                    instance.OverrideFoliage();
                    instance.OverrideMask();
                    EditorUtility.ClearProgressBar();
                }
                EditorGUILayout.Space();
            }

            DrawInstructionGUI();
            DrawGeometryOverrideGUI();
            DrawShadingOverrideGUI();
            DrawRenderingOverrideGUI();
            DrawFoliageOverrideGUI();
            DrawMaskOverrideGUI();
            DrawDataGUI();
            DrawReArrangeGUI();
            DrawMatchEdgeGUI();
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "instruction" + instance.GetInstanceID();

            GEditorCommon.Foldout(label, false, id, () =>
            {
                string s = string.Format(
                    "Quickly override properties value for every terrain object in the same group, maintain the consistency of terrain config in the scene.");
                EditorGUILayout.LabelField(s, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawGeometryOverrideGUI()
        {
            string label = "Geometry Override";
            string id = "geometryoverride" + instance.GetInstanceID();

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Default"),
                false,
                () => { instance.ResetGeometry(); });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Reset"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm",
                        "Reset geometry data on this terrain group? This action cannot be undone!",
                        "OK", "Cancel"))
                    {
                        GCommon.ForEachTerrain(
                            instance.GroupId,
                            (t) =>
                            {
                                if (t.TerrainData == null)
                                    return;
                                t.TerrainData.Geometry.ResetFull();
                            });
                        GStylizedTerrain.MatchEdges(instance.GroupId);
                    }
                });
            context.AddItem(
                new GUIContent("Update"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Geometry.SetRegionDirty(GCommon.UnitRect);
                            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
                        });
                });
            context.AddItem(
                new GUIContent("Clean Up"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Geometry.CleanUp();
                        });
                });

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.LabelField("Use with cause, this section takes time to process. You may want to use Deferred Update.", GEditorCommon.WordWrapItalicLabel);

                EditorGUI.indentLevel -= 1;
                GGeometryOverride g = instance.GeometryOverride;
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth - TOGGLE_WIDTH;

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Dimension");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                g.OverrideWidth = EditorGUILayout.Toggle(g.OverrideWidth, GUILayout.Width(TOGGLE_WIDTH));
                g.Width = EditorGUILayout.DelayedFloatField("Width", g.Width);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideHeight = EditorGUILayout.Toggle(g.OverrideHeight, GUILayout.Width(TOGGLE_WIDTH));
                g.Height = EditorGUILayout.DelayedFloatField("Height", g.Height);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideLength = EditorGUILayout.Toggle(g.OverrideLength, GUILayout.Width(TOGGLE_WIDTH));
                g.Length = EditorGUILayout.DelayedFloatField("Length", g.Length);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Height Map");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                g.OverrideHeightMapResolution = EditorGUILayout.Toggle(g.OverrideHeightMapResolution, GUILayout.Width(TOGGLE_WIDTH));
                g.HeightMapResolution = EditorGUILayout.DelayedIntField("Height Map Resolution", g.HeightMapResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Mesh Generation");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                g.OverrideMeshBaseResolution = EditorGUILayout.Toggle(g.OverrideMeshBaseResolution, GUILayout.Width(TOGGLE_WIDTH));
                g.MeshBaseResolution = EditorGUILayout.DelayedIntField("Mesh Base Resolution", g.MeshBaseResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideMeshResolution = EditorGUILayout.Toggle(g.OverrideMeshResolution, GUILayout.Width(TOGGLE_WIDTH));
                g.MeshResolution = EditorGUILayout.DelayedIntField("Mesh Resolution", g.MeshResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideChunkGridSize = EditorGUILayout.Toggle(g.OverrideChunkGridSize, GUILayout.Width(TOGGLE_WIDTH));
                g.ChunkGridSize = EditorGUILayout.DelayedIntField("Chunk Grid Size", g.ChunkGridSize);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideLodCount = EditorGUILayout.Toggle(g.OverrideLodCount, GUILayout.Width(TOGGLE_WIDTH));
                g.LODCount = EditorGUILayout.DelayedIntField("LOD Count", g.LODCount);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideDisplacementSeed = EditorGUILayout.Toggle(g.OverrideDisplacementSeed, GUILayout.Width(TOGGLE_WIDTH));
                g.DisplacementSeed = EditorGUILayout.DelayedIntField("Displacement Seed", g.DisplacementSeed);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideDisplacementStrength = EditorGUILayout.Toggle(g.OverrideDisplacementStrength, GUILayout.Width(TOGGLE_WIDTH));
                g.DisplacementStrength = EditorGUILayout.DelayedFloatField("Displacement Strength", g.DisplacementStrength);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideAlbedoToVertexColorMode = EditorGUILayout.Toggle(g.OverrideAlbedoToVertexColorMode, GUILayout.Width(TOGGLE_WIDTH));
                g.AlbedoToVertexColorMode = (GAlbedoToVertexColorMode)EditorGUILayout.EnumPopup("Albedo To Vertex Color", g.AlbedoToVertexColorMode);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideSmoothNormal = EditorGUILayout.Toggle(g.OverrideSmoothNormal, GUILayout.Width(TOGGLE_WIDTH));
                g.SmoothNormal = EditorGUILayout.Toggle("Smooth Normal", g.SmoothNormal);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideUseSmoothNormalMask = EditorGUILayout.Toggle(g.OverrideUseSmoothNormalMask, GUILayout.Width(TOGGLE_WIDTH));
                g.UseSmoothNormalMask = EditorGUILayout.Toggle("Smooth Normal Use Mask (G)", g.UseSmoothNormalMask);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideMergeUv = EditorGUILayout.Toggle(g.OverrideMergeUv, GUILayout.Width(TOGGLE_WIDTH));
                g.MergeUv = EditorGUILayout.Toggle("Merge UV", g.MergeUv);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Utilities");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                g.OverrideStorageMode = EditorGUILayout.Toggle(g.OverrideStorageMode, GUILayout.Width(TOGGLE_WIDTH));
                g.StorageMode = (GGeometry.GStorageMode)EditorGUILayout.EnumPopup("Storage", g.StorageMode);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                g.OverrideAllowTimeSlicedGeneration = EditorGUILayout.Toggle(g.OverrideAllowTimeSlicedGeneration, GUILayout.Width(TOGGLE_WIDTH));
                g.AllowTimeSlicedGeneration = EditorGUILayout.Toggle("Time Sliced", g.AllowTimeSlicedGeneration);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = labelWidth;

                if (EditorGUI.EndChangeCheck())
                {
                    instance.GeometryOverride = g;
                    if (!instance.DeferredUpdate)
                    {
                        EditorUtility.DisplayProgressBar("Overriding", "Re-generating terrains geometry", 1);
                        instance.OverrideGeometry();
                        EditorUtility.ClearProgressBar();
                    }
                }
                EditorGUI.indentLevel += 1;
            }, context);
        }

        private void DrawShadingOverrideGUI()
        {
            string label = "Shading Override";
            string id = "shadingoverride" + instance.GetInstanceID();

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Default"),
                false,
                () =>
                {
                    instance.ResetShading();
                });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Reset"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Shading.ResetFull();
                        });
                });
            context.AddItem(
                new GUIContent("Refresh"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Shading.UpdateLookupTextures();
                            t.TerrainData.Shading.UpdateMaterials();
                        });
                });
            context.AddItem(
                new GUIContent("Set Shader"),
                false,
                () =>
                {
                    GWizardWindow.ShowSetShaderTab(instance.GroupId);
                });


            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                GShadingOverride s = instance.ShadingOverride;
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth - TOGGLE_WIDTH;

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Material");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                s.OverrideCustomMaterial = EditorGUILayout.Toggle(s.OverrideCustomMaterial, GUILayout.Width(TOGGLE_WIDTH));
                s.CustomMaterial = EditorGUILayout.ObjectField("Material Template", s.CustomMaterial, typeof(Material), false) as Material;
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Color Map & Gradient Lookup");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                s.OverrideAlbedoMapResolution = EditorGUILayout.Toggle(s.OverrideAlbedoMapResolution, GUILayout.Width(TOGGLE_WIDTH));
                s.AlbedoMapResolution = EditorGUILayout.DelayedIntField("Albedo Map Resolution", s.AlbedoMapResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideMetallicMapResolution = EditorGUILayout.Toggle(s.OverrideMetallicMapResolution, GUILayout.Width(TOGGLE_WIDTH));
                s.MetallicMapResolution = EditorGUILayout.DelayedIntField("Metallic Map Resolution", s.MetallicMapResolution);
                EditorGUILayout.EndHorizontal();

                SerializedObject so = new SerializedObject(instance);
                SerializedProperty sp = so.FindProperty("shadingOverride");

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorByNormal = EditorGUILayout.Toggle(s.OverrideColorByNormal, GUILayout.Width(TOGGLE_WIDTH));
                SerializedProperty cbnProps = sp.FindPropertyRelative("colorByNormal");
                EditorGUILayout.PropertyField(cbnProps);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorBlendCurve = EditorGUILayout.Toggle(s.OverrideColorBlendCurve, GUILayout.Width(TOGGLE_WIDTH));
                s.ColorBlendCurve = EditorGUILayout.CurveField("Blend By Height", s.ColorBlendCurve, Color.red, new Rect(0, 0, 1, 1));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorByHeight = EditorGUILayout.Toggle(s.OverrideColorByHeight, GUILayout.Width(TOGGLE_WIDTH));
                SerializedProperty cbhProps = sp.FindPropertyRelative("colorByHeight");
                EditorGUILayout.PropertyField(cbhProps);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Splats");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplats = EditorGUILayout.Toggle(s.OverrideSplats, GUILayout.Width(TOGGLE_WIDTH));
                s.Splats = EditorGUILayout.ObjectField("Prototypes", s.Splats, typeof(GSplatPrototypeGroup), false) as GSplatPrototypeGroup;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatControlResolution = EditorGUILayout.Toggle(s.OverrideSplatControlResolution, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatControlResolution = EditorGUILayout.DelayedIntField("Splat Control Maps Resolution", s.SplatControlResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Advanced");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                s.OverrideAlbedoMapPropertyName = EditorGUILayout.Toggle(s.OverrideAlbedoMapPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.AlbedoMapPropertyName = EditorGUILayout.DelayedTextField("Albedo Map Property Name", s.AlbedoMapPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideMetallicMapPropertyName = EditorGUILayout.Toggle(s.OverrideMetallicMapPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.MetallicMapPropertyName = EditorGUILayout.DelayedTextField("Metallic Map Property Name", s.MetallicMapPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorByHeightPropertyName = EditorGUILayout.Toggle(s.OverrideColorByHeightPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.ColorByHeightPropertyName = EditorGUILayout.DelayedTextField("Color By Height Property Name", s.ColorByHeightPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorByNormalPropertyName = EditorGUILayout.Toggle(s.OverrideColorByNormalPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.ColorByNormalPropertyName = EditorGUILayout.DelayedTextField("Color By Normal Property Name", s.ColorByNormalPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideColorBlendPropertyName = EditorGUILayout.Toggle(s.OverrideColorBlendPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.ColorBlendPropertyName = EditorGUILayout.DelayedTextField("Color Blend Property Name", s.ColorBlendPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideDimensionPropertyName = EditorGUILayout.Toggle(s.OverrideDimensionPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.DimensionPropertyName = EditorGUILayout.DelayedTextField("Dimension Property Name", s.DimensionPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatControlMapPropertyName = EditorGUILayout.Toggle(s.OverrideSplatControlMapPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatControlMapPropertyName = EditorGUILayout.DelayedTextField("Splat Control Map Property Name", s.SplatControlMapPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatMapPropertyName = EditorGUILayout.Toggle(s.OverrideSplatMapPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatMapPropertyName = EditorGUILayout.DelayedTextField("Splat Map Property Name", s.SplatMapPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatNormalPropertyName = EditorGUILayout.Toggle(s.OverrideSplatNormalPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatNormalPropertyName = EditorGUILayout.DelayedTextField("Splat Normal Property Name", s.SplatNormalPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatMetallicPropertyName = EditorGUILayout.Toggle(s.OverrideSplatMetallicPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatMetallicPropertyName = EditorGUILayout.DelayedTextField("Splat Metallic Property Name", s.SplatMetallicPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                s.OverrideSplatSmoothnessPropertyName = EditorGUILayout.Toggle(s.OverrideSplatSmoothnessPropertyName, GUILayout.Width(TOGGLE_WIDTH));
                s.SplatSmoothnessPropertyName = EditorGUILayout.DelayedTextField("Splat Smoothness Property Name", s.SplatSmoothnessPropertyName);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = labelWidth;

                if (EditorGUI.EndChangeCheck())
                {
                    so.ApplyModifiedProperties();
                    instance.ShadingOverride = s;
                    if (!instance.DeferredUpdate)
                    {
                        instance.OverrideShading();
                    }
                }
                so.Dispose();
                sp.Dispose();
                cbhProps.Dispose();
                cbnProps.Dispose();
                EditorGUI.indentLevel += 1;
            }, context);
        }

        private void DrawRenderingOverrideGUI()
        {
            string label = "Rendering Override";
            string id = "renderingoverride" + instance.GetInstanceID();

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Default"),
                false,
                () =>
                {
                    instance.ResetRendering();
                });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Reset"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Rendering.ResetFull();
                        });
                });


            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                GRenderingOverride r = instance.RenderingOverride;
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth - TOGGLE_WIDTH;

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Terrain Shadow");
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.BeginHorizontal();
                r.OverrideCastShadow = EditorGUILayout.Toggle(r.OverrideCastShadow, GUILayout.Width(TOGGLE_WIDTH));
                r.CastShadow = EditorGUILayout.Toggle("Cast Shadow", r.CastShadow);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideReceiveShadow = EditorGUILayout.Toggle(r.OverrideReceiveShadow, GUILayout.Width(TOGGLE_WIDTH));
                r.ReceiveShadow = EditorGUILayout.Toggle("Receive Shadow", r.ReceiveShadow);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Tree Rendering");
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.BeginHorizontal();
                r.OverrideDrawTrees = EditorGUILayout.Toggle(r.OverrideDrawTrees, GUILayout.Width(TOGGLE_WIDTH));
                r.DrawTrees = EditorGUILayout.Toggle("Draw", r.DrawTrees);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideEnableInstancing = EditorGUILayout.Toggle(r.OverrideEnableInstancing, GUILayout.Width(TOGGLE_WIDTH));
                r.EnableInstancing = EditorGUILayout.Toggle("Enable Instancing", r.EnableInstancing);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideBillboardStart = EditorGUILayout.Toggle(r.OverrideBillboardStart, GUILayout.Width(TOGGLE_WIDTH));
                r.BillboardStart = EditorGUILayout.Slider("Billboard Start", r.BillboardStart, 0f, GCommon.MAX_TREE_DISTANCE);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideTreeDistance = EditorGUILayout.Toggle(r.OverrideTreeDistance, GUILayout.Width(TOGGLE_WIDTH));
                r.TreeDistance = EditorGUILayout.Slider("Tree Distance", r.TreeDistance, 0f, GCommon.MAX_TREE_DISTANCE);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.Toggle(true, GUILayout.Width(TOGGLE_WIDTH));
                GUI.enabled = true;
                GRuntimeSettings.Instance.renderingDefault.treeCullBias = EditorGUILayout.Slider("Cull Bias", GRuntimeSettings.Instance.renderingDefault.treeCullBias, 0f, 100f);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Grass & Detail Rendering");
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.BeginHorizontal();
                r.OverrideDrawGrasses = EditorGUILayout.Toggle(r.OverrideDrawGrasses, GUILayout.Width(TOGGLE_WIDTH));
                r.DrawGrasses = EditorGUILayout.Toggle("Draw", r.DrawGrasses);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideGrassDistance = EditorGUILayout.Toggle(r.OverrideGrassDistance, GUILayout.Width(TOGGLE_WIDTH));
                r.GrassDistance = EditorGUILayout.Slider("Grass Distance", r.GrassDistance, 0f, GCommon.MAX_GRASS_DISTANCE);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                r.OverrideGrassFadeStart = EditorGUILayout.Toggle(r.OverrideGrassFadeStart, GUILayout.Width(TOGGLE_WIDTH));
                r.GrassFadeStart = EditorGUILayout.Slider("Fade Start", r.GrassFadeStart, 0f, 1f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.Toggle(true, GUILayout.Width(TOGGLE_WIDTH));
                GUI.enabled = true;
                GRuntimeSettings.Instance.renderingDefault.grassCullBias = EditorGUILayout.Slider("Cull Bias", GRuntimeSettings.Instance.renderingDefault.grassCullBias, 0f, 100f);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = labelWidth;

                if (EditorGUI.EndChangeCheck())
                {
                    instance.RenderingOverride = r;
                    if (!instance.DeferredUpdate)
                    {
                        instance.OverrideRendering();
                    }
                }
                EditorGUI.indentLevel += 1;
            }, context);
        }

        private void DrawFoliageOverrideGUI()
        {
            string label = "Foliage Override";
            string id = "foliageoverride" + instance.GetInstanceID();

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Default"),
                false,
                () =>
                {
                    instance.ResetFoliage();
                });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Reset"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm",
                        "Reset foliage data on this terrain group? This action cannot be undone!",
                        "OK", "Cancel"))
                    {
                        GCommon.ForEachTerrain(
                            instance.GroupId,
                            (t) =>
                            {
                                if (t.TerrainData == null)
                                    return;
                                t.TerrainData.Foliage.ResetFull();
                            });
                    }
                });
            context.AddItem(
                new GUIContent("Refresh"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Foliage.Refresh();
                        });
                });
            context.AddItem(
                new GUIContent("Clear All Trees"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog(
                   "Confirm",
                   "Clear all trees on this terrain group? This action cannot be undone!",
                   "OK", "Cancel"))
                    {
                        GCommon.ForEachTerrain(
                            instance.GroupId,
                            (t) =>
                            {
                                if (t.TerrainData == null)
                                    return;
                                t.TerrainData.Foliage.TreeInstances.Clear();
                            });
                    }
                });
            context.AddItem(
                new GUIContent("Clear All Grass"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog(
                   "Confirm",
                   "Clear all grasses on this terrain group? This action cannot be undone!",
                   "OK", "Cancel"))
                    {
                        GCommon.ForEachTerrain(
                            instance.GroupId,
                            (t) =>
                            {
                                if (t.TerrainData == null)
                                    return;
                                t.TerrainData.Foliage.ClearGrassInstances();
                            });
                    }
                });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Update Trees"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Foliage.SetTreeRegionDirty(new Rect(0, 0, 1, 1));
                            t.UpdateTreesPosition();
                            t.TerrainData.Foliage.ClearTreeDirtyRegions();
                            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                        });
                });
            context.AddItem(
                new GUIContent("Update Grasses"),
                false,
                () =>
                {
                    GCommon.ForEachTerrain(
                        instance.GroupId,
                        (t) =>
                        {
                            if (t.TerrainData == null)
                                return;
                            t.TerrainData.Foliage.SetGrassRegionDirty(new Rect(0, 0, 1, 1));
                            t.UpdateGrassPatches();
                            t.TerrainData.Foliage.ClearGrassDirtyRegions();
                            t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                        });
                });

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                GFoliageOverride f = instance.FoliageOverride;
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth - TOGGLE_WIDTH;

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Trees");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                f.OverrideTrees = EditorGUILayout.Toggle(f.OverrideTrees, GUILayout.Width(TOGGLE_WIDTH));
                f.Trees = EditorGUILayout.ObjectField("Prototypes", f.Trees, typeof(GTreePrototypeGroup), false) as GTreePrototypeGroup;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideTreeSnapMode = EditorGUILayout.Toggle(f.OverrideTreeSnapMode, GUILayout.Width(TOGGLE_WIDTH));
                f.TreeSnapMode = (GSnapMode)EditorGUILayout.EnumPopup("Tree Snap Mode", f.TreeSnapMode);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideTreeSnapLayerMask = EditorGUILayout.Toggle(f.OverrideTreeSnapLayerMask, GUILayout.Width(TOGGLE_WIDTH));
                f.TreeSnapLayerMask = GEditorCommon.LayerMaskField("Tree Snap Layers", f.TreeSnapLayerMask);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                GEditorCommon.Header("Grasses & Details");
                EditorGUI.indentLevel -= 1;

                EditorGUILayout.BeginHorizontal();
                f.OverrideGrasses = EditorGUILayout.Toggle(f.OverrideGrasses, GUILayout.Width(TOGGLE_WIDTH));
                f.Grasses = EditorGUILayout.ObjectField("Prototypes", f.Grasses, typeof(GGrassPrototypeGroup), false) as GGrassPrototypeGroup;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideGrassSnapMode = EditorGUILayout.Toggle(f.OverrideGrassSnapMode, GUILayout.Width(TOGGLE_WIDTH));
                f.GrassSnapMode = (GSnapMode)EditorGUILayout.EnumPopup("Grass Snap Mode", f.GrassSnapMode);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideGrassSnapLayerMask = EditorGUILayout.Toggle(f.OverrideGrassSnapLayerMask, GUILayout.Width(TOGGLE_WIDTH));
                f.GrassSnapLayerMask = GEditorCommon.LayerMaskField("Grass Snap Layers", f.GrassSnapLayerMask);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverridePatchGridSize = EditorGUILayout.Toggle(f.OverridePatchGridSize, GUILayout.Width(TOGGLE_WIDTH));
                f.PatchGridSize = EditorGUILayout.DelayedIntField("Patch Grid Size", f.PatchGridSize);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideEnableInteractiveGrass = EditorGUILayout.Toggle(f.OverrideEnableInteractiveGrass, GUILayout.Width(TOGGLE_WIDTH));
                f.EnableInteractiveGrass = EditorGUILayout.Toggle("Interactive Grass", f.EnableInteractiveGrass);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideVectorFieldMapResolution = EditorGUILayout.Toggle(f.OverrideVectorFieldMapResolution, GUILayout.Width(TOGGLE_WIDTH));
                f.VectorFieldMapResolution = EditorGUILayout.DelayedIntField("Vector Field Map Resolution", f.VectorFieldMapResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideBendSensitive = EditorGUILayout.Toggle(f.OverrideBendSensitive, GUILayout.Width(TOGGLE_WIDTH));
                f.BendSensitive = EditorGUILayout.DelayedFloatField("Bend Sensitive", f.BendSensitive);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                f.OverrideRestoreSensitive = EditorGUILayout.Toggle(f.OverrideRestoreSensitive, GUILayout.Width(TOGGLE_WIDTH));
                f.RestoreSensitive = EditorGUILayout.DelayedFloatField("Restore Sensitive", f.RestoreSensitive);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = labelWidth;

                if (EditorGUI.EndChangeCheck())
                {
                    instance.FoliageOverride = f;
                    if (!instance.DeferredUpdate)
                    {
                        instance.OverrideFoliage();
                    }
                }
                EditorGUI.indentLevel += 1;
            }, context);
        }

        private void DrawMaskOverrideGUI()
        {
            string label = "Mask Override";
            string id = "maskoverride" + instance.GetInstanceID();

            GenericMenu context = new GenericMenu();
            context.AddItem(
                new GUIContent("Default"),
                false,
                () =>
                {
                    instance.ResetMask();
                });
            context.AddSeparator(null);
            context.AddItem(
                new GUIContent("Reset"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm",
                        "Reset mask data on this terrain group? This action cannot be undone!",
                        "OK", "Cancel"))
                    {
                        GCommon.ForEachTerrain(
                            instance.GroupId,
                            (t) =>
                            {
                                if (t.TerrainData == null)
                                    return;
                                t.TerrainData.Mask.ResetFull();
                            });
                    }
                });


            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUI.indentLevel -= 1;
                GMaskOverride m = instance.MaskOverride;
                EditorGUI.BeginChangeCheck();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth - TOGGLE_WIDTH;
                                
                EditorGUILayout.BeginHorizontal();
                m.OverrideMaskMapResolution = EditorGUILayout.Toggle(m.OverrideMaskMapResolution, GUILayout.Width(TOGGLE_WIDTH));
                m.MaskMapResolution = EditorGUILayout.DelayedIntField("Resolution", m.MaskMapResolution);
                EditorGUILayout.EndHorizontal();

                EditorGUIUtility.labelWidth = labelWidth;

                if (EditorGUI.EndChangeCheck())
                {
                    instance.MaskOverride = m;
                    if (!instance.DeferredUpdate)
                    {
                        instance.OverrideMask();
                    }
                }
                EditorGUI.indentLevel += 1;
            }, context);
        }

        private void DrawDataGUI()
        {
            string label = "Data";
            string id = "data" + instance.GetInstanceID();

            GEditorCommon.Foldout(label, false, id, () =>
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Import", EditorStyles.miniButtonLeft))
                {
                    ShowImportContext();
                }
                if (GUILayout.Button("Export", EditorStyles.miniButtonRight))
                {
                    ShowExportContext();
                }
                EditorGUILayout.EndHorizontal();
            });
        }

        private void ShowImportContext()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Unity Terrain Data"),
                false,
                () =>
                {
                    ShowUnityTerrainDataImporter();
                });

            menu.AddItem(
                new GUIContent("Raw"),
                false,
                () =>
                {
                    ShowRawImporter();
                });
            menu.AddItem(
                new GUIContent("Textures"),
                false,
                () =>
                {
                    ShowTextureImporter();
                });

            menu.ShowAsContext();
        }

        private void ShowExportContext()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Unity Terrain Data"),
                false,
                () =>
                {
                    ShowUnityTerrainDataExporter();
                });
            menu.AddItem(
                new GUIContent("Raw"),
                false,
                () =>
                {
                    ShowRawExporter();
                });
            menu.AddItem(
                new GUIContent("Textures"),
                false,
                () =>
                {
                    ShowTextureExporter();
                });

            menu.ShowAsContext();
        }

        private void ShowUnityTerrainDataImporter()
        {
            GUnityTerrainDataImporterWindow window = GUnityTerrainDataImporterWindow.ShowWindow();
            window.BulkImport = true;
            window.BulkImportGroupId = instance.GroupId;
        }

        private void ShowUnityTerrainDataExporter()
        {
            GUnityTerrainDataExporterWindow window = GUnityTerrainDataExporterWindow.ShowWindow();
            window.BulkExport = true;
            window.BulkExportGroupId = instance.GroupId;
        }

        private void ShowRawImporter()
        {
            GRawImporterWindow window = GRawImporterWindow.ShowWindow();
            window.BulkImport = true;
            window.BulkImportGroupId = instance.GroupId;
        }

        private void ShowRawExporter()
        {
            GRawExporterWindow window = GRawExporterWindow.ShowWindow();
            window.BulkExport = true;
            window.BulkExportGroupId = instance.GroupId;
        }

        private void ShowTextureImporter()
        {
            GTextureImporterWindow window = GTextureImporterWindow.ShowWindow();
            window.BulkImport = true;
            window.BulkImportGroupId = instance.GroupId;
        }

        private void ShowTextureExporter()
        {
            GTextureExporterWindow window = GTextureExporterWindow.ShowWindow();
            window.BulkExport = true;
            window.BulkExportGroupId = instance.GroupId;
        }

        private void DrawReArrangeGUI()
        {
            string label = "Re-Arrange";
            string id = "rearrange" + instance.GetInstanceID();

            GEditorCommon.Foldout(label, false, id, () =>
            {
                string s = string.Format(
                    "This function requires terrain neighboring to be set and the following properties to be overriden:\n" +
                    "   - Width\n" +
                    "   - Length");
                EditorGUILayout.LabelField(s, GEditorCommon.WordWrapItalicLabel);
                GGeometryOverride g = instance.GeometryOverride;
                GUI.enabled =
                    g.OverrideWidth &&
                    g.OverrideLength;
                if (GUILayout.Button("Re-Arrange"))
                {
                    instance.ReArrange();
                    GUtilities.MarkCurrentSceneDirty();
                }

                GUI.enabled = true;
            });
        }

        private void DrawMatchEdgeGUI()
        {
            string label = "Match Edge";
            string id = "match-edge" + instance.GetInstanceID();

            GEditorCommon.Foldout(label, false, id, () =>
            {
                if (GUILayout.Button("Match"))
                {
                    GStylizedTerrain.MatchEdges(instance.GroupId);
                    GUtilities.MarkCurrentSceneDirty();
                }
            });
        }
    }
}
#endif
