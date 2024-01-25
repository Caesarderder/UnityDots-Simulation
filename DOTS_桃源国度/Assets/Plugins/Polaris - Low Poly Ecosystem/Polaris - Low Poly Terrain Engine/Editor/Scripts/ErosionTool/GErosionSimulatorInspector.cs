#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Rendering;
using Pinwheel.Griffin.BackupTool;

namespace Pinwheel.Griffin.ErosionTool
{
    [CustomEditor(typeof(GErosionSimulator))]
    public class GErosionSimulatorInspector : Editor
    {
        private GErosionSimulator instance;
        private Dictionary<string, RenderTexture> previewTextures;

        private static readonly string HISTORY_PREFIX_GEOMETRY = "Apply Erosion (Geometry)";
        private static readonly string HISTORY_PREFIX_TEXTURE = "Apply Erosion (Texture)";

        private void OnEnable()
        {
            instance = target as GErosionSimulator;

            Tools.hidden = true;
            SceneView.duringSceneGui += DuringSceneGUI;
            Undo.undoRedoPerformed += OnUndoRedo;
            GCommon.RegisterBeginRender(OnCameraRender);
            GCommon.RegisterBeginRenderSRP(OnCameraRenderSRP);

            instance.UpdateFalloffTexture();
            instance.Initialize();
        }

        private void OnDisable()
        {
            Tools.hidden = false;
            SceneView.duringSceneGui -= DuringSceneGUI;
            Undo.undoRedoPerformed -= OnUndoRedo;
            GCommon.UnregisterBeginRender(OnCameraRender);
            GCommon.UnregisterBeginRenderSRP(OnCameraRenderSRP);


            if (previewTextures != null)
            {
                foreach (string k in previewTextures.Keys)
                {
                    RenderTexture rt = previewTextures[k];
                    if (rt == null)
                        continue;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
            }
        }

        private class GBaseGUI
        {
            public static readonly GUIContent GROUP_ID = new GUIContent("Group Id", "Id of the terrain group which is edited by this tool");
            public static readonly GUIContent ENABLE_TERRAIN_MASK = new GUIContent("Enable Terrain Mask", "Use terrain mask (R) to lock a particular region from editing");
            public static readonly GUIContent SHOW_TERRAIN_MASK = new GUIContent("Show Terrain Mask", "Draw an overlay of the terrain mask in the scene view");
            public static readonly GUIContent ENABLE_TOPOGRAPHIC = new GUIContent("Enable Topographic", "Draw topographic view over the terrain for better sense of altitude");
            public static readonly GUIContent LIVE_PREVIEW_MODE = new GUIContent("Live Preview Mode", "Draw a preview over the terrain");
            public static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Define the blend factor between the simulation result and the existing terrain geometry");
        }

        public override void OnInspectorGUI()
        {
            instance.GroupId = GEditorCommon.ActiveTerrainGroupPopupWithAllOption(GBaseGUI.GROUP_ID, instance.GroupId);
            GEditorSettings.Instance.erosionTools.livePreviewMode = (GEditorSettings.ErosionToolsSettings.LivePreviewMode)EditorGUILayout.EnumPopup(GBaseGUI.LIVE_PREVIEW_MODE, GEditorSettings.Instance.erosionTools.livePreviewMode);
            instance.EnableTerrainMask = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TERRAIN_MASK, instance.EnableTerrainMask);
            if (instance.EnableTerrainMask)
            {
                GEditorSettings.Instance.erosionTools.showTerrainMask = EditorGUILayout.Toggle(GBaseGUI.SHOW_TERRAIN_MASK, GEditorSettings.Instance.erosionTools.showTerrainMask);
            }
            GEditorSettings.Instance.topographic.enable = EditorGUILayout.Toggle(GBaseGUI.ENABLE_TOPOGRAPHIC, GEditorSettings.Instance.topographic.enable);

            EditorGUI.BeginChangeCheck();
            instance.FalloffCurve = EditorGUILayout.CurveField(GBaseGUI.FALLOFF, instance.FalloffCurve, Color.red, GCommon.UnitRect);
            if (EditorGUI.EndChangeCheck())
            {
                instance.UpdateFalloffTexture();
            }

            DrawInstruction();
            DrawInitGUI();
            DrawHydraulicConfigs();
            DrawThermalConfigs();
            DrawTexturingConfigs();
            DrawDataViewGUI();
            DrawFinalizationGUI();
            GEditorCommon.DrawBackupHelpBox();
            EditorUtility.SetDirty(GEditorSettings.Instance);
        }

