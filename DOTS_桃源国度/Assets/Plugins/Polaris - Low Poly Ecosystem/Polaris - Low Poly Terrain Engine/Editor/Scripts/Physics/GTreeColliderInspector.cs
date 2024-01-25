#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin.Physic
{
    [CustomEditor(typeof(GTreeCollider))]
    public class GTreeColliderInspector : Editor
    {
        private GTreeCollider instance;

        private void OnEnable()
        {
            instance = target as GTreeCollider;
        }

        public override void OnInspectorGUI()
        {
            instance.Terrain = EditorGUILayout.ObjectField("Terrain", instance.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            instance.Target = EditorGUILayout.ObjectField("Target", instance.Target, typeof(GameObject), true) as GameObject;
            instance.Distance = EditorGUILayout.FloatField("Distance", instance.Distance);
            instance.CopyTreeTag = EditorGUILayout.Toggle("Copy Tree Tag", instance.CopyTreeTag);
        }
    }
}
#endif
