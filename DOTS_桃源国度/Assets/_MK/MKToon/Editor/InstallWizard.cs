//////////////////////////////////////////////////////
// MK Install Wizard Base                      	    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
using Configuration = MK.Toon.Editor.InstallWizard.Configuration;
namespace MK.Toon.Editor.InstallWizard
{
    public sealed class InstallWizard : EditorWindow
    {
        public static InstallWizard instance = null;
        #pragma warning disable CS0414
        private static readonly string _version = "3.0.26B";
        #pragma warning restore CS0414
        
        private static readonly Vector2Int _referenceResolution = new Vector2Int(2560, 1440);
        private static float _sizeScale;
        private static int _scaledWidth;
        private static int _scaledHeight;
        private static Vector2 _windowScrollPos;

        private static readonly int _rawWidth = 360;
        private static readonly int _rawHeight = 640;
        private static readonly string _title = "MK Toon Install Wizard";

        private GUIStyle _flowTextStyle { get { return new GUIStyle(EditorStyles.label) { wordWrap = true }; } }
        private static readonly int _loadTimeInFrames = 72;
        private static int _waitFramesTillReload = _loadTimeInFrames;

        private static InstallWizard _window;
        private static RenderPipeline _targetRenderPipeline = RenderPipeline.Built_in;
        private static bool _showInstallerOnReload = true;

        [MenuItem("Window/MK/Toon/Install Wizard")]
        private static void ShowWindow()
        {
            if(Screen.currentResolution.height > Screen.currentResolution.width)
                _sizeScale = (float) Screen.currentResolution.width / (float)_referenceResolution.x;
            else
                _sizeScale = (float) Screen.currentResolution.height / (float)_referenceResolution.y;

            _scaledWidth = (int)((float)_rawWidth * _sizeScale);
            _scaledHeight = (int)((float)_rawHeight * _sizeScale);
            _window = (InstallWizard)EditorWindow.GetWindow<InstallWizard>(true, _title, true);
            _window.minSize = new Vector2(_scaledWidth, _scaledHeight);
            _window.maxSize = new Vector2(_scaledWidth * 2, _scaledHeight * 2);
            Configuration.ConfigureGlobalShaderFeatures();
            Configuration.BeginRegisterChangesOnGlobalShaderFeatures();
            Configuration.SetCompileDirectives();
            _window.Show();
        }

        [InitializeOnLoadMethod]
        private static void ShowInstallerOnReload()
        {
            QueryReload();
        }

        private static void QueryReload()
        {
            _waitFramesTillReload = _loadTimeInFrames;
            EditorApplication.update += Reload;
        }

        private static void Reload()
        {
            if (_waitFramesTillReload > 0)
            {
                --_waitFramesTillReload;
            }
            else
            {
                EditorApplication.update -= Reload;
                if(Configuration.isReady && Configuration.TryGetShowInstallerOnReload())
                    ShowWindow();
            }
        }

