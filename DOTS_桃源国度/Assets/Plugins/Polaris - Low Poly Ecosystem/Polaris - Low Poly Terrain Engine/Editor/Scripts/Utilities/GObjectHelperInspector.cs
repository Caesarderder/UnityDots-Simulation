#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GObjectHelper))]
    public class GObjectHelperInspector : Editor
    {
        private GObjectHelper instance;
        private void OnEnable()
        {
            instance = target as GObjectHelper;
        }

        public override void OnInspectorGUI()
        {
            instance.Terrain = EditorGUILayout.ObjectField("Terrain", instance.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            instance.SnapMode = (GSnapMode)EditorGUILayout.EnumPopup("Snap Mode", instance.SnapMode);

            SerializedObject so = new SerializedObject(instance);
            SerializedProperty sp = so.FindProperty("layerMask");
            if (sp != null)
            {
                EditorGUILayout.PropertyField(sp);
            }
            so.ApplyModifiedProperties();
            sp.Dispose();
            so.Dispose();

            instance.AlignToSurface = EditorGUILayout.Toggle("Align To Surface", instance.AlignToSurface);

            if (GUILayout.Button("Snap", GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                instance.Snap();
            }
        }
    }
}
#endif
