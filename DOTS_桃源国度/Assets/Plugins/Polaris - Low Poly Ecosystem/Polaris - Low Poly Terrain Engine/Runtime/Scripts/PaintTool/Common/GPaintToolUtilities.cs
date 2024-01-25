#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Jobs;

namespace Pinwheel.Griffin.PaintTool
{
    public static class GPaintToolUtilities
    {
        public static void AddCustomSpawnFilter(List<Type> filters)
        {
            for (int i = 0; i < GSpawnFilter.AllFilters.Count; ++i)
            {
                Type t = GSpawnFilter.AllFilters[i];
                if (!IsBuiltinFilter(t))
                    filters.Add(t);
            }
        }

        private static bool IsBuiltinFilter(Type t)
        {
            return t == typeof(GAlignToSurfaceFilter) ||
                    t == typeof(GHeightConstraintFilter) ||
                    t == typeof(GRotationRandomizeFilter) ||
                    t == typeof(GScaleClampFilter) ||
                    t == typeof(GScaleRandomizeFilter) ||
                    t == typeof(GSlopeConstraintFilter);
        }

        public static Matrix4x4 GetUnitRectToWorldMatrix(Vector3 position, float radius, float rotation)
        {
            Matrix4x4 m = Matrix4x4.TRS(position, Quaternion.Euler(0, rotation, 0), Vector3.one * radius);
            Matrix4x4 offset = Matrix4x4.Translate(new Vector3(-0.5f, 0, -0.5f));

            return m * offset;
        }

        public static List<GOverlapTestResult> OverlapTest(int groupId, Vector3 position, float radius, float rotation)
        {
            Vector3[] corners = GCommon.GetBrushQuadCorners(position, radius, rotation);
            return GCommon.OverlapTest(groupId, corners);
        }

        public static Rect GetDirtyRect(GStylizedTerrain t, Vector3[] worldCorners)
        {
            Vector2[] uvCorners = new Vector2[worldCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorners[i]);
            }
            Rect dirtyRect = GUtilities.GetRectContainsPoints(uvCorners);
            return dirtyRect;
        }

        public static Vector2[] WorldToUvCorners(GStylizedTerrain t, Vector3[] worldCorners)
        {
            Vector2[] uvCorners = new Vector2[worldCorners.Length];
            for (int i = 0; i < uvCorners.Length; ++i)
            {
                uvCorners[i] = t.WorldPointToUV(worldCorners[i]);
            }
            return uvCorners;
        }
    }
}
#endif
