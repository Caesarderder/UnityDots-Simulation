#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
#endif
using Unity.Collections;
using Unity.Mathematics;

namespace Pinwheel.Griffin
{
    /// <summary>
    /// Utility class for helper function
    /// </summary>
    public static class GUtilities
    {
        public static float DELTA_TIME = 0.0167f;

        private static Mesh quadMesh;
        public static Mesh QuadMesh
        {
            get
            {
                if (quadMesh == null)
                {
                    quadMesh = GUtilities.GetPrimitiveMesh(PrimitiveType.Quad);
                }
                return quadMesh;
            }
        }

        public static string ListElementsToString<T>(this IEnumerable<T> list, string separator)
        {
            IEnumerator<T> i = list.GetEnumerator();
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            if (i.MoveNext())
                s.Append(i.Current.ToString());
            while (i.MoveNext())
                s.Append(separator).Append(i.Current.ToString());
            return s.ToString();
        }

        public static T[][] CreateJaggedArray<T>(int dimension1, int dimension2)
        {
            T[][] jaggedArray = new T[dimension1][];
            for (int i = 0; i < dimension1; ++i)
            {
                jaggedArray[i] = new T[dimension2];
            }
            return jaggedArray;
        }

        public static T[] To1dArray<T>(T[][] jaggedArray)
        {
            List<T> result = new List<T>();
            for (int z = 0; z < jaggedArray.Length; ++z)
            {
                for (int x = 0; x < jaggedArray[z].Length; ++x)
                {
                    result.Add(jaggedArray[z][x]);
                }
            }
            return result.ToArray();
        }

