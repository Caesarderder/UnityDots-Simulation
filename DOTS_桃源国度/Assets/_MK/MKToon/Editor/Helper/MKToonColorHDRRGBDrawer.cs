//////////////////////////////////////////////////////
// MK Toon Editor Color RGB Drawer        			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MK.Toon.Editor
{
    internal class MKToonColorHDRRGBDrawer : MK.Toon.Editor.MaterialPropertyDrawer
    {
        public MKToonColorHDRRGBDrawer(GUIContent ui) : base(ui) {}
        public MKToonColorHDRRGBDrawer() : base(GUIContent.none) {}

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Color color = prop.colorValue;
            EditorGUI.BeginChangeCheck();

            color = EditorGUI.ColorField(position, new GUIContent(label), color, true, false, true);

            if (EditorGUI.EndChangeCheck())
            {
                prop.colorValue = color;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}
#endif