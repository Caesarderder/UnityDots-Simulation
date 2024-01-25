#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace Pinwheel.Griffin
{
    public struct GGeometryJobUtilities
    {
        public const float SUB_DIV_STEP = 0.1f;
        public const byte STATE_NOT_CREATED = 0;
        public const byte STATE_CREATED = 1;
        public const byte STATE_LEAF = 2;

        public const int VERTEX_MARKER_DIMENSION = 256;

        public const int METADATA_LEAF_COUNT = 0;
        public const int METADATA_LEAF_REMOVED = 1;
        public const int METADATA_NEW_VERTEX_CREATED = 2;
        public const int METADATA_LENGTH = 3;

        public static int GetStartIndex(ref int resolution)
        {
            int count = 0;
            for (int i = 0; i < resolution; ++i)
            {
                count += Mathf.FloorToInt(Mathf.Pow(2, i));
            }
            count *= 2;
            return count;
        }

        public static Color GetColorBilinear(GTextureNativeDataDescriptor<Color32> tex, Vector2 uv)
        {
            return GetColorBilinear(tex, ref uv);
        }

        public static Color GetColorBilinear(GTextureNativeDataDescriptor<Color32> tex, ref Vector2 uv)
        {
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, tex.width - 1, uv.x),
                Mathf.Lerp(0, tex.height - 1, uv.y));
            //apply a bilinear filter
            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int xCeil = Mathf.CeilToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            int yCeil = Mathf.CeilToInt(pixelCoord.y);

            Color f00 = tex.data[To1DIndex(ref xFloor, ref yFloor, ref tex.width)];
            Color f01 = tex.data[To1DIndex(ref xFloor, ref yCeil, ref tex.width)];
            Color f10 = tex.data[To1DIndex(ref xCeil, ref yFloor, ref tex.width)];
            Color f11 = tex.data[To1DIndex(ref xCeil, ref yCeil, ref tex.width)];

            Vector2 unitCoord = new Vector2(
                pixelCoord.x - xFloor,
                pixelCoord.y - yFloor);

            Color color =
                f00 * (1 - unitCoord.x) * (1 - unitCoord.y) +
                f01 * (1 - unitCoord.x) * unitCoord.y +
                f10 * unitCoord.x * (1 - unitCoord.y) +
                f11 * unitCoord.x * unitCoord.y;

            return color;
        }

        public static Color GetColorPoint(GTextureNativeDataDescriptor<Color32> tex, Vector2 uv)
        {
            Vector2 pixelCoord = new Vector2(
                Mathf.Lerp(0, tex.width - 1, uv.x),
                Mathf.Lerp(0, tex.height - 1, uv.y));

            int xFloor = Mathf.FloorToInt(pixelCoord.x);
            int yFloor = Mathf.FloorToInt(pixelCoord.y);
            return tex.data[To1DIndex(ref xFloor, ref yFloor, ref tex.width)];
        }

        public static int To1DIndex(ref int x, ref int y, ref int width)
        {
            return y * width + x;
        }

        public static void GetChildrenNodeIndex(ref int index, ref int leftIndex, ref int rightIndex)
        {
            leftIndex = (index + 1) * 2;
            rightIndex = (index + 1) * 2 + 1;
        }

        public static int GetElementCountForSubdivLevel(ref int level)
        {
            return 2 * Mathf.FloorToInt(Mathf.Pow(2, level));
        }

        public static void NormalizeToPoint(ref Vector2 uv, ref Rect uvRect, ref Vector2 nodeVertex)
        {
            uv.x = Mathf.Lerp(uvRect.min.x, uvRect.max.x, nodeVertex.x);
            uv.y = Mathf.Lerp(uvRect.min.y, uvRect.max.y, nodeVertex.y);
        }

        public static void MarkVertex(ref NativeArray<bool> markerArray, ref Vector2 p)
        {
            int dimension = VERTEX_MARKER_DIMENSION;
            int x = (int)(p.x * (dimension - 1));
            int y = (int)(p.y * (dimension - 1));
            markerArray[GGeometryJobUtilities.To1DIndex(ref x, ref y, ref dimension)] = true;
        }

        public static void MarkVertices(ref NativeArray<bool> markerArray, ref GSubdivNode n)
        {
            MarkVertex(ref markerArray, ref n.v0);
            MarkVertex(ref markerArray, ref n.v1);
            MarkVertex(ref markerArray, ref n.v2);
        }

        public static bool GetVertexMark(NativeArray<bool> markers, Vector2 p, bool flipX = false, bool flipY = false)
        {
            if (flipX)
            {
                p.x = 1 - p.x;
            }
            if (flipY)
            {
                p.y = 1 - p.y;
            }

            int dimension = VERTEX_MARKER_DIMENSION;
            int x = (int)(p.x * (dimension - 1));
            int y = (int)(p.y * (dimension - 1));
            //mark = markerArray[GGeometryJobUtilities.To1DIndex(ref x, ref y, ref dimension)];
            return markers[GGeometryJobUtilities.To1DIndex(ref x, ref y, ref dimension)];
        }


    }
}
#endif