        public static T[] To1dArray<T>(T[,] grid)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);
            T[] result = new T[height * width];
            for (int z = 0; z < height; ++z)
            {
                for (int x = 0; x < width; ++x)
                {
                    result[To1DIndex(x, z, width)] = grid[z, x];
                }
            }
            return result;
        }

        public static void Fill<T>(NativeArray<T> array, T value) where T : struct
        {
            int length = array.Length;
            for (int i = 0; i < length; ++i)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        public static void CopyTo<T>(T[] src, T[] des)
        {
            int limit = Mathf.Min(src.Length, des.Length);
            for (int i = 0; i < limit; ++i)
            {
                des[i] = src[i];
            }
        }

        public static int Sum(int[,] array)
        {
            int sum = 0;
            int length = array.GetLength(0);
            int width = array.GetLength(1);
            for (int z = 0; z < length; ++z)
            {
                for (int x = 0; x < width; ++x)
                {
                    sum += array[z, x];
                }
            }
            return sum;
        }

        public static int To1DIndex(int x, int z, int width)
        {
            return z * width + x;
        }

        public static bool IsInRange(float number, float minValue, float maxValue)
        {
            return number >= minValue && number <= maxValue;
        }

        public static bool IsInRangeExclusive(float number, float minValue, float maxValue)
        {
            return number > minValue && number < maxValue;
        }

        public static float GetFraction(float value, float floor, float ceil)
        {
            return (value - floor) / (ceil - floor);
        }

        public static void ClearChildren(Transform t)
        {
            int childCount = t.childCount;
            for (int i = childCount - 1; i >= 0; --i)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    GameObject.DestroyImmediate(t.GetChild(i).gameObject);
                }
                else
                {
                    GameObject.Destroy(t.GetChild(i).gameObject);
                }
#else
                GameObject.Destroy(t.GetChild(i).gameObject);
#endif
            }
        }

        public static Gradient CreateFullWhiteGradient()
        {
            Gradient g = new Gradient();
            GradientColorKey color = new GradientColorKey(Color.white, 1);
            GradientAlphaKey alpha = new GradientAlphaKey(1, 1);
            g.SetKeys(new GradientColorKey[] { color }, new GradientAlphaKey[] { alpha });
            return g;
        }

        public static Gradient CreateFullTransparentGradient()
        {
            Gradient g = new Gradient();
            GradientColorKey color = new GradientColorKey(Color.white, 1);
            GradientAlphaKey alpha = new GradientAlphaKey(0, 1);
            g.SetKeys(new GradientColorKey[] { color }, new GradientAlphaKey[] { alpha });
            return g;
        }

        public static void CalculateBarycentricCoord(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector3 bary)
        {
            Vector2 v0 = p2 - p1;
            Vector2 v1 = p3 - p1;
            Vector2 v2 = p - p1;
            float d00 = Vector2.Dot(v0, v0);
            float d01 = Vector2.Dot(v0, v1);
            float d11 = Vector2.Dot(v1, v1);
            float d20 = Vector2.Dot(v2, v0);
            float d21 = Vector2.Dot(v2, v1);
            float inverseDenom = 1 / (d00 * d11 - d01 * d01);
            bary.y = (d11 * d20 - d01 * d21) * inverseDenom;
            bary.z = (d00 * d21 - d01 * d20) * inverseDenom;
            bary.x = 1.0f - bary.y - bary.z;
        }

        public static void ExpandTrisUvCoord(Texture2D tex, Vector2[] trisUv)
        {
            Vector2 texelSize = tex.texelSize;
            Vector2 center = (trisUv[0] + trisUv[1] + trisUv[2]) / 3;
            trisUv[0] += (trisUv[0] - center).normalized * texelSize.magnitude;
            trisUv[0].x = Mathf.Clamp01(trisUv[0].x);
            trisUv[0].y = Mathf.Clamp01(trisUv[0].y);

            trisUv[1] += (trisUv[1] - center).normalized * texelSize.magnitude;
            trisUv[1].x = Mathf.Clamp01(trisUv[1].x);
            trisUv[1].y = Mathf.Clamp01(trisUv[1].y);

            trisUv[2] += (trisUv[2] - center).normalized * texelSize.magnitude;
            trisUv[2].x = Mathf.Clamp01(trisUv[2].x);
            trisUv[2].y = Mathf.Clamp01(trisUv[2].y);
        }

        public static bool ContainIdenticalElements<T>(T[] arr1, T[] arr2)
        {
            if (arr1 == null && arr2 == null)
                return true;
            if (arr1 == null && arr2 != null)
                return false;
            if (arr1 != null && arr2 == null)
                return false;
            if (arr1.Length != arr2.Length)
                return false;

            for (int i = 0; i < arr1.Length; ++i)
            {
                if (!arr1[i].Equals(arr2[i]))
                    return false;
            }

            return true;
        }

        public static float GetNearestMultiple(float number, float multipleOf)
        {
            int multiplier = 0;
            while (multipleOf * multiplier < number)
            {
                multiplier += 1;
            }

            float floor = multipleOf * (multiplier - 1);
            float ceil = multipleOf * multiplier;
            float f0 = number - floor;
            float f1 = ceil - number;

            if (f0 < f1)
                return floor;
            else
                return ceil;
        }

        public static Transform GetChildrenWithName(Transform parent, string name)
        {
            Transform t = parent.Find(name);
            if (t == null)
            {
                GameObject g = new GameObject(name);
                g.transform.parent = parent;
                ResetTransform(g.transform, parent);
                t = g.transform;
            }
            return t;
        }

        public static void ResetTransform(Transform t, Transform parent)
        {
            t.parent = parent;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void DestroyGameobject(GameObject g)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                GameObject.Destroy(g);
            else
                GameObject.DestroyImmediate(g);
#else
            GameObject.Destroy(g);
#endif
        }

        public static void DestroyGameObjectWithUndo(GameObject g)
        {
#if UNITY_EDITOR
            Undo.DestroyObjectImmediate(g);
#else
            DestroyGameobject(g);
#endif
        }

        public static void DestroyObject(Object o)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o, true);
#else
            GameObject.Destroy(o);
