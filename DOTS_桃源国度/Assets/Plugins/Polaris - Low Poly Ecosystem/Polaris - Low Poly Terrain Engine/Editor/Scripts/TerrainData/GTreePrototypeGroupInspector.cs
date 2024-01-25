#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GTreePrototypeGroup))]
    public class GTreePrototypeGroupInspector : Editor
    {
        private GTreePrototypeGroup instance;

        private void OnEnable()
        {
            instance = target as GTreePrototypeGroup;
        }

        public override void OnInspectorGUI()
        {
            GTreePrototypeGroupInspectorDrawer.Create(instance).DrawGUI();
            EditorUtility.SetDirty(instance);
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        [MenuItem("CONTEXT/GTreePrototypeGroup/Refresh Prototypes")]
        public static void RefreshPrototypes()
        {
            Object o = Selection.activeObject;
            if (o is GTreePrototypeGroup)
            {
                GTreePrototypeGroup group = o as GTreePrototypeGroup;
                for (int i = 0; i < group.Prototypes.Count; ++i)
                {
                    group.Prototypes[i].Refresh();
                }
                EditorUtility.SetDirty(group);
            }
        }

        [MenuItem("CONTEXT/GTreePrototypeGroup/(Internal) Fix Missing Sample Trees")]
        public static void FixMissingSampleTrees()
        {
            Object o = Selection.activeObject;
            if (o is GTreePrototypeGroup)
            {
                string[] prefabAssetPaths = new string[]
                {
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/AutumnTree1.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/SpringTree1.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_00.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_01.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead_Break.prefab"
                };

                GTreePrototypeGroup group = o as GTreePrototypeGroup;
                group.Prototypes.Clear();
                for (int i = 0; i < prefabAssetPaths.Length; ++i)
                {
                    GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPaths[i]);
                    GTreePrototype proto = GTreePrototype.Create(p);
                    proto.Refresh();
                    group.Prototypes.Add(proto);
                }

                EditorUtility.SetDirty(group);
            }
        }

        [MenuItem("CONTEXT/GTreePrototypeGroup/(Internal) Fix missing trees Demo_00")]
        public static void FixMissingTreeDemo00()
        {
            Object o = Selection.activeObject;
            if (o is GTreePrototypeGroup)
            {
                string[] prefabAssetPaths = new string[]
                {
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_00.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_01.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead_Break.prefab"
                };

                GTreePrototypeGroup group = o as GTreePrototypeGroup;
                group.Prototypes.Clear();
                for (int i = 0; i < prefabAssetPaths.Length; ++i)
                {
                    GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPaths[i]);
                    GTreePrototype proto = GTreePrototype.Create(p);
                    proto.Refresh();
                    group.Prototypes.Add(proto);
                }

                EditorUtility.SetDirty(group);
            }
        }

        [MenuItem("CONTEXT/GTreePrototypeGroup/(Internal) Fix missing trees Demo_02")]
        public static void FixMissingTreeDemo02()
        {
            Object o = Selection.activeObject;
            if (o is GTreePrototypeGroup)
            {
                string[] prefabAssetPaths = new string[]
                {
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/SpringTree1.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/AutumnTree3.prefab",
                };

                GTreePrototypeGroup group = o as GTreePrototypeGroup;
                group.Prototypes.Clear();
                for (int i = 0; i < prefabAssetPaths.Length; ++i)
                {
                    GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPaths[i]);
                    GTreePrototype proto = GTreePrototype.Create(p);
                    proto.Refresh();
                    group.Prototypes.Add(proto);
                }

                EditorUtility.SetDirty(group);
            }
        }
    }
}
#endif
