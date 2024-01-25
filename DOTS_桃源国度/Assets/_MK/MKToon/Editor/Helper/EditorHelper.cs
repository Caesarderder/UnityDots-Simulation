//////////////////////////////////////////////////////
// MK Toon Editor Helper Main  	    	       		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MK.Toon.Editor
{
    public static partial class EditorHelper
    {
        /// <summary>
        /// Draw a default splitter
        /// </summary>
        internal static void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(0f, 1f);

            rect.xMin = 0f;
            rect.width += 4f;

            if(Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, EditorStyles.splitter);
        }

        /// <summary>
        /// Foldout for settings
        /// </summary>
        /// <param name="title"></param>
        /// <param name="titleRight"></param>
        /// <returns></returns>
        private static Rect DrawFoldoutHeader(string title, bool titleOffset, string titleRight = "", bool drawSplitter = true)
        {
            var gap = GUILayoutUtility.GetRect(0f, 0f);
            gap.xMin = 0f;
            gap.width += 4f;
            EditorGUI.DrawRect(gap, UnityEngine.Color.clear);
            if(drawSplitter)
                DrawSplitter();
            var rect = GUILayoutUtility.GetRect(16f, 16f);

            rect.xMin = 0f;
            rect.width += 4f;

            var lavelRect = new Rect(rect);
            lavelRect.xMin += titleOffset ? 22 : 12;
            EditorGUI.DrawRect(rect, EditorStyles.headerBackground);
            EditorGUI.LabelField(lavelRect, title, UnityEditor.EditorStyles.boldLabel);
            EditorGUI.LabelField(lavelRect, titleRight, EditorStyles.rightAlignetLabel);

            return rect;
        }
        
        /// <summary>
        /// Creates a empty space with the height of 1
        /// </summary>
        internal static void VerticalSpace()
        {
            GUILayoutUtility.GetRect(1f, EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Draws a header
        /// </summary>
        /// <param name="text"></param>
        internal static void DrawHeader(string text)
        {
            EditorGUILayout.LabelField(text, UnityEditor.EditorStyles.boldLabel);
        }

		/// <summary>
		/// Draw a clickable behavior including a checkbox for a feature
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="title"></param>
		/// <param name="titleRight"></param>
		/// <param name="behavior"></param>
		/// <param name="feature"></param>
		/// <returns></returns>
        internal static bool HandleBehavior(string title, string titleRight, MaterialProperty behavior, MaterialProperty feature, MaterialEditor materialEditor, bool drawSplitter = true)
        {
            Rect rect = DrawFoldoutHeader(title, feature != null, titleRight, drawSplitter);
                
            var e = Event.current;

            var foldoutRect = new Rect(EditorGUIUtility.currentViewWidth * 0.5f, rect.y, 13f, 13f);
            if(behavior.hasMixedValue)
            {
                foldoutRect.x -= 13;
            }

            if(feature != null)
            {
                EditorGUI.showMixedValue = feature.hasMixedValue;
                var toggleRect = new Rect(rect.x + 4f, rect.y + ((feature.hasMixedValue) ? 0.0f : 2.0f), 13f, 13f);
                bool fn = System.Convert.ToBoolean(feature.floatValue);
                EditorGUI.BeginChangeCheck();

                fn = EditorGUI.Toggle(toggleRect, "", fn, EditorStyles.headerCheckbox);

                if(EditorGUI.EndChangeCheck())
                {
                    feature.floatValue = System.Convert.ToInt32(fn);
                    if(fn)
                        materialEditor.RegisterPropertyChangeUndo(feature.displayName + " enabled");
                    else
                        materialEditor.RegisterPropertyChangeUndo(feature.displayName + " disabled");
                }
                EditorGUI.showMixedValue = false;

                EditorGUI.showMixedValue = behavior.hasMixedValue;
            }

            EditorGUI.BeginChangeCheck();
            if(e.type == EventType.MouseDown)
            {
                if(rect.Contains(e.mousePosition))
                {
                    if(behavior.hasMixedValue)
                        behavior.floatValue = System.Convert.ToInt32(false);
                    else
                        behavior.floatValue = behavior.floatValue > 0 ? 0 : 1;
                    e.Use();
                }
            }
            if(EditorGUI.EndChangeCheck())
            {
                if(System.Convert.ToBoolean(behavior.floatValue))
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " show");
                else
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " hide");
            }

            EditorGUI.showMixedValue = false;

            if (e.type == EventType.Repaint && behavior.hasMixedValue)
                UnityEditor.EditorStyles.radioButton.Draw(foldoutRect, "", false, false, true, false);
            else
                EditorGUI.Foldout(foldoutRect, System.Convert.ToBoolean(behavior.floatValue), "");

            if (behavior.hasMixedValue)
                return true;
            else
                return System.Convert.ToBoolean(behavior.floatValue);
        }

        internal static void SetKeyword(bool enable, string keyword, Material mat)
        {
            if(enable)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }
        
        public static void Divider()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
        }
	}
}
#endif
