#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    [CustomEditor(typeof(GPrefabPrototypeGroup))]
    public class GPrefabPrototypeGroupInspector : Editor
    {
        private GPrefabPrototypeGroup instance;
        private void OnEnable()
        {
            instance = target as GPrefabPrototypeGroup;
        }

        public override void OnInspectorGUI()
        {
            GPrefabPrototypeGroupInspectorDrawer.Create(instance).DrawGUI();
            EditorUtility.SetDirty(instance);
        }

        [MenuItem("CONTEXT/GPrefabPrototypeGroup/(Internal) Fix Missing Pine Prefab")]
        public static void FixMissingPinePrefab()
        {
            Object o = Selection.activeObject;
            if (o is GPrefabPrototypeGroup)
            {
                string[] prefabAssetPaths = new string[]
                {
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_00.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Pine_01.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Dead_Break.prefab"
                };

                GPrefabPrototypeGroup group = o as GPrefabPrototypeGroup;
                group.Prototypes.Clear();
                for (int i = 0; i < prefabAssetPaths.Length; ++i)
                {
                    GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPaths[i]);
                    group.Prototypes.Add(p);
                }
                EditorUtility.SetDirty(group);
            }
        }

        [MenuItem("CONTEXT/GPrefabPrototypeGroup/(Internal) Fix Missing Rock Prefab")]
        public static void FixMissingRockPrefab()
        {
            Object o = Selection.activeObject;
            if (o is GPrefabPrototypeGroup)
            {
                string[] prefabAssetPaths = new string[]
                {
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Rock.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Rock1.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Rock2.prefab",
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Samples/Pinwheel Studio/Prefabs/Rock3.prefab"
                };

                GPrefabPrototypeGroup group = o as GPrefabPrototypeGroup;
                group.Prototypes.Clear();
                for (int i = 0; i < prefabAssetPaths.Length; ++i)
                {
                    GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPaths[i]);
                    group.Prototypes.Add(p);
                }
                EditorUtility.SetDirty(group);
            }
        }
    }
}
#endif
