#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

namespace Pinwheel.Griffin
{
    public class GGrassPrototypeGroupInspectorDrawer
    {
        private GGrassPrototypeGroup instance;

        public GGrassPrototypeGroupInspectorDrawer(GGrassPrototypeGroup group)
        {
            instance = group;
        }

        public static GGrassPrototypeGroupInspectorDrawer Create(GGrassPrototypeGroup group)
        {
            return new GGrassPrototypeGroupInspectorDrawer(group);
        }

        public void DrawGUI()
        {
            DrawInstruction();
            DrawPrototypesListGUI();
            DrawAddPrototypeGUI();

            GEditorCommon.DrawAffLinks(
                "These lively vegetation assets can bring your project to life",
                "https://assetstore.unity.com/packages/3d/vegetation/trees/polygon-nature-low-poly-3d-art-by-synty-120152",
                "https://assetstore.unity.com/lists/stylized-vegetation-120082");

            GEditorCommon.Separator();
            DrawConvertAssetGUI();
        }

        private void DrawInstruction()
        {
            string label = "Instruction";
            string id = "instruction" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, false, id, () =>
            {
                string text = string.Format(
                    "Some properties require Foliage Data to be processed on a terrain to take effect.\n" +
                    "Go to Terrain > Foliage > CONTEXT > Update Grasses to do it.");
                EditorGUILayout.LabelField(text, GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawPrototypesListGUI()
        {
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GGrassPrototype p = instance.Prototypes[i];

                string label = string.Empty;
                if (p.Shape != GGrassShape.DetailObject)
                    label = p.Texture != null && !string.IsNullOrEmpty(p.Texture.name) ? p.Texture.name : "Grass " + i;
                else
                    label = p.Prefab != null && !string.IsNullOrEmpty(p.Prefab.name) ? p.Prefab.name : "Grass " + i;
                string id = "grassprototype" + i + instance.GetInstanceID().ToString();

                int index = i;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"),
                    false,
                    () => { ConfirmAndRemovePrototypeAtIndex(index); });

                GEditorCommon.Foldout(label, false, id, () =>
                {
                    EditorGUI.BeginChangeCheck();
                    if (p.Shape != GGrassShape.DetailObject)
                    {
                        p.Texture = EditorGUILayout.ObjectField("Texture", p.Texture, typeof(Texture2D), false) as Texture2D;
                        p.Shape = (GGrassShape)EditorGUILayout.EnumPopup("Shape", p.Shape);
                        if (p.Shape == GGrassShape.CustomMesh)
                        {
                            p.CustomMesh = EditorGUILayout.ObjectField("Mesh", p.CustomMesh, typeof(Mesh), false) as Mesh;
                        }
                    }
                    else
                    {
                        DrawPreview(p.Prefab);
                        p.Shape = (GGrassShape)EditorGUILayout.EnumPopup("Shape", p.Shape);
                        p.Prefab = EditorGUILayout.ObjectField("Prefab", p.Prefab, typeof(GameObject), false) as GameObject;
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField("Material", p.DetailMaterial, typeof(Material), false);
                        GUI.enabled = true;
                    }
                    p.Color = EditorGUILayout.ColorField("Color", p.Color);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(instance);
                    }

                    EditorGUI.BeginChangeCheck();
                    p.Size = GEditorCommon.InlineVector3Field("Size", p.Size);
                    p.PivotOffset = EditorGUILayout.DelayedFloatField("Pivot Offset", p.PivotOffset);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ResetNativeArrays();
                        EditorUtility.SetDirty(instance);
                    }

                    p.BendFactor = EditorGUILayout.FloatField("Bend Factor", p.BendFactor);
                    p.Layer = EditorGUILayout.LayerField("Layer", p.Layer);
                    p.AlignToSurface = EditorGUILayout.Toggle("Align To Surface", p.AlignToSurface);
                    p.ShadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Cast Shadow", p.ShadowCastingMode);
                    p.ReceiveShadow = EditorGUILayout.Toggle("Receive Shadow", p.ReceiveShadow);

                    if (p.Shape != GGrassShape.DetailObject)
                    {
                        p.IsBillboard = EditorGUILayout.Toggle("Billboard", p.IsBillboard);
                    }

                    if (p.Shape == GGrassShape.DetailObject)
                    {
                        EditorGUILayout.LabelField("Detail Object uses the first sub-mesh and material found in its prefab, and may NOT affected by wind.", GEditorCommon.WarningLabel);

                        if (p.DetailMaterial!=null && !p.DetailMaterial.enableInstancing)
                        {
                            EditorGUILayout.LabelField("Prototype's material has GPU Instancing option turned off. This prototype will not be rendered.", GEditorCommon.WarningLabel);
                        }
                    }

                    if (p.Shape != GGrassShape.DetailObject && p.IsBillboard)
                    {
                        EditorGUILayout.LabelField("Billboard will not work if Interactive Grass is enabled.", GEditorCommon.WarningLabel);
                    }
                },
                menu);
            }
        }