        private class GInstructionGUI
        {
            public static readonly string LABEL = "Instruction";
            public static readonly string ID = "erosion-sim-instruction";
            public static readonly string INSTRUCTION =
                "Simulate natural erosion caused by water flow and temperature, produce better geometry.\n" +
                "1. Set the simulation boundary by using Transform component or Transform Gizmos.\n" +
                "2. Set Detail Level.\n" +
                "3. Simulate hydraulic and thermal erosion. Set Live Preview Mode to Geometry to see the effect.\n" +
                "4. Use the simulation data for texturing. Set Live Preview Mode to Texture to see the color.\n" +
                "5. Click Apply Geometry or Apply Texture.\n" +
                "Simulation data will be reseted when you move/rotate/scale the simulator.";
        }

        private void DrawInstruction()
        {
            GEditorCommon.Foldout(GInstructionGUI.LABEL, true, GInstructionGUI.ID, () =>
            {
                EditorGUILayout.LabelField(GInstructionGUI.INSTRUCTION, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private class GInitGUI
        {
            public static readonly string LABEL = "Initialization";
            public static readonly string ID = "erosion-sim-init";
            public static readonly GUIContent DETAIL_LEVEL = new GUIContent("Detail Level", "Smaller value runs faster and produces larger features, while larger value is more expensive but produces more micro details");
            public static readonly string INIT = "Initialize";
        }

        private void DrawInitGUI()
        {
            GEditorCommon.Foldout(GInitGUI.LABEL, true, GInitGUI.ID, () =>
            {
                EditorGUI.BeginChangeCheck();
                instance.DetailLevel = EditorGUILayout.Slider(GInitGUI.DETAIL_LEVEL, instance.DetailLevel, 0f, 2f);
                if (EditorGUI.EndChangeCheck())
                {
                    instance.Initialize();
                }

                if (GUILayout.Button(GInitGUI.INIT))
                {
                    instance.Initialize();
                }
            });
        }

        private class GHydraulicConfigsGUI
        {
            public static readonly string LABEL = "Hydraulic Erosion";
            public static readonly string ID = "erosion-sim-hydraulic-config";

            public static readonly GUIContent WATER_SOURCE_AMOUNT = new GUIContent("Water Source", "The amount of water pour into the system in each iteration");
            public static readonly GUIContent WATER_SOURCE_OVER_TIME = new GUIContent(" ", "Strength of the water source that changes over time");
            public static readonly GUIContent WATER_SOURCE_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the water source amount without modifying its base value");

            public static readonly GUIContent RAIN_RATE = new GUIContent("Rain Rate", "Rain probability, more rain causes more erosion");
            public static readonly GUIContent RAIN_OVER_TIME = new GUIContent(" ", "Strength of the rain that changes over time");
            public static readonly GUIContent RAIN_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the rain amount without modifying its base value");

            public static readonly GUIContent FLOW_RATE = new GUIContent("Flow Rate", "Water flow speed. Default value is fine, too high may cause numerical error");
            public static readonly GUIContent FLOW_OVER_TIME = new GUIContent(" ", "Flow speed of the water that changes over time");
            public static readonly GUIContent FLOW_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the flow speed without modifying its base value");

            public static readonly GUIContent EROSION_RATE = new GUIContent("Erosion Rate", "Strength of the erosion, higher value will pick up more soil and carve deeper into the terrain");
            public static readonly GUIContent EROSION_OVER_TIME = new GUIContent(" ", "Strength of the erosion that changes over time");
            public static readonly GUIContent EROSION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the erosion strength without modifying its base value");

            public static readonly GUIContent DEPOSITION_RATE = new GUIContent("Deposition Rate", "Strength of the deposition, higher value will add more soil back to the terrain, while lower value will make the deposition wide spread");
            public static readonly GUIContent DEPOSITION_OVER_TIME = new GUIContent(" ", "Strength of the deposition that changes over time");
            public static readonly GUIContent DEPOSITION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the deposition strength without modifying its base value");

            public static readonly GUIContent EVAPORATION_RATE = new GUIContent("Evaporation Rate", "Strength of the evaporation that remove water from the system");
            public static readonly GUIContent EVAPORATION_OVER_TIME = new GUIContent(" ", "Strength of the evaporation that changes over time");
            public static readonly GUIContent EVAPORATION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the evaporation strength without modifying its base value");

            public static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration", "The number of simulation step to perform");

            public static readonly string SIMULATE_LABEL = "Simulate";
        }

        private void DrawHydraulicConfigs()
        {
            GEditorCommon.Foldout(GHydraulicConfigsGUI.LABEL, false, GHydraulicConfigsGUI.ID, () =>
            {
                GHydraulicErosionConfigs config = instance.HydraulicConfigs;
                config.WaterSourceAmount = EditorGUILayout.FloatField(GHydraulicConfigsGUI.WATER_SOURCE_AMOUNT, config.WaterSourceAmount);
                config.WaterSourceOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.WATER_SOURCE_OVER_TIME, config.WaterSourceOverTime, Color.blue, GCommon.UnitRect);
                config.WaterSourceMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.WATER_SOURCE_MULTIPLIER, config.WaterSourceMultiplier, 0f, 2f);

                config.RainRate = EditorGUILayout.FloatField(GHydraulicConfigsGUI.RAIN_RATE, config.RainRate);
                config.RainOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.RAIN_OVER_TIME, config.RainOverTime, Color.cyan, GCommon.UnitRect);
                config.RainMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.RAIN_MULTIPLIER, config.RainMultiplier, 0f, 2f);

                config.FlowRate = EditorGUILayout.FloatField(GHydraulicConfigsGUI.FLOW_RATE, config.FlowRate);
                config.FlowOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.FLOW_OVER_TIME, config.FlowOverTime, Color.yellow, GCommon.UnitRect);
                config.FlowMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.FLOW_MULTIPLIER, config.FlowMultiplier, 0f, 2f);

                config.ErosionRate = EditorGUILayout.FloatField(GHydraulicConfigsGUI.EROSION_RATE, config.ErosionRate);
                config.ErosionOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.EROSION_OVER_TIME, config.ErosionOverTime, Color.red, GCommon.UnitRect);
                config.ErosionMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.EROSION_MULTIPLIER, config.ErosionMultiplier, 0f, 2f);

