#if GRIFFIN
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Pinwheel.Griffin.PaintTool
{
    public class GPrefabPrototypeGroupInspectorDrawer
    {
        private GPrefabPrototypeGroup instance;
        private GPrefabPrototypeGroupInspectorDrawer(GPrefabPrototypeGroup instance)
        {
            this.instance = instance;
        }

        public static GPrefabPrototypeGroupInspectorDrawer Create(GPrefabPrototypeGroup instance)
        {
            return new GPrefabPrototypeGroupInspectorDrawer(instance);
        }

        public void DrawGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawPrototypesListGUI();
            DrawAddPrototypeGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(instance);
            }

            GEditorCommon.DrawAffLinks(
                "These rocks, props and models can help your scene to be more compelling",
                "https://assetstore.unity.com/packages/3d/props/low-poly-ultimate-pack-54733",
                "https://assetstore.unity.com/lists/stylized-rock-props-120083");

            GEditorCommon.Separator();
            DrawConvertAssetGUI();
        }

        private void DrawPrototypesListGUI()
        {
            instance.Prototypes.RemoveAll(g => g == null);
            CachePrefabPath();
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GameObject g = instance.Prototypes[i];
                if (g == null)
                    continue;

                string label = g.name;
                string id = "treeprototype" + i + instance.GetInstanceID().ToString();

                int index = i;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"),
                    false,
                    () => { ConfirmAndRemovePrototypeAtIndex(index); });

                GEditorCommon.Foldout(label, false, id, () =>
                {
                    DrawPreview(g);
                }, menu);
            }
        }

        private void CachePrefabPath()
        {
            instance.Editor_PrefabAssetPaths.Clear();
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                if (instance.Prototypes[i] == null)
                {
                    instance.Editor_PrefabAssetPaths[i] = string.Empty;
                }
                else
                {
                    instance.Editor_PrefabAssetPaths[i] = AssetDatabase.GetAssetPath(instance.Prototypes[i]);
                }
            }
        }

        private void ConfirmAndRemovePrototypeAtIndex(int index)
        {
            GameObject g = instance.Prototypes[index];
            string label = g.name;
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove " + label,
                "OK", "Cancel"))
            {
                instance.Prototypes.RemoveAt(index);
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
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            GameObject g = GEditorCommon.ObjectSelectorDragDrop<GameObject>(r, "Drop a Game Object here!", "t:GameObject");
            if (g != null)
            {
                if (!instance.Prototypes.Contains(g))
                {
                    instance.Prototypes.Add(g);
                }
            }
        }

        private void DrawConvertAssetGUI()
        {
            if (GUILayout.Button("Create Tree Prototype Group"))
            {
                ConvertToTreePrototypeGroup();
            }
            if (GUILayout.Button("Create Grass/Detail Object Prototype Group"))
            {
                ConvertToGrassPrototypeGroup();
            }
        }

        private void ConvertToTreePrototypeGroup()
        {
            GTreePrototypeGroup group = ScriptableObject.CreateInstance<GTreePrototypeGroup>();
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GameObject prefab = instance.Prototypes[i];
                if (prefab != null)
                {
                    group.Prototypes.Add(GTreePrototype.Create(prefab));
                }
            }

            string path = AssetDatabase.GetAssetPath(instance);
            string directory = Path.GetDirectoryName(path);
            string filePath = Path.Combine(directory, string.Format("{0}_{1}_{2}.asset", instance.name, "Trees", GCommon.GetUniqueID()));
            AssetDatabase.CreateAsset(group, filePath);

            Selection.activeObject = group;
        }

        private void ConvertToGrassPrototypeGroup()
        {
            GGrassPrototypeGroup group = ScriptableObject.CreateInstance<GGrassPrototypeGroup>();
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GameObject prefab = instance.Prototypes[i];
                if (prefab != null)
                {
                    group.Prototypes.Add(GGrassPrototype.Create(prefab));
                }
            }

            string path = AssetDatabase.GetAssetPath(instance);
            string directory = Path.GetDirectoryName(path);
            string filePath = Path.Combine(directory, string.Format("{0}_{1}_{2}.asset", instance.name, "DetailObjects", GCommon.GetUniqueID()));
            AssetDatabase.CreateAsset(group, filePath);

            Selection.activeObject = group;
        }
    }
}
#endif
