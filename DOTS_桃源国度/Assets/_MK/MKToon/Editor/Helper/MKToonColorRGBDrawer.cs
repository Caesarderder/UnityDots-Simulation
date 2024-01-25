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
using System;

namespace MK.Toon.Editor
{
    internal class MKToonColorRGBDrawer : MK.Toon.Editor.MaterialPropertyDrawer
    {
        public MKToonColorRGBDrawer(GUIContent ui) : base(ui) {}
        public MKToonColorRGBDrawer() : base(GUIContent.none) {}

        public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Color color = prop.colorValue;
            EditorGUI.BeginChangeCheck();

            color = EditorGUI.ColorField(position, new GUIContent(label), color, true, false, false);

            if (EditorGUI.EndChangeCheck())
            {
                prop.colorValue = color;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}
#endif