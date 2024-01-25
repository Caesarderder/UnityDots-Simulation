#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin
{
    //[CreateAssetMenu(menuName = "Griffin/Generated Data")]
    [PreferBinarySerialization]
    public class GTerrainGeneratedData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private GTerrainData terrainData;
        public GTerrainData TerrainData
        {
            get
            {
                return terrainData;
            }
            internal set
            {
                terrainData = value;
            }
        }

        private Dictionary<string, Mesh> generatedMeshes;
        private Dictionary<string, Mesh> GeneratedMeshes
        {
            get
            {
                if (generatedMeshes == null)
                    generatedMeshes = new Dictionary<string, Mesh>();
                return generatedMeshes;
            }
        }

        private Dictionary<Vector3Int, Mesh> meshes;
        private Dictionary<Vector3Int, Mesh> Meshes
        {
            get
            {
                if (meshes == null)
                {
                    meshes = new Dictionary<Vector3Int, Mesh>();
                }
                return meshes;
            }
        }

        [SerializeField]
        [HideInInspector]
        private List<string> generatedMeshesKeys;

        [SerializeField]
        [HideInInspector]
        private List<Mesh> generatedMeshesValues;

        [SerializeField]
        private List<Vector3Int> meshesKeys;
        [SerializeField]
        private List<Mesh> meshesValues;

        [SerializeField]
        private float serializeVersion = 0;
        private const float SERIALIZE_VERSION_KEY_INT3 = 253;

        private void Reset()
        {
            generatedMeshes = new Dictionary<string, Mesh>();
            generatedMeshesKeys = new List<string>();
            generatedMeshesValues = new List<Mesh>();

            meshes = new Dictionary<Vector3Int, Mesh>();
            meshesKeys = new List<Vector3Int>();
            meshesValues = new List<Mesh>();

            serializeVersion = GVersionInfo.Number;
        }

        private void OnEnable()
        {
            if (serializeVersion < SERIALIZE_VERSION_KEY_INT3)
            {
                UpgradeSerializeVersion();
            }
        }

        public void OnBeforeSerialize()
        {
            //remain for backup
            generatedMeshesKeys.Clear();
            generatedMeshesValues.Clear();
            foreach (string k in GeneratedMeshes.Keys)
            {
                if (GeneratedMeshes[k] != null)
                {
                    generatedMeshesKeys.Add(k);
                    generatedMeshesValues.Add(GeneratedMeshes[k]);
                }
            }

            meshesKeys.Clear();
            meshesValues.Clear();
            foreach (Vector3Int k in Meshes.Keys)
            {
                if (Meshes[k] != null)
                {
                    meshesKeys.Add(k);
                    meshesValues.Add(Meshes[k]);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            //remain for backup
            if (generatedMeshesKeys != null && generatedMeshesValues != null)
            {
                GeneratedMeshes.Clear();
                for (int i = 0; i < generatedMeshesKeys.Count; ++i)
                {
                    string k = generatedMeshesKeys[i];
                    Mesh m = generatedMeshesValues[i];
                    if (!string.IsNullOrEmpty(k) && m != null)
                    {
                        GeneratedMeshes[k] = m;
                    }
                }
            }

            if (meshesKeys != null && meshesValues != null)
            {
                Meshes.Clear();
                for (int i = 0; i < meshesKeys.Count; ++i)
                {
                    Vector3Int k = meshesKeys[i];
                    Mesh m = meshesValues[i];
                    Meshes[k] = m;
                }
            }
        }

        public void SetMesh(Vector3Int key, Mesh mesh)
        {
            if (Meshes.ContainsKey(key))
            {
                Mesh oldMesh = Meshes[key];
                if (oldMesh != null)
                {
                    GUtilities.DestroyObject(oldMesh);
                }
                Meshes.Remove(key);
            }
            GCommon.TryAddObjectToAsset(mesh, this);
            Meshes.Add(key, mesh);
            GCommon.SetDirty(this);
        }

        public Mesh GetMesh(Vector3Int key)
        {
            if (Meshes.ContainsKey(key))
                return Meshes[key];
            else
                return null;
        }

        public void DeleteMesh(Vector3Int key)
        {
            if (Meshes.ContainsKey(key))
            {
                Mesh m = Meshes[key];
                if (m != null)
                {
                    GUtilities.DestroyObject(m);
                }
                Meshes.Remove(key);
                GCommon.SetDirty(this);
            }
        }

        public List<Vector3Int> GetKeys()
        {
            return new List<Vector3Int>(Meshes.Keys);
        }

        private void UpgradeSerializeVersion()
        {
            Meshes.Clear();
            foreach (string k in GeneratedMeshes.Keys)
            {
                int subStrIndex = k.IndexOf('_');
                if (subStrIndex < 0)
                    continue;
                string indexString = k.Substring(subStrIndex);
                string[] indices = indexString.Split(new string[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries);
                if (indices.Length != 3)
                    continue;
                int x = 0;
                int y = 0;
                int z = 0;
                int.TryParse(indices[0], out x);
                int.TryParse(indices[1], out y);
                int.TryParse(indices[2], out z);
                Vector3Int newKey = new Vector3Int(x, y, z);
                Mesh m = GeneratedMeshes[k];
                Meshes.Add(newKey, m);
            }
            serializeVersion = GVersionInfo.Number;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
#endif
