#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Pinwheel.Griffin.SplineTool
{
    public partial class GSplineCreator : MonoBehaviour
    {


        public NativeArray<Rect> GenerateSweepRectsNA()
        {
            NativeArray<GSplineAnchor.GSweepTestData> anchors = Spline.GetAnchorSweepTestData();
            NativeArray<GSplineSegment.GSweepTestData> segments = Spline.GetSegmentSweepTestData();
            NativeArray<Rect> rects = new NativeArray<Rect>(Smoothness * Spline.Segments.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            GGenerateSweepRectsJob job = new GGenerateSweepRectsJob()
            {
                anchors = anchors,
                segments = segments,
                rects = rects,
                smoothness = Smoothness,
                splineWidth = Width,
                splineFalloffWidth = FalloffWidth,
                localToWorldMatrix = transform.localToWorldMatrix
            };
            JobHandle jHandle = job.Schedule(segments.Length, 1);
            jHandle.Complete();

            anchors.Dispose();
            segments.Dispose();
            return rects;
        }

        public List<GOverlapTestResult> SweepTest()
        {
            List<GOverlapTestResult> results = new List<GOverlapTestResult>();
            GCommon.ForEachTerrain(GroupId, (t) =>
            {
                if (t.TerrainData == null)
                    return;
                GOverlapTestResult r = new GOverlapTestResult();
                r.Terrain = t;
                results.Add(r);
            });

            NativeArray<Rect> terrainRects = new NativeArray<Rect>(results.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < terrainRects.Length; ++i)
            {
                terrainRects[i] = results[i].Terrain.Rect;
            }

            NativeArray<Rect> splineRects = GenerateSweepRectsNA();
            NativeArray<bool> terrainTestResult = new NativeArray<bool>(results.Count, Allocator.TempJob, NativeArrayOptions.ClearMemory);

            {
                GRectTestJob rectTestJob = new GRectTestJob()
                {
                    rectToTest = terrainRects,
                    sweepRects = splineRects,
                    smoothness = Smoothness,
                    result = terrainTestResult
                };

                JobHandle jHandle = rectTestJob.Schedule(Spline.Segments.Count, 1);
                jHandle.Complete();
            }

            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                r.IsOverlapped = terrainTestResult[i];
                results[i] = r;
            }

            terrainRects.Dispose();
            terrainTestResult.Dispose();


            List<JobHandle> chunkTestHandles = new List<JobHandle>();
            List<NativeArray<Rect>> chunkRectsHandles = new List<NativeArray<Rect>>();
            List<NativeArray<bool>> chunkTestResultsHandles = new List<NativeArray<bool>>();
            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                if (!r.IsOverlapped)
                {
                    chunkTestHandles.Add(default);
                    chunkRectsHandles.Add(default);
                    chunkTestResultsHandles.Add(default);
                    continue;
                }

                NativeArray<Rect> chunkRects = r.Terrain.GetChunkRectsNA();
                NativeArray<bool> chunkTestResults = new NativeArray<bool>(chunkRects.Length, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                GRectTestJob job = new GRectTestJob()
                {
                    rectToTest = chunkRects,
                    sweepRects = splineRects,
                    result = chunkTestResults,
                    smoothness = Smoothness
                };

                JobHandle jHandle = job.Schedule(Spline.Segments.Count, 1);
                chunkTestHandles.Add(jHandle);
                chunkRectsHandles.Add(chunkRects);
                chunkTestResultsHandles.Add(chunkTestResults);
            }

            for (int i = 0; i < results.Count; ++i)
            {
                GOverlapTestResult r = results[i];
                if (!r.IsOverlapped)
                {
                    continue;
                }    

                chunkTestHandles[i].Complete();
                r.IsChunkOverlapped = chunkTestResultsHandles[i].ToArray();
                results[i] = r;

                chunkRectsHandles[i].Dispose();
                chunkTestResultsHandles[i].Dispose();
            }

            splineRects.Dispose();

            return results;
        }

#if GRIFFIN_BURST
        [BurstCompile(CompileSynchronously = false)]
#endif
        public struct GGenerateSweepRectsJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<GSplineAnchor.GSweepTestData> anchors;
            [ReadOnly]
            public NativeArray<GSplineSegment.GSweepTestData> segments;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<Rect> rects;

            public int smoothness;
            public float splineWidth;
            public float splineFalloffWidth;
            public Matrix4x4 localToWorldMatrix;

            public void Execute(int segmentIndex)
            {
                float splineSize = Mathf.Max(1, splineWidth + splineFalloffWidth * 2);
                Vector2 sweepRectSize = Vector2.one * splineSize * 1.41f;
                Rect sweepRect = new Rect();
                sweepRect.size = sweepRectSize;

                for (int i = 0; i < smoothness; ++i)
                {
                    float t = Mathf.InverseLerp(0, smoothness - 1, i);
                    Vector3 localPos = EvaluatePosition(segmentIndex, t);
                    Vector3 worldPos = localToWorldMatrix.MultiplyPoint(localPos);
                    Vector3 localScale = EvaluateScale(segmentIndex, t);
                    Vector3 worldScale = localToWorldMatrix.MultiplyVector(localScale);

                    float maxScaleComponent = Mathf.Max(Mathf.Max(Mathf.Abs(worldScale.x), Mathf.Abs(worldScale.y)), Mathf.Abs(worldScale.z));
                    sweepRect.center = new Vector2(worldPos.x, worldPos.z);
                    sweepRect.size = sweepRectSize * maxScaleComponent;

                    rects[segmentIndex * smoothness + i] = sweepRect;
                }
            }

            public Vector3 EvaluatePosition(int segmentIndex, float t)
            {
                GSplineSegment.GSweepTestData s = segments[segmentIndex];
                GSplineAnchor.GSweepTestData startAnchor = anchors[s.startIndex];
                GSplineAnchor.GSweepTestData endAnchor = anchors[s.endIndex];

                Vector3 p0 = startAnchor.position;
                Vector3 p1 = s.startTangent;
                Vector3 p2 = s.endTangent;
                Vector3 p3 = endAnchor.position;

                t = Mathf.Clamp01(t);
                float oneMinusT = 1 - t;
                Vector3 p =
                    oneMinusT * oneMinusT * oneMinusT * p0 +
                    3 * oneMinusT * oneMinusT * t * p1 +
                    3 * oneMinusT * t * t * p2 +
                    t * t * t * p3;
                return p;
            }

            public Vector3 EvaluateScale(int segmentIndex, float t)
            {
                GSplineSegment.GSweepTestData s = segments[segmentIndex];
                GSplineAnchor.GSweepTestData startAnchor = anchors[s.startIndex];
                GSplineAnchor.GSweepTestData endAnchor = anchors[s.endIndex];
                return Vector3.Lerp(startAnchor.scale, endAnchor.scale, t);
            }

        }

        public struct GRectTestJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Rect> rectToTest;
            [ReadOnly]
            public NativeArray<Rect> sweepRects;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<bool> result;

            public int smoothness;

            public void Execute(int segmentIndex)
            {
                for (int i = 0; i < rectToTest.Length; ++i)
                {
                    Rect testRect = rectToTest[i];
                    for (int j = 0; j < smoothness; ++j)
                    {
                        Rect sweepRect = sweepRects[segmentIndex * smoothness + j];
                        if (sweepRect.Overlaps(testRect))
                        {
                            result[i] = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
#endif
