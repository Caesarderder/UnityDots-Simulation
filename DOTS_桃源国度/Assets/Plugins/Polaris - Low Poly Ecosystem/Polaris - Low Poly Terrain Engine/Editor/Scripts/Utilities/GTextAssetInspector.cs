#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GTextAsset))]
    public class GTextAssetInspector : Editor
    {
        private GTextAsset instance;
        private void OnEnable()
        {
            instance = target as GTextAsset;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Text", EditorStyles.boldLabel);
            instance.Text = EditorGUILayout.TextArea(instance.Text, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Spacify"))
            {
                instance.Text = instance.Text.Replace("\t", "    ");
            }
            EditorUtility.SetDirty(instance);
        }
    }
}
#endif
