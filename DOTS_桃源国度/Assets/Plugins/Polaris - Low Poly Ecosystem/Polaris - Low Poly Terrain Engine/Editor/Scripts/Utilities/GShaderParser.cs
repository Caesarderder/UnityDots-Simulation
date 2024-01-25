#if GRIFFIN
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GShaderParser
    {
        //private static string debugFile = "Assets/debug.txt";

        //[MenuItem("Testing/Debug Parse Shader")]
        public static void Debug()
        {
            Shader shader = Shader.Find("SyntyStudios/Trees");
            GetProperties(shader, "Color");
        }

        public static List<string> GetProperties(Shader shader, string type)
        {
            List<string> props = new List<string>();
            List<string> lines = GetShaderContent(shader);
            CleanUpContent(lines);
            ExtractPropertiesLines(lines);
            CleanUpContent(lines);

            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i] = lines[i].Replace(" ", "").Trim();
                if (lines[i].Contains("," + type))
                {
                    int openParathesisPos = lines[i].IndexOf("(");
                    if (openParathesisPos > 0)
                    {
                        props.Add(lines[i].Substring(0, openParathesisPos));
                    }
                }
            }

            //File.WriteAllLines(debugFile, props.ToArray());

            return props;
        }

        private static List<string> GetShaderContent(Shader shader)
        {
            List<string> lines = new List<string>();
            string path = AssetDatabase.GetAssetPath(shader);
            if (File.Exists(path))
            {
                lines.AddRange(File.ReadAllLines(path));
            }

            return lines;
        }

        private static void CleanUpContent(List<string> lines)
        {
            //remove double slashes comments
            for (int i = 0; i < lines.Count; ++i)
            {
                int doubleSlashesPosition = lines[i].IndexOf("//");
                if (doubleSlashesPosition >= 0)
                {
                    lines[i] = lines[i].Remove(doubleSlashesPosition);
                }
            }

            //remove block comments
            bool commentStarted = false;
            for (int i = 0; i < lines.Count; ++i)
            {
                if (!commentStarted)
                {
                    int pos = lines[i].IndexOf("/*");
                    if (pos >= 0)
                    {
                        lines[i] = lines[i].Remove(pos);
                        commentStarted = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    int pos = lines[i].IndexOf("*/");
                    if (pos < 0)
                    {
                        lines[i] = string.Empty;
                    }
                    else
                    {
                        lines[i] = lines[i].Remove(0, pos + 2);
                        commentStarted = false;
                    }
                }
            }

            //remove indentations
            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i] = lines[i].Replace("\t", "").Trim();
            }

            //remove empty lines
            lines.RemoveAll(s => string.IsNullOrEmpty(s) || s.Equals("\n"));
        }

        private static void ExtractPropertiesLines(List<string> lines)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < lines.Count; ++i)
            {
                s.AppendLine(lines[i]);
            }

            string content = s.ToString();
            int pos = content.IndexOf("Properties");

            if (pos < 0)
            {
                lines.Clear();
                return;
            }

            int openBracketPos = content.IndexOf("{", pos);
            int closeBracketPos = openBracketPos;
            int blockLevel = 1;
            for (int i = openBracketPos + 1; i < content.Length; ++i)
            {
                if (content[i].Equals('{'))
                {
                    blockLevel += 1;
                }
                if (content[i].Equals('}'))
                {
                    blockLevel -= 1;
                    if (blockLevel == 0)
                    {
                        closeBracketPos = i;
                        break;
                    }
                }
            }

            string propertiesContent = content.Substring(openBracketPos + 1, closeBracketPos - openBracketPos - 1);

            lines.Clear();
            lines.AddRange(propertiesContent.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries));

            //remove attributes
            for (int i = 0; i < lines.Count; ++i)
            {
                int lastBracket = lines[i].LastIndexOf("]");
                if (lastBracket >= 0)
                {
                    lines[i] = lines[i].Remove(0, lastBracket + 1);
                }
            }
        }
    }
}
#endif