                config.DepositionRate = EditorGUILayout.FloatField(GHydraulicConfigsGUI.DEPOSITION_RATE, config.DepositionRate);
                config.DepositionOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.DEPOSITION_OVER_TIME, config.DepositionOverTime, Color.green, GCommon.UnitRect);
                config.DepositionMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.DEPOSITION_MULTIPLIER, config.DepositionMultiplier, 0f, 2f);

                config.EvaporationRate = EditorGUILayout.FloatField(GHydraulicConfigsGUI.EVAPORATION_RATE, config.EvaporationRate);
                config.EvaporationOverTime = EditorGUILayout.CurveField(GHydraulicConfigsGUI.EVAPORATION_OVER_TIME, config.EvaporationOverTime, Color.gray, GCommon.UnitRect);
                config.EvaporationMultiplier = EditorGUILayout.Slider(GHydraulicConfigsGUI.EVAPORATION_MULTIPLIER, config.EvaporationMultiplier, 0f, 2f);

                config.IterationCount = EditorGUILayout.IntField(GHydraulicConfigsGUI.ITERATION_COUNT, config.IterationCount);

                if (GUILayout.Button(GHydraulicConfigsGUI.SIMULATE_LABEL))
                {
                    instance.SimulateHydraulicErosion();
                }
            });
        }

        private class GThermalGUI
        {
            public static readonly string LABEL = "Thermal Erosion";
            public static readonly string ID = "erosion-sim-thermal-configs";

            public static readonly GUIContent EROSION_RATE = new GUIContent("Erosion Rate", "Strength of the erosion, higher value causes more soil to slide down the slope");
            public static readonly GUIContent EROSION_OVER_TIME = new GUIContent(" ", "Strength of the erosion that change over time");
            public static readonly GUIContent EROSION_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the erosion strength without modifying its base value");

            public static readonly GUIContent RESTING_ANGLE = new GUIContent("Resting Angle", "The angle in degree where soil stop sliding");
            public static readonly GUIContent RESTING_ANGLE_OVER_TIME = new GUIContent(" ", "The resting angle that change over time");
            public static readonly GUIContent RESTING_ANGLE_MULTIPLIER = new GUIContent(" ", "Overall multiplier. Change the resting angle without modifying its base value");

            public static readonly GUIContent ITERATION_COUNT = new GUIContent("Iteration", "The number of simulation step to perform");

            public static readonly string SIMULATE_LABEL = "Simulate";
        }

        private void DrawThermalConfigs()
        {
            GEditorCommon.Foldout(GThermalGUI.LABEL, false, GThermalGUI.ID, () =>
            {
                GThermalErosionConfigs config = instance.ThermalConfigs;
                config.ErosionRate = EditorGUILayout.FloatField(GThermalGUI.EROSION_RATE, config.ErosionRate);
                config.ErosionOverTime = EditorGUILayout.CurveField(GThermalGUI.EROSION_OVER_TIME, config.ErosionOverTime, Color.red, GCommon.UnitRect);
                config.ErosionMultiplier = EditorGUILayout.Slider(GThermalGUI.EROSION_MULTIPLIER, config.ErosionMultiplier, 0f, 2f);

                config.RestingAngle = EditorGUILayout.FloatField(GThermalGUI.RESTING_ANGLE, config.RestingAngle);
                config.RestingAngleOverTime = EditorGUILayout.CurveField(GThermalGUI.RESTING_ANGLE_OVER_TIME, config.RestingAngleOverTime, Color.green, GCommon.UnitRect);
                config.RestingAngleMultiplier = EditorGUILayout.Slider(GThermalGUI.RESTING_ANGLE_MULTIPLIER, config.RestingAngleMultiplier, 0f, 2f);

                config.IterationCount = EditorGUILayout.IntField(GThermalGUI.ITERATION_COUNT, config.IterationCount);

                if (GUILayout.Button(GThermalGUI.SIMULATE_LABEL))
                {
                    instance.SimulateThermalErosion();
                }
            });
        }

        private class GTexturingGUI
        {
            public static readonly string LABEL = "Texturing";
            public static readonly string ID = "erosion-sim-texturing";

            public static readonly GUIContent MODE = new GUIContent("Mode", "Texturing mode, depend on which shader your terrains are using");

            public static readonly string EROSION_HEADER = "Erosion";
            public static readonly GUIContent EROSION_INTENSITY = new GUIContent("Intensity", "Intensity factor for erosion splat/color");
            public static readonly GUIContent EROSION_EXPONENT = new GUIContent("Exponent", "Exponential factor for erosion splat/color");
            public static readonly GUIContent EROSION_SPLAT_INDEX = new GUIContent("Splat", "The splat texture to apply on eroded regions");
            public static readonly GUIContent EROSION_ALBEDO = new GUIContent("Albedo", "The albedo color for eroded regions");
            public static readonly GUIContent EROSION_METALLIC = new GUIContent("Metallic", "Metallic value for eroded regions");
            public static readonly GUIContent EROSION_SMOOTHNESS = new GUIContent("Smoothness", "Smoothness value for eroded regions");

            public static readonly string DEPOSITION_HEADER = "Deposition";
            public static readonly GUIContent DEPOSITION_INTENSITY = new GUIContent("Intensity", "Intensity factor for deposition splat/color");
            public static readonly GUIContent DEPOSITION_EXPONENT = new GUIContent("Exponent", "Exponential factor for deposition splat/color");
            public static readonly GUIContent DEPOSITION_SPLAT_INDEX = new GUIContent("Splat", "The splat texture to apply on deposited regions");
            public static readonly GUIContent DEPOSITION_ALBEDO = new GUIContent("Albedo", "The albedo color for deposited regions");
            public static readonly GUIContent DEPOSITION_METALLIC = new GUIContent("Metallic", "Metallic value for deposited regions");
            public static readonly GUIContent DEPOSITION_SMOOTHNESS = new GUIContent("Smoothness", "Smoothness value for deposited regions");
        }

        private void DrawTexturingConfigs()
        {
            GEditorCommon.Foldout(GTexturingGUI.LABEL, false, GTexturingGUI.ID, () =>
            {
                GErosionTexturingConfigs config = instance.TexturingConfigs;
                config.TexturingMode = (GErosionTexturingConfigs.GMode)EditorGUILayout.EnumPopup(GTexturingGUI.MODE, config.TexturingMode);

                GEditorCommon.Header(GTexturingGUI.EROSION_HEADER);
                config.ErosionIntensity = EditorGUILayout.FloatField(GTexturingGUI.EROSION_INTENSITY, config.ErosionIntensity);
                config.ErosionExponent = EditorGUILayout.FloatField(GTexturingGUI.EROSION_EXPONENT, config.ErosionExponent);
                if (config.TexturingMode == GErosionTexturingConfigs.GMode.Splat)
                {
                    EditorGUILayout.PrefixLabel(GTexturingGUI.EROSION_SPLAT_INDEX);
                    config.ErosionSplatIndex = GEditorCommon.SplatSetSelectionGrid(instance.GroupId, config.ErosionSplatIndex);
                }
                else
                {
                    config.ErosionAlbedo = EditorGUILayout.ColorField(GTexturingGUI.EROSION_ALBEDO, config.ErosionAlbedo);
                    config.ErosionMetallic = EditorGUILayout.Slider(GTexturingGUI.EROSION_METALLIC, config.ErosionMetallic, 0f, 1f);
                    config.ErosionSmoothness = EditorGUILayout.Slider(GTexturingGUI.EROSION_SMOOTHNESS, config.ErosionSmoothness, 0f, 1f);
                }

                GEditorCommon.Header(GTexturingGUI.DEPOSITION_HEADER);
                config.DepositionIntensity = EditorGUILayout.FloatField(GTexturingGUI.DEPOSITION_INTENSITY, config.DepositionIntensity);
                config.DepositionExponent = EditorGUILayout.FloatField(GTexturingGUI.DEPOSITION_EXPONENT, config.DepositionExponent);
                if (config.TexturingMode == GErosionTexturingConfigs.GMode.Splat)
                {
                    EditorGUILayout.PrefixLabel(GTexturingGUI.DEPOSITION_SPLAT_INDEX);
                    config.DepositionSplatIndex = GEditorCommon.SplatSetSelectionGrid(instance.GroupId, config.DepositionSplatIndex);
                }
                else
                {
                    config.DepositionAlbedo = EditorGUILayout.ColorField(GTexturingGUI.DEPOSITION_ALBEDO, config.DepositionAlbedo);
                    config.DepositionMetallic = EditorGUILayout.Slider(GTexturingGUI.DEPOSITION_METALLIC, config.DepositionMetallic, 0f, 1f);
                    config.DepositionSmoothness = EditorGUILayout.Slider(GTexturingGUI.DEPOSITION_SMOOTHNESS, config.DepositionSmoothness, 0f, 1f);
                }
            });
        }

        private class GDataViewGUI
        {
            public static readonly string LABEL = "Data View";
            public static readonly string ID = "erosion-sim-data-view";

            public static readonly GUIContent DATA_VIEW = new GUIContent("View", "Select the data to view");
            public static readonly GUIContent DATA_SCALE = new GUIContent("Data Scale", "Scale the data for better view");
            public static readonly GUIContent DATA_CHANNEL = new GUIContent("Channel", "The channel to view");
            public static readonly GUIContent SIMULATION_DATA = new GUIContent("Simulation Data", "RGBA (terrain height, dissolved sediment, internal use, water level)");
            public static readonly GUIContent SIMULATION_MASK = new GUIContent("Simulation Mask", "RGBA (water source, rain, unused, erosion strength)");
            public static readonly GUIContent EROSION_MAP = new GUIContent("Erosion Map", "RG (erosion, deposition)");

            public static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
            public static readonly int SCALE = Shader.PropertyToID("_Scale");
            public static readonly int CHANNEL = Shader.PropertyToID("_Channel");

            private static Material dataViewMaterial;
            public static Material DataViewMaterial
            {
                get
                {
                    if (dataViewMaterial == null)
                    {
                        dataViewMaterial = new Material(Shader.Find("Hidden/Polaris/ErosionDataView"));
                    }
                    return dataViewMaterial;
                }
            }
        }

        private void DrawDataViewGUI()
        {
            GEditorCommon.Foldout(GDataViewGUI.LABEL, false, GDataViewGUI.ID, () =>
            {
                GEditorSettings.Instance.erosionTools.dataView = (GEditorSettings.ErosionToolsSettings.DataViewSelection)EditorGUILayout.EnumPopup(GDataViewGUI.DATA_VIEW, GEditorSettings.Instance.erosionTools.dataView);
                GEditorSettings.Instance.erosionTools.dataViewScale = EditorGUILayout.FloatField(GDataViewGUI.DATA_SCALE, GEditorSettings.Instance.erosionTools.dataViewScale);
                GEditorSettings.Instance.erosionTools.dataViewChannel = (GEditorSettings.ErosionToolsSettings.DataViewChannel)EditorGUILayout.EnumPopup(GDataViewGUI.DATA_CHANNEL, GEditorSettings.Instance.erosionTools.dataViewChannel);

                Texture t = null;
                GUIContent dataInfo;
                if (GEditorSettings.Instance.erosionTools.dataView == GEditorSettings.ErosionToolsSettings.DataViewSelection.SimulationData)
                {
                    t = instance.SimulationData;
                    dataInfo = GDataViewGUI.SIMULATION_DATA;
                }
                else if (GEditorSettings.Instance.erosionTools.dataView == GEditorSettings.ErosionToolsSettings.DataViewSelection.SimulationMask)
                {
                    t = instance.SimulationMask;
                    dataInfo = GDataViewGUI.SIMULATION_MASK;
                }
                else if (GEditorSettings.Instance.erosionTools.dataView == GEditorSettings.ErosionToolsSettings.DataViewSelection.ErosionMap)
                {
                    t = instance.ErosionMap;
                    dataInfo = GDataViewGUI.EROSION_MAP;
                }
                else
                {
                    t = Texture2D.blackTexture;
                    dataInfo = null;
                }

                Material mat = GDataViewGUI.DataViewMaterial;
                mat.SetFloat(GDataViewGUI.SCALE, GEditorSettings.Instance.erosionTools.dataViewScale);
                mat.SetFloat(GDataViewGUI.CHANNEL, (float)GEditorSettings.Instance.erosionTools.dataViewChannel);

                Rect r = GUILayoutUtility.GetAspectRect(1);
                EditorGUI.DrawPreviewTexture(r, t != null ? t : Texture2D.blackTexture, mat);
                if (dataInfo != null)
                {
                    EditorGUILayout.LabelField(dataInfo.tooltip);
                }
            });
        }

        private class GFinalizationGUI
        {
            public static readonly string LABEL = "Finalization";
            public static readonly string ID = "erosion-sim-finalization";

            public static readonly string APPLY_GEOMETRY_LABEL = "Apply Geometry";
            public static readonly string APPLY_TEXTURE_LABEL = "Apply Texture";
        }

        private void DrawFinalizationGUI()
        {
            GEditorCommon.Foldout(GFinalizationGUI.LABEL, true, GFinalizationGUI.ID, () =>
            {
                if (GUILayout.Button(GFinalizationGUI.APPLY_GEOMETRY_LABEL))
                {
                    List<GStylizedTerrain> terrains = instance.GetIntersectedTerrains();
                    GBackupInternal.TryCreateAndMergeInitialBackup(HISTORY_PREFIX_GEOMETRY, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
                    instance.ApplyGeometry();
                    GBackupInternal.TryCreateAndMergeBackup(HISTORY_PREFIX_GEOMETRY, terrains, GCommon.HeightMapAndFoliageResourceFlags, true);
                }
                if (GUILayout.Button(GFinalizationGUI.APPLY_TEXTURE_LABEL))
                {
                    List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
                    if (instance.TexturingConfigs.TexturingMode == GErosionTexturingConfigs.GMode.Splat)
                    {
                        flags.Add(GTerrainResourceFlag.SplatControlMaps);
                    }
                    else
                    {
                        flags.Add(GTerrainResourceFlag.AlbedoMap);
                        flags.Add(GTerrainResourceFlag.MetallicMap);
                    }
                    List<GStylizedTerrain> terrains = instance.GetIntersectedTerrains();
                    GBackupInternal.TryCreateAndMergeInitialBackup(HISTORY_PREFIX_TEXTURE, terrains, flags, true);
                    instance.ApplyTexture();
                    GBackupInternal.TryCreateAndMergeBackup(HISTORY_PREFIX_TEXTURE, terrains, flags, true);
                }
            });
        }

        private class GSceneViewGUI
        {
            public static readonly Vector3[] LOCAL_BOX = new Vector3[4]
            {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, -0.5f)
            };
        }

        private void DuringSceneGUI(SceneView sv)
        {
            Vector3[] worldBox = new Vector3[4];
            for (int i = 0; i < worldBox.Length; ++i)
            {
                worldBox[i] = instance.transform.TransformPoint(GSceneViewGUI.LOCAL_BOX[i]);
            }


            CompareFunction zTest = Handles.zTest;
            Color c = Color.yellow;
            c.a = 0.1f;
            Handles.color = c;
            Handles.zTest = CompareFunction.Greater;
            Handles.DrawPolyLine(worldBox[0], worldBox[1], worldBox[2], worldBox[3], worldBox[0]);

            c.a = 1f;
            Handles.color = c;
            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawPolyLine(worldBox[0], worldBox[1], worldBox[2], worldBox[3], worldBox[0]);
            Handles.zTest = zTest;

            EditorGUI.BeginChangeCheck();
            if (Tools.current == Tool.Move)
            {
                instance.transform.position = Handles.DoPositionHandle(instance.transform.position, instance.transform.rotation);
            }
            else if (Tools.current == Tool.Rotate)
            {
                Quaternion rotation = Handles.DoRotationHandle(instance.transform.rotation, instance.transform.position);
                Vector3 euler = rotation.eulerAngles;
                euler.x = 0;
                euler.z = 0;
                instance.transform.rotation = Quaternion.Euler(euler);

            }
            else if (Tools.current == Tool.Scale)
            {
                Vector3 scale = Handles.DoScaleHandle(instance.transform.localScale, instance.transform.position, instance.transform.rotation, HandleUtility.GetHandleSize(instance.transform.position));
                scale.x = Mathf.Clamp(scale.x, 100, 2000);
                scale.y = 1;
                scale.z = Mathf.Clamp(scale.z, 100, 2000);
                instance.transform.localScale = scale;
            }
            if (EditorGUI.EndChangeCheck())
            {
                instance.Initialize();
            }
        }

        private void OnUndoRedo()
        {
            if (Selection.activeGameObject != instance.gameObject)
                return;
            if (string.IsNullOrEmpty(GUndoCompatibleBuffer.Instance.CurrentBackupName))
                return;
            GBackup.Restore(GUndoCompatibleBuffer.Instance.CurrentBackupName);
        }

        private void OnCameraRender(Camera cam)
        {
            if (cam.cameraType != CameraType.SceneView)
                return;
            if (GEditorSettings.Instance.erosionTools.livePreviewMode != GEditorSettings.ErosionToolsSettings.LivePreviewMode.Off)
            {
                DrawLivePreview(cam);
            }
            if (instance.EnableTerrainMask && GEditorSettings.Instance.erosionTools.showTerrainMask)
            {
                DrawMask(cam);
            }
        }

        private void OnCameraRenderSRP(ScriptableRenderContext context, Camera cam)
        {
            OnCameraRender(cam);
        }

        private void DrawLivePreview(Camera cam)
        {
            List<GOverlapTestResult> overlapTests = GCommon.OverlapTest(instance.GroupId, instance.GetQuad());
            foreach (GOverlapTestResult test in overlapTests)
            {
                if (!test.IsOverlapped)
                    continue;
                DrawLivePreview(test.Terrain, cam, test.IsChunkOverlapped);
            }
        }

        private void DrawLivePreview(GStylizedTerrain t, Camera cam, bool[] chunkCulling)
        {
            if (t.transform.rotation != Quaternion.identity ||
                t.transform.lossyScale != Vector3.one)
                return;

            if (GEditorSettings.Instance.erosionTools.livePreviewMode == GEditorSettings.ErosionToolsSettings.LivePreviewMode.Geometry)
            {
                GErosionPreviewDrawer.DrawGeometryLivePreview(t, cam, instance, chunkCulling);
            }
            else if (GEditorSettings.Instance.erosionTools.livePreviewMode == GEditorSettings.ErosionToolsSettings.LivePreviewMode.Texture)
            {
                GErosionTexturingConfigs config = instance.TexturingConfigs;
                if (config.TexturingMode == GErosionTexturingConfigs.GMode.Splat)
                {
                    int controlMapCount = t.TerrainData.Shading.SplatControlMapCount;
                    int controlMapResolution = t.TerrainData.Shading.SplatControlResolution;
                    RenderTexture[] rtControls = new RenderTexture[controlMapCount];
                    for (int i = 0; i < controlMapCount; ++i)
                    {
                        Texture2D splatControl = t.TerrainData.Shading.GetSplatControl(i);
                        rtControls[i] = GetPreviewTexture(t, "controlMap" + i, controlMapResolution, splatControl.filterMode);
                        GCommon.ClearRT(rtControls[i]);
                    }

                    GErosionApplier applier = new GErosionApplier(instance);
                    applier.Internal_ApplySplat(t, rtControls);

                    GLivePreviewDrawer.DrawSplatLivePreview(t, cam, rtControls, chunkCulling);
                }
                else
                {
                    int albedoResolution = t.TerrainData.Shading.AlbedoMapResolution;
                    RenderTexture rtAlbedo = GetPreviewTexture(t, "albedo", albedoResolution, t.TerrainData.Shading.AlbedoMap.filterMode);

                    int metallicResolution = t.TerrainData.Shading.MetallicMapResolution;
                    RenderTexture rtMetallic = GetPreviewTexture(t, "metallic", metallicResolution, t.TerrainData.Shading.MetallicMap.filterMode);

                    GErosionApplier applier = new GErosionApplier(instance);
                    applier.Internal_ApplyAMS(t, rtAlbedo, rtMetallic);
                    GLivePreviewDrawer.DrawAMSLivePreview(t, cam, rtAlbedo, rtMetallic, chunkCulling);
                }
            }
        }

        private void DrawMask(Camera cam)
        {
            GCommon.ForEachTerrain(instance.GroupId, (t) =>
            {
                DrawMask(t, cam);
            });
        }

        private void DrawMask(GStylizedTerrain t, Camera cam)
        {
            GLivePreviewDrawer.DrawTerrainMask(t, cam);
        }

        private RenderTexture GetPreviewTexture(GStylizedTerrain t, string mapName, int resolution, FilterMode filter)
        {
            if (previewTextures == null)
            {
                previewTextures = new Dictionary<string, RenderTexture>();
            }

            string key = string.Format("{0}_{1}", t.GetInstanceID(), mapName);
            if (!previewTextures.ContainsKey(key) ||
                previewTextures[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                previewTextures[key] = rt;
            }
            else if (previewTextures[key].width != resolution || previewTextures[key].height != resolution)
            {
                previewTextures[key].Release();
                Object.DestroyImmediate(previewTextures[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                rt.wrapMode = TextureWrapMode.Clamp;
                previewTextures[key] = rt;
            }

            previewTextures[key].filterMode = filter;
            return previewTextures[key];
        }

    }
}
#endif
