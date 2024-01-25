#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GTerrainChunk))]
    public class GTerrainChunkInspector : Editor
    {
        private GTerrainChunk instance;

        public void OnEnable()
        {
            instance = target as GTerrainChunk;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GStylizedTerrain terrain = instance.Terrain;
            if (terrain.TerrainData == null)
                return;

            //string label = "Vertex Count";
            //string id = "vertexcount" + instance.GetInstanceID();

            //GEditorCommon.Foldout(label, false, id, () =>
            //{
            //    EditorGUILayout.LabelField("Vertex Count");
            //    for (int i = 0; i < terrain.TerrainData.Geometry.LODCount; ++i)
            //    {
            //        Mesh m = terrain.TerrainData.GeometryData.GetMesh(GTerrainChunk.GetChunkMeshKey(instance.Index, i));
            //        if (m != null)
            //        {
            //            EditorGUILayout.LabelField(string.Format("LOD {0}", i.ToString()), m.vertexCount.ToString());
            //        }
            //        else
            //        {
            //            EditorGUILayout.LabelField(string.Format("LOD {0}", i.ToString()), "null");
            //        }
            //    }
            //});
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }
    }
}
#endif
