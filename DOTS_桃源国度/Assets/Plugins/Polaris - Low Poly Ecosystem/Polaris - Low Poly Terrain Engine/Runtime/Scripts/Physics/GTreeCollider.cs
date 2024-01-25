#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Linq;

namespace Pinwheel.Griffin.Physic
{
    [ExecuteInEditMode]
    public class GTreeCollider : MonoBehaviour
    {
        [SerializeField]
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            set
            {
                GStylizedTerrain oldValue = terrain;
                GStylizedTerrain newValue = value;
                terrain = newValue;
                if (oldValue != newValue)
                {
                    InitTreeInstances();
                }
            }
        }

        [SerializeField]
        private GameObject target;
        public GameObject Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }

        [SerializeField]
        private float distance;
        public float Distance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private bool copyTreeTag;
        public bool CopyTreeTag
        {
            get
            {
                return copyTreeTag;
            }
            set
            {
                copyTreeTag = value;
            }
        }

        private List<CapsuleCollider> colliders;
        private List<CapsuleCollider> Colliders
        {
            get
            {
                if (colliders == null)
                {
                    GUtilities.ClearChildren(transform);
                    colliders = new List<CapsuleCollider>();
                }
                return colliders;
            }
        }

        private NativeArray<GTreeInstance> nativeTreeInstances;
        private GTreeInstance[] treeInstances;

        private NativeArray<bool> nativeCullResults;
        private bool[] cullResults;

        private void OnEnable()
        {
            GTerrainData.GlobalDirty += OnTerrainDataDirty;
            InitTreeInstances();
        }

        private void OnDisable()
        {
            GTerrainData.GlobalDirty -= OnTerrainDataDirty;
            CleanUp();
        }

        private void CleanUp()
        {
            GNativeArrayUtilities.Dispose(nativeTreeInstances);
            GNativeArrayUtilities.Dispose(nativeCullResults);
        }

        private void OnTerrainDataDirty(GTerrainData data, GTerrainData.DirtyFlags flag)
        {
            if (Terrain != null &&
                Terrain.TerrainData == data)
            {
                if (flag == GTerrainData.DirtyFlags.All ||
                    flag == GTerrainData.DirtyFlags.Foliage)
                {
                    InitTreeInstances();
                }
            }
        }

        private void InitTreeInstances()
        {
            CleanUp();
            if (Terrain == null)
                return;
            if (Terrain.TerrainData == null)
                return;

            List<GTreeInstance> instances = Terrain.TerrainData.Foliage.TreeInstances;
            nativeTreeInstances = new NativeArray<GTreeInstance>(instances.ToArray(), Allocator.Persistent);
            treeInstances = new GTreeInstance[instances.Count];

            nativeCullResults = new NativeArray<bool>(instances.Count, Allocator.Persistent);
            cullResults = new bool[instances.Count];

            Vector3 terrainSize = Terrain.TerrainData.Geometry.Size;
            GTransformTreesToLocalSpaceJob job = new GTransformTreesToLocalSpaceJob()
            {
                instances = nativeTreeInstances,
                terrainSize = terrainSize
            };

            JobHandle handle = job.Schedule(nativeTreeInstances.Length, 100);
            handle.Complete();

            nativeTreeInstances.CopyTo(treeInstances);
        }

        public void Reset()
        {
            Terrain = GetComponentInParent<GStylizedTerrain>();
            if (Terrain == null)
                Terrain = GetComponent<GStylizedTerrain>();
            Target = null;
            Distance = 50;
            CopyTreeTag = false;
            InitTreeInstances();
        }

        private void LateUpdate()
        {
            if (Terrain == null)
                return;
            if (Terrain.TerrainData == null)
                return;
            if (Terrain.TerrainData.Foliage.Trees == null)
                return;
            if (Terrain.TerrainData.Foliage.Trees.Prototypes.Count == 0)
                return;
            if (treeInstances == null || treeInstances.Length == 0)
                return;

            GameObject actualTarget = null;
            if (Target != null)
                actualTarget = Target;
            else if (Camera.main != null)
                actualTarget = Camera.main.gameObject;

            if (actualTarget == null)
                return;
            Vector3 targetLocalPos = Terrain.transform.InverseTransformPoint(actualTarget.transform.position);
            GTreeColliderCullJob job = new GTreeColliderCullJob()
            {
                instances = nativeTreeInstances,
                cullResults = nativeCullResults,
                maxDistance = distance,
                targetPos = targetLocalPos
            };
            JobHandle handle = job.Schedule(nativeTreeInstances.Length, 100);
            handle.Complete();

            if (cullResults == null || cullResults.Length != nativeCullResults.Length)
            {
                cullResults = new bool[nativeCullResults.Length];
            }

            nativeCullResults.CopyTo(cullResults);

            List<GTreePrototype> prototypes = Terrain.TerrainData.Foliage.Trees.Prototypes;
            int colliderIndex = 0;
            Vector3 terrainPos = Terrain.transform.position;
            Vector3 worldPos = Vector3.zero;
            if (terrain.TerrainData.Rendering.DrawTrees)
            {
                for (int i = 0; i < treeInstances.Length; ++i)
                {
                    if (cullResults[i] == false)
                        continue;

                    GTreeInstance tree = treeInstances[i];
                    if (tree.prototypeIndex < 0 || tree.prototypeIndex >= prototypes.Count)
                        continue;

                    GTreePrototype prototype = prototypes[tree.prototypeIndex];
                    if (prototype.prefab == null)
                        continue;
                    if (!prototype.hasCollider)
                        continue;

                    CapsuleCollider col = GetCollider(colliderIndex);
                    colliderIndex += 1;

                    worldPos.Set(
                        tree.position.x + terrainPos.x,
                        tree.position.y + terrainPos.y,
                        tree.position.z + terrainPos.z);
                    col.transform.position = worldPos;
                    col.transform.rotation = tree.rotation;
                    col.transform.localScale = tree.scale;
                    GTreeColliderInfo colliderInfo = prototype.colliderInfo;
                    col.center = colliderInfo.center;
                    col.radius = colliderInfo.radius;
                    col.height = colliderInfo.height;
                    col.direction = colliderInfo.direction;
                    col.gameObject.layer = prototype.layer;
                    if (CopyTreeTag)
                    {
                        col.gameObject.tag = prototype.prefab.tag;
                    }
                    col.gameObject.SetActive(true);
                }
            }

            int colliderCount = Colliders.Count;
            for (int i = colliderIndex; i < colliderCount; ++i)
            {
                CapsuleCollider col = GetCollider(i);
                col.gameObject.SetActive(false);
            }
        }

        private CapsuleCollider GetCollider(int index)
        {
            while (index >= Colliders.Count)
            {
                Colliders.Add(null);
            }
            if (Colliders[index] == null)
            {
                GameObject g = new GameObject("Collider_" + index.ToString());
                GUtilities.ResetTransform(g.transform, transform);

                CapsuleCollider col = g.AddComponent<CapsuleCollider>();
                Colliders[index] = col;
                g.hideFlags = HideFlags.DontSave;
            }
            return Colliders[index];
        }
    }
}
#endif
