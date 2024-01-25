#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GAssetExplorer))]
    public class GAssetExplorerInspector : Editor
    {
        private GAssetExplorer instance;

        public GUIStyle titleStyle;
        public GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.label);
                    titleStyle.fontStyle = FontStyle.Bold;
                    titleStyle.fontSize = 13;
                    titleStyle.alignment = TextAnchor.UpperLeft;
                }
                return titleStyle;
            }
        }

        public void OnEnable()
        {
            instance = target as GAssetExplorer;
        }

        public override void OnInspectorGUI()
        {
            DrawInstructionGUI();
            DrawFeaturedAssetsGUI();
            DrawCollectionsGUI();
            DrawCrossPromotionGUI();
        }

        public override bool RequiresConstantRepaint()
        {
            return false;
        }

        private void DrawInstructionGUI()
        {
            string label = "Instruction";
            string id = "instruction" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                EditorGUILayout.LabelField(
                    "Polaris is more than a terrain engine. It's an ecosystem specialized for Low Poly scene creation!\n" +
                    "Below are some asset suggestions which we found helpful to enhance your scene.",
                    GEditorCommon.WordWrapItalicLabel);
            });
        }

        private void DrawFeaturedAssetsGUI()
        {
            string label = "Featured Assets";
            string id = "featured-assets" + instance.GetInstanceID();

            GEditorCommon.Foldout(label, true, id, () =>
            {
                DrawFeatureAssetEntry(
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Editor/Textures/PolarisIcon.png",
                    "Polaris",
                    "Low poly terrain modeling, texturing and planting.",
                    "Leave a review",
                    GAssetExplorer.POLARIS_LINK);

                GEditorCommon.SpacePixel(0);

                DrawFeatureAssetEntry(
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Editor/Textures/PoseidonIcon.png",
                    "Poseidon",
                    "Low poly water with high fidelity and performance.",
                    GPackageInitializer.isPoseidonInstalled ? "Leave a review" : "Learn more",
                    GAssetExplorer.POSEIDON_LINK);

                GEditorCommon.SpacePixel(0);

                DrawFeatureAssetEntry(
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Editor/Textures/JupiterIcon.png",
                    "Jupiter",
                    "Single-pass procedural sky with day night cycle.",
                    GPackageInitializer.isJupiterInstalled ? "Leave a review" : "Learn more",
                    GAssetExplorer.JUPITER_LINK);

                GEditorCommon.SpacePixel(0);

                DrawFeatureAssetEntry(
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Editor/Textures/CSharpWizardIcon.png",
                    "CSharp Wizard",
                    "Skeleton code for C# and Polaris Extension.",
                    GPackageInitializer.isCSharpWizardInstalled ? "Leave a review" : "Learn more",
                    GAssetExplorer.CSHARP_WIZARD_LINK);

                GEditorCommon.SpacePixel(0);

                DrawFeatureAssetEntry(
                    "Assets/Polaris - Low Poly Ecosystem/Polaris - Low Poly Terrain Engine/Editor/Textures/MeshToFileIcon.png",
                    "Mesh To File",
                    "Export mesh and Polaris terrain to 3D files.",
                    GPackageInitializer.isMeshToFileInstalled ? "Leave a review" : "Learn more",
                    GAssetExplorer.MESH_TO_FILE_LINK);
            });
        }

        private void DrawFeatureAssetEntry(
            string iconPath,
            string title,
            string description,
            string cta,
            string link)
        {
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.GetControlRect(GUILayout.Width(GEditorCommon.indentSpace));
            Rect iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            EditorGUI.DrawPreviewTexture(iconRect, icon ?? Texture2D.blackTexture);

            EditorGUILayout.BeginVertical();
            Rect titleRect = EditorGUILayout.GetControlRect(GUILayout.Height(17));
            EditorGUI.LabelField(titleRect, title, TitleStyle);
            EditorGUILayout.LabelField(GEditorCommon.Ellipsis(description, 50), GEditorCommon.WordWrapItalicLabel);
            Rect ctaRect = EditorGUILayout.GetControlRect();
            string colorStr = ColorUtility.ToHtmlStringRGBA(GEditorCommon.selectedItemColor);
            string ctaStr = "<i><color=#" + colorStr + ">" + GEditorCommon.Ellipsis(cta, 25) + "</color></i>";
            if (GUI.Button(ctaRect, ctaStr, GEditorCommon.RichTextLabel))
            {
                Application.OpenURL(link);
            }
            EditorGUIUtility.AddCursorRect(ctaRect, MouseCursor.Link);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = indent;
        }

        private void DrawCollectionsGUI()
        {
            string label = "Collections";
            string id = "collections" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                Rect r;
                r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Other Assets From Pinwheel"))
                {
                    GAssetExplorer.ShowPinwheelAssets();
                }

                r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Vegetation"))
                {
                    GAssetExplorer.ShowVegetationLink();
                }

                r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Rock & Props"))
                {
                    GAssetExplorer.ShowRockPropsLink();
                }

                r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Character"))
                {
                    GAssetExplorer.ShowCharacterLink();
                }
            });
        }

        private void DrawCrossPromotionGUI()
        {
            string label = "Cross Promotion";
            string id = "crosspromo" + instance.GetInstanceID().ToString();
            GEditorCommon.Foldout(label, true, id, () =>
            {
                string text = "Are you a Publisher, send us a message to get more expose to the community!";
                EditorGUILayout.LabelField(text, GEditorCommon.WordWrapItalicLabel);
                Rect r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, "Send an Email"))
                {
                    GEditorCommon.OpenEmailEditor(
                        GCommon.BUSINESS_EMAIL,
                        "[Polaris] CROSS PROMOTION",
                        "DETAIL ABOUT YOUR ASSET HERE");
                }
            });
        }
    }
}
#endif
