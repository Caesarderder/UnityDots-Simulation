using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace SingularityGroup.HotReload.Editor {
    internal static class HotReloadWindowStyles {
        const int defaultPadding = 31;
                
        private static GUIStyle h1TitleStyle;
        private static GUIStyle h1TitleCenteredStyle;
        private static GUIStyle h2TitleStyle;
        private static GUIStyle h3TitleStyle;
        private static GUIStyle accountCreatedStyle;
        private static GUIStyle h4TitleStyle;
        private static GUIStyle h5TitleStyle;
        private static GUIStyle boxStyle;
        private static GUIStyle wrapStyle;
        private static GUIStyle noPaddingMiddleLeftStyle;
        private static GUIStyle middleLeftStyle;
        private static GUIStyle middleCenterStyle;
        private static GUIStyle mediumMiddleCenterStyle;
        private static GUIStyle textFieldWrapStyle;
        private static GUIStyle foldoutStyle;
        private static GUIStyle h3CenterTitleStyle;
        private static GUIStyle logoStyle;
        private static GUIStyle changelogPointersStyle;
        private static GUIStyle indicationIconBox;
        private static GUIStyle indicationTextBox;
        private static GUIStyle indicationText;
        private static GUIStyle textStyle;
        private static GUIStyle buttonStyle;
        private static GUIStyle indicationIconStyle;
        private static GUIStyle spinnerIconStyle;
        private static GUIStyle unsupportedChangesIconStyle;
        private static GUIStyle removeUnsupportedChangeStyle;
        private static GUIStyle startButtonStyle;
        private static GUIStyle logStyle;
        private static GUIStyle sectionOuterBoxStyle;
        private static GUIStyle sectionOuterBoxCompactStyle;
        private static GUIStyle dynamicSectionOuterBoxCompactStyle;
        private static GUIStyle sectionInnerBoxStyle;
        private static GUIStyle sectionInnerBoxWideStyle;
        private static GUIStyle dynamicSectionInnerBoxWideStyle;
        private static GUIStyle changelogSectionInnerBoxStyle;
        private static GUIStyle unsupportedChangesInnerBoxStyle;
        private static GUIStyle indicationBoxStyle;
        private static GUIStyle indicationIconBoxStyle;
        private static GUIStyle indicationTextBoxStyle;
        private static GUIStyle unsupportedChangesHeaderStyle;
        private static GUIStyle logStyleLight;
        private static GUIStyle logStyleDark;
        private static GUIStyle linkStyle;
        private static GUIStyle dropdownAreaStyle;
        private static GUIStyle dropdownBoxStyle;
        private static GUIStyle labelStyle;
        private static GUIStyle progressBarBarStyle;
        private static GUIStyle progressBarAnchorStyle;
        private static GUIStyle downloadInfoButtonStyle;
        
        public static bool IsDarkMode => EditorGUIUtility.isProSkin;
        public static int windowScreenWidth => HotReloadWindow.Current ? (int)HotReloadWindow.Current.position.width : Screen.width;
        public static int windowScreenHeight => HotReloadWindow.Current ? (int)HotReloadWindow.Current.position.height : Screen.height;
        public static GUIStyle H1TitleStyle {
            get {
                if (h1TitleStyle == null) {
                    h1TitleStyle = new GUIStyle(EditorStyles.label);
                    h1TitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h1TitleStyle.fontStyle = FontStyle.Bold;
                    h1TitleStyle.fontSize = 16;
                    h1TitleStyle.padding.top = 5;
                    h1TitleStyle.padding.bottom = 5;
                }
                return h1TitleStyle;
            }
        }
        
        public static GUIStyle H1TitleCenteredStyle {
            get {
                if (h1TitleCenteredStyle == null) {
                    h1TitleCenteredStyle = new GUIStyle(H1TitleStyle);
                    h1TitleCenteredStyle.alignment = TextAnchor.MiddleCenter;
                }
                return h1TitleCenteredStyle;
            }
        }
        
        public static GUIStyle H2TitleStyle {
            get {
                if (h2TitleStyle == null) {
                    h2TitleStyle = new GUIStyle(EditorStyles.label);
                    h2TitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h2TitleStyle.fontStyle = FontStyle.Bold;
                    h2TitleStyle.fontSize = 14;
                    h2TitleStyle.padding.top = 5;
                    h2TitleStyle.padding.bottom = 5;
                }
                return h2TitleStyle;
            }
        }
        
        public static GUIStyle H3TitleStyle {
            get {
                if (h3TitleStyle == null) {
                    h3TitleStyle = new GUIStyle(EditorStyles.label);
                    h3TitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h3TitleStyle.fontStyle = FontStyle.Bold;
                    h3TitleStyle.fontSize = 12;
                    h3TitleStyle.padding.top = 5;
                    h3TitleStyle.padding.bottom = 5;
                }
                return h3TitleStyle;
            }
        }
        
        public static GUIStyle H3CenteredTitleStyle {
            get {
                if (h3CenterTitleStyle == null) {
                    h3CenterTitleStyle = new GUIStyle(EditorStyles.label);
                    h3CenterTitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h3CenterTitleStyle.fontStyle = FontStyle.Bold;
                    h3CenterTitleStyle.alignment = TextAnchor.MiddleCenter;
                    h3CenterTitleStyle.fontSize = 12;
                }
                return h3CenterTitleStyle;
            }
        }

        public static GUIStyle H4TitleStyle {
            get {
                if (h4TitleStyle == null) {
                    h4TitleStyle = new GUIStyle(EditorStyles.label);
                    h4TitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h4TitleStyle.fontStyle = FontStyle.Bold;
                    h4TitleStyle.fontSize = 11;
                }
                return h4TitleStyle;
            }
        }

        public static GUIStyle H5TitleStyle {
            get {
                if (h5TitleStyle == null) {
                    h5TitleStyle = new GUIStyle(EditorStyles.label);
                    h5TitleStyle.normal.textColor = EditorStyles.label.normal.textColor;
                    h5TitleStyle.fontStyle = FontStyle.Bold;
                    h5TitleStyle.fontSize = 10;
                }
                return h5TitleStyle;
            }
        }
        
        public static GUIStyle LabelStyle {
            get {
                if (labelStyle == null) {
                    labelStyle = new GUIStyle(EditorStyles.label);
                    labelStyle.fontSize = 12;
                    labelStyle.clipping = TextClipping.Clip;
                    labelStyle.wordWrap = true;
                }
                if (IsDarkMode) {
                    labelStyle.normal.textColor = Color.white;
                } else {
                    labelStyle.normal.textColor = Color.black;
                    labelStyle.hover.textColor = Color.white;
                }
                return labelStyle;
            }
        }

        public static GUIStyle BoxStyle {
            get {
                if (boxStyle == null) {
                    boxStyle = new GUIStyle(GUI.skin.box);
                    boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                    boxStyle.fontStyle = FontStyle.Bold;
                    boxStyle.alignment = TextAnchor.UpperLeft;
                }
                if (!IsDarkMode) {
                    boxStyle.normal.background = Texture2D.blackTexture;
                }
                return boxStyle;
            }
        }

        public static GUIStyle WrapStyle {
            get {
                if (wrapStyle == null) {
                    wrapStyle = new GUIStyle(EditorStyles.label);
                    wrapStyle.fontStyle = FontStyle.Normal;
                    wrapStyle.wordWrap = true;
                }
                return wrapStyle;
            }
        }

        public static GUIStyle NoPaddingMiddleLeftStyle {
            get {
                if (noPaddingMiddleLeftStyle == null) {
                    noPaddingMiddleLeftStyle = new GUIStyle(EditorStyles.label);
                    noPaddingMiddleLeftStyle.normal.textColor = GUI.skin.label.normal.textColor;
                    noPaddingMiddleLeftStyle.padding = new RectOffset();
                    noPaddingMiddleLeftStyle.margin = new RectOffset();
                    noPaddingMiddleLeftStyle.alignment = TextAnchor.MiddleLeft;
                }
                return noPaddingMiddleLeftStyle;
            }
        }

        public static GUIStyle MiddleLeftStyle {
            get {
                if (middleLeftStyle == null) {
                    middleLeftStyle = new GUIStyle(EditorStyles.label);
                    middleLeftStyle.fontStyle = FontStyle.Normal;
                    middleLeftStyle.alignment = TextAnchor.MiddleLeft;
                }

                return middleLeftStyle;
            }
        }

        public static GUIStyle MiddleCenterStyle {
            get {
                if (middleCenterStyle == null) {
                    middleCenterStyle = new GUIStyle(EditorStyles.label);
                    middleCenterStyle.fontStyle = FontStyle.Normal;
                    middleCenterStyle.alignment = TextAnchor.MiddleCenter;
                }
                return middleCenterStyle;
            }
        }
        
        public static GUIStyle MediumMiddleCenterStyle {
            get {
                if (mediumMiddleCenterStyle == null) {
                    mediumMiddleCenterStyle = new GUIStyle(EditorStyles.label);
                    mediumMiddleCenterStyle.fontStyle = FontStyle.Normal;
                    mediumMiddleCenterStyle.fontSize = 12;
                    mediumMiddleCenterStyle.alignment = TextAnchor.MiddleCenter;
                }
                return mediumMiddleCenterStyle;
            }
        }

        public static GUIStyle TextFieldWrapStyle {
            get {
                if (textFieldWrapStyle == null) {
                    textFieldWrapStyle = new GUIStyle(EditorStyles.textField);
                    textFieldWrapStyle.wordWrap = true;
                }
                return textFieldWrapStyle;
            }
        }

        public static GUIStyle FoldoutStyle {
            get {
                if (foldoutStyle == null) {
                    foldoutStyle = new GUIStyle(EditorStyles.foldout);
                    foldoutStyle.normal.textColor = GUI.skin.label.normal.textColor;
                    foldoutStyle.alignment = TextAnchor.MiddleLeft;
                    foldoutStyle.fontStyle = FontStyle.Bold;
                    foldoutStyle.fontSize = 12;
                }
                return foldoutStyle;
            }
        }
        
        public static GUIStyle LogoStyle {
            get {
                if (logoStyle == null) {
                    logoStyle = new GUIStyle();
                    logoStyle.margin = new RectOffset(6, 6, 0, 0);
                    logoStyle.padding = new RectOffset(16, 16, 0, 0);
                }
                return logoStyle;
            }
        }
        
        public static GUIStyle ChangelogPointerStyle {
            get {
                if (changelogPointersStyle == null) {
                    changelogPointersStyle = new GUIStyle(EditorStyles.label);
                    changelogPointersStyle.wordWrap = true;
                    changelogPointersStyle.fontSize = 12;
                    changelogPointersStyle.padding.left = 20;
                }
                return changelogPointersStyle;
            }
        }
        
        public static GUIStyle IndicationIcon {
            get {
                if (indicationIconStyle == null) {
                    indicationIconStyle = new GUIStyle();
                    indicationIconStyle.padding = new RectOffset(5, 6, 6, 6);
                    indicationIconStyle.fixedWidth = 50;
                    indicationIconStyle.fixedHeight = 50;
                }
                return indicationIconStyle;
            }
        }
        
        public static GUIStyle SpinnerIcon {
            get {
                if (spinnerIconStyle == null) {
                    spinnerIconStyle = new GUIStyle();
                    spinnerIconStyle.padding = new RectOffset(5, 6, 6, 6);
                    spinnerIconStyle.fixedWidth = 45;
                    spinnerIconStyle.fixedHeight = 45;
                }
                return spinnerIconStyle;
            }
        }
        
        public static GUIStyle UnsupportedChangesIcon {
            get {
                if (unsupportedChangesIconStyle == null) {
                    unsupportedChangesIconStyle = new GUIStyle();
                    unsupportedChangesIconStyle.fixedWidth = 22;
                    unsupportedChangesIconStyle.fixedHeight = 22;
                    unsupportedChangesIconStyle.padding.left = -40;
                    unsupportedChangesIconStyle.padding.top = -4;
                }
                return unsupportedChangesIconStyle;
            }
        }
        
        public static GUIStyle RemoveUnsupportedChangeIcon {
            get {
                if (removeUnsupportedChangeStyle == null) {
                    removeUnsupportedChangeStyle = new GUIStyle();
                    removeUnsupportedChangeStyle.fixedWidth = 22;
                    removeUnsupportedChangeStyle.fixedHeight = 22;
                }
                return removeUnsupportedChangeStyle;
            }
        }
        
        public static GUIStyle UnsupportedChangesText {
            get {
                if (textStyle == null) {
                    textStyle = new GUIStyle(H1TitleStyle);
                    textStyle.fixedWidth = 250;
                    textStyle.padding.top = -2;
                }
                return textStyle;
            }
        }
            
        public static GUIStyle UnsupportedChangesButton {
            get {
                if (buttonStyle == null) {
                    buttonStyle = new GUIStyle(EditorStyles.miniButton);
                    buttonStyle.margin.left = 5;
                    buttonStyle.padding.left = 10;
                }
                buttonStyle.fixedWidth = EditorApplication.isPlaying ? 120 : 90;
                
                return buttonStyle;
            }
        }
        
        public static GUIStyle StartButton {
            get {
                if (startButtonStyle == null) {
                    startButtonStyle = new GUIStyle(EditorStyles.miniButton);
                    startButtonStyle.fixedHeight = 25;
                    startButtonStyle.padding.left = 0;
                    startButtonStyle.fixedWidth = 135;
                }
                return startButtonStyle;
            }
        }

        private static Texture2D _clearBackground;
        private static Texture2D clearBackground {
            get {    
                    if (_clearBackground == null) {
                        _clearBackground = new Texture2D(1, 1);
                        _clearBackground.SetPixel(0, 0, Color.clear);
                        _clearBackground.Apply();
                    }
                    return _clearBackground;
                    
            }
        }

        public static GUIStyle SectionOuterBox {
            get {
                var renderUnsupportedChanges = EditorCodePatcher.Running && !EditorCodePatcher.Starting && EditorCodePatcher.Failures.Count > 0;
                var sectionInnerBoxMinWidth = renderUnsupportedChanges ? UnsupportedChangesInnerBox.fixedWidth : SectionInnerBox.fixedWidth;
                var unknownPadding = renderUnsupportedChanges ? 40 : 23;
                if (sectionOuterBoxStyle == null) {
                    sectionOuterBoxStyle = new GUIStyle(GUI.skin.box);
                    sectionOuterBoxStyle.margin.bottom = 0;
                    sectionOuterBoxStyle.margin.top = 0;
                }
                // Looks better without a background
                sectionOuterBoxStyle.normal.background = clearBackground;                    
                sectionOuterBoxStyle.padding.top = windowScreenHeight > 500 ? 50 : 10;
                sectionOuterBoxStyle.padding.bottom = windowScreenHeight > 500 ? 50 : 10;
                sectionOuterBoxStyle.fixedWidth = Math.Max(windowScreenWidth - unknownPadding, sectionInnerBoxMinWidth);
                return sectionOuterBoxStyle;
            }
        }
        
        
        public static GUIStyle DynamicSectionOuterBoxCompact {
            get {
                const int unknownPadding = 30;
                if (dynamicSectionOuterBoxCompactStyle == null) {
                    dynamicSectionOuterBoxCompactStyle = new GUIStyle(SectionOuterBoxCompact);
                }
                // making it smaller than screen size removes the scroll bar
                
                dynamicSectionOuterBoxCompactStyle.fixedWidth = windowScreenWidth - unknownPadding;
                return dynamicSectionOuterBoxCompactStyle;
            }
        }



        public static GUIStyle SectionOuterBoxCompact {
            get {
                var sectionInnerBoxMinWidth = SectionInnerBox.fixedWidth - 50;
                const int unknownPadding = 31;
                if (sectionOuterBoxCompactStyle == null) {
                    sectionOuterBoxCompactStyle = new GUIStyle(GUI.skin.box);
                    sectionOuterBoxCompactStyle.padding.top = 10;
                    sectionOuterBoxCompactStyle.padding.bottom = 10;
                    sectionOuterBoxCompactStyle.margin.top = 0;
                    sectionOuterBoxCompactStyle.margin.bottom = 0;
                }
                // Looks better without a background
                sectionOuterBoxCompactStyle.normal.background = clearBackground;  
                sectionOuterBoxCompactStyle.fixedWidth = Math.Max(windowScreenWidth - unknownPadding, sectionInnerBoxMinWidth);
                return sectionOuterBoxCompactStyle;
            }
        }
        
        public static GUIStyle SectionInnerBox {
            get {
                const int sectionInnerBoxWidth = 400;
                const int unknownPadding = 23;
                if (sectionInnerBoxStyle == null) {
                    sectionInnerBoxStyle = new GUIStyle(EditorStyles.helpBox);
                    sectionInnerBoxStyle.fixedWidth = sectionInnerBoxWidth;
                    sectionInnerBoxStyle.padding.top = 15;
                    sectionInnerBoxStyle.padding.bottom = 15;
                    sectionInnerBoxStyle.padding.left = 5;
                    sectionInnerBoxStyle.padding.right = 10;
                }
                sectionInnerBoxStyle.margin.left = windowScreenWidth / 2 - sectionInnerBoxWidth / 2 - unknownPadding / 2;
                return sectionInnerBoxStyle;
            }
        }
        
        public static GUIStyle DynamicSectionInnerBoxWide {
            get {
                const int sectionInnerBoxWidth = 600;
                if (dynamicSectionInnerBoxWideStyle == null) {
                    dynamicSectionInnerBoxWideStyle = new GUIStyle(SectionInnerBoxWide);
                }
                dynamicSectionInnerBoxWideStyle.fixedWidth = Math.Min(windowScreenWidth - defaultPadding, sectionInnerBoxWidth);
                dynamicSectionInnerBoxWideStyle.margin.left = windowScreenWidth / 2 - (int)dynamicSectionInnerBoxWideStyle.fixedWidth / 2 - defaultPadding / 2;
                return dynamicSectionInnerBoxWideStyle;
            }
        }

        public static GUIStyle SectionInnerBoxWide {
            get {
                const int sectionInnerBoxWidth = 600;
                const int unknownPadding = 23;
                if (sectionInnerBoxWideStyle == null) {
                    sectionInnerBoxWideStyle = new GUIStyle(EditorStyles.helpBox);
                    sectionInnerBoxWideStyle.fixedWidth = sectionInnerBoxWidth;
                    sectionInnerBoxWideStyle.padding.top = 15;
                    sectionInnerBoxWideStyle.padding.bottom = 15;
                    sectionInnerBoxWideStyle.padding.left = 5;
                    sectionInnerBoxWideStyle.padding.right = 10;
                }
                sectionInnerBoxWideStyle.margin.left = windowScreenWidth / 2 - sectionInnerBoxWidth / 2 - unknownPadding / 2;
                return sectionInnerBoxWideStyle;
            }
        }

        public static GUIStyle ChangelogSectionInnerBox {
            get {
                const int sectionInnerBoxWidth = 585;
                if (changelogSectionInnerBoxStyle == null) {
                    changelogSectionInnerBoxStyle = new GUIStyle(EditorStyles.helpBox);
                    changelogSectionInnerBoxStyle.margin.bottom = 10;
                    changelogSectionInnerBoxStyle.margin.top = 10;
                }
                sectionOuterBoxCompactStyle.fixedWidth = Math.Max(windowScreenWidth - defaultPadding, sectionInnerBoxWidth);
                return changelogSectionInnerBoxStyle;
            }
        }

        public static GUIStyle IndicationBox {
            get {
                if (indicationBoxStyle == null) {
                    indicationBoxStyle = new GUIStyle();
                    indicationBoxStyle.margin.left = 35;
                    indicationBoxStyle.margin.right = 35;
                    indicationBoxStyle.margin.bottom = 10;
                }
                return indicationBoxStyle;
            }
        }
        
        public static GUIStyle IndicationIconBox {
            get {
                if (indicationIconBoxStyle == null) {
                    indicationIconBoxStyle = new GUIStyle(EditorStyles.helpBox);
                    indicationIconBoxStyle.fixedWidth = 50;
                    indicationIconBoxStyle.fixedHeight = 50;
                    indicationIconBoxStyle.margin.right = 7;
                }
                return indicationIconBoxStyle;
            }
        }
        
        public static GUIStyle IndicationTextBox {
            get {
                if (indicationTextBoxStyle == null) {
                    indicationTextBoxStyle = new GUIStyle(EditorStyles.helpBox);
                    indicationTextBoxStyle.fixedHeight = 50;
                    indicationTextBoxStyle.padding.top = 9;
                    indicationTextBoxStyle.fixedWidth = SectionInnerBox.fixedWidth - IndicationBox.margin.left - IndicationIconBox.fixedWidth - IndicationIconBox.margin.right - IndicationBox.margin.right - SectionInnerBox.padding.left - SectionInnerBox.padding.right;
                }
                return indicationTextBoxStyle;
            }
        }
        
        public static GUIStyle UnsupportedChangesInnerBox {
            get {
                const int width = 600;
                const int unknownPadding = 23;
                if (unsupportedChangesInnerBoxStyle == null) {
                    unsupportedChangesInnerBoxStyle = new GUIStyle(SectionInnerBox);
                    unsupportedChangesInnerBoxStyle.fixedWidth = width;
                }
                unsupportedChangesInnerBoxStyle.margin.left = windowScreenWidth/2 - width/2 - unknownPadding/2;
                return unsupportedChangesInnerBoxStyle;
            }
        }
        
        public static GUIStyle UnsupportedChangesHeader {
            get {
                if (unsupportedChangesHeaderStyle == null) {
                    unsupportedChangesHeaderStyle = new GUIStyle();
                    unsupportedChangesHeaderStyle.padding.top = 5;
                    unsupportedChangesHeaderStyle.padding.bottom = 5;
                }
                return unsupportedChangesHeaderStyle;
            }
        }
        
        public static GUIStyle LogStyle {
            get {
                if (logStyle == null) {
                    logStyle = new GUIStyle();
                    logStyle.padding.top = 17;
                    logStyle.padding.bottom = 5;
                    logStyle.fixedHeight = 60;
                    logStyle.fixedWidth = UnsupportedChangesInnerBox.fixedWidth - 5;
                }
                return logStyle;
            }
        }

        public static GUIStyle LogStyleLight {
            get {
                if (logStyleLight == null) {
                    logStyleLight = new GUIStyle(LogStyle);
                    Color lighterGrey = new Color(0.7f, 0.7f, 0.7f, 0.3f);
                    Texture2D lighterGreyTexture = new Texture2D(1, 1);
                    lighterGreyTexture.SetPixel(0, 0, lighterGrey);
                    lighterGreyTexture.Apply();
                    logStyleLight.normal.background = lighterGreyTexture;
                }
                return logStyleLight;
            }
        }
        
        public static GUIStyle LogStyleDark {
            get {
                if (logStyleDark == null) {
                    logStyleDark = new GUIStyle(LogStyle);
                    Color darkerGrey = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                    Texture2D darkerGreyTexture = new Texture2D(1, 1);
                    darkerGreyTexture.SetPixel(0, 0, darkerGrey);
                    darkerGreyTexture.Apply();
                    logStyleDark.normal.background = darkerGreyTexture;
                }
                return logStyleDark;
            }
        }
        
        public static GUIStyle LinkStyle {
            get {
                if (linkStyle == null) {
                    linkStyle = new GUIStyle(EditorStyles.label);
                    var color = IsDarkMode ? new Color32(0x3F, 0x9F, 0xFF, 0xFF) : new Color32(0x0F, 0x52, 0xD7, 0xFF);
                    linkStyle.normal.textColor = color;
                    linkStyle.fontStyle = FontStyle.Bold;
                }
                return linkStyle;
            }
        }

        public static GUIStyle DropdownAreaStyle {
            get {
                if (dropdownAreaStyle == null) {
                    dropdownAreaStyle = new GUIStyle(EditorStyles.helpBox);
                    dropdownAreaStyle.margin.left = 0;
                    dropdownAreaStyle.fixedWidth = UnsupportedChangesInnerBox.fixedWidth - 10;
                }
                return dropdownAreaStyle;
            }
        }

        public static GUIStyle DropdownBox {
            get {
                if (dropdownBoxStyle == null) {
                    dropdownBoxStyle = new GUIStyle(EditorStyles.textArea);
                    dropdownBoxStyle.margin.left = 0;
                }
                return dropdownBoxStyle;
            }
        }
        
        public static GUIStyle ProgressBarBarStyle {
            get {
                if (progressBarBarStyle != null) {
                    return progressBarBarStyle;
                }
                var styles = (EditorStyles)typeof(EditorStyles)
                    .GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic)
                    ?.GetValue(null);
                var style = styles?.GetType()
                    .GetField("m_ProgressBarBar", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(styles);
                progressBarBarStyle = style != null ? (GUIStyle)style : GUIStyle.none;
                return progressBarBarStyle;
            }
        }
        
        public static GUIStyle ProgressBarAnchorStyle {
            get {
                if (progressBarAnchorStyle != null) {
                    return progressBarAnchorStyle;
                }
                progressBarAnchorStyle = new GUIStyle();
                progressBarAnchorStyle.padding.bottom = SectionInnerBox.padding.bottom - 5;
                return progressBarAnchorStyle;
            }
        }
        
        public static GUIStyle DownloadInfoButtonStyle {
            get {
                if (downloadInfoButtonStyle != null) {
                    return downloadInfoButtonStyle;
                }
                downloadInfoButtonStyle = new GUIStyle(EditorStyles.miniButton);
                downloadInfoButtonStyle.fixedHeight = StartButton.fixedHeight - 1;
                downloadInfoButtonStyle.margin.top = -1;
                downloadInfoButtonStyle.fixedWidth = 60;
                return downloadInfoButtonStyle;
            }
        }
        
        public static GUIStyle AccountCreatedStyle {
            get {
                if (accountCreatedStyle == null) {
                    accountCreatedStyle = new GUIStyle(GUI.skin.box) {
                        stretchWidth = true,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(10, 10, 5, 5),
                        fontStyle = FontStyle.Bold,
                    };
                }
                accountCreatedStyle.normal.textColor = EditorGUIUtility.isProSkin ? EditorStyles.label.normal.textColor : GUI.skin.box.normal.textColor;
                return accountCreatedStyle;
            }
        }
    }
}
