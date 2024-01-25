#if GRIFFIN
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GUvLayoutExporterWindow : EditorWindow
    {
        private Mesh targetMesh;
        private int resolution;

        static Material lineMaterial;

        //[MenuItem("Window/Griffin/Internal/UV Layout Exporter")]
        public static void ShowWindow()
        {
            GUvLayoutExporterWindow window = GetWindow<GUvLayoutExporterWindow>();
            window.titleContent = new GUIContent("GUvLayoutExporter");
            window.Show();
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public void OnGUI()
        {
            targetMesh = EditorGUILayout.ObjectField("Mesh", targetMesh, typeof(Mesh), false) as Mesh;
            resolution = EditorGUILayout.IntField("Resolution", resolution);
            if (GUILayout.Button("Export"))
            {
                Export();
            }
        }

        private void Export()
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
            GCommon.FillTexture(rt, Color.clear);

            Vector2[] uv = targetMesh.uv;
            int[] tris = targetMesh.triangles;
            int trisCount = tris.Length / 3;

            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadOrtho();
            // Draw lines
            GL.Begin(GL.LINES);
            for (int i = 0; i < trisCount; ++i)
            {
                GL.Color(Color.black);
                Vector2 p0 = uv[tris[i * 3 + 0]];
                Vector2 p1 = uv[tris[i * 3 + 1]];
                Vector2 p2 = uv[tris[i * 3 + 2]];

                GL.Vertex3(p0.x, p0.y, 0);
                GL.Vertex3(p1.x, p1.y, 0);

                GL.Vertex3(p1.x, p1.y, 0);
                GL.Vertex3(p2.x, p2.y, 0);

                GL.Vertex3(p2.x, p2.y, 0);
                GL.Vertex3(p0.x, p0.y, 0);
            }
            GL.End();
            GL.PopMatrix();

            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            tex.Apply();

            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(string.Format("Assets/{0}_UV_{1}.png", targetMesh.name, resolution), data);
            GUtilities.DestroyObject(tex);

            RenderTexture.active = null;
            rt.Release();
            GUtilities.DestroyObject(rt);
        }

        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }
    }
}
#endif
