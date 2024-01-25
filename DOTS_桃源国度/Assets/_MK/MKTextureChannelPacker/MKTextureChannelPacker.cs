#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MK.TextureChannelPacker
{
    public class MKTextureChannelPacker : EditorWindow
    {
        private GUIStyle flowTextStyle { get { return new GUIStyle(EditorStyles.label) { wordWrap = true }; } }
        
        private static readonly string _defaultFilePath = "Assets/_MK/MKTextureChannelPacker";
        private string _filePath = _defaultFilePath;
        private string _filename = "NewTexture";

        private Vector2 _scrollPos = Vector2.zero;
        private TextureFormat _outputFormat = TextureFormat.PNG;
        private int _outputWidth = 1024, _outputHeight = 1024;

        private enum TextureFormat
        {
            PNG = 0
            #if UNITY_2018_3_OR_NEWER
            ,
            TGA = 1
            #endif
        };

        private enum TextureChannel
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Alpha = 3
        };

        private enum ChannelColor
        {
            Black = 0,
            White = 1
        };

        private Texture2D _sourceTexture0 = null;
        private bool _sourceChanncel0Invert = false;
        private TextureChannel _sourceChannel0 = TextureChannel.Red;
        private TextureChannel _targetChannel0 = TextureChannel.Red;
        private ChannelColor _targetChannelColor0 = ChannelColor.Black;
        private Texture2D _sourceTexture1 = null;
        private bool _sourceChanncel1Invert = false;
        private TextureChannel _sourceChannel1 = TextureChannel.Green;
        private TextureChannel _targetChannel1 = TextureChannel.Green;
        private ChannelColor _targetChannelColor1 = ChannelColor.Black;
        private Texture2D _sourceTexture2 = null;
        private bool _sourceChanncel2Invert = false;
        private TextureChannel _sourceChannel2 = TextureChannel.Blue;
        private TextureChannel _targetChannel2 = TextureChannel.Blue;
        private ChannelColor _targetChannelColor2 = ChannelColor.Black;
        private Texture2D _sourceTexture3 = null;
        private bool _sourceChanncel3Invert = false;
        private TextureChannel _sourceChannel3 = TextureChannel.Alpha;
        private TextureChannel _targetChannel3 = TextureChannel.Alpha;
        private ChannelColor _targetChannelColor3 = ChannelColor.Black;

        [MenuItem("Window/MK/Texture Channel Packer")]
        static void Init()
        {
            MKTextureChannelPacker window = (MKTextureChannelPacker)EditorWindow.GetWindow<MKTextureChannelPacker>(true, "MK Texture Channel Packer", true);
            window.maxSize = new Vector2(360, 480);
            window.minSize = new Vector2(360, 480);
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, true);
            EditorGUILayout.LabelField("V 0.2");
            EditorGUILayout.LabelField("Pack different color channels of input textures and create a new texture. The Aspect Ratio of the input should match!", flowTextStyle);
            Divider();

            EditorGUILayout.LabelField("How to use:", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.LabelField("1. Select your input textures.");
            EditorGUILayout.LabelField("2. Select the input channels you want to pack.");
            EditorGUILayout.LabelField("3. Select the target channels where you want to output.");
            EditorGUILayout.LabelField("4. Run the operation via \"Create Texture\".", flowTextStyle);
            EditorGUILayout.LabelField("Existing files will be overwritten!", UnityEditor.EditorStyles.boldLabel);

            Divider();
            EditorGUILayout.LabelField("Input:", UnityEditor.EditorStyles.boldLabel);
            PackerPopup(ref _sourceTexture0, ref _sourceChannel0, ref _sourceChanncel0Invert, ref _targetChannel0, ref _targetChannelColor0);
            PackerPopup(ref _sourceTexture1, ref _sourceChannel1, ref _sourceChanncel1Invert, ref _targetChannel1, ref _targetChannelColor1);
            PackerPopup(ref _sourceTexture2, ref _sourceChannel2, ref _sourceChanncel2Invert, ref _targetChannel2, ref _targetChannelColor2);
            PackerPopup(ref _sourceTexture3, ref _sourceChannel3, ref _sourceChanncel3Invert, ref _targetChannel3, ref _targetChannelColor3);
            Divider();

            EditorGUILayout.LabelField("Output Settings:", UnityEditor.EditorStyles.boldLabel);
            _outputFormat = (TextureFormat)EditorGUILayout.EnumPopup("Format", _outputFormat);
            EditorGUILayout.BeginHorizontal();
            float labelWidthBase = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 37;
            _outputWidth = EditorGUILayout.IntField("Width", _outputWidth);
            EditorGUIUtility.labelWidth = 42;
            _outputHeight = EditorGUILayout.IntField("Height", _outputHeight);
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

            EditorGUILayout.EndScrollView();

            bool oversized = _outputWidth >= 16384 || _outputHeight >= 16384;
            if(oversized)
            {
                EditorGUILayout.LabelField("Texture width or height is oversized.", UnityEditor.EditorStyles.boldLabel);
            }

            bool doubledChannelUsed = false;
            if(CheckDoubledOutputChannel(_targetChannel0) || CheckDoubledOutputChannel(_targetChannel1) ||
                CheckDoubledOutputChannel(_targetChannel2) || CheckDoubledOutputChannel(_targetChannel3))
                {
                    doubledChannelUsed = true;
                    EditorGUILayout.LabelField("Every output channel has to be unique.", UnityEditor.EditorStyles.boldLabel);
                }

            if(!doubledChannelUsed && !oversized && _filename != "")
            {
                if(GUILayout.Button("Create Texture"))
                {
                    EditorUtility.DisplayProgressBar("Texture progress", "Creating new texture...", 0.5f);
                    Texture2D _outputTexture = new Texture2D(_outputWidth, _outputHeight);

                    bool ra0 = false, ra1 = false, ra2 = false, ra3 = false;
                    
                    if(_sourceTexture0)
                    {
                        ra0 = SetTextureReadable(_sourceTexture0);
                    }
                    if(_sourceTexture1)
                    {
                        ra1 = SetTextureReadable(_sourceTexture1);
                    }
                    if(_sourceTexture2)
                    {
                        ra2 = SetTextureReadable(_sourceTexture2);
                    }
                    if(_sourceTexture3)
                    {
                        ra3 = SetTextureReadable(_sourceTexture3);
                    }

                    for(int i = 0; i < _outputHeight; i++)
                    {
                        Vector2 pPos = new Vector2();
                        pPos.y = (float)i / (float)_outputHeight;

                        for(int j = 0; j < _outputWidth; j++)
                        {
                            pPos.x = (float)j / (float)_outputWidth;
                            Color pixelColor = Color.black, sourceRead0, sourceRead1, sourceRead2, sourceRead3;

                            sourceRead0 = ReadPixelValue(_sourceTexture0, pPos, _targetChannelColor0);
                            sourceRead1 = ReadPixelValue(_sourceTexture1, pPos, _targetChannelColor1);
                            sourceRead2 = ReadPixelValue(_sourceTexture2, pPos, _targetChannelColor2);
                            sourceRead3 = ReadPixelValue(_sourceTexture3, pPos, _targetChannelColor3);

                            PackColorChannelValueIntoChannel(sourceRead0, _sourceChannel0, _targetChannel0, _sourceChanncel0Invert, ref pixelColor);
                            PackColorChannelValueIntoChannel(sourceRead1, _sourceChannel1, _targetChannel1, _sourceChanncel1Invert, ref pixelColor);
                            PackColorChannelValueIntoChannel(sourceRead2, _sourceChannel2, _targetChannel2, _sourceChanncel2Invert, ref pixelColor);
                            PackColorChannelValueIntoChannel(sourceRead3, _sourceChannel3, _targetChannel3, _sourceChanncel3Invert, ref pixelColor);

                            _outputTexture.SetPixel(j, i, pixelColor);
                        }
                    }
                    _outputTexture.Apply();
                    SaveOutputTexture(_outputTexture, _filePath, FilterFilename(_filename), _outputFormat, _outputWidth >= _outputHeight ? _outputWidth : _outputHeight);

                    if(_sourceTexture0)
                        SetTextureReadable(_sourceTexture0, ra0);
                    if(_sourceTexture1)
                        SetTextureReadable(_sourceTexture1, ra1);
                    if(_sourceTexture2)
                        SetTextureReadable(_sourceTexture2, ra2);
                    if(_sourceTexture3)
                        SetTextureReadable(_sourceTexture3, ra3);

                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private static bool SetTextureReadable(Texture2D texture, bool setReadable = true)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            bool isReadable = textureImporter.isReadable;
            textureImporter.isReadable = setReadable ? true : false;
            AssetDatabase.ImportAsset(texturePath);
            AssetDatabase.Refresh();

            return isReadable;
        }

        private static void PackColorChannelValueIntoChannel(Color source, TextureChannel sourceChannel, TextureChannel targetChannel, bool invertChannel, ref Color color)
        {
            source = invertChannel ? Color.white - source : source;
            switch(sourceChannel)
            {
                case TextureChannel.Alpha:
                    switch(targetChannel)
                    {
                        case TextureChannel.Alpha:
                            color.a = source.a;
                        break;
                        case TextureChannel.Blue:
                            color.b = source.a;
                        break;
                        case TextureChannel.Green:
                            color.g = source.a;
                        break;
                        default:
                            color.r = source.a;
                        break;
                    }
                break;
                case TextureChannel.Blue:
                    switch(targetChannel)
                    {
                        case TextureChannel.Alpha:
                            color.a = source.b;
                        break;
                        case TextureChannel.Blue:
                            color.b = source.b;
                        break;
                        case TextureChannel.Green:
                            color.g = source.b;
                        break;
                        default:
                            color.r = source.b;
                        break;
                    }
                break;
                case TextureChannel.Green:
                    switch(targetChannel)
                    {
                        case TextureChannel.Alpha:
                            color.a = source.g;
                        break;
                        case TextureChannel.Blue:
                            color.b = source.g;
                        break;
                        case TextureChannel.Green:
                            color.g = source.g;
                        break;
                        default:
                            color.r = source.g;
                        break;
                    }
                break;
                default:
                    switch(targetChannel)
                    {
                        case TextureChannel.Alpha:
                            color.a = source.r;
                        break;
                        case TextureChannel.Blue:
                            color.b = source.r;
                        break;
                        case TextureChannel.Green:
                            color.g = source.r;
                        break;
                        default:
                            color.r = source.r;
                        break;
                    }
                break;
            }
        }

        private static Color ReadPixelValue(Texture2D texture, Vector2 pPos, ChannelColor channelColor)
        {
            if(texture != null)
            {
                int width, height;
                width = Mathf.RoundToInt((float)texture.width * pPos.x);
                height = Mathf.RoundToInt((float)texture.height * pPos.y);

                return texture.GetPixel(width, height);
            }
            else
            {
                Color color;

                switch(channelColor)
                {
                    case ChannelColor.Black:
                        color = new Color(0,0,0,1);
                    break;
                    case ChannelColor.White:
                        color = new Color(1,1,1,1);
                    break;
                    default:
                        color = new Color(0,0,0,1);
                    break;
                }

                return color;
            }
        }

        private static void PackerPopup(ref Texture2D texture, ref TextureChannel sourceChannel, ref bool sourceChannelInvert, ref TextureChannel targetChannel, ref ChannelColor targetChannelColor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            texture = (Texture2D) EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            EditorGUILayout.EndVertical();
            if(texture != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Invert?:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                sourceChannelInvert = EditorGUILayout.Toggle(sourceChannelInvert, GUILayout.Width(70));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("From:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                sourceChannel = (TextureChannel) EditorGUILayout.EnumPopup(sourceChannel,  GUILayout.Width(70));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("To:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                targetChannel = (TextureChannel) EditorGUILayout.EnumPopup(targetChannel,  GUILayout.Width(70));
                EditorGUILayout.EndVertical();
            }
            else
            {
                sourceChannelInvert = false;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Assume:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                targetChannelColor = (ChannelColor) EditorGUILayout.EnumPopup(targetChannelColor,  GUILayout.Width(70));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("From:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                sourceChannel = (TextureChannel) EditorGUILayout.EnumPopup(sourceChannel,  GUILayout.Width(70));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("To:", UnityEditor.EditorStyles.boldLabel,  GUILayout.Width(70));
                targetChannel = (TextureChannel) EditorGUILayout.EnumPopup(targetChannel,  GUILayout.Width(70));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool CheckDoubledOutputChannel(TextureChannel channel)
        {
            int channelUsageCount = 0;
            if(channel == _targetChannel0)
                channelUsageCount++;
            if(channel == _targetChannel1)
                channelUsageCount++;
            if(channel == _targetChannel2)
                channelUsageCount++;
            if(channel == _targetChannel3)
                channelUsageCount++;
            
            if(channelUsageCount > 1)
                return true;
            else
                return false;
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

        private static void SaveOutputTexture(Texture2D tex, string path, string name, TextureFormat format, int maxSize)
        {
            path = path.Substring(path.IndexOf("Assets"));
            path += "/";
            
            string fileSuffix = "tex";
            #if UNITY_2018_3_OR_NEWER
            if(format == TextureFormat.PNG)
                fileSuffix = ".png";
            else
                fileSuffix = ".tga";
            #endif

            path = path + name + fileSuffix;

            #if UNITY_2018_3_OR_NEWER
            if(format == TextureFormat.PNG)
                System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            else
                System.IO.File.WriteAllBytes(path, tex.EncodeToTGA());
            #endif

            AssetDatabase.Refresh();

            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);
            textureImporter.wrapMode = TextureWrapMode.Repeat;
            textureImporter.maxTextureSize = maxSize;
            textureImporter.alphaSource = TextureImporterAlphaSource.None;
            textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
            textureImporter.mipmapEnabled = true;

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
#endif