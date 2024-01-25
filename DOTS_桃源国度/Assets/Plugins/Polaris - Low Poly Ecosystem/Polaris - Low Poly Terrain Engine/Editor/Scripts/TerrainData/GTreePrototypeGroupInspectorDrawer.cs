#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

namespace Pinwheel.Griffin
{
    public class GTreePrototypeGroupInspectorDrawer
    {
        private GTreePrototypeGroup instance;

        public GTreePrototypeGroupInspectorDrawer(GTreePrototypeGroup group)
        {
            instance = group;
        }

        public static GTreePrototypeGroupInspectorDrawer Create(GTreePrototypeGroup group)
        {
            return new GTreePrototypeGroupInspectorDrawer(group);
        }

        public void DrawGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawPrototypesListGUI();
            DrawAddPrototypeGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SetFoliageDirty();
                EditorUtility.SetDirty(instance);
            }

            GEditorCommon.DrawAffLinks(
                "These vivid vegetations can breath life into your project",
                "https://assetstore.unity.com/packages/3d/vegetation/trees/polygon-nature-low-poly-3d-art-by-synty-120152",
                "https://assetstore.unity.com/lists/stylized-vegetation-120082");

            GEditorCommon.Separator();
            DrawConvertAssetGUI();
        }

        private void DrawPrototypesListGUI()
        {
            string label, id;
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GTreePrototype p = instance.Prototypes[i];
                CachePrefabPath(p);

                label = p.Prefab != null && !string.IsNullOrEmpty(p.Prefab.name) ? p.Prefab.name : "Tree " + i;
                id = "treeprototype" + i + instance.GetInstanceID().ToString();

                int index = i;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"),
                    false,
                    () => { ConfirmAndRemovePrototypeAtIndex(index); });
                menu.AddItem(
                    new GUIContent("Sync with Prefab"),
                    false,
                    () => { p.Refresh(); });

                GEditorCommon.Foldout(label, false, id, () =>
                {
                    if (p.Prefab != null)
                    {
                        DrawPreview(p.Prefab);
                    }

                    p.Prefab = EditorGUILayout.ObjectField("Prefab", p.Prefab, typeof(GameObject), false) as GameObject;
                    p.Billboard = EditorGUILayout.ObjectField("Billboard", p.Billboard, typeof(BillboardAsset), false) as BillboardAsset;

                    EditorGUI.BeginChangeCheck();
                    p.PivotOffset = EditorGUILayout.Slider("Pivot Offset", p.PivotOffset, -1f, 1f);
                    p.BaseRotation = Quaternion.Euler(GEditorCommon.InlineVector3Field("Base Rotation", p.BaseRotation.eulerAngles));
                    p.BaseScale = GEditorCommon.InlineVector3Field("Base Scale", p.BaseScale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ResetNativeArrays();
                    }
                    GUI.enabled = !p.KeepPrefabLayer;
                    p.Layer = EditorGUILayout.LayerField("Layer", p.Layer);
                    GUI.enabled = true;
                    p.KeepPrefabLayer = EditorGUILayout.Toggle("Keep Prefab Layer", p.KeepPrefabLayer);

                    p.ShadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Cast Shadow", p.ShadowCastingMode);
                    p.ReceiveShadow = EditorGUILayout.Toggle("Receive Shadow", p.ReceiveShadow);

                    p.BillboardShadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Billboard Cast Shadow", p.BillboardShadowCastingMode);
                    p.BillboardReceiveShadow = EditorGUILayout.Toggle("Billboard Receive Shadow", p.BillboardReceiveShadow);

                    GUI.enabled = false;
                    EditorGUILayout.Toggle("Has Collider", p.HasCollider);
                    GUI.enabled = true;
                }, menu);
            }
        }

        private void CachePrefabPath(GTreePrototype p)
        {
            if (p.Prefab == null)
            {
                p.Editor_PrefabAssetPath = null;
            }
            else
            {
                p.Editor_PrefabAssetPath = AssetDatabase.GetAssetPath(p.Prefab);
            }
        }

        private void ConfirmAndRemovePrototypeAtIndex(int index)
        {
            GTreePrototype p = instance.Prototypes[index];
            string label = p.Prefab != null ? p.Prefab.name : "Tree " + index;
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
                GTreePrototype p = GTreePrototype.Create(g);
                instance.Prototypes.Add(p);
            }
        }

        private void ResetNativeArrays()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null && t.TerrainData.Foliage.Trees == instance)
                {
                    t.TerrainData.Foliage.TreeAllChanged();
                }
            }
        }

        private void SetFoliageDirty()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null)
                {
                    t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
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
