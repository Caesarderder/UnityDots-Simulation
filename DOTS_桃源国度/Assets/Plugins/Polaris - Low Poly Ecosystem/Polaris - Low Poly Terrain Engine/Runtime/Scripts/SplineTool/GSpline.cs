#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

namespace Pinwheel.Griffin.SplineTool
{
    [System.Serializable]
    public class GSpline : IDisposable
    {
        [SerializeField]
        private List<GSplineAnchor> anchors;
        public List<GSplineAnchor> Anchors
        {
            get
            {
                if (anchors == null)
                {
                    anchors = new List<GSplineAnchor>();
                }
                return anchors;
            }
        }

        [SerializeField]
        private List<GSplineSegment> segments;
        public List<GSplineSegment> Segments
        {
            get
            {
                if (segments == null)
                {
                    segments = new List<GSplineSegment>();
                }
                return segments;
            }
        }

        public bool HasBranch
        {
            get
            {
                return CheckHasBranch();
            }
        }

        public bool IsSegmentValid(int segmentIndex)
        {
            GSplineSegment s = Segments[segmentIndex];
            if (s == null)
                return false;
            bool startIndexValid =
                s.StartIndex >= 0 &&
                s.StartIndex < Anchors.Count &&
                Anchors[s.StartIndex] != null;
            bool endIndexValid =
                s.EndIndex >= 0 &&
                s.EndIndex < Anchors.Count &&
                Anchors[s.EndIndex] != null;
            return startIndexValid && endIndexValid;
        }

        public void AddAnchor(GSplineAnchor a)
        {
            Anchors.Add(a);
        }

        public void RemoveAnchor(int index)
        {
            GSplineAnchor a = Anchors[index];
            List<int> segmentIndices = FindSegments(index);
            for (int i = 0; i < segmentIndices.Count; ++i)
            {
                int sIndex = segmentIndices[i];
                Segments[sIndex].Dispose();
            }

            Segments.RemoveAll(s => s.StartIndex == index || s.EndIndex == index);
            Segments.ForEach(s =>
            {
                if (s.StartIndex > index)
                    s.StartIndex -= 1;
                if (s.EndIndex > index)
                    s.EndIndex -= 1;
            });
            Anchors.RemoveAt(index);
        }

        public GSplineSegment AddSegment(int startIndex, int endIndex)
        {
            GSplineSegment s = Segments.Find(s0 =>
                (s0.StartIndex == startIndex && s0.EndIndex == endIndex) ||
                (s0.StartIndex == endIndex && s0.EndIndex == startIndex));
            if (s != null)
                return s;
            GSplineSegment newSegment = new GSplineSegment();
            newSegment.StartIndex = startIndex;
            newSegment.EndIndex = endIndex;
            Segments.Add(newSegment);
            GSplineAnchor startAnchor = Anchors[newSegment.StartIndex];
            GSplineAnchor endAnchor = Anchors[newSegment.EndIndex];
            Vector3 direction = (endAnchor.Position - startAnchor.Position).normalized;
            float length = (endAnchor.Position - startAnchor.Position).magnitude / 3;
            newSegment.StartTangent = startAnchor.Position + direction * length;
            newSegment.EndTangent = endAnchor.Position - direction * length;
            return newSegment;
        }

        public void RemoveSegment(int index)
        {
            Segments[index].Dispose();
            Segments.RemoveAt(index);
        }

        public Vector3 EvaluatePosition(int segmentIndex, float t)
        {
            GSplineSegment s = Segments[segmentIndex];
            GSplineAnchor startAnchor = Anchors[s.StartIndex];
            GSplineAnchor endAnchor = Anchors[s.EndIndex];

            Vector3 p0 = startAnchor.Position;
            Vector3 p1 = s.StartTangent;
            Vector3 p2 = s.EndTangent;
            Vector3 p3 = endAnchor.Position;

            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;
            Vector3 p =
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3 * oneMinusT * oneMinusT * t * p1 +
                3 * oneMinusT * t * t * p2 +
                t * t * t * p3;
            return p;
        }

        public Quaternion EvaluateRotation(int segmentIndex, float t)
        {
            GSplineSegment s = Segments[segmentIndex];
            GSplineAnchor startAnchor = Anchors[s.StartIndex];
            GSplineAnchor endAnchor = Anchors[s.EndIndex];
            return Quaternion.Lerp(startAnchor.Rotation, endAnchor.Rotation, t);
        }

        public Vector3 EvaluateScale(int segmentIndex, float t)
        {
            GSplineSegment s = Segments[segmentIndex];
            GSplineAnchor startAnchor = Anchors[s.StartIndex];
            GSplineAnchor endAnchor = Anchors[s.EndIndex];
            return Vector3.Lerp(startAnchor.Scale, endAnchor.Scale, t);
        }

