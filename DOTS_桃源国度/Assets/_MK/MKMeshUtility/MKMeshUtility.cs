#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MK.MeshUtility
{
    public class MKMeshUtility : EditorWindow
    {
        private class MeshData
        {
            public MeshData(MeshFilter meshFilter)
            {
                this._meshFilter = meshFilter;
            }

            public MeshData(SkinnedMeshRenderer skinnedMeshRenderer)
            {
                this._skinnedMeshRenderer = skinnedMeshRenderer;
            }

            private MeshFilter _meshFilter;
            private SkinnedMeshRenderer _skinnedMeshRenderer;

            public Mesh mesh 
            {
                get
                {
                    return _meshFilter ? _meshFilter.sharedMesh : _skinnedMeshRenderer.sharedMesh; 
                }
                set
                {
                    if(_meshFilter)
                    {
                        _meshFilter.sharedMesh = value;
                    }
                    else
                    {
                        _skinnedMeshRenderer.sharedMesh = value;
                    }
                }
            }
        }

        private enum Operation
        {
            AverageNormals = 1,
            CenterGeometry = 2
        };

        private enum NormalsOutputTarget
        {
            OverwriteNormals = 0,
            UV7 = 7,
        };

        private GUIStyle flowTextStyle { get { return new GUIStyle(EditorStyles.label) { wordWrap = true }; } }
        private Operation _operation = Operation.AverageNormals;
        private NormalsOutputTarget _normalsOutputTarget = NormalsOutputTarget.OverwriteNormals;
        private string _filenameSuffix = "";
        private static readonly string _defaultFilePath = "Assets/_MK/MKMeshUtility";
        private string _filePath = _defaultFilePath;
        private List<MeshData> _selectedMeshes;

        private List<GameObject> _allSelectedObjects;
        private void SelectAllObjects()
        {
            _allSelectedObjects = new List<GameObject>();
            _selectedMeshes = new List<MeshData>();
            foreach(GameObject go in Selection.gameObjects)
            {
                CheckDuplicatedSelection(go);
                GetObjectsRecursively(go);
            }
        }

        private void CheckDuplicatedSelection(GameObject go)
        {
            foreach(GameObject g in _allSelectedObjects)
            {
                if(g == go)
                    return;
            }

            _allSelectedObjects.Add(go);
        }

        private void GetObjectsRecursively(GameObject go)
        {
            if(go == null)
                return;

            for(int i = 0; i < go.transform.childCount; i++)
            {
                if(go.transform == null)
                    continue;

                Transform ct = go.transform.GetChild(i);
                CheckDuplicatedSelection(ct.gameObject);
                GetObjectsRecursively(ct.gameObject);
            }
        }

        private void SelectMeshes()
        {
            foreach(GameObject o in _allSelectedObjects)
			{
                bool assetInProject = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o));
                if(!assetInProject)
                {
                    MeshFilter meshFilter = (o as GameObject).GetComponent<MeshFilter>();
                    SkinnedMeshRenderer skinnedMeshRenderer = (o as GameObject).GetComponent<SkinnedMeshRenderer>();

                    if(skinnedMeshRenderer && _normalsOutputTarget == NormalsOutputTarget.OverwriteNormals)
                    {
                        if(skinnedMeshRenderer.sharedMesh)
                            _selectedMeshes.Add(new MeshData(skinnedMeshRenderer));
                    }
                    else if(meshFilter)
                    {
                        if(meshFilter.sharedMesh)
                            _selectedMeshes.Add(new MeshData(meshFilter));
                    }
                }
            }
        }

        private void OnFocus()
		{   
            SelectAllObjects();
            SelectMeshes();
            Repaint();
		}

		private void OnSelectionChange()
		{
			SelectAllObjects();
            SelectMeshes();
            Repaint();
        }

        [MenuItem("Window/MK/Mesh Utility")]
        static void Init()
        {
            MKMeshUtility window = (MKMeshUtility)EditorWindow.GetWindow<MKMeshUtility>(true, "MK Mesh Utility", true);
            window.maxSize = new Vector2(360, 435);
            window.minSize = new Vector2(360, 435);
            window.Show();
        }

        private Mesh MeshFromObjects(int i)
        {
            return _allSelectedObjects[i].GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("V 0.1");
            EditorGUILayout.LabelField("This tool is mainly created to be used with MK Toon to achieve smooth outlines. It creates new meshes based on the set operation.", flowTextStyle);
            EditorGUILayout.LabelField("For best practice you want to smooth/average your mesh normals in a 3D program and save them to the UV7 channel (Outline Data: Baked), or center your mesh geometry around (0,0,0) in local space (Outline: HullOrigin).", flowTextStyle);
            Divider();

            EditorGUILayout.LabelField("How to use:", UnityEditor.EditorStyles.boldLabel);
            EditorGUILayout.LabelField("1. Select meshes from your hierarchy (objects, which have a Mesh attached).", flowTextStyle);
            EditorGUILayout.LabelField("2. Pick your operation.");
            EditorGUILayout.LabelField("3. Set your target folder.");
            EditorGUILayout.LabelField("4. Run the operation. All selected meshes will be switched automatically.", flowTextStyle);
            EditorGUILayout.LabelField("Existing files will be overwritten!", UnityEditor.EditorStyles.boldLabel);
            Divider();
           
            _operation = (Operation)EditorGUILayout.EnumPopup(new GUIContent("Operation", "AverageNormals: Normals get smoothed based on a average value. + \n\n" +
                                                                                           "CenterGeomtry: The geometry gets centered around (0,0,0) in local space. Should be use with origin normal mode."), _operation);

            if(_operation == Operation.AverageNormals)
                _normalsOutputTarget = (NormalsOutputTarget)EditorGUILayout.EnumPopup(new GUIContent("Normals Write Target", "OverwriteNormals: Overwrite the original normals of the mesh. \n\n" +
                                                                                                                             "Baked: Normals are written onto the UVX vertex input."), _normalsOutputTarget);
            EditorGUILayout.BeginHorizontal();
            _filePath = EditorGUILayout.TextField("Output Folder", _filePath);
            if(GUILayout.Button("Select"))
                _filePath = EditorUtility.SaveFolderPanel("Output Folder", _filePath, "");
            if(_filePath == "")
                _filePath = _defaultFilePath;
            EditorGUILayout.EndHorizontal();
            _filenameSuffix = EditorGUILayout.TextField("Filename Suffix", _filenameSuffix);

            Divider();
            EditorGUILayout.LabelField("Selected Meshes: " + _selectedMeshes.Count.ToString());

            Divider();
            if(_selectedMeshes.Count <= 0)
                EditorGUILayout.LabelField("Please select at least one mesh.", UnityEditor.EditorStyles.boldLabel);
            if(_selectedMeshes.Count > 0)
            if(GUILayout.Button(_operation == Operation.CenterGeometry ? "Center Geometry" : "Average Normals"))
            {
                EditorUtility.DisplayProgressBar("Mesh progress", "Creating meshes...", 0.5f);
                for(int k = 0; k < _selectedMeshes.Count; k ++)
                {
                    List<Vector3> smoothedNormals = new List<Vector3>();
                    List<Vector3> smoothedVertices = new List<Vector3>();

                    if(_operation == Operation.AverageNormals)
                    {
                        Dictionary<Vector3, Vector3> _smoothedNormals = new Dictionary<Vector3, Vector3>();
                        Mesh mesh = _selectedMeshes[k].mesh;   
                        int l = mesh.vertices.Length;

                        for(int i = 0; i < l; i++)
                        {
                            if(!_smoothedNormals.ContainsKey(mesh.vertices[i]))
                                _smoothedNormals.Add(mesh.vertices[i], mesh.normals[i]);
                            else
                            {
                                Vector3 value = Vector3.zero;
                                _smoothedNormals.TryGetValue(mesh.vertices[i], out value);
                                value += mesh.normals[i];
                                _smoothedNormals.Remove(mesh.vertices[i]);
                                _smoothedNormals.Add(mesh.vertices[i], value);
                            }
                        }
                        for(int i = 0; i < l; i++)
                        {
                            Vector3 n = Vector3.zero;
                            _smoothedNormals.TryGetValue(mesh.vertices[i], out n);
                            n.Normalize();

                            smoothedNormals.Add(n);
                            smoothedVertices.Add(mesh.vertices[i]);
                        }
                    }
                    else
                    {
                        for(int i = 0; i < _selectedMeshes[k].mesh.vertices.Length; i++)
                        {
                            Vector3 centerOffset = _selectedMeshes[k].mesh.bounds.center;
                            smoothedNormals.Add(_selectedMeshes[k].mesh.normals[i]);
                            smoothedVertices.Add(_selectedMeshes[k].mesh.vertices[i]);
                        }
                    }
                    
                    //Create smoothed Mesh
                    Mesh modifiedMesh = new Mesh();
                    modifiedMesh.name = FilterFilename(_selectedMeshes[k].mesh.name + _filenameSuffix);

                    //Set basic _selectedMeshes[k] data
                    modifiedMesh.vertices = smoothedVertices.ToArray();
                    modifiedMesh.triangles = _selectedMeshes[k].mesh.triangles;
                    modifiedMesh.normals = (_operation == Operation.AverageNormals && (_normalsOutputTarget == NormalsOutputTarget.UV7)) ? _selectedMeshes[k].mesh.normals : smoothedNormals.ToArray();
                    //modifiedMesh.tangents = _selectedMeshes[k].mesh.tangents;
                    modifiedMesh.subMeshCount = _selectedMeshes[k].mesh.subMeshCount;

                    if (modifiedMesh.subMeshCount > 1)
                        for (int i = 0; i < _selectedMeshes[k].mesh.subMeshCount; i++)
                            modifiedMesh.SetTriangles(_selectedMeshes[k].mesh.GetTriangles(i), i);

                    //Set additional _selectedMeshes[k] data
                    List<Vector4> uv0 = new List<Vector4>();
                    List<Vector4> uv1 = new List<Vector4>();
                    List<Vector4> uv2 = new List<Vector4>();
                    List<Vector4> uv3 = new List<Vector4>();
                    List<Vector4> uv4 = new List<Vector4>();
                    List<Vector4> uv5 = new List<Vector4>();
                    List<Vector4> uv6 = new List<Vector4>();
                    _selectedMeshes[k].mesh.GetUVs(0, uv0);
                    _selectedMeshes[k].mesh.GetUVs(1, uv1);
                    _selectedMeshes[k].mesh.GetUVs(2, uv2);
                    _selectedMeshes[k].mesh.GetUVs(3, uv3);
                    _selectedMeshes[k].mesh.GetUVs(4, uv4);
                    _selectedMeshes[k].mesh.GetUVs(5, uv5);
                    _selectedMeshes[k].mesh.GetUVs(6, uv6);
                    modifiedMesh.SetUVs(0, uv0);
                    modifiedMesh.SetUVs(1, uv1);
                    modifiedMesh.SetUVs(2, uv2);
                    modifiedMesh.SetUVs(3, uv3);
                    modifiedMesh.SetUVs(4, uv4);
                    modifiedMesh.SetUVs(5, uv5);
                    modifiedMesh.SetUVs(6, uv6);
                    if(_operation == Operation.AverageNormals && _normalsOutputTarget == NormalsOutputTarget.UV7)
                        modifiedMesh.SetUVs(7, smoothedNormals);

                    modifiedMesh.colors = _selectedMeshes[k].mesh.colors;
                    CopyBlendShapes(_selectedMeshes[k].mesh, modifiedMesh);
                    modifiedMesh.bindposes = _selectedMeshes[k].mesh.bindposes;
                    modifiedMesh.boneWeights = _selectedMeshes[k].mesh.boneWeights;
                    #if UNITY_2017_3_OR_NEWER
                    modifiedMesh.indexFormat = _selectedMeshes[k].mesh.indexFormat;
                    #endif
                    modifiedMesh.RecalculateBounds();
                    modifiedMesh.RecalculateTangents();

                    _selectedMeshes[k].mesh = SaveModifiedMesh(modifiedMesh, _filePath, modifiedMesh.name);
                }

                foreach(GameObject go in _allSelectedObjects)
                    EditorUtility.SetDirty(go);
                    
                EditorUtility.ClearProgressBar();

                SelectModifiedMesh(_selectedMeshes[_selectedMeshes.Count - 1].mesh, _filePath, _selectedMeshes[_selectedMeshes.Count - 1].mesh.name);
            }
        }

        private static void CopyBlendShapes(Mesh mesh, Mesh newMesh)
        {
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                for (int j = 0; j < mesh.GetBlendShapeFrameCount(i); j++)
                {
                    Vector3[] dv = new Vector3[mesh.vertexCount];
                    Vector3[] dn = new Vector3[mesh.vertexCount];
                    Vector3[] dt = new Vector3[mesh.vertexCount];

                    float blendShapeFrameWeight = mesh.GetBlendShapeFrameWeight(i, j);
                    mesh.GetBlendShapeFrameVertices(i, j, dv, dn, dt);
                    newMesh.AddBlendShapeFrame(mesh.GetBlendShapeName(i), blendShapeFrameWeight, dv, dn, dt);
                }
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

        private static Mesh SaveModifiedMesh(Mesh mesh, string path, string name)
        {
            path = path.Substring(path.IndexOf("Assets"));
            path = path + "/" +  name + ".asset";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            return (Mesh) AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }

        private static void SelectModifiedMesh(Mesh mesh, string path, string name)
        {
            path = path.Substring(path.IndexOf("Assets"));
            path = path + "/" +  name + ".asset";
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
#endif