#endif
        }

        public static string Repeat(char src, int count)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(src, count);
            return sb.ToString();
        }

        public static void MarkCurrentSceneDirty()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
        }

        public static GameObject[] GetChildrenGameObjects(Transform parent)
        {
            GameObject[] children = new GameObject[parent.childCount];
            for (int i = 0; i < parent.childCount; ++i)
            {
                children[i] = parent.GetChild(i).gameObject;
            }
            return children;
        }

        public static Transform[] GetChildrenTransforms(Transform parent)
        {
            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; ++i)
            {
                children[i] = parent.GetChild(i);
            }
            return children;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="normal"></param>
        /// <param name="angleDegree"></param>
        /// <returns></returns>
        public static Vector3 RotateVectorAroundNormal(Vector3 vector, Vector3 normal, float angleDegree)
        {
            float sin = Mathf.Sin(angleDegree * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angleDegree * Mathf.Deg2Rad);
            Vector3 rotatedVector =
                vector * cos +
                Vector3.Cross(normal, vector) * sin +
                normal * Vector3.Dot(normal, vector) * (1 - cos);
            return rotatedVector;
        }

        public static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            Mesh m = g.GetComponent<MeshFilter>().sharedMesh;
            DestroyGameobject(g);
            return m;
        }

        public static void ShuffleList<T>(List<T> list)
        {
            int length = list.Count;
            for (int i = 0; i < length - 1; ++i)
            {
                int j = UnityEngine.Random.Range(0, length);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static int[] GetShuffleIndicesArray(int length)
        {
            int[] indices = GetIndicesArray(length);
            for (int i = 0; i < length - 1; ++i)
            {
                int j = UnityEngine.Random.Range(0, length);
                int tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

            return indices;
        }

        public static int[] GetIndicesArray(int length)
        {
            int[] indices = new int[length];
            for (int i = 0; i < length; ++i)
            {
                indices[i] = i;
            }
            return indices;
        }

        public static void ResetArray<T>(ref T[] array, int count, T defaultValue)
        {
            if (array != null && array.Length == count)
            {
                GUtilities.Fill(array, defaultValue);
            }
            else
            {
                array = new T[count];
            }
        }

        public static bool EnsureArrayLength<T>(ref T[] array, int count)
        {
            if (array == null || array.Length != count)
            {
                array = new T[count];
                return false;
            }
            return true;
        }

        public static float GetValueBilinear(float[] data, int width, int height, Vector2 uv)
        {
            float value = 0;
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, width - 1, uv.x),
                Mathf.Lerp(0, height - 1, uv.y));
            //apply a bilinear filter
            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int xCeil = Mathf.CeilToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            int yCeil = Mathf.CeilToInt(pixelCoord.y);

            float f00 = data[GUtilities.To1DIndex(xFloor, yFloor, width)];
            float f01 = data[GUtilities.To1DIndex(xFloor, yCeil, width)];
            float f10 = data[GUtilities.To1DIndex(xCeil, yFloor, width)];
            float f11 = data[GUtilities.To1DIndex(xCeil, yCeil, width)];

            Vector2 unitCoord = new Vector2(
                pixelCoord.x - xFloor,
                pixelCoord.y - yFloor);

            value =
                f00 * (1 - unitCoord.x) * (1 - unitCoord.y) +
                f01 * (1 - unitCoord.x) * unitCoord.y +
                f10 * unitCoord.x * (1 - unitCoord.y) +
                f11 * unitCoord.x * unitCoord.y;

            return value;
        }

        public static Color GetColorBilinear(Color[] textureData, int width, int height, Vector2 uv)
        {
            Color color = Color.clear;
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, width - 1, uv.x),
                Mathf.Lerp(0, height - 1, uv.y));
            //apply a bilinear filter
            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int xCeil = Mathf.CeilToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            int yCeil = Mathf.CeilToInt(pixelCoord.y);

            Color f00 = textureData[GUtilities.To1DIndex(xFloor, yFloor, width)];
            Color f01 = textureData[GUtilities.To1DIndex(xFloor, yCeil, width)];
            Color f10 = textureData[GUtilities.To1DIndex(xCeil, yFloor, width)];
            Color f11 = textureData[GUtilities.To1DIndex(xCeil, yCeil, width)];

            Vector2 unitCoord = new Vector2(
                pixelCoord.x - xFloor,
                pixelCoord.y - yFloor);

            color =
                f00 * (1 - unitCoord.x) * (1 - unitCoord.y) +
                f01 * (1 - unitCoord.x) * unitCoord.y +
                f10 * unitCoord.x * (1 - unitCoord.y) +
                f11 * unitCoord.x * unitCoord.y;

            return color;
        }

        public static GameObject CreatePreviewGameObject(Mesh m, Material mat, Vector3 position)
        {
            GameObject g = new GameObject("GO");
            g.transform.position = position;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.sharedMesh = m;

            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.sharedMaterial = mat;

            return g;
        }

        public static Camera CreatePreviewCamera(GameObject target, float distance, float padding)
        {
            GameObject g = new GameObject("CAM");
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            Camera cam = g.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.Nothing;
            cam.aspect = 1;

            MeshRenderer mr = target.GetComponent<MeshRenderer>();
            Bounds bounds = mr.bounds;
            cam.transform.position = bounds.center + new Vector3(-1, 0.5f, -1) * distance;
            cam.transform.LookAt(bounds.center);
            cam.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 2 + padding;

            return cam;
        }

        public static void EnsureDirectoryExists(string dir)
        {
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        public static void SetStaticRecursively(GameObject g, bool isStatic)
        {
#if UNITY_EDITOR
            g.isStatic = isStatic;
            GameObject[] children = GetChildrenGameObjects(g.transform);
            for (int i = 0; i < children.Length; ++i)
            {
                SetStaticRecursively(children[i], isStatic);
            }
#endif
        }

        public static void EnsureLengthSufficient<T>(List<T> list, int preferredLength) where T : Object
        {
            if (list == null)
                list = new List<T>();
            while (list.Count < preferredLength)
            {
                list.Add(null);
            }
        }

        public static void EnsureLengthSufficient(List<string> list, int preferredLength)
        {
            if (list == null)
                list = new List<string>();
            while (list.Count < preferredLength)
            {
                list.Add(string.Empty);
            }
        }

        public static void EnsureLengthSufficient(List<bool> list, int preferredLength)
        {
            if (list == null)
                list = new List<bool>();
            while (list.Count < preferredLength)
            {
                list.Add(false);
            }
        }

        public static string Ellipsis(string s, int length)
        {
            if (s.Length < length)
                return s;
            string s0 = s.Substring(0, length);
            return s0 + "...";
        }

        public static bool IsRectContainPointExclusive(Rect r, Vector2 p)
        {
            return
                p.x > r.min.x &&
                p.x < r.max.x &&
                p.y > r.min.y &&
                p.y < r.max.y;
        }

        public static Color GetColor(Color baseColor, float alpha)
        {
            return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        public static Rect GetRectContainsPoints(params Vector2[] points)
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            for (int i = 0; i < points.Length; ++i)
            {
                xMin = points[i].x < xMin ? points[i].x : xMin;
                xMax = points[i].x > xMax ? points[i].x : xMax;
                yMin = points[i].y < yMin ? points[i].y : yMin;
                yMax = points[i].y > yMax ? points[i].y : yMax;
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Rect GetRectContainsPoints(params Vector3[] points)
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float zMin = float.MaxValue;
            float zMax = float.MinValue;
            for (int i = 0; i < points.Length; ++i)
            {
                xMin = points[i].x < xMin ? points[i].x : xMin;
                xMax = points[i].x > xMax ? points[i].x : xMax;
                zMin = points[i].z < zMin ? points[i].z : zMin;
                zMax = points[i].z > zMax ? points[i].z : zMax;
            }
            return Rect.MinMaxRect(xMin, zMin, xMax, zMax);
        }

        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (a != b)
            {
                return (value - a) / (b - a);
            }
            return 0f;
        }

        public static Vector2 PointToNormalizedUnclampled(Rect r, Vector2 point)
        {
            return new Vector2(InverseLerpUnclamped(r.x, r.xMax, point.x), InverseLerpUnclamped(r.y, r.yMax, point.y));
        }

        public static Rect GetUvRect(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return Rect.MinMaxRect(
                Mathf.Min(v0.x, v1.x, v2.x),
                Mathf.Min(v0.y, v1.y, v2.y),
                Mathf.Max(v0.x, v1.x, v2.x),
                Mathf.Max(v0.y, v1.y, v2.y));
        }

        public static Gradient Clone(Gradient src)
        {
            if (src == null)
                return null;
            Gradient des = new Gradient();
            des.SetKeys(src.colorKeys, src.alphaKeys);
            return des;
        }

        public static AnimationCurve Clone(AnimationCurve src)
        {
            if (src == null)
                return null;
            AnimationCurve des = new AnimationCurve();
            Keyframe[] keys = src.keys;
            for (int i = 0; i < keys.Length; ++i)
            {
                des.AddKey(keys[i]);
            }

            des.preWrapMode = src.preWrapMode;
            des.postWrapMode = src.postWrapMode;
            return des;
        }

        public static bool IsPointInsideQuadXZ(Vector3 point, Vector3[] quad)
        {
            Vector3 bary = Vector3.zero;
            CalculateBarycentricCoord(
                new Vector2(point.x, point.z),
                new Vector2(quad[0].x, quad[0].z),
                new Vector2(quad[1].x, quad[1].z),
                new Vector2(quad[2].x, quad[2].z),
                ref bary);
            if (bary.x >= 0 && bary.y >= 0 && bary.z >= 0)
                return true;

            CalculateBarycentricCoord(
                new Vector2(point.x, point.z),
                new Vector2(quad[0].x, quad[0].z),
                new Vector2(quad[2].x, quad[2].z),
                new Vector2(quad[3].x, quad[3].z),
                ref bary);
            if (bary.x >= 0 && bary.y >= 0 && bary.z >= 0)
                return true;

            return false;
        }

        public static void DestroyMeshArray(Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; ++i)
            {
                if (meshes[i] != null)
                {
                    Object.DestroyImmediate(meshes[i], true);
                }
            }
        }

        public static Vector2 FlipY(Vector2 v)
        {
            return new Vector2(v.x, 1 - v.y);
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Delaunay_triangulation#Algorithms
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsPointInCircumcircle(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 p)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat.SetRow(0, new Vector4(v0.x, v0.y, v0.x * v0.x + v0.y * v0.y, 1));
            mat.SetRow(1, new Vector4(v2.x, v2.y, v2.x * v2.x + v2.y * v2.y, 1)); //a,b,c counterclockwise
            mat.SetRow(2, new Vector4(v1.x, v1.y, v1.x * v1.x + v1.y * v1.y, 1));
            mat.SetRow(3, new Vector4(p.x, p.y, p.x * p.x + p.y * p.y, 1));

            return mat.determinant > 0;
        }

        public static bool IsPointInCircumcircleXZ(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 p)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat.SetRow(0, new Vector4(v0.x, v0.z, v0.x * v0.x + v0.z * v0.z, 1));
            mat.SetRow(1, new Vector4(v2.x, v2.z, v2.x * v2.x + v2.z * v2.z, 1)); //a,b,c counterclockwise
            mat.SetRow(2, new Vector4(v1.x, v1.z, v1.x * v1.x + v1.z * v1.z, 1));
            mat.SetRow(3, new Vector4(p.x, p.z, p.x * p.x + p.z * p.z, 1));

            return mat.determinant > 0;
        }

        public static bool AreSetEqual(ushort[] setA, ushort[] setB)
        {
            HashSet<ushort> a = new HashSet<ushort>(setA);
            HashSet<ushort> b = new HashSet<ushort>(setB);
            return a.SetEquals(b);
        }

        //public static void Distinct<T>(this List<T> list)
        //{
        //    list.Distinct();
        //}

        public static void AddIfNotContains<T>(this IList<T> list, IEnumerable<T> items)
        {
            IEnumerator<T> iter = items.GetEnumerator();
            while (iter.MoveNext())
            {
                T current = iter.Current;
                if (!list.Contains(current))
                {
                    list.Add(current);
                }
            }
        }

        public static void AddIfNotContains<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        public static Vector3 ToX0Y(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector3 Mul(this Vector3 v0, Vector3 v1)
        {
            return new Vector3(v0.x * v1.x, v0.y * v1.y, v0.z * v1.z);
        }

        public static Rect MergeRects(IEnumerable<Rect> rects)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            IEnumerator<Rect> i = rects.GetEnumerator();
            while (i.MoveNext())
            {
                Rect r = i.Current;
                minX = Mathf.Min(minX, r.xMin);
                minY = Mathf.Min(minY, r.yMin);
                maxX = Mathf.Max(maxX, r.xMax);
                maxY = Mathf.Max(maxY, r.yMax);
            }

            float width = maxX - minX;
            float height = maxY - minY;
            return new Rect(minX, minY, width, height);
        }

        public static bool IsSceneViewOrGameCamera(Camera cam)
        {
            return cam.cameraType == CameraType.SceneView || cam.cameraType == CameraType.Game;
        }

        public static bool IsPrefabCamera(Camera cam)
        {
#if UNITY_EDITOR
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                if (prefabStage.scene == cam.scene)
                    return true;
            }
#endif
            return false;

        }

        public static bool TestOverlap(Rect r, IEnumerable<Rect> other)
        {
            IEnumerator<Rect> i = other.GetEnumerator();
            while (i.MoveNext())
            {
                if (r.Overlaps(i.Current))
                {
                    return true;
                }
            }
            return false;
        }

        public static float Vector2Cross(Vector2 lhs, Vector2 rhs)
        {
            return lhs.y * rhs.x - lhs.x * rhs.y;
        }

        public static List<GStylizedTerrain> ExtractTerrainsFromOverlapTest(List<GOverlapTestResult> sweepTests)
        {
            List<GStylizedTerrain> terrains = new List<GStylizedTerrain>();
            foreach (GOverlapTestResult st in sweepTests)
            {
                if (st.IsOverlapped)
                {
                    terrains.Add(st.Terrain);
                }
            }
            return terrains;
        }

        public static void Swap<T>(List<T> list, int firstIndex, int secondIndex)
        {
            T temp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = temp;
        }
    }
}
#endif
