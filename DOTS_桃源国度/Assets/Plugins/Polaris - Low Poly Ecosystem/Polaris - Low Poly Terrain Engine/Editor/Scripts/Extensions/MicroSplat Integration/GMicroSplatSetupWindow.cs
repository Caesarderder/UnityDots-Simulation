#if GRIFFIN
#if __MICROSPLAT_POLARIS__
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.MicroSplat
{
    public class GMicroSplatSetupWindow : EditorWindow
    {
        private HashSet<GStylizedTerrain> targets;
        private Vector2 scrollPos;

        [MenuItem("Window/Polaris/Tools/MicroSplat Integration")]
        public static void ShowWindow()
        {
            GMicroSplatSetupWindow window = GetWindow<GMicroSplatSetupWindow>();
            window.titleContent = new GUIContent("MicroSplat Setup");
            window.Show();
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            DrawInstructionGUI();
            DrawSetupGUI();
            EditorGUILayout.EndScrollView();
        }

        public void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "ms-integration-instruction";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUILayout.LabelField("This module is used for integration MicroSplat shader to Polaris terrain.");

                GEditorCommon.Header("Setup Steps");
                EditorGUILayout.LabelField("1. Select a directory for storing MicroSplat files, target terrains will share the same Texture Array Config and shader. Make sure the directory is an empty folder.");
                EditorGUILayout.LabelField("2. Pick a name for the generated shaders.");
                EditorGUILayout.LabelField("3. Drop the terrain game objects or the environment root into the selector box.");
                EditorGUILayout.LabelField("4. Click Setup.");

                GEditorCommon.Header("After Setup");
                EditorGUILayout.LabelField("1. Texture layers will be fetched from the Splat Prototype Group, for the first time only, if Init Texture Entries is on.");
                EditorGUILayout.LabelField("2. The Splat Prototype Group serves no purpose after this point. You have to do all material configs on MicroSplat side. Refer to its documentation for more info.");
                EditorGUILayout.LabelField("3. You can still use texturing tools to edit your terrains, Splat Control Maps will be sync between two systems.");
            });
        }

        public void DrawSetupGUI()
        {
            string label = "Setup";
            string id = "ms-integration-setup";

            GEditorCommon.Foldout(label, true, id, () =>
            {
                GMicroSplatIntegrationSettings settings = GMicroSplatIntegrationSettings.Instance;
                string dir = settings.DataDirectory;
                GEditorCommon.BrowseFolder("Directory", ref dir);
                settings.DataDirectory = dir;
                settings.ShaderNamePrefix = EditorGUILayout.TextField("Shader Name", settings.ShaderNamePrefix);
                settings.InitTextureEntries = EditorGUILayout.Toggle("Init Texture Entries", settings.InitTextureEntries);
                if (targets == null)
                {
                    targets = new HashSet<GStylizedTerrain>();
                }
                EditorGUILayout.LabelField("Target(s)", targets.Count.ToString());

                EditorGUI.indentLevel += 1;
                IEnumerator<GStylizedTerrain> iTargets = targets.GetEnumerator();
                while (iTargets.MoveNext())
                {
                    GStylizedTerrain t = iTargets.Current;
                    if (t == null)
                        continue;
                    EditorGUILayout.LabelField(" ", t.name, GEditorCommon.ItalicLabel);
                }
                EditorGUI.indentLevel -= 1;

                Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
                GameObject g = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r, "Drop a Game Object here", "t:GStylizedTerrain", true);
                if (g != null)
                {
                    GStylizedTerrain[] terrains = g.GetComponentsInChildren<GStylizedTerrain>();
                    for (int i = 0; i < terrains.Length; ++i)
                    {
                        targets.Add(terrains[i]);
                    }
                }

                if (GUILayout.Button("Setup"))
                {
                    GMicroSplatSetup.Setup(targets);
                }

                EditorUtility.SetDirty(settings);
            });
        }
    }
}
#endif
#endif
