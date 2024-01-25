#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GSplatPrototypeGroupInspectorDrawer
    {
        private GSplatPrototypeGroup instance;

        public GSplatPrototypeGroupInspectorDrawer(GSplatPrototypeGroup group)
        {
            instance = group;
        }

        public static GSplatPrototypeGroupInspectorDrawer Create(GSplatPrototypeGroup group)
        {
            return new GSplatPrototypeGroupInspectorDrawer(group);
        }

        public void DrawGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawPrototypesListGUI();
            DrawAddPrototypeGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SetShadingDirty();
                EditorUtility.SetDirty(instance);
            }

            GEditorCommon.DrawAffLinks(
                "Explore the high quality textures for stunning low poly scenes",
                "https://assetstore.unity.com/packages/2d/textures-materials/600-super-texture-collection-197591",
                "https://assetstore.unity.com/lists/stylized-texture-18967160722750");
        }

        private void DrawPrototypesListGUI()
        {
            for (int i = 0; i < instance.Prototypes.Count; ++i)
            {
                GSplatPrototype p = instance.Prototypes[i];

                string label = p.Texture != null && !string.IsNullOrEmpty(p.Texture.name) ? p.Texture.name : "Splat " + i;
                string id = "splat" + i + instance.GetInstanceID().ToString();

                int index = i;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("Remove"),
                    false,
                    () => { ConfirmAndRemovePrototypeAtIndex(index); });

                GEditorCommon.Foldout(label, false, id, () =>
                {
                    p.Texture = EditorGUILayout.ObjectField("Texture", p.Texture, typeof(Texture2D), false) as Texture2D;
                    p.NormalMap = EditorGUILayout.ObjectField("Normal Map", p.NormalMap, typeof(Texture2D), false) as Texture2D;
                    p.TileSize = EditorGUILayout.Vector2Field("Tile Size", p.TileSize);
                    p.TileOffset = EditorGUILayout.Vector2Field("Tile Offset", p.TileOffset);
                    p.Metallic = EditorGUILayout.Slider("Metallic", p.Metallic, 0f, 1f);
                    p.Smoothness = EditorGUILayout.Slider("Smoothness", p.Smoothness, 0f, 1f);
                }, menu);
            }
        }

        private void ConfirmAndRemovePrototypeAtIndex(int index)
        {
            GSplatPrototype p = instance.Prototypes[index];
            string label = p.Texture != null ? p.Texture.name : "Splat " + index;
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "Remove " + label,
                "OK", "Cancel"))
            {
                instance.Prototypes.RemoveAt(index);
            }
        }

        private void DrawAddPrototypeGUI()
        {
            EditorGUILayout.GetControlRect(GUILayout.Height(1));
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(GEditorCommon.objectSelectorDragDropHeight));
            Texture2D t = GEditorCommon.ObjectSelectorDragDrop<Texture2D>(r, "Drop a texture here!", "t:Texture2D");
            if (t != null)
            {
                GSplatPrototype p = new GSplatPrototype();
                p.Texture = t;
                instance.Prototypes.Add(p);
            }
        }

        private void SetShadingDirty()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (t.TerrainData != null)
                {
                    t.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
            }
        }
    }
}
#endif
