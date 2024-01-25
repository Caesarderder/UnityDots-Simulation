#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin
{
    public static class GSpawner
    {
        public static string GetPrototypeRootName(GameObject prototype)
        {
            return string.Format("~Root_{0}", prototype.name);
        }

        public static GameObject Spawn(GStylizedTerrain terrain, GameObject prototype, Vector3 worldPos, Transform parent = null)
        {
            GameObject g = null;
#if UNITY_EDITOR
            bool isPrefab = false;
#if UNITY_2018_1 || UNITY_2018_2
            isPrefab = PrefabUtility.GetPrefabObject(prototype);
#else
            isPrefab = PrefabUtility.IsPartOfPrefabAsset(prototype);
#endif

            if (isPrefab)
            {
                g = PrefabUtility.InstantiatePrefab(prototype) as GameObject;
            }
            else
            {
                g = GameObject.Instantiate<GameObject>(prototype);
            }

            string undoName = string.Format("Spawn {0}", prototype.name);
            Undo.RegisterCreatedObjectUndo(g, undoName);
#else
            g = GameObject.Instantiate<GameObject>(prototype);
#endif
            if (parent == null)
            {
                parent = GetRoot(terrain, prototype).transform;
            }
            g.transform.parent = parent;

            g.transform.position = worldPos;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            return g;
        }

        public static void DestroyIf(GStylizedTerrain terrain, GameObject prototype, System.Predicate<GameObject> condition)
        {
            Transform parent = GetRoot(terrain, prototype).transform;
            GameObject[] children = GUtilities.GetChildrenGameObjects(parent);
            for (int i = 0; i < children.Length; ++i)
            {
                if (condition.Invoke(children[i]))
                {
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(children[i]);
#else
                    GUtilities.DestroyGameobject(children[i]);
#endif
                }
            }
        }

        public static void DestroyIf(GStylizedTerrain terrain, System.Predicate<GameObject> condition)
        {
            List<Transform> parents = new List<Transform>();
            foreach (Transform t in terrain.transform)
            {
                if (t.name.StartsWith("~Root"))
                {
                    parents.Add(t);
                }
            }

            for (int i = 0; i < parents.Count; ++i)
            {
                GameObject[] children = GUtilities.GetChildrenGameObjects(parents[i]);
                for (int j = 0; j < children.Length; ++j)
                {
                    if (condition.Invoke(children[j]))
                    {
#if UNITY_EDITOR
                        Undo.DestroyObjectImmediate(children[j]);
#else
                    GUtilities.DestroyGameobject(children[j]);
#endif
                    }
                }
            }
        }

        public static GameObject GetRoot(GStylizedTerrain terrain, GameObject prototype)
        {
            Transform parent = terrain.transform;
            string name = GetPrototypeRootName(prototype);

            Transform t = parent.Find(name);
            if (t == null)
            {
                GameObject g = new GameObject(name);
                g.transform.parent = parent;
                GUtilities.ResetTransform(g.transform, parent);
                t = g.transform;

                GObjectHelper oh = g.AddComponent<GObjectHelper>();
                oh.Terrain = terrain;
            }
            return t.gameObject;
        }
    }
}
#endif
