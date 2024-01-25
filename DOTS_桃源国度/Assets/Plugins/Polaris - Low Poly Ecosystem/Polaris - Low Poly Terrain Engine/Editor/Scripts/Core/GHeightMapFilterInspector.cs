#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GHeightMapFilter))]
    public class GHeightMapFilterInspector : Editor
    {
        private GHeightMapFilter instance;

        private void OnEnable()
        {
            instance = target as GHeightMapFilter;
        }

        public override void OnInspectorGUI()
        {
            DrawBlurFilter();
            DrawStepFilter();
            DrawActions();
        }

        private void DrawBlurFilter()
        {
            string label = "Blur";
            string id = "blur" + instance.ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                instance.UseBlur = EditorGUILayout.Toggle("Enable", instance.UseBlur);
                instance.BlurRadius = EditorGUILayout.IntSlider("Radius", instance.BlurRadius, 1, 5);
            });
        }

        private void DrawStepFilter()
        {
            string label = "Step";
            string id = "step" + instance.ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                instance.UseStep = EditorGUILayout.Toggle("Enable", instance.UseStep);
                instance.StepCount = EditorGUILayout.IntField("Count", instance.StepCount);
            });
        }

        private void DrawActions()
        {
            string label = "Action";
            string id = "action" + instance.ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                if (GUILayout.Button("Update"))
                {
                    if (instance.Terrain != null && instance.Terrain.TerrainData != null)
                    {
                        instance.Terrain.TerrainData.Geometry.SetRegionDirty(GCommon.UnitRect);
                        instance.Terrain.TerrainData.SetDirty(GTerrainData.DirtyFlags.Geometry);
                    }
                }
            });
        }
    }
}
#endif
