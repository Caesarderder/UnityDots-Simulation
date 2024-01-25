using UnityEditor;
using UnityEngine;

namespace ProjectDawn.Navigation.Editor
{
    [CustomPropertyDrawer(typeof(NavigationLayers))]
    class NavigationLayersDrawer : PropertyDrawer
    {
        string[] m_PropertyLayerNames;

        public override void OnGUI(Rect controlRect, SerializedProperty property, GUIContent label)
        {
            int layers = property.enumValueFlag;

            var layerNames = AgentsNavigationSettings.Get<SpatialPartitioningSubSettings>().LayerNames;
            int layerCount = Mathf.Max(2, layerNames.Length);

            if (m_PropertyLayerNames == null || m_PropertyLayerNames.Length != layerCount)
                m_PropertyLayerNames = new string[layerCount];

            for (int i = 0; i < layerCount; ++i)
            {
                m_PropertyLayerNames[i] = layerNames.Length > i && !string.IsNullOrEmpty(layerNames[i]) ? layerNames[i] : $"Layer {i}";
            }

            EditorGUI.BeginProperty(controlRect, label, property);

            EditorGUI.BeginChangeCheck();
            layers = EditorGUI.MaskField(controlRect, label, layers, m_PropertyLayerNames);

            if (EditorGUI.EndChangeCheck())
                property.enumValueFlag = layers;

            EditorGUI.EndProperty();
        }
    }
}
