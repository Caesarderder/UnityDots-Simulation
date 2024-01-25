#if UNITY_2021_2_OR_NEWER
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEditor.Toolbars;

namespace SingularityGroup.HotReload.Editor {
    [Overlay(typeof(SceneView), "Hot Reload", true)]
    [Icon("Assets/HotReload/Editor/Resources/Icon_DarkMode.png")]
    internal class HotReloadOverlay : ToolbarOverlay {
        HotReloadOverlay() : base(HotReloadToolbarButton.id) {}
        
        [EditorToolbarElement(id, typeof(SceneView))]
        class HotReloadToolbarButton : EditorToolbarButton, IAccessContainerWindow {
            internal const string id = "HotReloadOverlay/LogoButton";
            public EditorWindow containerWindow { get; set; }
            
            internal HotReloadToolbarButton() {
                icon = GetIndicationIcon();
                tooltip = EditorIndicationState.IndicationStatusText;
                clicked += OnClick;
                EditorApplication.update += Update;
            }

            void OnClick() {
                EditorWindow.GetWindow<HotReloadWindow>().Show();
                EditorWindow.GetWindow<HotReloadWindow>().SelectTab(typeof(HotReloadRunTab));
            }
       
            void Update() {
                EditorCodePatcher.RequestServerInfo();
                icon = GetIndicationIcon();
                tooltip = EditorIndicationState.IndicationStatusText;
            }
        }

        private static Texture2D latestIcon;
        private static Dictionary<string, Texture2D> iconTextures = new Dictionary<string, Texture2D>();
        private static Spinner spinner = new Spinner(100);
        private static Texture2D GetIndicationIcon() {
            if (EditorIndicationState.IndicationIconPath == null || EditorIndicationState.SpinnerActive) {
                latestIcon = spinner.GetIcon();
            } else {
                var iconPath = EditorIndicationState.IndicationIconPath;
                if (!iconTextures.TryGetValue(iconPath, out latestIcon)) {
                    latestIcon = Resources.Load<Texture2D>(iconPath);
                    iconTextures[iconPath] = latestIcon;
                }
            }
            return latestIcon;
        }

        private static Image indicationIcon;
        private static Label indicationText;

        /// <summary>
        /// Create Hot Reload overlay panel.
        /// </summary>
        public override VisualElement CreatePanelContent() {
            var root = new VisualElement() { name = "Hot Reload Indication" };
            root.style.flexDirection = FlexDirection.Row;
            
            indicationIcon = new Image() { image = Resources.Load<Texture2D>(EditorIndicationState.greyIconPath) };
            indicationIcon.style.height = 30;
            indicationIcon.style.width = 30;
            indicationIcon.style.marginLeft = 2;
            indicationIcon.style.marginTop = 1;
            indicationIcon.style.marginRight = 5;
            indicationText = new Label(){text = EditorIndicationState.IndicationStatusText};
            indicationText.style.paddingTop = 9;
            indicationText.style.marginLeft = new StyleLength(StyleKeyword.Auto);
            indicationText.style.marginRight = new StyleLength(StyleKeyword.Auto);
            
            root.Add(indicationIcon);
            root.Add(indicationText);
            root.style.width = 190;
            root.style.height = 32;

            EditorApplication.update += Update;
            return root;
        }

        private void Update() {
            EditorCodePatcher.RequestServerInfo();
            indicationIcon.image = GetIndicationIcon();
            indicationText.text = EditorIndicationState.IndicationStatusText;
        }
    }
}
#endif