        public Vector3 EvaluateUpVector(int segmentIndex, float t)
        {
            Quaternion rotation = EvaluateRotation(segmentIndex, t);
            Matrix4x4 matrix = Matrix4x4.Rotate(rotation);
            return matrix.MultiplyVector(Vector3.up);
        }

        public Matrix4x4 TRS(int segmentIndex, float t)
        {
            Vector3 pos = EvaluatePosition(segmentIndex, t);
            Quaternion rotation = EvaluateRotation(segmentIndex, t);
            Vector3 scale = EvaluateScale(segmentIndex, t);
            return Matrix4x4.TRS(pos, rotation, scale);
        }

        private bool CheckHasBranch()
        {
            int[] count = new int[Anchors.Count];
            for (int i = 0; i < Segments.Count; ++i)
            {
                if (!IsSegmentValid(i))
                    continue;
                GSplineSegment s = Segments[i];
                count[s.StartIndex] += 1;
                count[s.EndIndex] += 1;
                if (count[s.StartIndex] > 2 || count[s.EndIndex] > 2)
                    return true;
            }

            return false;
        }

        public List<int> FindSegments(int anchorIndex)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < Segments.Count; ++i)
            {
                if (Segments[i].StartIndex == anchorIndex ||
                    Segments[i].EndIndex == anchorIndex)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        public void Dispose()
        {
            for (int i = 0; i < Segments.Count; ++i)
            {
                if (Segments[i] != null)
                {
                    Segments[i].Dispose();
                }
            }
        }

        public int[] SmoothTangents(params int[] anchorIndices)
        {
            int[] anchorRanks = new int[Anchors.Count];
            Vector3[] directions = new Vector3[Anchors.Count];
            float[] segmentLengths = new float[Segments.Count];

            for (int i = 0; i < Segments.Count; ++i)
            {
                GSplineSegment s = Segments[i];
                anchorRanks[s.StartIndex] += 1;
                anchorRanks[s.EndIndex] += 1;

                GSplineAnchor aStart = Anchors[s.StartIndex];
                GSplineAnchor aEnd = Anchors[s.EndIndex];

                Vector3 startToEnd = aEnd.Position - aStart.Position;
                Vector3 d = Vector3.Normalize(startToEnd);
                directions[s.StartIndex] += d;
                directions[s.EndIndex] += d;

                segmentLengths[i] = startToEnd.magnitude;
            }

            for (int i = 0; i < directions.Length; ++i)
            {
                if (anchorRanks[i] == 0)
                    continue;
                directions[i] = Vector3.Normalize(directions[i] / anchorRanks[i]);
            }

            if (anchorIndices == null || anchorIndices.Length == 0)
            {
                anchorIndices = GUtilities.GetIndicesArray(Anchors.Count);
            }

            for (int i = 0; i < anchorIndices.Length; ++i)
            {
                int index = anchorIndices[i];
                if (anchorRanks[index] > 0)
                {
                    Quaternion rot = Quaternion.LookRotation(directions[index], Vector3.up);
                    Anchors[index].Rotation = rot;
                }
            }

            List<int> segmentIndices = new List<int>();
            for (int i = 0; i < Segments.Count; ++i)
            {
                GSplineSegment s = Segments[i];
                for (int j = 0; j < anchorIndices.Length; ++j)
                {
                    int anchorIndex = anchorIndices[j];
                    if (s.StartIndex == anchorIndex || s.EndIndex == anchorIndex)
                    {
                        segmentIndices.Add(i);
                    }
                }
            }

            for (int i = 0; i < segmentIndices.Count; ++i)
            {
                int index = segmentIndices[i];
                GSplineSegment s = Segments[index];
                GSplineAnchor aStart = Anchors[s.StartIndex];
                GSplineAnchor aEnd = Anchors[s.EndIndex];

                float sLength = segmentLengths[index];
                float tangentLength = sLength * 0.33f;
                Vector3 dirStart = directions[s.StartIndex];
                Vector3 dirEnd = directions[s.EndIndex];
                s.StartTangent = aStart.Position + dirStart * tangentLength;
                s.EndTangent = aEnd.Position - dirEnd * tangentLength;
            }
            return segmentIndices.ToArray();
        }

        public NativeArray<GSplineAnchor.GSweepTestData> GetAnchorSweepTestData()
        {
            NativeArray<GSplineAnchor.GSweepTestData> data = new NativeArray<GSplineAnchor.GSweepTestData>(Anchors.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < Anchors.Count; ++i)
            {
                data[i] = Anchors[i].SweepTestData;
            }
            return data;
        }

        public NativeArray<GSplineSegment.GSweepTestData> GetSegmentSweepTestData()
        {
            NativeArray<GSplineSegment.GSweepTestData> data = new NativeArray<GSplineSegment.GSweepTestData>(Segments.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < Segments.Count; ++i)
            {
                data[i] = Segments[i].SweepTestData;
            }
            return data;
        }
    }
}
#endif