        private void OnGUI()
        {
            instance = this;
            int setupIndex = 1;
            if(Configuration.isReady)
            {
                _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);
                Texture2D titleImage = Configuration.TryGetTitleImage();
                if(titleImage)
                {
                    float titleScaledWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.standardVerticalSpacing * 4;
                    float titleScaledHeight = titleScaledWidth * ((float)titleImage.height / (float)titleImage.width);
                    Rect titleRect = EditorGUILayout.GetControlRect();
                    titleRect.width = titleScaledWidth;
                    titleRect.height = titleScaledHeight;
                    GUI.DrawTexture(titleRect, titleImage, ScaleMode.ScaleToFit);
                    GUILayout.Label("", GUILayout.Height(titleScaledHeight - 20));
                    Divider();
                }
                EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Select your Render Pipeline"), UnityEditor.EditorStyles.boldLabel);
                _targetRenderPipeline = Configuration.TryGetRenderPipeline();
                EditorGUI.BeginChangeCheck();
                _targetRenderPipeline = (RenderPipeline) EditorGUILayout.EnumPopup("Render Pipeline", _targetRenderPipeline);
                if(EditorGUI.EndChangeCheck())
                    Configuration.TrySetRenderPipeline(_targetRenderPipeline);
                VerticalSpace();
                Divider();
                VerticalSpace();
                EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Import Package"), UnityEditor.EditorStyles.boldLabel);
                if(GUILayout.Button("Import / Update Package"))
                {
                    EditorUtility.DisplayProgressBar("MK Toon Install Wizard", "Importing Package", 0.5f);
                    Configuration.ImportShaders(_targetRenderPipeline);
                    EditorUtility.ClearProgressBar();
                }
                if(_targetRenderPipeline == RenderPipeline.Universal)
                {
                    VerticalSpace();
                    Divider();
                    VerticalSpace();
                    EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Setup Per Object Outlines"), UnityEditor.EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("1. Select your renderer asset.", _flowTextStyle);
                    EditorGUILayout.LabelField("All of your used renderer assets can be found in the renderers list on your Universal Render Pipeline Assets.", _flowTextStyle);
                    EditorGUILayout.LabelField("2. Click the “Add Renderer Features” button and add the “MK Toon Per Object Outlines” component.", _flowTextStyle);
                    VerticalSpace();
                    #if MK_URP
                    Configuration.ShowURPOutlineWarning();
                    #endif
                }
                VerticalSpace();
                Divider();
                VerticalSpace();
                {
                    EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Import Examples (optional)"), UnityEditor.EditorStyles.boldLabel);
                    switch(_targetRenderPipeline)
                    {
                        case RenderPipeline.Built_in:
                        EditorGUILayout.LabelField("Make sure Postprocessing Stack v2 and Text Mesh Pro is installed.", _flowTextStyle);
                        EditorGUILayout.LabelField("Example Scenes are based on Linear Color Space. Make sure to change from Gamma to Linear Color Space via Project Settings.", _flowTextStyle);
                        break;
                        //case RenderPipeline.Lightweight:
                        //EditorGUILayout.LabelField("Make sure Text Mesh Pro is installed first!", _flowTextStyle);
                        //break;
                        case RenderPipeline.Universal:
                        EditorGUILayout.LabelField("Make sure Text Mesh Pro is installed.", _flowTextStyle);
                        break;
                    }
                    EditorGUILayout.LabelField("Old Input Manager is used for the examples.", _flowTextStyle);
                    if(GUILayout.Button("Import Examples"))
                    {
                        EditorUtility.DisplayProgressBar("MK Toon Install Wizard", "Importing Examples", 0.5f);
                        Configuration.ImportExamples(_targetRenderPipeline);
                        EditorUtility.ClearProgressBar();
                    }
                    VerticalSpace();
                    Divider();
                    ExampleContainer[] examples = Configuration.TryGetExamples();
                    if(examples.Length > 0 && examples[0].scene != null)
                    {
                        VerticalSpace();
                        EditorGUILayout.LabelField("Example Scenes:");
                        for(int i = 0; i < examples.Length; i++)
                        {
                            if(i % 5 == 0)
                                EditorGUILayout.BeginHorizontal();
                            if(examples[i].scene != null)
                                examples[i].DrawEditorButton();
                            if(i != 0 && i % 5 == 4 || i == examples.Length - 1)
                                EditorGUILayout.EndHorizontal();

                        }
                        VerticalSpace();
                        Divider();
                    }
                }
                VerticalSpace();
                EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Read Me (Recommended)"), UnityEditor.EditorStyles.boldLabel);
                if(GUILayout.Button("Open Read Me"))
                {
                    Configuration.OpenReadMe();
                }

                VerticalSpace();
                Divider();
                VerticalSpace();

                EditorGUILayout.LabelField(string.Concat(setupIndex++.ToString(), ". ", "Customize the Shaders (optional)"), UnityEditor.EditorStyles.boldLabel);

                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.67f;
                Configuration.DrawGlobalShaderFeaturesInspector();
                
                if(Configuration.CheckGlobalShaderFeaturesChanges())
                {
                    if(GUILayout.Button("Apply Changes"))
                    {
                        Configuration.ConfigureGlobalShaderFeatures();
                        Configuration.BeginRegisterChangesOnGlobalShaderFeatures();
                    }
                }

                VerticalSpace();
                Divider();
                VerticalSpace();
                #if MK_URP
                if(_targetRenderPipeline == RenderPipeline.Universal)
                {
                    bool disablePerObjectOutlinesWarning = Configuration.TryGetDisablePerOutlinesWarning();
                    EditorGUI.BeginChangeCheck();
                    disablePerObjectOutlinesWarning = EditorGUILayout.Toggle("Disable Per Object Outlines Warning", disablePerObjectOutlinesWarning);
                    if(EditorGUI.EndChangeCheck())
                        Configuration.TrySetDisablePerOutlinesWarning(disablePerObjectOutlinesWarning);
                }
                #endif

                _showInstallerOnReload = Configuration.TryGetShowInstallerOnReload();
                EditorGUI.BeginChangeCheck();
                _showInstallerOnReload = EditorGUILayout.Toggle("Show Installer On Reload", _showInstallerOnReload);
                if(EditorGUI.EndChangeCheck())
                    Configuration.TrySetShowInstallerOnReload(_showInstallerOnReload);

                EditorGUILayout.EndScrollView();
                GUI.FocusControl(null);
            }
            else
            {
                Repaint();
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private static void VerticalSpace()
        {
            GUILayoutUtility.GetRect(1f, EditorGUIUtility.standardVerticalSpacing);
        }

        private static void Divider()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
        }
    }
}
#endif