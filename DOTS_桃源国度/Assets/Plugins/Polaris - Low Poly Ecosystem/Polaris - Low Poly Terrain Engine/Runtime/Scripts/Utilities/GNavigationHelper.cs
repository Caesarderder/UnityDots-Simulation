#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Griffin
{
    public class GNavigationHelper : MonoBehaviour
    {
        [SerializeField]
        private int groupId;
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        public void CreateStaticObstacles()
        {
            transform.root.position = Vector3.zero;
            transform.root.rotation = Quaternion.identity;
            transform.root.localScale = Vector3.one;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            try
            {
                IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
                while (terrains.MoveNext())
                {
                    GStylizedTerrain t = terrains.Current;
                    if (groupId < 0 ||
                        (groupId >= 0 && groupId == t.GroupId))
                    {
                        CreateStaticObstacles(t);
                    }
                }
            }
            catch (GProgressCancelledException)
            {
                Debug.Log("Create static obstacles process canceled!");
#if UNITY_EDITOR
                GCommonGUI.ClearProgressBar();
#endif
            }
        }

        private void CreateStaticObstacles(GStylizedTerrain t)
        {
            if (t.TerrainData == null)
                return;
            if (t.TerrainData.Foliage.Trees == null)
                return;
            if (t.TerrainData.Foliage.Trees.Prototypes.Count == 0)
                return;

#if UNITY_EDITOR
            string title = "Creating static obstacles";
            string info = t.name;
            GCommonGUI.CancelableProgressBar(title, info, 0);
#endif

            Vector3 terrainSize = new Vector3(
                t.TerrainData.Geometry.Width,
                t.TerrainData.Geometry.Height,
                t.TerrainData.Geometry.Length);

            Transform root = GUtilities.GetChildrenWithName(transform, string.Format("{0}_{1}", t.name, t.GetInstanceID()));
            List<GTreePrototype> prototypes = t.TerrainData.Foliage.Trees.Prototypes;

            GameObject[] templates = new GameObject[prototypes.Count];
            for (int i = 0; i < prototypes.Count; ++i)
            {
                if (!prototypes[i].IsValid)
                    continue;
                GameObject template = Instantiate(prototypes[i].Prefab) as GameObject;
                Component[] components = template.GetComponentsInChildren<Component>();
                for (int j = 0; j < components.Length; ++j)
                {
                    if (components[j] is Collider)
                        GUtilities.DestroyObject(components[j]);
                    if (components[j] is MeshRenderer)
                    {
                        MeshRenderer mr = components[j] as MeshRenderer;
                        mr.sharedMaterials = new Material[] { GInternalMaterials.NavHelperDummyGameObjectMaterial };
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                    }
                }
#if UNITY_EDITOR && !UNITY_2022_2_OR_NEWER
                GameObjectUtility.SetStaticEditorFlags(template, StaticEditorFlags.NavigationStatic);
#endif
                template.name = prototypes[i].Prefab.name;
                templates[i] = template;
            }

            List<GTreeInstance> instances = t.TerrainData.Foliage.TreeInstances;
            for (int i = 0; i < instances.Count; ++i)
            {
#if UNITY_EDITOR
                GCommonGUI.CancelableProgressBar(title, info, i * 1.0f / instances.Count);
#endif
                GTreeInstance tree = instances[i];
                if (templates[tree.PrototypeIndex] == null)
                    continue;

                GameObject g = Instantiate(templates[tree.PrototypeIndex]) as GameObject;
                g.transform.parent = root;

                Vector3 localPos = new Vector3(
                    tree.Position.x * terrainSize.x,
                    tree.Position.y * terrainSize.y,
                    tree.Position.z * terrainSize.z);
                Vector3 worldPos = t.transform.TransformPoint(localPos);
                g.transform.position = worldPos;
                g.transform.rotation = tree.Rotation;
                g.transform.localScale = tree.Scale;
                g.name = templates[tree.PrototypeIndex].name;
            }

            for (int i = 0; i < templates.Length; ++i)
            {
                GUtilities.DestroyGameobject(templates[i]);
            }
#if UNITY_EDITOR
            GCommonGUI.ClearProgressBar();
#endif
        }

        public void DeleteStaticObstacles()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                GStylizedTerrain t = terrains.Current;
                if (groupId < 0 ||
                    (groupId >= 0 && groupId == t.GroupId))
                {
                    DeleteStaticObstacles(t);
                }
            }
        }

        private void DeleteStaticObstacles(GStylizedTerrain t)
        {
            Transform root = GUtilities.GetChildrenWithName(transform, string.Format("{0}_{1}", t.name, t.GetInstanceID()));
            GUtilities.DestroyGameobject(root.gameObject);
        }
    }
}
#endif