        private void ConfirmAndRemovePrototypeAtIndex(int index)
        {
            GGrassPrototype p = instance.Prototypes[index];
            string label = p.Texture != null ? p.Texture.name : "Grass " + index;
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove " + label,
                "OK", "Cancel"))
            {
                instance.Prototypes.RemoveAt(index);
                RefreshInstanceList();
                EditorUtility.SetDirty(instance);
            }
        }

        private void DrawPreview(GameObject g)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.selectionGridTileSizeMedium.y));
            GEditorCommon.DrawPreview(r, g);
        }

        private void DrawAddPrototypeGUI()
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            Rect r0 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            Texture2D t = GEditorCommon.ObjectSelectorDragDrop<Texture2D>(r0, "Drop a Texture here!", "t:Texture2D");
            if (t != null)
            {
                GGrassPrototype g = GGrassPrototype.Create(t);
                instance.Prototypes.Add(g);
                RefreshInstanceList();
                EditorUtility.SetDirty(instance);
            }

            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            Rect r1 = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GameObject prefab = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r1, "Drop a Game Object here!", "t:GameObject");
            if (prefab != null)
            {
                GGrassPrototype p = GGrassPrototype.Create(prefab);
                instance.Prototypes.Add(p);
                RefreshInstanceList();
                EditorUtility.SetDirty(instance);
            }
        }

        private void RefreshInstanceList()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null && t.TerrainData.Foliage.Grasses == instance)
                {
                    t.TerrainData.Foliage.Refresh();
                }
            }
        }

        private void ResetNativeArrays()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null && t.TerrainData.Foliage.Grasses == instance)
                {
                    t.TerrainData.Foliage.GrassAllChanged();
                }
            }
        }

        private void DrawConvertAssetGUI()
        {
            if (GUILayout.Button("Create Prefab Prototype Group"))
            {
                ConvertToPrefabPrototypeGroup();
            }
        }

        private void ConvertToPrefabPrototypeGroup()
        {
            GPrefabPrototypeGroup group = ScriptableObject.CreateInstance<GPrefabPrototypeGroup>();
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                if (instance.Prototypes[i].Shape != GGrassShape.DetailObject)
                    continue;
                GameObject prefab = instance.Prototypes[i].Prefab;
                if (prefab != null)
                {
                    group.Prototypes.Add(prefab);
                }
            }

            string path = AssetDatabase.GetAssetPath(instance);
            string directory = Path.GetDirectoryName(path);
            string filePath = Path.Combine(directory, string.Format("{0}_{1}_{2}.asset", instance.name, "Prefabs", GCommon.GetUniqueID()));
            AssetDatabase.CreateAsset(group, filePath);

            Selection.activeObject = group;
        }
    }
}
#endif
