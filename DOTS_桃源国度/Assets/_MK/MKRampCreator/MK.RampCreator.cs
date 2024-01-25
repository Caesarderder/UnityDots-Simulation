#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace MK.RampCreator
{
    public class MKRampCreator : EditorWindow
    {
        SerializedObject serializedObject;
        private SerializedProperty _outputGradientProperty;
        private GUIStyle flowTextStyle { get { return new GUIStyle(EditorStyles.label) { wordWrap = true }; } }

        private static string _defaultFilePath = "Assets/_MK/MKRampCreator";
        private string _filePath = _defaultFilePath;
        private string _filename = "NewRamp";

        private TextureFormat _outputFormat = TextureFormat.PNG;
        private TextureWidth _outputWidth = TextureWidth._512;
        [SerializeField]
        private Gradient _outputGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.black, 0),
                new GradientColorKey(Color.white, 1)
            }
        };

        private enum TextureFormat
        {
            PNG = 0,
            #if UNITY_2018_3_OR_NEWER
            TGA = 1,
            #endif
            JPG = 2
        };

        private enum TextureWidth
        {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048
        };

        [MenuItem("Window/MK/Ramp Creator")]
        static void Init()
        {
            MKRampCreator window = (MKRampCreator)EditorWindow.GetWindow<MKRampCreator>(true, "MK Ramp Creator", true);
            window.maxSize = new Vector2(360, 360);
            window.minSize = new Vector2(360, 360);
            window.Show();
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            _outputGradientProperty = serializedObject.FindProperty("_outputGradient");
        }

        private void OnGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("V 0.1");
            EditorGUILayout.LabelField("This tool creates ramp maps based on a gradient.", flowTextStyle);
            Divider();

            EditorGUILayout.LabelField("How to use:", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.LabelField("1. Create your gradient.");
            EditorGUILayout.LabelField("2. Setup the output settings.");
            EditorGUILayout.LabelField("3. Run the operation via \"Create Ramp\".");
            EditorGUILayout.LabelField("Existing files will be overwritten!", UnityEditor.EditorStyles.boldLabel);
            Divider();

            EditorGUILayout.LabelField("Input:", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Dark -> Bright");
            EditorGUILayout.PropertyField(_outputGradientProperty, new GUIContent("Gradient"));
            Divider();

            EditorGUILayout.LabelField("Output Settings:", UnityEditor.EditorStyles.boldLabel);
            _outputFormat = (TextureFormat)EditorGUILayout.EnumPopup("Format", _outputFormat);
            EditorGUILayout.BeginHorizontal();
            float labelWidthBase = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 37;
            _outputWidth = (TextureWidth)EditorGUILayout.EnumPopup("Size", _outputWidth);
            EditorGUIUtility.labelWidth = 42;
            EditorGUIUtility.labelWidth = labelWidthBase;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _filePath = EditorGUILayout.TextField("Output Folder", _filePath);
            if(GUILayout.Button("Select"))
                _filePath = EditorUtility.SaveFolderPanel("Output Folder", _filePath, "");
            if(_filePath == "")
                _filePath = _defaultFilePath;
            EditorGUILayout.EndHorizontal();
             _filename = EditorGUILayout.TextField("Filename", _filename);
            Divider();

            serializedObject.ApplyModifiedProperties();

            if(_filename != "")
            if(GUILayout.Button("Create Ramp"))
            {
                EditorUtility.DisplayProgressBar("Texture progress", "Creating new texture...", 0.5f);
                Texture2D _outputTexture = new Texture2D((int)_outputWidth, 4);

                for(int i = 0; i < (int)_outputWidth; i++)
                {
                    float pPos = (float)i / (float)_outputWidth;
                    Color pixelColor = _outputGradient.Evaluate(pPos);
                    _outputTexture.SetPixel(i, 0, pixelColor);
                    _outputTexture.SetPixel(i, 1, pixelColor);
                    _outputTexture.SetPixel(i, 2, pixelColor);
                    _outputTexture.SetPixel(i, 3, pixelColor);
                }

                _outputTexture.Apply();
                SaveOutputTexture(_outputTexture, _filePath, FilterFilename(_filename), _outputFormat, _outputWidth);
                EditorUtility.ClearProgressBar();
            }
        }

        private static void Divider()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
        }
        
        private static string FilterFilename(string name)
		{
			List<char> notAllowedFilenameChars = new List<char>(System.IO.Path.GetInvalidFileNameChars());
            List<char> filename = new List<char>();

            foreach(char c in name)
			{
				if(!notAllowedFilenameChars.Contains(c))
					filename.Add(c);
			}

			return new string(filename.ToArray());
		}

        private static void SaveOutputTexture(Texture2D tex, string path, string name, TextureFormat format, TextureWidth width)
        {
            //path = System.Text.RegularExpressions.Regex.Replace(path, ".*:Assets:", "");
            path = path.Substring(path.IndexOf("Assets"));
            path += "/";

            string fileSuffix;
            if(format == TextureFormat.PNG)
                fileSuffix = ".png";
            #if UNITY_2018_3_OR_NEWER
            else if(format == TextureFormat.TGA)
                fileSuffix = ".tga";
            #endif
            else
                fileSuffix = ".jpg";

            if(format == TextureFormat.PNG)
                System.IO.File.WriteAllBytes(path + name + fileSuffix, tex.EncodeToPNG());
            #if UNITY_2018_3_OR_NEWER
            else if(format == TextureFormat.TGA)
                System.IO.File.WriteAllBytes(path + name + fileSuffix, tex.EncodeToTGA());
            #endif
            else
                System.IO.File.WriteAllBytes(path + name + fileSuffix, tex.EncodeToJPG());

            AssetDatabase.Refresh();

            string texturePath = path + name + fileSuffix;
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.maxTextureSize = (int) width;
            textureImporter.alphaSource = TextureImporterAlphaSource.None;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
            textureImporter.mipmapEnabled = true;
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(texturePath);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(texturePath, typeof(UnityEngine.Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
#endif