#if GRIFFIN
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GTerrainGeneratedData))]
    public class GTerrainGeneratedDataInspector : Editor
    {
        private GTerrainGeneratedData instance;
        private void OnEnable()
        {
            instance = target as GTerrainGeneratedData;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Terrain Data", instance.TerrainData, typeof(GTerrainData), false);
            GUI.enabled = true;
            DrawStatisticGUI();
        }

        private void DrawStatisticGUI()
        {
            string label = "Statistic";
            string id = "statistic" + instance.GetInstanceID();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                string filePath = AssetDatabase.GetAssetPath(instance);
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo != null)
                {
                    long length = fileInfo.Length;
                    EditorGUILayout.LabelField("Size", string.Format("{0} MB", (length / 1000000).ToString("0.00")));
                }
            });
        }
    }
}
#endif
