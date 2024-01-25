#if GRIFFIN && VEGETATION_STUDIO_PRO
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.MeshTerrains;
using UnityEditor;

namespace Pinwheel.Griffin.VegetationStudioPro
{
    [CustomEditor(typeof(GVSPPolarisTerrain))]
    public class GVSPPolarisTerrainEditor : MeshTerrainEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GVSPPolarisTerrain instance = (GVSPPolarisTerrain)target;
            instance.Terrain = EditorGUILayout.ObjectField("Terrain", instance.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
        }
    }
}
#endif
