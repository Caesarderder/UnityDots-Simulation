#if GRIFFIN
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public class GGridlineMeshCreatorWindow : EditorWindow
    {
        public GGenericContainer Container { get; set; }

        //[MenuItem("Window/Griffin/Tools/Gridline Mesh Creator")]
        public static void ShowWindow()
        {
            GGridlineMeshCreatorWindow window = GetWindow<GGridlineMeshCreatorWindow>();
            window.titleContent = new GUIContent("GGridlineMeshCreator");
            window.Show();
        }

        public void OnGUI()
        {
            Container = EditorGUILayout.ObjectField("Container", Container, typeof(GGenericContainer), false) as GGenericContainer;

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        private void Generate()
        {
            Mesh[] wireframeMeshes = new Mesh[GEditorSettings.Instance.livePreview.triangleMeshes.Length];
            for (int i = 0; i < wireframeMeshes.Length; ++i)
            {
                Mesh wm = ConvertToLineMesh(GEditorSettings.Instance.livePreview.triangleMeshes[i]);
                wm.name = "Wireframe Grid " + i.ToString();
                AssetDatabase.AddObjectToAsset(wm, Container);
                wireframeMeshes[i] = wm;
            }

            GEditorSettings.Instance.livePreview.wireframeMeshes = wireframeMeshes;
            EditorUtility.SetDirty(Container);
            EditorUtility.SetDirty(GEditorSettings.Instance);
        }

        private Mesh ConvertToLineMesh(Mesh m)
        {
            Mesh wm = new Mesh();
            wm.vertices = m.vertices;
            wm.uv = m.uv;
            wm.colors = m.colors;

            int[] tris = m.triangles;
            int trisCount = tris.Length / 3;

            List<int> indices = new List<int>();

            for (int i = 0; i < trisCount; ++i)
            {
                indices.Add(tris[i * 3 + 0]);
                indices.Add(tris[i * 3 + 1]);

                indices.Add(tris[i * 3 + 1]);
                indices.Add(tris[i * 3 + 2]);

                indices.Add(tris[i * 3 + 2]);
                indices.Add(tris[i * 3 + 0]);
            }

            wm.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

            return wm;
        }
    }
}
#endif
