//////////////////////////////////////////////////////
// MK Toon Editor Helper Styles	    	    	   	//
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
    public static partial class EditorHelper
    {
        //Based on unity builtin styles
        internal static class EditorStyles
        {
            internal static readonly GUIStyle rightAlignetLabel = new GUIStyle(UnityEditor.EditorStyles.label) { alignment = TextAnchor.MiddleRight };

            internal static readonly GUIStyle largeHeader = new GUIStyle(UnityEditor.EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 18 };

            internal static readonly GUIStyle headerCheckbox = new GUIStyle("ShurikenCheckMark");
            internal static readonly GUIStyle headerCheckboxMixed = new GUIStyle("ShurikenCheckMarkMixed");

            internal static readonly GUIStyle smallTickbox = new GUIStyle("ShurikenToggle");

            static readonly Color splitterDark = new UnityEngine.Color(0.12f, 0.12f, 0.12f, 1.333f);
            static readonly Color splitterLight = splitterLight = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1.333f);
            internal static Color splitter { get { return EditorGUIUtility.isProSkin ? splitterDark : splitterLight; } }

            static readonly Texture2D paneOptionsIconDark;
            static readonly Texture2D paneOptionsIconLight;

            internal static Texture2D paneOptionsIcon { get { return EditorGUIUtility.isProSkin ? paneOptionsIconDark : paneOptionsIconLight; } }

            internal static readonly GUIStyle headerLabel = new GUIStyle(UnityEditor.EditorStyles.miniLabel);

            static readonly Color headerBackgroundDark = new UnityEngine.Color(0.1f, 0.1f, 0.1f, 0.2f);
            static readonly Color headerBackgroundLight = new UnityEngine.Color(1f, 1f, 1f, 0.2f);
            internal static Color headerBackground { get { return EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight; } }

            internal static readonly GUIStyle wheelLabel = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
            internal static readonly GUIStyle wheelThumb = new GUIStyle("ColorPicker2DThumb");
            internal static readonly Vector2 wheelThumbSize = new Vector2
            (
                !Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
                !Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
            );

            internal static readonly GUIStyle preLabel = new GUIStyle("ShurikenLabel");

            internal static GUIContent VertexStreams = new GUIContent("Vertex Streams",
                "The vertex streams needed for this Material to function properly.");

            internal static string streamPositionText = "Position (POSITION.xyz)";
            internal static string streamNormalText = "Normal (NORMAL.xyz)";
            internal static string streamColorText = "Color (COLOR.xyzw)";
            internal static string streamUVText = "UV (TEXCOORD0.xy)";
            internal static string streamUV2Text = "UV2 (TEXCOORD0.zw)";
            internal static string streamAnimBlendText = "AnimBlend (TEXCOORD3.x)";
            internal static string streamTangentText = "Tangent (TANGENT.xyzw)";

            internal static GUIContent streamApplyToAllSystemsText = new GUIContent("Fix Now",
                "Apply the vertex stream layout to all Particle Systems using this material");

            internal static string undoApplyCustomVertexStreams = L10n.Tr("Apply custom vertex streams from material");

            internal static GUIStyle vertexStreamIcon = new GUIStyle();

            static EditorStyles()
            {
                paneOptionsIconDark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
                paneOptionsIconLight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
            }
        }
    }
}
#endif